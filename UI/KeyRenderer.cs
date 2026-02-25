using System;
using System.IO;
using System.Linq;
using HidSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using StreamDeckCarControl.Hid;

namespace StreamDeck_HID_parsing.UI
{
    internal class KeyRenderer
    {
        private readonly StreamDeckDevice _deck;
        private readonly HidStream _stream;

        private const int KeySize = 72;
        private const int PacketSize = 1024;
        private const byte ReportId = 0x02;

        public KeyRenderer(StreamDeckDevice deck)
        {
            _deck = deck ?? throw new ArgumentNullException(nameof(deck));
            _stream = deck.Stream;
        }

        public void SetButtonImage(int keyIndex, string imagePath)
        {
            if (!File.Exists(imagePath)) throw new FileNotFoundException("Image not found", imagePath);

            using Image<Rgba32> img = Image.Load<Rgba32>(imagePath);
            img.Mutate(x => x.Resize(120, 120));

            using var ms = new MemoryStream();
            img.SaveAsJpeg(ms);
            byte[] jpegBytes = ms.ToArray();

            SendImageToDeck(keyIndex, jpegBytes);
        }

        private void SendImageToDeck(int keyIndex, byte[] jpegData)
        {
            int packetSize = _stream.Device.GetMaxFeatureReportLength();
            const int headerSize = 16;

            int chunkSize = packetSize - headerSize;
            int totalPages = (int)Math.Ceiling(jpegData.Length / (double)chunkSize);

            for (int page = 0; page < totalPages; page++)
            {
                int offset = page * chunkSize;
                int size = Math.Min(chunkSize, jpegData.Length - offset);

                byte[] packet = new byte[packetSize];

                packet[0] = 0x00;                 // Required on Linux
                packet[1] = 0x02;                 // Report ID
                packet[2] = 0x07;                 // Set Image command
                packet[3] = (byte)keyIndex;
                packet[4] = 0x00;
                packet[5] = (byte)page;
                packet[6] = (byte)totalPages;
                // bytes 7-15 remain zero

                Array.Copy(jpegData, offset, packet, headerSize, size);

                _stream.SetFeature(packet);
            }
        }
    }
}