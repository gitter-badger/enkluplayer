using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace CreateAR.SpirePlayer.Editor
{
    public class TestListener
    {
        public void OnFinished(ITestResult result)
        {
            Debug.Log("CALLED!!!!!");
        }
    }

    public static class ExperimentalTools
    {
        private const string EDITOR_TESTRUNNER_ASSEMBLY = "UnityEditor.TestRunner, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        private const string ENGINE_TESTRUNNER_ASSEMBLY = "UnityEngine.TestRunner, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        private const string ENGINE_COREMODULE = "UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

        /// <summary>
        /// Unity has no API to run tests programmatically, thus-- this method.
        /// </summary>
        [MenuItem("Hacks/Run Tests")]
        public static void RunTests()
        {
            var listener = new TestListener();

            var launcherType = LoadType(EDITOR_TESTRUNNER_ASSEMBLY, "UnityEditor.TestTools.TestRunner.EditModeLauncher");
            var filterType = LoadType(ENGINE_TESTRUNNER_ASSEMBLY, "UnityEngine.TestTools.TestRunner.GUI.TestRunnerFilter");
            
            var filter = Activator.CreateInstance(filterType);
            var launcher = Activator.CreateInstance(launcherType, new object[] { filter });
            var runner = launcher
                .GetType()
                .GetField("m_EditModeRunner", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(launcher);
            var finishedEvent = runner
                .GetType()
                .GetField("m_TestFinishedEvent", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(runner);

            var action = new UnityAction<ITestResult>(listener.OnFinished);

            Debug.Log(finishedEvent.GetType().FullName);

            var addListenerMethod = finishedEvent
                .GetType()
                .GetMethod("AddListener", BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var param in addListenerMethod.GetParameters())
            {
                Debug.Log(param.ParameterType.Name);
            }
            
            addListenerMethod.Invoke(finishedEvent, new object[] { action });
            
            var runMethod = launcher.GetType().GetMethod("Run", BindingFlags.Instance | BindingFlags.Public);
            runMethod.Invoke(launcher, new object[0]);
        }

        private static Type LoadType(string assemblyName, string name)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (assemblyName == assembly.FullName)
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (type.FullName == name)
                        {
                            return type;
                        }
                    }
                }
            }

            return null;
        }
    }
}