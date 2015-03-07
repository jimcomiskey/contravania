// ParallaxingBackground.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Xml;
using System.IO;
using RunAndGun.Actors;

namespace RunAndGun
{
    class Stage
    {
        public Game game;
        public string StageID;
        private ContentManager worldContent;
        //private TiledSharp.TmxMap tmx;
        public int MapHeight;
        public int MapWidth;
        int iScreenTileWidth;
        //public TiledSharp.TmxMap Map
        //{
        //    get { return tmx; }
        //}
        public List<StageTile> StageTiles = new List<StageTile>();
        private List<Texture2D> tilesource = new List<Texture2D>();
        private int tileSourceCurrentFrame = 0;
        private int elapsedTime;
        private int frameTime = 180;

        //private Dictionary<string, Stage> portals;
        
        private System.Collections.Specialized.OrderedDictionary portals = new System.Collections.Specialized.OrderedDictionary();
        
        //public enum TileLayers { Display = 0, Collisions = 1 };

        //private const int iTilePlatformDropOffset = 4;
        private int iTilePlatformDropOffset
        {
            get
            {
                if (game.currentGame == Game.GameType.Contra)
                    return 4;
                else
                    return 0;
            }
        }
        public int iTileWidth = 32;
        public int iTileHeight = 32;
        public Vector2 CameraPosition;

        public List<Player> Players;
        public List<Projectile> Projectiles;
        public List<Actors.Enemy> ActiveEnemies;
        public List<Projectile> EnemyProjectiles;

        // enemies who are on the map, waiting for the player to arrive.
        // enemy is removed from this list and added to the activeEnemies collection when the enemy becomes visible on-screen.
        private const string waitingEnemiesObjectGroupName = "WaitingEnemies";
        public List<Actors.Enemy> waitingEnemies = new List<Actors.Enemy>();

        private const string specialStageBackgroundObjectsGroupName = "SpecialStageBackgroundObjects";        
        public List<StageObject> uniqueBackgroundObjects = new List<StageObject>();
        private const string specialStageForegroundObjectsGroupName = "SpecialStageForegroundObjects";
        public List<StageObject> uniqueForegroundObjects = new List<StageObject>();

        List<Animation> explosions;
        List<SoundEffectInstance> explosionsounds;

        public bool bProgresstoNextLevel;
        public bool bStageIsComplete;
        private float fMomentOfSilence = 0.5f;
        private bool bFanfarePlaying;
        
        // The music played during gameplay
        Song gameplayMusic;
        Song fanfare;
        SoundEffect redalertSound;

        // The rate at which the enemies appear
        TimeSpan enemySpawnTime;
        TimeSpan previousSpawnTime;

        private bool bAutoScroll;
        public bool AutoScroll 
        { 
            get 
            { 
                return bAutoScroll; 
            } 
            set 
            { 
                bAutoScroll = value;
                redalertSound.Play();
            } 
        }


        public Stage(ContentManager content)
        {
            CameraPosition = new Vector2(0, 0);
            worldContent = content;
            Projectiles = new List<Projectile>();
            ActiveEnemies = new List<Enemy>();
            EnemyProjectiles = new List<Projectile>();
            explosions = new List<Animation>();
            explosionsounds = new List<SoundEffectInstance>();

            bFanfarePlaying = false;
            bStageIsComplete = false;
            bProgresstoNextLevel = false;


        }

