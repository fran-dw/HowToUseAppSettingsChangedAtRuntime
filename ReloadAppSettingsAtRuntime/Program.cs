using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ReloadAppSettingsAtRuntime;

IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", false, true)
    .Build();

var serviceProvider = new ServiceCollection()
    .AddSingleton(config)
    .Configure<LogLevel>(config.GetSection("LogLevel"))
    .BuildServiceProvider();

AddSettingsMonitor(serviceProvider);
WriteInstructions();

Run(serviceProvider);

return;

static void Run(IServiceProvider serviceProvider)
{
    string command;

    while ((command = Console.ReadKey(true).KeyChar.ToString()) != "2")
    {
        switch (command)
        {
            case "1":
                var settings = serviceProvider.GetRequiredService<IOptions<LogLevel>>().Value;
                WriteLogLevelSettings(settings);
                WriteInstructions();
                break;
            default:
                WriteInstructions();
                break;
        }
    }
    
    Console.WriteLine("Buy");
}

static void AddSettingsMonitor(IServiceProvider serviceProvider)
{
    var logLevelsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<LogLevel>>();
    var isChanging = false;

    logLevelsMonitor.OnChange(settings =>
    {
        if (isChanging) return;
        isChanging = true;

        Console.WriteLine("LogLevels In File Have Changed During Runtime");
        
        WriteLogLevelSettings(settings);
        WriteInstructions();

        Task.Delay(TimeSpan.FromSeconds(1)).ContinueWith(_ => isChanging = false);
    });
}

static void WriteLogLevelSettings(LogLevel logLevel)
{
    Console.WriteLine($"LogLevel.Default: {logLevel.Default}");
}

static void WriteInstructions()
{
    Console.WriteLine();
    Console.WriteLine("1: show original settings, 2: quit");
}