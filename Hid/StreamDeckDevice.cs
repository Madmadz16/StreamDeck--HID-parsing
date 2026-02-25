using System;
using System.Linq;
using System.Threading;
using HidSharp;

namespace StreamDeckCarControl.Hid
{
    public class StreamDeckDevice : IDisposable
    {
        private HidDevice _device;
        public HidDevice Device => _device;

        private readonly HidStream _stream;
        public HidStream Stream => _stream;
        private readonly ReportParser _parser;

        // Expose parser publicly so you can subscribe to its events
        public ReportParser Parser => _parser;

        public StreamDeckDevice(int vID, int pID)
        {
            _parser = new ReportParser();

            _device = DeviceList.Local
                .GetHidDevices(vID, pID)
                //.Where(d => d.GetMaxFeatureReportLength() == 1024) // classic button interface
                .FirstOrDefault()
                ?? throw new Exception("Stream Deck button interface not found.");

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
                    try
                    {
                        int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            _parser.ParseReport(buffer);
                        }
                    }
                    catch (TimeoutException)
                    {
                        // Timeout is normal - device has no data, continue waiting
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading from device: {ex.Message}");
                        break;
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