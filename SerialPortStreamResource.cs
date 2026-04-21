namespace RfidModbusWorker;

using System.IO.Ports;
using NModbus.IO;

public sealed class SerialPortStreamResource : IStreamResource, IDisposable
{
    private readonly SerialPort _serialPort;

    public SerialPortStreamResource(SerialPort serialPort)
    {
        _serialPort = serialPort;
    }

    public int InfiniteTimeout => SerialPort.InfiniteTimeout;

    public int ReadTimeout
    {
        get => _serialPort.ReadTimeout;
        set => _serialPort.ReadTimeout = value;
    }

    public int WriteTimeout
    {
        get => _serialPort.WriteTimeout;
        set => _serialPort.WriteTimeout = value;
    }

    public void DiscardInBuffer()
    {
        _serialPort.DiscardInBuffer();
    }

    public int Read(byte[] buffer, int offset, int count)
    {
        return _serialPort.Read(buffer, offset, count);
    }

    public void Write(byte[] buffer, int offset, int count)
    {
        _serialPort.Write(buffer, offset, count);
    }

    public void Dispose()
    {
        _serialPort.Dispose();
    }
}
