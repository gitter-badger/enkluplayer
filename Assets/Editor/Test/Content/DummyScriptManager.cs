using System;
using System.Collections.Generic;

namespace CreateAR.EnkluPlayer.Test
{
    public class DummyScriptManager : IScriptManager
    {
        public EnkluScript FindOne(string id)
        {
            throw new NotImplementedException();
        }

        public void FindAll(string id, List<EnkluScript> scripts)
        {
            throw new NotImplementedException();
        }

        public EnkluScript FindOneTagged(string query)
        {
            throw new NotImplementedException();
        }

        public void FindAllTagged(string query, List<EnkluScript> scripts)
        {
            throw new NotImplementedException();
        }

        public EnkluScript Create(string scriptId, params string[] tags)
        {
            throw new NotImplementedException();
        }

        public void Send(string query, string name, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public void Release(EnkluScript script)
        {
            throw new NotImplementedException();
        }

        public void ReleaseAll(params string[] tags)
        {
            throw new NotImplementedException();
        }
    }
}