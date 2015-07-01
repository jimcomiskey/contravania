// ParallaxingBackground.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Xml;
using System.IO;
using RunAndGun.Actors;
using RunAndGun.GameObjects;
using RunAndGun.Animations;
using RunAndGun.StageObjects;

namespace RunAndGun
{
    public class Stage
    {
        public Game Game;
        public string StageID;
        private ContentManager _worldContent;
        //private TiledSharp.TmxMap tmx;
        public int MapHeight;
        public int MapWidth;
        private int _screenTileWidth;
        //public TiledSharp.TmxMap Map
        //{
        //    get { return tmx; }
        //}
        public List<StageTile> StageTiles = new List<StageTile>();
        private List<Texture2D> _tilesource = new List<Texture2D>();
        private int _tileSourceCurrentFrame = 0;
        private int _elapsedTime;
        private int _frameTime = 180;

        //private Dictionary<string, Stage> portals;
        
        private System.Collections.Specialized.OrderedDictionary _portals = new System.Collections.Specialized.OrderedDictionary();
        
        //public enum TileLayers { Display = 0, Collisions = 1 };

        //private const int iTilePlatformDropOffset = 4;
        private int iTilePlatformDropOffset
        {
            get
            {
                // in Contra, player is slightly inset into the platform tile.
                // in Castlevania, player is directly on top of the tile, they do not intersect with it.
                if (Game.CurrentGame == Game.GameType.Contra)
                    return 4;
                else
                    return 0;
            }
        }
        public int TileWidth = 32;
        public int TileHeight = 32;
        public Vector2 CameraPosition;

        public List<Player> Players;
        public List<Projectile> Projectiles;
        public List<Actors.Enemy> ActiveEnemies;
        public List<Projectile> EnemyProjectiles;

        // enemies who are on the map, waiting for the player to arrive.
        // enemy is removed from this list and added to the activeEnemies collection when the enemy becomes visible on-screen.
        private const string WaitingEnemiesObjectGroupName = "WaitingEnemies";
        public List<Actors.Enemy> waitingEnemies = new List<Actors.Enemy>();

        private const string SpecialStageBackgroundObjectsGroupName = "BackgroundObjects";        
        public List<StageObject> uniqueBackgroundObjects = new List<StageObject>();
        private const string SpecialStageForegroundObjectsGroupName = "ForegroundObjects";
        public List<StageObject> uniqueForegroundObjects = new List<StageObject>();

        private const string GameObjectsGroupName = "GameObjects";
        public List<StageObject> gameObjects = new List<StageObject>();

        private List<Animation> _explosions;
        private List<SoundEffectInstance> _explosionsounds;

        public bool ProgresstoNextLevel;
        public bool StageIsComplete;
        private float momentOfSilence = 0.5f; // amount of time to wait before playing stage complete tune, for dramatic effect
        private bool fanfarePlaying;
        
        // The music played during gameplay
        private Song _gameplayMusic;
        private Song _fanfare;
        private SoundEffect _redalertSound;

        // The rate at which the enemies appear
        private TimeSpan _enemySpawnTime;
        private TimeSpan _previousSpawnTime;

        private bool _autoScroll;
        public bool AutoScroll 
        { 
            get 
            { 
                return _autoScroll; 
            } 
            set 
            { 
                _autoScroll = value;
                _redalertSound.Play();
            } 
        }


        public Stage(ContentManager content)
        {
            CameraPosition = new Vector2(0, 0);
            _worldContent = content;
            Projectiles = new List<Projectile>();
            ActiveEnemies = new List<Enemy>();
            EnemyProjectiles = new List<Projectile>();
            _explosions = new List<Animation>();
            _explosionsounds = new List<SoundEffectInstance>();

            fanfarePlaying = false;
            StageIsComplete = false;
            ProgresstoNextLevel = false;
        }

