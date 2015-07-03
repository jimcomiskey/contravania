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
    public enum GunType { Standard, MachineGun, Rapid, Spread };
    public class Gun
    {
        public GunType GunType { get; set; }
        private double _recoilTimeRemaining;

        Texture2D _projectileTexture;
        private SoundEffect _soundProjectileHit;
        private SoundEffect _soundGunshot;
        private ContentManager _contentManager;

        public bool Automatic {get; set; }
        public bool Rapid { get; set; }

        public Gun()
        {
            GunType = GunType.Standard;
            _recoilTimeRemaining = 0;
        }
        public void Initialize(ContentManager content, GunType gunType)
        {
            _contentManager = content;
            GunType = gunType;
            string projectileTextureName = string.Empty;
            string projectileHitSoundName = string.Empty;
            string gunshotSoundName = string.Empty;
            Automatic = false;

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
                case GunType.MachineGun:
                    projectileTextureName = "Sprites/Projectiles/redbullet_medium_animated";
                    projectileHitSoundName = "Sounds/projectilehit";
                    gunshotSoundName = "Sounds/machinegunshot1";
                    Automatic = true;
                    break;
            }

            _projectileTexture = content.Load<Texture2D>(projectileTextureName);
            _soundProjectileHit = content.Load<SoundEffect>(projectileHitSoundName);
            _soundGunshot = content.Load<SoundEffect>(gunshotSoundName);
        }
        public double RecoilTimeRemaining
        {
            get { return _recoilTimeRemaining; }
            set { _recoilTimeRemaining = value; }
        }
        public List<Projectile> Fire(Vector2 gunBarrelLocation, int gunAngle, Stage currentStage)
        {
            if (_recoilTimeRemaining <= 0)
            {
                switch (GunType)
                {
                    default:
                        _recoilTimeRemaining = 0.15;
                        break;
                }
            }

            if (Rapid)
                _recoilTimeRemaining /= 2;

            _soundGunshot.Play();

            var projectiles = new List<Projectile>();
            Projectile projectile;
            Animation projectileAnimation;

            switch (GunType)
            {
                case GunType.Standard:
                case GunType.MachineGun:          
                    projectile = new Projectile();
                    projectileAnimation = new Animation();
                    projectileAnimation.Initialize(_projectileTexture, gunBarrelLocation, GunType == GunType.Standard ? 3 : 2, 10, Color.White, 1f, true, currentStage);
                    projectile.Initialize(projectileAnimation, _soundProjectileHit, gunBarrelLocation, gunAngle, currentStage, 3f);
                    projectiles.Add(projectile);
                    break;
                case GunType.Spread:
                    int spreadAngle = 15;
                    var bulletTextures = new List<Texture2D>()
                    {
                        _contentManager.Load<Texture2D>("Sprites/Projectiles/redbullet_small"),
                        _contentManager.Load<Texture2D>("Sprites/Projectiles/redbullet_medium"),
                        _contentManager.Load<Texture2D>("Sprites/Projectiles/redbullet_large")
                    };
                    int spreadGunFrameCount = 3;
                    int spreadGunFlickerSpeed = 100;
                    int flickerTime = 10;

                    projectile = new Projectile();
                    projectileAnimation = new SpreadBulletAnimation(bulletTextures, flickerTime);
                    projectileAnimation.Initialize(_projectileTexture, gunBarrelLocation, spreadGunFrameCount, spreadGunFlickerSpeed, Color.White, 1f, true, currentStage);
                    projectile.Initialize(projectileAnimation, _soundProjectileHit, gunBarrelLocation, gunAngle, currentStage, 3f);
                    projectiles.Add(projectile);

                    projectile = new Projectile();
                    projectileAnimation = new SpreadBulletAnimation(bulletTextures, flickerTime);
                    projectileAnimation.Initialize(_projectileTexture, gunBarrelLocation, spreadGunFrameCount, spreadGunFlickerSpeed, Color.White, 1f, true, currentStage);
                    projectile.Initialize(projectileAnimation, _soundProjectileHit, gunBarrelLocation, gunAngle - spreadAngle < 0 ? 360 + gunAngle - spreadAngle : gunAngle - spreadAngle, currentStage, 3f);
                    projectiles.Add(projectile);

                    projectile = new Projectile();
                    projectileAnimation = new SpreadBulletAnimation(bulletTextures, flickerTime);
                    projectileAnimation.Initialize(_projectileTexture, gunBarrelLocation, spreadGunFrameCount, spreadGunFlickerSpeed, Color.White, 1f, true, currentStage);
                    projectile.Initialize(projectileAnimation, _soundProjectileHit, gunBarrelLocation, gunAngle + spreadAngle > 360 ? gunAngle + spreadAngle - 360 : gunAngle + spreadAngle, currentStage, 3f);
                    projectiles.Add(projectile);

                    break;
            }
            
            return projectiles;
        }
        public void Update(GameTime gameTime)
        {
            if (_recoilTimeRemaining > 0)
            {
                _recoilTimeRemaining -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_recoilTimeRemaining < 0)
                {
                    _recoilTimeRemaining = 0;
                }
            }
        }
    }
}
