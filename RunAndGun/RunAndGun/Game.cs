using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using RunAndGun.Actors;

namespace RunAndGun
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        public const float iScreenModelWidth = 256f;
        public const float iScreenModelHeight = 240f;
        public const bool bDrawBoundingBox = false;
        public enum GameType { ContraVania = 1, Contra = 2}

        public enum GameState { TitleScreen, Initializing, Playing };
        public GameState CurrentGameState = GameState.TitleScreen;

        public GameType currentGame = GameType.ContraVania;
        
        GraphicsDeviceManager graphics;
        ContentManager worldContent;

        SpriteBatch spriteBatch;
        Matrix SpriteScale;

        //Player player1;
        List<Player> players;        
        Stage currentStage;        
        SoundEffect soundGamePause;        

        // The font used to display UI elements
        SpriteFont font;
        //private Texture2D titleScreen;
        private TitleScreen titleScreen;

        private bool bGamePaused;
        public bool GamePaused { get { return bGamePaused; } }
        //TextWriterTraceListener tr1 = new TextWriterTraceListener(System.IO.File.CreateText("Trace.txt"));

        public Game()
        {

            //Debug.Listeners.Add(tr1);
            
            graphics = new GraphicsDeviceManager(this);
            
            // Non-World-Specific Game Content: Player sprite, Generic sound effects, etc.
            Content.RootDirectory = "Content";

            if (!this.LaunchParameters.ContainsKey("WindowedMode"))
            {
                //graphics.IsFullScreen = true;
            }

            graphics.PreferredBackBufferWidth = 768;
            graphics.PreferredBackBufferHeight = 720;            
            this.Window.AllowUserResizing = true;

        }

        protected override void Initialize()
        {
            bGamePaused = false;            

            base.Initialize();
        }

        protected override void LoadContent()
        {

            players = new List<Player>();
            
            spriteBatch = new SpriteBatch(GraphicsDevice);

            float screenscaleWidth =
                (float)graphics.GraphicsDevice.Viewport.Width / (float)Game.iScreenModelWidth;

            float screenscaleHeight =
                (float)graphics.GraphicsDevice.Viewport.Height / (float)Game.iScreenModelHeight;
            
            // Create the scale transform for Draw. 
            // Do not scale the sprite depth (Z=1).
            //SpriteScale = Matrix.CreateScale(screenscale, screenscale, 1);
            SpriteScale = Matrix.CreateScale(screenscaleWidth, screenscaleHeight, 1);

            soundGamePause = Content.Load<SoundEffect>("Sounds/gamepause");            
            font = Content.Load<SpriteFont>("spriteFont1");

            titleScreen = new TitleScreen();
            titleScreen.Initialize(Content, font);
            

        }
        protected override void UnloadContent()
        {
            MediaPlayer.Stop();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            switch (CurrentGameState)
            {
                case GameState.TitleScreen:
                    {
                        if (titleScreen.Update(gameTime, this) == GameState.Playing)
                        {
                            CurrentGameState = GameState.Initializing;
                        }

                        break;
                    }

                case GameState.Initializing:
                    {
                        // if playing ContraVania, load world content from ContraVania folder.
                        // otherwise, assume we are playing Contra, which loads from Content folder, same as Core Game Content.
                        if (currentGame == GameType.ContraVania)
                        {
                            worldContent = new ContentManager(this.Services);
                            worldContent.RootDirectory = "ContraVania";
                        }
                        else
                        {
                            worldContent = Content;
                        }

                        currentStage = new Stage(worldContent);

                        for (int iPlayerID = 1; iPlayerID <= titleScreen.NumPlayers; iPlayerID ++)
                            players.Add(new Player(iPlayerID, this));
                        //players.Add(new Player(2, this));
                            
                        foreach (Player player in players)
                            player.Initialize(Content,
                                            new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X + ((player.ID - 1) * currentStage.iTileWidth) , GraphicsDevice.Viewport.TitleSafeArea.Y),
                                            currentStage);

                        if (currentGame == GameType.Contra)
                        {
                            currentStage.Initialize(this, worldContent, "Contra1-1Jungle", 32, 32);
                        }
                        else
                        {
                            currentStage.Initialize(this, worldContent, "Castlevania1-1-1", 16, 16);
                        }
                        currentStage.Players = players;

                        this.ResetElapsedTime();
                        CurrentGameState = GameState.Playing;
                        break;
                    }


                case GameState.Playing:
                    {

                        if (currentStage.bStageIsComplete)
                            this.Exit();

                        // as part of Player update, get total life count between the two players.
                        int iLifeCount = 0;
                        foreach (Player player in players)
                        {
                            player.Update(gameTime);
                            iLifeCount += player.LifeCount;
                        }

                        // if all players are out of lives, Game Over.
                        if (iLifeCount <= 0)
                            this.Exit();

                        if (!bGamePaused)
                        {
                            UpdateProjectiles(gameTime, currentStage.Projectiles);
                            UpdateProjectiles(gameTime, currentStage.EnemyProjectiles);

                            currentStage.Update(gameTime, players);

                        }

                        // TODO: update code so that currentStage doesn't advance until all players advance. 
                        // (already-advanced players will be inactive until game stage advances. //
                        if (players[0].currentStage.StageID != currentStage.StageID)
                            currentStage = players[0].currentStage;

                        break;
                    }
            }
            
            base.Update(gameTime);
        }

        public void TogglePause()
        {
            bGamePaused = !bGamePaused;
            if (bGamePaused)
            {
                soundGamePause.Play();
                currentStage.PauseMusic();
            }
            else
            {
                currentStage.PlayMusic();
            }
        }
        
        
        private void UpdateProjectiles(GameTime gameTime, List<Projectile> lstProjectiles)
        {
            // Update the Projectiles
            for (int i = lstProjectiles.Count - 1; i >= 0; i--)
            {

                lstProjectiles[i].Update(gameTime, currentStage.CameraPosition);

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
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, SpriteScale);

            switch(CurrentGameState)
            {
                case GameState.TitleScreen:
                    {
                        titleScreen.Draw(spriteBatch);                        
                        break;
                    }

                case GameState.Playing:
                    {
                    
                    //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                    currentStage.Draw(spriteBatch, currentStage.CameraPosition);

                    foreach (Player player in players)
                    {
                        if (player.Visible)
                            player.Draw(spriteBatch);
                    }

                    // Draw the Player Projectiles
                    for (int i = 0; i < currentStage.Projectiles.Count; i++)
                    {
                        currentStage.Projectiles[i].Draw(spriteBatch, currentStage.CameraPosition);
                    }

                    for (int i = 0; i < currentStage.EnemyProjectiles.Count; i++)
                    {
                        currentStage.EnemyProjectiles[i].Draw(spriteBatch, currentStage.CameraPosition);
                    }

                    
                    //spriteBatch.DrawString(font, players[0].IsOnStairs.ToString(), new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y ), Color.Red);
                    
                    
                    break;
                    } // GameState.Playing
            }

            //spriteBatch.DrawString(font, "gameTime: " + gameTime.ElapsedGameTime.ToString(), new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y ), Color.Red);

            spriteBatch.End();


            base.Draw(gameTime);
        }


    }
}
