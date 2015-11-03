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
using RunAndGun.GameObjects;
using RunAndGun.Animations;
using SharpDX.DirectInput;

namespace RunAndGun.Actors
{
    public class Player : Actors.Actor
    {

        public int ID;
        private Game _game;
#region "Declarations"

        public enum PlayerDirection { Left = -1, Right = 1};
        public PlayerDirection playerDirection;
        public Gun Gun;
        public enum GunDirection { StraightUp, High, Low, Neutral, StraightDown };
        public GunDirection gunDirection;

#endregion

        // for drawing prone sprite, and offsetting origin point of bullets
        const int iProneVerticalOffset = 16;

        #region Content Objects

        private Animation _idle;
        private Animation _idlelegs;
        private Animation _dropping;
        private Animation _prone;
        private Animation _deathAnimation;        
        private Animation _runningTorsoAnimation;
        private Animation _runningLegsAnimation;
        private Animation _jumpingAnimation;

        private SoundEffect _soundPlayerLand;
        
        private SoundEffect _soundDeath;
        private PlayerSpriteCollection _playermiscsprites;        

        
        #endregion

        
        private bool _dropInProgress;        
        private bool _isProne;   // is player ducking

        private bool _isInWater; // set to true when user is on a water platform (wading, jungle stage 1)
        public bool _isOnStairsLeft;
        public bool _isOnStairsRight;
        public bool IsOnStairs { get { if (_isOnStairsLeft || _isOnStairsRight) return true; else return false; } }
        private bool _ignoreNextPlatform;

        
        // Amount of lives the player has, including the current one.
        public int LifeCount;
        private float _spawnTimeRemaining;
        private const float _spawnTime = 3.0f;
        private int _spawnAnimationElapsedTime; // control flicker of player while spawning.
        public bool Visible;

        private Guid _USBGamePadId { get; set; }
        private Joystick _joystick;

        private InputState _previousUSBGamePadState;
        private InputState _currentUSBGGamePadState;
        

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
        private float _lastStairtopHeight;     // when moving up or down stairs, keep track of 
        private const float _stairHeight = 8f; // to the player, stairs are 8 pixels in height.
        public bool SecuringStairStep;

        public bool MovingTowardsStairs;
        public int MovingTowardsStairsPosition;
        public bool MovingTowardsStairsAscending;            

        public override int Width
        {
            get { return _idle.FrameWidth; }
        }
        
        public Player(int playerID, Game game)
        {
            //PlayerMoveSpeed = 1.5f;
            //MaxGroundVelocity = 1f;

            _runningTorsoAnimation = new Animation();
            _runningLegsAnimation = new Animation();
            _jumpingAnimation = new Animation();
            _playermiscsprites = new PlayerSpriteCollection();
            _idle = new Animation();
            _idlelegs = new Animation();
            _dropping = new Animation();
            _prone = new Animation();
            _deathAnimation = new Animation();
            
            this._game = game;
            Name = "Player" + playerID.ToString();
            ID = playerID;
            _spawnTimeRemaining = _spawnTime;
            JumpInProgress = true;
            MaxGroundVelocity = 1.2f;

            // X, Y - Y velocity is 0.0 by default, and increases/decreases depending on if player is jumping or falling.
            Velocity = new Vector2(0.0f, 0.0f);

            Gun = new Gun();
            Gun.Initialize(game.Content, GunType.Standard);

            _USBGamePadId = Guid.Empty;
            
        }
        public void InitializeJoystick(Guid joystickId, DirectInput directInput)
        {
            _USBGamePadId = joystickId;            
            _joystick = new Joystick(directInput, joystickId);

            _joystick.Properties.BufferSize = 128;

            _joystick.Acquire();
            
        }
        public bool IsVulnerable()
        {
            if (_spawnTimeRemaining > 0 || IsDying)
                return false;
            else
                return true;
        }
        
