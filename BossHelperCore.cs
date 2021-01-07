using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using SharpDX;


namespace BossHelper
{
    public class BossHelperCore : BaseSettingsPlugin<BossHelperSettings>
    {
        private Camera camera;
        private Coroutine bossHelperCoroutine;
        private bool CanTick = true;

        private ConcurrentDictionary<uint, Entity> trackedEntities = new ConcurrentDictionary<uint, Entity>();
        private RectangleF windowRectangle;
        private Size2F windowSize;
        private uint coroutineCounter;
        private Entity player;
        private FloatingIcon playerIcon;
        

        //private readonly Stopwatch DebugTimer = new Stopwatch();

        public BossHelperCore()
        {
            Name = "BossHelper";
        }

        


        public override void OnLoad()
        {
            //CanUseMultiThreading = true; //Enable multi threading here when we implement it
        }





        public override bool Initialise()
        {
            DebugWindow.LogMsg("Loading assets.");
            var skullIcon = Path.Combine(DirectoryFullName, "skull_icon.png").Replace('\\', '/');
            Graphics.InitImage(skullIcon, false);


            windowRectangle = GameController.Window.GetWindowRectangleReal();
            //windowSize = new Size2F(windowRectangle.Width / 2560, windowRectangle.Height / 1600);
            camera = GameController.Game.IngameState.Camera;
            player = GameController.Player;



           
            //playerIcon = new FloatingIcon(player, Settings.FloatingIconZValue);
            //
            //GameController.EntityListWrapper.PlayerUpdate += (sender, args) =>
            //{
            //    player = GameController.Player;
            //
            //    playerIcon = new FloatingIcon(player, Settings.FloatingIconZValue);
            //};



            //bossHelperCoroutine = new Coroutine(MainWorkCoroutine(), this, "Boss Helper");
            //Core.ParallelRunner.Run(bossHelperCoroutine);
            //bossHelperCoroutine.Pause();


            var initialEntities = GameController.EntityListWrapper.OnlyValidEntities.ToArray();

            foreach (var entity in initialEntities)
            {
                EntityAdded(entity);
            }

            return true;
        }


        /// <summary>
        /// Main coroutine used for multi threading support.
        /// TODO: Implement multi threading.
        /// </summary>
        /// <returns></returns>
        private IEnumerator MainWorkCoroutine()
        {
            while (true)
            {
                //yield return FindItemToPick();

                coroutineCounter++;
                bossHelperCoroutine.UpdateTicks(coroutineCounter);
                //yield return _workCoroutine;
            }
        }


        /// <summary>
        /// Method is called by HUD to perform plugin behavior.
        /// </summary>
        /// <returns></returns>
        public override Job Tick()
        {
            //if (bossHelperCoroutine.IsDone)
            //{
            //    var firstOrDefault = Core.ParallelRunner.Coroutines.FirstOrDefault(x => x.OwnerName == nameof(BossHelperCore));
            //
            //    if (firstOrDefault != null)
            //        bossHelperCoroutine = firstOrDefault;
            //}
            //
            //bossHelperCoroutine.Resume();


            //DrawDebugInfo();
            windowRectangle = GameController.Window.GetWindowRectangleReal();
            return  new Job(nameof(BossHelperCore), TickLogic);

        }

        /// <summary>
        /// Method draws debugging info.
        /// </summary>
        private void DrawDebugInfo()
        {
            Graphics.DrawBox(new RectangleF(0, 0, 20, 20), Color.Red);
            Graphics.DrawBox(new RectangleF(windowRectangle.Width - 20, windowRectangle.Height - 20, 20, 20), Color.Red);
            Graphics.DrawFrame(new RectangleF(0, 0, windowRectangle.Width, windowRectangle.Height), Color.Red, 1);

            
            var mouseCoordVector2 = Input.MousePosition;
            var mouseCoordsText = $"{mouseCoordVector2.X}, {mouseCoordVector2.Y}";
            var phi = 0.0;
            var mousePolarCoord = mouseCoordVector2.GetPolarCoordinates(out phi);

            Graphics.DrawText(mouseCoordsText, new Vector2(windowRectangle.Width/2, windowRectangle.Height/2), Color.Red, 5);
            Graphics.DrawText($"D: {mousePolarCoord} Phi: {MathHepler.ConvertToRadians(phi)}", new Vector2(windowRectangle.Width / 2, windowRectangle.Height / 2 + 15), Color.Red, 5);
            
        }


        /// <summary>
        /// Method iterates through tracked monsters and calls icon update method.
        /// Updates when we are rendering
        /// </summary>
        private void TickLogic()
        {

           foreach (var element in trackedEntities)
           {
               var icon = element.Value.GetHudComponent<FloatingIcon>();      
               UpdateIcon(icon);
           }

        }



