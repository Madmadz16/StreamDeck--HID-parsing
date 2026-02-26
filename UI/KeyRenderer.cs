using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HidSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using StreamDeckCarControl.Hid;

namespace StreamDeck_HID_parsing.UI
{
    public class KeyRenderer
    {
        private readonly StreamDeckDevice _deck;
        private readonly HidStream _stream;
        private readonly Dictionary<int, string[]> _buttonImageArrays;
        private readonly Dictionary<int, int> _buttonImageIndices;

        private const int KeySize = 120;       // Button resolution
        private const int PacketSize = 1024;  // HID packet size
        private const byte ReportId = 0x02;   // HID report ID
        private const byte CommandKeyImage = 0x07; // Stream Deck+ set key image

        public KeyRenderer(StreamDeckDevice deck)
        {
            _deck = deck ?? throw new ArgumentNullException(nameof(deck));
            _stream = deck.Stream;
            _buttonImageArrays = new Dictionary<int, string[]>();
            _buttonImageIndices = new Dictionary<int, int>();
        }

        public void SetButtonImage(int keyIndex, string imagePath)
        {
            SetButtonImages(keyIndex, new[] { imagePath });
        }

        public void SetButtonImages(int keyIndex, string[] imagePaths)
        {
            if (imagePaths == null || imagePaths.Length == 0)
                throw new ArgumentException("Image paths cannot be null or empty", nameof(imagePaths));
            
            _buttonImageArrays[keyIndex] = imagePaths;
            _buttonImageIndices[keyIndex] = 0;
            SendCurrentImage(keyIndex, 1.0f);
        }

        public void CycleButtonImage(int keyIndex)
        {
            if (!_buttonImageArrays.ContainsKey(keyIndex))
                return;

            var images = _buttonImageArrays[keyIndex];
            _buttonImageIndices[keyIndex] = (_buttonImageIndices[keyIndex] + 1) % images.Length;
            SendCurrentImage(keyIndex, 1.0f);
        }

        public void ShrinkButton(int keyIndex, float scale = 0.8f)
        {
            if (_buttonImageArrays.ContainsKey(keyIndex))
            {
                SendCurrentImage(keyIndex, scale);
            }
        }

        public void RestoreButton(int keyIndex)
        {
            if (_buttonImageArrays.ContainsKey(keyIndex))
            {
                SendCurrentImage(keyIndex, 1.0f);
            }
        }

        private void SendCurrentImage(int keyIndex, float scale)
        {
            if (!_buttonImageArrays.ContainsKey(keyIndex))
                return;

            var images = _buttonImageArrays[keyIndex];
            int currentIndex = _buttonImageIndices[keyIndex];
            SendImageToButton(keyIndex, images[currentIndex], scale);
        }

        private void SendImageToButton(int keyIndex, string imagePath, float scale)
        {
            using Image<Rgba32> img = Image.Load<Rgba32>(imagePath);

            // Resize to KeySize while preserving aspect ratio and padding black
            img.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(KeySize, KeySize),
                Mode = ResizeMode.Pad,
                Position = AnchorPositionMode.Center,
                PadColor = SixLabors.ImageSharp.Color.Black
            }));

            // Apply scale if needed (shrink effect)
            if (scale < 1.0f)
            {
                int scaledSize = (int)(KeySize * scale);
                
                // Create new image with black padding for shrink effect
                using var scaledImg = new Image<Rgba32>(KeySize, KeySize, SixLabors.ImageSharp.Color.Black);
                img.Mutate(x => x.Resize(scaledSize, scaledSize));
                scaledImg.Mutate(x => x.DrawImage(img, new Point((KeySize - scaledSize) / 2, (KeySize - scaledSize) / 2), 1f));
                
                // Encode to JPEG in memory
                byte[] jpegBytes;
                using (var ms = new MemoryStream())
                {
                    scaledImg.SaveAsJpeg(ms, new JpegEncoder { Quality = 95 });
                    jpegBytes = ms.ToArray();
                }
                SendButtonImageData(keyIndex, jpegBytes);
            }
            else
            {
                // Encode to JPEG in memory
                byte[] jpegBytes;
                using (var ms = new MemoryStream())
                {
                    img.SaveAsJpeg(ms, new JpegEncoder { Quality = 95 });
                    jpegBytes = ms.ToArray();
                }
                SendButtonImageData(keyIndex, jpegBytes);
            }
        }

        private void SendButtonImageData(int keyIndex, byte[] jpegBytes)
        {
            const int headerSize = 8;
            int maxChunkSize = PacketSize - headerSize;
            int totalChunks = (int)Math.Ceiling(jpegBytes.Length / (double)maxChunkSize);

            for (int chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++)
            {
                int offset = chunkIndex * maxChunkSize;
                int chunkLength = Math.Min(maxChunkSize, jpegBytes.Length - offset);

                byte[] packet = new byte[PacketSize];

                // 8-byte header
                packet[0] = ReportId;                     // HID report ID
                packet[1] = CommandKeyImage;              // Set key image command
                packet[2] = (byte)keyIndex;               // Button index
                packet[3] = (chunkIndex == totalChunks - 1) ? (byte)0x01 : (byte)0x00; // Done flag
                packet[4] = (byte)(chunkLength & 0xFF);       // Chunk size low byte
                packet[5] = (byte)((chunkLength >> 8) & 0xFF);// Chunk size high byte
                packet[6] = (byte)(chunkIndex & 0xFF);       // Chunk index low byte
                packet[7] = (byte)((chunkIndex >> 8) & 0xFF);// Chunk index high byte

                // Copy JPEG payload
                Array.Copy(jpegBytes, offset, packet, headerSize, chunkLength);

                _stream.Write(packet, 0, packet.Length);
            }
        }
    }
}