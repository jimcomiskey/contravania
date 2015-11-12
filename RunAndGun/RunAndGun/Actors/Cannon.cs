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
using RunAndGun;

namespace RunAndGun.Actors
{
    public class Cannon : Enemy
    {

        public enum EnemyCannonSpriteTypes
        {
            EmergeOne = 0, 
            EmergeTwo = 1, 
            GunNeutral = 2,
            GunHigh = 3, 
            GunUp = 4
        }

        private enum _currentAction
        {
            Loading = 0,
            Shooting = 1
        };

        private const double _emergeTime = 50;
        private const double _reloadTime = 3000;        
        private const double _recoveryTime = 250;

        private int _emergeStatus = 0;
        private int _shotsRemaining = 0;
        private double _emergeTimeRemaining = 0;
        private double _reloadTimeRemaining = 500;        
        private double _recoveryTimeRemaining = 0;

        private EnemyGun _gun;
        Texture2D projectileTexture;

        private int _targetClockAngle;

        List<PlayerSpriteCollection> spritecollectionlist;

        // "glowing" animation variables
        private const int FrameTime = 100;
        private int _elapsedTime;
        private bool _animatingforward;
        private int _currentFrame;


        public Cannon(ContentManager content, Vector2 position, Stage stage, string enemytype)
            : base(content, position, stage, enemytype)
        {
            PlayerSpriteCollection spritecollection;
            spritecollectionlist = new List<PlayerSpriteCollection>();
            Texture2D cannonTileset = content.Load<Texture2D>("Sprites/Enemies/Cannon");

            spritecollection = new PlayerSpriteCollection();
            spritecollection.Initialize(TextureHelper.SwapColor(cannonTileset, new Color(192, 32, 0), new Color(184, 28, 12)), position, 5, Color.White, 1f);
            spritecollectionlist.Add(spritecollection);

            spritecollection = new PlayerSpriteCollection();
            spritecollection.Initialize(TextureHelper.SwapColor(cannonTileset, new Color(192, 32, 0), new Color(228, 68, 52)), position, 5, Color.White, 1f);
            spritecollectionlist.Add(spritecollection);

            spritecollection = new PlayerSpriteCollection();
            spritecollection.Initialize(cannonTileset, position, 5, Color.White, 1f);
            spritecollectionlist.Add(spritecollection);

            CollisionIsHazardous = false;

            _health = 15;

            _currentFrame = 0;
            _animatingforward = true;

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

            iBoundingBoxTopOffset = 0;
            iBoundingBoxBottomOffset = 0;
            iBoundingBoxLeftOffset = 0;
            iBoundingBoxRightOffset = 0;

            EnemyMoveSpeed = 0.0f;

            return new Rectangle((int)proposedPosition.X + iBoundingBoxLeftOffset, (int)proposedPosition.Y + iBoundingBoxTopOffset, spritecollectionlist[0].FrameWidth - iBoundingBoxRightOffset, spritecollectionlist[0].FrameHeight - iBoundingBoxBottomOffset - iBoundingBoxTopOffset);
        }

        private Vector2 GunBarrelLocation()
        {
            Vector2 newLocation = new Vector2(this.BoundingBox().Center.X, this.BoundingBox().Center.Y);

            if (this.direction == Player.PlayerDirection.Left)
            {                    
                if (_targetClockAngle == 10 || _targetClockAngle == 2)
                    newLocation += new Vector2(-17, -8);
                else if (_targetClockAngle == 11 || _targetClockAngle == 1)
                    newLocation += new Vector2(-10, -17);
                else
                    newLocation += new Vector2(-12, -1);
            }
            else
            {
                if (_targetClockAngle == 10 || _targetClockAngle == 2)
                    newLocation += new Vector2(17, -8);
                else if (_targetClockAngle == 11 || _targetClockAngle == 1)
                    newLocation += new Vector2(10, -17);
                else
                    newLocation += new Vector2(12, -1);
            }


            return newLocation;
        }

