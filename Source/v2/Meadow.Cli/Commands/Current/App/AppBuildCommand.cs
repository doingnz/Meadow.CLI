﻿using CliFx.Attributes;
using Meadow.Cli;
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

    public AppBuildCommand(IPackageManager packageManager, ISettingsManager settingsManager, ILoggerFactory loggerFactory)
        : base(settingsManager, loggerFactory)
    {
        _packageManager = packageManager;
    }

    protected override async ValueTask ExecuteCommand(CancellationToken cancellationToken)
    {
        string path = Path == null
            ? AppDomain.CurrentDomain.BaseDirectory
            : Path;

        // is the path a file?
        if (!File.Exists(path))
        {
            // is it a valid directory?
            if (!Directory.Exists(path))
            {
                Logger.LogError($"Invalid application path '{path}'");
                return;
            }
        }

        if (Configuration == null) Configuration = "Release";

        Logger.LogInformation($"Building {Configuration} configuration of of {path}...");

        // TODO: enable cancellation of this call
        var success = _packageManager.BuildApplication(path, Configuration);

        if (!success)
        {
            Logger.LogError($"Build failed!");
        }
        else
        {
            Logger.LogError($"Build success.");
        }
    }
}

[Command("app trim", Description = "Runs an already-compiled Meadow application through reference trimming")]
public class AppTrimCommand : BaseCommand<AppTrimCommand>
{
    private IPackageManager _packageManager;

    [CommandOption('c', Description = "The build configuration to trim", IsRequired = false)]
    public string? Configuration { get; set; }

    [CommandParameter(0, Name = "Path to project file", IsRequired = false)]
    public string? Path { get; set; } = default!;

    public AppTrimCommand(IPackageManager packageManager, ISettingsManager settingsManager, ILoggerFactory loggerFactory)
        : base(settingsManager, loggerFactory)
    {
        _packageManager = packageManager;
    }

    protected override async ValueTask ExecuteCommand(CancellationToken cancellationToken)
    {
        string path = Path == null
            ? AppDomain.CurrentDomain.BaseDirectory
            : Path;

        // is the path a file?
        FileInfo file;

        if (!File.Exists(path))
        {
            // is it a valid directory?
            if (!Directory.Exists(path))
            {
                Logger.LogError($"Invalid application path '{path}'");
                return;
            }

            // it's a directory - we need to determine the latest build (they might have a Debug and Release config)
            var candidates = PackageManager.GetAvailableBuiltConfigurations(path, "App.dll");

            if (candidates.Length == 0)
            {
                Logger.LogError($"Cannot find a compiled application at '{path}'");
                return;
            }

            file = candidates.OrderByDescending(c => c.LastWriteTime).First();
        }
        else
        {
            file = new FileInfo(path);
        }

        // if no configuration was provided, find the most recently built
        Logger.LogInformation($"Trimming {file.FullName} (this may take a few seconds)...");

        await _packageManager.TrimApplication(file, false, null, cancellationToken);
    }
}
