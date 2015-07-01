using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RunAndGun.Actors;

namespace RunAndGun.StageObjects
{
    public class EnemySpawnLocation : StageObject
    {
        private string _enemyType;
        private double _enemySpawnTime;
        private Enemy _spawnedEnemy;
        
        public EnemySpawnLocation(string enemyType)
        {
            _enemyType = enemyType;
            _enemySpawnTime = 0;
        }
        public override void Update(GameTime gameTime)
        {
            Random r;
            if (_spawnedEnemy != null && _spawnedEnemy.Active == false)
            {
                _spawnedEnemy = null;
                r = new Random();
                _enemySpawnTime = r.Next(200, 2000);
            }
            if (_spawnedEnemy == null && CurrentStage.ScreenCoordinates().Intersects(VirtualBox()) && VirtualBox().Right > CurrentStage.ScreenCoordinates().Right)
            {
                _enemySpawnTime -= gameTime.ElapsedGameTime.TotalMilliseconds;

                if (_enemySpawnTime < 0)
                {
                    Vector2 spawnLocation =
                    new Vector2(this.VirtualBox().Left + (this.CurrentStage.ScreenCoordinates().Right - this.VirtualBox().Left),
                    VirtualBox().Bottom);

                    CurrentStage.AddEnemy(_enemyType, spawnLocation);

                    r = new Random();
                    // TODO: rework this so that enemies spawn more similarly to classic Contra
                    _enemySpawnTime = Math.Sqrt(r.Next(50000, 4000000));

                }
                
                base.Update(gameTime);
            }
        }
        public Rectangle VirtualBox()
        {
            return new Rectangle((int)WorldPosition.X, (int)WorldPosition.Y, this.Width, this.Height);
        }
    }
}
