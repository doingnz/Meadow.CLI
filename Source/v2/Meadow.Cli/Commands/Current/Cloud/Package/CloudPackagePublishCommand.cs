﻿using CliFx.Attributes;
using Meadow.Cloud.Client;
using Meadow.Cloud.Client.Identity;
using Microsoft.Extensions.Logging;

namespace Meadow.CLI.Commands.DeviceManagement;

[Command("cloud package publish", Description = "Publishes a Meadow Package (MPAK)")]
public class CloudPackagePublishCommand : BaseCloudCommand<CloudPackagePublishCommand>
{
    private readonly PackageService _packageService;

    [CommandParameter(0, Name = "PackageID", Description = "ID of the package to publish", IsRequired = true)]
    public string PackageId { get; init; } = default!;

    [CommandOption("collectionId", 'c', Description = "The target collection for publishing", IsRequired = true)]
    public string CollectionId { get; init; } = default!;

    [CommandOption("metadata", 'm', Description = "Pass through metadata", IsRequired = false)]
    public string? Metadata { get; init; }

    public CloudPackagePublishCommand(
        IMeadowCloudClient meadowCloudClient,
        UserService userService,
        PackageService packageService,
        ILoggerFactory loggerFactory)
        : base(meadowCloudClient, userService, loggerFactory)
    {
        _packageService = packageService;
    }

    protected override async ValueTask ExecuteCloudCommand()
    {
        try
        {
            Logger?.LogInformation($"Publishing package {PackageId} to collection {CollectionId}...");

            await _packageService.PublishPackage(PackageId, CollectionId, Metadata ?? string.Empty, Host, CancellationToken);
            Logger?.LogInformation("Publish successful.");
        }
        catch (MeadowCloudException mex)
        {
            Logger?.LogError($"Publish failed: {mex.Message}");
        }
    }
}