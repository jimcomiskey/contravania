using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RunAndGun.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RunAndGun.GameObjects
{
    public enum GunType { Standard, Spread };
    public class Gun
    {        
        public GunType GunType { get; set; }
        private double recoilTimeRemaining;

        Texture2D projectileTexture;
        private SoundEffect soundProjectileHit;
        private SoundEffect soundGunshot;
        private ContentManager contentManager;

        public Gun()
        {
            GunType = GunType.Standard;
            recoilTimeRemaining = 0;
        }
        public void Initialize(ContentManager content, GunType gunType)
        {
            contentManager = content;
            GunType = gunType;
            string projectileTextureName = string.Empty;
            string projectileHitSoundName = string.Empty;
            string gunshotSoundName = string.Empty;
            switch (GunType)
            {
                case GunType.Standard:
                    projectileTextureName = "Sprites/Projectiles/basicbulletanimated";
                    projectileHitSoundName = "Sounds/projectilehit";
                    gunshotSoundName = "Sounds/gunshot3";
                    break;
                case GunType.Spread:
                    projectileTextureName = "Sprites/Projectiles/redbullet_medium";
                    projectileHitSoundName = "Sounds/projectilehit";
                    gunshotSoundName = "Sounds/spreadgunshot";
                    break;
            }

            projectileTexture = content.Load<Texture2D>(projectileTextureName);
            soundProjectileHit = content.Load<SoundEffect>(projectileHitSoundName);
            soundGunshot = content.Load<SoundEffect>(gunshotSoundName);
        }
        public double RecoilTimeRemaining
        {
            get { return recoilTimeRemaining; }
            set { recoilTimeRemaining = value; }
        }
        public List<Projectile> Fire(Vector2 gunBarrelLocation, int gunAngle, Stage currentStage)
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

            soundGunshot.Play();

            var projectiles = new List<Projectile>();
            Projectile projectile;
            Animation projectileAnimation;

            switch (GunType)
            {
                case GunType.Standard:            
                    projectile = new Projectile();
                    projectileAnimation = new Animation();
                    projectileAnimation.Initialize(projectileTexture, gunBarrelLocation, 3, 10, Color.White, 1f, true, currentStage);
                    projectile.Initialize(projectileAnimation, soundProjectileHit, gunBarrelLocation, gunAngle, currentStage, 3f);
                    projectiles.Add(projectile);
                    break;
                case GunType.Spread:
                    int spreadAngle = 15;
                    var bulletTextures = new List<Texture2D>()
                    {
                        contentManager.Load<Texture2D>("Sprites/Projectiles/redbullet_small"),
                        contentManager.Load<Texture2D>("Sprites/Projectiles/redbullet_medium"),
                        contentManager.Load<Texture2D>("Sprites/Projectiles/redbullet_large")
                    };
                    int spreadGunFrameCount = 3;
                    int spreadGunFlickerSpeed = 100;
                    int flickerTime = 10;

                    projectile = new Projectile();
                    projectileAnimation = new SpreadBulletAnimation(bulletTextures, flickerTime);
                    projectileAnimation.Initialize(projectileTexture, gunBarrelLocation, spreadGunFrameCount, spreadGunFlickerSpeed, Color.White, 1f, true, currentStage);
                    projectile.Initialize(projectileAnimation, soundProjectileHit, gunBarrelLocation, gunAngle, currentStage, 3f);
                    projectiles.Add(projectile);

                    projectile = new Projectile();
                    projectileAnimation = new SpreadBulletAnimation(bulletTextures, flickerTime);
                    projectileAnimation.Initialize(projectileTexture, gunBarrelLocation, spreadGunFrameCount, spreadGunFlickerSpeed, Color.White, 1f, true, currentStage);
                    projectile.Initialize(projectileAnimation, soundProjectileHit, gunBarrelLocation, gunAngle - spreadAngle < 0 ? 360 + gunAngle - spreadAngle : gunAngle - spreadAngle, currentStage, 3f);
                    projectiles.Add(projectile);

                    projectile = new Projectile();
                    projectileAnimation = new SpreadBulletAnimation(bulletTextures, flickerTime);
                    projectileAnimation.Initialize(projectileTexture, gunBarrelLocation, spreadGunFrameCount, spreadGunFlickerSpeed, Color.White, 1f, true, currentStage);
                    projectile.Initialize(projectileAnimation, soundProjectileHit, gunBarrelLocation, gunAngle + spreadAngle > 360 ? gunAngle + spreadAngle - 360 : gunAngle + spreadAngle, currentStage, 3f);
                    projectiles.Add(projectile);

                    break;
            }
            
            return projectiles;
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
