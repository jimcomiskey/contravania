using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;


namespace RunAndGun
{
    class TitleScreen
    {

        private Texture2D titleScreen;
        private SpriteFont spriteFont;
        private Texture2D cursor;

        private GamePadState previousGamePadState;
        private GamePadState currentGamePadState;
        private KeyboardState previousKeyboardState;
        private KeyboardState currentKeyboardState;

        public int NumPlayers = 1;
        private int GameMenuPosition = 1;
        private string[] gameMenu = new string[] {"ContraVania", "Contra", "Back"};
        
        private enum TitleScreenMenuState { PlayerSelect, GameSelect };

        private TitleScreenMenuState menuState = TitleScreenMenuState.PlayerSelect;


        public void Initialize(ContentManager content, SpriteFont font)
        {
            titleScreen = content.Load<Texture2D>("Sprites/TitleScreen");
            spriteFont = content.Load<SpriteFont>("spriteFont1"); // font;
            cursor = content.Load<Texture2D>("Sprites/Cursor");
        }
        public Game.GameState Update(GameTime gameTime, Game game)
        {
            previousGamePadState = currentGamePadState;
            previousKeyboardState = currentKeyboardState;

            currentGamePadState = GamePad.GetState(PlayerIndex.One);
            currentKeyboardState = Keyboard.GetState();

            if (menuState == TitleScreenMenuState.PlayerSelect)
                {
                if (!previousGamePadState.IsButtonDown(Buttons.DPadDown) && currentGamePadState.IsButtonDown(Buttons.DPadDown))
                    NumPlayers++;
                else if (!previousKeyboardState.IsKeyDown(Keys.Down) && currentKeyboardState.IsKeyDown(Keys.Down))
                    NumPlayers++;

                if (!previousGamePadState.IsButtonDown(Buttons.DPadUp) && currentGamePadState.IsButtonDown(Buttons.DPadUp))
                    NumPlayers--;
                else if (!previousKeyboardState.IsKeyDown(Keys.Up) && currentKeyboardState.IsKeyDown(Keys.Up))
                    NumPlayers--;

                if (NumPlayers < 1)
                    NumPlayers = 4;
                else if (NumPlayers > 4)
                    NumPlayers = 1;

                if ((previousGamePadState.Buttons.Start != ButtonState.Pressed && currentGamePadState.Buttons.Start == ButtonState.Pressed) ||
                    (!previousKeyboardState.IsKeyDown(Keys.Enter) && currentKeyboardState.IsKeyDown(Keys.Enter)))
                {
                    menuState = TitleScreenMenuState.GameSelect;
                    return game.CurrentGameState;
                }
                }
            else if (menuState == TitleScreenMenuState.GameSelect)
            {
                if (!previousGamePadState.IsButtonDown(Buttons.DPadDown) && currentGamePadState.IsButtonDown(Buttons.DPadDown))
                    GameMenuPosition++; 
                else if (!previousKeyboardState.IsKeyDown(Keys.Down) && currentKeyboardState.IsKeyDown(Keys.Down))
                    GameMenuPosition++;

                if (!previousGamePadState.IsButtonDown(Buttons.DPadUp) && currentGamePadState.IsButtonDown(Buttons.DPadUp))
                    GameMenuPosition--;
                else if (!previousKeyboardState.IsKeyDown(Keys.Up) && currentKeyboardState.IsKeyDown(Keys.Up))
                    GameMenuPosition--;

                GameMenuPosition = (int)MathHelper.Clamp(GameMenuPosition, 1, gameMenu.GetUpperBound(0) + 1);

                if ((previousGamePadState.Buttons.Start != ButtonState.Pressed && currentGamePadState.Buttons.Start == ButtonState.Pressed) ||
                    (!previousKeyboardState.IsKeyDown(Keys.Enter) && currentKeyboardState.IsKeyDown(Keys.Enter)))                
                {
                    if (GameMenuPosition == gameMenu.GetUpperBound(0) + 1)
                    {
                        menuState = TitleScreenMenuState.PlayerSelect;
                    }
                    else
                    {
                        game.currentGame = (Game.GameType)GameMenuPosition;
                        game.CurrentGameState = Game.GameState.Playing;
                    }
                }
            }

            return game.CurrentGameState;
            
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            //spriteBatch.Draw(titleScreen, new Rectangle(0, 0, graphics.GraphicsDevice.Viewport.X, graphics.GraphicsDevice.Viewport.Y), Color.White);
            spriteBatch.Draw(titleScreen, new Rectangle(0, 0, 256, 256), Color.White);

            float startTextTop = Game.iScreenModelHeight * 0.5f;
            float startTextLeft = Game.iScreenModelWidth * 0.35f;
            float textLineHeight = cursor.Height * 1.1f;

            if (menuState == TitleScreenMenuState.PlayerSelect)
            {
                spriteBatch.DrawString(spriteFont, "1 Player", new Vector2(startTextLeft, startTextTop + (textLineHeight * 1)), Color.White);
                spriteBatch.DrawString(spriteFont, "2 Player", new Vector2(startTextLeft, startTextTop + (textLineHeight * 2)), Color.White);
                spriteBatch.DrawString(spriteFont, "3 Player", new Vector2(startTextLeft, startTextTop + (textLineHeight * 3)), Color.White);
                spriteBatch.DrawString(spriteFont, "4 Player", new Vector2(startTextLeft, startTextTop + (textLineHeight * 4)), Color.White);

                spriteBatch.Draw(cursor, new Rectangle((int)startTextLeft - cursor.Width - 5, (int)(startTextTop + (textLineHeight * NumPlayers) - 2), cursor.Width, cursor.Height), Color.White);
            }
            else if (menuState == TitleScreenMenuState.GameSelect)
            {
                for (int i = 0; i <= gameMenu.GetUpperBound(0); i++)
                {
                    spriteBatch.DrawString(spriteFont, gameMenu[i], new Vector2(startTextLeft, startTextTop + (textLineHeight * (i + 1))), Color.White);
                }

                spriteBatch.Draw(cursor, new Rectangle((int)startTextLeft - cursor.Width - 5, (int)(startTextTop + (textLineHeight * GameMenuPosition) - 2), cursor.Width, cursor.Height), Color.White);
            }
        }
    }
}
