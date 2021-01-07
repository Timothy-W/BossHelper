using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExileCore.PoEMemory.MemoryObjects;
using SharpDX;

namespace BossHelper
{
    class FloatingIcon
    {
        private uint _iconHeight = 30;
        private uint _iconWidth = 30;
        private int _iconZOffset;
        private bool _skipDrawing = true;
        
        private Entity _entity;
        private RectangleF _imageRect;



        //public int IconZOffset { get; set; } // make adjustable
        //public static int IconWidth { get; set; } 
        //public static int IconHeight { get; set; }

        public bool SkipDrawing { get => _skipDrawing; set => _skipDrawing = value; }
        public Entity Entity { get => _entity; }
        public RectangleF ImageRect { get => _imageRect; set => _imageRect = value; }

        public uint IconHeight
        {
            get => _iconHeight;
            set => _iconHeight = value;
        }

        public uint IconWidth
        {
            get => _iconWidth;
            set => _iconWidth = value;
        }

        public int IconZOffset
        {
            get => _iconZOffset;
            set => _iconZOffset = value;
        }


        public Vector2 ScreenPosition;
        public bool EntityIsOffScreen;

        private readonly string _entitName;
        private readonly uint _entityId;



        /// <summary>
        /// Floating Icon for given entity.
        /// </summary>
        /// <param name="entity"></param>
        public FloatingIcon(Entity entity, int zVal)
        {
            _entity = entity;
            IconZOffset = zVal;
            _entitName = entity.RenderName;
            _entityId = entity.Id;

            EntityIsOffScreen = isEntityOffScreen();
        }

        private bool isEntityOffScreen()
        {
            return false;
        }
    }
}
