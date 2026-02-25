using HidSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using StreamDeckCarControl.Hid;
using System;
using System.IO;

namespace StreamDeck_HID_parsing.UI
{
    internal class KeyRenderer
    {
        private readonly StreamDeckDevice _deck;
        private readonly HidStream _stream;

        private const int KeySize = 72;       // Button resolution
        private const int PacketSize = 1024;  // HID packet size
        private const byte ReportId = 0x02;   // HID report ID
        private const byte CommandKeyImage = 0x07; // Stream Deck+ command

        public KeyRenderer(StreamDeckDevice deck)
        {
            _deck = deck ?? throw new ArgumentNullException(nameof(deck));
            _stream = deck.Stream;
        }

        public void SetButtonImage(int keyIndex, string imagePath)
        {
            using Image<Rgba32> img = Image.Load<Rgba32>(imagePath);
            img.Mutate(x => x.Resize(KeySize, KeySize));

            // Convert image to JPEG in memory
            byte[] jpegBytes;
            using (var ms = new MemoryStream())
            {
                img.SaveAsJpeg(ms, new JpegEncoder { Quality = 80 });
                jpegBytes = ms.ToArray();
            }

            SendButtonImage(keyIndex, jpegBytes);
        }

        private void SendButtonImage(int keyIndex, byte[] jpegBytes)
        {
            const int headerSize = 8;
            int maxChunkSize = PacketSize - headerSize;
            int totalChunks = (int)Math.Ceiling(jpegBytes.Length / (double)maxChunkSize);

            for (int chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++)
            {
                int offset = chunkIndex * maxChunkSize;
                int chunkLength = Math.Min(maxChunkSize, jpegBytes.Length - offset);

                byte[] packet = new byte[PacketSize];

                packet[0] = ReportId;                     // HID report ID
                packet[1] = CommandKeyImage;              // Command
                packet[2] = (byte)keyIndex;               // Key index
                packet[3] = (chunkIndex == totalChunks - 1) ? (byte)0x01 : (byte)0x00; // Done flag
                packet[4] = (byte)(chunkLength & 0xFF);        // Chunk size low byte
                packet[5] = (byte)((chunkLength >> 8) & 0xFF); // Chunk size high byte
                packet[6] = (byte)(chunkIndex & 0xFF);        // Chunk index low byte
                packet[7] = (byte)((chunkIndex >> 8) & 0xFF); // Chunk index high byte

                Array.Copy(jpegBytes, offset, packet, headerSize, chunkLength);

                _stream.Write(packet, 0, packet.Length);
            }
        }
    }
}