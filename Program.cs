using System;
using StreamDeck__HID_parsing.EventHandlers;
using StreamDeck__HID_parsing.Setup;
using StreamDeckCarControl.Hid;

// Device configuration
int VID = 0x0FD9;
int PID = 0x0084;

// Initialize device
StreamDeckDevice deck = new StreamDeckDevice(VID, PID);
deck.StartReading();

// Setup buttons and dials
var setup = new StreamDeckSetup(deck);
setup.InitializeAll();

// Setup event handlers
var eventHandler = new StreamDeckEventHandler(deck, setup.KeyRenderer);
eventHandler.Subscribe();

Console.WriteLine("Images sent. Press Enter to exit...");
Console.ReadLine();