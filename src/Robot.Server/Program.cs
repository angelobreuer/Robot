using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Robot.Server;

using var serviceProvider = new ServiceCollection()
    .AddLogging(x => x.AddConsole().SetMinimumLevel(LogLevel.Trace))
    .AddSingleton<RobotServer>()
    .BuildServiceProvider();

serviceProvider.GetRequiredService<RobotServer>().Start();

Thread.Sleep(-1);
