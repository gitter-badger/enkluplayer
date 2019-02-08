using System.Collections.Generic;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    public class TestScriptManager : IScriptManager
    {
        private readonly Dictionary<string, EnkluScript> _scriptLookup = new Dictionary<string, EnkluScript>();
        
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
            EnkluScript rtn;
            _scriptLookup.TryGetValue(scriptId, out rtn);
            return rtn;
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

        public void AddEntry(string scriptId, EnkluScript script)
        {
            if (!_scriptLookup.ContainsKey(scriptId))
            {
                _scriptLookup.Add(scriptId, script);
            }
        }

        public void RemoveEntry(string scriptId)
        {
            _scriptLookup.Remove(scriptId);
        }
    }
}