namespace RfidModbusWorker;

using System.Globalization;

public static class Timestamp
{
    public static string Now()
    {
        return DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture);
    }
}
