using Microsoft.Xna.Framework;
using RunAndGun.Actors;
using RunAndGun.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RunAndGun.GameObjects
{
    static class TargetingLogic
    {
        public static Actor FindNearest(Vector2 position, IEnumerable<Actor> actors)
        {
            return actors.Aggregate((p1, p2) => (position - p1.BoundingBox().Center.ToVector()).Length() < (position - p2.BoundingBox().Center.ToVector()).Length() ? p1 : p2);
        }

        public static float FindAngle(Vector2 source, Vector2 target)
        {
            double d = Math.Atan2(target.X - source.X, -(target.Y - source.Y));
            float angleBetweenSourceAndTarget = MathHelper.ToDegrees((float)d);

            if (angleBetweenSourceAndTarget < 0)
                angleBetweenSourceAndTarget += 360;

            return angleBetweenSourceAndTarget;
        }

        public static int FindClockPosition(Vector2 source, Vector2 target)
        {                        
            return (int)Math.Round(FindAngle(source, target) / 30);
        }
    }
}
