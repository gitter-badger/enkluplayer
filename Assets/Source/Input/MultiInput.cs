using System.Collections.Generic;
using UnityEngine;

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
            UpdateFromTouches();
            UpdateFromMouse();
        }

        private void UpdateFromTouches()
        {
            var touches = Input.touches;

            for (int i = 0, len = touches.Length; i < len; i++)
            {
                var touch = touches[i];
                var point = PointById(touch.fingerId);

                if (null == point)
                {
                    point = new InputPoint(touch.fingerId);
                    point.Down(touch.position, _time);

                    Points.Add(point);
                }
                else if (TouchPhase.Ended == touch.phase
                         || TouchPhase.Canceled == touch.phase)
                {
                    point.Up(touch.position, _time);
                }
                else
                {
                    point.Update(touch.position, _time);
                }
            }
        }

        private void UpdateFromMouse()
        {
            UpdateLeftClick();
            UpdateRightClick();
        }

        private void UpdateLeftClick()
        {
            // left click has control
            if (Points.Count > 1)
            {
                return;
            }

            var id = 0;
            var down = Input.GetMouseButton(id);
            if (1 == Points.Count)
            {
                Points[0].Update(Input.mousePosition, _time);
            }
            else if (down)
            {
                var touch = new InputPoint(id);
                touch.Down(Input.mousePosition, _time);

                Points.Add(touch);
            }

            if (Input.GetMouseButtonUp(id))
            {
                if (1 == Points.Count)
                {
                    Points[0].Up(Input.mousePosition, _time);
                }
            }
        }

        private void UpdateRightClick()
        {
            // right click has control
            if (0 != Points.Count && 2 != Points.Count)
            {
                return;
            }

            var down = Input.GetMouseButton(1);
            if (2 == Points.Count)
            {
                Points[0].Update(Input.mousePosition, _time);
                Points[1].Update(Input.mousePosition, _time);
            }
            else if (down)
            {
                var touch = new InputPoint(0);
                touch.Down(Input.mousePosition, _time);
                Points.Add(touch);

                touch = new InputPoint(1);
                touch.Down(Input.mousePosition, _time);
                Points.Add(touch);
            }

            if (Input.GetMouseButtonUp(1))
            {
                if (2 == Points.Count)
                {
                    Points[0].Up(Input.mousePosition, _time);
                    Points[1].Up(Input.mousePosition, _time);
                }
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