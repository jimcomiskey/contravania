using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using RunAndGun.GameObjects;
using RunAndGun.Animations;
using RunAndGun.Helpers;

namespace RunAndGun.Actors
{
    class Turret : Enemy
    {

        public float AngleBetweenTurretAndPlayer;

        private int _currentPosition;
        public enum TurretDirection
        { Clockwise = 1, CounterClockWise = -1, TargetFound = 0 };
        public TurretDirection TurretTurnDirection;
        public float TimeSinceLastMove;
        public float TimeTargetLocked;
        private const float TurretMoveDelay = 0.5f;
        private const float TurretFireDelay = 1.7f;
        private const int FrameTime = 100;
        private int _elapsedTime;
        private bool _animatingforward;
        private int _currentFrame;

        private EnemyGun _gun;
        

        List<PlayerSpriteCollection> spritecollectionlist;
        Texture2D projectileTexture;
        

        public Turret(ContentManager content, Vector2 position, Stage stage, string enemytype)
            : base(content, position, stage, enemytype)
        {

            PlayerSpriteCollection spritecollection;
            Texture2D turrettileset;

            spritecollectionlist = new List<PlayerSpriteCollection>();
            turrettileset = content.Load<Texture2D>("Sprites/Enemies/Turret");

            spritecollection = new PlayerSpriteCollection();
            spritecollection.Initialize(TextureHelper.SwapColor(turrettileset, new Color(192, 32, 0), new Color(184, 28, 12)), position, 12, Color.White, 1f);
            spritecollectionlist.Add(spritecollection);

            spritecollection = new PlayerSpriteCollection();
            spritecollection.Initialize(TextureHelper.SwapColor(turrettileset, new Color(192, 32, 0), new Color(228, 68, 52)), position, 12, Color.White, 1f);
            spritecollectionlist.Add(spritecollection);            

            spritecollection = new PlayerSpriteCollection();
            spritecollection.Initialize(turrettileset, position, 12, Color.White, 1f);
            spritecollectionlist.Add(spritecollection);
            
            CollisionIsHazardous = false;

            _health = 15;
            _currentPosition = 3;
            TimeTargetLocked = 0.0f;
            _elapsedTime = 0;
            _currentFrame = 0;
            _animatingforward = true;

            // RGB colors for blinking animations of turret.
            //R 255 228  184 
            //G 140 68   28
            //B 124 52   12

            projectileTexture = content.Load<Texture2D>("Sprites/Projectiles/basicbullet");

            ExplosionAnimation.Initialize(content.Load<Texture2D>("Sprites/Explosion2"), WorldPosition, 5, 150, Color.White, 1f, false, this.CurrentStage);
            ExplosionSound = content.Load<SoundEffect>("Sounds/Explosion2");

            _gun = new EnemyGun(this, projectileTexture);

        }

        

        public override Rectangle BoundingBox(Vector2 proposedPosition)
        {
            int iBoundingBoxTopOffset;
            int iBoundingBoxBottomOffset;
            int iBoundingBoxLeftOffset;
            int iBoundingBoxRightOffset;

            iBoundingBoxTopOffset = 7;
            iBoundingBoxBottomOffset = 7;
            iBoundingBoxLeftOffset = 7;
            iBoundingBoxRightOffset = 7;

            EnemyMoveSpeed = 0.0f;

            return new Rectangle((int)proposedPosition.X + iBoundingBoxLeftOffset, (int)proposedPosition.Y + iBoundingBoxTopOffset, spritecollectionlist[0].FrameWidth - iBoundingBoxRightOffset - iBoundingBoxLeftOffset, spritecollectionlist[0].FrameHeight - iBoundingBoxBottomOffset - iBoundingBoxTopOffset);
        }

        public override void Move(CVGameTime gameTime)
        {

            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            TimeSinceLastMove += elapsedTime;

            // do nothing 
            Player nearestPlayer = (Player) TargetingLogic.FindNearest(this.WorldPosition, CurrentStage.Players); 
            
            AngleBetweenTurretAndPlayer = TargetingLogic.FindClockPosition(this.WorldPosition, nearestPlayer.BoundingBox().Center.ToVector());

            if (AngleBetweenTurretAndPlayer != _currentPosition)
            {
                if (TimeSinceLastMove > TurretMoveDelay)
                {
                    //TimeSinceLastMove -= TurretMoveDelay;
                    TimeSinceLastMove = 0;
            
                    //float currentTurretAngle = iCurrentPosition * 30;
                    int iCounterDistance = 0;
                    if (_currentPosition < AngleBetweenTurretAndPlayer)
                        iCounterDistance = 12 - (int)AngleBetweenTurretAndPlayer + _currentPosition;
                    else
                        iCounterDistance = _currentPosition - (int)AngleBetweenTurretAndPlayer;

                    if (iCounterDistance < 6)
                        TurretTurnDirection = TurretDirection.CounterClockWise;
                    else
                        TurretTurnDirection = TurretDirection.Clockwise;

                    _currentPosition += (int)TurretTurnDirection;
                    if (_currentPosition == 12)
                        _currentPosition = 0;
                    else if (_currentPosition == -1)
                        _currentPosition = 11;
                }

            }
            else
            {
                TurretTurnDirection = TurretDirection.TargetFound;
                TimeTargetLocked += elapsedTime;
                if (TimeTargetLocked > TurretFireDelay)
                {
                    TimeTargetLocked = 0.0f;
                    _gun.AddProjectile(this.CurrentStage, GunBarrelLocation(), _currentPosition * 30, 2f);
                }
            }
            
            //float f = AngleBetweenTurretAndPlayer - currentTurretAngle;

            //currentTurretDirection = (TurretDirection) Math.Sign(-f % 360 - f % 360);


        }

        private Vector2 GunBarrelLocation()
        {            
            return this.BoundingBox().Center.ToVector() + GunBarrelLocationOffset(_currentPosition);
        }
        private Vector2 GunBarrelLocationOffset(int position)
        {
            switch (_currentPosition)
            {
                case 0:
                    return new Vector2(-2, -16);
                case 1:
                    return new Vector2(6, -16);
                case 2:
                    return new Vector2(13, -10);
                case 3:
                    return new Vector2(14, -2);
                case 4:
                    return new Vector2(13, 6);
                case 5:
                    return new Vector2(6, 12);
                case 6:
                    return new Vector2(-2, 12);
                case 7:
                    return new Vector2(-9, 12);
                case 8:
                    return new Vector2(-15, 7);
                case 9:
                    return new Vector2(-17, -2);
                case 10:
                    return new Vector2(-15, -10);
                case 11:
                    return new Vector2(-9, -16);
                default:
                    return new Vector2(0, 0);
            }
        }

        public override void ApplyPhysics(CVGameTime gameTime)
        {
            // do nothing- Turret is fixed to the Stage.
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

            foreach(PlayerSpriteCollection psc in spritecollectionlist)
                psc.ScreenPosition = ScreenPosition;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            int iCurrentFrame = 0;
            if (_currentPosition < 0)
                iCurrentFrame = 12 + _currentPosition;
            else
                iCurrentFrame = _currentPosition;

            spritecollectionlist[_currentFrame].Draw(spriteBatch, this.direction, 1f, iCurrentFrame);
            base.Draw(spriteBatch);

        }



    }
}
