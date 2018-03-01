using System.Collections.Generic;

namespace CreateAR.SpirePlayer
{
    public interface IElementControllerManager
    {
        IElementControllerManager Filter(IElementControllerFilter filter);
        IElementControllerManager Unfilter(IElementControllerFilter filter);

        IElementControllerManager Add<T>(object context) where T : ElementDesignController;
        IElementControllerManager Remove<T>() where T : ElementDesignController;

        void All<T>(IList<T> collection);
    }

    public class ElementControllerManager : IElementControllerManager
    {
        public IElementControllerManager Filter(IElementControllerFilter filter)
        {
            throw new System.NotImplementedException();
        }

        public IElementControllerManager Unfilter(IElementControllerFilter filter)
        {
            throw new System.NotImplementedException();
        }

        public IElementControllerManager Add<T>(object context) where T : ElementDesignController
        {
            throw new System.NotImplementedException();
        }

        public IElementControllerManager Remove<T>() where T : ElementDesignController
        {
            throw new System.NotImplementedException();
        }

        public void All<T>(IList<T> collection)
        {
            throw new System.NotImplementedException();
        }
    }
}