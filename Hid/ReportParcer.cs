using System;

namespace StreamDeckCarControl.Hid
{
    public class ReportParser
    {
        private readonly bool[] _knobPressed = new bool[4];
        private readonly bool[] _buttonPressed = new bool[8];

        public event Action<int, int> KnobRotated;
        public event Action<int, bool> KnobPressed;
        public event Action<int, bool> ButtonPressed;
        public event Action<int, int, int>? StripTapped;
        public event Action<int, int, int>? StripLongPressed;
        public event Action<int, int, int, int>? StripDragged;

        public void ParseReport(byte[] report)
        {
            if (report == null || report.Length < 2) return;

            int lastIndex = report.Length - 1;
            while (lastIndex > 0 && report[lastIndex] == 0x00) lastIndex--;

            if (lastIndex < 1) return;

            byte type = report[1];

            // Console.WriteLine(string.Join("-", report.Select(b => b.ToString("X2"))));

            switch (type)
            {
                case 0x03:
                    ParseKnob(report);
                    break;
                case 0x00:
                    ParseButtons(report);
                    break;
                case 0x02:
                    ParseStrip(report);
                    break;
                default:
                    break;
            }
        }

        private static byte[] TrimBytes(byte[] report, int headerLength) {
            if (report == null || report.Length == 0)
                return [];

            byte[] trimmed = [.. report.Reverse()
                .SkipWhile(b => b == 0x00)
                .Reverse()];

            // Need at least 5 bytes to have data after skipping the header (01-03-05-00)
            if (trimmed.Length < headerLength) return [];

            return trimmed[headerLength..];
        }
        private void ParseKnob(byte[] report)
        {
            if (report == null || report.Length < 2) return;

            // Log raw trimmed report
            byte[] trimmedReport = [.. report
                .Reverse()
                .SkipWhile(b => b == 0x00)
                .Reverse()];

            int knobCount = _knobPressed.Length;

            // Structure: [header 0-3][rotation/click byte 4][k0 5][k1 6][k2 7][k3 8]
            if (report[4] == 0x00) {
                for (int i = 0; i < knobCount; i++)
                {
                    // Knob values start at index 5
                    bool pressed = (report.Length > 5 + i) && report[5 + i] != 0;

                    // Fire event only if state changed
                    if (_knobPressed[i] != pressed)
                    {
                        _knobPressed[i] = pressed;
                        KnobPressed?.Invoke(i, pressed);
                    }
                }
            } else {
                // Rotation handling from byte 4
                if (trimmedReport.Length <= 4) return;
                
                byte rotationByte = trimmedReport.Last();
                int delta = rotationByte < 128 ? rotationByte : rotationByte - 256;

                if (delta != 0)
                {
                    // Find which knob is being rotated based on pressed knob
                    int index = trimmedReport.Length - 6;
                    KnobRotated?.Invoke(index, delta);     
                }
                
            }
        }

        private void ParseButtons(byte[] report)
        {
            bool[] newStates = [.. report
                .Skip(4)
                .Take(8)
                .Select(b => b != 0)];

            for (int i = 0; i < newStates.Length; i++)
            {
                bool oldState = _buttonPressed[i];
                bool newState = newStates[i];

                if (oldState != newState)
                {
                    _buttonPressed[i] = newState;

                    ButtonPressed?.Invoke(i, newState);
                }
            }
        }
        private void ParseStrip(byte[] report)
        {
            byte gesture = report[4];

            int x = report[6] | (report[7] << 8);
            int y = report[8] | (report[9] << 8);

            int zoneCount = _knobPressed.Length;
            int zone = GetZoneIndex(x, zoneCount);

            switch (gesture)
            {
                case 1: // SHORT
                    StripTapped?.Invoke(zone, x, y);
                    break;

                case 2: // LONG
                    StripLongPressed?.Invoke(zone, x, y);
                    break;

                case 3: // DRAG
                    int xOut = report[10] | (report[11] << 8);
                    int yOut = report[12] | (report[13] << 8);
                    int zoneOut = GetZoneIndex(xOut, zoneCount);
                    StripDragged?.Invoke(x, y, yOut, yOut);
                    break;
            }
        }

        private static int GetZoneIndex(int x, int zoneCount)
        {
            Console.WriteLine($"{x}, {zoneCount}");
            if (zoneCount <= 1)
            {
                return 0;
            }

            const int maxCoord = 800;
            int index = (int)((long)x * zoneCount / (maxCoord + 1));
            return Math.Clamp(index, 0, zoneCount - 1);
        }
    }
}

/*
 * knob 1:
01-03-05-00-01-01-00-00-00 // Clockwise
01-03-05-00-01-FF-00-00-00 // Counterclockwise
01-03-05-00-00-01-00-00-00 // Click
01-03-05 //Release

knob 2:
01-03-05-00-01-00-01-00-00 // Clockwise
01-03-05-00-01-00-FF-00-00 // Counterclockwise
01-03-05-00-00-00-01-00-00 // Click
01-03-05 //Release

knob 3:
01-03-05-00-01-00-00-01-00 // Clockwise
01-03-05-00-01-00-00-FF-00 // Counterclockwise
01-03-05-00-00-00-00-01-00 // Click
01-03-05 //Release

knob 4:
01-03-05-00-01-00-00-00-01 // Clockwise
01-03-05-00-01-00-00-00-FF // Counterclockwise
01-03-05-00-00-00-00-00-01 // Click
01-03-05 //Release

Button 1:
01-00-08-00-01-00-00-00-00-00-00-00 // Click
01-00-08-00-00-00-00-00-00-00-00-00 // Release

Button 2:
01-00-08-00-00-01-00-00-00-00-00-00 // Click
01-00-08-00-00-00-00-00-00-00-00-00 // Release

Button 3:
01-00-08-00-00-00-01-00-00-00-00-00 // Click
01-00-08-00-00-00-00-00-00-00-00-00 // Release

Button 4:
01-00-08-00-00-00-00-01-00-00-00-00 // Click
01-00-08-00-00-00-00-00-00-00-00-00 // Release

Button 5:
01-00-08-00-00-00-00-00-01-00-00-00 // Click
01-00-08-00-00-00-00-00-00-00-00-00 // Release

Button 6:
01-00-08-00-00-00-00-00-00-01-00-00 // Click
01-00-08-00-00-00-00-00-00-00-00-00 // Release

Button 7:
01-00-08-00-00-00-00-00-00-00-01-00 // Click
01-00-08-00-00-00-00-00-00-00-00-00 // Release

Button 8:
01-00-08-00-00-00-00-00-00-00-00-01 // Click
01-00-08-00-00-00-00-00-00-00-00-00 // Release

Strip click 1:
01-02-0E-00-01-01-68-00-34

Strip click 2:
01-02-0E-00-02-01-2D-01-50

Strip click 3:
01-02-0E-00-02-01-F2-01-21

Strip click 4:
01-02-0E-00-01-01-C8-02-24
*/