        // when player stage changes, move the animations along with him!
        public override Stage CurrentStage
        {
            get
            {
                return base.CurrentStage;
            }
            set
            {
                base.CurrentStage = value;

                _runningTorsoAnimation.currentStage = CurrentStage;
                _runningLegsAnimation.currentStage = CurrentStage;                
                _jumpingAnimation.currentStage = CurrentStage;                
                _idle.currentStage = CurrentStage;                
                _idlelegs.currentStage = CurrentStage;                
                _prone.currentStage = CurrentStage;                
                _dropping.currentStage = CurrentStage;
                _deathAnimation.currentStage = CurrentStage;

            }
        }

        // Initialize the player
        public void Initialize(ContentManager content, Vector2 position, Stage stage)
        {

            // Set the player life count
            LifeCount = 30;

            CurrentStage = stage; 

            //running = animation;
            
            //Status = PlayerStatus.Idle;
            playerDirection = PlayerDirection.Right;
            
            // 32wx32h sprite, 6 frames of animation, 350 frame timee (miliseconds?)
            _runningTorsoAnimation.Initialize(SwapColor(content.Load<Texture2D>("Sprites/billrizerrunningtorso")), Vector2.Zero, 32, 32, 6, 150, Color.White, 1f, true, false, CurrentStage);
            // 32wx24h sprite, 3 frames of animation, , 350 frame timee (miliseconds?)
            _runningLegsAnimation.Initialize(SwapColor(content.Load<Texture2D>("Sprites/billrizerrunninglegs")), Vector2.Zero, 32, 24, 3, 150, Color.White, 1f, true, false, CurrentStage);
            // 32x32 sprite, 4 frames animation, 350 frame time (miliseconds?)
            _jumpingAnimation.Initialize(SwapColor(content.Load<Texture2D>("Sprites/billrizerjumping")), Vector2.Zero, 32, 32, 4, 150, Color.White, 1f, true, false, CurrentStage);
            _playermiscsprites.Initialize(SwapColor(content.Load<Texture2D>("Sprites/billrizermiscsprites")), Vector2.Zero, 6, Color.White, 1);
            _idle.Initialize(SwapColor(content.Load<Texture2D>("Sprites/billrizeridle")), position, 1, 1, Color.White, 1f, true, CurrentStage);
            _idlelegs.Initialize(SwapColor(content.Load<Texture2D>("Sprites/billrizeridlelegs")), position, 1, 1, Color.White, 1f, true, CurrentStage);            
            _dropping.Initialize(SwapColor(content.Load<Texture2D>("Sprites/billrizerdropping")), position, 1, 1, Color.White, 1f, true, CurrentStage);
            _prone.Initialize(SwapColor(content.Load<Texture2D>("Sprites/billrizerprone")), position, 1, 1, Color.White, 1f, true, CurrentStage);
            _deathAnimation.Initialize(SwapColor(content.Load<Texture2D>("Sprites/billrizerdying")), position, 6, 200, Color.White, 1f, false, CurrentStage);
            _deathAnimation.Active = false; 

            

            _soundPlayerLand = content.Load<SoundEffect>("Sounds/jumpland");
            
            _soundDeath = content.Load<SoundEffect>("Sounds/deathsound");
            
            // Set the starting position of the player around the middle of the screen and to the back            
            WorldPosition = position; 

            
        }
        // Switch player color to denote player 1 (blue default), player 2 (red), player 3 etc...
        private Texture2D SwapColor(Texture2D thisTexture)
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
                _game.TogglePause();                

