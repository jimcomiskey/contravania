using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using RunAndGun.GameObjects;
using RunAndGun.Animations;

namespace RunAndGun.Actors
{
    public abstract class Actor
    {

        public string Name;  // I only use this for debugging purposes at this point.
        public string DebugInfo;        

        public virtual Stage CurrentStage { get; set; }
        
        public Vector2 previousPosition;

        public Vector2 WorldPosition;

        public Vector2 ScreenPosition
        {
            get { return WorldPosition - CurrentStage.CameraPosition; }
        }
        protected float PreviousBottom;

        //public float movement;
        public Vector2 Velocity;

        protected float JumpTime;
        public bool IsJumping; // set to true when user hits jump button and is eligible to jump. goes back to false after jump initiated.
        public bool JumpInProgress; // set to true when user hits jump button, and goes to false once the player lands.
        protected bool WasJumping;

        public abstract Rectangle BoundingBox();
        public abstract int Width { get; }

        protected float MaxGroundVelocity = 1.2f; // 1.2f;
        public float MaxJumpTime = 0.42f; // 0.35f; // 0.35f;
        public float JumpLaunchVelocity = -1200f; // -3500.0f;
        public float JumpLaunchVelocityDying = -600f;
        public float GravityAcceleration = 500f; // 3400.0f;
        public float MaxFallVelocity = 600f; //  550.0f;
        protected const float JumpControlPower = 0.14f; // 0.14f;
        protected const float MoveStickScale = 1.0f;

        public bool IsDying;
        public bool IsOnGround; // set to false at beginning of ApplyPhysics and set to true when a collision is detected. therefore, gravity is applied at all times, even when player is on the ground.
        public Animation ExplosionAnimation = new Animation();
        public SoundEffect ExplosionSound;        

        public List<StageTile> getCurrentPlatformTiles()
        {
            List<StageTile> currentPlatformTiles = new List<StageTile>();

            if (this.IsOnGround)
            {
                Rectangle actorbounds = this.BoundingBox();

                //TODO: borrowing code from HandleCollisions, possible consolidation?
                int leftTile = (int)Math.Floor((float)actorbounds.Left / CurrentStage.TileWidth);
                int rightTile = (int)Math.Ceiling(((float)actorbounds.Right / CurrentStage.TileWidth)) - 1;
                int bottomTile = (int)Math.Ceiling(((float)actorbounds.Bottom / CurrentStage.TileHeight)) - 1;

                for (int iIndex = leftTile; iIndex <= rightTile; iIndex++)
                    currentPlatformTiles.Add(CurrentStage.getStageTileByGridPosition(iIndex, bottomTile));

                return currentPlatformTiles;
            }
            else
            {
                return null;
            }

        }
        public abstract void Move(CVGameTime gameTime);
        public virtual void ApplyPhysics(CVGameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousWorldPosition = this.WorldPosition;
            previousPosition = this.WorldPosition;

            // Get Thumbstick Controls            
            //this.Velocity.X = this.currentGamePadState.ThumbSticks.Left.X * this.MaxGroundVelocity;
            //Velocity.X = movement * elapsed; 

            this.Velocity.Y = MathHelper.Clamp(this.Velocity.Y + GravityAcceleration * elapsed, -MaxFallVelocity, MaxFallVelocity);
            this.Velocity.Y = DoJump(this.Velocity.Y, gameTime);
            
            this.WorldPosition.X += this.Velocity.X;

            if (this.Velocity.Y > 0 && this.Velocity.Y * elapsed < 1)
                this.WorldPosition.Y += 1;
            else
                this.WorldPosition.Y += this.Velocity.Y * elapsed;

            this.WorldPosition.Y = (float)Math.Round(this.WorldPosition.Y);

            HandleCollisions(gameTime);

            this.IsJumping = false;

            if (WorldPosition.X == previousPosition.X)
                Velocity.X = 0.0f;

            if (WorldPosition.Y == previousPosition.Y)
                Velocity.Y = 0.0f;

        }        
        public virtual void HandleCollisions(CVGameTime gameTime)
        {
            if (this.WorldPosition.Y > Game.iScreenModelWidth && IsDying == false)
            {
                Die(gameTime);
            }

            Rectangle actorbounds = this.BoundingBox();

            bool wasonground = this.IsOnGround;
            
            // get nearest tile below player.
            this.IsOnGround = false;
            
            int leftTile = (int)Math.Floor((float)actorbounds.Left / CurrentStage.TileWidth);
            int rightTile = (int)Math.Ceiling(((float)actorbounds.Right / CurrentStage.TileWidth)) - 1;
            int topTile = (int)Math.Floor((float)actorbounds.Top / CurrentStage.TileHeight);
            int bottomTile = (int)Math.Ceiling(((float)actorbounds.Bottom / CurrentStage.TileHeight)) - 1;

            // For each potentially colliding platform tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    StageTile stageTile = CurrentStage.getStageTileByGridPosition(x, y);
                    if (stageTile != null)
                    {
                        if (stageTile.IsImpassable())
                        {
                            Rectangle tilebounds = CurrentStage.getTileBoundsByGridPosition(x, y);
                            Vector2 depth = RectangleExtensions.GetIntersectionDepth(actorbounds, tilebounds);

                            if (actorbounds.Intersects(tilebounds))
                            {
                                WorldPosition = new Vector2(WorldPosition.X + depth.X, WorldPosition.Y);
                                actorbounds = this.BoundingBox();
                            }
                        }

                        else if (stageTile.IsPlatform() && y == bottomTile)
                        {
                            List<Platform> tileboundsList = CurrentStage.getTilePlatformBoundsByGridPosition(x, bottomTile);
                            foreach (Platform platformbounds in tileboundsList)
                            {
                                Rectangle tilebounds = platformbounds.PlatformBounds;
                                Vector2 depth = RectangleExtensions.GetIntersectionDepth(actorbounds, tilebounds);


                                if (this.PreviousBottom <= tilebounds.Top && Velocity.Y >= 0 && actorbounds.Intersects(tilebounds))
                                //if (Velocity.Y >= 0 && (depth.Y < 0)) // || this.IgnoreNextPlatform))
                                {                                    
                                    this.IsOnGround = true;
                                    this.JumpInProgress = false;
                                    //this.Velocity.X = 0f;

                                    this.WorldPosition.Y += depth.Y;
                                    // perform further collisions with the new bounds
                                    actorbounds = this.BoundingBox();
                                }
                            }
                        }
                    }
                }
            }

