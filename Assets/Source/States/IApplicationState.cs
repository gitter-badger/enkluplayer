﻿namespace CreateAR.SpirePlayer
{
    public interface IApplicationState
    {
        void Enter();
        void Update(float dt);
        void Exit();
    }
}