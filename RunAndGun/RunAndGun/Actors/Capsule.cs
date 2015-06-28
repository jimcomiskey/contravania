using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RunAndGun.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RunAndGun.Actors
{
    enum CapsuleDirection { Up = -1, Down = 1 };
    class Capsule : Enemy
    {
        private Texture2D _imageTexture;
        //private SoundEffect soundDestroyed;
        
        private float _startingVerticalPosition;
        private const float _swingRange = 20f;
        private Vector2 _startPosition;
        private ContentManager _contentManager;
        
        public Capsule(ContentManager content, Vector2 position, Stage stage, string enemytype) : base (content, position, stage, enemytype)
        {
            _contentManager = content;
            _startPosition = position;
            EnemyMoveSpeed = 2.0f;

            _imageTexture = content.Load<Texture2D>("Sprites/Capsule");

            ExplosionAnimation.Initialize(content.Load<Texture2D>("Sprites/Explosion1"), this.WorldPosition, 36, 36, 3, 150, Color.White, 1f, false, false, this.currentStage);
            ExplosionSound = content.Load<SoundEffect>("Sounds/Explosion1");

            _startingVerticalPosition = position.Y;

            CollisionIsHazardous = false;       
        }

        public new int Width()
        {
            return _imageTexture.Width;
        }

        public int Height()
        {
            return _imageTexture.Height;
        }
        
        // Position of the Projectile relative to the upper left side of the screen
        
        public void Initialize(ContentManager content, Texture2D texture, SoundEffect hitsound, Vector2 position, Stage stage)
        {
            _imageTexture = texture;
            //soundDestroyed = hitsound;
            currentStage = stage;
            WorldPosition = position;            
        }

        public override void Move(Microsoft.Xna.Framework.GameTime gameTime)
        {
            // TODO: create "wave" movement pattern by manipulating velocity
            Velocity.X = EnemyMoveSpeed;            
        }
        public override void ApplyPhysics(GameTime gameTime)
        {
            WorldPosition.X += EnemyMoveSpeed;
            WorldPosition.Y = _startPosition.Y + (-(float)Math.Cos(WorldPosition.X / 20) * 20);            
        }
        public override void Die(GameTime gameTime)
        {
            ExplosionSound.Play();
            var item = new PlayerItem(_contentManager, WorldPosition, currentStage, "SpreadGun");
            currentStage.ActiveEnemies.Add(item);
            
            base.Die(gameTime);
        }

        protected override void UpdateAnimations(GameTime gametime)
        {
            // Do nothing; this sprite is not animated.
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_imageTexture, new Rectangle((int)ScreenPosition.X, (int)ScreenPosition.Y, _imageTexture.Width, _imageTexture.Height), Color.White);
            base.Draw(spriteBatch);
        }
        public override Rectangle BoundingBox()
        {
            return new Rectangle((int)WorldPosition.X, (int)WorldPosition.Y, _imageTexture.Width, _imageTexture.Height);
        }
        public override Rectangle BoundingBox(Vector2 proposedPosition)
        {
            return new Rectangle((int)proposedPosition.X, (int)proposedPosition.Y, _imageTexture.Width, _imageTexture.Height);
        }
        public override bool SpawnConditionsMet()
        {
            return (this.currentStage.CameraPosition.X > this.BoundingBox().Left);
        }

    }
}
