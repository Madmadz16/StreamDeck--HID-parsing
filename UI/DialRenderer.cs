using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using StreamDeckCarControl.Hid;

namespace StreamDeck_HID_parsing.UI
{
    internal class DialRenderer
    {
        private readonly StreamDeckDevice _deckDevice;

        // Packet sizes matching Python library
        private const int ImgPacketLen = 1024;
        private const int LcdPacketHeader = 16;
        private const int LcdPacketPayloadLen = ImgPacketLen - LcdPacketHeader;

        // LCD command for setting image
        private const byte CmdSetLcdImage = 0x02;

        public DialRenderer(StreamDeckDevice device)
        {
            _deckDevice = device ?? throw new ArgumentNullException(nameof(device));
        }

        /// <summary>
        /// Send a single JPEG image to the LCD.
        /// The image is resized to 800x100 and sent in 1024-byte HID packets.
        /// </summary>
        public void SendLcdImage(string filePath)
        {
            byte[] jpegBytes = LoadAndConvertToJpeg(filePath, 800, 100);
            SendImageChunks(jpegBytes);
        }

        private byte[] LoadAndConvertToJpeg(string filePath, int width, int height)
        {
            using var bmp = new Bitmap(filePath);
            using var resized = new Bitmap(bmp, width, height);
            using var ms = new MemoryStream();
            resized.Save(ms, ImageFormat.Jpeg);
            return ms.ToArray();
        }

        private void SendImageChunks(byte[] imageData)
        {
            int reportLen = _deckDevice.Device.GetMaxOutputReportLength();
            int payloadPerChunk = reportLen - 16; // 16-byte LCD header
            byte reportId = 0x01;

            int totalChunks = (int)Math.Ceiling(imageData.Length / (double)payloadPerChunk);

            for (int chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++)
            {
                int offset = chunkIndex * payloadPerChunk;
                int bytesInChunk = Math.Min(payloadPerChunk, imageData.Length - offset);

                byte[] packet = new byte[reportLen];
                packet[0] = reportId;           // Report ID
                packet[1] = CmdSetLcdImage;     // LCD command
                packet[2] = (byte)(chunkIndex & 0xFF);
                packet[3] = (byte)(totalChunks & 0xFF);
                packet[4] = (byte)((bytesInChunk >> 8) & 0xFF);
                packet[5] = (byte)(bytesInChunk & 0xFF);
                // bytes 6..15 = 0 (padding)

                Array.Copy(imageData, offset, packet, 16, bytesInChunk);

                _deckDevice.Stream.Write(packet, 0, packet.Length);
                Thread.Sleep(2);
            }
        }
    }
}