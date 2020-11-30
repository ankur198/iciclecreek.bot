﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Lucy;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Tests
{
    [TestClass]
    public class LucyRecognizerTests
    {
        public static ResourceExplorer ResourceExplorer { get; set; }

        public static string Json { get; set; }

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext context)
        {
            ComponentRegistration.Add(new DeclarativeComponentRegistration());
            ComponentRegistration.Add(new DialogsComponentRegistration());
            ComponentRegistration.Add(new AdaptiveComponentRegistration());
            ComponentRegistration.Add(new LanguageGenerationComponentRegistration());
            ComponentRegistration.Add(new AdaptiveTestingComponentRegistration());
            //            ComponentRegistration.Add(new QLuceneComponentRegistration());

            var parent = Environment.CurrentDirectory;
            while (!string.IsNullOrEmpty(parent))
            {
                if (System.IO.Directory.EnumerateFiles(parent, "*proj").Any())
                {
                    break;
                }
                else
                {
                    parent = System.IO.Path.GetDirectoryName(parent);
                }
            }

            ResourceExplorer = new ResourceExplorer();
            ResourceExplorer.ResourceTypes.Add("yaml");
            ResourceExplorer.AddFolder(parent, monitorChanges: false);

        }

        [TestMethod]
        public async Task TestEntityResultMapping()
        {
            var recognizer = new LucyRecognizer()
            {
                ResourceId = "lucy.yaml",
                ExternalEntityRecognizer = new MockLuisRecognizer()
            };
            var activity = new Activity(ActivityTypes.Message) { Text = "height is 6 inches" };
            var tc = new TurnContext(new TestAdapter(), activity);
            tc.TurnState.Add(ResourceExplorer);
            var dc = new DialogContext(new DialogSet(), tc, new DialogState());
            var results = await recognizer.RecognizeAsync(dc, activity);

            Assert.IsNotNull(results.Entities.Property("Add()"));
            Assert.IsNotNull(results.Entities.Property("number"));
            Assert.IsNull(results.Entities.Property("foo"));

            dynamic heightProperty = results.Entities["Add()"][0]["heightProperty"];
            Assert.IsNotNull(heightProperty);

            dynamic dimension = heightProperty[0].dimension;
            Assert.IsNotNull(dimension);

            dynamic resolution = dimension[0];
            Assert.AreEqual(6, (int)resolution.number);
            Assert.AreEqual("Inch", (string)resolution.units);

            dynamic dimensionInstance = heightProperty[0]["$instance"]["dimension"][0];
            Assert.AreEqual("dimension", (string)dimensionInstance.type);
            Assert.AreEqual(10, (int)dimensionInstance.startIndex);
            Assert.AreEqual(18, (int)dimensionInstance.endIndex);
            Assert.AreEqual("6 inches", (string)dimensionInstance.text);

            dynamic instanceEntities = results.Entities["$instance"];
            Assert.IsNotNull(instanceEntities);
            Assert.IsNotNull(instanceEntities.Property("Add()"));
            Assert.IsNotNull(instanceEntities.Property("number"));
            Assert.IsNull(instanceEntities.Property("foo"));
        }

        [TestMethod]
        public async Task TestIntents()
        {
            var recognizer = new LucyRecognizer()
            {
                ResourceId = "lucy.yaml",
                Intents = new string[] { "Add()" },
                ExternalEntityRecognizer = new MockLuisRecognizer()
            };
            var activity = new Activity(ActivityTypes.Message) { Text = "height is 6 inches" };
            var tc = new TurnContext(new TestAdapter(), activity);
            tc.TurnState.Add(ResourceExplorer);
            var dc = new DialogContext(new DialogSet(), tc, new DialogState());
            var results = await recognizer.RecognizeAsync(dc, activity);

            Assert.AreEqual(1.0f, results.Intents["Add()"].Score);
        }

    }
}