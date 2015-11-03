using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using RunAndGun.Actors;
using RunAndGun.GameObjects;
using SharpDX.DirectInput;

namespace RunAndGun
{
    // Command Line Arguments:
    // 
    // [/StartupGame:TitleScreen||Contra||ContraVania]
    // [/PlayerStartingPosition:[X]
    // WindowedMode
    // StartupStage
    // DoNotSpawnEnemies - when setting is present, do not spawn enemies
    // DoNotSpawnRandomEnemies
    // 

    public class Game : Microsoft.Xna.Framework.Game
    {
        public const float iScreenModelWidth = 256f;
        public const float iScreenModelHeight = 240f;
        public const bool bDrawBoundingBox = false;
        public enum GameType { ContraVania = 1, Contra = 2}

        public enum GameState { TitleScreen, Initializing, Playing };
        public GameState CurrentGameState;

        public GameType CurrentGame;
        
        private GraphicsDeviceManager _graphics;
        private ContentManager _worldContent;

        private SpriteBatch _spriteBatch;
        private Matrix _spriteScale;

        //Player player1;
        private List<Player> _players;        
        private Stage _currentStage;        
        private SoundEffect _soundGamePause;        

        // The font used to display UI elements
        private SpriteFont _font;
        //private Texture2D titleScreen;
        private TitleScreen _titleScreen;

        private bool gamePaused;
        public bool GamePaused { get { return gamePaused; } }
        //TextWriterTraceListener tr1 = new TextWriterTraceListener(System.IO.File.CreateText("Trace.txt"));

        public Game()
        {
            CurrentGameState = GameState.TitleScreen;
                        
            _graphics = new GraphicsDeviceManager(this);
            
            // Non-World-Specific Game Content: Player sprite, Generic sound effects, etc.
            Content.RootDirectory = "Content";

            _graphics.PreferredBackBufferWidth = 768;
            _graphics.PreferredBackBufferHeight = 720;
            Window.AllowUserResizing = true;

        }

        protected override void Initialize()
        {
            gamePaused = false;

            InitializeGameLaunchParameters();
            
            base.Initialize();
        }

        private void InitializeGameLaunchParameters()
        {
            if (this.LaunchParameters.ContainsKey("StartupGame") && this.LaunchParameters["StartupGame"] != "TitleScreen")
            {
                string startupGame = this.LaunchParameters["StartupGame"];
                if (startupGame == "Contra")
                {
                    CurrentGame = GameType.Contra;
                }
                else if (startupGame == "ContraVania")
                {
                    CurrentGame = GameType.ContraVania;
                }
                else
                {
                    throw new System.Exception(string.Format("Unexpected StartupGame Launch Parameter: {0}", startupGame));
                }
                CurrentGameState = GameState.Initializing;
            }

            if (!LaunchParameters.ContainsKey("WindowedMode"))
            {
                //graphics.IsFullScreen = true;
            }
        }

