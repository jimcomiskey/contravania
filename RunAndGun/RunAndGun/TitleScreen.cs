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

        private Texture2D _titleScreen;
        private SpriteFont _spriteFont;
        private Texture2D _cursor;

        private GamePadState _previousGamePadState;
        private GamePadState _currentGamePadState;
        private KeyboardState _previousKeyboardState;
        private KeyboardState _currentKeyboardState;

        public int NumPlayers = 1;
        private int GameMenuPosition = 1;
        private string[] _gameMenu = new string[] {"ContraVania", "Contra", "Back"};
        
        private enum TitleScreenMenuState { PlayerSelect, GameSelect };

        private TitleScreenMenuState _menuState = TitleScreenMenuState.PlayerSelect;


        public void Initialize(ContentManager content, SpriteFont font)
        {
            _titleScreen = content.Load<Texture2D>("Sprites/TitleScreen");
            _spriteFont = content.Load<SpriteFont>("spriteFont1"); // font;
            _cursor = content.Load<Texture2D>("Sprites/Cursor");
        }
        public Game.GameState Update(GameTime gameTime, Game game)
        {
            _previousGamePadState = _currentGamePadState;
            _previousKeyboardState = _currentKeyboardState;

            _currentGamePadState = GamePad.GetState(PlayerIndex.One);
            _currentKeyboardState = Keyboard.GetState();

            if (_menuState == TitleScreenMenuState.PlayerSelect)
                {
                if (!_previousGamePadState.IsButtonDown(Buttons.DPadDown) && _currentGamePadState.IsButtonDown(Buttons.DPadDown))
                    NumPlayers++;
                else if (!_previousKeyboardState.IsKeyDown(Keys.Down) && _currentKeyboardState.IsKeyDown(Keys.Down))
                    NumPlayers++;

                if (!_previousGamePadState.IsButtonDown(Buttons.DPadUp) && _currentGamePadState.IsButtonDown(Buttons.DPadUp))
                    NumPlayers--;
                else if (!_previousKeyboardState.IsKeyDown(Keys.Up) && _currentKeyboardState.IsKeyDown(Keys.Up))
                    NumPlayers--;

                if (NumPlayers < 1)
                    NumPlayers = 4;
                else if (NumPlayers > 4)
                    NumPlayers = 1;

                if ((_previousGamePadState.Buttons.Start != ButtonState.Pressed && _currentGamePadState.Buttons.Start == ButtonState.Pressed) ||
                    (!_previousKeyboardState.IsKeyDown(Keys.Enter) && _currentKeyboardState.IsKeyDown(Keys.Enter)))
                {
                    _menuState = TitleScreenMenuState.GameSelect;
                    return game.CurrentGameState;
                }
                }
            else if (_menuState == TitleScreenMenuState.GameSelect)
            {
                if (!_previousGamePadState.IsButtonDown(Buttons.DPadDown) && _currentGamePadState.IsButtonDown(Buttons.DPadDown))
                    GameMenuPosition++; 
                else if (!_previousKeyboardState.IsKeyDown(Keys.Down) && _currentKeyboardState.IsKeyDown(Keys.Down))
                    GameMenuPosition++;

                if (!_previousGamePadState.IsButtonDown(Buttons.DPadUp) && _currentGamePadState.IsButtonDown(Buttons.DPadUp))
                    GameMenuPosition--;
                else if (!_previousKeyboardState.IsKeyDown(Keys.Up) && _currentKeyboardState.IsKeyDown(Keys.Up))
                    GameMenuPosition--;

                GameMenuPosition = (int)MathHelper.Clamp(GameMenuPosition, 1, _gameMenu.GetUpperBound(0) + 1);

                if ((_previousGamePadState.Buttons.Start != ButtonState.Pressed && _currentGamePadState.Buttons.Start == ButtonState.Pressed) ||
                    (!_previousKeyboardState.IsKeyDown(Keys.Enter) && _currentKeyboardState.IsKeyDown(Keys.Enter)))                
                {
                    if (GameMenuPosition == _gameMenu.GetUpperBound(0) + 1)
                    {
                        _menuState = TitleScreenMenuState.PlayerSelect;
                    }
                    else
                    {
                        game.CurrentGame = (Game.GameType)GameMenuPosition;
                        game.CurrentGameState = Game.GameState.Playing;
                    }
                }
            }

            return game.CurrentGameState;
            
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            //spriteBatch.Draw(titleScreen, new Rectangle(0, 0, graphics.GraphicsDevice.Viewport.X, graphics.GraphicsDevice.Viewport.Y), Color.White);
            spriteBatch.Draw(_titleScreen, new Rectangle(0, 0, 256, 256), Color.White);

            float startTextTop = Game.iScreenModelHeight * 0.5f;
            float startTextLeft = Game.iScreenModelWidth * 0.35f;
            float textLineHeight = _cursor.Height * 1.1f;

            if (_menuState == TitleScreenMenuState.PlayerSelect)
            {
                spriteBatch.DrawString(_spriteFont, "1 Player", new Vector2(startTextLeft, startTextTop + (textLineHeight * 1)), Color.White);
                spriteBatch.DrawString(_spriteFont, "2 Player", new Vector2(startTextLeft, startTextTop + (textLineHeight * 2)), Color.White);
                spriteBatch.DrawString(_spriteFont, "3 Player", new Vector2(startTextLeft, startTextTop + (textLineHeight * 3)), Color.White);
                spriteBatch.DrawString(_spriteFont, "4 Player", new Vector2(startTextLeft, startTextTop + (textLineHeight * 4)), Color.White);

                spriteBatch.Draw(_cursor, new Rectangle((int)startTextLeft - _cursor.Width - 5, (int)(startTextTop + (textLineHeight * NumPlayers) - 2), _cursor.Width, _cursor.Height), Color.White);
            }
            else if (_menuState == TitleScreenMenuState.GameSelect)
            {
                for (int i = 0; i <= _gameMenu.GetUpperBound(0); i++)
                {
                    spriteBatch.DrawString(_spriteFont, _gameMenu[i], new Vector2(startTextLeft, startTextTop + (textLineHeight * (i + 1))), Color.White);
                }

                spriteBatch.Draw(_cursor, new Rectangle((int)startTextLeft - _cursor.Width - 5, (int)(startTextTop + (textLineHeight * GameMenuPosition) - 2), _cursor.Width, _cursor.Height), Color.White);
            }
        }
    }
}