            if (wasonground && !IsOnGround)
                Velocity.Y = 0;

            this.PreviousBottom = actorbounds.Bottom;
        }
        protected virtual float DoJump(float velocityY, CVGameTime gameTime)
        {
            float launchVelocity = 0f;
            if (IsDying)
                launchVelocity = JumpLaunchVelocityDying;
            else
                launchVelocity = JumpLaunchVelocity;


            // If the player wants to jump
            if (this.IsJumping || this.JumpInProgress)
            {
                // Begin or continue a jump
                if ((!this.WasJumping && this.IsOnGround) || this.JumpTime > 0.0f)
                {
                    this.JumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    //sprite.PlayAnimation(jumpAnimation);
                }

                // If we are in the ascent of the jump
                if (0.0f < this.JumpTime && this.JumpTime <= MaxJumpTime)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocityY = launchVelocity * (1.0f - (float)Math.Pow(this.JumpTime / MaxJumpTime, Player.JumpControlPower));
                }
                else
                {
                    // Reached the apex of the jump
                    this.JumpTime = 0.0f;
                }
            }
            else
            {
                // Continues not jumping or cancels a jump in progress
                this.JumpTime = 0.0f;
            }
            this.WasJumping = this.IsJumping;

            return velocityY;
        } 
        public abstract void Die(CVGameTime gameTime);

        public virtual void Draw(SpriteBatch spriteBatch)
        {
        }

        public void DrawDebug(SpriteBatch spriteBatch)
        {
            Rectangle drawBox;
            if (this.GetType() == typeof(Player))
            {
                drawBox = ((Player)this).HurtBox();
            }
            else
            {
                drawBox = this.BoundingBox();
            }
            Texture2D txt = new Texture2D(spriteBatch.GraphicsDevice, drawBox.Width, drawBox.Height);

            Color[] data = new Color[txt.Width * txt.Height];

            for (int i = 0; i < data.Length; i++)
            {
                data[i].R = 255;
            }

            txt.SetData(data);
            spriteBatch.Draw(txt, new Vector2(drawBox.X - CurrentStage.CameraPosition.X, drawBox.Y - CurrentStage.CameraPosition.Y), Color.White);

        }
    }
}
