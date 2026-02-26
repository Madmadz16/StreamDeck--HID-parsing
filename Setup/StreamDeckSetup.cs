using System;
using System.Threading;
using StreamDeck_HID_parsing.UI;
using StreamDeckCarControl.Hid;

namespace StreamDeck__HID_parsing.Setup;

public class StreamDeckSetup
{
    private readonly StreamDeckDevice _device;
    private readonly KeyRenderer _keyRenderer;
    private readonly DialRenderer _dialRenderer;

    public StreamDeckSetup(StreamDeckDevice device)
    {
        _device = device;
        _keyRenderer = new KeyRenderer(device);
        _dialRenderer = new DialRenderer(device, 20);
    }

    public void InitializeButtons()
    {
        _keyRenderer.SetButtonImages(0, ["Images/Smiley.jpeg", "Images/Smiley2.jpeg", "Images/Smiley3.png"]);
        _keyRenderer.SetButtonImage(1, "Images/Smiley.jpeg");
        _keyRenderer.SetButtonImage(2, "Images/Smiley.jpeg");
        _keyRenderer.SetButtonImage(3, "Images/Smiley.jpeg");
        _keyRenderer.SetButtonImage(4, "Images/Smiley.jpeg");
        _keyRenderer.SetButtonImage(5, "Images/Smiley.jpeg");
        _keyRenderer.SetButtonImage(6, "Images/Smiley.jpeg");
        _keyRenderer.SetButtonImage(7, "Images/Smiley.jpeg");
    }

    public void InitializeDials()
    {
        string[] images =
        [
            @"Images/Smiley.jpeg",
            @"Images/Smiley.jpeg",
            @"",
            @"Images/Smiley.jpeg"
        ];
        string image = "Images/Smiley.jpeg";

        _dialRenderer.SendFourImages(images);
        Thread.Sleep(1000);
        _dialRenderer.UpdateSingleImage(2, image);
    }

    public void InitializeAll()
    {
        InitializeButtons();
        InitializeDials();
    }

    public KeyRenderer KeyRenderer => _keyRenderer;
}
