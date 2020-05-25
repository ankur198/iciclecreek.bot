﻿using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Iciclecreek.Bot.Builder.Dialogs.Database.Cosmos
{
    /// <summary>
    /// Create cosmos db item in container
    /// </summary>
    public class ReplaceItem : Dialog
    {
        [JsonProperty("$kind")]
        public const string Kind = "Iciclecreek.Cosmos.ReplaceItem";

        [JsonConstructor]
        public ReplaceItem([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets the disabled state for the action.
        /// </summary>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; }

        /// <summary>
        /// Gets or sets the ConnectionString for querying the database.
        /// </summary>
        [JsonProperty("connectionString")]
        public StringExpression ConnectionString { get; set; }

        /// <summary>
        /// database name
        /// </summary>
        [JsonProperty("database")]
        public StringExpression Database { get; set; }

        /// <summary>
        /// Container name
        /// </summary>
        [JsonProperty("container")]
        public StringExpression Container { get; set; }

        /// <summary>
        /// Item 
        /// </summary>
        [JsonProperty("item")]
        public ObjectExpression<object> Item { get; set; }

        /// <summary>
        /// Item Id (default will be to look for id on the Item object itself)
        /// </summary>
        [JsonProperty("itemId")]
        public StringExpression ItemId { get; set; }

        /// <summary>
        /// PartitionKey value
        /// </summary>
        [JsonProperty("partitionKey")]
        public StringExpression PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the property path to store the query result in.
        /// </summary>
        /// <value>
        /// The property path to store the dialog result in.
        /// </value>
        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (this.Disabled != null && this.Disabled.GetValue(dc.State) == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            var connectionString = ConnectionString.GetValue(dc.State);
            var databaseName = Database.GetValue(dc.State);
            var containerName = Container.GetValue(dc.State);
            var item = Item.GetValue(dc.State);
            var itemId = ItemId?.GetValue(dc.State) ?? ObjectPath.GetPathValue<string>(item, "id") ?? ObjectPath.GetPathValue<string>(item, "Id");
            var partitionKeyValue = PartitionKey.GetValue(dc.State);
            PartitionKey? partitionKey = (partitionKeyValue != null) ? new PartitionKey(partitionKeyValue) : (PartitionKey?)null;
            var client = CosmosClientCache.GetClient(connectionString);
            var database = client.GetDatabase(databaseName);
            var container = database.GetContainer(containerName);

            var result = await container.ReplaceItemAsync(item, itemId, partitionKey: partitionKey, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (this.ResultProperty != null)
            {
                dc.State.SetValue(this.ResultProperty.GetValue(dc.State), result.Resource);
            }

            return await dc.EndDialogAsync(result: result.Resource, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
