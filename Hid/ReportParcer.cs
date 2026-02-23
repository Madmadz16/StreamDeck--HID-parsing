using System;

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

            byte last = report[lastIndex];

            byte type = report[1];

            switch (type)
            {
                case 0x03:
                    ParseKnob(report, lastIndex, last);
                    break;
                case 0x00:
                    //ParseButton(report, lastIndex, last);
                    break;
                case 0x02:
                    //ParseStrip(report, lastIndex, last);
                    break;
            }
        }

        private void ParseKnob(byte[] report, int lastIndex, byte last)
        {
            // Count consecutive 0x00 backwards before last byte
            int zeroCount = 0;
            for (int i = lastIndex - 1; i >= 0; i--)
            {
                if (report[i] == 0x00) zeroCount++;
                else break;
            }

            int knobIndex = zeroCount;
            if (knobIndex < 0 || knobIndex > 3) return;

            // Rotation handling
            if (last == 0x01)
            {
                EncoderRotated?.Invoke(knobIndex, 1);
            }
            else if (last == 0xFF)
            {
                EncoderRotated?.Invoke(knobIndex, -1);
            }
            else
            {
                // Press handling: only fire if last byte is exactly 0x01 in the press position
                // The press position is always the last byte when rotation is 0
                bool pressed = last != 0x00;
                if (_encoderPressed[knobIndex] != pressed)
                {
                    _encoderPressed[knobIndex] = pressed;
                    EncoderPressed?.Invoke(knobIndex, pressed);
                }
            }
        }

        //private void ParseButton(byte[] report, int lastIndex, byte last)
        //{
        //    // Determine index: first non-zero byte after byte 4
        //    int index = -1;
        //    for (int i = 4; i < report.Length; i++)
        //    {
        //        if (report[i] != 0)
        //        {
        //            index = i - 4; // maps to button 0..n
        //            break;
        //        }
        //    }
        //    if (index == -1) return;

        //    bool pressed = report[4 + index] != 0;
        //    if (_buttonPressed[index] != pressed)
        //    {
        //        _buttonPressed[index] = pressed;
        //        ButtonPressed?.Invoke(index, pressed);
        //    }
        //}

        //private void ParseStrip(byte[] report, int lastIndex, byte last)
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
01-00-08-00-00-00-01-00-00-00-00-00 // Click
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