using System;
using Microsoft.Xna.Framework;

namespace RunAndGun.GameObjects
{
    public class CVGameTime
    {
        private bool _isRunningSlowly;
        private TimeSpan _totalGameTime;
        private TimeSpan _elapsedGameTime;

        public CVGameTime(GameTime gameTime)
        {
            _isRunningSlowly = gameTime.IsRunningSlowly;
            _totalGameTime = gameTime.TotalGameTime;
            _elapsedGameTime = gameTime.ElapsedGameTime;
        }

        public bool IsRunningSlowly { get { return _isRunningSlowly; } }

        public TimeSpan TotalGameTime
        {
            get { return new TimeSpan((long)(_totalGameTime.Ticks * _gameSpeed)); }
        }
        public TimeSpan ElapsedGameTime
        {
            get { return new TimeSpan((long)(_elapsedGameTime.Ticks * _gameSpeed)); }
        }

        private float _gameSpeed = 1.0f;

    }
}
