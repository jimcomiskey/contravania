using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RunAndGun.GameObjects
{
    public class Platform 
    {
        public enum PlatformTypes
        {
            Normal, 
            Stairs, 
            StairsBottom
        }
        private Rectangle _platformBounds;
        private PlatformTypes _platformType;
        public Platform(Rectangle platformBounds, PlatformTypes platformType)
        {
            _platformBounds = platformBounds;
            _platformType = platformType;
        }
        public Rectangle PlatformBounds
        {
            get { return _platformBounds; }
        }
        public PlatformTypes PlatformType
        {
            get
            {
                return _platformType;
            }
        }
            
            

    }
}
