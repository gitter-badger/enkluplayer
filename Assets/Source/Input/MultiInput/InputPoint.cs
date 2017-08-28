using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class InputPoint
    {
        private float _currentTime;

        public readonly int Id;

        public bool IsDown;
        public Vector2 DownPosition;
        public Vector3 DownWorldSpacePosition;
        public float DownTime;
        
        public Vector2 CurrentPosition;
        public Vector3 CurrentWorldSpacePosition;

        public bool IsUp;
        public Vector2 UpPosition;
        public Vector3 UpWorldSpacePosition;
        public float UpTime;

        public InputPoint(int id)
        {
            Id = id;
        }

        public void Down(Vector2 position, float time)
        {
            _currentTime = DownTime = time;
            DownPosition = position;
            DownWorldSpacePosition = CameraUtil.ScreenSpaceToFloorIntersection(
                Camera.main,
                position);

            IsDown = true;
        }

        public void Update(Vector2 position, float time)
        {
            _currentTime = time;
            CurrentPosition = position;
            CurrentWorldSpacePosition = CameraUtil.ScreenSpaceToFloorIntersection(
                Camera.main,
                position);

            IsDown = false;
        }

        public void Up(Vector2 position, float time)
        {
            _currentTime = UpTime = time;
            UpPosition = position;
            CurrentWorldSpacePosition = CameraUtil.ScreenSpaceToFloorIntersection(
                Camera.main,
                position);

            IsUp = true;
        }
    }
}