        /// <summary>
        /// Method updates icons fields/properties such as screen position, look, etc.
        /// </summary>
        /// <param name="icon"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateIcon(FloatingIcon icon)
        {
            if (icon.Entity.Type == EntityType.Player && !Settings.TestIconOnPlayer)
            {
                icon.SkipDrawing = true;
                return;
            }


            var oldScreenPosX = icon.ScreenPosition.X;
            var oldScreenPosY = icon.ScreenPosition.Y;
            icon.IconZOffset = Settings.FloatingIconZValue;

            var worldCoords = icon.Entity.Pos;
            worldCoords.Z += icon.IconZOffset;

            var screenPos = camera.WorldToScreen(worldCoords);
            
            screenPos.Y -= icon.IconHeight;
            screenPos.X -= icon.IconWidth;

            icon.ScreenPosition = screenPos;

            // Location is on screen
            if (!IsVector2OnScreen(screenPos, icon))
            {
                // Set normal icon
                if (screenPos.X < 0 || screenPos.X > windowRectangle.Width - icon.IconWidth)
                {
                    icon.ScreenPosition.X = oldScreenPosX;
                }

                if (screenPos.Y < 0 || screenPos.Y > windowRectangle.Height - icon.IconWidth)
                {
                    icon.ScreenPosition.Y = oldScreenPosY;

                }

                // Entity center position is also on screen
                if (IsVector2OnScreen(camera.WorldToScreen(icon.Entity.BoundsCenterPos), icon))
                {
  
                    // Set icon to skull

                }
                else
                {
                    //Set icon to arrow bubble to edge of screen and follow entity

                }

            }

            icon.ImageRect = new RectangleF(icon.ScreenPosition.X, icon.ScreenPosition.Y, icon.IconWidth, icon.IconHeight);

        }



        /// <summary>
        /// Checks if screen position is within the games window rectangle.
        /// </summary>
        /// <param name="screenPos"></param>
        /// <returns></returns>
        private bool IsVector2OnScreen(Vector2 screenPos, FloatingIcon icon)
        {
            return screenPos.PointInRectangle(new RectangleF(0,0, windowRectangle.Width - icon.IconWidth, windowRectangle.Height - icon.IconHeight)); // This might not return the correct answer if upper left corner of rectangle is not (0,0)
        }




        /// <summary>
        /// Method performs rendering behavior for plugin. Called during render phase.
        /// </summary>
        public override void Render()
        {

            if (!CanTick)
                return;

            foreach (var element in trackedEntities)
            {
                var icon = element.Value.GetHudComponent<FloatingIcon>();

                if (icon.SkipDrawing)
                {
                    icon.SkipDrawing = false;
                    continue;

                }
                DrawIcon(icon);

            }
        }





        /// <summary>
        /// Method draws icon. Called from Render() during render phase.
        /// </summary>
        /// <param name="icon"></param>
        private void DrawIcon(FloatingIcon icon)
        {
            //DebugWindow.LogMsg($"{entity.RenderName} @ {entityScreenPosition.X},{entityScreenPosition.Y}");
            //Graphics.DrawBox(new RectangleF(xPos, yPos, 10, 10), Color.Red);

            //Graphics.DrawImage("skull_icon.png", new RectangleF(icon.ScreenPosition.X, icon.ScreenPosition.Y, 30, 30));
            Graphics.DrawImage("skull_icon.png", icon.ImageRect); 

        }





        /// <summary>
        /// Method clears tracked monster cache. Called when area changes.
        /// </summary>
        /// <param name="area"></param>
        public override void AreaChange(AreaInstance area)
        {
            // Do some kinda of clearing of cached monsters here
            // when we implement that feature to track multiple bosses
            DebugWindow.LogMsg("Changed area, clearing tracked monster cache.");
            trackedEntities.Clear();
        }





        /// <summary>
        /// Method filters out entities if they do not meet criteria for tracking.
        /// </summary>
        /// <param name="entity"></param>
        public override void EntityAdded(Entity entity)
        {
            if (ShouldTrackEntity(entity))
            {
                entity.SetHudComponent(new FloatingIcon(entity, Settings.FloatingIconZValue));
                trackedEntities.TryAdd(entity.Id, entity);
                DebugWindow.LogMsg(
                    $"Tracking <{entity.Type}> {entity.RenderName} ID: {entity.Id} \nTracking {trackedEntities.Count} monsters.");
            }
        }

        private bool ShouldTrackEntity(Entity entity)
        {
            bool shouldTrackEntity = false;
            
            // if (Entity.Type == EntityType.Player || Entity.Type != EntityType.Monster || Entity.GetComponent<ObjectMagicProperties>().Rarity != MonsterRarity.Unique) // Not filtering properly
            //     return;




            if (entity.Type == EntityType.Monster && entity.GetComponent<ObjectMagicProperties>().Rarity == MonsterRarity.Unique)
                shouldTrackEntity = true;
                
            if (entity.Type == EntityType.Player) 
                shouldTrackEntity = true;

            DebugWindow.LogMsg(
                $"Tracking <{entity.Type}> {entity.RenderName} ID: {entity.Id} \nTracking = {shouldTrackEntity}.");


            return shouldTrackEntity;

        }





        /// <summary>
        /// Method removes entity from tracking cache if it is removed from game.
        /// </summary>
        /// <param name="Entity"></param>
        public override void EntityRemoved(Entity Entity)
        {
            if (trackedEntities.ContainsKey(Entity.Id))
            {
                trackedEntities.TryRemove(Entity.Id, out Entity);
                DebugWindow.LogMsg($"Removed {Entity.RenderName} from tracked monsters. " +
                                   $"\nTracking {trackedEntities.Count} monsters.");
            }

        }

    }

}

