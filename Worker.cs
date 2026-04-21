namespace RfidModbusWorker;

using System.Diagnostics;
using System.IO.Ports;
using NModbus;

public class Worker : BackgroundService
{
    private readonly RfidOptions _options;

    public Worker(RfidOptions options)
    {
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        SerialPortStreamResource? streamResource = null;
        IModbusSerialMaster? master = null;

        while (!stoppingToken.IsCancellationRequested)
        {
            var iterationStarted = Stopwatch.GetTimestamp();

            try
            {
                master ??= OpenMaster(out streamResource);

                var registers = master.ReadHoldingRegisters(
                    _options.SlaveId,
                    _options.StartAddress,
                    _options.RegisterCount);

                if (registers.Length > 0 && registers[0] == 1)
                {
                    Console.WriteLine(ResultLine.Ok(registers));
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                DisposeConnection(ref master, ref streamResource);
                Console.WriteLine(ResultLine.Error(ex));
            }

            try
            {
                await DelayUntilNextPoll(iterationStarted, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        DisposeConnection(ref master, ref streamResource);
    }

    private IModbusSerialMaster OpenMaster(out SerialPortStreamResource streamResource)
    {
        var serialPort = new SerialPort(_options.PortName)
        {
            BaudRate = _options.BaudRate,
            DataBits = _options.DataBits,
            Parity = _options.Parity,
            StopBits = _options.StopBits,
            ReadTimeout = _options.ReadTimeoutMs,
            WriteTimeout = _options.WriteTimeoutMs
        };

        serialPort.Open();
        streamResource = new SerialPortStreamResource(serialPort);

        var master = new ModbusFactory().CreateRtuMaster(streamResource);
        master.Transport.ReadTimeout = _options.ReadTimeoutMs;
        master.Transport.WriteTimeout = _options.WriteTimeoutMs;
        master.Transport.Retries = 0;
        master.Transport.WaitToRetryMilliseconds = (int)_options.PollInterval.TotalMilliseconds;
        master.Transport.SlaveBusyUsesRetryCount = true;

        return master;
    }

    private async Task DelayUntilNextPoll(long iterationStarted, CancellationToken stoppingToken)
    {
        var elapsed = Stopwatch.GetElapsedTime(iterationStarted);
        var remaining = _options.PollInterval - elapsed;

        if (remaining > TimeSpan.Zero)
        {
            await Task.Delay(remaining, stoppingToken);
        }
    }

    private static void DisposeConnection(
        ref IModbusSerialMaster? master,
        ref SerialPortStreamResource? streamResource)
    {
        if (master is IDisposable disposableMaster)
        {
            disposableMaster.Dispose();
        }

        master = null;
        streamResource?.Dispose();
        streamResource = null;
    }
}
