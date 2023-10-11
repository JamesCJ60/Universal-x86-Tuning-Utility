using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Resources;
using System.Windows;

namespace Universal_x86_Tuning_Utility.Scripts.UXTU_Super_Resolution
{
    internal class ScaleModelManager
    {
        private readonly FileSystemWatcher scaleModelsWatcher = new();

        private ScaleModel[]? scaleModels = null;

        public event Action? ScaleModelsChanged;

        public ScaleModelManager()
        {
            LoadFromLocal();

            // 监视ScaleModels.json的更改
            scaleModelsWatcher.Path = AppDomain.CurrentDomain.BaseDirectory;
            scaleModelsWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            scaleModelsWatcher.Filter = App.SCALE_MODELS_JSON_PATH.Substring(App.SCALE_MODELS_JSON_PATH.LastIndexOf('\\') + 1);
            scaleModelsWatcher.Changed += ScaleModelsWatcher_Changed;
            scaleModelsWatcher.Deleted += ScaleModelsWatcher_Changed;
            try
            {
                scaleModelsWatcher.EnableRaisingEvents = true;
            }
            catch (FileNotFoundException e)
            {

            }
        }

        public ScaleModel[]? GetScaleModels()
        {
            return scaleModels;
        }

        public bool IsValid()
        {
            return scaleModels != null && scaleModels.Length > 0;
        }

        private void LoadFromLocal()
        {
            string json = "";
            if (File.Exists(App.SCALE_MODELS_JSON_PATH))
            {
                try
                {
                    json = File.ReadAllText(App.SCALE_MODELS_JSON_PATH);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                try
                {
                    Uri uri = new("pack://application:,,,/Assets/BuiltInScaleModels.json", UriKind.Absolute);
                    StreamResourceInfo info = Application.GetResourceStream(uri);
                    using (StreamReader reader = new(info.Stream))
                    {
                        json = reader.ReadToEnd();
                    }
                    File.WriteAllText(App.SCALE_MODELS_JSON_PATH, json);
                }
                catch (Exception e)
                {
                }
            }

            try
            {
                // 解析缩放配置
                scaleModels = JsonNode.Parse(
                    json,
                    new JsonNodeOptions { PropertyNameCaseInsensitive = false },
                    new JsonDocumentOptions
                    {
                        CommentHandling = JsonCommentHandling.Skip,
                        AllowTrailingCommas = true
                    }
                )?.AsArray().Select(model => {
                    if (model == null)
                    {
                        throw new Exception("json 非法");
                    }

                    JsonNode name = model["name"] ?? throw new Exception("未找到 name 字段");
                    JsonNode effects = model["effects"] ?? throw new Exception("未找到 effects 字段");

                    return new ScaleModel
                    {
                        Name = name.GetValue<string>(),
                        Effects = effects.ToJsonString()
                    };
                }).ToArray();

                if (scaleModels == null || scaleModels.Length == 0)
                {
                    throw new Exception("解析 json 失败");
                }
            }
            catch (Exception e)
            {
                scaleModels = null;
            }

            if (ScaleModelsChanged != null)
            {
                ScaleModelsChanged.Invoke();
            }
        }

        private void ScaleModelsWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Thread.Sleep(10);
            Application.Current.Dispatcher.Invoke(LoadFromLocal);
        }

        public class ScaleModel
        {
            public string Name { get; set; } = "";

            public string Effects { get; set; } = "";
        }
    }
}
