using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.SpirePlayer.Test
{
    public class RuntimeTestResult
    {
        public string Name;
        public bool Success;
        public string Error;

        public override string ToString()
        {
            if (Success)
            {
                return string.Format("\t{0}\t:\tSuccess",
                    Name);
            }

            return string.Format(
                "\t{0}\t:\tFailure - {1}",
                Name,
                Error);
        }
    }
    
    public class RuntimeTestFixtureResult
    {
        public string Name;
        public readonly List<RuntimeTestResult> Tests = new List<RuntimeTestResult>();

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

    public class RuntimeTestSuiteResult
    {
        public readonly List<RuntimeTestFixtureResult> Fixtures = new List<RuntimeTestFixtureResult>();

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
    
    public class RuntimeTestRunner : MonoBehaviour
    {
        public bool Run;

        private bool _isRunning = false;
        private RuntimeTestSuiteResult _result;

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

        private IEnumerator StartTests()
        {
            _result = new RuntimeTestSuiteResult();
            
            var tests = new List<MethodInfo>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
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