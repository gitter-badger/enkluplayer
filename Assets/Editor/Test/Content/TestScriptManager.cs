using System.Collections.Generic;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    public class TestScriptManager : IScriptManager
    {
        public EnkluScript FindOne(string id)
        {
            throw new System.NotImplementedException();
        }

        public void FindAll(string id, List<EnkluScript> scripts)
        {
            throw new System.NotImplementedException();
        }

        public EnkluScript FindOneTagged(string query)
        {
            throw new System.NotImplementedException();
        }

        public void FindAllTagged(string query, List<EnkluScript> scripts)
        {
            throw new System.NotImplementedException();
        }

        public EnkluScript Create(string scriptId, params string[] tags)
        {
            throw new System.NotImplementedException();
        }

        public void Send(string query, string name, params object[] parameters)
        {
            throw new System.NotImplementedException();
        }

        public void Release(EnkluScript script)
        {
            throw new System.NotImplementedException();
        }

        public void ReleaseAll(params string[] tags)
        {
            throw new System.NotImplementedException();
        }
    }
}