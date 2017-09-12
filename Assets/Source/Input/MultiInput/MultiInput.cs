using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.Assertions;

namespace CreateAR.SpirePlayer
{
    public class MultiInput : IMultiInput
    {
        /// <summary>
        /// Elapsed time.
        /// </summary>
        private float _time;

        /// <inheritdoc cref="IMultiInput"/>
        public Camera Camera { get; set; }

        /// <inheritdoc cref="IMultiInput"/>
        public List<InputPoint> Points { get; private set; }

        /// <summary>
        /// Creates a new MultiInput.
        /// </summary>
        public MultiInput()
        {
            Points = new List<InputPoint>();
        }

        /// <inheritdoc cref="IMultiInput"/>
        public void Update(float dt)
        {
            _time += dt;

            RemoveUpPoints();

            if (!UpdateFromTouches())
            {
                UpdateFromMouse();
            }
        }

        /// <summary>
        /// Updates <c>InputPoint</c>s from Touch data.
        /// </summary>
        /// <returns></returns>
        private bool UpdateFromTouches()
        {
            var touches = Input.touches;

            for (int i = 0, len = touches.Length; i < len; i++)
            {
                var touch = touches[i];

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                    {
                        var point = new InputPoint(touch.fingerId);
                        point.Down(touch.position, _time);

                        Points.Add(point);

                            Log.Info(this, "New touch.");

                        break;
                    }
                    case TouchPhase.Stationary:
                    case TouchPhase.Moved:
                    {
                        var point = PointById(touch.fingerId);

                        Assert.IsNotNull(point, "Point should not be null for moving touch.");

                        point.Update(touch.position, _time);

                        break;
                    }
                    case TouchPhase.Canceled:
                    case TouchPhase.Ended:
                    {
                        var point = PointById(touch.fingerId);

                        Assert.IsNotNull(point, "Point should not be null for ending touch.");

                        point.Up(touch.position, _time);

                        Log.Info(this, "End touch.");

                        break;
                    }
                }
            }

            return touches.Length > 0;
        }

        /// <summary>
        /// Updates <c>InputPoint</c>s from Mouse data.
        /// </summary>
        private void UpdateFromMouse()
        {
            UpdateLeftClick();
            UpdateRightClick();
            UpdateMouseWheel();
        }

        /// <summary>
        /// Updates the left click. Left click creates a single InputPoint.
        /// </summary>
        private void UpdateLeftClick()
        {
            // right click has control
            if (Points.Count > 1)
            {
                return;
            }

            var id = 0;
            var isDown = Input.GetMouseButtonDown(id);
            var isUp = Input.GetMouseButtonUp(0);
            var down = Input.GetMouseButton(id);
            if (isDown)
            {
                Assert.IsTrue(0 == Points.Count, "Input should not have points until down.");

                var point = new InputPoint(id);
                point.Down(Input.mousePosition, _time);

                Points.Add(point);
            }
            else if (isUp)
            {
                Assert.IsTrue(1 == Points.Count, "Input should have exactly 1 point.");

                Points[0].Up(Input.mousePosition, _time);
            }
            else if (down)
            {
                Assert.IsTrue(1 == Points.Count, "Input should have exactly 1 point while updating.");
                Points[0].Update(Input.mousePosition, _time);
            }
        }

        /// <summary>
        /// Updates the right click. Creates two InputPoints like a two finger
        /// gesture would.
        /// </summary>
        private void UpdateRightClick()
        {
            // left click has control
            if (0 != Points.Count && 2 != Points.Count)
            {
                return;
            }

            var id = 1;
            var isDown = Input.GetMouseButtonDown(id);
            var isUp = Input.GetMouseButtonUp(id);
            var down = Input.GetMouseButton(id);
            if (isDown)
            {
                Assert.IsTrue(0 == Points.Count, "Input should have exactly 0 points.");

                var point = new InputPoint(0);
                point.Down(Input.mousePosition, _time);
                Points.Add(point);

                point = new InputPoint(1);
                point.Down(Input.mousePosition, _time);
                Points.Add(point);
            }
            else if (isUp)
            {
                Assert.IsTrue(2 == Points.Count, "Input should have exactly 2 points.");

                Points[0].Up(Input.mousePosition, _time);
                Points[1].Up(Input.mousePosition, _time);
            }
            else if (down)
            {
                Assert.IsTrue(2 == Points.Count, "Input should have exactly 2 points.");

                Points[0].Update(Input.mousePosition, _time);
                Points[1].Update(Input.mousePosition, _time);
            }
        }

        /// <summary>
        /// Updates the mouse wheel. Creates two InputPoints and uses them like
        /// pinch + zoom.
        /// </summary>
        private void UpdateMouseWheel()
        {
            //var wheel = Input.GetAxis("Mouse ScrollWheel");
        }

        /// <summary>
        /// Retrieves the point for an id.
        /// </summary>
        private InputPoint PointById(int id)
        {
            for (int i = 0, len = Points.Count; i < len; i++)
            {
                if (Points[i].Id == id)
                {
                    return Points[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Purges all points that are up.
        /// </summary>
        private void RemoveUpPoints()
        {
            for (var i = Points.Count - 1; i >= 0; i--)
            {
                if (Points[i].IsUp)
                {
                    Points.RemoveAt(i);
                }
            }
        }
    }
}