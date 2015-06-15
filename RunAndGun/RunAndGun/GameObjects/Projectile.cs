// Projectile.cs
//Using declarations
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using RunAndGun.Actors;
using System.Collections.Generic;
using RunAndGun.Animations;

namespace RunAndGun.GameObjects
{
    public class Projectile
    {
        // Image representing the Projectile
        //public Texture2D Texture;
        private Animation _image = null;                
        
        public virtual Animation Image()
        {
            return _image;            
        }
        private SoundEffect soundProjectileHit;
        private Stage currentStage;

        // Position of the Projectile relative to the upper left side of the screen
        public Vector2 WorldPosition;
        public Vector2 ScreenPosition
        {
            get { return WorldPosition - currentStage.CameraPosition; }
        }

        // State of the Projectile
        public bool Active;

        // The amount of damage the projectile can inflict to an enemy
        public int Damage;

        public Vector2 Velocity;
        private float projectileSpeed = 3.5f;

        
        public int Width()
        {
            return Image().FrameWidth;
            
        }

        public int Height()
        {
            return Image().FrameHeight;            
        }


        //public void Initialize(Texture2D texture, SoundEffect hitsound, Vector2 position, int angle, Stage stage, float p_projectileSpeed)
        //{

        //    double radians = (Math.PI / 180) * angle;

            
        //    //Image = new Animation();

        //    //Texture = texture;
        //    ImageTexture = texture;
        //    soundProjectileHit = hitsound;
        //    currentStage = stage;
        //    WorldPosition = position;

        //    //Image.Initialize(texture, position, 3, 10, Color.White, 1, true, currentStage);

        //    Velocity = new Vector2((float)Math.Sin(radians), -(float)Math.Cos(radians)) * p_projectileSpeed;
        //    projectileSpeed = p_projectileSpeed;

        //    Active = true;

        //    Damage = 2;
        //}
        public void Initialize(Animation animation, SoundEffect hitsound, Vector2 position, int angle, Stage stage, float p_projectileSpeed)
        {

            double radians = (Math.PI / 180) * angle;


            _image = animation;
            soundProjectileHit = hitsound;
            currentStage = stage;
            WorldPosition = position;            

            Velocity = new Vector2((float)Math.Sin(radians), -(float)Math.Cos(radians)) * p_projectileSpeed;
            projectileSpeed = p_projectileSpeed;

            Active = true;

            Damage = 2;
        }
        public void PlayHitSound()
        {
            soundProjectileHit.Play();
        }
        //public void Initialize(Texture2D texture, Vector2 position, Player player, Stage stage)
        //{
        //    Image = new Animation();
            
        //    //Texture = texture;
        //    currentStage = stage;
        //    WorldPosition = position;

        //    Image.Initialize(texture, position, 3, 10, Color.White, 1, true, currentStage);

        //    if (player.gunDirection == Player.GunDirection.StraightUp)
        //        Velocity = new Vector2(0, -projectileSpeed);
        //    else if (player.gunDirection == Player.GunDirection.StraightDown)
        //        Velocity = new Vector2(0, projectileSpeed);
        //    else
        //    {
        //        int directionFactor;
        //        if (player.playerDirection == Player.PlayerDirection.Right)
        //            directionFactor = 1;
        //        else
        //            directionFactor = -1;

        //        switch (player.gunDirection)
        //        {
        //            case Player.GunDirection.High:
        //                Velocity = new Vector2(projectileSpeed * directionFactor, -((projectileSpeed * 2) / 3));
        //                break;
        //            case Player.GunDirection.Low:
        //                Velocity = new Vector2(projectileSpeed * directionFactor, ((projectileSpeed * 2) / 3));
        //                break;
        //            default:
        //                Velocity = new Vector2(projectileSpeed * directionFactor, 0);
        //                break;
        //        }
        //    }

            
        //    Active = true;

        //    Damage = 2;

        //}

        public void Update(GameTime gameTime, Vector2 cameraPosition)
        {
            WorldPosition += Velocity;

            Image().Update(gameTime);
            Image().WorldPosition = WorldPosition;
            //if (Direction == Player.GunDirection.Right)
            //    Position.X += projectileMoveSpeed;
            //else if (Direction == Player.GunDirection.Left)
            //    Position.X -= projectileMoveSpeed;

            // Deactivate the bullet if it goes out of screen

            if (ScreenPosition.X < 0 || ScreenPosition.Y < 0 || ScreenPosition.X > Game.iScreenModelWidth || ScreenPosition.Y > Game.iScreenModelHeight)
                Active = false;            
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            Image().Draw(spriteBatch, Player.PlayerDirection.Right, 1);            
        }
    }
}
