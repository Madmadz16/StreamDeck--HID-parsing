using System;
using System.Linq;
using System.Threading;
using HidSharp;

namespace StreamDeckCarControl.Hid
{
    public class StreamDeckDevice : IDisposable
    {
        private readonly HidDevice _device;
        private readonly HidStream _stream;
        private readonly ReportParser _parser;

        // Expose parser publicly so you can subscribe to its events
        public ReportParser Parser => _parser;

        public StreamDeckDevice(int vID, int pID)
        {
            _parser = new ReportParser();

            _device = DeviceList.Local
                .GetHidDevices(vID)
                .FirstOrDefault(d => d.ProductName.Contains("Stream Deck"));

            if (_device == null)
                throw new Exception("Stream Deck not found.");

            if (!_device.TryOpen(out _stream))
                throw new Exception("Failed to open Stream Deck device.");
        }

        public void StartReading()
        {
            byte[] buffer = new byte[_device.GetMaxInputReportLength()];

            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        _parser.ParseReport(buffer);
                    }
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}