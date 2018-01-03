using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CreateAR.Commons.Unity.Logging;
using NUnit.Framework.Interfaces;
using UnityEditor;
using UnityEngine.Events;

namespace CreateAR.SpirePlayer.Editor
{
    public class TestListener
    {
        public void OnFinished(ITestResult result)
        {
            Log.Debug(this, "CALLED!!!!!");
        }
    }

    public static class ExperimentalTools
    {
        private const string EDITOR_TESTRUNNER_ASSEMBLY = "UnityEditor.TestRunner, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        private const string ENGINE_TESTRUNNER_ASSEMBLY = "UnityEngine.TestRunner, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

        private static TestListener _listener;

        /// <summary>
        /// Unity has no API to run tests programmatically, thus-- this method.
        /// </summary>
        [MenuItem("Hacks/Run Tests! (v9)")]
        public static void RunTests()
        {
            if (Log.Targets.Length == 0)
            {
                Log.AddLogTarget(new FileLogTarget(
                    new DefaultLogFormatter(),
                    Path.Combine(
                        UnityEngine.Application.persistentDataPath,
                        "UnitTests.log")));
                Log.AddLogTarget(new UnityLogTarget(new DefaultLogFormatter()));
            }

            _listener = new TestListener();
            
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
            
            var methods = finishedEvent
                .GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .ToArray();

            MethodInfo addListenerMethod = null;
            foreach (var method in methods)
            {
                if (method.GetParameters().Length == 1 && method.Name == "AddListener")
                {
                    addListenerMethod = method;
                    break;
                }
            }

            var action = new UnityAction<ITestResult>(_listener.OnFinished);
            addListenerMethod.Invoke(finishedEvent, new object[] { action });
            
            var runMethod = launcher.GetType().GetMethod("Run", BindingFlags.Instance | BindingFlags.Public);
            runMethod.Invoke(launcher, null);
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