        public override void Move(CVGameTime gameTime)
        {
            if (_emergeStatus == 0 && SpawnConditionsMet())
            {
                _emergeStatus = 1;
                _emergeTimeRemaining = _emergeTime;
            }
            else if (_emergeStatus == 1 || _emergeStatus == 2)
            {
                 if (_emergeTimeRemaining > 0)
                {
                    _emergeTimeRemaining -= gameTime.ElapsedGameTime.Milliseconds;
                    if (_emergeTimeRemaining <= 0)
                    {
                        _emergeStatus++;
                        _emergeTimeRemaining = _emergeTime + Math.Abs(_emergeTimeRemaining);
                    }
                }
            }
            else if (_emergeStatus == 3)
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

                        //if (_recoilTimeRemaining > 0)
                        //{
                        //    _recoilTimeRemaining -= elapsedTime;
                        //    if (_recoilTimeRemaining < 0)
                        //    {
                        //        _recoilTimeRemaining = 0;
                        //        _recoveryTimeRemaining += _recoilTimeRemaining;
                        //        elapsedTime = _recoilTimeRemaining * -1; // keep the difference to carry into recovery time to be decreased
                        //    }
                        //    else
                        //    {
                        //        elapsedTime = 0f;
                        //    }
                        //}
                        if (_recoveryTimeRemaining > 0)
                        {
                            _recoveryTimeRemaining -= elapsedTime;
                        }

                        if (//_recoilTimeRemaining <= 0 && 
                            _recoveryTimeRemaining <= 0)
                        {
                            // target
                            Player nearestPlayer = (Player)TargetingLogic.FindNearest(this.BoundingBox().Center.ToVector(), CurrentStage.Players);

                            // aim                            
                            _targetClockAngle = TargetingLogic.FindClockPosition(this.BoundingBox().Center.ToVector(), nearestPlayer.BoundingBox().Center.ToVector());

                            if (_targetClockAngle >= 9 && _targetClockAngle <= 11)
                            {
                                // fire
                                _gun.AddProjectile(this.CurrentStage, GunBarrelLocation(), _targetClockAngle * 30, 1.2f);
                                _shotsRemaining--;
                            }

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
        }

        public override bool SpawnConditionsMet()
        {
            return (CurrentStage.ScreenCoordinates().Contains(BoundingBox()));            
        }

        protected override void UpdateAnimations(CVGameTime gameTime)
        {

            _elapsedTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_elapsedTime > FrameTime)
            {
                // Move to the next frame
                if (_animatingforward)
                {
                    _currentFrame++;
                    if (_currentFrame == spritecollectionlist.Count)
                    {
                        _currentFrame = spritecollectionlist.Count - 1;
                        _animatingforward = false;

                    }
                }
                else
                {
                    _currentFrame--;
                    if (_currentFrame == 0)
                    {

                        _animatingforward = true;

                        _currentFrame = 0;

                    }
                }

                // Reset the elapsed time to zero
                _elapsedTime = 0;
            }

            foreach (PlayerSpriteCollection psc in spritecollectionlist)
                psc.ScreenPosition = ScreenPosition;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            EnemyCannonSpriteTypes e;

            if (_emergeStatus > 0 && _emergeStatus < 3)
            {
                spritecollectionlist[_currentFrame].Draw(spriteBatch, this.direction, 1f, _emergeStatus-1);
            }
            else
            {
                if (_targetClockAngle == 3 || _targetClockAngle == 9)
                    e = EnemyCannonSpriteTypes.GunNeutral;
                else if (_targetClockAngle == 10 || _targetClockAngle == 2)
                    e = EnemyCannonSpriteTypes.GunHigh;
                else if (_targetClockAngle == 11 || _targetClockAngle == 12)
                    e = EnemyCannonSpriteTypes.GunUp;
                else
                    e = EnemyCannonSpriteTypes.GunNeutral;

                spritecollectionlist[_currentFrame].Draw(spriteBatch, this.direction, 1f, (int)e);
            }
            
        }



    }
}
