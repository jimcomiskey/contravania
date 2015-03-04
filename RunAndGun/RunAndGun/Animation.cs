// Animation.cs
//Using declarations
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RunAndGun.Actors;

namespace RunAndGun
{
    class Animation
    {
        // The image representing the collection of images used for animation
        Texture2D spriteStrip;

        // The scale used to display the sprite strip
        float scale;

        // The time since we last updated the frame
        public int elapsedTime;

        // The time we display a frame until the next one
        int frameTime;

        // The number of frames that the animation contains
        int frameCount;

        // The index of the current frame we are displaying
        int currentFrame;

        // The color of the frame we will be displaying
        Color color;

        // The area of the image strip we want to display
        Rectangle sourceRect = new Rectangle();
        
        // The area where we want to display the image strip in the game
        Rectangle destinationRect = new Rectangle();

        // Width of a given frame
        public int FrameWidth
        { get { return spriteStrip.Width / frameCount; } }

        // Height of a given frame
        public int FrameHeight
        { get { return spriteStrip.Height; } }

        // The state of the Animation
        public bool Active;


        // Determines if the animation will keep playing or deactivate after one run
        public bool Looping;
        public bool Loopingbackandforth;
        private bool animatingforward;

        
        public Vector2 WorldPosition;
        public Stage currentStage;
        public Vector2 ScreenPosition()
        {
            return WorldPosition - currentStage.CameraPosition;
        }
        
        public void Initialize(Texture2D texture, Vector2 position,
            int frameCount, int frametime, Color color, float scale, bool looping, Stage stage)
        {
            this.color = color;
            WorldPosition = position;
            currentStage = stage; 
            spriteStrip = texture;
            Looping = looping;
            this.frameCount = frameCount;
            Active = true;
            animatingforward = true;
            elapsedTime = 0;
            currentFrame = 0;
            this.frameTime = frametime; 
        }
        public void Initialize(Texture2D texture, Vector2 position,
            int frameWidth, int frameHeight, int frameCount,
            int frametime, Color color, float scale, bool looping, bool loopingbackandforth, Stage stage)
        {
            // Keep a local copy of the values passed in
            this.color = color;
            this.frameCount = frameCount;
            this.frameTime = frametime;
            this.scale = scale;

            this.animatingforward = true;

            Loopingbackandforth = loopingbackandforth;
            Looping = looping;
            WorldPosition = position;
            currentStage = stage;
            spriteStrip = texture;


            // Set the time to zero
            elapsedTime = 0;
            currentFrame = 0;


            // Set the Animation to active by default
            Active = true;
        }

        public void Play()
        {
            elapsedTime = 0;
            currentFrame = 0;
            Active = true;
        }

        public void Update(GameTime gameTime)
        {
            
            // Update the elapsed time
            elapsedTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;


            // If the elapsed time is larger than the frame time
            // we need to switch frames
            if (elapsedTime > frameTime)
            {
                // Move to the next frame
                if (animatingforward)
                {
                    currentFrame++;
                    // If the currentFrame is equal to frameCount reset currentFrame to zero
                    if (currentFrame == frameCount)
                    {
                        if (Loopingbackandforth)
                        {
                            currentFrame = frameCount - 1;
                            animatingforward = false;
                        }
                        else
                            currentFrame = 0;

                        // If we are not looping deactivate the animation
                        if (Looping == false)
                            Active = false;
                    }
                }
                else
                {
                    currentFrame--;
                    if (currentFrame == 0)
                    {

                        if (Loopingbackandforth)
                            animatingforward = true;
                        
                        currentFrame = 0;

                        // If we are not looping deactivate the animation
                        if (Looping == false)
                            Active = false;
                    }
                }

                // Reset the elapsed time to zero
                elapsedTime = 0;
            }


            // Grab the correct frame in the image strip by multiplying the currentFrame index by the frame width
            sourceRect = new Rectangle(currentFrame * FrameWidth, 0, FrameWidth, FrameHeight);


            // Grab the correct frame in the image strip by multiplying the currentFrame index by the frame width
            //destinationRect = new Rectangle((int)Position.X - (int)(FrameWidth * scale) / 2,
            //(int)Position.Y - (int)(FrameHeight * scale) / 2,
            //(int)(FrameWidth * scale),
            //(int)(FrameHeight * scale));
            destinationRect = new Rectangle((int)ScreenPosition().X, (int)ScreenPosition().Y, FrameWidth, FrameHeight);
        }


        // Draw the Animation Strip
        public void Draw(SpriteBatch spriteBatch, Player.PlayerDirection dir, float depth)
        {
            // Only draw the animation when we are active
            if (Active)
            {
                if (dir == Player.PlayerDirection.Left)
                {
                    spriteBatch.Draw(spriteStrip, destinationRect, sourceRect, color, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, depth);                    
                }
                else
                {   
                    spriteBatch.Draw(spriteStrip, destinationRect, sourceRect, color, 0, Vector2.Zero, SpriteEffects.None, depth);
                }
            }
        }

    }
}