        public void Initialize(Game game, ContentManager worldcontent, string stageid, int tilewidth, int tileheight)
        {
            StageID = stageid;
            this.game = game;
            iTileHeight = tileheight;
            iTileWidth = tilewidth;
            
            worldContent = worldcontent;

            TiledSharp.TmxMap tmx;
            string appDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            if (game.currentGame == Game.GameType.Contra)
            {
                tilesource.Add(worldcontent.Load<Texture2D>("StageData/Level1Tileset"));
                tilesource.Add(worldcontent.Load<Texture2D>("StageData/Level1TilesetALT"));
                //explosionTexture = content.Load<Texture2D>("Sprites/Explosion1");
                //explosionSound = content.Load<SoundEffect>("Sounds/Explosion1");
                redalertSound = game.Content.Load<SoundEffect>("Sounds/redalert");

                gameplayMusic = worldcontent.Load<Song>("Music/Contra - Jungle Theme");
                fanfare = worldcontent.Load<Song>("Music/fanfare");

                PlayMusic();

                string tmxFile =  appDirectory + "\\LevelMaps\\" + stageid + ".tmx";
                
                if (File.Exists(tmxFile))
                {
                    Trace.TraceInformation("Tmx file located: {0}", tmxFile);
                    tmx = new TiledSharp.TmxMap(tmxFile);
                    Trace.TraceInformation("Tmx file loaded.");
                }
                else
                {
                    throw new FileNotFoundException(string.Format("Cannot load file: {0}", tmxFile));
                }
                iScreenTileWidth = 8;
                
            }
            else
            {

                switch (stageid)
                {
                    case "Castlevania1-1-1":
                        {
                            gameplayMusic = worldcontent.Load<Song>("Music/Level1VampireKiller");
                            PlayMusic();
                            tilesource.Add(worldcontent.Load<Texture2D>("StageData/Level1A"));
                            tmx = new TiledSharp.TmxMap(appDirectory + "\\LevelMaps\\" + stageid + ".tmx");
                            iScreenTileWidth = 16;

                            Stage nextSection = new Stage(worldcontent);
                            nextSection.Initialize(this.game, worldcontent, "Castlevania1-1-2", this.iTileWidth, this.iTileHeight);
                            
                            portals.Add("Castlevania1-1-2", nextSection);
                            break;
                        }
                    case "Castlevania1-1-2":
                        {
                            tilesource.Add(worldcontent.Load<Texture2D>("StageData/Level1B"));
                            tmx = new TiledSharp.TmxMap(appDirectory + "\\LevelMaps\\" + stageid + ".tmx");
                            iScreenTileWidth = 16;
                            break;
                        }
                    default:
                        {
                            throw new Exception("Unexpected level ID: " + stageid);                            
                        }
                }
            }

            // Initialize StageTiles
            InitializeStageTiles(tmx);

            if (game.currentGame == Game.GameType.Contra)
            {
                InitializeWaitingEnemies(worldcontent, tmx);
                InitializeSpecialStageObjects(worldcontent, tmx, tmx.ObjectGroups[specialStageBackgroundObjectsGroupName].Objects);
                InitializeSpecialStageObjects(worldcontent, tmx, tmx.ObjectGroups[specialStageForegroundObjectsGroupName].Objects);
            }

            // Set the time keepers to zero
            previousSpawnTime = TimeSpan.Zero;

            Random r = new Random();
            enemySpawnTime = TimeSpan.FromMilliseconds(r.Next(500, 3000));
            
            // Used to determine how fast enemy respawns
            //enemySpawnTime = TimeSpan.FromSeconds(3.0f); 
        }

