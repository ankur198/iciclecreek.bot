
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Lucy.Tests
{
    [TestClass]
    public class SerializationTests
    {
        private JsonConverter patternModelConverter = new PatternModelConverter();

        private IDeserializer yamlDeserializer = new DeserializerBuilder()
                                                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                                    .Build();
        private ISerializer yamlSerializer = new SerializerBuilder()
                                                .JsonCompatible()
                                                .Build();

        [TestMethod]
        public void TestLoadYaml()
        {
            LucyModel lucyModel;
            using (var streamReader = new StreamReader(File.OpenRead(Path.Combine(@"..", "..", "..", "lucy.yml"))))
            {
                var x = yamlDeserializer.Deserialize(streamReader);
                var json = yamlSerializer.Serialize(x);
                lucyModel = JsonConvert.DeserializeObject<LucyModel>(json, patternModelConverter);
            }

            var engine = new LucyEngine(lucyModel);

            string text = "the box is 9 inches by 7.";

            var results = engine.MatchEntities(text);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));
            Trace.TraceInformation("\n" + LucyEngine.VizualizeResultsAsHierarchy(text, results));

            var entities = results.Where(e => e.Type == "boxSize").ToList();
            Assert.AreEqual(1, entities.Count);
            var entity = entities.Single().Children.Single();
            Assert.AreEqual("twoDimensional", entity.Type);
            Assert.AreEqual(2, entity.Children.Count);
            Assert.AreEqual(1, entity.Children.Where(e => e.Type == "number").Count());
            Assert.AreEqual(1, entity.Children.Where(e => e.Type == "dimension").Count());
        }

        [TestMethod]
        public void TestLoadMacroTest()
        {
            LucyModel lucyModel;
            using (var streamReader = new StreamReader(File.OpenRead(Path.Combine(@"..", "..", "..", "lucy.yml"))))
            {
                var x = yamlDeserializer.Deserialize(streamReader);
                var json = yamlSerializer.Serialize(x);
                lucyModel = JsonConvert.DeserializeObject<LucyModel>(json, patternModelConverter);
            }

            var engine = new LucyEngine(lucyModel);

            string text = "flight from seattle to chicago.";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));
            Trace.TraceInformation("\n" + LucyEngine.VizualizeResultsAsHierarchy(text, results));

            Assert.AreEqual(1, results.Where(e => e.Type == "trip").Count());
            Assert.AreEqual(1, results.Where(e => e.Type == "destination").Count());
            Assert.AreEqual(1, results.Where(e => e.Type == "departure").Count());
            Assert.AreEqual(2, results.Where(e => e.Type == "placeAndTime").Count());
            Assert.AreEqual(2, results.Where(e => e.Type == "city").Count());
        }
    }
}