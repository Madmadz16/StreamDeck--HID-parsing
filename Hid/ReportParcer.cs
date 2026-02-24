﻿using System;

namespace StreamDeckCarControl.Hid
{
    public class ReportParser
    {
        private readonly TouchStripParser _stripParser = new TouchStripParser();

        private readonly bool[] _encoderPressed = new bool[4];
        private readonly bool[] _buttonPressed = new bool[8];

        public event Action<int, int> EncoderRotated;
        public event Action<int, bool> EncoderPressed;
        public event Action<int, bool> ButtonPressed;
        public event Action<int, int, int, int>? StripTapped;
        public event Action<int, int, int>? StripDragged;
        public event Action? StripReleased;

        public ReportParser()
        {
            // Forward TouchStripParser events
            _stripParser.StripTapped += (zone, x, y, z) => StripTapped?.Invoke(zone, x, y, z);
            _stripParser.StripDragged += (x, y, z) => StripDragged?.Invoke(x, y, z);
            _stripParser.StripReleased += () => StripReleased?.Invoke();
        }

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
                    _stripParser.ParseReport(report);
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
            byte[] data = TrimBytes(report, 4);
            if (data.Length == 0) return;

            int index = data.Length - 2;
            // Click handling
            if (data[0] == 0x00)
            {
                EncoderPressed?.Invoke(index, true);
            } else {
                // Rotation handling
                if (data[^1] < 255/2)
                {
                    EncoderRotated?.Invoke(index, data[^1]);
                } else {
                    EncoderRotated?.Invoke(index, data[^1]-256);

                }
            }
        }

        private void ParseButtons(byte[] report)
        {
            // Get button data; empty array on release
            byte[] data = TrimBytes(report, 4);

            // Count how many buttons were previously pressed
            int pressedCount = _buttonPressed.Count(b => b);

            if (data.Length == 0)
            {
                // Release report: trigger release for all pressed buttons
                _buttonPressed
                    .Select((pressed, index) => (pressed, index))
                    .Where(x => x.pressed)
                    .ToList()
                    .ForEach(x =>
                    {
                        _buttonPressed[x.index] = false;
                        ButtonPressed?.Invoke(x.index, false);
                    });

                return;
            }

            // Press report: update state for each pressed button
            data
                .Select((value, index) => (value, index))
                .Where(x => x.value != 0 && !_buttonPressed[x.index])
                .ToList()
                .ForEach(x =>
                {
                    _buttonPressed[x.index] = true;
                    ButtonPressed?.Invoke(x.index, true);
                });
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