        private void InitializeStageTiles(TiledSharp.TmxMap tmx)
        {
            MapHeight = tmx.Height;
            MapWidth = tmx.Width;

            for (int i = 0; i < tmx.Layers[0].Tiles.Count; i++)            
            {
                StageTile st = new StageTile();
                st.X = tmx.Layers[0].Tiles[i].X;
                st.Y = tmx.Layers[0].Tiles[i].Y;
                st.BackgroundGID = tmx.Layers["Background"].Tiles[i].Gid;
                if (game.currentGame == Game.GameType.Contra)
                {
                    st.DestructionLayer1GID = tmx.Layers["Destruction1"].Tiles[i].Gid;
                    st.DestructionLayer1GID = tmx.Layers["Destruction2"].Tiles[i].Gid;
                }
                
                st.MetaGID = tmx.Layers["Meta"].Tiles[i].Gid;
                //if (tmx.Layers["Meta"].Tiles[i].Gid > 0)
                //    // at minimum, it is a platform.
                //    st.CollisionType = StageTile.TileCollisionType.Platform;

                //TiledSharp.TmxLayerTile t = tmx.Layers["Meta"].Tiles[i];

                TiledSharp.TmxTilesetTile t = tmx.GetTmxTilesetTileByGID(tmx.Layers["Meta"].Tiles[i].Gid);

                if (t != null)
                {
                    if (t != null & t.Properties != null & t.Properties.ContainsKey("Collision"))
                    {
                        if (t.Properties["Collision"] == "PlatformHalfdrop")
                            st.CollisionType = StageTile.TileCollisionType.PlatformHalfDrop;

                        if (t.Properties["Collision"] == "Impassable")
                            st.CollisionType = StageTile.TileCollisionType.Impassable;

                        if (t.Properties["Collision"] == "Platform")
                            st.CollisionType = StageTile.TileCollisionType.Platform;

                        if (t.Properties["Collision"] == "StairsLeft")
                            st.CollisionType = StageTile.TileCollisionType.StairsLeft;
                        if (t.Properties["Collision"] == "StairsRight")
                            st.CollisionType = StageTile.TileCollisionType.StairsRight;

                        
                    }
                    if (t.Properties.ContainsKey("Collision") && t.Properties.ContainsKey("WaterTile"))
                    {
                        if (t.Properties["WaterTile"] == "Yes")
                            st.CollisionType = StageTile.TileCollisionType.PlatformWater;
                    }
                    
                }

                t = tmx.GetTmxTilesetTileByGID(tmx.Layers["Background"].Tiles[i].Gid);
                if (t != null)
                {
                    if (t.Properties.ContainsKey("Portal"))
                    {
                        t.Properties.TryGetValue("Portal", out st.PortalID);
                    }
                }

                StageTiles.Add(st);
            }
        }
        private void InitializeWaitingEnemies(ContentManager content, TiledSharp.TmxMap tmx)
        {
            foreach (TiledSharp.TmxObjectGroup.TmxObject tmxObject in tmx.ObjectGroups[waitingEnemiesObjectGroupName].Objects)
            {
                // object's Y-coordinate is oriented to the bottom of the placement of the object, and we need top.
                // so, we get the tile and look at its height and subtract that from the object's Y-coordinate to detemrine placement of object.
                float fObjectHeight = 0;

                int gid = tmxObject.Tile.Gid;
                for (int i = 0; i < tmx.Tilesets.Count; i++)
                {
                    fObjectHeight = (float)tmx.Tilesets[i].TileHeight;
                    if (gid > tmx.Tilesets[i].FirstGid)
                        break;
                }

                Enemy e = null;
                switch (tmxObject.Type)
                {
                    case "Sniper":
                        e = new Actors.Sniper(content, new Vector2((float)tmxObject.X, (float)tmxObject.Y - fObjectHeight), this, tmxObject.Type);
                        break;

                    case "Turret":
                        e = new Actors.Turret(content, new Vector2((float)tmxObject.X, (float)tmxObject.Y - fObjectHeight), this, tmxObject.Type);
                        break;

                    case "Level1BossPanel":
                        e = new Actors.Level1BossPanel(content, new Vector2((float)tmxObject.X, (float)tmxObject.Y - fObjectHeight), this, tmxObject.Type);
                        break;

                    default:
                        throw new Exception("Unexpected enemy type encountered: " + tmxObject.Type);
                }

                waitingEnemies.Add(e);
            }
        }

