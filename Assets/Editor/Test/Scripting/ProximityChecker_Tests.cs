using CreateAR.SpirePlayer.IUX;
using CreateAR.SpirePlayer.Scripting;
using Jint;
using NUnit.Framework;

namespace CreateAR.SpirePlayer.Test.Scripting
{
    /// <summary>
    /// Tests the actual <see cref="ProximityChecker"/> class, with a wide range of mock scene configurations
    /// </summary>
    [TestFixture]
    public class ProximityChecker_Tests
    {
        private Engine _engine;
        private ProximityChecker _proximityChecker;

        private IEntityJs _elementA;
        private IEntityJs _elementB;

        private int enterCount = 0;
        private int stayCount = 0;
        private int exitCount = 0;

        [SetUp]
        public void Setup()
        {
            _engine = new Engine();
            _proximityChecker = new ProximityChecker();

            enterCount = 0;
            stayCount = 0;
            exitCount = 0;

            _proximityChecker.OnEnter += (IEntityJs callbackListener, IEntityJs callbackTrigger) => {
                Assert.AreEqual(_elementA, callbackListener);
                Assert.AreEqual(_elementB, callbackTrigger);
                enterCount++;
            };

            _proximityChecker.OnStay += (IEntityJs callbackListener, IEntityJs callbackTrigger) => {
                Assert.AreEqual(_elementA, callbackListener);
                Assert.AreEqual(_elementB, callbackTrigger);
                stayCount++;
            };

            _proximityChecker.OnExit += (IEntityJs callbackListener, IEntityJs callbackTrigger) => {
                Assert.AreEqual(_elementA, callbackListener);
                Assert.AreEqual(_elementB, callbackTrigger);
                exitCount++;
            };
        }

        /// <summary>
        /// Test the basic flow. One Listener, one trigger. Ensure all events fire as expected.
        /// </summary>
        [Test]
        public void SimpleEventFiring()
        {
            // Setup elements. A is a listener, B is a trigger
            _elementA = BuildElementJs(true, false, 3, 5);
            _elementB = BuildElementJs(false, true, 2, 5);

            // Check for no-op
            _elementB.transform.position = new Vec3(10, 0, 0);
            _proximityChecker.Update();
            CheckCallbackCounts(0, 0, 0);

            // Check for enter
            _elementB.transform.position = new Vec3(4, 0, 0);
            _proximityChecker.Update();
            CheckCallbackCounts(1, 0, 0);

            // Check for stay (between inner/outer)
            _elementB.transform.position = new Vec3(7, 0, 0);
            _proximityChecker.Update();
            CheckCallbackCounts(1, 1, 0);

            // Check for exit
            _elementB.transform.position = new Vec3(12, 0, 0);
            _proximityChecker.Update();
            CheckCallbackCounts(1, 1, 1);
        }

        /// <summary>
        /// Test that two listeners don't send events to each other.
        /// </summary>
        [Test]
        public void ListenersIgnoreEachOther()
        {
            // Setup elements. A and B are both listeners
            ElementJs listenerA = BuildElementJs(true, false, 2.5f, 5);
            ElementJs listenerB = BuildElementJs(true, false, 2.5f, 5);

            // Check for no-op
            listenerB.transform.position = new Vec3(10, 0, 0);
            _proximityChecker.Update();
            CheckCallbackCounts(0, 0, 0);

            // Check for no-op again
            listenerB.transform.position = new Vec3(4, 0, 0);
            _proximityChecker.Update();
            CheckCallbackCounts(0, 0, 0);
        }

        /// <summary>
        /// Test that two triggers don't send events to each other.
        /// </summary>
        [Test]
        public void TriggersIgnoreEachOther()
        {
            // Setup elements. A and B are both triggers
            ElementJs triggerA = BuildElementJs(false, true, 2.5f, 5);
            ElementJs triggerB = BuildElementJs(false, true, 2.5f, 5);

            // Check for no-op
            triggerB.transform.position = new Vec3(10, 0, 0);
            _proximityChecker.Update();
            CheckCallbackCounts(0, 0, 0);

            // Check for no-op again
            triggerB.transform.position = new Vec3(4, 0, 0);
            _proximityChecker.Update();
            CheckCallbackCounts(0, 0, 0);
        }

        /// <summary>
        /// Test that a listener won't have a child trigger sending events to itself
        /// </summary>
        [Test]
        public void ChildrenAreIgnored()
        {
            // Setup elements. A is a listener, B is a trigger
            ElementJs parent = BuildElementJs(true, false, 2.5f, 5);
            ElementJs child = BuildElementJs(false, true, 2.5f, 5);
            parent.addChild(child);


            // Check for no-op
            child.transform.position = new Vec3(10, 0, 0);
            _proximityChecker.Update();
            CheckCallbackCounts(0, 0, 0);

            // Check for no-op again
            child.transform.position = new Vec3(4, 0, 0);
            _proximityChecker.Update();
            CheckCallbackCounts(0, 0, 0);
        }

