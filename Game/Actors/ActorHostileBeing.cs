﻿using Game.Engine;
using Library.Engine;
using Library.Engine.Types;

namespace Game.Actors
{
    public class ActorHostileBeing : ActorBase
    {
        public ActorHostileBeing(EngineCore core)
            : base(core)
        {
            RotationMode = RotationMode.None;
            Velocity.MaxSpeed = 10;
            Velocity.ThrottlePercentage = 100;
        }
    }
}
