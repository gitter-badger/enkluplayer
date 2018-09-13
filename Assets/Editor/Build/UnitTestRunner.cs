using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework.Interfaces;
using UnityEditor;
using UnityEngine.Events;

namespace CreateAR.EnkluPlayer.Editor
{
    /// <summary>
    /// Runs unit tests from an internal API.
    /// </summary>
    public static class UnitTestRunner
    {
        /// <summary>
        /// TestRunner assembly.
        /// </summary>
        private const string EDITOR_TESTRUNNER_ASSEMBLY = "UnityEditor.TestRunner, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

        /// <summary>
        /// Engine assembly.
        /// </summary>
        private const string ENGINE_TESTRUNNER_ASSEMBLY = "UnityEngine.TestRunner, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

        /// <summary>
        /// Field we poll for completion.
        /// </summary>
        private static FieldInfo _isRunning;

        /// <summary>
        /// Runner!
        /// </summary>
        private static IUnitTestListener _listener;

        /// <summary>
        /// Runs unit tests.
        /// </summary>
        /// <param name="listener">Listener object.</param>
        public static void Run(IUnitTestListener listener)
        {
            _listener = listener;

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
            
            UnityEditor.EditorApplication.update += OnUpdate;
        }

        /// <summary>
        /// Called on EditorUpdate.
        /// </summary>
        private static void OnUpdate()
        {
            if ((bool) _isRunning.GetValue(null))
            {
                return;
            }

            UnityEditor.EditorApplication.update -= OnUpdate;

            _listener.OnFinishedAll();
        }

        /// <summary>
        /// Loads a type from an assembly.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly to load from.</param>
        /// <param name="name">Name of the type to load.</param>
        /// <returns></returns>
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