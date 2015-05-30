using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RunAndGun.GameObjects
{
    public enum GunType { Standard };
    public class Gun
    {        
        public GunType GunType { get; set; }
        private double recoilTimeRemaining;
        public Gun()
        {
            GunType = GunType.Standard;
            recoilTimeRemaining = 0;
        }
        public double RecoilTimeRemaining
        {
            get { return recoilTimeRemaining; }
            set { recoilTimeRemaining = value; }
        }
        public void Fire()
        {
            if (recoilTimeRemaining <= 0)
            {
                switch (GunType)
                {
                    default:
                        recoilTimeRemaining = 0.15;
                        break;
                }
            }
        }
        public void Update(GameTime gameTime)
        {
            if (recoilTimeRemaining > 0)
            {
                recoilTimeRemaining -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (recoilTimeRemaining < 0)
                {
                    recoilTimeRemaining = 0;
                }
            }
        }
    }
}
