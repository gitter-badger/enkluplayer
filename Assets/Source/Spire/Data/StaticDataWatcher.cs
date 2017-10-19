using System;

namespace CreateAR.SpirePlayer
{
    public class StaticDataWatcher<T> where T : StaticData
    {
        private readonly IAppDataManager _appData;
        private readonly string _id;
        
        public T Value { get; private set; }

        public event Action<T> OnRemoved;
        public event Action<T> OnUpdated;

        public StaticDataWatcher(
            IAppDataManager appData,
            T value)
        {
            _appData = appData;
            _appData.OnUpdated += AppData_OnUpdated;
            _appData.OnRemoved += Appdata_OnRemoved;

            Value = value;
            _id = Value.Id;
        }

        public void Destroy()
        {
            _appData.OnUpdated -= AppData_OnUpdated;
            _appData.OnRemoved -= Appdata_OnRemoved;
        }

        private void AppData_OnUpdated(StaticData staticData)
        {
            if (staticData.Id == _id)
            {
                Value = (T) staticData;

                if (null != OnUpdated)
                {
                    OnUpdated(Value);
                }
            }
        }

        private void Appdata_OnRemoved(StaticData staticData)
        {
            if (staticData.Id == _id)
            {
                if (null != OnRemoved)
                {
                    OnRemoved(Value);
                }
            }
        }
    }
}