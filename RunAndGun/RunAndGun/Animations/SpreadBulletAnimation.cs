using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RunAndGun.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RunAndGun.Actors;

namespace RunAndGun.Animations
{
    public class SpreadBulletAnimation : Animation
    {
        private List<Texture2D> frames;
        private int elapsedFlickerTime;
        private int flickerTime;
        private bool flickerDisplay;

        public SpreadBulletAnimation(List<Texture2D> initFrames, int flickerTime)
        {
            frames = initFrames;
            frameCount = 3;
            currentFrame = 0;
            elapsedFlickerTime = 0;
            this.flickerTime = flickerTime;
            flickerDisplay = true;
        }
        public override void Update(GameTime gameTime)
        {
            // Update the elapsed time
            elapsedTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            elapsedFlickerTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (elapsedTime > frameTime)
            {
                if (currentFrame < frameCount - 1)
                {
                    currentFrame++;                    
                }
                // Reset the elapsed time to zero
                elapsedTime = 0;
            }
            if (elapsedFlickerTime > flickerTime)
            {
                flickerDisplay = !flickerDisplay;
                elapsedFlickerTime = 0;
            }
            
            destinationRect = new Rectangle((int)ScreenPosition().X, (int)ScreenPosition().Y, frames[currentFrame].Width, frames[currentFrame].Height);
        }
        public override void Draw(SpriteBatch spriteBatch, Player.PlayerDirection dir, float depth, Vector2 offset)
        {
            var frame = frames[currentFrame];
            var sourceRect = new Rectangle(0, 0, frame.Width, frame.Height);
            var drawRect = new Rectangle((int)(destinationRect.X + offset.X), (int)(destinationRect.Y + offset.Y), destinationRect.Width, destinationRect.Height);
            // Only draw the animation when we are active
            if (Active && flickerDisplay)
            {
                spriteBatch.Draw(frames[currentFrame], drawRect, sourceRect, color, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, depth);                
            }
        }

    }
}
