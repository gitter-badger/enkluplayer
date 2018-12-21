using System.Collections.Generic;
using CreateAR.EnkluPlayer.IUX;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    public static class WidgetUtil
    {
        public static Widget CreateWidget()
        {
            return new Widget(new GameObject("ScriptRunner_Tests"), null, null, null);
        }
        
        public static Widget CreateWidget(TestScriptManager scriptManager, params EnkluScript[] scripts)
        {
            var widget = new Widget(new GameObject("ScriptRunner_Tests"), null, null, null);
            AddScriptToWidget(widget, scriptManager, scripts);
            return widget;
        }

        public static void AddScriptToWidget(Widget widget, TestScriptManager scriptManager,  params EnkluScript[] scripts)
        {
            var existingScripts = JArray.Parse(
                widget.Schema.GetOwn("scripts", "[]").Value);

            for (int i = 0, len = scripts.Length; i < len; i++)
            {
                var script = scripts[i];
                
                if (!existingScripts.Contains(script.Data.Id))
                {
                    existingScripts.Add(JToken.FromObject(new Dictionary<string, string>
                    {
                        { "id", script.Data.Id }
                    }));
                
                    scriptManager.AddEntry(script.Data.Id, script);
                }
            }

            widget.Schema.Set("scripts", JsonConvert.SerializeObject(existingScripts));
        }
    }
}