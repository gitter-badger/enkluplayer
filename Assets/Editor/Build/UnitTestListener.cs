using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework.Interfaces;
using UnityEngine;

namespace CreateAR.SpirePlayer.Editor
{
    /// <summary>
    /// Default implementation of <c>IUnitTestListener</c>.
    /// </summary>
    public class UnitTestListener : IUnitTestListener
    {
        /// <summary>
        /// Results.
        /// </summary>
        private readonly List<ITestResult> _results = new List<ITestResult>();
        
        /// <inheritdoc cref="IUnitTestListener"/>
        public void OnFinishedTest(ITestResult result)
        {
            _results.Add(result);
        }

        /// <inheritdoc cref="IUnitTestListener"/>
        public void OnFinishedAll()
        {
            var builder = new StringBuilder();
            foreach (var result in _results)
            {
                if (result.FailCount > 0
                    // tests without stack traces are parents to the failed test
                    && !string.IsNullOrEmpty(result.StackTrace))
                {
                    builder.AppendFormat("Test Failed! : {0}\n", result.FullName);
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
}