using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class InputPoint
    {
        public readonly int Id;

        public bool IsDown;
        public Vector2 DownPosition;
        public Vector3 DownWorldSpacePosition;
        public float DownTime;

        public Vector2 PreviousPosition;
        public Vector3 PreviousWorldSpacePosition;
        public float PreviousTime;

        public Vector2 CurrentPosition;
        public Vector3 CurrentWorldSpacePosition;
        public float CurrentTime;

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
            PreviousTime = CurrentTime = DownTime = time;
            PreviousPosition = CurrentPosition = DownPosition = position;
            PreviousWorldSpacePosition = CurrentWorldSpacePosition = DownWorldSpacePosition =
                CameraUtil.ScreenSpaceToFloorIntersection(
                    Camera.main,
                    position);

            IsDown = true;
        }

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

        public void Up(Vector2 position, float time)
        {
            CurrentTime = UpTime = time;
            UpPosition = position;
            CurrentWorldSpacePosition = CameraUtil.ScreenSpaceToFloorIntersection(
                Camera.main,
                position);

            IsUp = true;
        }
    }
}