            if (!_game.GamePaused)
            {
                this.Gun.Update(gameTime);

                if (_spawnTimeRemaining > 0)
                    _spawnTimeRemaining -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                this.UpdateAnimations(gameTime);

                this.Move(gameTime);

                this.ApplyPhysics(gameTime);

                if (this.LifeCount > 0)
                {
                    if (IsDying && _deathAnimation.Active == false)
                    {
                        // dead player's death animation has completed, re-spawn player.
                        this.Spawn();
                    }

                    if (this._spawnTimeRemaining > 0)
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
            WorldPosition = CurrentStage.CameraPosition + new Vector2(5, 0);
            //Position.Y = 0;
            this.WorldPosition.Y = 0;
            WorldPosition.Y = 0;
            Velocity = new Vector2(0f, 0f);
            IsDying = false;
            _spawnTimeRemaining = _spawnTime;
            
        }
        private void UpdateSpawnFlicker(GameTime gameTime)
        {
            _spawnAnimationElapsedTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            const int spawnFlickerTime = 15;
            if (_spawnAnimationElapsedTime >= spawnFlickerTime)
            {
                // toggle visibility to create flicker effect.
                Visible = !Visible;
                _spawnAnimationElapsedTime = 0;
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

            _runningTorsoAnimation.WorldPosition = WorldPosition;
            _runningLegsAnimation.WorldPosition = legsPosition;
            _jumpingAnimation.WorldPosition = WorldPosition;
            _idle.WorldPosition = WorldPosition;
            _idlelegs.WorldPosition = legsPosition;
            _prone.WorldPosition = new Vector2(WorldPosition.X, WorldPosition.Y + iProneVerticalOffset);
            _dropping.WorldPosition = WorldPosition;
            _deathAnimation.WorldPosition = WorldPosition;

            _playermiscsprites.ScreenPosition = ScreenPosition;            

            _runningTorsoAnimation.Update(gameTime);
            _runningLegsAnimation.Update(gameTime);
            _jumpingAnimation.Update(gameTime);
            _idle.Update(gameTime);
            _idlelegs.Update(gameTime);
            _prone.Update(gameTime);
            _dropping.Update(gameTime);

            if (_deathAnimation.Active)
            {                
                _deathAnimation.Update(gameTime);                
            }
    
        }
        public void GetInput()
        {   
            GamePadState currentXBox360GamePadState;
            Microsoft.Xna.Framework.Input.KeyboardState currentKeyboardState;

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

            if (_USBGamePadId != Guid.Empty)
            {
                if (_currentUSBGGamePadState != null)
                {
                    ProcessUSBInput();
                    
                }
                else
                {
                    _previousUSBGamePadState = new InputState();
                    _currentUSBGGamePadState = new InputState();
                }

            }

            

            currentXBox360GamePadState = GetGamePadInput(); 
            // only handle keyboard input for player #1. other players must use gamepads.
            currentKeyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();                
            
            if ((currentXBox360GamePadState.IsButtonDown(Buttons.DPadUp)) || (this.ID == 1 && currentKeyboardState.IsKeyDown(Keys.Up)) || _currentUSBGGamePadState.DirectionUp)
                CurrentInputState.DirectionUp = true;
            if ((currentXBox360GamePadState.IsButtonDown(Buttons.DPadDown)) || (this.ID == 1 && currentKeyboardState.IsKeyDown(Keys.Down)) || _currentUSBGGamePadState.DirectionDown)
                CurrentInputState.DirectionDown = true;
            if ((currentXBox360GamePadState.IsButtonDown(Buttons.DPadLeft)) || (this.ID == 1 && currentKeyboardState.IsKeyDown(Keys.Left)) || _currentUSBGGamePadState.DirectionLeft)
                CurrentInputState.DirectionLeft = true;
            if ((currentXBox360GamePadState.IsButtonDown(Buttons.DPadRight)) || (this.ID == 1 && currentKeyboardState.IsKeyDown(Keys.Right)) || _currentUSBGGamePadState.DirectionRight)
                CurrentInputState.DirectionRight = true;
            if ((currentXBox360GamePadState.IsButtonDown(Buttons.X)) || (this.ID == 1 && currentKeyboardState.IsKeyDown(Keys.A)) || _currentUSBGGamePadState.WeaponButtonPressed)
                CurrentInputState.WeaponButtonPressed = true;
            if ((currentXBox360GamePadState.IsButtonDown(Buttons.A)) || (this.ID == 1 && currentKeyboardState.IsKeyDown(Keys.Space)) || _currentUSBGGamePadState.JumpButtonPressed)
                CurrentInputState.JumpButtonPressed = true;
            if ((currentXBox360GamePadState.IsButtonDown(Buttons.Start)) || (this.ID == 1 && currentKeyboardState.IsKeyDown(Keys.Enter)) || _currentUSBGGamePadState.StartButtonPressed)
                CurrentInputState.StartButtonPressed = true;

            if (!IsDying && !_game.GamePaused)
            {
                DetermineAnalogHorizontalMovement(currentXBox360GamePadState, currentKeyboardState);                
            }
        }

        private void ProcessUSBInput()
        {
            _previousUSBGamePadState.CopyFrom(_currentUSBGGamePadState);

            _joystick.Poll();

            var datas = _joystick.GetBufferedData();
            foreach (var state in datas)
            {
                switch (state.Offset)
                {
                    case JoystickOffset.Buttons4:
                    case JoystickOffset.Buttons3:
                        _currentUSBGGamePadState.WeaponButtonPressed = state.Value == 128 ? true : false;
                        break;
                    case JoystickOffset.Buttons2:
                    case JoystickOffset.Buttons1:
                        _currentUSBGGamePadState.JumpButtonPressed = state.Value == 128 ? true : false;
                        break;
                    case JoystickOffset.Buttons9:
                        _currentUSBGGamePadState.StartButtonPressed = state.Value == 128 ? true : false;
                        break;
                    case JoystickOffset.X:
                        if (state.Value < 32767)
                        {
                            _currentUSBGGamePadState.DirectionRight = false;
                            _currentUSBGGamePadState.DirectionLeft = true;
                        }
                        else if (state.Value > 32767)
                        {
                            _currentUSBGGamePadState.DirectionLeft = false;
                            _currentUSBGGamePadState.DirectionRight = true;
                        }
                        else
                        {
                            _currentUSBGGamePadState.DirectionLeft = false;
                            _currentUSBGGamePadState.DirectionRight = false;
                        }
                        break;
                    case JoystickOffset.Y:
                        if (state.Value < 32767)
                        {
                            _currentUSBGGamePadState.DirectionUp = true;
                            _currentUSBGGamePadState.DirectionDown = false;
                        }
                        else if (state.Value > 32767)
                        {
                            _currentUSBGGamePadState.DirectionUp = false;
                            _currentUSBGGamePadState.DirectionDown = true;
                        }
                        else
                        {
                            _currentUSBGGamePadState.DirectionDown = false;
                            _currentUSBGGamePadState.DirectionUp = false;
                        }
                        break;
                }
            }
        }

        private void DetermineAnalogHorizontalMovement(GamePadState currentGamePadState, Microsoft.Xna.Framework.Input.KeyboardState currentKeyboardState)
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
                    this._isProne = false;
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
                            this._isProne = true;
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
                    this._isProne = false;
                    if (CurrentInputState.DirectionDown && 
                        ((this.BoundingBox().Bottom / CurrentStage.TileHeight) < CurrentStage.MapHeight-1))
                    {
                        // player is dropping down                    
                        this._dropInProgress = true;
                        this._ignoreNextPlatform = true;
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
                    (CurrentInputState.DirectionRight && this._isOnStairsRight) || 
                    (CurrentInputState.DirectionLeft && this._isOnStairsLeft)) && 
                    !this.MovingDownStairs && !this.MovingUpStairs && 
                    !this.StandingOnStairTop())
                {
                    MovingUpStairs = true;
                    _lastStairtopHeight = this.BoundingBox().Bottom;
                    if (this._isOnStairsLeft)
                        playerDirection = PlayerDirection.Left;
                    else
                        playerDirection = PlayerDirection.Right;
                    
                }
                #endregion

                #region Move Down Stairs if on stairs and pressing down
                if (this.IsOnStairs && 
                    (CurrentInputState.DirectionDown ||
                    (CurrentInputState.DirectionRight && this._isOnStairsLeft) || 
                    (CurrentInputState.DirectionLeft && this._isOnStairsRight)) &&
                    !this.MovingDownStairs && !this.MovingUpStairs)
                {
                    MovingDownStairs = true;
                    _lastStairtopHeight = this.BoundingBox().Bottom;
                    if (this._isOnStairsLeft)
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

                if (((!PreviousInputState.WeaponButtonPressed || Gun.Automatic) && CurrentInputState.WeaponButtonPressed) &&
                    // firing gun (note: player cannot fire gun while underwater)
                    !(this._isInWater && this._isProne))
                {
                    if (this.Gun.RecoilTimeRemaining <= 0)
                    {
                        if (this.playerDirection == Player.PlayerDirection.Right)
                            FireGun(new Vector2(this.WorldPosition.X + this.Width - 4, this.WorldPosition.Y + 18), Gun);
                        else
                            FireGun(new Vector2(this.WorldPosition.X + 4, this.WorldPosition.Y + 18), Gun);
                        
                    }
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
                        st = CurrentStage.getStageTileByWorldPosition(playerbounds.Center.X, playerbounds.Bottom);
                        this._isOnStairsLeft = st.CollisionType == StageTile.TileCollisionType.StairsBottomLeft;
                        this._isOnStairsRight = st.CollisionType == StageTile.TileCollisionType.StairsBottomRight;
                    }
                    else
                    {
                        this.MovingDownStairs = true;
                        st = CurrentStage.getStageTileByWorldPosition(playerbounds.Center.X, playerbounds.Bottom + 1);
                        this._isOnStairsLeft = st.CollisionType == StageTile.TileCollisionType.StairsLeft;
                        this._isOnStairsRight = st.CollisionType == StageTile.TileCollisionType.StairsRight;
                    }
                    

                    _lastStairtopHeight = this.BoundingBox().Bottom;
                }                
            }
            if (this.MovingUpStairs)
            {
                if (this._isOnStairsRight)
                {
                    this.WorldPosition.X += 1;
                    this.WorldPosition.Y -= 1;                    
                }
                else if (this._isOnStairsLeft)
                {
                    this.WorldPosition.X -= 1;
                    this.WorldPosition.Y -= 1;                    
                }
                if (this.BoundingBox().Bottom <= _lastStairtopHeight - _stairHeight)
                {
                    this.SecuringStairStep = true;
                    this.MovingDownStairs = false;
                    this.MovingUpStairs = false;
                    this.PreviousBottom = this.BoundingBox().Bottom;
                }
            }
            else if (this.MovingDownStairs)
            {
                if (this.BoundingBox().Bottom + 1 <= _lastStairtopHeight + _stairHeight)
                {
                    if (this._isOnStairsRight)
                    {
                        this.WorldPosition.X -= 1;
                        this.WorldPosition.Y += 1;
                    }
                    else if (this._isOnStairsLeft)
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
            this.WorldPosition.X = MathHelper.Clamp(this.WorldPosition.X, CurrentStage.CameraPosition.X, this.WorldPosition.X);

            if (!CurrentStage.AutoScroll)
            {
                if (this.WorldPosition.X > this.CurrentStage.CameraPosition.X + Game.iScreenModelWidth - (this.Width * 3))
                    CurrentStage.CameraPosition.X += this.WorldPosition.X - (this.CurrentStage.CameraPosition.X + Game.iScreenModelWidth - (this.Width * 3));

                if (this.WorldPosition.X > (this.CurrentStage.MapWidth * CurrentStage.TileWidth - Game.iScreenModelWidth ))
                {
                    if (CurrentStage.Game.CurrentGame == Game.GameType.Contra)
                        CurrentStage.AutoScroll = true;
                }
            }

            if (!IsDying || IsOnGround)
                this.CurrentInputState.Movement = 0.0f;
                        
        }
        public Vector2? FindNearbyStairbase()
        {
            int searchRange = CurrentStage.TileWidth;

            Rectangle playerbounds = this.BoundingBox(new Vector2(0f, 1f));
            Rectangle playerSearchbounds = new Rectangle(playerbounds.Left - searchRange , playerbounds.Top, playerbounds.Width + searchRange, playerbounds.Height);
            
            int leftTile = (int)Math.Floor((float)(playerSearchbounds.Left) / CurrentStage.TileWidth);
            leftTile = leftTile < 0 ? 0 : leftTile;
            int rightTile = (int)Math.Ceiling(((float)(playerSearchbounds.Right) / CurrentStage.TileWidth)) - 1;
            int bottomTile = (int)Math.Ceiling(((float)playerSearchbounds.Bottom / CurrentStage.TileHeight)) - 1;

            for (int x = leftTile; x <= rightTile; ++x)
            {
                StageTile stageTile = CurrentStage.getStageTileByGridPosition(x, bottomTile);
                if (stageTile.CollisionType == StageTile.TileCollisionType.StairsBottomLeft || stageTile.CollisionType == StageTile.TileCollisionType.StairsBottomRight)
                {
                    foreach(var platform in CurrentStage.getTilePlatformBoundsByGridPosition(x, bottomTile))
                    {
                        return new Vector2(platform.PlatformBounds.Center.X, platform.PlatformBounds.Bottom);                        
                    }
                }
            }

            return null;
        }
        public Vector2? FindNearbyStairtop()
        {
            int searchRange = CurrentStage.TileWidth;

            Rectangle playerbounds = this.BoundingBox(new Vector2(0f, 1f));
            Rectangle playerSearchbounds = new Rectangle(playerbounds.Left - searchRange, playerbounds.Top, playerbounds.Width + searchRange, playerbounds.Height);

            int leftTile = (int)Math.Floor((float)(playerSearchbounds.Left) / CurrentStage.TileWidth);
            leftTile = leftTile < 0 ? 0 : leftTile;
            int rightTile = (int)Math.Ceiling(((float)(playerSearchbounds.Right) / CurrentStage.TileWidth)) - 1;
            int bottomTile = (int)Math.Ceiling(((float)playerSearchbounds.Bottom / CurrentStage.TileHeight)) - 1;

            for (int x = leftTile; x <= rightTile; ++x)
            {
                StageTile stageTile = CurrentStage.getStageTileByGridPosition(x, bottomTile);                
                if (stageTile != null && stageTile.IsStairs())
                {
                    Rectangle? stairTopBounds = CurrentStage.getStairTopBoundsByGridPosition(x, bottomTile);
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
            this._isOnStairsLeft = false;
            this._isOnStairsRight = false;
            
            int leftTile = (int)Math.Floor((float)playerbounds.Left / CurrentStage.TileWidth);
            int rightTile = (int)Math.Ceiling(((float)playerbounds.Right / CurrentStage.TileWidth)) - 1;
            int topTile = (int)Math.Floor((float)playerbounds.Top / CurrentStage.TileHeight);
            int bottomTile = (int)Math.Ceiling(((float)playerbounds.Bottom / CurrentStage.TileHeight)) - 1;

            // For each potentially colliding platform tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    StageTile stageTile = CurrentStage.getStageTileByGridPosition(x, y);

                    if (stageTile != null)
                    {
                        if (stageTile.IsImpassable())
                        {
                            Rectangle tilebounds = CurrentStage.getTileBoundsByGridPosition(x, y);
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
                                List<Platform> tileboundsList = CurrentStage.getTilePlatformBoundsByGridPosition(x, bottomTile);
                                foreach (Platform platformBounds in tileboundsList)
                                {
                                    Rectangle tileBounds = platformBounds.PlatformBounds;
                                    Vector2 depth = RectangleExtensions.GetIntersectionDepth(playerbounds, tileBounds);

                                    if (this.PreviousBottom <= tileBounds.Top && Velocity.Y >= 0 && playerbounds.Intersects(tileBounds))
                                    //if (Velocity.Y >= 0 && (depth.Y < 0)) // || this.IgnoreNextPlatform))
                                    {
                                        if (this._ignoreNextPlatform == false)
                                        {
                                            this.JumpInProgress = false;
                                            this._dropInProgress = false;
                                            this.IsOnGround = true;
                                            this.SecuringStairStep = false;

                                            this.WorldPosition.Y += depth.Y;


                                            if (stageTile.CollisionType == StageTile.TileCollisionType.StairsLeft)
                                            {
                                                this._isOnStairsLeft = true;
                                                if (!this.StandingOnStairTop())
                                                    this.WorldPosition.X += tileBounds.Left - this.BoundingBox().Left;
                                            }
                                            else
                                            {
                                                this._isOnStairsRight = true;
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

                            List<Platform> platforms = CurrentStage.getTilePlatformBoundsByGridPosition(x, bottomTile);
                            foreach (Platform platform in platforms)
                            {
                                Rectangle tilebounds = platform.PlatformBounds;
                                Vector2 depth = RectangleExtensions.GetIntersectionDepth(playerbounds, tilebounds);

                                if (this.PreviousBottom <= tilebounds.Top && Velocity.Y >= 0 && playerbounds.Intersects(tilebounds))
                                //if (Velocity.Y >= 0 && (depth.Y < 0)) // || this.IgnoreNextPlatform))
                                {
                                    if (this._ignoreNextPlatform == false)
                                    {
                                        this.JumpInProgress = false;
                                        this._dropInProgress = false;
                                        this.IsOnGround = true;
                                        this.SecuringStairStep = false;
                                        if (stageTile.IsWaterPlatform())
                                            this._isInWater = true;
                                        else
                                            this._isInWater = false;

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
                this._ignoreNextPlatform = false;

            if (this.IsOnGround && !playerwasonground)
            {
                //Debug.WriteLine("Player Hit Ground " + gameTime.ElapsedGameTime.Seconds.ToString());
                _soundPlayerLand.Play();
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
            if (_isProne)
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


            return new Rectangle((int)position.X + iSpriteOffsetX, (int)position.Y + iSpriteOffsetTop, iFootWidth, _idle.FrameHeight - iSpriteOffsetBottom);
        }
        public Rectangle HurtBox()
        {
            if (this.JumpInProgress)
            {
                int iSpriteOffsetTop = 6;
                int iSpriteOffsetBottom = 30;
                int iSpriteOffsetX = 8;                
                Vector2 position = this.WorldPosition;

                return new Rectangle((int)position.X + iSpriteOffsetX, (int)position.Y + iSpriteOffsetTop, this.Width - (iSpriteOffsetX * 2), _idle.FrameHeight - iSpriteOffsetBottom);
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

            int leftTile = (int)Math.Floor((float)playerbounds.Left / CurrentStage.TileWidth);
            int rightTile = (int)Math.Ceiling(((float)playerbounds.Right / CurrentStage.TileWidth)) - 1;            
            int bottomTile = (int)Math.Ceiling(((float)playerbounds.Bottom / CurrentStage.TileHeight)) - 1;

            bool bReturnValue = false;
            
            for (int x = leftTile; x <= rightTile; ++x)
                {   
                    StageTile stageTile = CurrentStage.getStageTileByGridPosition(x, bottomTile);

                    if (stageTile.IsStairs())
                    {
                        if ((stageTile.CollisionType == StageTile.TileCollisionType.StairsLeft && !CurrentStage.getStageTileByGridPosition(x - 1, bottomTile - 1).IsStairs()) ||
                            (stageTile.CollisionType == StageTile.TileCollisionType.StairsRight && !CurrentStage.getStageTileByGridPosition(x + 1, bottomTile - 1).IsStairs()))
                        {
                            if (this.BoundingBox().Bottom == CurrentStage.getStairTopBoundsByGridPosition(x, bottomTile).Value.Top)
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
                if (0.0f < this.JumpTime && this.JumpTime <= MaxJumpTime)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocityY = launchVelocity * (1.0f - (float)Math.Pow(this.JumpTime / MaxJumpTime, Player.JumpControlPower));
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
            _soundDeath.Play();
            _deathAnimation.Play();
            
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

            // player loses special gun if they die.
            if (Gun.GunType != GunType.Standard)
            {
                Gun = new Gun();
                Gun.Initialize(_game.Content, GunType.Standard);
            }
        }
        private void FireGun(Vector2 position, Gun gun)
        {
            
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

            if (this._isProne)
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

            Vector2 gunBarrelLocation;

            gunBarrelLocation = new Vector2(position.X + fHorizontalOffset, position.Y + fVerticalOffset);

            var projectiles = gun.Fire(gunBarrelLocation, gunAngle, CurrentStage);            
            CurrentStage.Projectiles.AddRange(projectiles);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Vector2 torsoOffset;
            if (this.Gun.RecoilTimeRemaining > 0)
            {
                torsoOffset = new Vector2(0, 1);
            }
            else
            {
                torsoOffset = new Vector2(0, 0);
            }
            if (_deathAnimation.Active)
            {
                _deathAnimation.Draw(spriteBatch, playerDirection, 1f);
            }
            else if (JumpInProgress == true)
            {
                _jumpingAnimation.Draw(spriteBatch, playerDirection, 1f);
            }
            else if (_dropInProgress == true)
            {
                _dropping.Draw(spriteBatch, playerDirection, 1f);
            }
            else if (_isProne == true)
            {
                if (_isInWater)
                    _playermiscsprites.Draw(spriteBatch, playerDirection, 1f, PlayerSpriteCollection.PlayerSpriteTypes.Underwater);
                else
                    _prone.Draw(spriteBatch, playerDirection, 1f, torsoOffset);
            }
            else if (_isInWater)
            {
                _playermiscsprites.Draw(spriteBatch, playerDirection, 1f, PlayerSpriteCollection.PlayerSpriteTypes.Wading, torsoOffset);
            }
            else
            {
                if (Math.Abs(Velocity.X) > 0)
                {
                    _runningLegsAnimation.Draw(spriteBatch, playerDirection, 0.9f);
                    switch (gunDirection)
                    {
                        case GunDirection.High:
                            _playermiscsprites.Draw(spriteBatch, playerDirection, 1f, PlayerSpriteCollection.PlayerSpriteTypes.GunHigh, torsoOffset);
                            break;
                        case GunDirection.Low:
                            _playermiscsprites.Draw(spriteBatch, playerDirection, 1f, PlayerSpriteCollection.PlayerSpriteTypes.GunLow, torsoOffset);
                            break;
                        default:
                            if (this.Gun.RecoilTimeRemaining > 0)
                            {
                                _playermiscsprites.Draw(spriteBatch, playerDirection, 1.0f, PlayerSpriteCollection.PlayerSpriteTypes.GunNeutral, torsoOffset);
                            }
                            else
                            {
                                _runningTorsoAnimation.Draw(spriteBatch, playerDirection, 1f, torsoOffset);                                
                            }
                            break;
                    }
                }
                else
                {
                    if (gunDirection == GunDirection.StraightUp)
                    {
                        _idlelegs.Draw(spriteBatch, playerDirection, 0.9f);
                        _playermiscsprites.Draw(spriteBatch, playerDirection, 1.0f, PlayerSpriteCollection.PlayerSpriteTypes.GunStraightUp, torsoOffset);
                    }
                    else
                    {
                        _idlelegs.Draw(spriteBatch, playerDirection, 0.9f);
                        _playermiscsprites.Draw(spriteBatch, playerDirection, 1.0f, PlayerSpriteCollection.PlayerSpriteTypes.GunNeutral, torsoOffset);
                        //idle.Draw(spriteBatch, playerDirection, 1f);
                    }
                }
            }

            base.Draw(spriteBatch);

        }

    }
}
