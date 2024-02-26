﻿using CliFx.Attributes;
using Meadow.Cloud.Client;
using Meadow.Cloud.Client.Identity;
using Microsoft.Extensions.Logging;

namespace Meadow.CLI.Commands.DeviceManagement;

[Command("cloud collection list", Description = "List Meadow Collections")]
public class CloudCollectionListCommand : BaseCloudCommand<CloudCollectionListCommand>
{
    [CommandOption("orgId", 'o', Description = "Organization Id", IsRequired = false)]
    public string? OrgId { get; init; }

    private readonly CollectionService _collectionService;

    public CloudCollectionListCommand(
        IMeadowCloudClient meadowCloudClient,
        UserService userService,
        CollectionService collectionService,
        ILoggerFactory loggerFactory)
        : base(meadowCloudClient, userService, loggerFactory)
    {
        _collectionService = collectionService;
    }

    protected override async ValueTask ExecuteCloudCommand()
    {
        var org = await GetOrg(Host, OrgId, CancellationToken);

        if (org == null) return;

        var collections = await _collectionService.GetOrgCollections(org.Id, Host, CancellationToken);

        if (collections == null || collections.Count == 0)
        {
            Logger?.LogInformation("No collections found.");
        }
        else
        {
            Logger?.LogInformation("Collections:");
            foreach (var collection in collections)
            {
                Logger?.LogInformation($" {collection.Id} | {collection.Name}");
            }
        }
    }
}
