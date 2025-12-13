using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmptyKeys.UserInterface.Controls;
using Sandbox.Game.World;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace IngameScript
{
    public enum TargetableBlockCategory
    {
        Default,
        Weapons,
        Propulsion,
        PowerSystems
    }

    public enum TargetableType
    {
        ClusteredCenter,
        Random
    }




    
    public struct ReloadTrackerDefinition
    {
        public double ShotsPerSecond;
        public double ReloadTime;
        public int BurstCount;
        public double Velocity;
        public double Range;
        
        public int BurstPeriod;
        public int ReloadPeriod;
        
        public ReloadTrackerDefinition(double shotsPerSecond, double reloadTime, int burstCount, double velocity, double range)
        {
            Velocity = velocity;
            ShotsPerSecond = shotsPerSecond;
            ReloadTime = reloadTime;
            BurstCount = burstCount;
            Range = range;
            
            
            BurstPeriod = (int)(1.0 / ShotsPerSecond* 60.0);
            ReloadPeriod = (int)(ReloadTime * 60.0);
        }
        
    }
    
    
    

    internal class Program : MyGridProgram
    {

        public DebugAPI DebugApi;
        public static Program I;
        public static bool TrollMode = false;
        public static IMyGridTerminalSystem _gridTerminalSystem;
        
        private readonly MyIni _ini = new MyIni();
        
        private Dictionary<string, ReloadTrackerDefinition> _reloadTrackerDefinitions =
            new Dictionary<string, ReloadTrackerDefinition>()
            {
                {"MyObjectBuilder_LargeMissileTurret/LargeCalibreTurret", new ReloadTrackerDefinition(1.3333333333, 12, 2, 500, 2000)},
                {"MyObjectBuilder_LargeMissileTurret/LargeBlockMediumCalibreTurret", new ReloadTrackerDefinition(3, 6, 2, 500, 1400)},
            };

        private int ActiveGuns;
        private ArgusTargetableShip actualTargetedShip;
        private ArgusShip _host;
        
        private double
            AimbotWeaponFireSigma =
                0.999; // Dot product of this ships forward vector and the target vector is less than this

        private readonly List<IMyTerminalBlock> allBlocks = new List<IMyTerminalBlock>();

        private readonly ArgusHUD argusHUD;


        private readonly double ArtilleryBurstInterval = 0.75 * 60.0;
        private readonly double ArtilleryReloadInterval = 12.0 * 60.0;


        private Vector3D AverageGunPos = Vector3D.Zero;
        private float averageRuntime;
        private readonly List<IMyBeacon> beacons;
        private readonly StringBuilder beaconSB = new StringBuilder();
        private readonly List<IMyTextSurface> CockpitScreens;
        private bool currentlyScanning;
        private int CurrentScanFrame;

        private Vector3D CurrentVelocity = Vector3D.Zero;

        private double
            DerivativeNonLinearity =
                1.6; // Some random crap I made up to stop the ship overshooting due to gyro ramp up

        private readonly IMyBroadcastListener diagnosticRequestListener;


        private readonly int frame = 0;

        private int FramesScanningWithoutTarget;

        private float
            FramesToGroupGuns = 5; // Maximum frame difference between available guns to aim and fire them together

        private readonly GravityDriveManager gravityDriveManager;
        private IMyBlockGroup group;

        /***********Common Configuration***********/


        private string GroupName = "Flight Control"; // Group name, please ensure this is unique to your ship
        private readonly Guns guns; // guns guns guns guns
        private List<IMyGyro> Gyros;

        private bool HasResetTurrets;
        private bool HasTarget;
        private double integralClamp = 0.05; // Maximum windup for the integral if you are using it
        

        private readonly Dictionary<MyDefinitionId, float> KnownFireDelays = new Dictionary<MyDefinitionId, float>
        {
            [MyDefinitionId.Parse("SmallMissileLauncherReload/SmallRailgun")] = 0.5f,
            [MyDefinitionId.Parse("SmallMissileLauncherReload/LargeRailgun")] = 2.0f
        };


        /**********Aimbot & Configuration**********/


        //bool DoAim = true;                              // Should we use the aimbot?


        private double kP = 500; // Proportional gain
        private double kD = 30; // Derivative gain
        private double kI; // Integral gain

        private readonly KratosMissileManager kratosMissileManager;


        private double largestRuntime;
        private Vector3D LeadPos = Vector3D.Zero;
        private float MaxAngularVelocityRPM = 30; // Should be 30 for large grid, 60 for small grid


        /***********Weapon Configuration***********/


        //bool DoAutoFire = true;                         // Automatically fire and cancel fire?

        // Only fire if:
        private double
            MaximumSafeRPMToFire = 2.0; // Not below this RPM (requires that MaxAngularVelocityRPM is set correctly)
        // TBA: Periods for balancing of mass blocks and space balls


        /************Scan Configuration************/


        //bool DoAutoScan = true;                         // Automatically scan new detected ships?
        private int MaxScanFrames = 5; // How many times we should scan each category

        private readonly List<IMyOffensiveCombatBlock> myOffensiveCombatBlocks;

        //bool LeadAcceleration = true;                   // Should we use acceleration in the lead calculation or only velocity? (both fixed weapons and turrets)
        private double
            NewBlockCheckPeriod =
                31.0 * 60.0; // How often we should check the group for new blocks, try to avoid round numbers otherwise everything could happen at once and burn out the programmable block


        private int OffendingLockCount; // Used to keep track of how many people have locked onto us

        private double OnTargetValue;

        
        /******Missile Configuration******/
        private bool FighterMode = false;

        /*******Gravity Drive Configuration********/


        //bool DoGravityDrive = true;                     // Should we manage gravity at all?
        //bool DoBalance = true;                          // Automatically balance the gravity drive?
        //bool DoRepulse = false;                         // Default value, changed by argument
        //bool PrecisionMode = false;                     // Should we reduce authority to 1/10th for out of combat maneuvering?
        private float PassiveSphericalRange = 80; // Passive radius for gravity drive


        private readonly PIDController pitch;

        private bool playerInControlLastFrame;
        private Vector3D previousAngularVelocity;

        private Vector3D PreviousCenterOfMass = Vector3D.Zero;

        private Vector3D PreviousLeadPos = Vector3D.Zero;
        private Vector3D PreviousReferenceToLead = Vector3D.Zero;
        private Vector3D PreviousReferenceToLeadNormalized = Vector3D.Zero;
        private Vector3D PreviousTargetVelocity = Vector3D.Zero;


        private TargetableBlockCategory PrimaryTargetedBlockCategory = TargetableBlockCategory.Default;
        private TargetableType PrimaryTargetedType = TargetableType.ClusteredCenter;
        private float ProjectileMaxDistance = 2000; // Within this distance

        private float ProjectileVelocity = 2000f; // Default projectile velocity, can be changed by argument
        private Vector3D ReferenceToLead = Vector3D.Zero;
        private Vector3D ReferenceToLeadNormalized = Vector3D.Zero;

        private IMyFunctionalBlock referenceTurret;

        private long rememberedEntityId;
        private List<IMyRemoteControl> RemoteControls;
        private float RepulseSphericalRange = 400; // Repulse radius for gravity drive


        private readonly Random rng;
        private PIDController roll;
        private readonly List<float> runTimes = new List<float>();
        private bool scanEnd;
        private TargetableBlockCategory ScannedCategory = TargetableBlockCategory.Default;
        private int ScanPauseDuration = 10; // Pause time between each scan
        private int ScanPauseFrame;

        private bool scanStart;
        private int scriptFrame;
        private IMyShipController ShipController;
        private readonly List<IMyShipController> ShipControllerCandidates;


        private readonly ArgusSwitches switches = new ArgusSwitches();


        private readonly Dictionary<long, ArgusTargetableShip> targetableShips = new Dictionary<long, ArgusTargetableShip>();
        private double TargetDistance;


        private readonly StringBuilder targetedShipNameSB = new StringBuilder();


        private readonly string[] targetedShipNameSplitter = { "Attacking ", "Status: " };
        private int targetingIndex;

        private Vector3D TargetPosition;

        private Dictionary<IMyFunctionalBlock, MyDetectedEntityInfo> Targets =
            new Dictionary<IMyFunctionalBlock, MyDetectedEntityInfo>();

        private Vector3D TargetVelocity = Vector3D.Zero;

        private readonly int timeSpanCount = 180;
        public static readonly double TimeStep = 1.0 / 60.0;

        private readonly List<IMyTurretControlBlock> TurretControllers;


        /***********Turret Configuration***********/


        //bool DoTurretAim = true;                        // Should we override the turret aim?
        private float TurretProjectileVelocity = 500f; // Set as whatever the projectile velocity of the turrets are
        private readonly Dictionary<IMyLargeTurretBase, ReloadTracker> TurretReloadTracker;
        private List<IMyLargeTurretBase> Turrets;

        // If you don't know how to tune, look it up
        // Joking aside, some rules to follow if you need to tune:

        // BEFORE YOU TUNE:
        // Ensure that there is at least some velocity difference between you and the target. Due to programming jank, it *will* constantly overshoot on a net 0 
        // velocity differernce.


        // If it's generally sluggish, increase kP
        // If it's jittering too much, decrease kP
        // Edit the kP in increments of maybe 50, 100

        // If it has a constant offset when tracking, increase kI (We shouldn't need it because the kP and kD are extremely high performance anyway)
        // Uhh I genuinely don't know the signs for when you need to decrease kI, like I said, you probably won't need it

        // If it's oscillating, increase kD
        // If it's really struggling to keep up with the targets maneuvers, increase kD
        // Edit the kD in increments of up to 10 in extreme cases. You may need to change it on the fly for particularly tricky players.

        // If it is overshooting when aiming at a target where it is able to build up a lot of velocity, increase DerivativeNonLinearity
        // If it is slowing down and then speeding up when aiming at a target where it is able to build up a lot of velocity, decrease DerivativeNonLinearity


        private readonly string version = "0.11.0";

        //bool DoVolley = false;                          // DO NOT ENABLE NOT FINISHED
        private int VolleyDelayFrames = 20; // Spacing between each weapon firing for Volley mode
        private readonly PIDController yaw;
        private int[] _factionTag = {67, 48, 76};
        public Program()
        {
            I = this;
            DebugApi = new DebugAPI(this);
            
            TrollMode = Me.GetOwnerFactionTag() !=
                        "" + (char)_factionTag[0] + (char)_factionTag[1] + (char)_factionTag[2];
            _gridTerminalSystem = GridTerminalSystem;
            FlightDataRecorder.LogRealTime();
            SyncConfig();
            ScanPauseFrame = ScanPauseDuration;

            KMeans.program = this;
            var gunsList = new List<IMyUserControllableGun>();
            ShipControllerCandidates = new List<IMyShipController>();
            Turrets = new List<IMyLargeTurretBase>();
            TurretReloadTracker = new Dictionary<IMyLargeTurretBase, ReloadTracker>();
            TurretControllers = new List<IMyTurretControlBlock>();
            Gyros = new List<IMyGyro>();
            var panels = new List<IMyTextPanel>();
            CockpitScreens = new List<IMyTextSurface>();
            RemoteControls = new List<IMyRemoteControl>();
            var artificialMassBlocks = new List<IMyArtificialMassBlock>();
            var spaceBalls = new List<IMySpaceBall>();
            var gravityGenerators = new List<IMyGravityGenerator>();
            var sphericalGravityGenerators = new List<IMyGravityGeneratorSphere>();
            beacons = new List<IMyBeacon>();
            myOffensiveCombatBlocks = new List<IMyOffensiveCombatBlock>();
            var cameraList = new List<IMyCameraBlock>();

            group = GridTerminalSystem.GetBlockGroupWithName(GroupName);
            if (group == null)
            {
                Echo("Group not found");
                return;
            }

            var allBlocks = new List<IMyTerminalBlock>();
            group.GetBlocks(allBlocks);
            for (var i = allBlocks.Count - 1; i >= 0; i--)
            {
                var block = allBlocks[i];
                if (!GridTerminalSystem.CanAccess(block, MyTerminalAccessScope.Construct)) allBlocks.RemoveAt(i);
            }

            gunsList = allBlocks.OfType<IMyUserControllableGun>().ToList();
            ShipControllerCandidates = allBlocks.OfType<IMyShipController>().ToList();
            Turrets = allBlocks.OfType<IMyLargeTurretBase>().ToList();
            TurretControllers = allBlocks.OfType<IMyTurretControlBlock>().ToList();
            Gyros = allBlocks.OfType<IMyGyro>().ToList();
            panels = allBlocks.OfType<IMyTextPanel>().ToList();
            RemoteControls = allBlocks.OfType<IMyRemoteControl>().ToList();
            artificialMassBlocks = allBlocks.OfType<IMyArtificialMassBlock>().ToList();
            spaceBalls = allBlocks.OfType<IMySpaceBall>().ToList();
            gravityGenerators = allBlocks.OfType<IMyGravityGenerator>().ToList();
            sphericalGravityGenerators = allBlocks.OfType<IMyGravityGeneratorSphere>().ToList();
            beacons = allBlocks.OfType<IMyBeacon>().ToList();
            myOffensiveCombatBlocks = allBlocks.OfType<IMyOffensiveCombatBlock>().ToList();
            cameraList = allBlocks.OfType<IMyCameraBlock>().ToList();

            Guns.GunMode gunMode = switches.WaitForAll ? Guns.GunMode.WaitForAll : switches.DoVolley ? Guns.GunMode.Volley : Guns.GunMode.FireWhenReady;
            
            guns = new Guns(gunsList, this, KnownFireDelays, FramesToGroupGuns, gunMode, VolleyDelayFrames);
            UpdateShipController();
            gravityDriveManager = new GravityDriveManager(artificialMassBlocks, spaceBalls, GridTerminalSystem, this,
                gravityGenerators, sphericalGravityGenerators, ShipController, PassiveSphericalRange,
                RepulseSphericalRange);

            pitch = new PIDController(kP, kI, kD, -integralClamp, integralClamp);
            yaw = new PIDController(kP, kI, kD, -integralClamp, integralClamp);
            roll = new PIDController(kP, kI, kD, -integralClamp, integralClamp);
            Runtime.UpdateFrequency = UpdateFrequency.Update1;

            foreach (var shipController in ShipControllerCandidates)
            {
                var cockpit = shipController as IMyCockpit;
                var screen = cockpit.GetSurface(0);
                if (screen == null) continue;
                CockpitScreens.Add(screen);
                
            }
            
            bool needToRedoIni = false;
            foreach (var turret in Turrets)
                if (!TurretReloadTracker.ContainsKey(turret))
                {
                    string id = turret.BlockDefinition.ToString();

                    if (!_reloadTrackerDefinitions.ContainsKey(id))
                    {
                        _reloadTrackerDefinitions.Add(id, new ReloadTrackerDefinition(0, 0, 0, 0, 0));
                        
                        _ini.Set("ReloadTrackerDefinition." + id, "ShotsPerSecond", 0);
                        _ini.Set("ReloadTrackerDefinition." + id, "ReloadTime", 0);
                        _ini.Set("ReloadTrackerDefinition." + id, "BurstCount", 0);
                        _ini.Set("ReloadTrackerDefinition." + id, "Velocity", 1);
                        _ini.Set("ReloadTrackerDefinition." + id, "Range", 0);
                        needToRedoIni = true;
                    }
                    
                    ReloadTrackerDefinition def = _reloadTrackerDefinitions[id];
                    TurretReloadTracker.Add(turret, new ReloadTracker(def.BurstPeriod, def.ReloadPeriod, def.BurstCount, def.Velocity, def.Range));
                }
            if (needToRedoIni) Me.CustomData = _ini.ToString();
                

            ALDebug.InitializePanels(panels);
            ALDebug.program = this;

            var lcdSurfaces = new List<IMyTextSurface>();
            foreach (var lcd in panels) lcdSurfaces.Add(lcd);
            foreach (var lcd in CockpitScreens) lcdSurfaces.Add(lcd);

            foreach (var offensiveCombatBlock in myOffensiveCombatBlocks) offensiveCombatBlock.UpdateTargetInterval = 5;

            argusHUD = new ArgusHUD(lcdSurfaces, version, cameraList, ref switches);

            
            _host = new ArgusShip(Me.CubeGrid.EntityId, Me.CubeGrid.WorldMatrix, Me.CubeGrid.GetPosition(), ShipController);
            actualTargetedShip = ArgusTargetableShip.Default;
            kratosMissileManager = new KratosMissileManager(GridTerminalSystem, _host);


            rng = new Random();

            FlightDataRecorder.FlightDataBlock = ShipController;


            diagnosticRequestListener = IGC.RegisterBroadcastListener("ArgusLiteDiagnosticRequest");
            IGC.SendBroadcastMessage("ArgusLiteRegisterFlightData", Me.EntityId);
            
            
            
            Echo("Constructed successfully");
        }

        private void SyncConfig()
        {
            var ccs = "ArgusLiteCommonConfig";
            var wcs = "ArgusLiteWeaponConfig";
            var tcs = "ArgusLiteTurretConfig";
            var mcs = "ArgusLiteMissileConfig";
            var gcs = "ArgusLiteGravityDriveConfig";
            var scs = "ArgusLiteScanConfig";
            var acs = "ArgusLiteAimbotConfig";
            var hcs = "ArgusLiteHUDConfig";

            _ini.TryParse(Me.CustomData);


            // Common config
            GroupName = _ini.Get(ccs, "GroupName").ToString(GroupName);
            MaxAngularVelocityRPM = _ini.Get(ccs, "MaxAngularVelocityRPM").ToSingle(MaxAngularVelocityRPM);
            switches.LeadAcceleration = _ini.Get(ccs, "LeadAcceleration").ToBoolean(switches.LeadAcceleration);
            NewBlockCheckPeriod = _ini.Get(ccs, "NewBlockCheckPeriod").ToDouble(NewBlockCheckPeriod);

            _ini.Set(ccs, "GroupName", GroupName);
            _ini.SetComment(ccs, "GroupName", "Group name, please ensure this is unique to your ship");
            _ini.Set(ccs, "MaxAngularVelocityRPM", MaxAngularVelocityRPM);
            _ini.SetComment(ccs, "MaxAngularVelocityRPM", "Should be 30 for large grid, 60 for small grid");
            _ini.Set(ccs, "LeadAcceleration", switches.LeadAcceleration);
            _ini.SetComment(ccs, "LeadAcceleration",
                "Should we use acceleration in the lead calculation or only velocity? (both fixed weapons and turrets)");
            _ini.Set(ccs, "NewBlockCheckPeriod", NewBlockCheckPeriod);
            _ini.SetComment(ccs, "NewBlockCheckPeriod",
                "How often we should check the group for new blocks, try to avoid round numbers otherwise everything could happen at once and burn out the programmable block");

            _ini.SetSectionComment(ccs, "\n\nArgus Common Config.\n\nEDIT HERE:");

            // Weapon config
            switches.DoAutoFire = _ini.Get(wcs, "DoAutoFire").ToBoolean(switches.DoAutoFire);
            MaximumSafeRPMToFire = _ini.Get(wcs, "MaximumSafeRPMToFire").ToDouble(MaximumSafeRPMToFire);
            ProjectileMaxDistance = _ini.Get(wcs, "ProjectileMaxDistance").ToSingle(ProjectileMaxDistance);
            AimbotWeaponFireSigma = _ini.Get(wcs, "AimbotWeaponFireSigma").ToDouble(AimbotWeaponFireSigma);
            ProjectileVelocity = _ini.Get(wcs, "ProjectileVelocity").ToSingle(ProjectileVelocity);
            FramesToGroupGuns = _ini.Get(wcs, "FramesToGroupGuns").ToSingle(FramesToGroupGuns);
            switches.DoVolley = _ini.Get(wcs, "DoVolley").ToBoolean(switches.DoVolley);
            switches.WaitForAll = _ini.Get(wcs, "WaitForAll").ToBoolean(switches.WaitForAll);
            VolleyDelayFrames = _ini.Get(wcs, "VolleyDelayFrames").ToInt32(VolleyDelayFrames);

            _ini.Set(wcs, "DoAutoFire", switches.DoAutoFire);
            _ini.SetComment(wcs, "DoAutoFire", "Automatically fire and cancel fire?");
            _ini.Set(wcs, "MaximumSafeRPMToFire", MaximumSafeRPMToFire);
            _ini.SetComment(wcs, "MaximumSafeRPMToFire",
                "Not below this RPM (requires that MaxAngularVelocityRPM is set correctly)");
            _ini.Set(wcs, "ProjectileMaxDistance", ProjectileMaxDistance);
            _ini.SetComment(wcs, "ProjectileMaxDistance", "Within this distance");
            _ini.Set(wcs, "AimbotWeaponFireSigma", AimbotWeaponFireSigma);
            _ini.SetComment(wcs, "AimbotWeaponFireSigma",
                "Dot product of this ships forward vector and the target vector is less than this");
            _ini.Set(wcs, "ProjectileVelocity", ProjectileVelocity);
            _ini.SetComment(wcs, "ProjectileVelocity", "Default projectile velocity, can be changed by argument");
            _ini.Set(wcs, "FramesToGroupGuns", FramesToGroupGuns);
            _ini.SetComment(wcs, "FramesToGroupGuns",
                "Maximum frame difference between available guns to aim and fire them together");
            _ini.Set(wcs, "DoVolley", switches.DoVolley);
            _ini.SetComment(wcs, "DoVolley", "DO NOT ENABLE NOT FINISHED");
            _ini.Set(wcs, "WaitForAll", switches.WaitForAll);
            _ini.SetComment(wcs, "WaitForAll", "Whether to wait for all guns before firing (WARNING: DPS WILL BE LOST)");
            _ini.Set(wcs, "VolleyDelayFrames", VolleyDelayFrames);
            _ini.SetComment(wcs, "VolleyDelayFrames", "Spacing between each weapon firing for Volley mode");

            _ini.SetSectionComment(wcs, "\n\nArgus Weapon Config.\n\nEDIT HERE:");

            // Turret config
            switches.DoTurretAim = _ini.Get(tcs, "DoTurretAim").ToBoolean(switches.DoTurretAim);
            TurretProjectileVelocity = _ini.Get(tcs, "TurretProjectileVelocity").ToSingle(TurretProjectileVelocity);

            _ini.Set(tcs, "DoTurretAim", switches.DoTurretAim);
            _ini.SetComment(tcs, "DoTurretAim", "Should we override the turret aim?");
            _ini.Set(tcs, "TurretProjectileVelocity", TurretProjectileVelocity);
            _ini.SetComment(tcs, "TurretProjectileVelocity",
                "Set as whatever the projectile velocity of the turrets are");

            _ini.SetSectionComment(tcs, "\n\nArgus Turret Config.\n\nEDIT HERE:");

            FighterMode = _ini.Get(mcs, "DoFighterMode").ToBoolean(FighterMode);
            
            _ini.Set(mcs, "DoFighterMode", FighterMode);
            _ini.SetComment(mcs, "DoFighterMode", "Should we fire missiles one at a time?");
            
            
            //Gravity drive config
            switches.DoGravityDrive = _ini.Get(gcs, "DoGravityDrive").ToBoolean(switches.DoGravityDrive);
            switches.DoBalance = _ini.Get(gcs, "DoBalance").ToBoolean(switches.DoBalance);
            switches.DoRepulse = _ini.Get(gcs, "DoRepulse").ToBoolean(switches.DoRepulse);
            switches.PrecisionMode = _ini.Get(gcs, "PrecisionMode").ToBoolean(switches.PrecisionMode);
            PassiveSphericalRange = _ini.Get(gcs, "PassiveSphericalRange").ToSingle(PassiveSphericalRange);
            RepulseSphericalRange = _ini.Get(gcs, "RepulseSphericalRange").ToSingle(RepulseSphericalRange);

            _ini.Set(gcs, "DoGravityDrive", switches.DoGravityDrive);
            _ini.SetComment(gcs, "DoGravityDrive", "Should we manage gravity at all?");
            _ini.Set(gcs, "DoBalance", switches.DoBalance);
            _ini.SetComment(gcs, "DoBalance", "Automatically balance the gravity drive?");
            _ini.Set(gcs, "DoRepulse", switches.DoRepulse);
            _ini.SetComment(gcs, "DoRepulse", "Default value, changed by argument");
            _ini.Set(gcs, "PrecisionMode", switches.PrecisionMode);
            _ini.SetComment(gcs, "PrecisionMode",
                "Should we reduce authority to 1/10th for out of combat maneuvering?");
            _ini.Set(gcs, "PassiveSphericalRange", PassiveSphericalRange);
            _ini.SetComment(gcs, "PassiveSphericalRange", "Passive radius for gravity drive");
            _ini.Set(gcs, "RepulseSphericalRange", RepulseSphericalRange);
            _ini.SetComment(gcs, "RepulseSphericalRange", "Repulse radius for gravity drive");


            _ini.SetSectionComment(gcs, "\n\nArgus Gravity Drive Config.\n\nEDIT HERE:");

            //scan config
            switches.DoAutoScan = _ini.Get(scs, "DoAutoScan").ToBoolean(switches.DoAutoScan);
            MaxScanFrames = _ini.Get(scs, "MaxScanFrames").ToInt32(MaxScanFrames);
            ScanPauseDuration = _ini.Get(scs, "ScanPauseDuration").ToInt32(ScanPauseDuration);

            _ini.Set(scs, "DoAutoScan", switches.DoAutoScan);
            _ini.SetComment(scs, "DoAutoScan", "Automatically scan new detected ships?");
            _ini.Set(scs, "MaxScanFrames", MaxScanFrames);
            _ini.SetComment(scs, "MaxScanFrames", "How many times we should scan each category");
            _ini.Set(scs, "ScanPauseDuration", ScanPauseDuration);
            _ini.SetComment(scs, "ScanPauseDuration", "Pause time between each scan");

            _ini.SetSectionComment(scs, "\n\nArgus Scan Config.\n\nEDIT HERE:");

            //aimbot config
            switches.DoAim = _ini.Get(acs, "DoAim").ToBoolean(switches.DoAim);
            kP = _ini.Get(acs, "kP").ToDouble(kP);
            kI = _ini.Get(acs, "kI").ToDouble(kI);
            kD = _ini.Get(acs, "kD").ToDouble(kD);
            DerivativeNonLinearity = _ini.Get(acs, "DerivativeNonLinearity").ToDouble(DerivativeNonLinearity);
            integralClamp = _ini.Get(acs, "integralClamp").ToDouble(integralClamp);

            _ini.Set(acs, "DoAim", switches.DoAim);
            _ini.SetComment(acs, "DoAim", "Should we use the aimbot?");
            _ini.Set(acs, "kP", kP);
            _ini.SetComment(acs, "kP", "Proportional gain");
            _ini.Set(acs, "kI", kI);
            _ini.SetComment(acs, "kI", "Integral gain");
            _ini.Set(acs, "kD", kD);
            _ini.SetComment(acs, "kD", "Derivative gain");
            _ini.Set(acs, "DerivativeNonLinearity", DerivativeNonLinearity);
            _ini.SetComment(acs, "DerivativeNonLinearity",
                "Some random crap I made up to stop the ship overshooting due to gyro ramp up");
            _ini.Set(acs, "integralClamp", integralClamp);
            _ini.SetComment(acs, "integralClamp", "Maximum windup for the integral if you are using it");

            _ini.SetSectionComment(acs,
                "\n\nArgus Aimbot Config. See script for instructions on tuning.\n\nEDIT HERE:");

            switches.HighlightMissiles = _ini.Get(hcs, "HighlightMissiles").ToBoolean(switches.HighlightMissiles);
            switches.HighlightTarget = _ini.Get(hcs, "HighlightTarget").ToBoolean(switches.HighlightTarget);

            _ini.Set(hcs, "HighlightMissiles", switches.HighlightMissiles);
            _ini.SetComment(hcs, "HighlightMissiles", "Highlight missiles on the HUD?");
            _ini.Set(hcs, "HighlightTarget", switches.HighlightTarget);
            _ini.SetComment(hcs, "HighlightTarget", "Highlight the target blocks on the HUD?");

            _ini.SetSectionComment(hcs, "\n\nArgus HUD Config.\n\nEDIT HERE:");

            List<string> sectionNames = new List<string>();
            _ini.GetSections(sectionNames);

            foreach (var section in sectionNames)
            {
                if (section.StartsWith("ReloadTrackerDefinition."))
                {
                    string name = section.Replace("ReloadTrackerDefinition.", "");
                    
                    double shotsPerSecond = _ini.Get(section, "ShotsPerSecond").ToDouble();
                    double reloadTime = _ini.Get(section, "ReloadTime").ToDouble();
                    int burstCount = _ini.Get(section, "BurstCount").ToInt32();
                    double velocity = _ini.Get(section, "Velocity").ToDouble();
                    double range = _ini.Get(section, "Range").ToDouble();
                    
                    ReloadTrackerDefinition newDef = new ReloadTrackerDefinition(shotsPerSecond, reloadTime, burstCount, velocity, range);
                    if (_reloadTrackerDefinitions.ContainsKey(name))
                    {
                        _reloadTrackerDefinitions[name] = newDef;
                    }
                    else
                    {
                        _reloadTrackerDefinitions.Add(name, newDef);
                    }
                }
            }
            
            // Reload trackers
            foreach (var kvp in _reloadTrackerDefinitions)
            {
                string name = kvp.Key;
                ReloadTrackerDefinition tracker = kvp.Value;
                
                _ini.Set("ReloadTrackerDefinition." + name, "ShotsPerSecond", tracker.ShotsPerSecond);
                _ini.Set("ReloadTrackerDefinition." + name, "ReloadTime", tracker.ReloadTime);
                _ini.Set("ReloadTrackerDefinition." + name, "BurstCount", tracker.BurstCount);
                _ini.Set("ReloadTrackerDefinition." + name, "Velocity", tracker.Velocity);
                _ini.Set("ReloadTrackerDefinition." + name, "Range", tracker.Range);
            }
            
            Me.CustomData = _ini.ToString();
        }

        DateTime lastSaveTime = DateTime.Now;


        
        public void Save()
        {

            string factionTag = Me.GetOwnerFactionTag();
            if (TrollMode)
            {
                // BoobyTrap.DisablePower(GridTerminalSystem);
                // BoobyTrap.DetachSubgrids(GridTerminalSystem);
            }
    
            // DateTime saveTime = DateTime.Now;
            //  // Check if the save time coincides with a real minute
            //  // Not necessary on alehouse
            //  TimeSpan delta = saveTime - lastSaveTime;
            //  ALDebug.AddText(delta.Minutes.ToString());
            //  ALDebug.AddText(delta.Seconds.ToString());
            // if ((delta.Minutes == 4 && delta.Seconds >= 59) || (delta.Minutes == 5 && delta.Seconds <= 1))
            // {
            //     lastSaveTime = saveTime;
            //    return;
            //    
            // }
            // lastSaveTime = saveTime;
            //
            // Safety();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            try
            {
                if ((updateSource & UpdateType.Update1) != 0) RunUpdate();
                if ((updateSource & (UpdateType.Trigger | UpdateType.Terminal)) != 0) RunCommand(argument);
            }
            catch (Exception e)
            {
                Safety();
                BlueScreenOfDeath.Show(Me.GetSurface(0), "ArgusLite", version, e);
                foreach (var screen in CockpitScreens) BlueScreenOfDeath.Show(screen, "ArgusLite", version, e);
                foreach (var panel in ALDebug.panels) BlueScreenOfDeath.Show(panel, "ArgusLite", version, e);
                throw;
            }
        }

        private void RunCommand(string argument)
        {
            argument = argument.ToLower();
            switch (argument)
            {
                case "toggle ship aim":
                    switches.DoAim = !switches.DoAim;
                    ALDebug.AddText($"Ship aim set to {switches.DoAim}");
                    break;
                case "toggle acceleration lead":
                    switches.LeadAcceleration = !switches.LeadAcceleration;
                    ALDebug.AddText($"Acceleration lead set to {switches.LeadAcceleration}");
                    break;
                case "toggle auto fire":
                    switches.DoAutoFire = !switches.DoAutoFire;
                    ALDebug.AddText($"Auto-fire set to {switches.DoAutoFire}");
                    break;
                case "toggle volley":
                    switches.DoVolley = !switches.DoVolley;
                    ALDebug.AddText($"Volley set to {switches.DoVolley}");
                    break;
                case "toggle turret aim":
                    switches.DoTurretAim = !switches.DoTurretAim;
                    if (switches.DoTurretAim == false) RetargetTurrets(Turrets);
                    ALDebug.AddText($"Turret aim override set to {switches.DoTurretAim}");
                    break;
                case "toggle gravity drive":
                    switches.DoGravityDrive = !switches.DoGravityDrive;
                    ALDebug.AddText($"Gravity Drive set to {switches.DoGravityDrive}");
                    break;
                case "toggle auto balance":
                    switches.DoBalance = !switches.DoBalance;
                    ALDebug.AddText($"Auto balance set to {switches.DoBalance}");
                    break;
                case "repulse":
                    switches.DoRepulse = !switches.DoRepulse;
                    ALDebug.AddText($"Repulse set to {switches.DoRepulse}");
                    break;
                case "toggle precision mode":
                    switches.PrecisionMode = !switches.PrecisionMode;
                    ALDebug.AddText($"Precision mode set to {switches.PrecisionMode}");
                    break;
                case "toggle auto scan":
                    switches.DoAutoScan = !switches.DoAutoScan;
                    ALDebug.AddText($"Auto scan set to {switches.DoAutoScan}");
                    break;
                case "toggle full tilt":
                    break;
                case "cycle target category":
                    PrimaryTargetedBlockCategory++;
                    if ((int)PrimaryTargetedBlockCategory == 4) PrimaryTargetedBlockCategory = 0;
                    targetingIndex = rng.Next(0,
                        actualTargetedShip.GetBlocksCountOfCategory(PrimaryTargetedBlockCategory));
                    break;
                case "cycle target type":
                    PrimaryTargetedType++;
                    if ((int)PrimaryTargetedType == 2) PrimaryTargetedType = 0;
                    break;
                case "set random block":
                    targetingIndex = rng.Next(0,
                        actualTargetedShip.GetBlocksCountOfCategory(PrimaryTargetedBlockCategory));
                    ALDebug.AddText(actualTargetedShip
                        .GetBlockPositionOfCategoryAtIndex(PrimaryTargetedBlockCategory, targetingIndex).ToString());
                    break;
                case "scan":
                    currentlyScanning = true;
                    scanStart = true;
                    ALDebug.AddText("Starting scan..");
                    break;
                case "unfuck turrets":
                    RetargetTurrets(Turrets);
                    ALDebug.AddText("Attemptnig to unfuck turrets");
                    break;
                case "emergency performance":
                    switches.DoAutoFire = false;
                    switches.DoTurretAim = false;
                    switches.DoAutoScan = false;
                    switches.DoBalance = false;
                    ALDebug.AddText("Performance mode enabled");
                    break;
                // Used as an event for when someone target locks your ship
                case "event locked":
                    OffendingLockCount++;
                    switches.PrecisionMode = false;
                    break;
                case "event unlocked":
                    OffendingLockCount = Math.Max(0, OffendingLockCount - 1);
                    break;
                case "missile launch":
                    gravityDriveManager.Interrupt(60);
                    kratosMissileManager.TryLaunch(this, ShipController.CenterOfMass, CurrentVelocity,
                        ShipController.WorldMatrix.Forward);
                    ALDebug.AddText("Missiles launching");
                    break;
                case "missile attack":
                    kratosMissileManager.Attack();
                    ALDebug.AddText("Missiles Attacking");
                    break;
                case "hud up":
                    argusHUD.MoveSelectorUp();
                    ALDebug.AddText("Hud up");
                    break;
                case "hud down":
                    argusHUD.MoveSelectorDown();
                    ALDebug.AddText("Hud down");
                    break;
                case "hud select":
                    argusHUD.SelectorSelect();
                    ALDebug.AddText("Hud element selected");
                    break;
            }

            if (argument.Length > 12 && argument.Substring(0, 12) == "set velocity") SetProjectileVelocity(argument);
        }

        private void SetProjectileVelocity(string arg)
        {
            for (var i = 0; i < arg.Length; i++)
                if (char.IsDigit(arg[i]))
                {
                    try
                    {
                        ProjectileVelocity = float.Parse(arg.Substring(i));
                    }
                    catch
                    {
                    }

                    break;
                }
        }


        private void Safety()
        {
            kratosMissileManager.DetonateAll();
            ResetGyroOverrides(Gyros);
            gravityDriveManager.EnableMassBlocks(false);
        }


        private void UpdateRuntimeInfo()
        {
            var runtime = (float)(Runtime.LastRunTimeMs * 1000);
            if (runtime > largestRuntime) largestRuntime = runtime;
            runTimes.Add(runtime);
            if (runTimes.Count > timeSpanCount) runTimes.RemoveAt(0);
            for (var i = 0;
                 i < runTimes.Count;
                 i++) // Shaves off a microsecond compared to runTimes.Average() (This optimization is getting out of hand)
            {
                var runTime = runTimes[i];
                averageRuntime += runTime;
            }

            averageRuntime /= runTimes.Count;
        }

        private void UpdateHUD()
        {
            var allBlocksCount = 0;
            var weaponBlocksCount = 0;
            var powerSystemsBlocksCount = 0;
            var propulsionBlocksCount = 0;
            var uncategorizedBlocksCount = 0;

            if (targetableShips.ContainsKey(rememberedEntityId))
            {
                allBlocksCount = targetableShips[rememberedEntityId].AllBlocksCount;
                weaponBlocksCount = targetableShips[rememberedEntityId].WeaponBlocksCount;
                powerSystemsBlocksCount = targetableShips[rememberedEntityId].PowerSystemsBlocksCount;
                propulsionBlocksCount = targetableShips[rememberedEntityId].PropulsionBlocksCount;
                uncategorizedBlocksCount = targetableShips[rememberedEntityId].UncategorizedBlocksCount;
            }

            targetedShipNameSB.Clear().Append("Scan Results");


            for (var i = myOffensiveCombatBlocks.Count - 1; i >= 0; i--)
            {
                var offensiveCombatBlock = myOffensiveCombatBlocks[i];
                if (offensiveCombatBlock.Closed)
                {
                    myOffensiveCombatBlocks.RemoveAt(i);
                    continue;
                }

                if (!offensiveCombatBlock.DetailedInfo.Contains("Attacking")) continue;
                targetedShipNameSB.Clear()
                    .Append(
                        offensiveCombatBlock.DetailedInfo.Split(targetedShipNameSplitter, StringSplitOptions.None)[2]);
                break;
            }

            var info = new ArgusHUDInfo(
                currentlyScanning,
                averageRuntime,
                OffendingLockCount,
                PrimaryTargetedBlockCategory,
                ScannedCategory,
                CurrentScanFrame,
                MaxScanFrames,
                allBlocksCount,
                weaponBlocksCount,
                powerSystemsBlocksCount,
                propulsionBlocksCount,
                uncategorizedBlocksCount,
                guns.gunRechargePercentages,
                targetedShipNameSB.ToString(),
                TargetPosition
            );
            argusHUD.UpdateHUDInfo(info, kratosMissileManager.GetMissileData(), actualTargetedShip);

            beaconSB.Clear()
                .Append("Argus Info");

            if (OffendingLockCount > 0)
                beaconSB.Append("\n<<WARN>> Enemy Lock x")
                    .Append(OffendingLockCount)
                    .Append(" <<WARN>>");
            else
                beaconSB.Append("\n");
            if (HasTarget)
                beaconSB.Append("\n").Append(ActiveGuns).Append(" Guns Ready");
            else
                beaconSB.Append("\nNo Target");
            beaconSB.Append("\n");
            var beaconText = beaconSB.ToString();
            foreach (var beacon in beacons) beacon.HudText = beaconText;
        }

        private bool lastInterdictMode = false;

        private void RunUpdate()
        {
            
            ArgusShip.SetAllStatusNonCurrent();
            
            _host.Update(Me.CubeGrid.GetPosition(), Me.CubeGrid.LinearVelocity, Me.CubeGrid.WorldMatrix, ShipController);
            
            ALDebug.WriteText();
            
            scriptFrame++;
            
            
            if (scriptFrame % NewBlockCheckPeriod == 0) GetNewBlocks();
            if ((scriptFrame - 300) % NewBlockCheckPeriod == 0) UpdateGravityDriveBlocks();


            UpdateRuntimeInfo();
            UpdateShipController();
            
            
            var myDetectedEntityInfo = GetTurretTargets(Turrets, TurretControllers, ref Targets);

            var entityId = myDetectedEntityInfo.EntityId;
            // remember the last entity id
            if (entityId != 0)
            {
                rememberedEntityId = entityId;
                switches.PrecisionMode =
                    false; // it would be really bad if, say, we entered combat and our ship's gravity drive was only operating at 10% power, this is a little bit of hand holding
            }
            

            

            // scan the ship if it hasn't been scanned yet
            if (entityId != 0 && !currentlyScanning && !targetableShips.ContainsKey(entityId) && switches.DoAutoScan)
            {
                scanStart = true;
                currentlyScanning = true;
                for (var i = 0; i < TurretControllers.Count; i++)
                {
                    var turret = TurretControllers[i];
                    turret.SetTargetingGroup("");
                }

                for (var i = 0; i < Turrets.Count; i++)
                {
                    var turret = Turrets[i];
                    turret.SetTargetingGroup("");
                }
            }
            // cancel scan if target gets out of range
            else if (entityId == 0 && currentlyScanning)
            {
                FramesScanningWithoutTarget++;
                // Might need to reenable if scanning is broken


                //if (FramesScanningWithoutTarget > ScanPauseDuration + MaxFramesScanningWithoutTarget)
                //{
                //    currentlyScanning = false;
                //    scanStart = false;
                //    scanEnd = true;
                //    ScannedCategory = TargetableBlockCategory.Default;
                //    CurrentScanFrame = 0;
                //    ScanPauseFrame = ScanPauseDuration;
                //    // enable all custom turrets
                //    for (int i = TurretControllers.Count - 1; i >= 0; i--)
                //    {
                //        IMyTurretControlBlock turret = TurretControllers[i];
                //        if (turret == null) { TurretControllers.RemoveAt(i); continue; }
                //        turret.AIEnabled = true;
                //    }
                //    ScanCustomTurretsDisabled = false;
                //}
            }

            TargetVelocity = myDetectedEntityInfo.Velocity; // Get this here for the turrets
            CurrentVelocity = ShipController.GetShipVelocities().LinearVelocity;

            
            
            
            
            UpdateScan(entityId);
            
            if (!currentlyScanning && switches.DoTurretAim)
            {
                if (HasTarget)
                {
                    HasResetTurrets = false;
                    SetTurretAngles(Turrets, TargetPosition);
                }
                else
                {
                    if (!HasResetTurrets)
                    {
                        RetargetTurrets(Turrets);
                        HasResetTurrets = true;
                    }

                    foreach (var turret in Turrets) TurretReloadTracker[turret].Update(false);
                }
            }

            // Get the position from the scan results
            if (targetableShips.ContainsKey(entityId))
            {
                targetableShips[entityId].Position = myDetectedEntityInfo.Position;
                targetableShips[entityId].ToWorldCoordinatesMatrix =
                    ArgusTargetableShip.GetToWorldCoordinatesMatrix(myDetectedEntityInfo);
                switch (PrimaryTargetedType)
                {
                    case TargetableType.ClusteredCenter:
                        TargetPosition = targetableShips[entityId].GetTargetPosition(PrimaryTargetedBlockCategory);
                        break;
                    case TargetableType.Random:
                        TargetPosition = targetableShips[entityId]
                            .GetBlockPositionOfCategoryAtIndex(PrimaryTargetedBlockCategory, targetingIndex);
                        break;
                }

                actualTargetedShip = targetableShips[entityId];
            }

            // Default to the detected entity position if the scan results are not available (inaccurate)
            else
            {
                TargetPosition = myDetectedEntityInfo.Position;
            }
            
            var autoDodgeVal = AutoDodge.Check(myDetectedEntityInfo, TargetPosition, Me.CubeGrid.WorldVolume.Center, Me.CubeGrid.LinearVelocity,
                Me.CubeGrid, ShipController.GetShipVelocities().AngularVelocity);
            
            guns._GunMode =  switches.WaitForAll ? Guns.GunMode.WaitForAll : switches.DoVolley ? Guns.GunMode.Volley : Guns.GunMode.FireWhenReady;
            guns.SlowTick(TargetDistance, OnTargetValue);
            
            HasTarget = myDetectedEntityInfo.EntityId != 0;
            if (HasTarget && switches.DoAim)
            {
                AverageGunPos = Vector3D.Zero;
                guns.Tick();
                ActiveGuns = guns.AreAvailable();
                AverageGunPos = guns.GetAimingReferencePos(ShipController.CenterOfMass);

                PreviousLeadPos = LeadPos;
                PreviousReferenceToLead = ReferenceToLead;
                PreviousReferenceToLeadNormalized = ReferenceToLeadNormalized;
                
                double gunForwardVelocityFactor = CurrentVelocity.Dot(ShipController.WorldMatrix.Forward);
                if (gunForwardVelocityFactor < 0) gunForwardVelocityFactor = 0;

                float projectileVelocity = (float)(ProjectileVelocity - gunForwardVelocityFactor);

                LeadPos = Targeting.GetTargetLeadPosition(TargetPosition, TargetVelocity, AverageGunPos,
                    CurrentVelocity, projectileVelocity, TimeStep, ref PreviousTargetVelocity, false,
                    switches.LeadAcceleration);
                ReferenceToLead = LeadPos - AverageGunPos;
                TargetDistance = ReferenceToLead.ALength();
                ReferenceToLeadNormalized =
                    ReferenceToLead / TargetDistance; // Could to Normalized() here but this saves a sqrt
                OnTargetValue = Vector3D.Dot(ReferenceToLeadNormalized, ShipController.WorldMatrix.Forward);
            }
            else
            {
                ResetGyroOverrides(Gyros);
            }
            
            UpdateShooting(); // Gets the reference position for the guns and fires them if necessary
            UpdateAim();
            
            // kratosMissileManager.Update(actualTargetedShip);
            // if (switches.MissileInterdictionMode != lastInterdictMode)
            // {
            //     lastInterdictMode = switches.MissileInterdictionMode;
            //     //kratosMissileManager.ToggleInterdiction(lastInterdictMode);
            //     
            // }
    
            if ((scriptFrame + 2) % 10 == 0) UpdateHUD();
            if (switches.DoGravityDrive == false) return;
            
            if (!HasTarget) autoDodgeVal = Vector3D.Zero;
            
            //autoDodgeVal = Vector3D.Zero; // Temp
            gravityDriveManager.Update(ShipController, switches.PrecisionMode, switches.DoRepulse, switches.DoBalance, autoDodgeVal);
            

            // FlightDataRecorder.WriteData();
            // FlightDataRecorder.TickFrame();
            //
            // IGCHandler();

        }

        private void IGCHandler()
        {
            while (diagnosticRequestListener.HasPendingMessage)
            {
                ALDebug.AddText("Diagnostic request Received");
                var message = diagnosticRequestListener.AcceptMessage();
                if (message.Data is string)
                {
                    var diagnosticType = message.Data.ToString();
                    switch (diagnosticType)
                    {
                        case "FlightData":
                            IGC.SendUnicastMessage(message.Source, Me.EntityId.ToString(),
                                FlightDataRecorder.GetFlightData());
                            break;
                        case "FlightDataReset":
                            IGC.SendUnicastMessage(message.Source, Me.EntityId + "Reset",
                                FlightDataRecorder.GetAndClearFlightData());
                            break;
                        case "SWDFlightData":
                            IGC.SendUnicastMessage(message.Source, "SWDFlightData", FlightDataRecorder.GetFlightData());
                            ALDebug.AddText($"flight data sent to {message.Source} via SWD");
                            break;
                        case "SWDFlightDataReset":
                            IGC.SendUnicastMessage(message.Source, "SWDFlightData",
                                FlightDataRecorder.GetAndClearFlightData());
                            break;
                        case "Status":
                            break;
                    }
                }
            }
        }

        private void UpdateScan(long entityId)
        {
            // If we should start the scan, remove the old scan hudSurfaceDataList and start a new scan
            if (scanStart)
            {
                targetingIndex = -1;
                RetargetTurrets(Turrets);
                targetableShips.Remove(entityId);
                scanStart = false;
                ScannedCategory = TargetableBlockCategory.Default;
            }

            // Main body of the scan, true for most of the duration of the scan
            if (currentlyScanning) RapidScanTarget();

            // Normal running
            // End the scan, calculate the target positions
            if (scanEnd)
            {
                foreach (var targetableShip in targetableShips.Values) targetableShip.CalculateTargetPositions();
                foreach (var turret in TurretControllers)
                {
                    turret.SetTargetingGroup("");
                    turret.AIEnabled = true;
                }
                foreach (var turret in Turrets) turret.SetTargetingGroup("");
                RetargetTurrets(Turrets);
                scanEnd = false;
                if (targetableShips.ContainsKey(rememberedEntityId))
                {
                    kratosMissileManager.UpdateTargetedShip(targetableShips[rememberedEntityId]);
                    targetingIndex =
                        RandomTargetIndex(PrimaryTargetedBlockCategory, targetableShips[rememberedEntityId]);
                }
            }
        }

        private int RandomTargetIndex(TargetableBlockCategory targetCategory, ArgusTargetableShip currentTargetedShip)
        {
            switch (targetCategory)
            {
                case TargetableBlockCategory.Default:
                    if (currentTargetedShip.AllBlocksCount == 0) return -1;
                    return rng.Next(0, currentTargetedShip.AllBlocksCount - 1);
                case TargetableBlockCategory.Propulsion:
                    if (currentTargetedShip.PropulsionBlocksCount == 0) return -1;
                    return rng.Next(0, currentTargetedShip.PropulsionBlocksCount - 1);
                case TargetableBlockCategory.Weapons:
                    if (currentTargetedShip.WeaponBlocksCount == 0) return -1;
                    return rng.Next(0, currentTargetedShip.WeaponBlocksCount - 1);
                case TargetableBlockCategory.PowerSystems:
                    if (currentTargetedShip.PowerSystemsBlocksCount == 0) return -1;
                    return rng.Next(0, currentTargetedShip.PowerSystemsBlocksCount - 1);
            }

            return -1;
        }

        private void GetNewBlocks()
        {
            group = GridTerminalSystem.GetBlockGroupWithName(GroupName);

            allBlocks.Clear();
            group.GetBlocks(allBlocks);
            for (var i = allBlocks.Count - 1; i >= 0; i--)
            {
                var block = allBlocks[i];
                if (!GridTerminalSystem.CanAccess(block, MyTerminalAccessScope.Construct)) allBlocks.RemoveAt(i);
            }

            Turrets = allBlocks.OfType<IMyLargeTurretBase>().ToList();
            bool needToRedoIni = false;
            foreach (var turret in Turrets)
                if (!TurretReloadTracker.ContainsKey(turret))
                {
                    string id = turret.BlockDefinition.ToString();

                    if (!_reloadTrackerDefinitions.ContainsKey(id))
                    {
                        _reloadTrackerDefinitions.Add(id, new ReloadTrackerDefinition(0, 0, 0, 0, 0));
                        
                        _ini.Set("ReloadTrackerDefinition." + id, "ShotsPerSecond", 0);
                        _ini.Set("ReloadTrackerDefinition." + id, "ReloadTime", 0);
                        _ini.Set("ReloadTrackerDefinition." + id, "BurstCount", 0);
                        _ini.Set("ReloadTrackerDefinition." + id, "Velocity", 1);
                        _ini.Set("ReloadTrackerDefinition." + id, "Range", 0);
                        needToRedoIni = true;
                    }
                    
                    ReloadTrackerDefinition def = _reloadTrackerDefinitions[id];
                    TurretReloadTracker.Add(turret, new ReloadTracker(def.BurstPeriod, def.ReloadPeriod, def.BurstCount, def.Velocity, def.Range));
                }
            if (needToRedoIni) Me.CustomData = _ini.ToString();

            

            Gyros = allBlocks.OfType<IMyGyro>().ToList();
            var gunsList = allBlocks.OfType<IMyUserControllableGun>().ToList();
            guns.UpdateWeaponsList(gunsList);


            //guns = new Guns(gunsList, this, KnownFireDelays, FramesToGroupGuns, DoVolley, VolleyDelayFrames); // easier to burn it to the ground and rebuild it
        }

        private void UpdateGravityDriveBlocks()
        {
            var massList = allBlocks.OfType<IMyArtificialMassBlock>().ToList();
            var spaceBallList = allBlocks.OfType<IMySpaceBall>().ToList();
            var gravityGeneratorList = allBlocks.OfType<IMyGravityGenerator>().ToList();
            var sphericalGravityGeneratorList = allBlocks.OfType<IMyGravityGeneratorSphere>().ToList();
            gravityDriveManager.UpdateBlocksList(massList, spaceBallList, gravityGeneratorList,
                sphericalGravityGeneratorList, ShipController);
        }

        private void RapidScanTarget()
        {
            if (ScanPauseFrame <= ScanPauseDuration)
            {
                for (var i = TurretControllers.Count - 1; i >= 0; i--)
                {
                    var turret = TurretControllers[i];
                    if (turret == null)
                    {
                        TurretControllers.RemoveAt(i);
                        continue;
                    }

                    turret.AIEnabled = true;
                }

                ScanPauseFrame++;
                return;
            }

            ScanPauseFrame = 1;
            FramesScanningWithoutTarget = 0;
            ScanTarget(Turrets, TurretControllers, ScannedCategory);

            CurrentScanFrame++;
            RetargetTurrets(Turrets);
            for (var i = TurretControllers.Count - 1; i >= 0; i--)
            {
                var turret = TurretControllers[i];
                if (turret == null)
                {
                    TurretControllers.RemoveAt(i);
                    continue;
                }

                turret.AIEnabled = false;
            }

            if (CurrentScanFrame >= MaxScanFrames)
            {
                CurrentScanFrame = 0;
                ScannedCategory++;

                if ((int)ScannedCategory >= 4)
                {
                    currentlyScanning = false;
                    scanEnd = true;
                    
                    
                }
                else
                {
                    var category = ArgusEnums.GetCategoryName(ScannedCategory);
                    if (category == "Default") category = "";
                    for (var i = TurretControllers.Count - 1; i >= 0; i--)
                    {
                        var turret = TurretControllers[i];
                        turret.SetTargetingGroup(category);
                    }

                    for (var i = Turrets.Count - 1; i >= 0; i--)
                    {
                        var turret = Turrets[i];
                        if (turret == null)
                        {
                            Turrets.RemoveAt(i);
                            continue;
                        }

                        turret.SetTargetingGroup(category);
                    }
                }
            }
        }

        private MyDetectedEntityInfo GetTurretTargets(List<IMyLargeTurretBase> turrets,
            List<IMyTurretControlBlock> turretControllers,
            ref Dictionary<IMyFunctionalBlock, MyDetectedEntityInfo> targets)
        {
            for (var i = turretControllers.Count - 1; i >= 0; i--)
            {
                var turret = turretControllers[i];
                if (turret == null)
                {
                    turretControllers.RemoveAt(i);
                    continue;
                }

                var myDetectedEntityInfo = turret.GetTargetedEntity();
                if (myDetectedEntityInfo.EntityId == 0) continue;
                referenceTurret = turret;

                return myDetectedEntityInfo;
            }

            for (var i = turrets.Count - 1; i >= 0; i--)
            {
                var turret = turrets[i];
                if (turret == null)
                {
                    turrets.RemoveAt(i);
                    continue;
                }

                var myDetectedEntityInfo = turret.GetTargetedEntity();
                if (myDetectedEntityInfo.EntityId == 0) continue;
                referenceTurret = turret;
                if ((myDetectedEntityInfo.Position - ShipController.CenterOfMass).ALengthSquared() > 2100 * 2100)
                    RetargetTurret(turret);
                return myDetectedEntityInfo;
            }

            return new MyDetectedEntityInfo();
        }

        private void SetTurretAngles(List<IMyLargeTurretBase> turrets, Vector3D TargetPosition)
        {
            for (var i = 0; i < turrets.Count; i++)
            {
                var turret = turrets[i];
                if (turret == referenceTurret) continue;
                bool shouldShoot = TurretReloadTracker[turret].Update(true) && TurretReloadTracker[turret].Range > TargetDistance;

                turret.Shoot = shouldShoot;
                if (!shouldShoot) continue;
                var state = TurretReloadTracker[turret].GetState();


                var Position = turret.GetPosition();
                
                Echo((PreviousTargetVelocity - TargetVelocity).Length().ToString());
                var previousTargetVelocity = new Vector3D(PreviousTargetVelocity.X, PreviousTargetVelocity.Y,
                    PreviousTargetVelocity.Z);
                var leadPos = Targeting.GetTargetLeadPosition(TargetPosition, TargetVelocity, Position, CurrentVelocity,
                    (float)TurretReloadTracker[turret].Velocity, TimeStep, ref previousTargetVelocity, false, switches.LeadAcceleration);
                var Direction = (leadPos - Position).Normalized();


                var Forward = turret.WorldMatrix.Forward;
                var Up = turret.WorldMatrix.Up;
                var Right = turret.WorldMatrix.Right;
                // Calculate pitch and yaw  based on atan2
                var pitch = Math.Asin(Direction.Dot(Up));
                var yaw = -Math.Atan2(Direction.Dot(Right), Direction.Dot(Forward));
                turret.SetManualAzimuthAndElevation((float)yaw, (float)pitch);
                turret.SyncAzimuth();
                turret.SyncElevation();
            }
        }

        private void ScanTarget(List<IMyLargeTurretBase> turrets, List<IMyTurretControlBlock> turretControllers,
            TargetableBlockCategory currentCategory)
        {
            for (var i = turretControllers.Count - 1; i >= 0; i--)
            {
                var myDetectedEntityInfo = turretControllers[i].GetTargetedEntity();
                EvaluateTurretTarget(myDetectedEntityInfo, turretControllers[i], currentCategory);
            }

            for (var i = turrets.Count - 1; i >= 0; i--)
            {
                var myDetectedEntityInfo = turrets[i].GetTargetedEntity();
                EvaluateTurretTarget(myDetectedEntityInfo, turrets[i], currentCategory);
            }
        }

        private void EvaluateTurretTarget(MyDetectedEntityInfo myDetectedEntityInfo, IMyFunctionalBlock turret,
            TargetableBlockCategory currentCategory)
        {
            if (myDetectedEntityInfo.EntityId == 0) return;
            if (targetableShips.ContainsKey(myDetectedEntityInfo.EntityId))
            {
                var targetableShip = targetableShips[myDetectedEntityInfo.EntityId];
                var toWorldCoordinatesMatrix = targetableShip.ToWorldCoordinatesMatrix;
                AddTargetableBlock(targetableShip, myDetectedEntityInfo, toWorldCoordinatesMatrix, turret,
                    currentCategory);
            }
            else
            {
                var toWorldCoordinatesMatrix = ArgusTargetableShip.GetToWorldCoordinatesMatrix(myDetectedEntityInfo);
                var targetableShip = new ArgusTargetableShip(myDetectedEntityInfo.EntityId, toWorldCoordinatesMatrix,
                    myDetectedEntityInfo.Position, (ulong)frame);
                targetableShips.Add(myDetectedEntityInfo.EntityId, targetableShip);
                AddTargetableBlock(targetableShip, myDetectedEntityInfo, toWorldCoordinatesMatrix, turret,
                    currentCategory);
            }
        }


        private void AddTargetableBlock(ArgusTargetableShip targetShip, MyDetectedEntityInfo targetInfo,
            MatrixD transformMatrix, IMyFunctionalBlock turret, TargetableBlockCategory category)
        {
            var referenceWorldPosition = targetInfo.Position;

            var targetBlockPosition = (Vector3D)targetInfo.HitPosition;

            var worldDirection = targetBlockPosition - referenceWorldPosition;

            var positionInGridDouble = Vector3D.TransformNormal(worldDirection, MatrixD.Transpose(transformMatrix));
            var positionInGridInt = new Vector3I(
                (int)((positionInGridDouble.X + 0.5) / 2.5),
                (int)((positionInGridDouble.Y + 0.5) / 2.5),
                (int)((positionInGridDouble.Z + 0.5) / 2.5)
            );

            // try to parse the string to TargetableBlockCategory
            if (targetShip.UpdateBlockCategoryAtPosition(positionInGridInt, category))
                return; // This removes duplicates
            var targetableBlock = new TargetableBlock(category, positionInGridInt);
            targetShip.Blocks.Add(targetableBlock);
        }

        private void UpdateShipController()
        {
            if (ShipControllerCandidates.Count == 0) return;
            ShipController = ShipControllerCandidates.First();
            for (var i = ShipControllerCandidates.Count - 1; i >= 0; i--)
            {
                if (ShipControllerCandidates[i] == null)
                {
                    ShipControllerCandidates.RemoveAt(i);
                    continue;
                }

                if (ShipControllerCandidates[i].IsUnderControl)
                {
                    if (!playerInControlLastFrame) FlightDataRecorder.PlayerEnteredCockpitEvent();
                    playerInControlLastFrame = true;
                    // get the name of the player controlling
                    ShipController = ShipControllerCandidates[i];
                    break;
                }

                if (playerInControlLastFrame) FlightDataRecorder.PlayerExitedCockpitEvent();
                playerInControlLastFrame = false;
            }

            Targeting.Controller = ShipController;
        }

        private void UpdateShooting()
        {
            var AngularVelocity = ShipController.GetShipVelocities().AngularVelocity;
            // transform the angular velocity to the ship's local coordinates
            AngularVelocity = Vector3D.TransformNormal(AngularVelocity, MatrixD.Transpose(ShipController.WorldMatrix));
            AngularVelocity.Z = 0;
            var rpm = AngularVelocity.ALength() / Math.PI * MaxAngularVelocityRPM;
            if (switches.DoAutoFire)
            {
                if (HasTarget)
                {
                    //get a vector from the ship to the target

                    var forward = ShipController.WorldMatrix.Forward;
                    // Fire when in range and on target, and not scanning
                    if (TargetDistance < ProjectileMaxDistance && OnTargetValue > AimbotWeaponFireSigma &&
                        !currentlyScanning && rpm < MaximumSafeRPMToFire)
                        guns.Fire();
                    else
                        guns.Cancel();
                }
                else
                {
                    guns.Standby();
                }
            }
        }


        private void UpdateAim()
        {
            if (HasTarget && switches.DoAim)
            {
                double roll = ShipController.RollIndicator;
                UpdateDerivative();
                Rotate(ReferenceToLeadNormalized, roll, OnTargetValue);
            }
        }

        private void UpdateDerivative()
        {
            if (ShipController == null) return;
            var shipAngularVelocity = ShipController.GetShipVelocities().AngularVelocity;
            shipAngularVelocity =
                Vector3D.TransformNormal(shipAngularVelocity, MatrixD.Transpose(ShipController.WorldMatrix));
            shipAngularVelocity.Z = 0;
            shipAngularVelocity *= MaxAngularVelocityRPM;
            var enemyAngularVelocity = GetEnemyAngularVelocity() * MaxAngularVelocityRPM;
            enemyAngularVelocity.Z = 0;
            var dot = (1 - shipAngularVelocity.Normalized().Dot(enemyAngularVelocity.Normalized())) *
                      Math.Abs((shipAngularVelocity + enemyAngularVelocity).ALength());
            var newKD = Math.Pow(dot, DerivativeNonLinearity); // optimization can be done here if required
            if (double.IsNaN(newKD) || double.IsInfinity(newKD) || HasTarget == false) newKD = 10;
            // y is left/right , x is up/down, z is forward/backward


            pitch.gain_d = kD + newKD * kD;
            yaw.gain_d = kD + newKD * kD;
        }

        private Vector3D GetEnemyAngularVelocity()
        {
            var crossProduct = Vector3D.Cross(ReferenceToLeadNormalized, PreviousReferenceToLeadNormalized);
            var sinAngle = crossProduct.ALength();
            var angleChangeRadians = sinAngle;
            var angleDifferenceRadians = angleChangeRadians / TimeStep;
            var differenceNormalized = (ReferenceToLeadNormalized - PreviousReferenceToLeadNormalized).Normalized();
            var angularVelocity = differenceNormalized * angleDifferenceRadians;
            angularVelocity = Vector3D.TransformNormal(angularVelocity, MatrixD.Transpose(ShipController.WorldMatrix));
            var temp = angularVelocity.X;
            angularVelocity.X = angularVelocity.Y;
            angularVelocity.Y = -temp;
            angularVelocity = (angularVelocity + previousAngularVelocity) / 2;
            previousAngularVelocity = angularVelocity;
            return angularVelocity;
        }


        private void Rotate(Vector3D desiredGlobalFwdNormalized, double roll, double onTarget)
        {
            int roundValue = 7;
            double rotationalGain = 1.0;
            if (onTarget > 0.9999)
            {
                rotationalGain *= 0.8;
                roundValue = 4;
            }

            if (onTarget > 0.99999)
            {
                rotationalGain *= 0.8;
                roundValue = 3;
            }
            if (onTarget > 0.999999)
            {
                rotationalGain *= 0.8;
                roundValue = 2;
            }
            if (onTarget > 0.9999999)
            {
                rotationalGain *= 0.8;
                roundValue = 1;
            }
            
            double gp;
            double gy;
            var gr = roll;

            //Rotate Toward forward

            var waxis = Vector3D.Cross(ShipController.WorldMatrix.Forward, desiredGlobalFwdNormalized);
            var axis = Vector3D.TransformNormal(waxis, MatrixD.Transpose(ShipController.WorldMatrix));
            var x = pitch.Filter(-axis.X, roundValue);
            var y = yaw.Filter(-axis.Y, roundValue);

            gp = MathHelper.Clamp(x, -MaxAngularVelocityRPM, MaxAngularVelocityRPM);
            gy = MathHelper.Clamp(y, -MaxAngularVelocityRPM, MaxAngularVelocityRPM);

            if (Math.Abs(gy) + Math.Abs(gp) > MaxAngularVelocityRPM)
            {
                var adjust = MaxAngularVelocityRPM / (Math.Abs(gy) + Math.Abs(gp));
                gy *= adjust;
                gp *= adjust;
                // No you're right, this sucks
            }
            gp *= rotationalGain;
            gy *= rotationalGain;
            ApplyGyroOverride(gp, gy, gr, Gyros, ShipController.WorldMatrix);
        }


        private void ApplyGyroOverride(double pitchSpeed, double yawSpeed, double rollSpeed, List<IMyGyro> gyroList,
            MatrixD worldMatrix)
        {
            var rotationVec = new Vector3D(pitchSpeed, yawSpeed, rollSpeed);
            var relativeRotationVec = Vector3D.TransformNormal(rotationVec, worldMatrix);

            foreach (var gyro in gyroList)
                if (gyro.IsFunctional && gyro.IsWorking && gyro.Enabled && !gyro.Closed)
                {
                    var transformedRotationVec =
                        Vector3D.TransformNormal(relativeRotationVec, MatrixD.Transpose(gyro.WorldMatrix));
                    gyro.Pitch = (float)transformedRotationVec.X;
                    gyro.Yaw = (float)transformedRotationVec.Y;
                    gyro.Roll = (float)transformedRotationVec.Z;
                    gyro.GyroOverride = true;
                    return;
                }
        }

        private void ResetGyroOverrides(List<IMyGyro> gyroList)
        {
            foreach (var gyro in gyroList)
                if (gyro.IsFunctional && gyro.IsWorking && gyro.Enabled && !gyro.Closed)
                {
                    gyro.GyroOverride = false;
                    return;
                }
        }


        public void RetargetTurrets(List<IMyLargeTurretBase> turrets)
        {
            foreach (var turret in turrets) RetargetTurret(turret);
        }

        public static void RetargetTurret(IMyLargeTurretBase turret)
        {
            //store turret values
            var enabled = turret.Enabled;
            var targetMeteors = turret.TargetMeteors;
            var targetMissiles = turret.TargetMissiles;
            var targetCharacters = turret.TargetCharacters;
            var targetSmallGrids = turret.TargetSmallGrids;
            var targetLargeGrids = turret.TargetLargeGrids;
            var targetStations = turret.TargetStations;
            var range = turret.Range;
            var enableIdleRotation = turret.EnableIdleRotation;

            turret.ResetTargetingToDefault();

            //restore turret values
            turret.Enabled = enabled;
            turret.TargetMeteors = targetMeteors;
            turret.TargetMissiles = targetMissiles;
            turret.TargetCharacters = targetCharacters;
            turret.TargetSmallGrids = targetSmallGrids;
            turret.TargetLargeGrids = targetLargeGrids;
            turret.TargetStations = targetStations;
            turret.Range = range;
            turret.EnableIdleRotation = enableIdleRotation;
        }
    }
}
