using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RunAndGun.Helpers
{
    static class TextureHelper
    {
        public static Texture2D SwapColor(Texture2D thisTexture, Color searchColor, Color replaceColor)
        {
            Texture2D newTexture = new Texture2D(thisTexture.GraphicsDevice, thisTexture.Width, thisTexture.Height);

            Color[] data = new Color[thisTexture.Width * thisTexture.Height];
            thisTexture.GetData(data);

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].Equals(searchColor))
                {
                    data[i].A = replaceColor.A;
                    data[i].R = replaceColor.R;
                    data[i].G = replaceColor.G;
                    data[i].B = replaceColor.B;
                }
            }
            newTexture.SetData(data);
            return newTexture;

        }
        public static Texture2D SwapPlayerColor(Texture2D playerOneTexture, int newPlayerId)
        {
            if (newPlayerId == 1)
                // no color swap for player 1
                return playerOneTexture;
            else
            {
                Texture2D newTexture = new Texture2D(playerOneTexture.GraphicsDevice, playerOneTexture.Width, playerOneTexture.Height);

                Color[] data = new Color[playerOneTexture.Width * playerOneTexture.Height];
                playerOneTexture.GetData(data);

                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i].B > 200)
                    {
                        switch (newPlayerId)
                        {
                            case 2:
                                data[i] = new Color(255, 0, 0);
                                break;
                            case 3:
                                data[i] = new Color(255, 0, 255);
                                break;
                            case 4:
                                data[i] = new Color(255, 255, 0);
                                break;
                        }
                    }
                }
                newTexture.SetData(data);
                return newTexture;
            }
        }
    }
}
