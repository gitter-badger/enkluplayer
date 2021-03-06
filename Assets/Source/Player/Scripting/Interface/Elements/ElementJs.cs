﻿using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using Enklu.Data;
using Enklu.Orchid;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Js API for an element.
    /// </summary>
    public class ElementJs : JsEventDispatcher, IEntityJs
    {
        /// <summary>
        /// Scratch list for find.
        /// </summary>
        private readonly List<Element> _findScratch = new List<Element>();

        /// <summary>
        /// Runs scripts.
        /// </summary>
        protected readonly IScriptManager _scripts;

        /// <summary>
        /// Caches ElementJs instances for an engine.
        /// </summary>
        protected readonly IElementJsCache _cache;

        /// <summary>
        /// Element we're wrapping.
        /// </summary>
        protected readonly Element _element;

        /// <summary>
        /// The Element we're wrapping.
        /// </summary>
        [DenyJsAccess]
        public Element Element
        {
            get { return _element; }
        }

        /// <summary>
        /// The schema interface.
        /// </summary>
        public readonly ElementSchemaJsApi schema;

        /// <summary>
        /// The transform interface.
        /// </summary>
        public IElementTransformJsApi transform { get; private set; }

        /// <summary>
        /// Unique id of the element.
        /// </summary>
        public string id
        {
            get { return _element.Id; }
        }

        /// <summary>
        /// Type name of the element.
        /// </summary>
        public string type
        {
            get { return _element.GetType().Name; }
        }

        /// <summary>
        /// Gets the parent of the element.
        /// </summary>
        public ElementJs parent
        {
            get
            {
                return _cache.Element(_element.Parent);
            }
        }

        /// <summary>
        /// Array of children.
        /// </summary>
        public ElementJs[] children
        {
            get
            {
                var all = _element.Children;

                var wrappers = new ElementJs[all.Count];
                for (int i = 0, len = all.Count; i < len; i++)
                {
                    wrappers[i] = _cache.Element(all[i]);
                }

                return wrappers;
            }
        }

        /// <summary>
        /// Visibility.
        /// </summary>
        public bool visible
        {
            get { return _element.Schema.Get<bool>("visible").Value; }
            set { _element.Schema.Set("visible", value); }
        }

        /// <summary>
        /// Called when Cleanup is called, so that external systems can perform cleanup logic.
        /// </summary>
        public event Action<ElementJs> OnCleanup;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementJs(
            IScriptManager scripts,
            IElementJsCache cache,
            Element element)
        {
            _scripts = scripts;
            _element = element;
            _cache = cache;

            schema = new ElementSchemaJsApi(_element.Schema);
            transform = new ElementTransformJsApi(_element);
        }

        /// <summary>
        /// Cleans up ElementJS instance.
        /// </summary>
        [DenyJsAccess]
        public virtual void Cleanup()
        {
            if (null != OnCleanup)
            {
                OnCleanup(this);
            }
        }

        /// <inheritdoc />
        public bool isChildOf(IEntityJs parent)
        {
            var parentAsElement = parent as ElementJs;
            if (parentAsElement == null)
            {
                return false;
            }

            return _element.IsChildOf(parentAsElement._element);
        }

        /// <summary>
        /// Adds a child.
        /// </summary>
        /// <param name="element">The element to add as a child.</param>
        public void addChild(ElementJs element)
        {
            if (null == element)
            {
                return;
            }

            _element.AddChild(element._element);
        }

        /// <summary>
        /// Removes a child.
        /// </summary>
        /// <param name="element">The child to remove.</param>
        public bool removeChild(ElementJs element)
        {
            if (null == element)
            {
                return false;
            }

            return _element.RemoveChild(element._element);
        }

        /// <summary>
        /// Finds a single element by a query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public ElementJs findOne(string query)
        {
            var element = _element.FindOne<Element>(query);

            return _cache.Element(element);
        }

        /// <summary>
        /// Finds a collection of elements that match a query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public ElementJs[] find(string query)
        {
            _findScratch.Clear();
            _element.Find(query, _findScratch);

            var results = new ElementJs[_findScratch.Count];
            for (int i = 0, len = _findScratch.Count; i < len; i++)
            {
                results[i] = _cache.Element(_findScratch[i]);
            }

            return results;
        }

        /// <summary>
        /// Destroys element.
        /// </summary>
        public virtual void destroy()
        {
            _element.Destroy();
        }

        /// <summary>
        /// Passes a message to all attached scripts.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parameters">The parameters.</param>
        public void send(string name, params object[] parameters)
        {
            _scripts.Send(_element.Id, name, parameters);
        }

        /// <summary>
        /// Returns the position of this ElementJs relative to another ElementJs. This value should not
        /// be cached as elements aren't guaranteed to sit under the same world anchor.
        ///
        /// TODO: Make this more friendly/understandable for people unfamiliar with anchoring woes.
        /// </summary>
        /// <param name="other">The other element.</param>
        /// <returns></returns>
        [Obsolete]
        public Vec3 positionRelativeTo(ElementJs other)
        {
            Log.Warning(this, "Element.positionRelativeTo is obsolete. Please use Element.transform.positionRelativeTo instead.");

            var thisAsWidget = _element as Widget;
            var otherAsWidget = other._element as Widget;

            // TODO: Traverse the hierarchy if these aren't widgets
            if (thisAsWidget == null || otherAsWidget == null)
            {
                Log.Error(this, "Trying to find relative position with a non-widget?!");
                return Vec3.Zero;
            }

            return (thisAsWidget.GameObject.transform.position - otherAsWidget.GameObject.transform.position).ToVec();
        }

        /// <summary>
        /// ToString implementation for ElementJs.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[ElementJs Element={0}]", _element);
        }

        /// <summary>
        /// Tests whether two ElementJs instances are equivalent.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool operator ==(ElementJs @this, object element)
        {
            if (null == (object) @this && null == element)
            {
                return true;
            }

            if (null == @this)
            {
                return false;
            }

            var elementJs = element as ElementJs;
            if (null == elementJs)
            {
                return false;
            }

            return @this._element == elementJs._element;
        }

        /// <summary>
        /// Tests whether two ElementJs instances are inequivalent.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool operator !=(ElementJs @this, object element)
        {
            if (null == (object) @this && null == element)
            {
                return false;
            }

            if (null == @this)
            {
                return true;
            }

            var elementJs = element as ElementJs;
            if (null == elementJs)
            {
                return true;
            }

            return @this._element != elementJs._element;
        }

        /// <summary>
        /// Required for ==.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return _element != null ? _element.GetHashCode() : 0;
        }

        /// <summary>
        /// Tests whether an object equals this ElementJs instance.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var elementJs = obj as ElementJs;
            return elementJs != null && _element == elementJs._element;
        }

        /// <summary>
        /// Required for == overload.
        /// </summary>
        /// <param name="other">Other ElementJS.</param>
        /// <returns></returns>
        protected bool Equals(ElementJs other)
        {
            return null != other && _element == other._element;
        }
    }
}