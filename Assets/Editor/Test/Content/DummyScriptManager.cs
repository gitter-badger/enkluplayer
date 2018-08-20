using System;
using System.Collections.Generic;

namespace CreateAR.SpirePlayer.Test
{
    public class DummyScriptManager : IScriptManager
    {
        public SpireScript FindOne(string id)
        {
            throw new NotImplementedException();
        }

        public void FindAll(string id, List<SpireScript> scripts)
        {
            throw new NotImplementedException();
        }

        public SpireScript FindOneTagged(string query)
        {
            throw new NotImplementedException();
        }

        public void FindAllTagged(string query, List<SpireScript> scripts)
        {
            throw new NotImplementedException();
        }

        public SpireScript Create(string scriptId, params string[] tags)
        {
            throw new NotImplementedException();
        }

        public void Send(string query, string name, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public void Release(SpireScript script)
        {
            throw new NotImplementedException();
        }

        public void ReleaseAll(params string[] tags)
        {
            throw new NotImplementedException();
        }
    }
}