using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Xml;


namespace RunAndGun.Actors
{
    class Player : Actors.Actor
    {

        public int ID;
        private Game game;
#region "Declarations"

        public enum PlayerDirection { Left = -1, Right = 1};
        public PlayerDirection playerDirection;
        public enum GunDirection { StraightUp, High, Low, Neutral, StraightDown };
        public GunDirection gunDirection;
#endregion

        // for drawing prone sprite, and offsetting origin point of bullets
        const int iProneVerticalOffset = 16;

        #region Content Objects

        private Animation idle;
        private Animation idlelegs;
        private Animation dropping;
        private Animation prone;
        private Animation deathAnimation;        
        private Animation runningTorsoAnimation;
        private Animation runningLegsAnimation;
        private Animation jumpingAnimation;

        private SoundEffect soundPlayerLand;
        private SoundEffect soundGunshot;
        private SoundEffect soundDeath;
        private PlayerSpriteCollection playermiscsprites;        

        Texture2D projectileTexture;
        private SoundEffect soundProjectileHit;
        #endregion

        
        private bool DropInProgress;        
        private bool IsProne;   // is player ducking

        private bool IsInWater; // set to true when user is on a water platform (wading, jungle stage 1)
        public bool IsOnStairsLeft;
        public bool IsOnStairsRight;
        public bool IsOnStairs { get { if (IsOnStairsLeft || IsOnStairsRight) return true; else return false; } }
        private bool IgnoreNextPlatform;

        
        // Amount of lives the player has, including the current one.
        public int LifeCount;
        private float SpawnTimeRemaining;
        private const float SpawnTime = 3.0f;
        private int spawnAnimationElapsedTime; // control flicker of player while spawning.
        public bool Visible;

        public InputState CurrentInputState;
        public InputState PreviousInputState;

        // Keyboard states used to determine key presses
        //public KeyboardState currentKeyboardState;
        //public KeyboardState previousKeyboardState;
        
        // Gamepad states used to determine button presses
        //public GamePadState currentGamePadState;
        //private GamePadState previousGamePadState;

        //public float movement;

        public bool MovingUpStairs;
        public bool MovingDownStairs;
        private float lastStairtopHeight;     // when moving up or down stairs, keep track of 
        private const float StairHeight = 8f; // to the player, stairs are 8 pixels in height.
        public bool SecuringStairStep;

        public bool MovingTowardsStairs;
        public int MovingTowardsStairsPosition;
        public bool MovingTowardsStairsAscending;            

        public override int Width
        {
            get { return idle.FrameWidth; }
        }
        
        public Player(int playerID, Game game)
        {
            //PlayerMoveSpeed = 1.5f;
            //MaxGroundVelocity = 1f;

            runningTorsoAnimation = new Animation();
            runningLegsAnimation = new Animation();
            jumpingAnimation = new Animation();
            playermiscsprites = new PlayerSpriteCollection();
            idle = new Animation();
            idlelegs = new Animation();
            dropping = new Animation();
            prone = new Animation();
            deathAnimation = new Animation();
            
            this.game = game;
            Name = "Player" + playerID.ToString();
            ID = playerID;
            SpawnTimeRemaining = SpawnTime;
            JumpInProgress = true;
            MaxGroundVelocity = 1.0f;

            // X, Y - Y velocity is 0.0 by default, and increases/decreases depending on if player is jumping or falling.
            Velocity = new Vector2(0.0f, 0.0f);
        }
        public bool IsVulnerable()
        {
            if (SpawnTimeRemaining > 0 || IsDying)
                return false;
            else
                return true;
        }
        
        // when player stage changes, move the animations along with him!
        public override Stage currentStage
        {
            get
            {
                return base.currentStage;
            }
            set
            {
                base.currentStage = value;

                runningTorsoAnimation.currentStage = currentStage;
                runningLegsAnimation.currentStage = currentStage;                
                jumpingAnimation.currentStage = currentStage;                
                idle.currentStage = currentStage;                
                idlelegs.currentStage = currentStage;                
                prone.currentStage = currentStage;                
                dropping.currentStage = currentStage;
                deathAnimation.currentStage = currentStage;

            }
        }

