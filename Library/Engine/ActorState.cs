﻿using Library.Engine.Types;
using System;

namespace Library.Engine
{
    public class ActorState
    {
        public Guid ActorUID { get; set; }
        public StateOfBeing State { get; set; }
        /// <summary>
        /// Game time until expiration (in pimutes)
        /// </summary>
        public int? ExpireTime { get; set; }
        /// <summary>
        /// The amount of a trait that was modified.
        /// </summary>
        public int? ModificationAmount { get; set; }
    }
}
