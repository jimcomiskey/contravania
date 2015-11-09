﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.Xml;
using RunAndGun.GameObjects;

namespace RunAndGun.Actors
{
    public abstract class Enemy : Actors.Actor
    {
        
        protected RunAndGun.Actors.Player.PlayerDirection direction;

        protected float EnemyMoveSpeed;

        public int Health;

        public bool Active;
        public bool CollisionIsHazardous;
        public bool VulnerableToBullets;
        public bool BulletProof;

        public Enemy(ContentManager content, Vector2 position, Stage stage, string enemytype)
        {
            Name = enemytype;
            Active = true;
            Health = 1;
            CollisionIsHazardous = true;
            VulnerableToBullets = true;
            direction = Player.PlayerDirection.Left;
            EnemyMoveSpeed = 0.0f;

            WorldPosition = position;
            CurrentStage = stage;            
        }
        

        protected abstract void UpdateAnimations(CVGameTime gameTime);

        public void Update(CVGameTime gameTime)
        {
            
            UpdateAnimations(gameTime);

            Move(gameTime);

            ApplyPhysics(gameTime);

            // if enemy goes off-screen or health runs out, deactivate
            if (this.BoundingBox().Right < CurrentStage.CameraPosition.X || this.BoundingBox().Left > CurrentStage.CameraPosition.X + Game.iScreenModelWidth + (this.BoundingBox().X - this.WorldPosition.X) || Health <= 0)
            {   Active = false;                
            }

        }
        
        protected void ChangeDirection()
        {
            Velocity.X = -Velocity.X;

            if (Math.Abs((int)Velocity.X) > 0)
            {
                if (this.direction == Player.PlayerDirection.Right)
                    this.direction = Player.PlayerDirection.Left;
                else
                    this.direction = Player.PlayerDirection.Right;
            }
        }

        public override Rectangle BoundingBox()
        {
            return BoundingBox(this.WorldPosition);
        }
        public abstract Rectangle BoundingBox(Vector2 proposedPosition);        

        public virtual bool SpawnConditionsMet()
        {
            return this.BoundingBox().Intersects(CurrentStage.ScreenCoordinates());
        }
        
        public override void Die(CVGameTime gameTime)
        {
            Active = false;
            // do nothing 
        }
        
        public override int Width
        {
            get { return this.BoundingBox().Width; }
        }

    }
}
