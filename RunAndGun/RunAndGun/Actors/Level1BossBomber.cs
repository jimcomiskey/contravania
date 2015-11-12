using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RunAndGun.GameObjects;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RunAndGun.Animations;
using Microsoft.Xna.Framework.Audio;
using RunAndGun.Helpers;

namespace RunAndGun.Actors
{
    public class Level1BossBomber : Enemy
    {   
        private PlayerSpriteCollection _frames; 
        private Texture2D projectileTexture;        
        
        private enum _frameTypes
        {
            Left = 0, LeftRecoil = 1, LeftDestroyed = 2, Right = 3, RightRecoil = 4, RightDestroyed = 5
        }

        private string _enemyType;

        public Level1BossBomber(ContentManager content, Vector2 position, Stage stage, string enemytype)
            : base(content, position, stage, enemytype)
        {
            _frames = new PlayerSpriteCollection();
            _frames.Initialize(content.Load<Texture2D>("Sprites/Bosses/boss1bomber"), position, 6, Color.White, 1f);
            _enemyType = enemytype;
            
            _health = 32;

            ExplosionAnimation.Initialize(content.Load<Texture2D>("Sprites/Explosion2"), position, 32, 32, 5, 150, Color.White, 1f, false, false, this.CurrentStage);
            ExplosionSound = content.Load<SoundEffect>("Sounds/Explosion1");

            projectileTexture = content.Load<Texture2D>("Sprites/Projectiles/redbullet_large");
        }
        public override Rectangle BoundingBox(Vector2 proposedPosition)
        {   
            int iBoundingBoxTopOffset;
            int iBoundingBoxBottomOffset;
            int iBoundingBoxLeftOffset;
            int iBoundingBoxRightOffset;

            iBoundingBoxTopOffset = -2;
            iBoundingBoxBottomOffset = 0;
            iBoundingBoxLeftOffset = 0;
            iBoundingBoxRightOffset = 0;

            EnemyMoveSpeed = 0.0f;

            return new Rectangle((int)proposedPosition.X + iBoundingBoxLeftOffset, (int)proposedPosition.Y + iBoundingBoxTopOffset, _frames.FrameWidth - iBoundingBoxRightOffset - iBoundingBoxLeftOffset, _frames.FrameHeight - iBoundingBoxBottomOffset - iBoundingBoxTopOffset);
        }

        public override void Move(CVGameTime gameTime)
        {
            // TODO: fire projectiles at intervals
        }
        public override void ApplyPhysics(CVGameTime gameTime)
        {
            // do nothing            
        }
        public override void Die(CVGameTime gameTime)
        {   
            CurrentStage.AddExplosion(this.BoundingBox().Center.ToVector() - new Vector2(ExplosionAnimation.FrameWidth / 2, ExplosionAnimation.FrameHeight / 2), ExplosionAnimation, ExplosionSound);
            IsDead = true;
            VulnerableToBullets = false;
            CollisionIsHazardous = false;            
        }

        protected override void UpdateAnimations(CVGameTime gameTime)
        {
            _frames.ScreenPosition = ScreenPosition;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsDead)
            {
                if (_enemyType.Contains("Left"))
                {
                    _frames.Draw(spriteBatch, this.direction, 1f, (int)_frameTypes.Left);
                }
                else
                {
                    _frames.Draw(spriteBatch, this.direction, 1f, (int)_frameTypes.Right);
                }
            }
            else
            {
                if (_enemyType.Contains("Left"))
                {
                    _frames.Draw(spriteBatch, this.direction, 1f, (int)_frameTypes.LeftDestroyed);
                }
                else
                {
                    _frames.Draw(spriteBatch, this.direction, 1f, (int)_frameTypes.RightDestroyed);
                }
            }
            base.Draw(spriteBatch);
        }
    }
}
