using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;


namespace RunAndGun
{
    class StageObject
    {
        protected Stage currentStage;
        protected Texture2D texture;
        public bool Active;
        public Vector2 WorldPosition;
        public Vector2 ScreenPosition { get { return WorldPosition - currentStage.CameraPosition; } }
        public int Height;
        public int Width;

        public void Initialize(Texture2D p_texture, Stage p_stage, Vector2 p_worldPosition)
        {
            Active = true;
            currentStage = p_stage;            
            WorldPosition = p_worldPosition;
            if (p_texture != null)
            {
                texture = p_texture;
                Height = p_texture.Height;
                Width = p_texture.Width;
            }
        }
        

        public Rectangle VisibleRectangle()
        {

            return new Rectangle((int)WorldPosition.X, (int)WorldPosition.Y, Width, Height);
        }

        public virtual void Update(GameTime gameTime)
        {
            // do nothing by default.  inherited objects might do something
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true && texture != null)
                spriteBatch.Draw(texture, new Rectangle((int)ScreenPosition.X, (int)ScreenPosition.Y, (int)texture.Width, (int)texture.Height), Color.White);
        }
    }
}
