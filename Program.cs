using DotNetEnv;
using RfidModbusWorker;

var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath);
}

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Configuration.AddEnvironmentVariables();

RfidOptions options;
try
{
    options = RfidOptions.FromConfiguration(builder.Configuration);
}
catch (Exception ex) when (ex is InvalidOperationException or FormatException)
{
    Console.Error.WriteLine($"{Timestamp.Now()} CONFIG_ERROR {ex.Message}");
    Environment.ExitCode = 1;
    return;
}

builder.Services.AddSingleton(options);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();
