﻿using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Runtime.Plugins;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Iciclecreek.Bot.Builder.Dialogs.Files
{
    /// <summary>
    /// Class which contains registration of components for CosmosDB.
    /// </summary>
    public class FilesComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes, IBotPlugin
    {
        public void Load(IBotPluginLoadContext context)
        {
            Add(new FilesComponentRegistration());
        }

        /// <summary>
        /// Gets declarative type registrations for QnAMAker.
        /// </summary>
        /// <param name="resourceExplorer">resourceExplorer to use for resolving references.</param>
        /// <returns>enumeration of DeclarativeTypes.</returns>
        public IEnumerable<DeclarativeType> GetDeclarativeTypes(ResourceExplorer resourceExplorer)
        {
            yield return new DeclarativeType<ReadTextFile>(ReadTextFile.Kind);
            yield return new DeclarativeType<WriteTextFile>(WriteTextFile.Kind);
            yield return new DeclarativeType<DeleteFile>(DeleteFile.Kind);
        }

        /// <summary>
        /// Gets JsonConverters for DeclarativeTypes for QnAMaker.
        /// </summary>
        /// <param name="resourceExplorer">resourceExplorer to use for resolving references.</param>
        /// <param name="sourceContext">SourceContext to build debugger source map.</param>
        /// <returns>enumeration of json converters.</returns>
        public IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            yield break;
        }
    }
}
