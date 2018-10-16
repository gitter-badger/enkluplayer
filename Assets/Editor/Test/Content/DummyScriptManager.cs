using System;
using System.Collections.Generic;

namespace CreateAR.EnkluPlayer.Test
{
    public class DummyScriptManager : IScriptManager
    {
        private readonly bool _throwErrors;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="throwErrors">Whether method calls throw a
        /// <see cref="NotImplementedException"/> or not.</param>
        public DummyScriptManager(bool throwErrors = true)
        {
            _throwErrors = throwErrors;
        }

        public EnkluScript FindOne(string id)
        {
            if (_throwErrors)
            {
                throw new NotImplementedException();
            }
            return null;
        }

        public void FindAll(string id, List<EnkluScript> scripts)
        {
            if (_throwErrors)
            {
                throw new NotImplementedException();
            }
        }

        public EnkluScript FindOneTagged(string query)
        {
            if (_throwErrors)
            {
                throw new NotImplementedException();
            }
            return null;
        }

        public void FindAllTagged(string query, List<EnkluScript> scripts)
        {
            if (_throwErrors)
            {
                throw new NotImplementedException();
            }
        }

        public EnkluScript Create(string scriptId, params string[] tags)
        {
            if (_throwErrors)
            {
                throw new NotImplementedException();
            }
            return null;
        }

        public void Send(string query, string name, params object[] parameters)
        {
            if (_throwErrors) 
            {
                throw new NotImplementedException();
            }
        }

        public void Release(EnkluScript script)
        {
            if (_throwErrors) 
            {
                throw new NotImplementedException();
            }
        }

        public void ReleaseAll(params string[] tags)
        {
            if (_throwErrors) 
            {
                throw new NotImplementedException();
            }
        }
    }
}