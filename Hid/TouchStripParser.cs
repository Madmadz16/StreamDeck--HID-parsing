public class TouchStripParser
{
    private bool _touchActive = false;
    private int? _lastZone = null;

    // ===== Add these public events =====
    public event Action<int, int, int, int>? StripTapped;   // x, y, pressure
    public event Action<int, int, int>? StripDragged;  // x, y, pressure
    public event Action? StripReleased;

    private const int ZoneCount = 4;

    public void ParseReport(byte[] report)
    {
        if (report == null || report.Length < 9) return;

        byte mode = report[4];

        switch (mode)
        {
            case 0x01:
                HandleTap(report);
                break;
            case 0x03:
                HandleDrag(report);
                break;
            default:
                HandleRelease();
                break;
        }
    }

    private void HandleTap(byte[] report)
    {
        int x = report[6];
        int y = report[7];
        int z = report[8];

        int zone = MapXToZone(x);

        if (!_touchActive || _lastZone != zone)
        {
            _touchActive = true;
            _lastZone = zone;
            StripTapped?.Invoke(zone, x, y, z);  // invoke the public event
        }
    }

    private void HandleDrag(byte[] report)
    {
        int x = report[6] | (report[7] << 8);
        int y = report[8] | (report[9] << 8);
        int z = report.Length > 10 ? report[10] : 0;

        if (!_touchActive) _touchActive = true;

        StripDragged?.Invoke(x, y, z); // invoke public event
    }

    private void HandleRelease()
    {
        if (_touchActive)
        {
            _touchActive = false;
            _lastZone = null;
            StripReleased?.Invoke();  // invoke public event
        }
    }

    private int MapXToZone(int x)
    {
        x = Math.Clamp(x, 0, 255);
        int zone = (x * 4) / 256;
        return Math.Clamp(zone, 0, 3);
    }
}