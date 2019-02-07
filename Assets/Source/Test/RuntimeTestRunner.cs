using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Test
{
    /// <summary>
    /// Test result for a single test.
    /// </summary>
    public class RuntimeTestResult
    {
        /// <summary>
        /// Name of the test.
        /// </summary>
        public string Name;
        
        /// <summary>
        /// True iff successful.
        /// </summary>
        public bool Success;
        
        /// <summary>
        /// The error, if Success is false.
        /// </summary>
        public string Error;

        /// <inheritdoc />
        public override string ToString()
        {
            if (Success)
            {
                return string.Format(
                    "\tSuccess\t: [{0}]",
                    Name);
            }

            return string.Format(
                "\tFailure\t: [{0}] {1}",
                Name,
                Error);
        }
    }
    
    /// <summary>
    /// Result for a whole fixture of tests.
    /// </summary>
    public class RuntimeTestFixtureResult
    {
        /// <summary>
        /// Name of the test.
        /// </summary>
        public string Name;
        
        /// <summary>
        /// List of all tests.
        /// </summary>
        public readonly List<RuntimeTestResult> Tests = new List<RuntimeTestResult>();

        /// <summary>
        /// True iff all tests in fixture were successful.
        /// </summary>
        public bool Success
        {
            get
            {
                foreach (var test in Tests)
                {
                    if (!test.Success)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
        
        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendFormat("\n\nFixture [{0}]: {1}\n",
                Name,
                Success ? "Success" : "Failure");
            foreach (var test in Tests)
            {
                builder.AppendLine(test.ToString());
            }
            
            return builder.ToString();
        }
    }

    /// <summary>
    /// Test results for multiple fixtures.
    /// </summary>
    public class RuntimeTestSuiteResult
    {
        /// <summary>
        /// Results of fixtures.
        /// </summary>
        public readonly List<RuntimeTestFixtureResult> Fixtures = new List<RuntimeTestFixtureResult>();
    
        /// <summary>
        /// True iff all fixtures report success.
        /// </summary>
        public bool Success
        {
            get
            {
                foreach (var fixture in Fixtures)
                {
                    if (!fixture.Success)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendFormat(
                "{0}!\n\nTest Result: {0}\n===============\n",
                Success ? "Success" : "Failure");

            foreach (var fixture in Fixtures)
            {
                builder.Append(fixture);
            }
               
            return builder.ToString();
        }
    }
    
    /// <summary>
    /// Runs all unit tests.
    /// </summary>
    public class RuntimeTestRunner : MonoBehaviour
    {
        /// <summary>
        /// When true, runs tests.
        /// </summary>
        public bool Run;

        /// <summary>
        /// True iff tests are currently running.
        /// </summary>
        private bool _isRunning = false;
        
        /// <summary>
        /// The result of all tests.
        /// </summary>
        private RuntimeTestSuiteResult _result;

        /// <inheritdoc />
        private void Update()
        {
            if (!Run)
            {
                return;
            }

            Run = false;

            if (_isRunning)
            {
                return;
            }
            
            _isRunning = true;
            StartCoroutine(StartTests());
        }

        /// <summary>
        /// Retrieves assemblies.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Assembly> GetAssemblies()
        {
#if NETFX_CORE || (!UNITY_EDITOR && UNITY_WSA)
            // TODO: https://stackoverflow.com/questions/44813060/equivalence-for-appdomain-getassemblies-in-uwp
            return new Assembly[0];
#else
            return AppDomain.CurrentDomain.GetAssemblies();
#endif
        }

        /// <summary>
        /// Runs tests.
        /// </summary>
        private IEnumerator StartTests()
        {
            _result = new RuntimeTestSuiteResult();
            
            var tests = new List<MethodInfo>();

            foreach (var assembly in GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.GetCustomAttributes(typeof(RuntimeTestFixtureAttribute), true).Any())
                    {
                        var methods = type.GetMethods();
                        
                        tests.Clear();
                        MethodInfo setup = null, setupFixture = null;
                        
                        foreach (var method in methods)
                        {
                            if (method.GetCustomAttributes(typeof(RuntimeSetUpAttribute), true).Any())
                            {
                                setup = method;
                            }
                            else if (method.GetCustomAttributes(typeof(RuntimeSetUpFixtureAttribute), true).Any())
                            {
                                setupFixture = method;
                            }
                            else if (method.GetCustomAttributes(typeof(RuntimeTestAttribute), true).Any())
                            {
                                tests.Add(method);
                            }
                        }

                        if (0 == tests.Count)
                        {
                            continue;
                        }

                        var fixtureResults = new RuntimeTestFixtureResult
                        {
                            Name = type.Name
                        };
                        _result.Fixtures.Add(fixtureResults);
                        
                        var instance = Activator.CreateInstance(type);
                        var nullArgs = new object[0];

                        if (null != setupFixture)
                        {
                            setupFixture.Invoke(instance, nullArgs);

                            yield return null;
                        }

                        foreach (var test in tests)
                        {
                            if (null != setup)
                            {
                                setup.Invoke(instance, nullArgs);
                            }

                            var testResult = new RuntimeTestResult
                            {
                                Name = test.Name
                            };

                            try
                            {
                                test.Invoke(instance, nullArgs);
                                testResult.Success = true;
                            }
                            catch (TargetInvocationException exception)
                            {
                                testResult.Success = false;
                                testResult.Error = exception.InnerException.Message;
                            }
                            
                            fixtureResults.Tests.Add(testResult);

                            yield return null;
                        }
                    }
                }
            }
            
            Log.Info(this, _result);

            _isRunning = false;
        }
    }
}