namespace RfidModbusWorker;

using System.Globalization;
using System.IO.Ports;
using Microsoft.Extensions.Configuration;

public sealed class RfidOptions
{
    public required string PortName { get; init; }

    public required int BaudRate { get; init; }

    public required Parity Parity { get; init; }

    public required int DataBits { get; init; }

    public required StopBits StopBits { get; init; }

    public required byte SlaveId { get; init; }

    public required ushort StartAddress { get; init; }

    public required ushort RegisterCount { get; init; }

    public required TimeSpan PollInterval { get; init; }

    public required int ReadTimeoutMs { get; init; }

    public required int WriteTimeoutMs { get; init; }

    public static RfidOptions FromConfiguration(IConfiguration configuration)
    {
        return new RfidOptions
        {
            PortName = Required(configuration, "RFID_PORT_NAME"),
            BaudRate = RequiredInt(configuration, "RFID_BAUD_RATE", 1, 1_000_000),
            Parity = RequiredParity(configuration, "RFID_PARITY"),
            DataBits = RequiredInt(configuration, "RFID_DATA_BITS", 5, 8),
            StopBits = RequiredStopBits(configuration, "RFID_STOP_BITS"),
            SlaveId = (byte)RequiredInt(configuration, "RFID_SLAVE_ID", 1, 247),
            StartAddress = (ushort)RequiredInt(configuration, "RFID_START_ADDRESS", 0, ushort.MaxValue),
            RegisterCount = (ushort)RequiredInt(configuration, "RFID_REGISTER_COUNT", 1, 125),
            PollInterval = TimeSpan.FromMilliseconds(RequiredInt(configuration, "RFID_POLL_INTERVAL_MS", 1, 60_000)),
            ReadTimeoutMs = RequiredInt(configuration, "RFID_READ_TIMEOUT_MS", 1, 60_000),
            WriteTimeoutMs = RequiredInt(configuration, "RFID_WRITE_TIMEOUT_MS", 1, 60_000)
        };
    }

    private static string Required(IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing required environment variable '{key}'. Create a .env file next to the app or set the variable in the OS environment.");
        }

        return value.Trim();
    }

    private static int RequiredInt(IConfiguration configuration, string key, int min, int max)
    {
        var raw = Required(configuration, key);
        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            throw new FormatException($"Environment variable '{key}' must be an integer. Current value: '{raw}'.");
        }

        if (value < min || value > max)
        {
            throw new InvalidOperationException($"Environment variable '{key}' must be between {min} and {max}. Current value: {value}.");
        }

        return value;
    }

    private static Parity RequiredParity(IConfiguration configuration, string key)
    {
        var raw = Required(configuration, key);
        return raw.ToUpperInvariant() switch
        {
            "N" => Parity.None,
            "NONE" => Parity.None,
            "E" => Parity.Even,
            "EVEN" => Parity.Even,
            "O" => Parity.Odd,
            "ODD" => Parity.Odd,
            "M" => Parity.Mark,
            "MARK" => Parity.Mark,
            "S" => Parity.Space,
            "SPACE" => Parity.Space,
            _ => throw new FormatException($"Environment variable '{key}' must be one of None, Even, Odd, Mark, or Space. Current value: '{raw}'.")
        };
    }

    private static StopBits RequiredStopBits(IConfiguration configuration, string key)
    {
        var raw = Required(configuration, key);
        return raw.ToUpperInvariant() switch
        {
            "1" => StopBits.One,
            "ONE" => StopBits.One,
            "1.5" => StopBits.OnePointFive,
            "ONEPOINTFIVE" => StopBits.OnePointFive,
            "2" => StopBits.Two,
            "TWO" => StopBits.Two,
            _ => throw new FormatException($"Environment variable '{key}' must be one of One, OnePointFive, or Two. Current value: '{raw}'.")
        };
    }
}
