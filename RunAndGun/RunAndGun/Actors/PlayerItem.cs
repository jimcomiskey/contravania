﻿using System;
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
        private Texture2D imageTexture;
        //private SoundEffect soundDestroyed;

        public Gun Gun { get; }

        public PlayerItem(ContentManager content, Vector2 position, Stage stage, string itemType) : base(content, position, stage, itemType)
        {
            imageTexture = content.Load<Texture2D>(string.Format("Sprites/PlayerItem{0}", itemType));
            ExplosionSound = content.Load<SoundEffect>("Sounds/playeritem");
            ExplosionAnimation.Initialize(content.Load<Texture2D>("Sprites/Explosion1"), this.WorldPosition, 36, 36, 3, 150, Color.White, 1f, false, false, this.currentStage);
            CollisionIsHazardous = false;
            VulnerableToBullets = false;

            switch(itemType)
            {
                case "SpreadGun":
                    Gun = new Gun();
                    Gun.Initialize(content, GunType.Spread);
                    break;
            }
        }
        public override Rectangle BoundingBox(Vector2 proposedPosition)
        {
            return new Rectangle((int)WorldPosition.X, (int)WorldPosition.Y, imageTexture.Width, imageTexture.Height);
        }        

        public override void Move(GameTime gameTime)
        {
            // aside from physics (gravity), capsules do not move
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(imageTexture, new Rectangle((int)ScreenPosition.X, (int)ScreenPosition.Y, imageTexture.Width, imageTexture.Height), Color.White);
            base.Draw(spriteBatch);
        }

        protected override void UpdateAnimations(GameTime gameTime)
        {
            //
        }
        public override void Die(GameTime gameTime)
        {
            //soundDestroyed.Play();
            ExplosionSound.Play();
            base.Die(gameTime);
        }
    }
}