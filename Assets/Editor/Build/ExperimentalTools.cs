using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CreateAR.Commons.Unity.Logging;
using NUnit.Framework.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace CreateAR.SpirePlayer.Editor
{
    public class TestListener
    {
        private readonly List<ITestResult> _results = new List<ITestResult>();
        
        public void OnFinishedTest(ITestResult result)
        {
            _results.Add(result);
        }

        public void OnFinishedAll()
        {
            var builder = new StringBuilder();
            foreach (var result in _results)
            {
                if (result.FailCount > 0)
                {
                    builder.AppendFormat("Test : {0}\n", result.FullName);
                    builder.AppendFormat("\tFailed {0} tests.\n", result.FailCount);
                    builder.AppendFormat("\t{0}\n", result.Output);
                    builder.AppendFormat("{0}\n\n", result.StackTrace);
                }
            }

            if (builder.Length > 0)
            {
                Debug.LogError(builder.ToString());

                throw new Exception("Tests failed.");
            }

            Debug.Log("All tests passed.");
        }
    }

    public static class ExperimentalTools
    {
        private const string EDITOR_TESTRUNNER_ASSEMBLY = "UnityEditor.TestRunner, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        private const string ENGINE_TESTRUNNER_ASSEMBLY = "UnityEngine.TestRunner, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

        private static FieldInfo _isRunning;
        private static TestListener _listener;

        /// <summary>
        /// Unity has no API to run tests programmatically, thus-- this method.
        /// </summary>
        [MenuItem("Tools/Run Tests (EXPERIMENTAL)")]
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

            _isRunning = runner.GetType().GetField("RunningTests",
                BindingFlags.Static | BindingFlags.Public);
            if (null == _isRunning)
            {
                throw new Exception("Could not find IsRunning.");
            }
            
            var testFinishedEvent = runner
                .GetType()
                .GetField("m_TestFinishedEvent", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(runner);
            
            var methods = testFinishedEvent
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

            var action = new UnityAction<ITestResult>(_listener.OnFinishedTest);
            addListenerMethod.Invoke(testFinishedEvent, new object[] { action });
            
            var runMethod = launcher.GetType().GetMethod("Run", BindingFlags.Instance | BindingFlags.Public);
            runMethod.Invoke(launcher, null);
            
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            if ((bool) _isRunning.GetValue(null))
            {
                return;
            }

            EditorApplication.update -= OnUpdate;

            _listener.OnFinishedAll();
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