        public void Initialize(Game game, ContentManager worldcontent, string stageid, int tilewidth, int tileheight)
        {
            StageID = stageid;
            this.Game = game;
            TileHeight = tileheight;
            TileWidth = tilewidth;
            
            _worldContent = worldcontent;

            TiledSharp.TmxMap tmx;
            string appDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            if (game.CurrentGame == Game.GameType.Contra)
            {
                _tilesource.Add(worldcontent.Load<Texture2D>("StageData/Level1Tileset"));
                _tilesource.Add(worldcontent.Load<Texture2D>("StageData/Level1TilesetALT"));
                //explosionTexture = content.Load<Texture2D>("Sprites/Explosion1");
                //explosionSound = content.Load<SoundEffect>("Sounds/Explosion1");
                _redalertSound = game.Content.Load<SoundEffect>("Sounds/redalert");

                _gameplayMusic = worldcontent.Load<Song>("Music/Contra - Jungle Theme");
                _fanfare = worldcontent.Load<Song>("Music/fanfare");

                PlayMusic();

                string tmxFile = appDirectory + "\\LevelMaps\\" + stageid + ".tmx";
                
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
                _screenTileWidth = 8;
                
            }
            else
            {

                switch (stageid)
                {
                    case "Castlevania1-1-1":
                        {
                            _gameplayMusic = worldcontent.Load<Song>("Music/Level1VampireKiller");
                            PlayMusic();
                            _tilesource.Add(worldcontent.Load<Texture2D>("StageData/Level1A"));
                            tmx = new TiledSharp.TmxMap(appDirectory + "\\LevelMaps\\" + stageid + ".tmx");
                            
                            _screenTileWidth = 16;

                            Stage nextSection = new Stage(worldcontent);
                            nextSection.Initialize(this.Game, worldcontent, "Castlevania1-1-2", this.TileWidth, this.TileHeight);
                            
                            _portals.Add("Castlevania1-1-2", nextSection);
                            break;
                        }
                    case "Castlevania1-1-2":
                        {
                            _tilesource.Add(worldcontent.Load<Texture2D>("StageData/Level1B"));
                            tmx = new TiledSharp.TmxMap(appDirectory + "\\LevelMaps\\" + stageid + ".tmx");
                            _screenTileWidth = 16;
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

            if (game.CurrentGame == Game.GameType.Contra)
            {
                InitializeWaitingEnemies(worldcontent, tmx);
                InitializeSpecialStageObjects(worldcontent, tmx, tmx.ObjectGroups[SpecialStageBackgroundObjectsGroupName].Objects);
                InitializeSpecialStageObjects(worldcontent, tmx, tmx.ObjectGroups[SpecialStageForegroundObjectsGroupName].Objects);                
            }

            // Set the time keepers to zero
            _previousSpawnTime = TimeSpan.Zero;

            Random r = new Random();
            _enemySpawnTime = TimeSpan.FromMilliseconds(r.Next(500, 3000));
            
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
                if (Game.CurrentGame == Game.GameType.Contra)
                {
                    st.DestructionLayer1GID = tmx.Layers["Destruction1"].Tiles[i].Gid;
                    st.DestructionLayer1GID = tmx.Layers["Destruction2"].Tiles[i].Gid;
                }
                
                st.MetaGID = tmx.Layers["Meta"].Tiles[i].Gid;
                
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

                        if (t.Properties["Collision"] == "StairsBottomLeft")
                            st.CollisionType = StageTile.TileCollisionType.StairsBottomLeft;
                        if (t.Properties["Collision"] == "StairsBottomRight")
                            st.CollisionType = StageTile.TileCollisionType.StairsBottomRight;

                        if ((st.CollisionType == StageTile.TileCollisionType.Impassable || st.CollisionType == StageTile.TileCollisionType.Platform) && 
                            st.X > 0 && st.Y > 0 && st.X + 1 < this.MapWidth)
                        {
                            if (this.getStageTileByGridPosition(st.X-1, st.Y-1).CollisionType == StageTile.TileCollisionType.StairsLeft)
                            {
                                st.CollisionType = StageTile.TileCollisionType.StairsBottomLeft;
                            }
                            if (this.getStageTileByGridPosition(st.X+1, st.Y-1).CollisionType == StageTile.TileCollisionType.StairsRight)
                            {
                                st.CollisionType = StageTile.TileCollisionType.StairsBottomRight;
                            }
                        }

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
            foreach (TiledSharp.TmxObjectGroup.TmxObject tmxObject in tmx.ObjectGroups[WaitingEnemiesObjectGroupName].Objects)
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
                var enemyLocation = new Vector2((float)tmxObject.X, (float)tmxObject.Y - fObjectHeight);
                switch (tmxObject.Type)
                {
                    case "FootSoldier":
                        e = new Actors.FootSoldier(content, enemyLocation, this, tmxObject.Type);
                        break;
                    case "Sniper":
                        e = new Actors.Sniper(content, enemyLocation, this, tmxObject.Type);
                        break;
                    case "Turret":
                        e = new Actors.Turret(content, enemyLocation, this, tmxObject.Type);
                        break;
                    case "Level1BossPanel":
                        e = new Actors.Level1BossPanel(content, enemyLocation, this, tmxObject.Type);
                        break;
                    case "Capsule":
                        e = new Actors.Capsule(content, enemyLocation, this, tmxObject.Type);
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
                switch(tmxObject.Type)
                {
                    case "Bridge":
                    case "EnemySpawnLocation":
                        switch (tmxObject.Type)
                        {
                            case "Bridge":
                                s = new StageBridge();
                                break;
                            case "EnemySpawnLocation":
                                s = new EnemySpawnLocation("EnemyFootSoldier");
                                break;
                            default:
                                throw new InvalidDataException(string.Format("Unexpected background object type: {0}", tmxObject.Type));
                        }

                        s.Initialize(null, this, new Vector2((float)tmxObject.X, (float)tmxObject.Y - fObjectHeight));
                        if (s is StageBridge)
                        {
                            ((StageBridge)s).InitializeBridge(content, new Point(tmxObject.X, tmxObject.Y), 4);
                        }
                        s.Height = tmxObject.Height;
                        s.Width = tmxObject.Width;
                        break;

                    default:
                        int gid = tmxObject.Tile.Gid;
                        for (int i = 0; i < StageTiles.Count; i++)
                        {
                            fObjectHeight = (float)tmx.Tilesets[i].TileHeight;
                            if (gid <= tmx.Tilesets[i].FirstGid)
                                break;
                        }

                        s = new StageObject();
                        s.Initialize(content.Load<Texture2D>(tmxObject.Type), this, new Vector2((float)tmxObject.X, (float)tmxObject.Y - fObjectHeight));
                        break;
                }

                //e.Initialize(content, new Vector2((float)tmxObject.X, (float)tmxObject.Y), this, "Sniper");
                uniqueBackgroundObjects.Add(s);
            }
        }

        public void PlayMusic()
        {
            if (!Game.LaunchParameters.ContainsKey("DoNotPlayMusic"))
            {
                // Due to the way the MediaPlayer plays music,
                // we have to catch the exception. Music will play when the game is not tethered
                try
                {
                    // Play the music
                    if (MediaPlayer.State == MediaState.Paused)
                        MediaPlayer.Resume();
                    else
                        MediaPlayer.Play(_gameplayMusic);

                    // Loop the currently playing song
                    MediaPlayer.IsRepeating = true;
                }
                catch { }
            }
        }
        public void PauseMusic()
        {
            MediaPlayer.Pause();
        }

        public StageTile getStageTileByWorldPosition(int x, int y)
        {
            int xTile = x / TileWidth;
            int yTile = y / TileHeight;

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
            int xTile = x / TileWidth;
            int yTile = y / TileHeight;

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
            return new Rectangle(x * TileWidth, (y * TileHeight) + iTilePlatformDropOffset, TileWidth, TileHeight);
        }
        public Rectangle getTileBoundsByWorldPosition(int x, int y)
        {
            return new Rectangle((x / TileWidth) * TileWidth, (y / TileHeight) * TileHeight, TileWidth, TileHeight);
        }
        public List<Platform> getTilePlatformBoundsByGridPosition(int x, int y)
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

            List<Platform> returnValue = new List<Platform>();

            if (stageTile != null)
            {

                if (stageTile.CollisionType == StageTile.TileCollisionType.PlatformHalfDrop)
                {
                    returnValue.Add(
                        new Platform(
                            new Rectangle(x * TileWidth, 
                            (y * TileHeight) + (TileHeight / 2) + iTilePlatformDropOffset, 
                            TileWidth, 
                            1), 
                            Platform.PlatformTypes.Normal));
                }
                else if (stageTile.CollisionType == StageTile.TileCollisionType.StairsBottomLeft)
                {
                    returnValue.Add(
                        new Platform(
                            new Rectangle(
                                x * TileWidth,
                                (y * TileHeight) + iTilePlatformDropOffset,
                                TileWidth / 2,
                                4), 
                            Platform.PlatformTypes.StairsBottom)
                            );

                    returnValue.Add(new Platform(new Rectangle(
                        x * TileWidth,
                        (y * TileHeight) + iTilePlatformDropOffset,
                        TileWidth, 1), Platform.PlatformTypes.Normal));
                }
                else if (stageTile.CollisionType == StageTile.TileCollisionType.StairsBottomRight)
                {
                    returnValue.Add(
                        new Platform(
                        new Rectangle(
                        (x * TileWidth) + (TileWidth / 2),
                        (y * TileHeight) + iTilePlatformDropOffset,
                        TileWidth / 2,
                        4), Platform.PlatformTypes.StairsBottom));

                    returnValue.Add(new Platform(new Rectangle(
                        x * TileWidth,
                        (y * TileHeight) + iTilePlatformDropOffset,
                        TileWidth, 1), Platform.PlatformTypes.Normal));

                }
                else if (stageTile.CollisionType == StageTile.TileCollisionType.StairsLeft)
                {
                    // stairs going up to the left
                    // upper left
                    returnValue.Add(new Platform(new Rectangle(
                        x * TileWidth, 
                        (y * TileHeight) + iTilePlatformDropOffset, 
                        TileWidth / 2, 
                        4), Platform.PlatformTypes.Stairs));
                    // bottom right
                    returnValue.Add(new Platform(new Rectangle(
                        (x * TileWidth) + (TileWidth / 2), 
                        (y * TileHeight) + iTilePlatformDropOffset + (TileHeight / 2), 
                        TileWidth / 2, 
                        4), Platform.PlatformTypes.Stairs));
                }
                else if (stageTile.CollisionType == StageTile.TileCollisionType.StairsRight)
                {
                    // stairs going up to the right
                    // bottom left
                    returnValue.Add(new Platform(new Rectangle(
                        x * TileWidth, 
                        (y * TileHeight) + iTilePlatformDropOffset + (TileHeight / 2), 
                        TileWidth / 2, 
                        4), Platform.PlatformTypes.Stairs));
                    // upper right
                    returnValue.Add(new Platform(new Rectangle(
                        (x * TileWidth) + (TileWidth / 2), 
                        (y * TileHeight) + iTilePlatformDropOffset, 
                        TileWidth / 2, 
                        4), Platform.PlatformTypes.Stairs));
                }
                else
                {
                    returnValue.Add(new Platform(new Rectangle(
                        x * TileWidth, 
                        (y * TileHeight) + iTilePlatformDropOffset, 
                        TileWidth, 1), Platform.PlatformTypes.Normal));
                }
            }

            return returnValue;
        }

        public Rectangle? getStairTopBoundsByGridPosition(int x, int y)
        { 
            StageTile stageTile = null;

            stageTile = this.getStageTileByGridPosition(x, y);

            Rectangle? returnValue = null;

            if (stageTile != null)
            {
                if (stageTile.CollisionType == StageTile.TileCollisionType.StairsLeft)
                {
                    // stairs going up to the left
                    returnValue = new Rectangle(x * TileWidth, (y * TileHeight) + iTilePlatformDropOffset, TileWidth / 2, 4); 
                }
                else if (stageTile.CollisionType == StageTile.TileCollisionType.StairsRight)
                {
                    // stairs going up to the right                    
                    returnValue  = new Rectangle((x * TileWidth) + (TileWidth / 2), (y * TileHeight) + iTilePlatformDropOffset, TileWidth / 2, 4);
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

                if (t.X >= ((int)cameraPosition.X / TileWidth) && (t.X <= ((int)cameraPosition.X / TileWidth) + _screenTileWidth))
                {
                    int x = (((int)(t.BackgroundGID) - 1) * TileWidth) % _tilesource[0].Width;
                    int y = ((((int)(t.BackgroundGID) - 1) * TileWidth) / _tilesource[0].Width) * TileHeight;

                    spriteBatch.Draw(_tilesource[_tileSourceCurrentFrame], new Rectangle((t.X * TileWidth) - (int) cameraPosition.X, t.Y * TileHeight, TileHeight, TileWidth), new Rectangle(x, y, TileWidth, TileHeight), Color.White);


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
            for (int i = 0; i < _explosions.Count; i++)
            {
                _explosions[i].Draw(spriteBatch, Player.PlayerDirection.Right, 1f);
            }

        }

        public void Update(GameTime gameTime, List<Player> players)
        {

            _elapsedTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_elapsedTime > _frameTime)
            {
                _tileSourceCurrentFrame++;
                _elapsedTime = 0;
                if (_tileSourceCurrentFrame == _tilesource.Count)
                    _tileSourceCurrentFrame = 0;
            }


            if (ProgresstoNextLevel)
            {

                UpdateExplosions(gameTime);
                
                UpdateEnemies(gameTime);

                if (_explosions.Count == 0 && ActiveEnemies.Count == 0 && _explosionsounds.Count == 0)
                {
                    if (momentOfSilence > 0.0f)
                    {
                        // do nothing- wait for enemies to die (they should), explosions to finish.     
                        momentOfSilence = MathHelper.Clamp(momentOfSilence - (float)gameTime.ElapsedGameTime.TotalSeconds, 0.0f, 5.0f);
                    }
                    else if (!fanfarePlaying)
                    {
                        fanfarePlaying = true;
                        MediaPlayer.Play(_fanfare);
                        MediaPlayer.IsRepeating = false;
                    }
                    else if (MediaPlayer.State == MediaState.Stopped)
                    {
                        StageIsComplete = true;
                    }

                        
                }
                
            }
            else
            {

                if (AutoScroll)
                {
                    if (CameraPosition.X + Game.iScreenModelWidth < TileWidth * (MapWidth - 1))
                        CameraPosition.X += 1.2f;
                }

                if (!Game.LaunchParameters.ContainsKey("DoNotSpawnEnemies"))
                {
                    SpawnEnemies(gameTime);
                }

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
                foreach (var enemy in ActiveEnemies.Where(e => e.VulnerableToBullets))
                //for (int j = 0; j < ActiveEnemies.Count; j++)
                {

                    // Create the rectangles we need to determine if we collided with each other
                    rectangle2 = enemy.BoundingBox();

                    // Determine if the two objects collided with each other
                    if (rectangle1.Intersects(rectangle2))
                    {
                        enemy.Health -= Projectiles[i].Damage;
                        if (enemy.Health > 0)
                            Projectiles[i].PlayHitSound();
                        Projectiles[i].Active = false;
                    }
                }
            }

            // player Collision: enemies, portals
            foreach (Player player in players.Where(p => p.IsVulnerable()))
            {
                //playerBoundingBox = player.BoundingBox();
                playerBoundingBox = player.HurtBox();
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
                    if (ActiveEnemies[i].CollisionIsHazardous || ActiveEnemies[i].GetType() == typeof(PlayerItem))
                    {
                        rectangle1 = ActiveEnemies[i].BoundingBox();

                        if (rectangle1.Intersects(playerBoundingBox) && ActiveEnemies[i].CollisionIsHazardous)
                        {
                            player.Die(gameTime);
                        }
                        if (rectangle1.Intersects(playerBoundingBox) && ActiveEnemies[i].GetType() == typeof(PlayerItem))
                        {
                            // if item acquired is a Gun, equip it to the player.
                            if (((PlayerItem)ActiveEnemies[i]).Gun != null)
                            {
                                player.Gun = ((PlayerItem)ActiveEnemies[i]).Gun;
                            }
                            ActiveEnemies[i].Die(gameTime);
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
                            player.currentStage = (Stage)this._portals[StageTiles[i].PortalID];
                            player.WorldPosition.X = 0;
                            player.Update(gameTime);
                        }
                    }
                }
            }
        }

        public void AddEnemy(string enemyType, Vector2 spawnPosition)
        {
            Enemy e;
            //Enemies.Add(new Enemy(worldPosition,       
            switch (enemyType)
            {
                case "EnemyFootSoldier":
                    e = new Actors.FootSoldier(_worldContent, spawnPosition, this, "EnemyFootSoldier");
                    e.WorldPosition = new Vector2(spawnPosition.X, spawnPosition.Y - e.BoundingBox().Height);
                    ActiveEnemies.Add(e);
                    break;
                case "Zombie":
                    e = new Actors.Zombie(_worldContent, spawnPosition, this, "Zombie");
                    e.WorldPosition = new Vector2(spawnPosition.X, spawnPosition.Y - e.BoundingBox().Height);
                    ActiveEnemies.Add(e);
                    break;
            }            
        }
        public void AddExplosion(Vector2 position, Animation explosion, SoundEffect explosionSound)
        {   
            //explosion.Initialize(explosionTexture, position, 36, 36, 3, 150, Color.White, 1f, false, false, this);
            explosion.WorldPosition = position;
            _explosions.Add(explosion);

            SoundEffectInstance e = explosionSound.CreateInstance();
            e.Play();
            _explosionsounds.Add(e);            
        }
        public void SpawnEnemies(GameTime gameTime)
        {
            if (!Game.LaunchParameters.ContainsKey("DoNotSpawnRandomEnemies"))
            {
                // randomly spawwn foot soldiers

                if (gameTime.TotalGameTime - _previousSpawnTime > _enemySpawnTime && ActiveEnemies.Count <= 3)
                {
                    _previousSpawnTime = gameTime.TotalGameTime;

                    float fVerticalSpawnLocation = 0;
                    float fHorizontalSpawnLocation = CameraPosition.X + Game.iScreenModelWidth;
                    if (CameraPosition.X + Game.iScreenModelWidth < this.MapWidth * this.TileWidth)
                    {
                        int iYTile = 0;
                        for (iYTile = 0; iYTile < MapHeight; iYTile++)
                        {

                            if (this.getStageTileByGridPosition((int)fHorizontalSpawnLocation / TileWidth, iYTile).IsPlatform())
                            {
                                fVerticalSpawnLocation = iYTile * TileHeight;
                                break;
                            }
                        }
                    }

                    Vector2 enemySpawnPosition = new Vector2(CameraPosition.X + Game.iScreenModelWidth, fVerticalSpawnLocation);

                    if (this.Game.CurrentGame == Game.GameType.Contra)
                    {
                        AddEnemy("EnemyFootSoldier", enemySpawnPosition);
                    }
                    else
                    {
                        AddEnemy("Zombie", enemySpawnPosition);
                    }
                    

                    Random r = new Random();
                    _enemySpawnTime = TimeSpan.FromMilliseconds(r.Next(200, 1500));
                }
            }

            for (int i = waitingEnemies.Count-1; i >= 0; i--)
            {
                Enemy e = waitingEnemies[i];
                if (e.SpawnConditionsMet())
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
            for (int i = _explosions.Count - 1; i >= 0; i--)
            {   
                _explosions[i].Update(gameTime);
                if (_explosions[i].Active == false)
                {
                    _explosions.RemoveAt(i);
                }
            }

            for (int i = _explosionsounds.Count - 1; i >= 0; i--)
            {
                if (_explosionsounds[i].State == SoundState.Stopped)
                    _explosionsounds.RemoveAt(i);
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

            ProgresstoNextLevel = true;

            // from here, the Update method will wait until explosions are done, pause for a moment of silence of the awesomeness that just occurred, and then play the fanfare song.
            // then, the update method will wait for that song to be finished, and game will end.

        }

    }
}
