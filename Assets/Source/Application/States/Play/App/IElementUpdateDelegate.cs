using CreateAR.Commons.Unity.Async;
using CreateAR.EnkluPlayer.IUX;
using Enklu.Data;
using ElementData = CreateAR.EnkluPlayer.IUX.ElementData;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// An interface for an object that updates elements.
    /// </summary>
    public interface IElementUpdateDelegate
    {
        /// <summary>
        /// Gets and sets the active scene id.
        /// </summary>
        string Active { get; set; }

        /// <summary>
        /// Creates a new element in active scene.
        /// </summary>
        /// <param name="data">The data.</param>
        IAsyncToken<Element> Create(ElementData data);

        /// <summary>
        /// Creates a new element in active scene.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="parentId">The parent to attach to.</param>
        IAsyncToken<Element> Create(ElementData data, string parentId);

        /// <summary>
        /// Deletes an element from active scene.
        /// </summary>
        /// <param name="element">The element.</param>
        IAsyncToken<Element> Destroy(Element element);

        /// <summary>
        /// Deletes all elements from active scene.
        /// </summary>
        IAsyncToken<Void> DestroyAll();

        /// <summary>
        /// Called when element has been updated.
        /// </summary>
        void Update(Element element, string key, string value);

        /// <summary>
        /// Called when element has been updated.
        /// </summary>
        void Update(Element element, string key, int value);

        /// <summary>
        /// Called when element has been updated.
        /// </summary>
        void Update(Element element, string key, float value);

        /// <summary>
        /// Called when element has been updated.
        /// </summary>
        void Update(Element element, string key, bool value);

        /// <summary>
        /// Called when element has been updated.
        /// </summary>
        void Update(Element element, string key, Vec3 value);

        /// <summary>
        /// Called when element has been updated.
        /// </summary>
        void Update(Element element, string key, Col4 value);

        /// <summary>
        /// Finalizes update.
        /// </summary>
        void FinalizeUpdate(Element element);

        /// <summary>
        /// Reparents an element.
        /// </summary>
        /// <param name="element">The element to move.</param>
        /// <param name="parent">The new parent.</param>
        /// <returns></returns>
        IAsyncToken<Element> Reparent(Element element, Element parent);
    }
}