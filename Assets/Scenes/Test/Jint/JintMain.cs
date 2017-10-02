using System;
using Jint.Unity;
using UnityEngine;

namespace CreateAR.SpirePlayer.Test
{
    public class StrangeScriptDependencyResolver : IScriptDependencyResolver
    {
        public object Resolve(string name)
        {
            return null;
        }
    }

    public class JintMain : MonoBehaviour
    {
	    private readonly UnityScriptingHost _host = new UnityScriptingHost(
            new ResourcesScriptLoader(),
            new StrangeScriptDependencyResolver());

        private void Awake()
        {
            _host.SetValue("foo", new Action<int>(Foo));
            _host.Execute("var a = 5;foo(a);");
        }

        private void Foo(int bar)
        {
            Debug.Log(string.Format("Foo({0})", bar));
        }
    }
}