        /// <summary>
        /// Test that a trigger won't send events to a child listener
        /// </summary>
        [Test]
        public void ParentsAreIgnored()
        {
            // Setup elements. A is a listener, B is a trigger
            ElementJs parent = BuildElementJs(false, true, 2.5f, 5);
            ElementJs child = BuildElementJs(true, false, 2.5f, 5);
            parent.addChild(child);

            // Check for no-op
            child.transform.position = new Vec3(10, 0, 0);
            _proximityChecker.Update();
            CheckCallbackCounts(0, 0, 0);

            // Check for no-op again
            child.transform.position = new Vec3(4, 0, 0);
            _proximityChecker.Update();
            CheckCallbackCounts(0, 0, 0);
        }

        /// <summary>
        /// Check that a listening Element can also be a trigger. Callback counts in this test will be 2x to check for bi-directionality.
        /// </summary>
        [Test]
        public void ListenersCanBeTriggers()
        {
            // Both elements are listeners and triggers
            ElementJs mixedA = BuildElementJs(true, true, 3, 5);
            ElementJs mixedB = BuildElementJs(true, true, 2, 5);

            // We'll need some custom callback logic, so clear the existing ones
            _proximityChecker.OnEnter = null;
            _proximityChecker.OnStay = null;
            _proximityChecker.OnExit = null;

            ElementJs cachedListenerElement = null;
            ElementJs cachedTriggerElement = null;

            _proximityChecker.OnEnter += (IEntityJs callbackListener, IEntityJs callbackTrigger) => {
                Assert.AreNotEqual(callbackListener, callbackTrigger);

                // Gross, but we need to check that the callbacks are actually bi-directional and not just 2x
                if (cachedListenerElement != null)
                {
                    Assert.AreNotEqual(cachedListenerElement, callbackListener);
                }
                cachedListenerElement = (ElementJs)callbackListener;

                if (cachedTriggerElement != null)
                {
                    Assert.AreNotEqual(cachedTriggerElement, callbackTrigger);
                    
                }
                cachedTriggerElement = (ElementJs)callbackTrigger;

                enterCount++;
            };

            _proximityChecker.OnStay += (IEntityJs callbackListener, IEntityJs callbackTrigger) => {
                Assert.AreNotEqual(callbackListener, callbackTrigger);

                // Gross, but we need to check that the callbacks are actually bi-directional and not just 2x
                if (cachedListenerElement != null)
                {
                    Assert.AreNotEqual(cachedListenerElement, callbackListener);
                }
                cachedListenerElement = (ElementJs)callbackListener;

                if (cachedTriggerElement != null)
                {
                    Assert.AreNotEqual(cachedTriggerElement, callbackTrigger);

                }
                cachedTriggerElement = (ElementJs)callbackTrigger;

                stayCount++;
            };

            _proximityChecker.OnExit += (IEntityJs callbackListener, IEntityJs callbackTrigger) => {
                Assert.AreNotEqual(callbackListener, callbackTrigger);

                // Gross, but we need to check that the callbacks are actually bi-directional and not just 2x
                if (cachedListenerElement != null)
                {
                    Assert.AreNotEqual(cachedListenerElement, callbackListener);
                }
                cachedListenerElement = (ElementJs)callbackListener;

                if (cachedTriggerElement != null)
                {
                    Assert.AreNotEqual(cachedTriggerElement, callbackTrigger);

                }
                cachedTriggerElement = (ElementJs)callbackTrigger;

                exitCount++;
            };


            // Check for no-op
            mixedB.transform.position = new Vec3(10, 0, 0);
            _proximityChecker.Update();
            CheckCallbackCounts(0, 0, 0);

            // Check for enter
            mixedB.transform.position = new Vec3(4, 0, 0);
            _proximityChecker.Update();
            CheckCallbackCounts(2, 0, 0);
            Assert.NotNull(cachedListenerElement);
            Assert.NotNull(cachedTriggerElement);
            cachedListenerElement = null;
            cachedTriggerElement = null;

            // Check for stay (between inner/outer)
            mixedB.transform.position = new Vec3(7, 0, 0);
            _proximityChecker.Update();
            CheckCallbackCounts(2, 2, 0);
            Assert.NotNull(cachedListenerElement);
            Assert.NotNull(cachedTriggerElement);
            cachedListenerElement = null;
            cachedTriggerElement = null;

            // Check for exit
            mixedB.transform.position = new Vec3(12, 0, 0);
            _proximityChecker.Update();
            CheckCallbackCounts(2, 2, 2);
            Assert.NotNull(cachedListenerElement);
            Assert.NotNull(cachedTriggerElement);
            cachedListenerElement = null;
            cachedTriggerElement = null;
        }

