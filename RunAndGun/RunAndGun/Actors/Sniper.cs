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
    class Sniper : Enemy
    {

        

        PlayerSpriteCollection snipersprites;

        public Sniper(ContentManager content, Vector2 position, Stage stage, string enemytype)
            : base(content, position, stage, enemytype)
        {
            snipersprites = new PlayerSpriteCollection();
            snipersprites.Initialize(content.Load<Texture2D>("Sprites/Enemies/Sniper"), position, 1, Color.White, 1f);

            ExplosionAnimation.Initialize(content.Load<Texture2D>("Sprites/Explosion1"), position, 36, 36, 3, 150, Color.White, 1f, false, false, this.currentStage);
            ExplosionSound = content.Load<SoundEffect>("Sounds/Explosion1");
        }

        public override Rectangle BoundingBox(Vector2 proposedPosition)
        {
            int iBoundingBoxTopOffset;
            int iBoundingBoxBottomOffset;
            int iBoundingBoxLeftOffset;
            int iBoundingBoxRightOffset;

            iBoundingBoxTopOffset = 17;
            iBoundingBoxBottomOffset = 0;
            iBoundingBoxLeftOffset = 7;
            iBoundingBoxRightOffset = 12;

            EnemyMoveSpeed = 0.0f;

            return new Rectangle((int)proposedPosition.X + iBoundingBoxLeftOffset, (int)proposedPosition.Y + iBoundingBoxTopOffset, snipersprites.FrameWidth - iBoundingBoxRightOffset, snipersprites.FrameHeight - iBoundingBoxBottomOffset - iBoundingBoxTopOffset);
        }

        public override void Move(GameTime gameTime)
        {
            // do nothing 
        }

        protected override void UpdateAnimations(GameTime gameTime)
        {
            // do nothing. 
            snipersprites.ScreenPosition = ScreenPosition;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            snipersprites.Draw(spriteBatch, this.direction, 1f, PlayerSpriteCollection.EnemySniperSpriteTypes.GunNeutral);
        }


        
    }
}
