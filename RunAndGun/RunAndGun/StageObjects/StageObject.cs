using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using RunAndGun.GameObjects;

namespace RunAndGun
{
    public class StageObject
    {
        protected Stage CurrentStage;
        protected Texture2D Texture;
        public bool Active;
        public Vector2 WorldPosition;
        public Vector2 ScreenPosition { get { return WorldPosition - CurrentStage.CameraPosition; } }
        public int Height;
        public int Width;

        public void Initialize(Texture2D p_texture, Stage p_stage, Vector2 p_worldPosition)
        {
            Active = true;
            CurrentStage = p_stage;            
            WorldPosition = p_worldPosition;
            if (p_texture != null)
            {
                Texture = p_texture;
                Height = p_texture.Height;
                Width = p_texture.Width;
            }
        }
        

        public Rectangle VisibleRectangle()
        {

            return new Rectangle((int)WorldPosition.X, (int)WorldPosition.Y, Width, Height);
        }

        public virtual void Update(CVGameTime gameTime)
        {
            // do nothing by default.  inherited objects might do something
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
#if DEBUG
            Rectangle boundingbox = this.CollisionBox().Value;
            Texture2D txt = new Texture2D(spriteBatch.GraphicsDevice, boundingbox.Width, boundingbox.Height);

            Color[] data = new Color[txt.Width * txt.Height];

            for (int i = 0; i < data.Length; i++)
            {
                data[i].B = 255;
            }

            txt.SetData(data);
            spriteBatch.Draw(txt, new Vector2(boundingbox.X - CurrentStage.CameraPosition.X, boundingbox.Y - CurrentStage.CameraPosition.Y), Color.White);
#endif 

            if (Active == true && Texture != null)
                spriteBatch.Draw(Texture, new Rectangle((int)ScreenPosition.X, (int)ScreenPosition.Y, (int)Texture.Width, (int)Texture.Height), Color.White);
        }
        public virtual Rectangle? CollisionBox()
        {
            return new Rectangle((int)WorldPosition.X, (int)WorldPosition.Y, Width, Height);
        }

        public virtual Rectangle? HurtBox()
        {
            return CollisionBox();
        }
        public virtual Rectangle? HitBox()
        {
            return CollisionBox();
        }        
    }
}