        private void InitializeSpecialStageObjects(ContentManager content, TiledSharp.TmxMap tmx, TiledSharp.TmxList<TiledSharp.TmxObjectGroup.TmxObject> objectgroup)
        {
            // Initialize Special Background Objects
            foreach (TiledSharp.TmxObjectGroup.TmxObject tmxObject in objectgroup)
            {
                // object's Y-coordinate is oriented to the bottom of the placement of the object, and we need top.
                // so, we get the tile and look at its height and subtract that from the object's Y-coordinate to detemrine placement of object.
                float fObjectHeight = 0;

                StageObject s = null;
                if (tmxObject.Type == "Bridge")
                {
                    StageBridge sb = new StageBridge();

                    sb.Initialize(null, this, new Vector2((float)tmxObject.X, (float)tmxObject.Y - fObjectHeight));
                    sb.InitializeBridge(content, new Point(tmxObject.X, tmxObject.Y), 4);
                    sb.Height = tmxObject.Height;
                    sb.Width = tmxObject.Width;

                    s = sb;
                    
                }
                else
                {
                    int gid = tmxObject.Tile.Gid;
                    for (int i = 0; i < StageTiles.Count; i++)
                    {
                        fObjectHeight = (float)tmx.Tilesets[i].TileHeight;
                        if (gid <= tmx.Tilesets[i].FirstGid)
                            break;
                    }

                    s = new StageObject();
                    s.Initialize(content.Load<Texture2D>(tmxObject.Type), this, new Vector2((float)tmxObject.X, (float)tmxObject.Y - fObjectHeight));
                }

                //e.Initialize(content, new Vector2((float)tmxObject.X, (float)tmxObject.Y), this, "Sniper");
                uniqueBackgroundObjects.Add(s);
            }
        }

        public void PlayMusic()
        {
            // Due to the way the MediaPlayer plays music,
            // we have to catch the exception. Music will play when the game is not tethered
            try
            {
                // Play the music
                if (MediaPlayer.State == MediaState.Paused)
                    MediaPlayer.Resume();
                else
                    MediaPlayer.Play(gameplayMusic);

                // Loop the currently playing song
                MediaPlayer.IsRepeating = true;
            }
            catch { }
        }
        public void PauseMusic()
        {
            MediaPlayer.Pause();
        }

        public StageTile getStageTileByWorldPosition(int x, int y)
        {
            int xTile = x / iTileWidth;
            int yTile = y / iTileHeight;

            for (int i = 0; i < StageTiles.Count; i++)
            {
                if (StageTiles[i].X == xTile && StageTiles[i].Y == yTile)
                    return StageTiles[i];
            }

            return null;
        }
        public StageTile getStageTileByGridPosition(int x, int y)
        {
            for (int i = 0; i < StageTiles.Count; i++)
            {
                if (StageTiles[i].X == x && StageTiles[i].Y == y)
                    return StageTiles[i];
            }

            return null;
        }
        public int getTileIDByPosition(int x, int y, string layer)
        {
            int xTile = x / iTileWidth;
            int yTile = y / iTileHeight;

            return getTileIDByGridPosition(xTile, yTile, layer);
            
        }
        public int getTileIDByGridPosition(int xTile, int yTile, string layer)
        {

            for (int i = 0; i < StageTiles.Count; i++)
            {
                if ((StageTiles[i].X == xTile) && (StageTiles[i].Y == yTile))
                {
                    if (layer == "Background")
                        return StageTiles[i].BackgroundGID;
                    else if (layer == "Meta")
                        return StageTiles[i].MetaGID;
                }
            }
            return -1;
        }

