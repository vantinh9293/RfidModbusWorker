namespace RfidModbusWorker;

using System.Globalization;

public static class ResultLine
{
    public static string Ok(IReadOnlyCollection<ushort> registers)
    {
        var decimalValues = string.Join(",", registers);
        var hexValues = string.Join(",", registers.Select(register => register.ToString("X4", CultureInfo.InvariantCulture)));

        return $"{Timestamp.Now()} OK dec=[{decimalValues}] hex=[{hexValues}]";
    }

    public static string Error(Exception exception)
    {
        return $"{Timestamp.Now()} ERROR {exception.GetType().Name}: {exception.Message}";
    }
}
