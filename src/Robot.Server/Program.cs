using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Robot.Devices.Camera;
using Robot.Devices.Camera.DirectShow;
using Robot.ObjectRecognition;
using Robot.ObjectRecognition.Parser;
using Robot.Server;
using Robot.Server.Management;
using Robot.Server.Stages;
using Robot.Server.Stages.Pipeline;
using Robot.Server.Stages.Recognition;

await using var serviceProvider = new ServiceCollection()
    .AddLogging(x => x.AddConsole().SetMinimumLevel(LogLevel.Trace))
    .AddSingleton<ICamera, DirectShowCamera>()
    .AddSingleton<ManagementServer>()
    .AddSingleton<ObjectRecognizer>()
    .AddTransient<YoloOutputParser>()
    .AddSingleton<UdpHolePuncher>()
    .AddTransient<DataAnalysisStage>()
    .AddTransient<RecognitionStage>()
    .AddTransient<ScalingStage>()
    .AddTransient<RecordingStage>()
    .BuildServiceProvider();

var progressReporter = new AsyncStageProgress(
    loggerFactory: serviceProvider.GetRequiredService<ILoggerFactory>());

var pipeline = ImmutablePipelineBuilder
    .Create<ICamera, IReadOnlyList<IPooledBitmap>, RecordingStage>(serviceProvider)
    .Append<IReadOnlyList<IPooledBitmap>, ScalingStage>()
    .Append<IReadOnlyList<IReadOnlyList<YoloBoundingBox>>, RecognitionStage>()
    .Append<IReadOnlyList<TableBoundary>, DataAnalysisStage>()
    .Pipeline;

var camera = serviceProvider.GetRequiredService<ICamera>();
await pipeline.ProcessAsync(camera, progressReporter);
