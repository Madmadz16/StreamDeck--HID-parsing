using System;
using System.Drawing;
using StreamDeck_HID_parsing.UI;
using StreamDeckCarControl.Hid;


int VID = 0x0FD9;
int PID = 0x0084;

StreamDeckDevice deck = new StreamDeckDevice(VID, PID);
deck.StartReading();

var renderer = new KeyRenderer(deck);
renderer.SetButtonImage(0, "Images/Smiley.jpeg"); // set image on button 0
renderer.SetButtonImage(1, "Images/Smiley.jpeg"); // set image on button 0
renderer.SetButtonImage(2, "Images/Smiley.jpeg"); // set image on button 0
renderer.SetButtonImage(3, "Images/Smiley.jpeg"); // set image on button 0
renderer.SetButtonImage(4, "Images/Smiley.jpeg"); // set image on button 0
renderer.SetButtonImage(5, "Images/Smiley.jpeg"); // set image on button 0
renderer.SetButtonImage(6, "Images/Smiley.jpeg"); // set image on button 0
renderer.SetButtonImage(7, "Images/Smiley.jpeg"); // set image on button 0

var dialRenderer = new DialRenderer(deck);
string[] images = new string[]
{
    @"Images/Smiley.jpeg",
    @"Images/Smiley.jpeg",    
    //@"Images/Smiley.jpeg",
    @"Images/Smiley.jpeg"
};
dialRenderer.SendLcdImage("Images/Smiley.jpeg");

/*
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
*/

Console.WriteLine("Images sent. Press Enter to exit...");
// Keep console alive
// Console.WriteLine("Press Enter to exit...");
Console.ReadLine();