using System;
using StreamDeck_HID_parsing.UI;
using StreamDeckCarControl.Hid;

namespace StreamDeck__HID_parsing.EventHandlers;

public class StreamDeckEventHandler
{
    private readonly StreamDeckDevice _device;
    private readonly KeyRenderer? _keyRenderer;

    public StreamDeckEventHandler(StreamDeckDevice device, KeyRenderer? keyRenderer = null)
    {
        _device = device;
        _keyRenderer = keyRenderer;
    }

    public void Subscribe()
    {
        _device.Parser.KnobRotated += OnKnobRotated;
        _device.Parser.KnobPressed += OnKnobPressed;
        _device.Parser.ButtonPressed += OnButtonPressed;
        _device.Parser.StripTapped += OnStripTapped;
        _device.Parser.StripLongPressed += OnStripLongPressed;
        _device.Parser.StripDragged += OnStripDragged;
    }

    public void Unsubscribe()
    {
        _device.Parser.KnobRotated -= OnKnobRotated;
        _device.Parser.KnobPressed -= OnKnobPressed;
        _device.Parser.ButtonPressed -= OnButtonPressed;
        _device.Parser.StripTapped -= OnStripTapped;
        _device.Parser.StripLongPressed -= OnStripLongPressed;
        _device.Parser.StripDragged -= OnStripDragged;
    }

    private void OnKnobRotated(int index, int delta)
    {
        Console.WriteLine($"Knob {index} rotated {delta}");
    }

    private void OnKnobPressed(int index, bool pressed)
    {
        Console.WriteLine($"Knob {index} pressed: {pressed}");
    }

    private void OnButtonPressed(int index, bool pressed)
    {
        Console.WriteLine($"Button {index} pressed: {pressed}");
        
        if (_keyRenderer != null)
        {
            if (pressed)
            {
                _keyRenderer.CycleButtonImage(index);
                _keyRenderer.ShrinkButton(index);
            }
            else
            {
                _keyRenderer.RestoreButton(index);
            }
        }
    }

    private void OnStripTapped(int zone, int x, int y)
    {
        Console.WriteLine($"Short touch @ {x},{y},{zone}");
    }

    private void OnStripLongPressed(int zone, int x, int y)
    {
        Console.WriteLine($"Long touch @ {x},{y},{zone}");
    }

    private void OnStripDragged(int x, int y, int outX, int outY)
    {
        Console.WriteLine($"Drag started @ {x},{y}, ended @ {outX},{outY}");
    }
}
