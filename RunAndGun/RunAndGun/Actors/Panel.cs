using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using RunAndGun.Animations;
using Microsoft.Xna.Framework.Audio;
using RunAndGun.GameObjects;

namespace RunAndGun.Actors
{
    class Panel : Enemy
    {
        List<PlayerSpriteCollection> spritecollectionlist;

        private int _elapsedGlowTime;
        private const int FrameGlowTime = 100;
        private bool _glowAnimatingforward;
        private int _currentGlowFrame;

        private int _elapsedOpenCloseTime;
        private const int FrameOpenCloseTime = 1100;
        private const int FrameOpenCloseAnimationTime = 300;
        private int _currentOpenCloseFrame;
        private bool _opening;

        private string _itemType;

        private ContentManager _contentManager;

        public Panel(ContentManager content, Vector2 position, Stage stage, string itemType) 
            : base(content, position, stage, "Panel")
        {
            _contentManager = content;
            _health = 1;
            _currentGlowFrame = 0;
            _glowAnimatingforward = true;
            CollisionIsHazardous = false;

            PlayerSpriteCollection spritecollection;
            Texture2D turrettileset;

            spritecollectionlist = new List<PlayerSpriteCollection>();
            turrettileset = content.Load<Texture2D>("Sprites/Enemies/Panel");
            ExplosionAnimation.Initialize(content.Load<Texture2D>("Sprites/Explosion2"), WorldPosition, 5, 150, Color.White, 1f, false, this.CurrentStage);
            ExplosionSound = content.Load<SoundEffect>("Sounds/Explosion2");

            spritecollection = new PlayerSpriteCollection();
            spritecollection.Initialize(SwapColor(turrettileset, new Color(192, 32, 0), new Color(184, 28, 12)), position, 3, Color.White, 1f);
            spritecollectionlist.Add(spritecollection);

            spritecollection = new PlayerSpriteCollection();
            spritecollection.Initialize(SwapColor(turrettileset, new Color(192, 32, 0), new Color(228, 68, 52)), position, 3, Color.White, 1f);
            spritecollectionlist.Add(spritecollection);

            spritecollection = new PlayerSpriteCollection();
            spritecollection.Initialize(turrettileset, position, 3, Color.White, 1f);
            spritecollectionlist.Add(spritecollection);

            _elapsedOpenCloseTime = FrameOpenCloseTime;

            _itemType = itemType;
        }
        public override Rectangle BoundingBox(Vector2 proposedPosition)
        {
            int iBoundingBoxTopOffset;
            int iBoundingBoxBottomOffset;
            int iBoundingBoxLeftOffset;
            int iBoundingBoxRightOffset;

            iBoundingBoxTopOffset = 7;
            iBoundingBoxBottomOffset = 7;
            iBoundingBoxLeftOffset = 7;
            iBoundingBoxRightOffset = 7;

            EnemyMoveSpeed = 0.0f;

            return new Rectangle((int)proposedPosition.X + iBoundingBoxLeftOffset, (int)proposedPosition.Y + iBoundingBoxTopOffset, spritecollectionlist[0].FrameWidth - iBoundingBoxRightOffset - iBoundingBoxLeftOffset, spritecollectionlist[0].FrameHeight - iBoundingBoxBottomOffset - iBoundingBoxTopOffset);
        }
        
        public override void Move(CVGameTime gameTime)
        {
            _elapsedOpenCloseTime += gameTime.ElapsedGameTime.Milliseconds;

            if (_elapsedOpenCloseTime > FrameOpenCloseTime && (_currentOpenCloseFrame == 0 || _currentOpenCloseFrame == 2))
            {
                _elapsedOpenCloseTime -= FrameOpenCloseTime;
                _currentOpenCloseFrame = 1;
                _opening = !_opening;                
            }            
            else if (_elapsedOpenCloseTime > FrameOpenCloseAnimationTime && _currentOpenCloseFrame == 1)
            {
                _elapsedOpenCloseTime -= FrameOpenCloseAnimationTime;
                _currentOpenCloseFrame += (_opening ? 1 : -1);
            }

            BulletProof = _currentOpenCloseFrame > 0 ? false : true;            
            
        }
        public override void ApplyPhysics(CVGameTime gameTime)
        {
            // do nothing - Panel is anchored to the stage
        }
        public override void Die(CVGameTime gameTime)
        {
            ExplosionSound.Play();
            var item = new PlayerItem(_contentManager, WorldPosition, CurrentStage, _itemType);
            CurrentStage.ActiveEnemies.Add(item);

            base.Die(gameTime);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spritecollectionlist[_currentGlowFrame].Draw(spriteBatch, this.direction, 1f, _currentOpenCloseFrame);
            base.Draw(spriteBatch);
        }

        protected override void UpdateAnimations(CVGameTime gameTime)
        {
            _elapsedGlowTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_elapsedGlowTime > FrameGlowTime)
            {
                // Move to the next frame
                if (_glowAnimatingforward)
                {
                    _currentGlowFrame++;
                    if (_currentGlowFrame == spritecollectionlist.Count)
                    {
                        _currentGlowFrame = spritecollectionlist.Count - 1;
                        _glowAnimatingforward = false;

                    }
                }
                else
                {
                    _currentGlowFrame--;
                    if (_currentGlowFrame == 0)
                    {

                        _glowAnimatingforward = true;

                        _currentGlowFrame = 0;

                    }
                }

                // Reset the elapsed time to zero
                _elapsedGlowTime = 0;
            }

            foreach (PlayerSpriteCollection psc in spritecollectionlist)
                psc.ScreenPosition = ScreenPosition;
        }

        private Texture2D SwapColor(Texture2D thisTexture, Color searchColor, Color replaceColor)
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
    }
}
