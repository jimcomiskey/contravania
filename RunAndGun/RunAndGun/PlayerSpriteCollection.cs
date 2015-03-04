using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RunAndGun.Actors;


namespace RunAndGun
{
    class PlayerSpriteCollection
    {

        public enum PlayerSpriteTypes
        {
            Underwater = 0,
            Wading = 1,
            GunHigh = 2,
            GunStraightUp = 3,
            GunLow = 4,
            GunNeutral = 5
        }
        public enum TurretSpriteTypes
        {
            Temporary = 0
        }
        public enum EnemySniperSpriteTypes
        {
            GunNeutral = 0
        }

        // The image representing the collection of images used for animation
        Texture2D spriteStrip;

        // The number of frames that the animation contains
        int frameCount;

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

        // Width of a given frame
        public Vector2 ScreenPosition;

        public void Initialize(Texture2D texture, Vector2 position,
            int frameCount, Color color, float scale)
        {
            this.color = color;
            ScreenPosition = position;
            spriteStrip = texture;            
            this.frameCount = frameCount;
            
        }
        
        // Draw the Animation Strip
        public void Draw(SpriteBatch spriteBatch, Player.PlayerDirection dir, float depth, PlayerSpriteTypes spriteType)
        {
            int iYOffSet = 0;

            if (spriteType == PlayerSpriteTypes.Wading || spriteType == PlayerSpriteTypes.Underwater)
                iYOffSet = 8;
            else
                iYOffSet = -5;

            sourceRect = new Rectangle((int)spriteType * FrameWidth, 0, FrameWidth, FrameHeight);
            destinationRect = new Rectangle((int)ScreenPosition.X, (int)ScreenPosition.Y + iYOffSet, FrameWidth, FrameHeight);

            if (dir == Player.PlayerDirection.Left)
            {
                spriteBatch.Draw(spriteStrip, destinationRect, sourceRect, color, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, depth);
            }
            else
            {   
                spriteBatch.Draw(spriteStrip, destinationRect, sourceRect, color, 0, Vector2.Zero, SpriteEffects.None, depth);
            }            
        }
        public void Draw(SpriteBatch spriteBatch, Actors.Player.PlayerDirection dir, float depth, EnemySniperSpriteTypes spriteType)
        {
            sourceRect = new Rectangle((int)spriteType * FrameWidth, 0, FrameWidth, FrameHeight);
            destinationRect = new Rectangle((int)ScreenPosition.X, (int)ScreenPosition.Y, FrameWidth, FrameHeight);

            spriteBatch.Draw(spriteStrip, destinationRect, sourceRect, color, 0, Vector2.Zero, SpriteEffects.None, depth);
        }
        public void Draw(SpriteBatch spriteBatch, Player.PlayerDirection dir, float depth, int frameID)
        {
            sourceRect = new Rectangle((int)frameID * FrameWidth, 0, FrameWidth, FrameHeight);
            destinationRect = new Rectangle((int)ScreenPosition.X, (int)ScreenPosition.Y, FrameWidth, FrameHeight);

            spriteBatch.Draw(spriteStrip, destinationRect, sourceRect, color, 0, Vector2.Zero, SpriteEffects.None, depth);
        }


    }
}
