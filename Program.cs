using System;
using StreamDeckCarControl.Hid;

int VID = 0x0FD9;
int PID = 0x0084;

StreamDeckDevice deck = new StreamDeckDevice(VID, PID);

// Subscribe to parser events
deck.Parser.KnobRotated += (index, delta) =>
{
    Console.WriteLine($"Knob {index} rotated {delta}");
};

deck.Parser.KnobPressed += (index, pressed) =>
{
    Console.WriteLine($"Knob {index} pressed: {pressed}");
};

deck.Parser.ButtonPressed += (index, pressed) =>
{
    Console.WriteLine($"Button {index} pressed: {pressed}");
};

deck.Parser.StripTapped += (zone, x, y) =>
{
    Console.WriteLine($"Short touch @ {x},{y},{zone}");
};
deck.Parser.StripLongPressed += (zone, x, y) =>
{
    Console.WriteLine($"Long touch @ {x},{y},{zone}");
};

deck.Parser.StripDragged += (x, y, OutX, OutY) =>
{
    Console.WriteLine($"Drag started @ {x},{y}, ended @ {OutX},{OutY}");
};

deck.StartReading();

// Keep console alive
Console.WriteLine("Press Enter to exit...");
Console.ReadLine();