        public Rectangle getTileBoundsByGridPosition(int x, int y)
        {
            return new Rectangle(x * iTileWidth, (y * iTileHeight) + iTilePlatformDropOffset, iTileWidth, iTileHeight);
        }
        public List<Rectangle> getTilePlatformBoundsByGridPosition(int x, int y)
        {

            ///int iTileID = this.getTileIDByGridPosition(x, y, "Meta");
            //bool bHalfDrop = false;
            StageTile stageTile = null;

            stageTile = getStageTileByGridPosition(x, y);
            //for (int i = 0; i < StageTiles.Count; i++)
            //{
            //    if ((StageTiles[i].X == x) && (StageTiles[i].Y == y))
            //    {
            //        stageTile = StageTiles[i];
            //        break;
            //        //if (StageTiles[i].CollisionType == StageTile.TileCollisionType.PlatformHalfDrop)
            //        //    bHalfDrop = true;                    
            //    }
            //}

            List<Rectangle> returnValue = new List<Rectangle>();

            if (stageTile != null)
            {

                if (stageTile.CollisionType == StageTile.TileCollisionType.PlatformHalfDrop)
                {
                    returnValue.Add(new Rectangle(x * iTileWidth, (y * iTileHeight) + (iTileHeight / 2) + iTilePlatformDropOffset, iTileWidth, 1));
                }
                else if (stageTile.CollisionType == StageTile.TileCollisionType.StairsLeft)
                {
                    // stairs going up to the left
                    returnValue.Add(new Rectangle(x * iTileWidth, (y * iTileHeight) + iTilePlatformDropOffset, iTileWidth / 2, 4));
                    returnValue.Add(new Rectangle((x * iTileWidth) + (iTileWidth / 2), (y * iTileHeight) + iTilePlatformDropOffset + (iTileHeight / 2), iTileWidth / 2, 4));
                }
                else if (stageTile.CollisionType == StageTile.TileCollisionType.StairsRight)
                {
                    // stairs going up to the right
                    returnValue.Add(new Rectangle(x * iTileWidth, (y * iTileHeight) + iTilePlatformDropOffset + (iTileHeight / 2), iTileWidth / 2, 4));
                    returnValue.Add(new Rectangle((x * iTileWidth) + (iTileWidth / 2), (y * iTileHeight) + iTilePlatformDropOffset, iTileWidth / 2, 4));
                }
                else
                {
                    returnValue.Add(new Rectangle(x * iTileWidth, (y * iTileHeight) + iTilePlatformDropOffset, iTileWidth, 1));
                }
            }

            return returnValue;
        }

        public Rectangle getStairTopBoundsByGridPosition(int x, int y)
        { 
            StageTile stageTile = null;

            stageTile = this.getStageTileByGridPosition(x, y);

            Rectangle returnValue = new Rectangle(0, 0, 0, 0);

            if (stageTile != null)
            {
                if (stageTile.CollisionType == StageTile.TileCollisionType.StairsLeft)
                {
                    // stairs going up to the left
                    returnValue = new Rectangle(x * iTileWidth, (y * iTileHeight) + iTilePlatformDropOffset, iTileWidth / 2, 4); 
                }
                else if (stageTile.CollisionType == StageTile.TileCollisionType.StairsRight)
                {
                    // stairs going up to the right                    
                    returnValue  = new Rectangle((x * iTileWidth) + (iTileWidth / 2), (y * iTileHeight) + iTilePlatformDropOffset, iTileWidth / 2, 4);
                }
            }

            return returnValue;
        }


        //public bool isTilePlatform(int x, int y)
        //{
        //    for (int i = 0; i < StageTiles.Count; i++)
        //    {
        //        if ((StageTiles[i].X == x) && (StageTiles[i].Y == y))
        //        {
        //            if (StageTiles[i].CollisionType == StageTile.TileCollisionType.Platform || StageTiles[i].CollisionType == StageTile.TileCollisionType.PlatformHalfDrop || StageTiles[i].CollisionType == StageTile.TileCollisionType.PlatformWater) 
        //                return true;
        //        }
        //    }            
        //    return false; 
        //}

        //public bool isTileStairs(int x, int y)
        //{
        //    for (int i = 0; i < StageTiles.Count; i++)
        //    {
        //        if ((StageTiles[i].X == x) && (StageTiles[i].Y == y))
        //        {
        //            if (StageTiles[i].CollisionType == StageTile.TileCollisionType.StairsLeft || StageTiles[i].CollisionType == StageTile.TileCollisionType.StairsRight)
        //                return true;
        //        }
        //    }
        //    return false;
        //}
        //public bool isTileImpassible(int x, int y)
        //{
        //    for (int i = 0; i < StageTiles.Count; i++)
        //    {
        //        if ((StageTiles[i].X == x) && (StageTiles[i].Y == y))
        //        {
        //            if (StageTiles[i].CollisionType == StageTile.TileCollisionType.Impassable)
        //                return true;
        //        }
        //    }
        //    return false;
        //}

