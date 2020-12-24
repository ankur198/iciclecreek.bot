﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Blazorise;
using BlazorMonacoYaml;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Lucy;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LucyPad2.Client
{
    public partial class Index
    {
        private MonacoEditorYaml yamlEditor;
        private Alert alertBox;
        private string selectedExample;

        private LucyEngine engine = null;
        private LucyRecognizer recognizer = null;
        private string lucyModel = null;
        private PatternModelConverter patternModelConverter = new PatternModelConverter();

        private IDeserializer yamlDeserializer = new DeserializerBuilder()
                                                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                                    .Build();
        private ISerializer yamlToJsonSerializer = new SerializerBuilder()
                                                .JsonCompatible()
                                                .Build();

        public Index()
        {
        }

        [Inject]
        public HttpClient Http { get; set; }

        public bool TopResultVisible = true;

        public bool AllResultsVisible = false;

        public string Yaml { get; set; }

        public string TopResult { get; set; }

        public string EntityResults { get; set; }

        public string Text { get; set; }

        public string Error { get; set; } = "---";

        public string Elapsed { get; set; }

        protected override void OnInitialized()
        {
            this.selectedExample = "simple";
            this.Yaml = LoadResource("LucyPad2.Client.Samples.simple.lucy.yaml");
        }

        async Task OnSelectedExampleChanged(string value)
        {
            this.selectedExample = value;
            this.Yaml = LoadResource($"LucyPad2.Client.Samples.{value}.lucy.yaml");
            await yamlEditor.SetValue(this.Yaml);
        }

        private string LoadResource(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(name))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }

        }

        async Task OnTextChanged(string value)
        {
            try
            {
                var yaml = await yamlEditor.GetValue();
                var text = value.Trim() ?? string.Empty;
                IEnumerable<LucyEntity> results = null;
                if (text.Length == 0)
                {
                    return;
                }
#if embedded
                if (lucyModel != yaml)
                {
                    LoadModel(yaml);
                }

                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                results = engine.MatchEntities(text);
                sw.Stop();
                this.Elapsed = $"{sw.Elapsed.TotalMilliseconds} ms";
#else
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                var result = await Http.PostAsJsonAsync("entities", new EntitiesRequest
                {
                    yaml = yaml,
                    text = text
                });
                sw.Stop();
                // this.Elapsed = $"Network: {sw.Elapsed.TotalMilliseconds} ms";
                var entityResponse = JsonConvert.DeserializeObject<EntitiesResponse>(await result.Content.ReadAsStringAsync());

                this.Elapsed = $"{entityResponse.elapsed} ms";

                if (!String.IsNullOrEmpty(entityResponse.message))
                {
                    this.Error = entityResponse.message;
                    this.alertBox.Show();
                }
                else
                {
                    this.alertBox.Hide();
                }
                results = entityResponse.entities;
#endif

                this.TopResultVisible = true;
                this.AllResultsVisible = false;
                this.TopResult = LucyEngine.VisualEntities(text, results);
                this.EntityResults = String.Join("\n\n", results.Select(entity => LucyEngine.VisualizeEntity(text, entity)));
            }
            catch (SemanticErrorException err)
            {
                this.Error = err.Message;
                this.alertBox.Show();
                //this.editor.ScrollToLine(err.Start.Line);
                //var line = this.editor.Document.GetLineByNumber(err.Start.Line - 1);
                //this.editor.Select(line.Offset, line.Length);
            }
            catch (SyntaxErrorException err)
            {
                this.Error = err.Message;
                this.alertBox.Show();
                //this.error.Visibility = Visibility.Visible;
                //this.editor.ScrollToLine(err.Start.Line);
                //var line = this.editor.Document.GetLineByNumber(err.Start.Line - 1);
                //this.editor.Select(line.Offset, line.Length);
            }
            catch (Exception err)
            {
                this.Error = err.Message;
                this.alertBox.Show();
                //this.error.Content = err.Message;
                //this.error.Visibility = Visibility.Visible;
            }
        }

        private void LoadModel(string yaml)
        {
            // Trace.TraceInformation("Loading model");
            var reader = new StringReader(yaml);
            var x = yamlDeserializer.Deserialize(new StringReader(yaml));
            var json = yamlToJsonSerializer.Serialize(x);
            var model = JsonConvert.DeserializeObject<LucyModel>(json, patternModelConverter);
            engine = new LucyEngine(model, useAllBuiltIns: true);
            recognizer = new LucyRecognizer()
            {
                Model = model,
            };

            // this.examplesBox.Text = sb.ToString();

            if (engine.Warnings.Any())
            {
                this.Error = String.Join("\n", engine.Warnings);
                this.alertBox.Show();
            }
            else
            {
                this.Error = string.Empty;
                this.alertBox.Hide();
            }
            lucyModel = yaml;
        }
    }
}