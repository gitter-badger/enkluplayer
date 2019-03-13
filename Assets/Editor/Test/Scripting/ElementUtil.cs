using System;
using System.Collections.Generic;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Test.UI;
using Enklu.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    public static class ElementUtil
    {
        public static Element CreateElement(TestScriptManager scriptManager, params EnkluScript[] scripts)
        {
            var element = new Element();
            AddScriptToElement(element, scriptManager, scripts);
            return element;
        }

        public static ContentWidget CreateContentWidget(TestScriptManager scriptManager, params EnkluScript[] scripts)
        {
            return CreateContentWidget(scriptManager, new DummyAssetAssembler(), scripts);
        }

        public static ContentWidget CreateContentWidget(TestScriptManager scriptManager, IAssetAssembler assetAssembler, params EnkluScript[] scripts)
        {
            var widget = new ContentWidget(
                new GameObject("WidgetUtil"), 
                null, 
                new TweenConfig(), 
                new ColorConfig(), 
                assetAssembler);
            AddScriptToElement(widget, scriptManager, scripts);
            
            // Will all tests require this, or need it deferred?
            widget.Load(new ElementData(), widget.Schema, new Element[] {});
            widget.FrameUpdate();
            return widget;
        }

        public static void AddScriptToElement(Element element, TestScriptManager scriptManager,  params EnkluScript[] scripts)
        {
            var existingScripts = JArray.Parse(
                element.Schema.GetOwn("scripts", "[]").Value);

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

            element.Schema.Set("scripts", JsonConvert.SerializeObject(existingScripts));
        }

        public static void RemoveScriptFromElement(Element element, TestScriptManager scriptManager, EnkluScript script)
        {
            var existingScripts = JArray.Parse(
                element.Schema.GetOwn("scripts", "[]").Value);

            for (var i = 0; i < existingScripts.Count; i++)
            {
                if (existingScripts[i]["id"].ToObject<string>() == script.Data.Id)
                {
                    existingScripts.RemoveAt(i);
                    scriptManager.RemoveEntry(script.Data.Id);
                    element.Schema.Set("scripts", JsonConvert.SerializeObject(existingScripts));
                    return;
                }
            }
            
            throw new ArgumentException("Script not present on Widget");
        }
    }
}