        /// <summary>
        /// Basic flow test, with multiple triggers to ensure events are sent for both.
        /// </summary>
        [Test]
        public void MultipleCollisions()
        {
            // Create 1 element as a listener, and 2 triggers
            ElementJs listener = BuildElementJs(true, false, 5, 10);
            _elementA = BuildElementJs(false, true, 2, 5);
            _elementB = BuildElementJs(false, true, 2, 5);

            // We'll need some custom callback logic, so clear the existing ones
            _proximityChecker.OnEnter = null;
            _proximityChecker.OnStay = null;
            _proximityChecker.OnExit = null;
            
            ElementJs cachedEnterTrigger = null;
            ElementJs cachedStayTrigger = null;
            ElementJs cachedExitTrigger = null;

            _proximityChecker.OnEnter += (IEntityJs callbackListener, IEntityJs callbackTrigger) => {
                Assert.AreEqual(listener, callbackListener);
                Assert.AreNotEqual(callbackListener, callbackTrigger);

                if (cachedEnterTrigger != null)
                {
                    Assert.AreNotEqual(cachedEnterTrigger, callbackTrigger);
                }
                cachedEnterTrigger = (ElementJs)callbackTrigger;

                enterCount++;
            };

            _proximityChecker.OnStay += (IEntityJs callbackListener, IEntityJs callbackTrigger) => {
                Assert.AreEqual(listener, callbackListener);
                Assert.AreNotEqual(callbackListener, callbackTrigger);

                if (cachedStayTrigger != null)
                {
                    Assert.AreNotEqual(cachedStayTrigger, callbackTrigger);

                }
                cachedStayTrigger = (ElementJs)callbackTrigger;

                stayCount++;
            };

            _proximityChecker.OnExit += (IEntityJs callbackListener, IEntityJs callbackTrigger) => {
                Assert.AreEqual(listener, callbackListener);
                Assert.AreNotEqual(callbackListener, callbackTrigger);

                if (cachedExitTrigger != null)
                {
                    Assert.AreNotEqual(cachedExitTrigger, callbackTrigger);

                }
                cachedExitTrigger = (ElementJs)callbackTrigger;

                exitCount++;
            };

            // Check for no-op
            _elementA.transform.position = new Vec3(10, 0, 0);
            _elementB.transform.position = new Vec3(-10, 0, 0);
            _proximityChecker.Update();
            CheckCallbackCounts(0, 0, 0);

            // Have one element enter
            _elementA.transform.position = new Vec3(5, 0, 0);
            _proximityChecker.Update();
            CheckCallbackCounts(1, 0, 0);
            Assert.AreEqual(_elementA, cachedEnterTrigger);
            cachedEnterTrigger = null;

            // Move the other element in range
            _elementB.transform.position = new Vec3(-5, 0, 0);
            _proximityChecker.Update();
            CheckCallbackCounts(2, 1, 0); // _elementA is still in range, so it should have stayed
            Assert.AreEqual(_elementB, cachedEnterTrigger);
            Assert.AreEqual(_elementA, cachedStayTrigger);
            cachedStayTrigger = null;

            // Have them both stay a frame
            _proximityChecker.Update();
            CheckCallbackCounts(2, 3, 0);
            cachedStayTrigger = null;

            // Have one exit
            _elementA.transform.position = new Vec3(20, 0, 0);
            _proximityChecker.Update();
            CheckCallbackCounts(2, 4, 1);
            Assert.AreEqual(_elementA, cachedExitTrigger);
            Assert.AreEqual(_elementB, cachedStayTrigger);
            cachedExitTrigger = null;

            // Have the other exit
            _elementB.transform.position = new Vec3(20, 0, 0);
            _proximityChecker.Update();
            CheckCallbackCounts(2, 4, 2);
            Assert.AreEqual(_elementB, cachedExitTrigger);
        }


        // Helpers

        private ElementJs BuildElementJs(bool isListening, bool isTrigger, float innerRadius, float outerRadius)
        {
            ElementJs element = new ElementJs(_engine, null, new Element());
            _proximityChecker.SetElementState(element, isListening, isTrigger);
            _proximityChecker.SetElementRadii(element, innerRadius, outerRadius);
            return element;
        }

        private void CheckCallbackCounts(int enterCount, int stayCount, int exitCount)
        {
            Assert.AreEqual(enterCount, this.enterCount);
            Assert.AreEqual(stayCount, this.stayCount);
            Assert.AreEqual(exitCount, this.exitCount);
        }
    }
}