        protected override void LoadContent()
        {

            _players = new List<Player>();
            
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            float screenscaleWidth =
                (float)_graphics.GraphicsDevice.Viewport.Width / (float)Game.iScreenModelWidth;

            float screenscaleHeight =
                (float)_graphics.GraphicsDevice.Viewport.Height / (float)Game.iScreenModelHeight;
            
            // Create the scale transform for Draw. 
            // Do not scale the sprite depth (Z=1).
            //SpriteScale = Matrix.CreateScale(screenscale, screenscale, 1);
            _spriteScale = Matrix.CreateScale(screenscaleWidth, screenscaleHeight, 1);

            _soundGamePause = Content.Load<SoundEffect>("Sounds/gamepause");            
            _font = Content.Load<SpriteFont>("spriteFont1");

            _titleScreen = new TitleScreen();
            _titleScreen.Initialize(Content, _font);
            

        }
        protected override void UnloadContent()
        {
            MediaPlayer.Stop();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            switch (CurrentGameState)
            {
                case GameState.TitleScreen:
                    {
                        if (_titleScreen.Update(gameTime, this) == GameState.Playing)
                        {
                            CurrentGameState = GameState.Initializing;
                        }

                        break;
                    }

                case GameState.Initializing:
                    {
                        // if playing ContraVania, load world content from ContraVania folder.
                        // otherwise, assume we are playing Contra, which loads from Content folder, same as Core Game Content.
                        if (CurrentGame == GameType.ContraVania)
                        {
                            _worldContent = new ContentManager(this.Services);
                            _worldContent.RootDirectory = "ContraVania";
                        }
                        else
                        {
                            _worldContent = Content;
                        }

                        var directInput = new DirectInput();

                        IList<DeviceInstance> devices = null;

                        if (App.Default.InputType == "USB Gamepad")
                        {
                            // TODO: only acquire devices if setting is specified in the application.
                            devices = directInput.GetDevices(DeviceType.Joystick,
                                DeviceEnumerationFlags.AllDevices);
                        }

                        _currentStage = new Stage(_worldContent);

                        for (int iPlayerID = 1; iPlayerID <= _titleScreen.NumPlayers; iPlayerID++)
                        {
                            var newPlayer = new Player(iPlayerID, this);
                            _players.Add(newPlayer);
                            
                            if (devices.Count >= iPlayerID)
                            {
                                newPlayer.InitializeJoystick(devices[iPlayerID-1].InstanceGuid, directInput);
                            }

                        }
                        //players.Add(new Player(2, this));

                        int playerStartingPosition = 0;
                        if (this.LaunchParameters.ContainsKey("PlayerStartingPosition"))
                        {
                            playerStartingPosition = int.Parse(this.LaunchParameters["PlayerStartingPosition"]);
                        }

                        string initialStage;
                        if (this.LaunchParameters.ContainsKey("StartupStage"))
                        {
                            initialStage = this.LaunchParameters["StartupStage"];
                        }
                        else
                        {
                            if (CurrentGame == GameType.Contra)
                            {
                                initialStage = "Contra1-1Jungle";
                            }
                            else
                            {
                                initialStage = "Castlevania1-1-1";
                            }
                        }

                        foreach (Player player in _players)
                            player.Initialize(Content,
                                            new Vector2(
                                            GraphicsDevice.Viewport.TitleSafeArea.X + ((player.ID - 1) * _currentStage.TileWidth) + playerStartingPosition, 
                                            GraphicsDevice.Viewport.TitleSafeArea.Y),
                                            _currentStage);

                        
                        if (CurrentGame == GameType.Contra)
                        {

                            _currentStage.Initialize(this, _worldContent, initialStage, 32, 32);
                        }
                        else
                        {
                            _currentStage.Initialize(this, _worldContent, initialStage, 16, 16);
                        }
                        _currentStage.Players = _players;

                        this.ResetElapsedTime();
                        CurrentGameState = GameState.Playing;
                        break;
                    }


                case GameState.Playing:
                    {

                        if (_currentStage.StageIsComplete)
                            this.Exit();

                        // as part of Player update, get total life count between the two players.
                        int iLifeCount = 0;
                        foreach (Player player in _players)
                        {
                            player.Update(gameTime);
                            iLifeCount += player.LifeCount;
                        }

                        // if all players are out of lives, Game Over.
                        if (iLifeCount <= 0)
                            this.Exit();

                        if (!gamePaused)
                        {
                            UpdateProjectiles(gameTime, _currentStage.Projectiles);
                            UpdateProjectiles(gameTime, _currentStage.EnemyProjectiles);

                            _currentStage.Update(gameTime, _players);

                        }

                        // TODO: update code so that currentStage doesn't advance until all players advance. 
                        // (already-advanced players will be inactive until game stage advances. //
                        if (_players[0].CurrentStage.StageID != _currentStage.StageID)
                            _currentStage = _players[0].CurrentStage;

                        break;
                    }
            }
            
            base.Update(gameTime);
        }

        public void TogglePause()
        {
            gamePaused = !gamePaused;
            if (gamePaused)
            {
                _soundGamePause.Play();
                _currentStage.PauseMusic();
            }
            else
            {
                _currentStage.PlayMusic();
            }
        }
        
        
        private void UpdateProjectiles(GameTime gameTime, List<Projectile> lstProjectiles)
        {
            // Update the Projectiles
            for (int i = lstProjectiles.Count - 1; i >= 0; i--)
            {

                lstProjectiles[i].Update(gameTime, _currentStage.CameraPosition);

                if (lstProjectiles[i].Active == false)
                {
                    lstProjectiles.RemoveAt(i);
                }
            }
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _spriteScale);

            switch(CurrentGameState)
            {
                case GameState.TitleScreen:
                    {
                        _titleScreen.Draw(_spriteBatch);                        
                        break;
                    }

                case GameState.Playing:
                    {
                    
                    //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                    _currentStage.Draw(_spriteBatch, _currentStage.CameraPosition);

                    foreach (Player player in _players)
                    {
                        if (player.Visible)
                            player.Draw(_spriteBatch);
                    }

                    // Draw the Player Projectiles
                    for (int i = 0; i < _currentStage.Projectiles.Count; i++)
                    {
                        _currentStage.Projectiles[i].Draw(_spriteBatch, _currentStage.CameraPosition);
                    }

                    for (int i = 0; i < _currentStage.EnemyProjectiles.Count; i++)
                    {
                        _currentStage.EnemyProjectiles[i].Draw(_spriteBatch, _currentStage.CameraPosition);
                    }


                        //spriteBatch.DrawString(font, players[0].IsOnStairs.ToString(), new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y ), Color.Red);
                        if (this.LaunchParameters.ContainsKey("DisplayDebugInfo"))
                        {
                            _spriteBatch.DrawString(_font, _players[0].WorldPosition.X.ToString(), new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y), Color.Red);
                            //spriteBatch.DrawString(font, players[0].WorldPosition.Y.ToString(), new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + 20), Color.Red);
                            _spriteBatch.DrawString(_font, (_players[0].BoundingBox().Bottom / _currentStage.TileHeight).ToString(), new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + 20), Color.Red);

                            _spriteBatch.DrawString(_font, string.Format("IsOnGround: {0}", _players[0].IsOnGround), new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + 40), Color.Blue);
                            //spriteBatch.DrawString(font, players[0].IsOnStairsRight.ToString(), new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + 60), Color.Red);
                        }
                        
                        break;
                    } // GameState.Playing
            }

            //spriteBatch.DrawString(font, "gameTime: " + gameTime.ElapsedGameTime.ToString(), new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y ), Color.Red);

            _spriteBatch.End();


            base.Draw(gameTime);
        }


    }
}
