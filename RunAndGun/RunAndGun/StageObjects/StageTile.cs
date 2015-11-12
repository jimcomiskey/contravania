using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RunAndGun
{
    public class StageTile
    {
        // coordinates of tile on map
        public int X = 0;
        public int Y = 0;

        // different GID layers being managed by the tile
        public int BackgroundGID = 0;
        public int MetaGID = 0;
        public int DestructionLayer1GID = 0;
        public int DestructionLayer2GID = 0;
        public string PortalID;

        public enum TileStatus { Active, BeingDestroyed, Destroyed}
        public TileStatus Status = TileStatus.Active;

        public enum TileCollisionType { None, Impassable, Platform, PlatformHalfDrop, PlatformWater, StairsLeft, StairsRight, StairsBottomRight, StairsBottomLeft }
        public TileCollisionType CollisionType = TileCollisionType.None;
        
        public bool IsImpassable()
        {
            if (CollisionType == TileCollisionType.Impassable)
                return true;
            else
                return false;
        }
        public bool IsStairs()
        {
            if (CollisionType == TileCollisionType.StairsLeft || CollisionType == TileCollisionType.StairsRight)
                return true;
            else
                return false;
        }
        public bool IsPlatform()
        {
            if (CollisionType == StageTile.TileCollisionType.Platform || 
                CollisionType == StageTile.TileCollisionType.PlatformHalfDrop || 
                CollisionType == StageTile.TileCollisionType.PlatformWater || 
                CollisionType == StageTile.TileCollisionType.StairsBottomLeft || 
                CollisionType == StageTile.TileCollisionType.StairsBottomRight)
                return true;
            else
                return false;
        }
        public bool IsWaterPlatform()
        {
            if (CollisionType == TileCollisionType.PlatformWater)
                return true;
            else
                return false;
        }
    }
}
