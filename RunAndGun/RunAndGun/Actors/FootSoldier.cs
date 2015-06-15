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

namespace RunAndGun.Actors
{
    class FootSoldier : Enemy
    {

        private Animation runningAnimation;

        public FootSoldier(ContentManager content, Vector2 position, Stage stage, string enemytype)
            : base(content, position, stage, enemytype)
        {
            EnemyMoveSpeed = 1.2f;
            runningAnimation = new Animation();            
            runningAnimation.Initialize(content.Load<Texture2D>("Sprites/EnemyFootSoldierRunning"), this.WorldPosition, 32, 48, 6, 150, Color.White, 1, true, false, currentStage);

            ExplosionAnimation.Initialize(content.Load<Texture2D>("Sprites/Explosion1"), this.WorldPosition, 36, 36, 3, 150, Color.White, 1f, false, false, this.currentStage);
            ExplosionSound = content.Load<SoundEffect>("Sounds/Explosion1");
        }

        public override void Move(Microsoft.Xna.Framework.GameTime gameTime)
        {
            
            Vector2 proposedPosition;
            if (Velocity.X > 0)
                proposedPosition = new Vector2(WorldPosition.X + BoundingBox().Width * 0.9f, WorldPosition.Y);
            else if (Velocity.X < 0)
                proposedPosition = new Vector2(WorldPosition.X - BoundingBox().Width * 0.9f, WorldPosition.Y);
            else
                proposedPosition = WorldPosition;
            // simulate gravity to determine if proposed position would collide with platform tile or not.
            // otherwise
            proposedPosition.Y += 1;

            Rectangle proposedbounds = this.BoundingBox(proposedPosition);
            bool bChangeDirection = false;

            bool bWouldBeOnGround = false;

            int leftTile = (int)Math.Floor((float)proposedbounds.Left / currentStage.iTileWidth);
            int rightTile = (int)Math.Ceiling(((float)proposedbounds.Right / currentStage.iTileWidth)) - 1;
            int topTile = (int)Math.Floor((float)proposedbounds.Top / currentStage.iTileHeight);
            int bottomTile = (int)Math.Ceiling(((float)proposedbounds.Bottom / currentStage.iTileHeight)) - 1;

            // For each potentially colliding platform tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    StageTile stageTile = currentStage.getStageTileByGridPosition(x, y);
                    if (stageTile != null)
                    {
                        if (stageTile.IsImpassable())
                        {
                            Rectangle tilebounds = currentStage.getTileBoundsByGridPosition(x, y);
                            if (proposedbounds.Intersects(tilebounds))
                            {
                                bChangeDirection = true;
                            }
                        }

                        if (stageTile.IsPlatform() && y == bottomTile)
                        {

                            List<Platform> tileboundsList = currentStage.getTilePlatformBoundsByGridPosition(x, bottomTile);

                            foreach (Platform platformbounds in tileboundsList)
                            {
                                Rectangle tilebounds = platformbounds.PlatformBounds;
                                if (proposedbounds.Bottom >= tilebounds.Top && proposedbounds.Intersects(tilebounds))
                                {
                                    bWouldBeOnGround = true;
                                }
                            }
                        }
                    }
                }
            }
            if (bChangeDirection)
            {
                ChangeDirection();
            }
            else if (bWouldBeOnGround == false)
            {
                ChangeDirection();
            }
            else if (this.direction == Player.PlayerDirection.Left)
            {
                Velocity.X = -EnemyMoveSpeed;
            }
            else if (this.direction == Player.PlayerDirection.Right)
            {
                Velocity.X = EnemyMoveSpeed;
            }
        }

        

        //public override void Initialize(Microsoft.Xna.Framework.Content.ContentManager content, Microsoft.Xna.Framework.Vector2 position, Stage stage, string enemytype)
        //{
        //    base.Initialize(content, position, stage, enemytype);

        //    //ScreenPosition = currentStage.CameraPosition + new Vector2(Game.iScreenModelWidth, 62);            
            
        //}

        protected override void UpdateAnimations(GameTime gameTime)
        {
            runningAnimation.WorldPosition = this.WorldPosition;
            ExplosionAnimation.WorldPosition = this.WorldPosition;
            runningAnimation.Update(gameTime);

        }

        public override Rectangle BoundingBox(Vector2 proposedPosition)
        {
            int iBoundingBoxTopOffset;
            int iBoundingBoxBottomOffset;
            int iBoundingBoxLeftOffset;
            int iBoundingBoxRightOffset;

            iBoundingBoxTopOffset = 8;
            iBoundingBoxBottomOffset = 16;
            iBoundingBoxLeftOffset = 7;
            iBoundingBoxRightOffset = 12;

            return new Rectangle((int)proposedPosition.X + iBoundingBoxLeftOffset, (int)proposedPosition.Y + iBoundingBoxTopOffset, runningAnimation.FrameWidth - iBoundingBoxRightOffset, runningAnimation.FrameHeight - iBoundingBoxBottomOffset);            
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            runningAnimation.Draw(spriteBatch, direction, 1f);
         
            base.Draw(spriteBatch);
            
        }
        
    }
}
