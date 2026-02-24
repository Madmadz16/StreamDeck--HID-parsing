﻿using System;

namespace StreamDeckCarControl.Hid
{
    public class ReportParser
    {
        private readonly bool[] _encoderPressed = new bool[4];
        private readonly bool[] _buttonPressed = new bool[12];

        public event Action<int, int> EncoderRotated;
        public event Action<int, bool> EncoderPressed;
        public event Action<int, bool> ButtonPressed;
        public event Action<int> StripClicked;

        public void ParseReport(byte[] report)
        {
            if (report == null || report.Length < 2) return;

            int lastIndex = report.Length - 1;
            while (lastIndex > 0 && report[lastIndex] == 0x00) lastIndex--;

            if (lastIndex < 1) return;

            byte type = report[1];

            switch (type)
            {
                case 0x03:
                    ParseKnob(report);
                    break;
                case 0x00:
                    ParseButton(report);
                    break;
                case 0x02:
                    //ParseStrip(report);
                    break;
            }
        }

        static private byte[] TrimBytes(byte[] report, int toTrim) {
            byte[] trimmed = [.. report.Reverse()
                .SkipWhile(b => b == 0x00)
                .Reverse()];

            // Need at least 5 bytes to have data after skipping the header (01-03-05-00)
            if (trimmed.Length < 5) return [];

            byte[] data = trimmed[toTrim..];
            return data;
        }
        private void ParseKnob(byte[] report)
        {
            byte[] data = TrimBytes(report, 4);
            if (data.Length == 0) return;

            // Click handling
            if (data[0] == 0x00)
            {
                EncoderPressed?.Invoke(data.Length-2, true);
            } else {
                // Rotation handling
                if (data[^1] < 255/2)
                {
                    EncoderRotated?.Invoke(data.Length-2, data[^1]);
                } else {
                    EncoderRotated?.Invoke(data.Length-2, data[^1]-256);

                }
            }
        }

        private void ParseButton(byte[] report)
        {
            byte[] data = TrimBytes(report, 4);
            if (data.Length == 0) return;

            Console.WriteLine(string.Join("-", data.Select(b => b.ToString("X2"))));
            //    bool pressed = report[4 + index] != 0;
            //    if (_buttonPressed[index] != pressed)
            //    {
            //        _buttonPressed[index] = pressed;
            //        ButtonPressed?.Invoke(index, pressed);
            //    }
        }

        //private void ParseStrip(byte[] report)
        //{
        //    int index = report.Length > 4 ? report[4] : 0;
        //    if (!_stripClicked[index])
        //    {
        //        _stripClicked[index] = true;
        //        StripClicked?.Invoke(index);
        //    }
        //}
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