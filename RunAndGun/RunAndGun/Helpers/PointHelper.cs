using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RunAndGun.Helpers
{
    public static class PointHelper
    {
        public static Vector2 ToVector(this Point pt)
        {
            return new Vector2(pt.X, pt.Y);
        }
    }
}
