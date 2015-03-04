using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;


namespace RunAndGun.Actors
{
    class Turret : Enemy
    {

        public float AngleBetweenTurretAndPlayer;

        private int iCurrentPosition;
        public enum TurretDirection
        { Clockwise = 1, CounterClockWise = -1, TargetFound = 0 };
        public TurretDirection TurretTurnDirection;
        public float TimeSinceLastMove;
        public float TimeTargetLocked;
        private const float TurretMoveDelay = 0.5f;
        private const float TurretFireDelay = 1.0f;
        private const int frameTime = 100;
        private int elapsedTime;
        private bool animatingforward;
        private int currentFrame;
        

        List<PlayerSpriteCollection> spritecollectionlist;
        Texture2D projectileTexture;
        

        public Turret(ContentManager content, Vector2 position, Stage stage, string enemytype)
            : base(content, position, stage, enemytype)
        {

            PlayerSpriteCollection spritecollection;
            Texture2D turrettileset;

            spritecollectionlist = new List<PlayerSpriteCollection>();
            turrettileset = content.Load<Texture2D>("Sprites/Enemies/Turret");

            spritecollection = new PlayerSpriteCollection();
            spritecollection.Initialize(swapColor(turrettileset, new Color(192, 32, 0), new Color(184, 28, 12)), position, 12, Color.White, 1f);
            spritecollectionlist.Add(spritecollection);

            spritecollection = new PlayerSpriteCollection();
            spritecollection.Initialize(swapColor(turrettileset, new Color(192, 32, 0), new Color(228, 68, 52)), position, 12, Color.White, 1f);
            spritecollectionlist.Add(spritecollection);            

            spritecollection = new PlayerSpriteCollection();
            spritecollection.Initialize(turrettileset, position, 12, Color.White, 1f);
            spritecollectionlist.Add(spritecollection);
            
            CollisionIsHazardous = false;

            Health = 15;
            iCurrentPosition = 3;
            TimeTargetLocked = 0.0f;
            elapsedTime = 0;
            currentFrame = 0;
            animatingforward = true;

            // RGB colors for blinking animations of turret.
            //R 255 228  184 
            //G 140 68   28
            //B 124 52   12

            projectileTexture = content.Load<Texture2D>("Sprites/basicbullet");

            ExplosionAnimation.Initialize(content.Load<Texture2D>("Sprites/Explosion2"), WorldPosition, 5, 150, Color.White, 1f, false, this.currentStage);
            ExplosionSound = content.Load<SoundEffect>("Sounds/Explosion2");

        }

        private Texture2D swapColor(Texture2D thisTexture, Color searchColor, Color replaceColor)
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

        public override void Move(GameTime gameTime)
        {

            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            TimeSinceLastMove += elapsedTime;

                
            // do nothing 
            Player nearestPlayer = null;
            foreach (Player player in currentStage.Players)
            {
                if (nearestPlayer == null)
                    nearestPlayer = player;
                else if ((this.WorldPosition - nearestPlayer.WorldPosition).Length() > (this.WorldPosition - player.WorldPosition).Length())
                    // TODO: test and see if this actually works!
                    nearestPlayer = player;
            }

            double d = Math.Atan2(nearestPlayer.WorldPosition.X - this.WorldPosition.X, -(nearestPlayer.WorldPosition.Y - this.WorldPosition.Y));
            AngleBetweenTurretAndPlayer = MathHelper.ToDegrees((float)d);
            if (AngleBetweenTurretAndPlayer < 0)
                AngleBetweenTurretAndPlayer = 360 + AngleBetweenTurretAndPlayer;

            AngleBetweenTurretAndPlayer = (float)Math.Round(AngleBetweenTurretAndPlayer / 30);

            if (AngleBetweenTurretAndPlayer != iCurrentPosition)
            {
                if (TimeSinceLastMove > TurretMoveDelay)
                {
                    TimeSinceLastMove -= TurretMoveDelay;
            
                    //float currentTurretAngle = iCurrentPosition * 30;
                    int iCounterDistance = 0;
                    if (iCurrentPosition < AngleBetweenTurretAndPlayer)
                        iCounterDistance = 12 - (int)AngleBetweenTurretAndPlayer + iCurrentPosition;
                    else
                        iCounterDistance = iCurrentPosition - (int)AngleBetweenTurretAndPlayer;

                    if (iCounterDistance < 6)
                        TurretTurnDirection = TurretDirection.CounterClockWise;
                    else
                        TurretTurnDirection = TurretDirection.Clockwise;

                    iCurrentPosition += (int)TurretTurnDirection;
                    if (iCurrentPosition == 12)
                        iCurrentPosition = 0;
                    else if (iCurrentPosition == -1)
                        iCurrentPosition = 11;
                }

            }
            else
            {
                TurretTurnDirection = TurretDirection.TargetFound;
                TimeTargetLocked += elapsedTime;
                if (TimeTargetLocked > TurretFireDelay)
                {
                    TimeTargetLocked = 0.0f;
                    AddProjectile(this.WorldPosition);
                }
            }
            
            //float f = AngleBetweenTurretAndPlayer - currentTurretAngle;

            //currentTurretDirection = (TurretDirection) Math.Sign(-f % 360 - f % 360);


        }

        private void AddProjectile(Vector2 position)
        {
            Projectile projectile = new Projectile();
            Vector2 gunBarrelLocation;

            float fHorizontalOffset = 0.0f;
            float fVerticalOffset = 0.0f;

            gunBarrelLocation = new Vector2(position.X + fHorizontalOffset, position.Y + fVerticalOffset);

            Rectangle bb = this.BoundingBox();
            
            projectile.Initialize(projectileTexture, null, new Vector2(bb.Center.X, bb.Center.Y), iCurrentPosition * 30, currentStage, 2f);
            currentStage.EnemyProjectiles.Add(projectile);
        }
        
        public override void ApplyPhysics(GameTime gameTime)
        {
            // do nothing- Turret is fixed to the Stage.
        }

        protected override void UpdateAnimations(GameTime gameTime)
        {

            elapsedTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (elapsedTime > frameTime)
            {
                // Move to the next frame
                if (animatingforward)
                {
                    currentFrame++;
                    if (currentFrame == spritecollectionlist.Count)
                    {
                        currentFrame = spritecollectionlist.Count - 1;
                        animatingforward = false;
                        
                    }
                }
                else
                {
                    currentFrame--;
                    if (currentFrame == 0)
                    {

                        animatingforward = true;

                        currentFrame = 0;

                    }
                }

                // Reset the elapsed time to zero
                elapsedTime = 0;
            }

            foreach(PlayerSpriteCollection psc in spritecollectionlist)
                psc.ScreenPosition = ScreenPosition;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            int iCurrentFrame = 0;
            if (iCurrentPosition < 0)
                iCurrentFrame = 12 + iCurrentPosition;
            else
                iCurrentFrame = iCurrentPosition;

            spritecollectionlist[currentFrame].Draw(spriteBatch, this.direction, 1f, iCurrentFrame);
            base.Draw(spriteBatch);

        }



    }
}
