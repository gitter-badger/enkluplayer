using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.Assertions;

namespace CreateAR.SpirePlayer
{
    public class MultiInput : IMultiInput
    {
        private float _time;

        public Camera Camera { get; set; }

        public List<InputPoint> Points { get; private set; }

        public MultiInput()
        {
            Points = new List<InputPoint>();
        }

        public void Update(float dt)
        {
            _time += dt;

            RemoveUpPoints();

            if (!UpdateFromTouches())
            {
                UpdateFromMouse();
            }
        }

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

        private void UpdateFromMouse()
        {
            UpdateLeftClick();
            UpdateRightClick();
        }

        private void UpdateLeftClick()
        {
            // right click has control
            if (Points.Count > 1)
            {
                return;
            }

            var id = 0;
            var down = Input.GetMouseButtonDown(id);
            if (down)
            {
                Assert.IsTrue(0 == Points.Count, "Input should not have points until down.");

                var point = new InputPoint(id);
                point.Down(Input.mousePosition, _time);

                Points.Add(point);
            }

            var up = Input.GetMouseButtonUp(0);
            if (up)
            {
                Assert.IsTrue(1 == Points.Count, "Input should have exactly 1 point.");

                Points[0].Up(Input.mousePosition, _time);
            }

            down = Input.GetMouseButton(id);
            if (down)
            {
                Assert.IsTrue(1 == Points.Count, "Input should have exactly 1 point while updating.");
                Points[0].Update(Input.mousePosition, _time);
            }
        }

        private void UpdateRightClick()
        {
            // left click has control
            if (0 != Points.Count && 2 != Points.Count)
            {
                return;
            }

            var id = 1;
            var down = Input.GetMouseButtonDown(id);
            if (down)
            {
                Assert.IsTrue(0 == Points.Count, "Input should have exactly 0 points.");

                var point = new InputPoint(0);
                point.Down(Input.mousePosition, _time);
                Points.Add(point);

                point = new InputPoint(1);
                point.Down(Input.mousePosition, _time);
                Points.Add(point);
            }

            var up = Input.GetMouseButtonUp(id);
            if (up)
            {
                Assert.IsTrue(2 == Points.Count, "Input should have exactly 2 points.");

                Points[0].Up(Input.mousePosition, _time);
                Points[1].Up(Input.mousePosition, _time);
            }

            down = Input.GetMouseButton(1);
            if (down)
            {
                Assert.IsTrue(2 == Points.Count, "Input should have exactly 2 points.");

                Points[0].Update(Input.mousePosition, _time);
                Points[1].Update(Input.mousePosition, _time);
            }
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