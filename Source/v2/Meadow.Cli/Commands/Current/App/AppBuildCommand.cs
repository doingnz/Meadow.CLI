﻿using CliFx.Attributes;
using Meadow.CLI;
using Microsoft.Extensions.Logging;

namespace Meadow.CLI.Commands.DeviceManagement;

[Command("app build", Description = "Compiles a Meadow application")]
public class AppBuildCommand : BaseCommand<AppBuildCommand>
{
    private IPackageManager _packageManager;

    [CommandOption('c', Description = "The build configuration to compile", IsRequired = false)]
    public string? Configuration { get; set; }

    [CommandParameter(0, Name = "Path to project file", IsRequired = false)]
    public string? Path { get; set; } = default!;

    public AppBuildCommand(IPackageManager packageManager, ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
        _packageManager = packageManager;
    }

    protected override async ValueTask ExecuteCommand()
    {
        await Task.Run(async () =>
        {
            string path = Path == null
            ? Environment.CurrentDirectory
            : Path;

            // is the path a file?
            if (!File.Exists(path))
            {
                // is it a valid directory?
                if (!Directory.Exists(path))
                {
                    Logger?.LogError($"Invalid application path '{path}'");
                    return;
                }
            }

            if (Configuration == null)
                Configuration = "Release";

            Logger?.LogInformation($"Building {Configuration} configuration of {path} (this may take a few seconds)...");

            // TODO: enable cancellation of this call
            var success = await Task.FromResult(_packageManager.BuildApplication(path, Configuration))
                .WithSpinner(Console!, 250);

            if (!success)
            {
                Logger?.LogError($"Build failed!");
            }
            else
            {
                Logger?.LogError($"Build success.");
            }
        });
    }
}