using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RunAndGun.Actors
{
    class ActorAction
    {
        public enum ActionType { AscendStairs, DescendStairs, MoveToAscendingStairs, MoveToDescendingStairs };
        public ActionType Action;
        public bool Active;
    }
}