        //public bool isTileWaterPlatform(int x, int y)
        //{
        //    for (int i = 0; i < StageTiles.Count; i++)
        //    {
        //        if ((StageTiles[i].X == x) && (StageTiles[i].Y == y))
        //        {
        //            if (StageTiles[i].CollisionType == StageTile.TileCollisionType.PlatformWater)
        //                return true;
        //        }
        //    }
        //    return false; 
        //}
        public Rectangle ScreenCoordinates()
        {            
            return new Rectangle((int)CameraPosition.X, (int)CameraPosition.Y, (int)Game.iScreenModelWidth, (int)Game.iScreenModelHeight);
        }

        

        public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            //Debug.WriteLine("Stage Draw");

            for (int i = 0; i < StageTiles.Count; i++ )
            {
                StageTile t = StageTiles[i];                

                if (t.X >= ((int)cameraPosition.X / iTileWidth) && (t.X <= ((int)cameraPosition.X / iTileWidth) + iScreenTileWidth))
                {
                    int x = (((int)(t.BackgroundGID) - 1) * iTileWidth) % tilesource[0].Width;
                    int y = ((((int)(t.BackgroundGID) - 1) * iTileWidth) / tilesource[0].Width) * iTileHeight;

                    spriteBatch.Draw(tilesource[tileSourceCurrentFrame], new Rectangle((t.X * iTileWidth) - (int) cameraPosition.X, t.Y * iTileHeight, iTileHeight, iTileWidth), new Rectangle(x, y, iTileWidth, iTileHeight), Color.White);


                }
            }

            foreach(StageObject s in uniqueBackgroundObjects)
            {
                if (s.VisibleRectangle().Intersects(this.ScreenCoordinates())) 
                    s.Draw(spriteBatch);
            }

            for (int i = 0; i < ActiveEnemies.Count; i++)
                ActiveEnemies[i].Draw(spriteBatch);

            foreach (StageObject s in uniqueForegroundObjects)
            {
                if (s.VisibleRectangle().Intersects(this.ScreenCoordinates()))
                    s.Draw(spriteBatch);
            }


            // Draw the explosions
            for (int i = 0; i < explosions.Count; i++)
            {
                explosions[i].Draw(spriteBatch, Player.PlayerDirection.Right, 1f);
            }

        }

        public void Update(GameTime gameTime, List<Player> players)
        {

            elapsedTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (elapsedTime > frameTime)
            {
                tileSourceCurrentFrame++;
                elapsedTime = 0;
                if (tileSourceCurrentFrame == tilesource.Count)
                    tileSourceCurrentFrame = 0;
            }


            if (bProgresstoNextLevel)
            {

                UpdateExplosions(gameTime);
                
                UpdateEnemies(gameTime);

                if (explosions.Count == 0 && ActiveEnemies.Count == 0 && explosionsounds.Count == 0)
                {
                    if (fMomentOfSilence > 0.0f)
                    {
                        // do nothing- wait for enemies to die (they should), explosions to finish.     
                        fMomentOfSilence = MathHelper.Clamp(fMomentOfSilence - (float)gameTime.ElapsedGameTime.TotalSeconds, 0.0f, 5.0f);
                    }
                    else if (!bFanfarePlaying)
                    {
                        bFanfarePlaying = true;
                        MediaPlayer.Play(fanfare);
                        MediaPlayer.IsRepeating = false;
                    }
                    else if (MediaPlayer.State == MediaState.Stopped)
                    {
                        bStageIsComplete = true;
                    }

                        
                }
                
            }
            else
            {

                if (AutoScroll)
                {
                    if (CameraPosition.X + Game.iScreenModelWidth < iTileWidth * (MapWidth - 1))
                        CameraPosition.X += 1.2f;
                }

                SpawnEnemies(gameTime);

                UpdateEnemies(gameTime);

                UpdateExplosions(gameTime);

                
            }

            UpdateStageObjects(gameTime, uniqueBackgroundObjects);
            UpdateStageObjects(gameTime, uniqueForegroundObjects);


            HandleCollisions(gameTime, players);

        }

