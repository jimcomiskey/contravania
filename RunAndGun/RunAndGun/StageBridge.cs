using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;


namespace RunAndGun
{
    class StageBridge : StageObject
    {
        private List<StageTile> bridgepieces = new List<StageTile>();
        private SoundEffect explosionSound;
        private SoundEffectInstance explosionSoundInstance;
        private Texture2D explosionAnimationStrip;
        public bool Exploding;
        private int explosionPlayCount = 0;
        
        public void InitializeBridge(ContentManager content, Point coordinates, int BridgeLength)
        {
            StageTile bridgestart = currentStage.getStageTileByWorldPosition(coordinates.X, coordinates.Y);
            //for (int i = 0; i < currentStage.; i++)            
            for (int i = 0; i < BridgeLength; i++)
            {
                bridgepieces.Add(currentStage.getStageTileByGridPosition(bridgestart.X + i, bridgestart.Y));
            }

            explosionSound = content.Load<SoundEffect>("Sounds/Explosion2");
            explosionAnimationStrip = content.Load<Texture2D>("Sprites/Explosion2");

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // as soon as a player moves past the bridge horizontally, have the bridge explode. 
            // occurs even if player is not standing on the bridge.
            foreach (Actors.Player player in currentStage.Players)
            {
                if (this.WorldPosition.X < player.WorldPosition.X && Exploding == false)
                    Explode();
            }

            if (Exploding)
            {
                if (explosionSoundInstance.State == SoundState.Stopped)
                {
                    explosionPlayCount++;
                    if (explosionPlayCount < 4)
                    {
                        ExplodeBridgePiece(explosionPlayCount);
                    }
                    else
                    {
                        Active = false;
                    }
                }
            }
        }
        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public void Explode() 
        {
            ExplodeBridgePiece(0);
            Exploding = true;

        }
        private void ExplodeBridgePiece(int iPiece)
        {
            bridgepieces[iPiece].Status = StageTile.TileStatus.Destroyed;
            bridgepieces[iPiece].MetaGID = 0;
            bridgepieces[iPiece].BackgroundGID = 0;
            bridgepieces[iPiece].CollisionType = StageTile.TileCollisionType.None;

            Animation explosion = new Animation();
            Vector2 explosionLocation = new Vector2(this.WorldPosition.X + (iPiece * currentStage.iTileWidth), this.WorldPosition.Y );
            explosion.Initialize(explosionAnimationStrip, explosionLocation, 32, 32, 5, 150, Color.White, 1f, false, false, currentStage);
            currentStage.AddExplosion(explosionLocation, explosion, explosionSound);

            explosionSoundInstance = explosionSound.CreateInstance();
            explosionSoundInstance.Play();
            
        }
    }
}
