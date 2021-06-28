﻿using Game.Engine;
using Library.Engine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Actors
{
    public class ActorLevelWarp : ActorBase
    {
        public ActorLevelWarp(EngineCore core)
            : base(core)
        {
            DoNotDraw = true;
        }
    }
}
