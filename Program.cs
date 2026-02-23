using System;
using StreamDeckCarControl.Hid;

int VID = 0x0FD9;
int PID = 0x0084;

StreamDeckDevice deck = new StreamDeckDevice(VID, PID);

// Subscribe to parser events
deck.Parser.EncoderRotated += (index, delta) =>
{
    Console.WriteLine($"Encoder {index} rotated {delta}");
};

deck.Parser.EncoderPressed += (index, pressed) =>
{
    Console.WriteLine($"Encoder {index} pressed: {pressed}");
};

deck.Parser.ButtonPressed += (index, pressed) =>
{
    Console.WriteLine($"Button {index} pressed: {pressed}");
};

deck.Parser.StripClicked += index =>
{
    Console.WriteLine($"Strip {index} clicked");
};

deck.StartReading();

// Keep console alive
Console.WriteLine("Press Enter to exit...");
Console.ReadLine();