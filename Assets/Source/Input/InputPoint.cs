using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class InputPoint
    {
        private float _currentTime;

        public readonly int Id;

        public bool IsDown;
        public Vector2 DownPosition;
        public float DownTime;
        
        public Vector2 CurrentPosition;

        public bool IsUp;
        public Vector2 UpPosition;
        public float UpTime;

        public InputPoint(int id)
        {
            Id = id;
        }

        public void Down(Vector2 position, float time)
        {
            DownPosition = position;
            DownTime = time;
            IsDown = true;
        }

        public void Update(Vector2 position, float time)
        {
            _currentTime = time;

            CurrentPosition = position;

            IsDown = false;
        }

        public void Up(Vector2 position, float time)
        {
            UpPosition = position;
            UpTime = time;
            IsUp = true;
        }
    }
}