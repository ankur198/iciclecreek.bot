﻿using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Iciclecreek.Bot.Builder.Dialogs.Database.Cosmos
{
    /// <summary>
    /// Create cosmos db item in container
    /// </summary>
    [Description("Delete item in a cosmos container")]
    public class DeleteItem  : Dialog
    {
        [JsonProperty("$kind")]
        public const string Kind = "Iciclecreek.Cosmos.DeleteItem";

        [JsonConstructor]
        public DeleteItem([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets the disabled state for the action.
        /// </summary>
        [JsonProperty("disabled")]
        [Description("Disable action")]
        public BoolExpression Disabled { get; set; }

        /// <summary>
        /// Gets or sets the ConnectionString for querying the database.
        /// </summary>
        [JsonProperty("connectionString")]
        [Description("Connection string for cosmosdb.")]
        [Required]
        public StringExpression ConnectionString { get; set; }

        /// <summary>
        /// database name
        /// </summary>
        [JsonProperty("database")]
        [Description("Database name.")]
        [Required]
        public StringExpression Database { get; set; }

        /// <summary>
        /// Container name
        /// </summary>
        [JsonProperty("container")]
        [Description("Name of the Container.")]
        [Required]
        public StringExpression Container { get; set; }

        /// <summary>
        /// Item Id (default will be to look for id on the Item object itself)
        /// </summary>
        [JsonProperty("itemId")]
        [Description("ItemId of the item to delete.")]
        public StringExpression ItemId { get; set; }

        /// <summary>
        /// Gets or sets the PartitionKey value to delete.
        /// </summary>
        [JsonProperty("partitionKey")]
        [Description("PartitionKey value of the item to delete.")]
        public StringExpression PartitionKey{ get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (this.Disabled != null && this.Disabled.GetValue(dc.State) == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            var connectionString = ConnectionString.GetValue(dc.State);
            var databaseName = Database.GetValue(dc.State);
            var containerName = Container.GetValue(dc.State);
            var itemId = ItemId?.GetValue(dc.State);
            var partitionKey = PartitionKey.GetValue(dc.State);
            var client = CosmosClientCache.GetClient(connectionString);
            var database = client.GetDatabase(databaseName);
            var container = database.GetContainer(containerName);

            var result = await container.DeleteItemAsync<object>(itemId, new PartitionKey(partitionKey), cancellationToken: cancellationToken).ConfigureAwait(false);
            
            return await dc.EndDialogAsync(result: result.Resource, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
