using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RunAndGun.GameObjects;

namespace RunAndGun.Actors
{
    class PlayerItem : Enemy
    {
        private Texture2D _imageTexture;
        //private SoundEffect soundDestroyed;

        public PlayerGun Gun { get; }

        public PlayerItem(ContentManager content, Vector2 position, Stage stage, string itemType) : base(content, position, stage, itemType)
        {
            _imageTexture = content.Load<Texture2D>(string.Format("Sprites/Enemies/PlayerItem{0}", itemType));
            ExplosionSound = content.Load<SoundEffect>("Sounds/playeritem");
            ExplosionAnimation.Initialize(content.Load<Texture2D>("Sprites/Explosion1"), this.WorldPosition, 36, 36, 3, 150, Color.White, 1f, false, false, this.CurrentStage);
            CollisionIsHazardous = false;
            VulnerableToBullets = false;

            switch(itemType)
            {                
                case "MachineGun":
                    Gun = new PlayerGun();
                    Gun.Initialize(content, GunType.MachineGun);
                    break;
                case "RapidGun":
                    Gun = new PlayerGun();
                    Gun.GunType = GunType.Rapid;                    
                    break;
                case "SpreadGun":
                    Gun = new PlayerGun();
                    Gun.Initialize(content, GunType.Spread);
                    break;
            }

            Velocity.X = 0.7f;
            IsOnGround = true;
            JumpInProgress = true;
            IsJumping = true;

            JumpLaunchVelocity = -1200f;
            GravityAcceleration = 300f;
            MaxJumpTime = 0.6f;

        }
        public override Rectangle BoundingBox(Vector2 proposedPosition)
        {
            return new Rectangle((int)WorldPosition.X, (int)WorldPosition.Y, _imageTexture.Width, _imageTexture.Height);
        }

        public override void ApplyPhysics(CVGameTime gameTime)
        {
            base.ApplyPhysics(gameTime);

            if (!this.IsJumping && this.IsOnGround)
            {
                this.Velocity.X = 0;
            }
        }
        public override void Move(CVGameTime gameTime)
        {
            // aside from physics (gravity), capsules do not move            
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_imageTexture, new Rectangle((int)ScreenPosition.X, (int)ScreenPosition.Y, _imageTexture.Width, _imageTexture.Height), Color.White);
            base.Draw(spriteBatch);
        }

        protected override void UpdateAnimations(CVGameTime gameTime)
        {
            //
        }
        public override void Die(CVGameTime gameTime)
        {
            //soundDestroyed.Play();
            ExplosionSound.Play();
            
            base.Die(gameTime);

            

        }
    }
}
