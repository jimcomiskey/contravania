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

namespace RunAndGun.Actors
{
    class Level1BossPanel : Enemy
    {

        private Animation _animation;


        public Level1BossPanel(ContentManager content, Vector2 position, Stage stage, string enemytype)
            : base(content, position, stage, enemytype)
        {

            _animation = new Animation();            
            
            _animation.Initialize(content.Load<Texture2D>("Sprites/Bosses/Boss1Panel"), position, 26, 32, 3, 100, Color.White, 1f, true, true, CurrentStage);

            ExplosionAnimation.Initialize(content.Load<Texture2D>("Sprites/Explosion2"), position, 32, 32, 5, 150, Color.White, 1f, false, false, CurrentStage);
            ExplosionSound = content.Load<SoundEffect>("Sounds/Explosion3");

            CollisionIsHazardous = false;
            _health = 50;            

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

        public override void Move(CVGameTime gameTime)
        {
            // do nothing
        }

        public override void ApplyPhysics(CVGameTime gameTime)
        {
            // do nothing- fixed to the Stage.
        }

        public override void Die(CVGameTime gameTime)
        {
            IsDead = true;
            Active = false;
            Vector2 firstExplosionPosition = this.WorldPosition;

            CurrentStage.AddExplosion(firstExplosionPosition + new Vector2(0, 0), ExplosionAnimation.CreateCopy(), ExplosionSound, 0);
            CurrentStage.AddExplosion(firstExplosionPosition + new Vector2(-15, -10), ExplosionAnimation.CreateCopy(), ExplosionSound, 100);
            CurrentStage.AddExplosion(firstExplosionPosition + new Vector2(-15, 10), ExplosionAnimation.CreateCopy(), null, 200);
            CurrentStage.AddExplosion(firstExplosionPosition + new Vector2(5, -10), ExplosionAnimation.CreateCopy(), ExplosionSound, 300);
            CurrentStage.AddExplosion(firstExplosionPosition + new Vector2(5, 10), ExplosionAnimation.CreateCopy(), null, 400);
            CurrentStage.AddExplosion(firstExplosionPosition + new Vector2(25, -10), ExplosionAnimation.CreateCopy(), ExplosionSound, 500);
            CurrentStage.AddExplosion(firstExplosionPosition + new Vector2(25, 10), ExplosionAnimation.CreateCopy(), null, 600);
            CurrentStage.AddExplosion(firstExplosionPosition + new Vector2(45, -10), ExplosionAnimation.CreateCopy(), ExplosionSound, 700);
            CurrentStage.AddExplosion(firstExplosionPosition + new Vector2(45, 10), ExplosionAnimation.CreateCopy(), null, 800);
            CurrentStage.AddExplosion(firstExplosionPosition + new Vector2(65, -10), ExplosionAnimation.CreateCopy(), ExplosionSound, 900);
            CurrentStage.AddExplosion(firstExplosionPosition + new Vector2(65, 10), ExplosionAnimation.CreateCopy(), null, 1000);

            foreach (var tile in CurrentStage.StageTiles.Where(t => t.DestructionLayer1GID > 0))
                tile.Status = StageTile.TileStatus.Destroyed;

            CurrentStage.StartComplete(gameTime, this);
        }

        protected override void UpdateAnimations(CVGameTime gameTime)
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
