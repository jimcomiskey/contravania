using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RunAndGun.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RunAndGun.Actors;
using RunAndGun.GameObjects;

namespace RunAndGun.Animations
{
    public class SpreadBulletAnimation : Animation
    {
        private List<Texture2D> _frames;
        private int _elapsedFlickerTime;
        private int _flickerTime;
        private bool _flickerDisplay;

        public SpreadBulletAnimation(List<Texture2D> initFrames, int flickerTime)
        {
            _frames = initFrames;
            frameCount = 3;
            currentFrame = 0;
            _elapsedFlickerTime = 0;
            this._flickerTime = flickerTime;
            _flickerDisplay = true;
        }
        public override void Update(CVGameTime gameTime)
        {
            // Update the elapsed time
            elapsedTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            _elapsedFlickerTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (elapsedTime > frameTime)
            {
                if (currentFrame < frameCount - 1)
                {
                    currentFrame++;                    
                }
                // Reset the elapsed time to zero
                elapsedTime = 0;
            }
            if (_elapsedFlickerTime > _flickerTime)
            {
                _flickerDisplay = !_flickerDisplay;
                _elapsedFlickerTime = 0;
            }
            
            destinationRect = new Rectangle((int)ScreenPosition().X, (int)ScreenPosition().Y, _frames[currentFrame].Width, _frames[currentFrame].Height);
        }
        public override void Draw(SpriteBatch spriteBatch, Player.PlayerDirection dir, float depth, Vector2 offset)
        {
            var frame = _frames[currentFrame];
            var sourceRect = new Rectangle(0, 0, frame.Width, frame.Height);
            var drawRect = new Rectangle((int)(destinationRect.X + offset.X), (int)(destinationRect.Y + offset.Y), destinationRect.Width, destinationRect.Height);
            // Only draw the animation when we are active
            if (Active && _flickerDisplay)
            {
                spriteBatch.Draw(_frames[currentFrame], drawRect, sourceRect, color, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, depth);                
            }
        }

    }
}
