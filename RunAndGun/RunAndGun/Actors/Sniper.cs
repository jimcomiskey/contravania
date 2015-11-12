using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using RunAndGun.Animations;
using RunAndGun.GameObjects;
using RunAndGun.Helpers;

namespace RunAndGun.Actors
{
    public class Sniper : Enemy
    {

        public enum EnemySniperSpriteTypes
        {
            GunHigh = 0,
            GunHighRecoil = 1,
            GunNeutral = 2,
            GunNeutralRecoil = 3,
            GunLow = 4
        }
        
        private const double _reloadTime = 1500;
        private const double _recoilTime = 100;
        private const double _recoveryTime = 150;


        private int _shotsRemaining = 0;
        private double _reloadTimeRemaining = _reloadTime;
        private double _recoilTimeRemaining = 0;
        private double _recoveryTimeRemaining = 0;

        private EnemyGun _gun;
        Texture2D projectileTexture;

        PlayerSpriteCollection snipersprites;
        private int _targetClockAngle;
        

        public Sniper(ContentManager content, Vector2 position, Stage stage, string enemytype)
            : base(content, position, stage, enemytype)
        {
            snipersprites = new PlayerSpriteCollection();
            snipersprites.Initialize(content.Load<Texture2D>("Sprites/Enemies/Sniper"), position, 5, Color.White, 1f);

            ExplosionAnimation.Initialize(content.Load<Texture2D>("Sprites/Explosion1"), position, 36, 36, 3, 150, Color.White, 1f, false, false, this.CurrentStage);
            ExplosionSound = content.Load<SoundEffect>("Sounds/Explosion1");

            projectileTexture = content.Load<Texture2D>("Sprites/Projectiles/basicbullet");

            _gun = new EnemyGun(this, projectileTexture);

            
        }

        public override Rectangle BoundingBox(Vector2 proposedPosition)
        {
            int iBoundingBoxTopOffset;
            int iBoundingBoxBottomOffset;
            int iBoundingBoxLeftOffset;
            int iBoundingBoxRightOffset;

            iBoundingBoxTopOffset = 10;
            iBoundingBoxBottomOffset = 0;
            iBoundingBoxLeftOffset = 7;
            iBoundingBoxRightOffset = 12;

            EnemyMoveSpeed = 0.0f;

            return new Rectangle((int)proposedPosition.X + iBoundingBoxLeftOffset, (int)proposedPosition.Y + iBoundingBoxTopOffset, snipersprites.FrameWidth - iBoundingBoxRightOffset, snipersprites.FrameHeight - iBoundingBoxBottomOffset - iBoundingBoxTopOffset);
        }

        private Vector2 GunBarrelLocation()
        {
            Vector2 newLocation = new Vector2(this.BoundingBox().Center.X, this.BoundingBox().Center.Y);

            if (this.direction == Player.PlayerDirection.Left)
            {
                if (_targetClockAngle == 3 || _targetClockAngle == 9)
                    newLocation += new Vector2(-13, -14);
                else if (_targetClockAngle < 3 || _targetClockAngle > 9)
                    newLocation += new Vector2(-11, -26);
                else
                    newLocation += new Vector2(-13, 0);
            }
            else
            {
                if (_targetClockAngle == 3 || _targetClockAngle == 9)
                    newLocation += new Vector2(7, -17);
                else if (_targetClockAngle < 3 || _targetClockAngle > 9)
                    newLocation += new Vector2(5, -26);
                else
                    newLocation += new Vector2(7, 0);
            }


            return newLocation;
        }
        
        public override void Move(CVGameTime gameTime)
        {   
            if (_reloadTimeRemaining > 0)
            {
                _reloadTimeRemaining -= gameTime.ElapsedGameTime.TotalMilliseconds;

                if (_reloadTimeRemaining <= 0)
                {
                    _shotsRemaining = 3;
                }
                
            }
            if (_reloadTimeRemaining <= 0)
            {
                if (_shotsRemaining > 0)
                {
                    var elapsedTime = gameTime.ElapsedGameTime.TotalMilliseconds;

                    if (_recoilTimeRemaining > 0)
                    {
                        _recoilTimeRemaining -= elapsedTime;
                        if (_recoilTimeRemaining < 0)
                        {
                            _recoilTimeRemaining = 0;
                            _recoveryTimeRemaining += _recoilTimeRemaining;
                            elapsedTime = _recoilTimeRemaining * -1; // keep the difference to carry into recovery time to be decreased
                        }                        
                        else
                        {
                            elapsedTime = 0f;
                        }
                    }
                    if (_recoveryTimeRemaining > 0)
                    {
                        _recoveryTimeRemaining -= elapsedTime;
                    }

                    if (_recoilTimeRemaining <= 0 && _recoveryTimeRemaining <= 0)
                    {
                        // target
                        Player nearestPlayer = (Player)TargetingLogic.FindNearest(this.BoundingBox().Center.ToVector(), CurrentStage.Players);

                        // aim
                        this.direction = nearestPlayer.WorldPosition.X < this.WorldPosition.X ? Player.PlayerDirection.Left : Player.PlayerDirection.Right;                        
                        _targetClockAngle = TargetingLogic.FindClockPosition(this.BoundingBox().Center.ToVector(), nearestPlayer.WorldPosition);

                        // fire
                        _gun.AddProjectile(this.CurrentStage, GunBarrelLocation(), _targetClockAngle * 30, 1.2f);
                        _shotsRemaining--;                        

                        _recoilTimeRemaining = _recoilTime;
                        _recoveryTimeRemaining = _recoveryTime;
                    }                    

                }
                else
                {                    
                    // out of shots, time to reload
                    _reloadTimeRemaining = _reloadTime;
                }
            }

        }

        protected override void UpdateAnimations(CVGameTime gameTime)
        {
            // do nothing. 
            snipersprites.ScreenPosition = ScreenPosition;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            EnemySniperSpriteTypes e;
            if (_targetClockAngle == 0)
                e = EnemySniperSpriteTypes.GunNeutral;
            else if (_targetClockAngle == 3 || _targetClockAngle == 9)
                e = _recoilTimeRemaining > 0 ? EnemySniperSpriteTypes.GunNeutral : EnemySniperSpriteTypes.GunNeutralRecoil;
            else if (_targetClockAngle > 9 || _targetClockAngle < 3)
                e = _recoilTimeRemaining > 0 ? EnemySniperSpriteTypes.GunHigh : EnemySniperSpriteTypes.GunHighRecoil;
            else
                e = EnemySniperSpriteTypes.GunLow;


            snipersprites.Draw(spriteBatch, this.direction, 1f, (int) e);
        }


        
    }
}
