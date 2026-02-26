using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Jpeg;
using StreamDeckCarControl.Hid;
using SixLabors.ImageSharp.Drawing.Processing;

namespace StreamDeck_HID_parsing.UI
{
    internal class DialRenderer
    {
        private readonly StreamDeckDevice _deckDevice;

        // Make these class-level so UpdateSingleImage can use them
        private readonly int canvasWidth = 800;
        private readonly int canvasHeight = 100;
        private readonly int imgWidth;
        private readonly int padding;

        public DialRenderer(StreamDeckDevice device, int padding = 2)
        {
            _deckDevice = device ?? throw new ArgumentNullException(nameof(device));
            this.padding = padding;
            this.imgWidth = (canvasWidth - padding * 3) / 4;
        }

        public void SendFourImages(string[] imagePaths)
        {
            if (imagePaths.Length != 4)
                throw new ArgumentException("Exactly 4 images are required.", nameof(imagePaths));

            using Image<Rgba32> canvas = new Image<Rgba32>(canvasWidth, canvasHeight, Color.Black);

            for (int i = 0; i < 4; i++)
            {
                int xPos = i * (imgWidth + padding);

                if (string.IsNullOrEmpty(imagePaths[i]))
                {
                    canvas.Mutate(ctx => ctx.Fill(Color.Black, new Rectangle(xPos, 0, imgWidth, canvasHeight)));
                }
                else
                {
                    if (!File.Exists(imagePaths[i]))
                        throw new FileNotFoundException("Image not found", imagePaths[i]);

                    using Image<Rgba32> img = Image.Load<Rgba32>(imagePaths[i]);
                    img.Mutate(x => x.Resize(imgWidth, canvasHeight));
                    canvas.Mutate(ctx => ctx.DrawImage(img, new Point(xPos, 0), 1f));
                }
            }

            SendImageToLcd(canvas, 0, 0);
        }

        public void UpdateSingleImage(int index, string imagePath)
        {
            int xPos = index * (imgWidth + padding);

            using Image<Rgba32> section = new Image<Rgba32>(imgWidth, canvasHeight);

            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
            {
                section.Mutate(ctx => ctx.Fill(Color.Black));
            }
            else
            {
                using Image<Rgba32> img = Image.Load<Rgba32>(imagePath);
                img.Mutate(x => x.Resize(imgWidth, canvasHeight));
                section.Mutate(ctx => ctx.DrawImage(img, new Point(0, 0), 1f));
            }

            SendImageToLcd(section, xPos, 0);
        }

        private void SendImageToLcd(Image<Rgba32> image, int xOffset, int yOffset)
        {
            using MemoryStream ms = new MemoryStream();
            image.Save(ms, new JpegEncoder { Quality = 90 });
            byte[] jpegBytes = ms.ToArray();

            int pageNumber = 0;
            int offset = 0;
            const int payloadSize = 1008;

            while (offset < jpegBytes.Length)
            {
                int bytesToSend = Math.Min(payloadSize, jpegBytes.Length - offset);
                bool isLast = (offset + bytesToSend) >= jpegBytes.Length;

                byte[] packet = new byte[1024];
                packet[0] = 0x02; // Command ID
                packet[1] = 0x0c; // Command Type
                BitConverter.GetBytes((short)xOffset).CopyTo(packet, 2);
                BitConverter.GetBytes((short)yOffset).CopyTo(packet, 4);
                BitConverter.GetBytes((short)image.Width).CopyTo(packet, 6);
                BitConverter.GetBytes((short)image.Height).CopyTo(packet, 8);
                packet[10] = isLast ? (byte)1 : (byte)0;
                BitConverter.GetBytes((short)pageNumber).CopyTo(packet, 11);
                BitConverter.GetBytes((short)bytesToSend).CopyTo(packet, 13);

                Array.Copy(jpegBytes, offset, packet, 16, bytesToSend);
                _deckDevice.Stream.Write(packet, 0, packet.Length);

                offset += bytesToSend;
                pageNumber++;
            }
        }
    }
}