        private void HandleCollisions(GameTime gameTime, List<Player> players)
        {
            Rectangle rectangle1;
            Rectangle rectangle2;
            Rectangle playerBoundingBox;

            // Player Projectile vs Enemy Collision
            for (int i = 0; i < Projectiles.Count; i++)
            {
                rectangle1 = new Rectangle((int)Projectiles[i].WorldPosition.X -
                    Projectiles[i].Width() / 2, (int)Projectiles[i].WorldPosition.Y -
                    Projectiles[i].Height() / 2, Projectiles[i].Width(), Projectiles[i].Height());
                for (int j = 0; j < ActiveEnemies.Count; j++)
                {
                    // Create the rectangles we need to determine if we collided with each other
                    rectangle2 = ActiveEnemies[j].BoundingBox();

                    // Determine if the two objects collided with each other
                    if (rectangle1.Intersects(rectangle2))
                    {
                        ActiveEnemies[j].Health -= Projectiles[i].Damage;
                        if (ActiveEnemies[j].Health > 0)
                            Projectiles[i].PlayHitSound();
                        Projectiles[i].Active = false;
                    }
                }
            }

            // player Collision: enemies, portals
            foreach (Player player in players)
            {
                if (player.IsVulnerable())
                {
                    playerBoundingBox = player.BoundingBox();
                    for (int i = 0; i < EnemyProjectiles.Count; i++)
                    {
                        rectangle1 = new Rectangle((int)EnemyProjectiles[i].WorldPosition.X -
                            EnemyProjectiles[i].Width() / 2, (int)EnemyProjectiles[i].WorldPosition.Y -
                            EnemyProjectiles[i].Height() / 2, EnemyProjectiles[i].Width(), EnemyProjectiles[i].Height());
                        if (rectangle1.Intersects(playerBoundingBox))
                        {
                            player.Die(gameTime);
                            EnemyProjectiles[i].Active = false;
                        }
                    }


                    for (int i = 0; i < ActiveEnemies.Count; i++)
                    {
                        if (ActiveEnemies[i].CollisionIsHazardous)
                        {
                            rectangle1 = ActiveEnemies[i].BoundingBox();


                            if (rectangle1.Intersects(playerBoundingBox))
                            {
                                player.Die(gameTime);
                            }
                        }
                    }

                    for (int i = 0; i < StageTiles.Count; i++)
                    {
                        if (StageTiles[i].PortalID != null)
                        {
                            rectangle1 = getTileBoundsByGridPosition(StageTiles[i].X, StageTiles[i].Y);
                            if (playerBoundingBox.Intersects(rectangle1))
                            {
                                player.currentStage = (Stage)this.portals[StageTiles[i].PortalID];
                                player.WorldPosition.X = 0;
                                player.Update(gameTime);
                            }
                        }
                    }


                }
            }
        }