        // Initialize the player
        public void Initialize(ContentManager content, Vector2 position, Stage stage)
        {

            // Set the player life count
            LifeCount = 30;

            currentStage = stage; 

            //running = animation;
            
            //Status = PlayerStatus.Idle;
            playerDirection = PlayerDirection.Right;
            
            // 32wx32h sprite, 6 frames of animation, 350 frame timee (miliseconds?)
            runningTorsoAnimation.Initialize(swapColor(content.Load<Texture2D>("Sprites/billrizerrunningtorso")), Vector2.Zero, 32, 32, 6, 150, Color.White, 1f, true, false, currentStage);
            // 32wx24h sprite, 3 frames of animation, , 350 frame timee (miliseconds?)
            runningLegsAnimation.Initialize(swapColor(content.Load<Texture2D>("Sprites/billrizerrunninglegs")), Vector2.Zero, 32, 24, 3, 150, Color.White, 1f, true, false, currentStage);
            // 32x32 sprite, 4 frames animation, 350 frame time (miliseconds?)
            jumpingAnimation.Initialize(swapColor(content.Load<Texture2D>("Sprites/billrizerjumping")), Vector2.Zero, 32, 32, 4, 150, Color.White, 1f, true, false, currentStage);
            playermiscsprites.Initialize(swapColor(content.Load<Texture2D>("Sprites/billrizermiscsprites")), Vector2.Zero, 6, Color.White, 1);
            idle.Initialize(swapColor(content.Load<Texture2D>("Sprites/billrizeridle")), position, 1, 1, Color.White, 1f, true, currentStage);
            idlelegs.Initialize(swapColor(content.Load<Texture2D>("Sprites/billrizeridlelegs")), position, 1, 1, Color.White, 1f, true, currentStage);            
            dropping.Initialize(swapColor(content.Load<Texture2D>("Sprites/billrizerdropping")), position, 1, 1, Color.White, 1f, true, currentStage);
            prone.Initialize(swapColor(content.Load<Texture2D>("Sprites/billrizerprone")), position, 1, 1, Color.White, 1f, true, currentStage);
            deathAnimation.Initialize(swapColor(content.Load<Texture2D>("Sprites/billrizerdying")), position, 6, 200, Color.White, 1f, false, currentStage);
            deathAnimation.Active = false; 

            projectileTexture = content.Load<Texture2D>("Sprites/basicbulletanimated");
            soundProjectileHit = content.Load<SoundEffect>("Sounds/projectilehit");

            soundPlayerLand = content.Load<SoundEffect>("Sounds/jumpland");
            soundGunshot = content.Load<SoundEffect>("Sounds/gunshot3");
            soundDeath = content.Load<SoundEffect>("Sounds/deathsound");
            
            // Set the starting position of the player around the middle of the screen and to the back            
            WorldPosition = position; 

            
        }
        // Switch player color to denote player 1 (blue default), player 2 (red), player 3 etc...
        private Texture2D swapColor(Texture2D thisTexture)
        {
            if (this.ID == 1)
                // no color swap for player 1
                return thisTexture;
            else
            {
                Texture2D newTexture = new Texture2D(thisTexture.GraphicsDevice, thisTexture.Width, thisTexture.Height);
                
                Color[] data = new Color[thisTexture.Width * thisTexture.Height];
                thisTexture.GetData(data);

                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i].B > 200)
                    {
                        switch (ID)
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

        // Update the player animation
        public void Update(GameTime gameTime)
        {
            this.GetInput();

            if (!PreviousInputState.StartButtonPressed && CurrentInputState.StartButtonPressed)
                game.TogglePause();                

            if (!game.GamePaused)
            {
                if (SpawnTimeRemaining > 0)
                    SpawnTimeRemaining -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                this.UpdateAnimations(gameTime);

                this.Move(gameTime);

                this.ApplyPhysics(gameTime);

                if (this.LifeCount > 0)
                {
                    if (IsDying && deathAnimation.Active == false)
                    {
                        // dead player's death animation has completed, re-spawn player.
                        this.Spawn();
                    }


                    if (this.SpawnTimeRemaining > 0)
                    {
                        this.UpdateSpawnFlicker(gameTime);
                    }
                    else
                    {
                        this.Visible = true;
                    }
                }
                else
                {
                    // player is inactive.
                    this.Visible = false;
                }
            }

        }
        public void Spawn()
        {
            //Position.X = currentStage.CameraPosition + new Vector2(5, 0);
            playerDirection = PlayerDirection.Right;
            WorldPosition = currentStage.CameraPosition + new Vector2(5, 0);
            //Position.Y = 0;
            this.WorldPosition.Y = 0;
            WorldPosition.Y = 0;
            Velocity = new Vector2(0f, 0f);
            IsDying = false;
            SpawnTimeRemaining = SpawnTime;
        }
        private void UpdateSpawnFlicker(GameTime gameTime)
        {
            spawnAnimationElapsedTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            const int spawnFlickerTime = 15;
            if (spawnAnimationElapsedTime >= spawnFlickerTime)
            {
                // toggle visibility to create flicker effect.
                Visible = !Visible;
                spawnAnimationElapsedTime = 0;
            }
        }
        
        private void UpdateAnimations(GameTime gameTime)
        {
            Vector2 legsPosition;
            if (playerDirection == PlayerDirection.Right)
            {
                legsPosition = new Vector2(WorldPosition.X, WorldPosition.Y + 21);                
            }
            else
            {
                legsPosition = new Vector2(WorldPosition.X, WorldPosition.Y + 21);                
            }

            runningTorsoAnimation.WorldPosition = WorldPosition;
            runningLegsAnimation.WorldPosition = legsPosition;
            jumpingAnimation.WorldPosition = WorldPosition;
            idle.WorldPosition = WorldPosition;
            idlelegs.WorldPosition = legsPosition;
            prone.WorldPosition = new Vector2(WorldPosition.X, WorldPosition.Y + iProneVerticalOffset);
            dropping.WorldPosition = WorldPosition;
            deathAnimation.WorldPosition = WorldPosition;

            playermiscsprites.ScreenPosition = ScreenPosition;            

            runningTorsoAnimation.Update(gameTime);
            runningLegsAnimation.Update(gameTime);
            jumpingAnimation.Update(gameTime);
            idle.Update(gameTime);
            idlelegs.Update(gameTime);
            prone.Update(gameTime);
            dropping.Update(gameTime);

            if (deathAnimation.Active)
            {                
                deathAnimation.Update(gameTime);                
            }
    
        }
        public void GetInput()
        {

            GamePadState currentGamePadState;
            KeyboardState currentKeyboardState;

            // Save the previous state of the input so we can determine single key/button presses
            if (CurrentInputState != null)
            {
                PreviousInputState.CopyFrom(CurrentInputState);
                CurrentInputState.Reset();
            }
            else
            {
                PreviousInputState = new InputState();
                CurrentInputState = new InputState();
            }

            currentGamePadState = GetGamePadInput(); 
            // only handle keyboard input for player #1. other players must use gamepads.
            currentKeyboardState = Keyboard.GetState();                
            
            if ((currentGamePadState.IsButtonDown(Buttons.DPadUp)) || (this.ID == 1 && currentKeyboardState.IsKeyDown(Keys.Up)))
                CurrentInputState.DirectionUp = true;
            if ((currentGamePadState.IsButtonDown(Buttons.DPadDown)) || (this.ID == 1 && currentKeyboardState.IsKeyDown(Keys.Down)))
                CurrentInputState.DirectionDown = true;
            if ((currentGamePadState.IsButtonDown(Buttons.DPadLeft)) || (this.ID == 1 && currentKeyboardState.IsKeyDown(Keys.Left)))
                CurrentInputState.DirectionLeft = true;
            if ((currentGamePadState.IsButtonDown(Buttons.DPadRight)) || (this.ID == 1 && currentKeyboardState.IsKeyDown(Keys.Right)))
                CurrentInputState.DirectionRight = true;
            if ((currentGamePadState.IsButtonDown(Buttons.X)) || (this.ID == 1 && currentKeyboardState.IsKeyDown(Keys.A)))
                CurrentInputState.WeaponButtonPressed = true;
            if ((currentGamePadState.IsButtonDown(Buttons.A)) || (this.ID == 1 && currentKeyboardState.IsKeyDown(Keys.Space)))
                CurrentInputState.JumpButtonPressed = true;
            if ((currentGamePadState.IsButtonDown(Buttons.Start)) || (this.ID == 1 && currentKeyboardState.IsKeyDown(Keys.Enter)))
                CurrentInputState.StartButtonPressed = true;

            if (!IsDying && !game.GamePaused)
            {
                DetermineAnalogHorizontalMovement(currentGamePadState, currentKeyboardState);                
            }
        }
        private void DetermineAnalogHorizontalMovement(GamePadState currentGamePadState, KeyboardState currentKeyboardState)
        {
            CurrentInputState.Movement = currentGamePadState.ThumbSticks.Left.X * MoveStickScale;

            // Ignore small movements to prevent running in place.
            if (Math.Abs(CurrentInputState.Movement) < 0.5f)
                CurrentInputState.Movement = 0.0f;

            if (currentGamePadState.IsButtonDown(Buttons.DPadLeft))
                CurrentInputState.Movement = -1.0f;
            if (ID == 1 && currentKeyboardState.IsKeyDown(Keys.Left))
                CurrentInputState.Movement = -1.0f;

            if (currentGamePadState.IsButtonDown(Buttons.DPadRight))
                CurrentInputState.Movement = 1.0f;
            if (ID == 1 && currentKeyboardState.IsKeyDown(Keys.Right))
                CurrentInputState.Movement = 1.0f;
        }
        private GamePadState GetGamePadInput()
        {
            GamePadState currentGamePadState;
            if (ID == 1)
                currentGamePadState = GamePad.GetState(PlayerIndex.One);
            else if (ID == 2)
                currentGamePadState = GamePad.GetState(PlayerIndex.Two);
            else if (ID == 3)
                currentGamePadState = GamePad.GetState(PlayerIndex.Three);
            else if (ID == 4)
                currentGamePadState = GamePad.GetState(PlayerIndex.Four);
            else
                throw new Exception("ID: " + ID.ToString() + " not supported.");

            return currentGamePadState;
        }

        public override void Move(GameTime gameTime)
        {
            #region Set Horizontal Velocity, if Player isn't in the process of performing an automated action such as climbing stairs
            if ((!this.IsOnStairs ||
                (this.IsOnStairs && this.StandingOnStairTop())) && 
                !this.SecuringStairStep && 
                !this.MovingTowardsStairs)
            {
                if (this.IsDying && !this.IsOnGround)
                {
                    // player knocked BACKWARDS at full velocity while in midair dying animation
                    Velocity.X = -(int)this.playerDirection * MaxGroundVelocity;
                }
                else
                {
                    Velocity.X = CurrentInputState.Movement * MaxGroundVelocity;
                    //this.Status = Player.PlayerStatus.Running;
                    if (this.Velocity.X > 0)
                        this.playerDirection = Player.PlayerDirection.Right;
                    else if (this.Velocity.X < 0)
                        this.playerDirection = Player.PlayerDirection.Left;
                }
            }
            #endregion

            if (!IsDying)
            {
                #region Walk Left or Right
                if ((!this.IsOnStairs ||
                    (this.IsOnStairs && this.StandingOnStairTop())) &&
                    !this.SecuringStairStep)
                {
                    if (CurrentInputState.DirectionLeft)
                    {
                        this.Velocity.X = -MaxGroundVelocity;
                        this.playerDirection = Player.PlayerDirection.Left;
                    }
                    else if (CurrentInputState.DirectionRight)
                    {
                        this.Velocity.X = MaxGroundVelocity;
                        this.playerDirection = Player.PlayerDirection.Right;
                    }
                
                }
                #endregion

                #region Player Ducking
                if (!this.IsOnStairs)
                {
                    // player only stays prone as long as he is pressing down.
                    this.IsProne = false;
                    if (CurrentInputState.DirectionDown)
                    {
                        if (this.FindNearbyStairtop().HasValue)
                        {
                            Vector2? moveToLocation = this.FindNearbyStairtop();
                            if (moveToLocation.Value.X == this.BoundingBox().Center.X)
                            {
                                // player already aligned with stairs                            
                                moveToLocation = null;
                                this.Velocity.X = 0;
                                this.MovingTowardsStairs = false;
                            }
                            else if (moveToLocation.Value.X < (int)this.BoundingBox().Center.X)
                            {
                                this.playerDirection = PlayerDirection.Left;
                                this.Velocity.X = -1;
                            }
                            else
                            {
                                this.playerDirection = PlayerDirection.Right;
                                this.Velocity.X = 1;
                            }

                            if (moveToLocation.HasValue)
                            {
                                this.MovingTowardsStairsPosition = (int)moveToLocation.Value.X;
                                this.MovingTowardsStairs = true;
                                this.MovingTowardsStairsAscending = false;
                            }
                        }
                        else if (this.Velocity.X == 0 && this.IsOnGround == true)
                        {
                            this.IsProne = true;
                        }
                    }
                }
                #endregion

                #region Initiate or Continue Move Towards Stairs if not on stairs and stairs are nearby
                if (this.IsOnGround && !this.IsOnStairs && 
                    (this.CurrentInputState.DirectionUp || this.MovingTowardsStairs))
                {
                    // are there stairs nearby?  
                    Vector2? moveToLocation = this.FindNearbyStairbase();
                    
                    if (moveToLocation.HasValue)
                    {   
                        if (moveToLocation.Value.X == this.BoundingBox().Center.X)
                        {
                            // player already aligned with stairs                            
                            moveToLocation = null;
                            this.Velocity.X = 0;
                            this.MovingTowardsStairs = false;
                        }
                        else if (moveToLocation.Value.X < (int)this.BoundingBox().Center.X)
                        {                            
                            this.playerDirection = PlayerDirection.Left;
                            this.Velocity.X = -1;
                        }
                        else
                        {
                            this.playerDirection = PlayerDirection.Right;
                            this.Velocity.X = 1;
                        }

                        if (moveToLocation.HasValue)
                        {
                            this.MovingTowardsStairsPosition = (int)moveToLocation.Value.X;
                            this.MovingTowardsStairs = true;
                            this.MovingTowardsStairsAscending = true;
                        }
                    }

                    // if yes, then what are the nearest set of stairs?

                    // if the player is not already 
                }
                #endregion

                #region Begin Jump/Drop Down
                // Jump/Drop Down
                if (!this.PreviousInputState.JumpButtonPressed && this.CurrentInputState.JumpButtonPressed && this.IsOnGround && !this.MovingUpStairs && !this.MovingDownStairs && !this.MovingTowardsStairs)
                {
                    // cancel prone status if it's activated.
                    this.IsProne = false;
                    if (CurrentInputState.DirectionDown && 
                        ((this.BoundingBox().Bottom / currentStage.iTileHeight) < currentStage.MapHeight-1))
                    {
                        // player is dropping down                    
                        this.DropInProgress = true;
                        this.IgnoreNextPlatform = true;
                    }
                    else
                    {
                        // initiate jump.
                        this.IsJumping = true;
                        this.JumpInProgress = true;
                    }                    
                }

                #endregion
                
                #region Move Up Stairs if on stairs and pressing up
                if (this.IsOnStairs && 
                    (CurrentInputState.DirectionUp ||
                    (CurrentInputState.DirectionRight && this.IsOnStairsRight) || 
                    (CurrentInputState.DirectionLeft && this.IsOnStairsLeft)) && 
                    !this.MovingDownStairs && !this.MovingUpStairs && 
                    !this.StandingOnStairTop())
                {
                    MovingUpStairs = true;
                    lastStairtopHeight = this.BoundingBox().Bottom;
                    if (this.IsOnStairsLeft)
                        playerDirection = PlayerDirection.Left;
                    else
                        playerDirection = PlayerDirection.Right;
                    
                }
                #endregion

                #region Move Down Stairs if on stairs and pressing down
                if (this.IsOnStairs && 
                    (CurrentInputState.DirectionDown ||
                    (CurrentInputState.DirectionRight && this.IsOnStairsLeft) || 
                    (CurrentInputState.DirectionLeft && this.IsOnStairsRight)) &&
                    !this.MovingDownStairs && !this.MovingUpStairs)
                {
                    MovingDownStairs = true;
                    lastStairtopHeight = this.BoundingBox().Bottom;
                    if (this.IsOnStairsLeft)
                        playerDirection = PlayerDirection.Right;
                    else
                        playerDirection = PlayerDirection.Left;
                }
                #endregion

                
                if (!this.MovingDownStairs && !this.MovingUpStairs & !this.SecuringStairStep)
                {
                    #region Point Gun
                    if (CurrentInputState.DirectionUp && !CurrentInputState.DirectionLeft && !CurrentInputState.DirectionRight)
                        gunDirection = GunDirection.StraightUp;
                    else if (CurrentInputState.DirectionDown && !CurrentInputState.DirectionLeft && !CurrentInputState.DirectionRight && JumpInProgress)
                        gunDirection = GunDirection.StraightDown;
                    else if (CurrentInputState.DirectionUp && (CurrentInputState.DirectionLeft || CurrentInputState.DirectionRight))
                        gunDirection = GunDirection.High;
                    else if (CurrentInputState.DirectionDown && (CurrentInputState.DirectionLeft || CurrentInputState.DirectionRight))
                        gunDirection = GunDirection.Low;
                    else
                        gunDirection = GunDirection.Neutral;
                    #endregion
                }
                else
                {
                    gunDirection = GunDirection.Neutral;
                }

                

                #region Fire gun if button pressed

                if ((!PreviousInputState.WeaponButtonPressed && CurrentInputState.WeaponButtonPressed) &&
                    // firing gun (note: player cannot fire gun while underwater)
                    !(this.IsInWater && this.IsProne))
                {

                    if (this.playerDirection == Player.PlayerDirection.Right)
                        AddProjectile(new Vector2(this.WorldPosition.X + this.Width - 4, this.WorldPosition.Y + 18));
                    else
                        AddProjectile(new Vector2(this.WorldPosition.X + 4, this.WorldPosition.Y + 18));

                    soundGunshot.Play();
                }
                #endregion

            }  // !IsDying

            

        }

        

        public override void ApplyPhysics(GameTime gameTime)
        {
            if (this.MovingTowardsStairs)
            {
                Rectangle playerbounds = this.BoundingBox();
                StageTile st = null;
                if ((this.playerDirection == PlayerDirection.Left && this.MovingTowardsStairsPosition > playerbounds.Left) ||
                    (this.playerDirection == PlayerDirection.Right && this.MovingTowardsStairsPosition < playerbounds.Right))
                {
                    if (this.playerDirection == PlayerDirection.Left)
                    {
                        this.WorldPosition.X += (this.MovingTowardsStairsPosition - playerbounds.Left);
                    }
                    if (this.playerDirection == PlayerDirection.Right)
                    {
                        this.WorldPosition.X += (this.MovingTowardsStairsPosition - playerbounds.Right);
                    }
                    this.MovingTowardsStairs = false;
                    this.MovingTowardsStairsPosition = 0;
                    if (this.MovingTowardsStairsAscending)
                    {
                        this.MovingUpStairs = true;
                        st = currentStage.getStageTileByWorldPosition(playerbounds.Center.X, playerbounds.Bottom);
                        this.IsOnStairsLeft = st.CollisionType == StageTile.TileCollisionType.StairsBottomLeft;
                        this.IsOnStairsRight = st.CollisionType == StageTile.TileCollisionType.StairsBottomRight;
                    }
                    else
                    {
                        this.MovingDownStairs = true;
                        st = currentStage.getStageTileByWorldPosition(playerbounds.Center.X, playerbounds.Bottom + 1);
                        this.IsOnStairsLeft = st.CollisionType == StageTile.TileCollisionType.StairsLeft;
                        this.IsOnStairsRight = st.CollisionType == StageTile.TileCollisionType.StairsRight;
                    }
                    

                    lastStairtopHeight = this.BoundingBox().Bottom;
                }                
            }
            if (this.MovingUpStairs)
            {
                if (this.IsOnStairsRight)
                {
                    this.WorldPosition.X += 1;
                    this.WorldPosition.Y -= 1;                    
                }
                else if (this.IsOnStairsLeft)
                {
                    this.WorldPosition.X -= 1;
                    this.WorldPosition.Y -= 1;                    
                }
                if (this.BoundingBox().Bottom <= lastStairtopHeight - StairHeight)
                {
                    this.SecuringStairStep = true;
                    this.MovingDownStairs = false;
                    this.MovingUpStairs = false;
                    this.PreviousBottom = this.BoundingBox().Bottom;
                }
            }
            else if (this.MovingDownStairs)
            {
                if (this.BoundingBox().Bottom + 1 <= lastStairtopHeight + StairHeight)
                {
                    if (this.IsOnStairsRight)
                    {
                        this.WorldPosition.X -= 1;
                        this.WorldPosition.Y += 1;
                    }
                    else if (this.IsOnStairsLeft)
                    {
                        this.WorldPosition.X += 1;
                        this.WorldPosition.Y += 1;
                    }
                }
                else
                {
                    this.MovingDownStairs = false;
                    this.MovingUpStairs = false;
                    this.SecuringStairStep = true;
                    this.PreviousBottom = this.BoundingBox().Bottom;
                    base.ApplyPhysics(gameTime);
                }
            }
            //else if (this.SecuringStairStep)
            //{
                
            //    this.SecuringStairStep = false;
            //}
            else
            {
                base.ApplyPhysics(gameTime);
            }
            
            // do not allow player to move off left side of camera view.
            this.WorldPosition.X = MathHelper.Clamp(this.WorldPosition.X, currentStage.CameraPosition.X, this.WorldPosition.X);

            if (!currentStage.AutoScroll)
            {
                if (this.WorldPosition.X > this.currentStage.CameraPosition.X + Game.iScreenModelWidth - (this.Width * 3))
                    currentStage.CameraPosition.X += this.WorldPosition.X - (this.currentStage.CameraPosition.X + Game.iScreenModelWidth - (this.Width * 3));

                if (this.WorldPosition.X > (this.currentStage.MapWidth * currentStage.iTileWidth - Game.iScreenModelWidth ))
                {
                    if (currentStage.game.currentGame == Game.GameType.Contra)
                        currentStage.AutoScroll = true;
                }
            }

            if (!IsDying || IsOnGround)
                this.CurrentInputState.Movement = 0.0f;
                        
        }
        public Vector2? FindNearbyStairbase()
        {
            int searchRange = currentStage.iTileWidth;

            Rectangle playerbounds = this.BoundingBox(new Vector2(0f, 1f));
            Rectangle playerSearchbounds = new Rectangle(playerbounds.Left - searchRange , playerbounds.Top, playerbounds.Width + searchRange, playerbounds.Height);
            
            int leftTile = (int)Math.Floor((float)(playerSearchbounds.Left) / currentStage.iTileWidth);
            leftTile = leftTile < 0 ? 0 : leftTile;
            int rightTile = (int)Math.Ceiling(((float)(playerSearchbounds.Right) / currentStage.iTileWidth)) - 1;
            int bottomTile = (int)Math.Ceiling(((float)playerSearchbounds.Bottom / currentStage.iTileHeight)) - 1;

            for (int x = leftTile; x <= rightTile; ++x)
            {
                StageTile stageTile = currentStage.getStageTileByGridPosition(x, bottomTile);
                if (stageTile.CollisionType == StageTile.TileCollisionType.StairsBottomLeft || stageTile.CollisionType == StageTile.TileCollisionType.StairsBottomRight)
                {
                    foreach(var platform in currentStage.getTilePlatformBoundsByGridPosition(x, bottomTile))
                    {
                        return new Vector2(platform.PlatformBounds.Center.X, platform.PlatformBounds.Bottom);                        
                    }
                }
            }

            return null;
        }
        public Vector2? FindNearbyStairtop()
        {
            int searchRange = currentStage.iTileWidth;

            Rectangle playerbounds = this.BoundingBox(new Vector2(0f, 1f));
            Rectangle playerSearchbounds = new Rectangle(playerbounds.Left - searchRange, playerbounds.Top, playerbounds.Width + searchRange, playerbounds.Height);

            int leftTile = (int)Math.Floor((float)(playerSearchbounds.Left) / currentStage.iTileWidth);
            leftTile = leftTile < 0 ? 0 : leftTile;
            int rightTile = (int)Math.Ceiling(((float)(playerSearchbounds.Right) / currentStage.iTileWidth)) - 1;
            int bottomTile = (int)Math.Ceiling(((float)playerSearchbounds.Bottom / currentStage.iTileHeight)) - 1;

            for (int x = leftTile; x <= rightTile; ++x)
            {
                StageTile stageTile = currentStage.getStageTileByGridPosition(x, bottomTile);
                if (stageTile.IsStairs())
                {
                    Rectangle? stairTopBounds = currentStage.getStairTopBoundsByGridPosition(x, bottomTile);
                    if (stairTopBounds.HasValue)
                    {
                        if (stairTopBounds.Value.Top == this.BoundingBox().Bottom)
                        {
                            return new Vector2(stairTopBounds.Value.Center.X, stairTopBounds.Value.Bottom);
                        }
                    }
                }
            }

            return null;
        }

        public override void HandleCollisions(GameTime gameTime)
        {

            if (this.WorldPosition.Y > Game.iScreenModelHeight && IsDying == false)
            {
                Die(gameTime);
            }
            Rectangle playerbounds;
            playerbounds = this.BoundingBox();            

            bool playerwasonground = this.IsOnGround;
            bool playerwasonstairs = this.IsOnStairs;
            bool droppedthroughplatform = false;

            // get nearest tile below player.
            this.IsOnGround = false;
            this.IsOnStairsLeft = false;
            this.IsOnStairsRight = false;
            
            int leftTile = (int)Math.Floor((float)playerbounds.Left / currentStage.iTileWidth);
            int rightTile = (int)Math.Ceiling(((float)playerbounds.Right / currentStage.iTileWidth)) - 1;
            int topTile = (int)Math.Floor((float)playerbounds.Top / currentStage.iTileHeight);
            int bottomTile = (int)Math.Ceiling(((float)playerbounds.Bottom / currentStage.iTileHeight)) - 1;

            // For each potentially colliding platform tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    StageTile stageTile = currentStage.getStageTileByGridPosition(x, y);

                    if (stageTile != null)
                    {
                        if (stageTile.IsImpassable())
                        {
                            Rectangle tilebounds = currentStage.getTileBoundsByGridPosition(x, y);
                            Vector2 depth = RectangleExtensions.GetIntersectionDepth(playerbounds, tilebounds);

                            if (playerbounds.Intersects(tilebounds))
                            {
                                WorldPosition = new Vector2(WorldPosition.X + depth.X, WorldPosition.Y);
                                playerbounds = this.BoundingBox();
                            }
                        }                                                        
                        else if (stageTile.IsStairs() && y == bottomTile)
                        {
                            if (playerwasonstairs || CurrentInputState.DirectionUp || SecuringStairStep)
                            {
                                List<Platform> tileboundsList = currentStage.getTilePlatformBoundsByGridPosition(x, bottomTile);
                                foreach (Platform platformBounds in tileboundsList)
                                {
                                    Rectangle tileBounds = platformBounds.PlatformBounds;
                                    Vector2 depth = RectangleExtensions.GetIntersectionDepth(playerbounds, tileBounds);

                                    if (this.PreviousBottom <= tileBounds.Top && Velocity.Y >= 0 && playerbounds.Intersects(tileBounds))
                                    //if (Velocity.Y >= 0 && (depth.Y < 0)) // || this.IgnoreNextPlatform))
                                    {
                                        if (this.IgnoreNextPlatform == false)
                                        {
                                            this.JumpInProgress = false;
                                            this.DropInProgress = false;
                                            this.IsOnGround = true;
                                            this.SecuringStairStep = false;

                                            this.WorldPosition.Y += depth.Y;


                                            if (stageTile.CollisionType == StageTile.TileCollisionType.StairsLeft)
                                            {
                                                this.IsOnStairsLeft = true;
                                                if (!this.StandingOnStairTop())
                                                    this.WorldPosition.X += tileBounds.Left - this.BoundingBox().Left;
                                            }
                                            else
                                            {
                                                this.IsOnStairsRight = true;
                                                if (!this.StandingOnStairTop())
                                                    this.WorldPosition.X += tileBounds.Right - (this.BoundingBox().Right);
                                            }

                                            // perform further collisions with the new bounds
                                            playerbounds = this.BoundingBox();

                                        }
                                        else
                                        {
                                            droppedthroughplatform = true;
                                        }
                                    }
                                }
                            }
                        }                        
                        else if (stageTile.IsPlatform() && y == bottomTile)
                        {

                            List<Platform> platforms = currentStage.getTilePlatformBoundsByGridPosition(x, bottomTile);
                            foreach (Platform platform in platforms)
                            {
                                Rectangle tilebounds = platform.PlatformBounds;
                                Vector2 depth = RectangleExtensions.GetIntersectionDepth(playerbounds, tilebounds);

                                if (this.PreviousBottom <= tilebounds.Top && Velocity.Y >= 0 && playerbounds.Intersects(tilebounds))
                                //if (Velocity.Y >= 0 && (depth.Y < 0)) // || this.IgnoreNextPlatform))
                                {
                                    if (this.IgnoreNextPlatform == false)
                                    {
                                        this.JumpInProgress = false;
                                        this.DropInProgress = false;
                                        this.IsOnGround = true;
                                        this.SecuringStairStep = false;
                                        if (stageTile.IsWaterPlatform())
                                            this.IsInWater = true;
                                        else
                                            this.IsInWater = false;

                                        this.WorldPosition.Y += depth.Y;
                                        // perform further collisions with the new bounds
                                        playerbounds = this.BoundingBox();

                                    }
                                    else
                                    {
                                        droppedthroughplatform = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            

            if (droppedthroughplatform)
                this.IgnoreNextPlatform = false;

            if (this.IsOnGround && !playerwasonground)
            {
                //Debug.WriteLine("Player Hit Ground " + gameTime.ElapsedGameTime.Seconds.ToString());
                soundPlayerLand.Play();
            }

            this.DebugInfo += this.IsOnStairs;
            //this.DebugInfo += " " + this.BoundingBox().ToString();

            if (playerwasonground && !IsOnGround)
                Velocity.Y = 0;

            this.PreviousBottom = playerbounds.Bottom;
        }
        public override Rectangle BoundingBox()
        {
            //int iFootWidth = 8;
            //int iSpriteOffsetTop;
            //int iSpriteOffsetBottom;
            //if (IsProne)
            //{
            //    iSpriteOffsetTop = 27;
            //    iSpriteOffsetBottom = 35;
            //}
            //else
            //{
            //    iSpriteOffsetTop = 6;
            //    iSpriteOffsetBottom = 14;
            //}
            //int iSpriteOffsetX = 0;

            //if (playerDirection == PlayerDirection.Right)
            //    iSpriteOffsetX = 11;
            //else
            //    iSpriteOffsetX = 11;


            //return new Rectangle((int)WorldPosition.X + iSpriteOffsetX, (int)WorldPosition.Y + iSpriteOffsetTop, iFootWidth, idle.FrameHeight - iSpriteOffsetBottom);
            return BoundingBox(new Vector2(0, 0));

        }
        public Rectangle BoundingBox(Vector2 offset)
        {
            Vector2 position = this.WorldPosition + offset;

            int iFootWidth = 8;
            int iSpriteOffsetTop;
            int iSpriteOffsetBottom;
            if (IsProne)
            {
                iSpriteOffsetTop = 27;
                iSpriteOffsetBottom = 35;
            }
            else
            {
                iSpriteOffsetTop = 6;
                iSpriteOffsetBottom = 14;
            }
            int iSpriteOffsetX = 0;

            if (playerDirection == PlayerDirection.Right)
                iSpriteOffsetX = 11;
            else
                iSpriteOffsetX = 11;


            return new Rectangle((int)position.X + iSpriteOffsetX, (int)position.Y + iSpriteOffsetTop, iFootWidth, idle.FrameHeight - iSpriteOffsetBottom);
        }
        public Rectangle HurtBox()
        {
            if (this.JumpInProgress)
            {
                int iSpriteOffsetTop = 6;
                int iSpriteOffsetBottom = 30;
                int iSpriteOffsetX = 8;                
                Vector2 position = this.WorldPosition;

                return new Rectangle((int)position.X + iSpriteOffsetX, (int)position.Y + iSpriteOffsetTop, this.Width - (iSpriteOffsetX * 2), idle.FrameHeight - iSpriteOffsetBottom);
            }
            else
            {
                return this.BoundingBox();
            }
        }
        public bool StandingOnStairTop()
        {
            Rectangle playerbounds = this.BoundingBox(new Vector2(0f, 1f));
            List<StageTile> tiles = new List<StageTile>();

            int leftTile = (int)Math.Floor((float)playerbounds.Left / currentStage.iTileWidth);
            int rightTile = (int)Math.Ceiling(((float)playerbounds.Right / currentStage.iTileWidth)) - 1;            
            int bottomTile = (int)Math.Ceiling(((float)playerbounds.Bottom / currentStage.iTileHeight)) - 1;

            bool bReturnValue = false;
            
            for (int x = leftTile; x <= rightTile; ++x)
                {   
                    StageTile stageTile = currentStage.getStageTileByGridPosition(x, bottomTile);

                    if (stageTile.IsStairs())
                    {
                        if ((stageTile.CollisionType == StageTile.TileCollisionType.StairsLeft && !currentStage.getStageTileByGridPosition(x - 1, bottomTile - 1).IsStairs()) ||
                            (stageTile.CollisionType == StageTile.TileCollisionType.StairsRight && !currentStage.getStageTileByGridPosition(x + 1, bottomTile - 1).IsStairs()))
                        {
                            if (this.BoundingBox().Bottom == currentStage.getStairTopBoundsByGridPosition(x, bottomTile).Value.Top)
                            //if (playerbounds.Intersects(currentStage.getStairTopBoundsByGridPosition(x, bottomTile)))
                                bReturnValue = true;
                        }
                    }
                }

            return bReturnValue;

        }
        
        protected override float DoJump(float velocityY, GameTime gameTime)
        {
            float launchVelocity = 0f;
            if (IsDying)
                launchVelocity = JumpLaunchVelocityDying;
            else
                launchVelocity = JumpLaunchVelocity;


            // If the player wants to jump
            if (this.IsJumping || this.JumpInProgress)
            {
                // Begin or continue a jump
                if ((!this.WasJumping && this.IsOnGround) || this.JumpTime > 0.0f)
                {
                    this.JumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    //sprite.PlayAnimation(jumpAnimation);
                }

                // If we are in the ascent of the jump
                if (0.0f < this.JumpTime && this.JumpTime <= Player.MaxJumpTime)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocityY = launchVelocity * (1.0f - (float)Math.Pow(this.JumpTime / Player.MaxJumpTime, Player.JumpControlPower));
                }
                else
                {
                    // Reached the apex of the jump
                    this.JumpTime = 0.0f;
                }
            }
            else
            {
                // Continues not jumping or cancels a jump in progress
                this.JumpTime = 0.0f;
            }
            this.WasJumping = this.IsJumping;

            return velocityY;
        }
        public override void Die(GameTime gameTime)
        {
            soundDeath.Play();
            deathAnimation.Play();
            
            if (LifeCount > 0)
                LifeCount--;

            IsDying = true;

            // player knocked into the air, creating jump effect             
            IsOnGround = true; //make player seem as if they were on ground even if they were killed midair, to create jump effect.
            IsJumping = true;
            JumpInProgress = true;
            if (playerDirection == PlayerDirection.Right)
            {
                CurrentInputState.Movement = -1.0f;
                Velocity.X = -MaxGroundVelocity;
            }
            else
            {
                CurrentInputState.Movement = 1.0f;
                Velocity.X = MaxGroundVelocity;
            }            
        }
        private void AddProjectile(Vector2 position)
        {
            Projectile projectile = new Projectile();
            Vector2 gunBarrelLocation;

            float fHorizontalOffset = 0.0f;
            float fVerticalOffset = 0.0f;

            int iDirection;
            if (playerDirection == PlayerDirection.Right)
                iDirection = 1;
            else
                iDirection = -1;

            const int iProneHorizontalOffset = 20;

            const int iStraightUpVerticalOffset = -20;
            const int iStraightUpHorizontalOffset = -10;
            const int iStraightDownVerticalOffset = 10;
            const int iStraightDownHorizontalOffset = -10;
            const int iHighHorizontalOffset = -4;
            const int iHighVerticalOffset = -10;
            const int iLowHorizontalOffset = -3;
            const int iLowVerticalOffset = 6;
            int gunAngle = 90;

            if (this.IsProne)
            {
                fHorizontalOffset = iProneHorizontalOffset * iDirection;
                fVerticalOffset = iProneVerticalOffset;
                gunAngle = 90;
            }
            else if (gunDirection == GunDirection.StraightUp)
            {
                fHorizontalOffset = iStraightUpHorizontalOffset * iDirection;
                fVerticalOffset = iStraightUpVerticalOffset;
                gunAngle = 0;
            }
            else if (gunDirection == GunDirection.High)
            {
                fHorizontalOffset = iHighHorizontalOffset * iDirection;
                fVerticalOffset = iHighVerticalOffset;
                gunAngle = 65;
            }
            else if (gunDirection == GunDirection.Low)
            {
                fHorizontalOffset = iLowHorizontalOffset * iDirection;
                fVerticalOffset = iLowVerticalOffset;
                gunAngle = 130;
            }
            else if (gunDirection == GunDirection.StraightDown)
            {
                fHorizontalOffset = iStraightDownHorizontalOffset * iDirection;
                fVerticalOffset = iStraightDownVerticalOffset;
                gunAngle = 180;
            }

            if (playerDirection == PlayerDirection.Left)
            {
                gunAngle = 360 - gunAngle;
            }
            
            gunBarrelLocation = new Vector2(position.X + fHorizontalOffset, position.Y + fVerticalOffset);
            
            //projectile.Initialize(projectileTexture, gunBarrelLocation, this, currentStage);
            Animation projectileAnimation = new Animation();
            projectileAnimation.Initialize(projectileTexture, gunBarrelLocation, 3, 10, Color.White, 1f, true, currentStage);
            projectile.Initialize(projectileAnimation, soundProjectileHit, gunBarrelLocation, gunAngle, currentStage, 3f);
            currentStage.Projectiles.Add(projectile);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (deathAnimation.Active)
            {
                deathAnimation.Draw(spriteBatch, playerDirection, 1f);
            }
            else if (JumpInProgress == true)
            {
                jumpingAnimation.Draw(spriteBatch, playerDirection, 1f);
            }
            else if (DropInProgress == true)
            {
                dropping.Draw(spriteBatch, playerDirection, 1f);
            }
            else if (IsProne == true)
            {
                if (IsInWater)
                    playermiscsprites.Draw(spriteBatch, playerDirection, 1f, PlayerSpriteCollection.PlayerSpriteTypes.Underwater);
                else
                    prone.Draw(spriteBatch, playerDirection, 1f);
            }
            else if (IsInWater)
            {
                playermiscsprites.Draw(spriteBatch, playerDirection, 1f, PlayerSpriteCollection.PlayerSpriteTypes.Wading);
            }
            else
            {
                if (Math.Abs(Velocity.X) > 0)
                {
                    runningLegsAnimation.Draw(spriteBatch, playerDirection, 0.9f);
                    switch (gunDirection)
                    {
                        case GunDirection.High:
                            playermiscsprites.Draw(spriteBatch, playerDirection, 1f, PlayerSpriteCollection.PlayerSpriteTypes.GunHigh);
                            break;
                        case GunDirection.Low:
                            playermiscsprites.Draw(spriteBatch, playerDirection, 1f, PlayerSpriteCollection.PlayerSpriteTypes.GunLow);
                            break;
                        default:                            
                            runningTorsoAnimation.Draw(spriteBatch, playerDirection, 1f);
                            break;
                    }
                }
                else
                {
                    if (gunDirection == GunDirection.StraightUp)
                    {
                        idlelegs.Draw(spriteBatch, playerDirection, 0.9f);
                        playermiscsprites.Draw(spriteBatch, playerDirection, 1.0f, PlayerSpriteCollection.PlayerSpriteTypes.GunStraightUp);
                    }
                    else
                        idle.Draw(spriteBatch, playerDirection, 1f);
                }
            }

            base.Draw(spriteBatch);

        }

    }
}
