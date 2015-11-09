using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using RunAndGun.Animations;

namespace RunAndGun.GameObjects
{
    class StageBridge : StageObject
    {
        private List<StageTile> bridgepieces = new List<StageTile>();
        private SoundEffect _explosionSound;
        private SoundEffectInstance _explosionSoundInstance;
        private Texture2D _explosionAnimationStrip;
        public bool Exploding;
        private int _explosionPlayCount = 0;
        
        public void InitializeBridge(ContentManager content, Point coordinates, int BridgeLength)
        {
            StageTile bridgestart = CurrentStage.getStageTileByWorldPosition(coordinates.X, coordinates.Y);
            //for (int i = 0; i < currentStage.; i++)            
            for (int i = 0; i < BridgeLength; i++)
            {
                bridgepieces.Add(CurrentStage.getStageTileByGridPosition(bridgestart.X + i, bridgestart.Y));
            }

            _explosionSound = content.Load<SoundEffect>("Sounds/Explosion2");
            _explosionAnimationStrip = content.Load<Texture2D>("Sprites/Explosion2");

        }

        public override void Update(CVGameTime gameTime)
        {
            base.Update(gameTime);

            // as soon as a player moves past the bridge horizontally, have the bridge explode. 
            // occurs even if player is not standing on the bridge.
            foreach (Actors.Player player in CurrentStage.Players)
            {
                if (this.WorldPosition.X < player.WorldPosition.X && Exploding == false)
                    Explode();
            }

            if (Exploding)
            {
                if (_explosionSoundInstance.State == SoundState.Stopped)
                {
                    _explosionPlayCount++;
                    if (_explosionPlayCount < 4)
                    {
                        ExplodeBridgePiece(_explosionPlayCount);
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
            Vector2 explosionLocation = new Vector2(this.WorldPosition.X + (iPiece * CurrentStage.TileWidth), this.WorldPosition.Y );
            explosion.Initialize(_explosionAnimationStrip, explosionLocation, 32, 32, 5, 150, Color.White, 1f, false, false, CurrentStage);
            CurrentStage.AddExplosion(explosionLocation, explosion, _explosionSound);

            _explosionSoundInstance = _explosionSound.CreateInstance();
            _explosionSoundInstance.Play();
            
        }
    }
}
