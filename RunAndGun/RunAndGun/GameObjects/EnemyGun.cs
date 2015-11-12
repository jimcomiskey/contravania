using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RunAndGun.Actors;
using RunAndGun.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RunAndGun.GameObjects
{
    public class EnemyGun
    {
        private Enemy _owner;
        private Texture2D _projectileTexture;
        public EnemyGun(Enemy owner, Texture2D projectileTexture)
        {
            _owner = owner;
            _projectileTexture = projectileTexture;
        }
        public void AddProjectile(Stage currentStage, Vector2 position, int angle, float projectileSpeed)
        {
            Projectile projectile = new Projectile();
            Rectangle bb = _owner.BoundingBox();

            Vector2 initPosition = new Vector2(bb.Center.X, bb.Center.Y);

            var projectileAnimation = new Animation();
            projectileAnimation.Initialize(_projectileTexture, position, 1, 0, Color.White, 1f, true, currentStage);
            projectile.Initialize(projectileAnimation, null, position, angle, currentStage, projectileSpeed);
            currentStage.EnemyProjectiles.Add(projectile);
        }
    }
}
