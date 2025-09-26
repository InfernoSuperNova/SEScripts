using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    public struct ArgusHUDInfo
    {
        public bool IsScanning;
        public float Runtime;
        public int OffendingLockCount;
        public TargetableBlockCategory TargetingCategory;
        public TargetableBlockCategory ScanningCategory;


        public int ScanningStep;
        public int MaxScanSteps;
        public int ScanResultAll;
        public int ScanResultWeapons;
        public int ScanResultPower;
        public int ScanResultPropulsion;
        public int ScanResultUncategorized;

        public List<float> RailgunReloadPercentages;
        public string TargetedShipName;
        public Vector3D TargetPosition;

        public ArgusHUDInfo(
            bool isScanning,
            float runtime,
            int offendingLockCount,
            TargetableBlockCategory targetingCategory,
            TargetableBlockCategory scanningCategory,
            int scanningStep,
            int maxScanSteps,
            int scanResultAll,
            int scanResultWeapons,
            int scanResultPower,
            int scanResultPropulsion,
            int scanResultUncategorized,
            List<float> railgunReloadPercentages,
            string targetedShipName,
            Vector3D targetPosition
        )
        {
            IsScanning = isScanning;
            Runtime = runtime;
            OffendingLockCount = offendingLockCount;
            TargetingCategory = targetingCategory;
            ScanningCategory = scanningCategory;


            ScanningStep = scanningStep;
            MaxScanSteps = maxScanSteps;
            ScanResultAll = scanResultAll;
            ScanResultWeapons = scanResultWeapons;
            ScanResultPower = scanResultPower;
            ScanResultPropulsion = scanResultPropulsion;
            ScanResultUncategorized = scanResultUncategorized;

            RailgunReloadPercentages = railgunReloadPercentages;
            TargetedShipName = targetedShipName;
            TargetPosition = targetPosition;
        }
    }

    internal class HUDSurfaceData
    {
        private readonly Vector2 boolHorizontalSpacing = new Vector2(-70, 0);
        private readonly int boolPositionColumns = 1;
        private readonly int boolPositionRows = 15;

        public Vector2[] boolPositions;
        public Vector2 boolSelectoroffset = new Vector2(-20, 0);

        private readonly Vector2 boolStartingOffset = new Vector2(0, -20);
        private readonly Vector2 boolVerticalSpacing = new Vector2(0, 20);
        public IMyCameraBlock camera;
        public Vector2 enemyOutlineBLocker = new Vector2(11, 11);
        public Vector2 enemyOutlineCategory = new Vector2(8, 8);


        public Vector2 enemyOutlineSizeLarge = new Vector2(15, 15);
        public bool hasLinkedCamera;


        public Vector2 initialBoolPosition;
        public Vector2 lockTextPosition;

        private readonly Vector2 lockWarningOffset = new Vector2(70, 50);
        public Color missileBackgroundBarColor = new Color(80, 0, 0);

        public Vector2 missileCountOffset = new Vector2(-180, 20);

        public Vector2 missileCountPosition;
        public Color missileEmptyColor = Color.Orange;
        public Vector2 missileFuelBarMaxSize = new Vector2(100, 3);

        public Vector2 missileFuelBarMinSize = new Vector2(0, 3);
        public Vector2 missileSmallFuelBarSize = new Vector2(3, 3);
        public int missileFuelBarMaxX = 16;
        private readonly Vector2 missileFuelBarOffset = new Vector2(-100, 20);
        public Vector2 missileFuelBarOnMissileOffset = new Vector2(0, 20);
        public Vector2 missileFuelBarPosition;
        public float missileFuelBarSpacing = 3;


        public Color missileFuelingColor = Color.CornflowerBlue;
        public Color missileFullColor = new Color(0, 80, 0);
        public Vector2 missileOutlineBlocker = new Vector2(1, 1);

        public Vector2 missileOutlineSizeLarge = new Vector2(3, 3);
        public Vector2 missileOutlineSizeSmall = new Vector2(11, 11);

        private readonly Vector2 progressBarStartingOffset = new Vector2(0, 20);
        public Vector2 railPosition;

        private readonly Vector2 runtimeOffset = new Vector2(-10, -20);
        public Vector2 runtimePosition;
        public Vector2 scanCategoryTextPosition;


        private readonly Vector2 scanLabelOffset = new Vector2(20, -100);


        public Vector2 scanLabelPosition;
        public Vector2 scanLabelSize = new Vector2(200, 30);
        public Vector2 scanLabelTextPosition;
        public float scanLineScale = 0.8f;
        private readonly Vector2 scanLineSpacing = new Vector2(0, 30);
        public Vector2 scanResultAllTextPosition;


        private readonly Vector2 scanResultLabelOffset = new Vector2(20, -100);
        private Vector2 scanResultLabelSize = new Vector2(150, 25);
        public float scanResultLineScale = 0.6f;
        private readonly Vector2 scanResultLineSpacing = new Vector2(0, 15);
        public Vector2 scanResultPowerTextPosition;
        public Vector2 scanResultPropulsionTextPosition;
        private readonly Vector2 scanResultTextOffset = new Vector2(10, -20);

        public float scanResultTextScale = 0.7f;
        public Vector2 scanResultWeaponsTextPosition;
        public Vector2 scanStepTextPosition;
        private readonly Vector2 scanTextOffset = new Vector2(22, -20);

        public float scanTextScale = 1.2f;
        public IMyTextSurface surface;

        public Vector2 targetedShipTextPosition;

        public Vector2 targetingTextPosition;

        public string version;


        public Vector2 versionTextPosition;
        public RectangleF viewport;


        public HUDSurfaceData(IMyTextSurface surface, RectangleF viewport, IMyCameraBlock camera, string version)
        {
            this.surface = surface;
            this.viewport = viewport;


            if (camera != null)
            {
                hasLinkedCamera = true;
                this.camera = camera;
            }

            versionTextPosition = new Vector2(viewport.Position.X + viewport.Width / 2,
                viewport.Position.Y + viewport.Height - 20);
            this.version = version;

            runtimePosition = viewport.Size + runtimeOffset + viewport.Position;
            lockTextPosition = viewport.Position + lockWarningOffset;
            railPosition = viewport.Position + progressBarStartingOffset;

            scanLabelPosition = new Vector2(0, viewport.Height) + scanLabelOffset;
            scanLabelTextPosition = scanLabelPosition + scanTextOffset;
            scanCategoryTextPosition = scanLabelTextPosition + scanLineSpacing * 2;
            scanStepTextPosition = scanCategoryTextPosition + scanLineSpacing;


            targetingTextPosition = new Vector2(viewport.Position.X + viewport.Width / 2,
                viewport.Position.Y + viewport.Height - 50);


            targetedShipTextPosition = new Vector2(0, viewport.Height) + scanResultLabelOffset + scanResultTextOffset;
            scanResultAllTextPosition = targetedShipTextPosition + scanResultLineSpacing * 2;
            scanResultWeaponsTextPosition = scanResultAllTextPosition + scanResultLineSpacing;
            scanResultPowerTextPosition = scanResultWeaponsTextPosition + scanResultLineSpacing;
            scanResultPropulsionTextPosition = scanResultPowerTextPosition + scanResultLineSpacing;

            initialBoolPosition = viewport.Size + boolStartingOffset + viewport.Position +
                boolHorizontalSpacing * boolPositionColumns - boolPositionRows * boolVerticalSpacing;

            boolPositions = new Vector2[boolPositionRows * boolPositionColumns];
            for (var i = 0; i < boolPositionColumns; i++)
            for (var j = 0; j < boolPositionRows; j++)
                boolPositions[i * boolPositionRows + j] =
                    initialBoolPosition + (j + 1) * boolVerticalSpacing - i * boolHorizontalSpacing;

            missileFuelBarPosition = new Vector2(viewport.Width, 0) + missileFuelBarOffset + viewport.Position;

            missileCountPosition = new Vector2(viewport.Width, 0) + missileCountOffset + viewport.Position;
        }
    }

    internal class ArgusHUD
    {
        private static readonly string primaryFontId = "White";
        private readonly float boolMinimumScale = 90;

        public ushort BoolSelectorIndex;
        private readonly float boolTextScale = 0.6f;

        private readonly Dictionary<IMyTextSurface, Dictionary<long, Vector2>> currentPos =
            new Dictionary<IMyTextSurface, Dictionary<long, Vector2>>();

        private readonly float defaultFontSize = 14f;

        public static readonly ArgusHUDInfo DefaultHudInfo = new ArgusHUDInfo(true, 0f, 0, TargetableBlockCategory.Default,
            TargetableBlockCategory.Default, 0, 0, 0, 0, 0, 0, 0, new List<float>(), "", Vector3D.Zero);

        private readonly Color falseColor = new Color(80, 0, 0);


        private readonly List<HUDSurfaceData> hudSurfaceDataList;

        private int keenWorkaround;
        private readonly int maxKeenWorkaround = 30;

        private readonly float PointerLineWidth = 1;

        private readonly Dictionary<IMyTextSurface, Dictionary<long, Vector2>> previousPos =
            new Dictionary<IMyTextSurface, Dictionary<long, Vector2>>();

        private readonly Vector2 progressBarMaxSize = new Vector2(200, 1);

        private readonly Vector2 progressBarMinSize = new Vector2(0, 1);
        
        private readonly int progressBarSpacing = 1;
        public ArgusSwitches switches;


        private readonly Color trueColor = new Color(0, 80, 0);

        public ArgusHUD(List<IMyTextSurface> surfaces, string version, List<IMyCameraBlock> cameras,
            ref ArgusSwitches switches)
        {
            this.switches = switches;
            hudSurfaceDataList = new List<HUDSurfaceData>();
            foreach (var surface in surfaces)
            {
                var viewport = new RectangleF(
                    (surface.TextureSize - surface.SurfaceSize) / 2f,
                    surface.SurfaceSize
                );
                surface.ContentType = ContentType.SCRIPT;
                surface.BackgroundColor = Color.Black;


                IMyCameraBlock camera = null;
                var panel = surface as IMyTextPanel;
                if (panel != null)
                    foreach (var otherCamera in cameras)
                    {
                        if (otherCamera.CustomData == "") continue;
                        if (otherCamera.CustomData == panel.CustomData) camera = otherCamera;
                    }
                

                var data = new HUDSurfaceData(surface, viewport, camera, version);
                hudSurfaceDataList.Add(data);

                previousPos.Add(surface, new Dictionary<long, Vector2>());
                currentPos.Add(surface, new Dictionary<long, Vector2>());

                Draw(data, DefaultHudInfo, new List<KratosMissileManager.MissileData>(), ArgusTargetableShip.Default);
            }
        }

        public void MoveSelectorUp()
        {
            BoolSelectorIndex--;
            if (BoolSelectorIndex >= hudSurfaceDataList.ElementAt(0).boolPositions.Length)
                BoolSelectorIndex = (ushort)(hudSurfaceDataList.ElementAt(0).boolPositions.Length - 1);
        }

        public void MoveSelectorDown()
        {
            BoolSelectorIndex++;
            if (BoolSelectorIndex >= hudSurfaceDataList.ElementAt(0).boolPositions.Length) BoolSelectorIndex = 0;
        }

        public void SelectorSelect()
        {
            var fields = switches.GetBools();

            if (fields.Length <= BoolSelectorIndex) return;
            fields[BoolSelectorIndex] = !fields[BoolSelectorIndex];
            switches.SetBools(fields);
        }

        public void UpdateHUDInfo(ArgusHUDInfo info, List<KratosMissileManager.MissileData> missileData,
            ArgusTargetableShip targetedShip)
        {
            keenWorkaround++;
            for (var i = 0; i < hudSurfaceDataList.Count; i++)
            {
                var data = hudSurfaceDataList.ElementAt(i);
                Draw(data, info, missileData, targetedShip);
            }
        }

        private void Draw(HUDSurfaceData data, ArgusHUDInfo info, List<KratosMissileManager.MissileData> missileData,
            ArgusTargetableShip targetedShip)
        {
            var frame = data.surface.DrawFrame();
            DrawSprites(ref frame, data, info, missileData, targetedShip);
            frame.Dispose();
        }


        public string TargetableBlockCategoryToString(TargetableBlockCategory category)
        {
            switch (category)
            {
                case TargetableBlockCategory.Default:
                    return "All Blocks";
                case TargetableBlockCategory.Weapons:
                    return "Weapons";
                case TargetableBlockCategory.Propulsion:
                    return "Propulsion";
                case TargetableBlockCategory.PowerSystems:
                    return "Power Systems";
            }

            return "fuck you";
        }

        public void DrawSprites(ref MySpriteDrawFrame frame, HUDSurfaceData data, ArgusHUDInfo info,
            List<KratosMissileManager.MissileData> missileData, ArgusTargetableShip targetedShip)
        {
            if (keenWorkaround > maxKeenWorkaround)
            {
                keenWorkaround = 0;
                var empty = new MySprite();
                frame.Add(empty);
            }

            DrawBools(ref frame, data, switches);
            DrawScanInfo(ref frame, data, info);


            var runtimeBackgroundSquare = new MySprite
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Position = data.runtimePosition,
                Size = new Vector2(200, 30),
                Color = Color.White,
                Alignment = TextAlignment.RIGHT
            };
            var runtimeText = new MySprite
            {
                Type = SpriteType.TEXT,
                // Data = info.Runtime.ToString("0.0") + " / 300µs",
                Data = info.Runtime.ToString("0.0"),
                Position = data.runtimePosition,
                RotationOrScale = 0.8f,
                Color = Color.OrangeRed,
                Alignment = TextAlignment.RIGHT,
                FontId = primaryFontId
            };
            frame.Add(runtimeText);


            if (info.OffendingLockCount > 0)
            {
                var lockText = new MySprite
                {
                    Type = SpriteType.TEXT,
                    Data = "<<Warning: Enemy lock x" + info.OffendingLockCount + ">>",
                    Position = data.lockTextPosition,
                    RotationOrScale = 1.0f,
                    Color = Color.Orange,
                    Alignment = TextAlignment.LEFT,
                    FontId = primaryFontId
                };
                frame.Add(lockText);
            }

            var railPosition = data.railPosition;
            
            
            for (var i = 0; i < info.RailgunReloadPercentages.Count; i++)
            {
                ProgressBar(ref frame, railPosition, true, info.RailgunReloadPercentages[i]);
                railPosition.Y += progressBarSpacing;
            }

            DrawVersionText(ref frame, data);

            if (switches.HighlightTarget) DrawEnemyFloorPlan(ref frame, targetedShip, data);


            int readyCount = 0;
            int activeCount = 0;
            foreach (KratosMissileManager.MissileData missile in missileData)
            {
                if (missile.HasLaunched) 
                    activeCount++;
                else readyCount++;
            }
            
            
            if (switches.HighlightMissiles)
            {
                if (missileData.Count == 0)
                {
                    var missileCountText = new MySprite
                    {
                        Type = SpriteType.TEXT,
                        Data = "0 / 0",
                        Position = data.missileFuelBarPosition,
                        RotationOrScale = 0.8f,
                        Color = Color.White,
                        Alignment = TextAlignment.LEFT,
                        FontId = primaryFontId
                    };
                    frame.Add(missileCountText);
                }
                else
                {
                    var missileCountText = new MySprite
                    {
                        Type = SpriteType.TEXT,
                        Data = activeCount + " / " + readyCount,
                        Position = data.missileCountPosition,
                        RotationOrScale = 0.8f,
                        Color = Color.White,
                        Alignment = TextAlignment.LEFT,
                        FontId = primaryFontId
                    };
                    frame.Add(missileCountText);
                }

                DrawMissilePositions(ref frame, data, missileData, info.TargetPosition, info.TargetingCategory);
            }
            else
            {
                var missileCountText = new MySprite
                {
                    Type = SpriteType.TEXT,
                    Data = missileData.Count + " / 20",
                    Position = data.missileFuelBarPosition,
                    RotationOrScale = 0.8f,
                    Color = Color.White,
                    Alignment = TextAlignment.LEFT,
                    FontId = primaryFontId
                };
                frame.Add(missileCountText);
            }
        }

        private void DrawEnemyFloorPlan(ref MySpriteDrawFrame frame, ArgusTargetableShip targetedShip, HUDSurfaceData data)
        {
            var startPos = new Vector2(200, 200);

            for (var i = 0; i < targetedShip.Blocks.Count; i++)
            {
                var block = targetedShip.Blocks[i];

                var panel = data.surface as IMyTextPanel;
                if (panel == null) return;
                if (data.camera == null) return;
                var cameraData = new cameraData(data.camera, panel);

                var pos = targetedShip.GetBlockPositionAtIndex(i);
                var distance = Vector3D.Distance(pos, cameraData.cameraPos);
                var size = (float)MathHelper.Clamp(
                    MathHelper.InterpLog((float)(2000 - distance) / 2000, 0.5f, 4),
                    1,
                    double.MaxValue
                );
                Vector2 targetPosScreen;

                var TargetInScreen =
                    WorldPositionToScreenPosition(pos, cameraData, data.camera, panel, out targetPosScreen);
                if (!TargetInScreen) continue;

                var color = Color.White;
                switch (block.category)
                {
                    case TargetableBlockCategory.Default:
                        color = Color.White;
                        break;
                    case TargetableBlockCategory.Weapons:
                        color = Color.Red;
                        break;
                    case TargetableBlockCategory.Propulsion:
                        color = Color.Yellow;
                        break;
                    case TargetableBlockCategory.PowerSystems:
                        color = Color.Blue;
                        break;
                }


                var blockSprite = new MySprite
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Position = targetPosScreen,
                    Size = new Vector2(size, size),
                    Color = color,
                    Alignment = TextAlignment.LEFT
                };
                frame.Add(blockSprite);
            }
        }


        private void DrawVersionText(ref MySpriteDrawFrame frame, HUDSurfaceData data)
        {
            var versionText = new MySprite
            {
                Type = SpriteType.TEXT,
                Data = data.version,
                Position = data.versionTextPosition,
                RotationOrScale = 0.7f,
                Color = Color.White,
                Alignment = TextAlignment.CENTER,
                FontId = primaryFontId
            };
            frame.Add(versionText);
        }

        private void DrawMissilePositions(ref MySpriteDrawFrame frame, HUDSurfaceData data,
            List<KratosMissileManager.MissileData> missileData, Vector3D target, TargetableBlockCategory category)
        {
            var panel = data.surface as IMyTextPanel;
            if (panel == null) return;
            if (data.camera == null) return;
            var cameraData = new cameraData(data.camera, panel);


            Vector2 targetPosScreen;

            var TargetInScreen
                = WorldPositionToScreenPosition(target, cameraData, data.camera, panel, out targetPosScreen)
                  && targetPosScreen.X > data.viewport.Position.X
                  && targetPosScreen.Y > data.viewport.Position.Y
                  && targetPosScreen.X < data.viewport.Position.X + data.viewport.Width
                  && targetPosScreen.Y < data.viewport.Position.Y + data.viewport.Height;



            bool drawSmallBars = missileData.Count > 60;
            var missileFuelBarPosition = data.missileFuelBarPosition;
            for (var i = 0; i < missileData.Count; i++)
            {
                missileFuelBarPosition.Y += data.missileFuelBarSpacing;
                var missile = missileData[i];
                Vector2 currentScreenPosPx;

                var backgroundColor = data.missileBackgroundBarColor;

                var foregroundColour = missile.HasLaunched
                    ? Color.Lerp(data.missileEmptyColor, data.missileFullColor, (float)missile.Fuel)
                    : data.missileFuelingColor;
                


                if (WorldPositionToScreenPosition(missile.Position, cameraData, data.camera, panel,
                        out currentScreenPosPx))
                {
                    currentPos[data.surface].Add(missile.Id, currentScreenPosPx);
                    var lerpPos = currentScreenPosPx;
                    

                    if (previousPos[data.surface].ContainsKey(missile.Id))
                        lerpPos = Vector2.Lerp(previousPos[data.surface][missile.Id], currentScreenPosPx, 1.5f);


                    //if (TargetInScreen)
                    //{
                    //    PointerLine(ref frame, lerpPos, targetPosScreen);
                    //}

                    var borderSprite = new MySprite
                    {
                        Type = SpriteType.TEXTURE,
                        Data = "SquareSimple",
                        Position = lerpPos,
                        Size = data.missileOutlineSizeLarge,
                        Color = Color.Lime,
                        Alignment = TextAlignment.CENTER
                    };
                    frame.Add(borderSprite);
                    var blockerSprite = new MySprite
                    {
                        Type = SpriteType.TEXTURE,
                        Data = "SquareSimple",
                        Position = lerpPos,
                        Size = data.missileOutlineBlocker,
                        Color = Color.Black,
                        Alignment = TextAlignment.CENTER
                    };
                    frame.Add(blockerSprite);
                    
                }

                if (!drawSmallBars)
                    
                    ProgressBar(ref frame, missileFuelBarPosition, data.missileFuelBarMinSize,
                        data.missileFuelBarMaxSize, foregroundColour, backgroundColor, true, (float)missile.Fuel, true);
            }

            if (drawSmallBars)
            {
                Vector2 position = data.missileFuelBarPosition;
                int missileFuelBarColumn = 0;
                int missileFuelBarRow = 0;
                for (var i = 0; i < missileData.Count; i++)
                {
                    var missile = missileData[i];
                    var fuel = missile.Fuel;

                    var fullColour = Color.White;
                    var emptyColour = Color.Blue;
                    if (missile.HasLaunched)
                    {
                        fullColour = trueColor;
                        emptyColour = falseColor;
                    }
                    Vector2 actualPosition = position + new Vector2(missileFuelBarColumn * data.missileFuelBarSpacing * 2, missileFuelBarRow * data.missileFuelBarSpacing * 2);
                    
                    ProgressSquare(ref frame, actualPosition, new Vector2(3, 3), (float)fuel, fullColour, emptyColour);
                    missileFuelBarColumn++;
                    if (missileFuelBarColumn >= data.missileFuelBarMaxX)
                    {
                        missileFuelBarColumn = 0; 
                        missileFuelBarRow++;
                    }
                }
            }
            //var targetBorderSprite = new MySprite()
            //{
            //    Type = SpriteType.TEXTURE,
            //    Data = "SquareSimple",
            //    Position = targetPosScreen,
            //    Size = data.enemyOutlineSizeLarge,
            //    Color = Color.Green,
            //    Alignment = TextAlignment.CENTER
            //};
            //frame.Add(targetBorderSprite);
            //var targetBlockerSprite = new MySprite()
            //{
            //    Type = SpriteType.TEXTURE,
            //    Data = "SquareSimple",
            //    Position = targetPosScreen,
            //    Size = data.enemyOutlineBLocker,
            //    Color = Color.Black,
            //    Alignment = TextAlignment.CENTER
            //};
            //frame.Add(targetBlockerSprite);
            //var color = Color.White;
            //switch (category)
            //{
            //    case ArgusEnums.TargetableBlockCategory.Default:
            //        color = Color.White;
            //        break;
            //    case ArgusEnums.TargetableBlockCategory.Weapons:
            //        color = Color.Red;
            //        break;
            //    case ArgusEnums.TargetableBlockCategory.Propulsion:
            //        color = Color.Yellow;
            //        break;
            //    case ArgusEnums.TargetableBlockCategory.PowerSystems:
            //        color = Color.Blue;
            //        break;

            //}
            //var targetCategorySprite = new MySprite()
            //{
            //    Type = SpriteType.TEXTURE,
            //    Data = "SquareSimple",
            //    Position = targetPosScreen,
            //    Size = data.enemyOutlineCategory,
            //    Color = color,
            //    Alignment = TextAlignment.CENTER
            //};
            //frame.Add(targetCategorySprite);

            var temp = currentPos[data.surface];
            currentPos[data.surface] = previousPos[data.surface];
            previousPos[data.surface] = temp;
            currentPos[data.surface].Clear();
        }

        private void DrawScanInfo(ref MySpriteDrawFrame frame, HUDSurfaceData data, ArgusHUDInfo info)
        {
            // Scanning text
            if (info.IsScanning)
                ScanningLines(ref frame, data, info);
            else
                ScanResults(ref frame, data, info);
        }


        private void ScanningLines(ref MySpriteDrawFrame frame, HUDSurfaceData data, ArgusHUDInfo info)
        {
            var backgroundSprite = new MySprite
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Position = data.scanLabelPosition,
                Size = data.scanLabelSize,
                Color = Color.LightBlue,
                Alignment = TextAlignment.LEFT
            };
            frame.Add(backgroundSprite);
            var scanningText = new MySprite
            {
                Type = SpriteType.TEXT,
                Data = "Scanning...",
                RotationOrScale = data.scanTextScale,
                Position = data.scanLabelTextPosition,

                Color = Color.Black,
                Alignment = TextAlignment.LEFT,
                FontId = primaryFontId
            };
            frame.Add(scanningText);
            var categoryText = new MySprite
            {
                Type = SpriteType.TEXT,
                Data = "Category: " + TargetableBlockCategoryToString(info.ScanningCategory),
                RotationOrScale = data.scanLineScale,
                Position = data.scanCategoryTextPosition,
                Color = Color.White,
                Alignment = TextAlignment.LEFT,
                FontId = primaryFontId
            };
            frame.Add(categoryText);
            var stepText = new MySprite
            {
                Type = SpriteType.TEXT,
                Data = "Step " + (info.ScanningStep + 1) + "/" + info.MaxScanSteps,
                RotationOrScale = data.scanLineScale,
                Position = data.scanStepTextPosition,
                Color = Color.White,
                Alignment = TextAlignment.LEFT,
                FontId = primaryFontId
            };
            frame.Add(stepText);
        }


        private void ScanResults(ref MySpriteDrawFrame frame, HUDSurfaceData data, ArgusHUDInfo info)
        {
            var targetingCategoryText = new MySprite
            {
                Type = SpriteType.TEXT,
                Data = "Targeting: " + TargetableBlockCategoryToString(info.TargetingCategory),
                RotationOrScale = data.scanResultTextScale,
                Position = data.targetingTextPosition,
                Color = Color.CornflowerBlue,
                Alignment = TextAlignment.CENTER,
                FontId = primaryFontId
            };
            frame.Add(targetingCategoryText);


            var targetedShipText = new MySprite
            {
                Type = SpriteType.TEXT,
                Data = info.TargetedShipName,
                RotationOrScale = data.scanResultTextScale,
                Position = data.targetedShipTextPosition,

                Color = Color.Red,
                Alignment = TextAlignment.LEFT,
                FontId = primaryFontId
            };
            frame.Add(targetedShipText);
            var allText = new MySprite
            {
                Type = SpriteType.TEXT,
                Data = "All: " + info.ScanResultAll,
                RotationOrScale = data.scanResultLineScale,
                Position = data.scanResultAllTextPosition,
                Color = Color.White,
                Alignment = TextAlignment.LEFT,
                FontId = primaryFontId
            };
            frame.Add(allText);
            var weaponsText = new MySprite
            {
                Type = SpriteType.TEXT,
                Data = "Weapons: " + info.ScanResultWeapons,
                RotationOrScale = data.scanResultLineScale,
                Position = data.scanResultWeaponsTextPosition,
                Color = Color.White,
                Alignment = TextAlignment.LEFT,
                FontId = primaryFontId
            };
            frame.Add(weaponsText);
            var powerText = new MySprite
            {
                Type = SpriteType.TEXT,
                Data = "Power: " + info.ScanResultPower,
                RotationOrScale = data.scanResultLineScale,
                Position = data.scanResultPowerTextPosition,
                Color = Color.White,
                Alignment = TextAlignment.LEFT,
                FontId = primaryFontId
            };
            frame.Add(powerText);
            var propulsionText = new MySprite
            {
                Type = SpriteType.TEXT,
                Data = "Propulsion: " + info.ScanResultPropulsion,
                RotationOrScale = data.scanResultLineScale,
                Position = data.scanResultPropulsionTextPosition,
                Color = Color.White,
                Alignment = TextAlignment.LEFT,
                FontId = primaryFontId
            };
            frame.Add(propulsionText);
        }


        private void DrawBools(ref MySpriteDrawFrame frame, HUDSurfaceData data, ArgusSwitches switches)
        {
            var fields = switches.GetBoolNames();

            for (var i = 0; i < fields.Count; i++)
                BoolIndicator(ref frame, data.boolPositions[i], fields.ElementAt(i).Value, fields.ElementAt(i).Key);

            var arrowSprite = new MySprite
            {
                Type = SpriteType.TEXTURE,
                Data = "Arrow",
                Position = data.boolPositions[BoolSelectorIndex] + data.boolSelectoroffset,
                Size = new Vector2(20, 20),
                RotationOrScale = 1.570796f,
                Color = Color.White,
                Alignment = TextAlignment.LEFT
            };
            frame.Add(arrowSprite);
        }


        private void PointerLine(ref MySpriteDrawFrame frame, Vector2 positionA, Vector2 positionB)
        {
            var color = Color.CornflowerBlue;

            var intermediatePosition = new Vector2(positionA.X, positionB.Y);

            var horizontalLineStart = positionB.X < intermediatePosition.X ? positionB : intermediatePosition;
            var horizontalLineSize = new Vector2(Math.Abs(positionB.X - intermediatePosition.X), PointerLineWidth);

            var positionAtoIntermediate = intermediatePosition - positionA;

            var verticalLineStart = positionA.Y < intermediatePosition.Y ? positionA : intermediatePosition;
            verticalLineStart.Y += Math.Abs(positionAtoIntermediate.Y / 2);
            var verticalLineSize = new Vector2(PointerLineWidth, Math.Abs(positionAtoIntermediate.Y));

            var horizontalLine = new MySprite
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Position = horizontalLineStart,
                Size = horizontalLineSize,
                Color = color,
                Alignment = TextAlignment.LEFT
            };

            var verticalLine = new MySprite
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Position = verticalLineStart,
                Size = verticalLineSize,
                Color = color,
                Alignment = TextAlignment.LEFT
            };


            frame.Add(horizontalLine);
            frame.Add(verticalLine);
        }

        private void BoolIndicator(ref MySpriteDrawFrame frame, Vector2 position, bool state, string text)
        {
            var color = state ? trueColor : falseColor;

            var width = Math.Max(boolTextScale * defaultFontSize * text.Length, boolMinimumScale);
            var backgroundSprite = new MySprite
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Position = position,
                Size = new Vector2(width, 20),
                Color = color,
                Alignment = TextAlignment.LEFT
            };
            var textSprite = new MySprite
            {
                Type = SpriteType.TEXT,
                Data = text,
                RotationOrScale = boolTextScale,
                Position = position + new Vector2(2, -10),
                Color = Color.White,
                Alignment = TextAlignment.LEFT,
                FontId = primaryFontId
            };
            frame.Add(backgroundSprite);
            frame.Add(textSprite);
        }

        private void ProgressBar(ref MySpriteDrawFrame frame, Vector2 position, bool state, float progress)
        {
            var backgroundBar = new MySprite
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Position = position,
                Size = progressBarMaxSize,
                Color = falseColor,
                Alignment = TextAlignment.LEFT
            };
            frame.Add(backgroundBar);
            var foregroundBarSize = Vector2.Lerp(progressBarMinSize, progressBarMaxSize, progress);

            if (progress == 1)
            {
                var foregroundBar = new MySprite
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Position = position,
                    Size = foregroundBarSize,
                    Color = trueColor,
                    Alignment = TextAlignment.LEFT
                };
                frame.Add(foregroundBar);
            }
            else
            {
                var foregroundBar = new MySprite
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Position = position,
                    Size = foregroundBarSize,
                    Color = Color.CornflowerBlue,
                    Alignment = TextAlignment.LEFT
                };
                frame.Add(foregroundBar);
            }
        }

        private void ProgressSquare(ref MySpriteDrawFrame frame, Vector2 position, Vector2 size, float progress, Color fullColour, Color emptyColour)
        {
            var foregroundBar = new MySprite
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Position = position,
                Size = new Vector2(5, 5),
                Color = Color.Lerp(emptyColour, fullColour, progress),
                Alignment = TextAlignment.LEFT
            };
            frame.Add(foregroundBar);
        }
        private void ProgressBar(ref MySpriteDrawFrame frame, Vector2 position, Vector2 minSize, Vector2 maxSize,
            Color foreground, Color background, bool state, float progress, bool inverted)
        {
            var backgroundBar = new MySprite
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Position = position,
                Size = maxSize,
                Color = background,
                Alignment = TextAlignment.LEFT
            };
            frame.Add(backgroundBar);
            var foregroundBarSize = Vector2.Lerp(minSize, maxSize, progress);


            position = inverted ? position + new Vector2(maxSize.X - foregroundBarSize.X, 0) : position;

            var foregroundBar = new MySprite
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Position = position,
                Size = foregroundBarSize,
                Color = foreground,
                Alignment = TextAlignment.LEFT
            };
            frame.Add(foregroundBar);
        }


        // Written by Whiplash141
        // Updated by DeltaWing for ArgusLite 0.9.1

        /// <summary>
        ///     Projects a world position to a location on the screen in pixels.
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="cam"></param>
        /// <param name="screen"></param>
        /// <param name="screenPositionPx"></param>
        /// <param name="screenWidthInMeters"></param>
        /// <returns>True if the solution can be displayed on the screen.</returns>
        private bool WorldPositionToScreenPosition(Vector3D worldPosition, cameraData cameraData, IMyCameraBlock cam,
            IMyTextPanel screen, out Vector2 screenPositionPx)
        {
            screenPositionPx = Vector2.Zero;


            // Project direction onto the screen plane (world coords)
            var direction = worldPosition - cameraData.cameraPos;
            var directionParallel = direction.Dot(cameraData.normal) * cameraData.normal;
            var distanceRatio = cameraData.distanceToScreen / directionParallel.ALength();

            var directionOnScreenWorld = distanceRatio * direction;

            // If we are pointing backwards, ignore
            if (directionOnScreenWorld.Dot(screen.WorldMatrix.Forward) < 0) return false;

            var planarCameraToScreen = cameraData.cameraToScreen -
                                       Vector3D.Dot(cameraData.cameraToScreen, cameraData.normal) * cameraData.normal;
            directionOnScreenWorld -= planarCameraToScreen;

            // Convert location to be screen local (world coords)
            var directionOnScreenLocal = new Vector2(
                (float)directionOnScreenWorld.Dot(screen.WorldMatrix.Right),
                (float)directionOnScreenWorld.Dot(screen.WorldMatrix.Down));

            // ASSUMPTION:
            // The screen is square
            double screenWidthInMeters = screen.CubeGrid.GridSize * 0.855f; // My magic number for large grid
            var metersToPx = (float)(screen.TextureSize.X / screenWidthInMeters);

            // Convert dorection to be screen local (pixel coords)
            directionOnScreenLocal *= metersToPx;

            // Get final location on screen
            var screenCenterPx = screen.TextureSize * 0.5f;
            screenPositionPx = screenCenterPx + directionOnScreenLocal;
            return true;
        }


        private struct cameraData
        {
            public readonly Vector3D cameraPos;
            public readonly Vector3D screenPosition;
            public readonly Vector3D normal;
            public readonly Vector3D cameraToScreen;
            public readonly double distanceToScreen;
            public Vector3D viewCenterWorld;

            public cameraData(IMyCameraBlock cam, IMyTextPanel screen)
            {
                cameraPos = cam.GetPosition() +
                            cam.WorldMatrix.Forward *
                            0.25; // There is a ~0.25 meter forward offset for the view origin of cameras
                screenPosition = screen.GetPosition() + screen.WorldMatrix.Forward * 0.5 * screen.CubeGrid.GridSize;
                normal = screen.WorldMatrix.Forward;
                cameraToScreen = screenPosition - cameraPos;
                distanceToScreen = Math.Abs(Vector3D.Dot(cameraToScreen, normal));

                viewCenterWorld = distanceToScreen * cam.WorldMatrix.Forward;
            }
        }
    }
}