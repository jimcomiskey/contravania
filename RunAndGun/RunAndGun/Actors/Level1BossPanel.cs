using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using RunAndGun.Animations;

namespace RunAndGun.Actors
{
    class Level1BossPanel : Enemy
    {

        private Animation _animation;


        public Level1BossPanel(ContentManager content, Vector2 position, Stage stage, string enemytype)
            : base(content, position, stage, enemytype)
        {

            _animation = new Animation();            
            //TODO: animation.initialize
            _animation.Initialize(content.Load<Texture2D>("Sprites/Bosses/Boss1Panel"), position, 26, 32, 3, 100, Color.White, 1f, true, true, CurrentStage);

            ExplosionAnimation.Initialize(content.Load<Texture2D>("Sprites/Explosion2"), position, 32, 32, 5, 150, Color.White, 1f, false, false, CurrentStage);
            ExplosionSound = content.Load<SoundEffect>("Sounds/Explosion3");

            CollisionIsHazardous = false;
            Health = 50;            

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

            return new Rectangle((int)proposedPosition.X + iBoundingBoxLeftOffset, (int)proposedPosition.Y + iBoundingBoxTopOffset, _animation.FrameWidth - iBoundingBoxRightOffset - iBoundingBoxLeftOffset, _animation.FrameHeight - iBoundingBoxBottomOffset - iBoundingBoxTopOffset);
        }

        public override void Move(GameTime gameTime)
        {
            // do nothing
        }

        public override void ApplyPhysics(GameTime gameTime)
        {
            // do nothing- fixed to the Stage.
        }

        public override void Die(GameTime gameTime)
        {
            base.Die(gameTime);

            CurrentStage.StartComplete();
        }

        protected override void UpdateAnimations(GameTime gameTime)
        {

            _animation.Update(gameTime);
            _animation.WorldPosition = this.WorldPosition;
            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _animation.Draw(spriteBatch, Player.PlayerDirection.Right, 1f);

            base.Draw(spriteBatch);

        }



    }
}
