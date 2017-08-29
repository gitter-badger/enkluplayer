using UnityEngine;
using UnityEngine.Assertions;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Data object for an input.
    /// </summary>
    public class InputPoint
    {
        /// <summary>
        /// Unique id.
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// Information kept on down frame.
        /// </summary>
        public bool IsDown;
        public Vector2 DownPosition;
        public Vector3 DownWorldSpacePosition;
        public float DownTime;

        /// <summary>
        /// Information from last frame.
        /// </summary>
        public Vector2 PreviousPosition;
        public Vector3 PreviousWorldSpacePosition;
        public float PreviousTime;

        /// <summary>
        /// Information from the current frame.
        /// </summary>
        public Vector2 CurrentPosition;
        public Vector3 CurrentWorldSpacePosition;
        public float CurrentTime;

        /// <summary>
        /// Information from the Up frame.
        /// </summary>
        public bool IsUp;
        public Vector2 UpPosition;
        public Vector3 UpWorldSpacePosition;
        public float UpTime;
        
        /// <summary>
        /// Creates a new InputPoint.
        /// </summary>
        /// <param name="id">Unique id.</param>
        public InputPoint(int id)
        {
            Id = id;
        }

        /// <summary>
        /// Call when input is first down.
        /// </summary>
        /// <param name="position">Current screen space position.</param>
        /// <param name="time">Current time.</param>
        public void Down(Vector2 position, float time)
        {
            Assert.IsFalse(IsDown);
            Assert.IsFalse(IsUp);

            PreviousTime = CurrentTime = DownTime = time;
            PreviousPosition = CurrentPosition = DownPosition = position;
            PreviousWorldSpacePosition = CurrentWorldSpacePosition = DownWorldSpacePosition =
                CameraUtil.ScreenSpaceToFloorIntersection(
                    Camera.main,
                    position);

            IsDown = true;
        }

        /// <summary>
        /// Called every frame the input changes.
        /// </summary>
        /// <param name="position">Current screen space position.</param>
        /// <param name="time">Current time</param>
        public void Update(Vector2 position, float time)
        {
            PreviousTime = CurrentTime;
            PreviousPosition = CurrentPosition;
            PreviousWorldSpacePosition = CurrentWorldSpacePosition;

            CurrentTime = time;
            CurrentPosition = position;
            CurrentWorldSpacePosition = CameraUtil.ScreenSpaceToFloorIntersection(
                Camera.main,
                position);

            IsDown = false;
        }

        /// <summary>
        /// Called on the up frame.
        /// </summary>
        /// <param name="position">Current screen space position.</param>
        /// <param name="time">Current time</param>
        public void Up(Vector2 position, float time)
        {
            Assert.IsFalse(IsUp);
            Assert.IsFalse(IsDown);

            CurrentTime = UpTime = time;
            UpPosition = position;
            CurrentWorldSpacePosition = CameraUtil.ScreenSpaceToFloorIntersection(
                Camera.main,
                position);

            IsUp = true;
        }
    }
}