        public void AddEnemy(Vector2 spawnPosition)
        {
            
            //Enemies.Add(new Enemy(worldPosition,       
            if (this.game.currentGame == Game.GameType.Contra)
            {                
                Enemy e = new Actors.FootSoldier(worldContent, spawnPosition, this, "EnemyFootSoldier");
                e.WorldPosition = new Vector2(spawnPosition.X, spawnPosition.Y - e.BoundingBox().Height);
                ActiveEnemies.Add(e);
            }
            else
            {
                Enemy e = new Actors.Zombie(worldContent, spawnPosition, this, "Zombie");
                e.WorldPosition = new Vector2(spawnPosition.X, spawnPosition.Y - e.BoundingBox().Height);
                ActiveEnemies.Add(e);
            }
        }
        public void AddExplosion(Vector2 position, Animation explosion, SoundEffect explosionSound)
        {   
            //explosion.Initialize(explosionTexture, position, 36, 36, 3, 150, Color.White, 1f, false, false, this);
            explosion.WorldPosition = position;
            explosions.Add(explosion);

            SoundEffectInstance e = explosionSound.CreateInstance();
            e.Play();
            explosionsounds.Add(e);            
        }
        public void SpawnEnemies(GameTime gameTime)
        {
            // randomly spawwn foot soldiers
            
            if (gameTime.TotalGameTime - previousSpawnTime > enemySpawnTime && ActiveEnemies.Count <= 3)
            {
                previousSpawnTime = gameTime.TotalGameTime;

                float fVerticalSpawnLocation = 0;
                float fHorizontalSpawnLocation = CameraPosition.X + Game.iScreenModelWidth;
                if (CameraPosition.X + Game.iScreenModelWidth < this.MapWidth * this.iTileWidth)
                {
                    int iYTile = 0;
                    for (iYTile = 0; iYTile < MapHeight; iYTile++)
                    {

                        if (this.getStageTileByGridPosition((int)fHorizontalSpawnLocation / iTileWidth, iYTile).IsPlatform())
                        {
                            fVerticalSpawnLocation = iYTile * iTileHeight;
                            break;
                        }
                    }
                }

                Vector2 enemySpawnPosition = new Vector2(CameraPosition.X + Game.iScreenModelWidth, fVerticalSpawnLocation);

                AddEnemy(enemySpawnPosition);
                
                Random r = new Random(); 
                enemySpawnTime = TimeSpan.FromMilliseconds(r.Next(200, 1500));
            }

            for (int i = waitingEnemies.Count-1; i >= 0; i--)
            {
                Enemy e = waitingEnemies[i];
                if (e.BoundingBox().Intersects(this.ScreenCoordinates()))
                {
                    ActiveEnemies.Add(e);
                    waitingEnemies.Remove(e);
                }
            }

        }
        public void UpdateEnemies(GameTime gameTime)
        {
            // Update the Enemies
            for (int i = ActiveEnemies.Count - 1; i >= 0; i--)
            {
                ActiveEnemies[i].Update(gameTime);

                if (ActiveEnemies[i].Active == false)
                {
                    // If not active and health <= 0
                    if (ActiveEnemies[i].Health <= 0)
                    {
                        // Add an explosion
                        AddExplosion(ActiveEnemies[i].WorldPosition, ActiveEnemies[i].ExplosionAnimation, ActiveEnemies[i].ExplosionSound);
                        ActiveEnemies[i].Die(gameTime);

                        // Play the explosion sound
                        

                        //Add to the player's score
                        //score += enemies[i].Value;
                    }
                    ActiveEnemies.RemoveAt(i);                    
                }
            }
        }

        private void UpdateExplosions(GameTime gameTime)
        {
            for (int i = explosions.Count - 1; i >= 0; i--)
            {   
                explosions[i].Update(gameTime);
                if (explosions[i].Active == false)
                {
                    explosions.RemoveAt(i);
                }
            }

            for (int i = explosionsounds.Count - 1; i >= 0; i--)
            {
                if (explosionsounds[i].State == SoundState.Stopped)
                    explosionsounds.RemoveAt(i);
            }
        }
        private void UpdateStageObjects(GameTime gameTime, List<StageObject> lStageObjects)
        {
            for (int i = lStageObjects.Count - 1; i >= 0; i--)
            {
                if (lStageObjects[i].Active)
                    lStageObjects[i].Update(gameTime);
                else
                    lStageObjects.RemoveAt(i);
            }
        }

        
        public void StartComplete()
        {
            // destroy all enemies on screen
            for (int i = 0; i < this.ActiveEnemies.Count; i++)
                this.ActiveEnemies[i].Active = false;

            MediaPlayer.Stop();

            bProgresstoNextLevel = true;

            // from here, the Update method will wait until explosions are done, pause for a moment of silence of the awesomeness that just occurred, and then play the fanfare song.
            // then, the update method will wait for that song to be finished, and game will end.

        }

    }
}
