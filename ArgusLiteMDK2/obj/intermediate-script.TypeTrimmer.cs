using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using System;
using VRageMath;
using System.Diagnostics;
using System.Linq;
using EmptyKeys.UserInterface.Controls;
using Sandbox.Game.World;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using System.Reflection.Emit;
using Sandbox.ModAPI.Interfaces;
using DirectShowLib;
using Sandbox.Game.EntityComponents;
using VRage.Game.ObjectBuilders.Definitions;
using Sandbox.Game.Entities;
internal class Program : MyGridProgram
{

    public DebugAPI DebugApi;
    public static Program I;
    public static bool TrollMode = false;
    public static IMyGridTerminalSystem _gridTerminalSystem;
        
    private readonly MyIni _ini = new MyIni();
        
    Dictionary<string, ReloadTrackerDefinition> _reloadTrackerDefinitions =
        new Dictionary<string, ReloadTrackerDefinition>()
        {
            {"MyObjectBuilder_LargeMissileTurret/LargeCalibreTurret", new ReloadTrackerDefinition(1.3333333333, 12, 2, 500, 2000)},
            {"MyObjectBuilder_LargeMissileTurret/LargeBlockMediumCalibreTurret", new ReloadTrackerDefinition(3, 6, 2, 500, 1400)},
        };

    int ActiveGuns;
    ArgusTargetableShip actualTargetedShip;
    ArgusShip _host;
        
    double
        AimbotWeaponFireSigma =
            0.999; // Dot product of this ships forward vector and the target vector is less than this

    private readonly List<IMyTerminalBlock> allBlocks = new List<IMyTerminalBlock>();

    private readonly ArgusHUD argusHUD;


    private readonly double ArtilleryBurstInterval = 0.75 * 60.0;
    private readonly double ArtilleryReloadInterval = 12.0 * 60.0;


    Vector3D AverageGunPos = Vector3D.Zero;
    float averageRuntime;
    private readonly List<IMyBeacon> beacons;
    private readonly StringBuilder beaconSB = new StringBuilder();
    private readonly List<IMyTextSurface> CockpitScreens;
    bool currentlyScanning;
    int CurrentScanFrame;

    Vector3D CurrentVelocity = Vector3D.Zero;

    double
        DerivativeNonLinearity =
            1.6; // Some random crap I made up to stop the ship overshooting due to gyro ramp up

    private readonly IMyBroadcastListener diagnosticRequestListener;


    private readonly int frame = 0;

    int FramesScanningWithoutTarget;

    float
        FramesToGroupGuns = 5; // Maximum frame difference between available guns to aim and fire them together

    private readonly GravityDriveManager gravityDriveManager;
    IMyBlockGroup group;

    /***********Common Configuration***********/


    string GroupName = "Flight Control"; // Group name, please ensure this is unique to your ship
    private readonly Guns guns; // guns guns guns guns
    List<IMyGyro> Gyros;

    bool HasResetTurrets;
    bool HasTarget;
    double integralClamp = 0.05; // Maximum windup for the integral if you are using it
        

    private readonly Dictionary<MyDefinitionId, float> KnownFireDelays = new Dictionary<MyDefinitionId, float>
    {
        [MyDefinitionId.Parse("SmallMissileLauncherReload/SmallRailgun")] = 0.5f,
        [MyDefinitionId.Parse("SmallMissileLauncherReload/LargeRailgun")] = 2.0f
    };


    /**********Aimbot & Configuration**********/


    //bool DoAim = true;                              // Should we use the aimbot?


    double kP = 500; // Proportional gain
    double kD = 30; // Derivative gain
    double kI; // Integral gain

    private readonly KratosMissileManager kratosMissileManager;


    double largestRuntime;
    Vector3D LeadPos = Vector3D.Zero;
    float MaxAngularVelocityRPM = 30; // Should be 30 for large grid, 60 for small grid


    /***********Weapon Configuration***********/


    //bool DoAutoFire = true;                         // Automatically fire and cancel fire?

    // Only fire if:
    double
        MaximumSafeRPMToFire = 2.0; // Not below this RPM (requires that MaxAngularVelocityRPM is set correctly)
    // TBA: Periods for balancing of mass blocks and space balls


    /************Scan Configuration************/


    //bool DoAutoScan = true;                         // Automatically scan new detected ships?
    int MaxScanFrames = 5; // How many times we should scan each category

    private readonly List<IMyOffensiveCombatBlock> myOffensiveCombatBlocks;

    //bool LeadAcceleration = true;                   // Should we use acceleration in the lead calculation or only velocity? (both fixed weapons and turrets)
    double
        NewBlockCheckPeriod =
            31.0 * 60.0; // How often we should check the group for new blocks, try to avoid round numbers otherwise everything could happen at once and burn out the programmable block


    int OffendingLockCount; // Used to keep track of how many people have locked onto us

    double OnTargetValue;

        
    /******Missile Configuration******/
    bool FighterMode = false;

    /*******Gravity Drive Configuration********/


    //bool DoGravityDrive = true;                     // Should we manage gravity at all?
    //bool DoBalance = true;                          // Automatically balance the gravity drive?
    //bool DoRepulse = false;                         // Default value, changed by argument
    //bool PrecisionMode = false;                     // Should we reduce authority to 1/10th for out of combat maneuvering?
    float PassiveSphericalRange = 80; // Passive radius for gravity drive


    private readonly PIDController pitch;

    bool playerInControlLastFrame;
    Vector3D previousAngularVelocity;

    Vector3D PreviousCenterOfMass = Vector3D.Zero;

    Vector3D PreviousLeadPos = Vector3D.Zero;
    Vector3D PreviousReferenceToLead = Vector3D.Zero;
    Vector3D PreviousReferenceToLeadNormalized = Vector3D.Zero;
    Vector3D PreviousTargetVelocity = Vector3D.Zero;


    TargetableBlockCategory PrimaryTargetedBlockCategory = TargetableBlockCategory.Default;
    TargetableType PrimaryTargetedType = TargetableType.ClusteredCenter;
    float ProjectileMaxDistance = 2000; // Within this distance

    float ProjectileVelocity = 2000f; // Default projectile velocity, can be changed by argument
    Vector3D ReferenceToLead = Vector3D.Zero;
    Vector3D ReferenceToLeadNormalized = Vector3D.Zero;

    IMyFunctionalBlock referenceTurret;

    long rememberedEntityId;
    List<IMyRemoteControl> RemoteControls;
    float RepulseSphericalRange = 400; // Repulse radius for gravity drive


    private readonly Random rng;
    PIDController roll;
    private readonly List<float> runTimes = new List<float>();
    bool scanEnd;
    TargetableBlockCategory ScannedCategory = TargetableBlockCategory.Default;
    int ScanPauseDuration = 10; // Pause time between each scan
    int ScanPauseFrame;

    bool scanStart;
    int scriptFrame;
    IMyShipController ShipController;
    private readonly List<IMyShipController> ShipControllerCandidates;


    private readonly ArgusSwitches switches = new ArgusSwitches();


    private readonly Dictionary<long, ArgusTargetableShip> targetableShips = new Dictionary<long, ArgusTargetableShip>();
    double TargetDistance;


    private readonly StringBuilder targetedShipNameSB = new StringBuilder();


    private readonly string[] targetedShipNameSplitter = { "Attacking ", "Status: " };
    int targetingIndex;

    Vector3D TargetPosition;

    Dictionary<IMyFunctionalBlock, MyDetectedEntityInfo> Targets =
        new Dictionary<IMyFunctionalBlock, MyDetectedEntityInfo>();

    Vector3D TargetVelocity = Vector3D.Zero;

    private readonly int timeSpanCount = 180;
    public static readonly double TimeStep = 1.0 / 60.0;

    private readonly List<IMyTurretControlBlock> TurretControllers;


    /***********Turret Configuration***********/


    //bool DoTurretAim = true;                        // Should we override the turret aim?
    float TurretProjectileVelocity = 500f; // Set as whatever the projectile velocity of the turrets are
    private readonly Dictionary<IMyLargeTurretBase, ReloadTracker> TurretReloadTracker;
    List<IMyLargeTurretBase> Turrets;

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
    int VolleyDelayFrames = 20; // Spacing between each weapon firing for Volley mode
    private readonly PIDController yaw;
    int[] _factionTag = {67, 48, 76};
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

    void SyncConfig()
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

    void RunCommand(string argument)
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

    void SetProjectileVelocity(string arg)
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


    void Safety()
    {
        kratosMissileManager.DetonateAll();
        ResetGyroOverrides(Gyros);
        gravityDriveManager.EnableMassBlocks(false);
    }


    void UpdateRuntimeInfo()
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

    void UpdateHUD()
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

    bool lastInterdictMode = false;

    void RunUpdate()
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

    void IGCHandler()
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

    void UpdateScan(long entityId)
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

    int RandomTargetIndex(TargetableBlockCategory targetCategory, ArgusTargetableShip currentTargetedShip)
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

    void GetNewBlocks()
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

    void UpdateGravityDriveBlocks()
    {
        var massList = allBlocks.OfType<IMyArtificialMassBlock>().ToList();
        var spaceBallList = allBlocks.OfType<IMySpaceBall>().ToList();
        var gravityGeneratorList = allBlocks.OfType<IMyGravityGenerator>().ToList();
        var sphericalGravityGeneratorList = allBlocks.OfType<IMyGravityGeneratorSphere>().ToList();
        gravityDriveManager.UpdateBlocksList(massList, spaceBallList, gravityGeneratorList,
            sphericalGravityGeneratorList, ShipController);
    }

    void RapidScanTarget()
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

    MyDetectedEntityInfo GetTurretTargets(List<IMyLargeTurretBase> turrets,
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

    void SetTurretAngles(List<IMyLargeTurretBase> turrets, Vector3D TargetPosition)
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

    void ScanTarget(List<IMyLargeTurretBase> turrets, List<IMyTurretControlBlock> turretControllers,
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

    void EvaluateTurretTarget(MyDetectedEntityInfo myDetectedEntityInfo, IMyFunctionalBlock turret,
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


    void AddTargetableBlock(ArgusTargetableShip targetShip, MyDetectedEntityInfo targetInfo,
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

    void UpdateShipController()
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

    void UpdateShooting()
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


    void UpdateAim()
    {
        if (HasTarget && switches.DoAim)
        {
            double roll = ShipController.RollIndicator;
            UpdateDerivative();
            Rotate(ReferenceToLeadNormalized, roll, OnTargetValue);
        }
    }

    void UpdateDerivative()
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

    Vector3D GetEnemyAngularVelocity()
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


    void Rotate(Vector3D desiredGlobalFwdNormalized, double roll, double onTarget)
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


    void ApplyGyroOverride(double pitchSpeed, double yawSpeed, double rollSpeed, List<IMyGyro> gyroList,
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

    void ResetGyroOverrides(List<IMyGyro> gyroList)
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
public static class ALDebug
{
    public static List<IMyTextPanel> panels;
    public static MyGridProgram program;

    public static StringBuilder sb = new StringBuilder();

    public static void InitializePanels(List<IMyTextPanel> panels)
    {
        ALDebug.panels = panels;
        foreach (var panel in panels) panel.ContentType = ContentType.TEXT_AND_IMAGE;
    }

    public static void AddText(string text)
    {
        sb.AppendLine(text);
    }

    public static void WriteText()
    {
        var text = sb.ToString();
        program.Echo(text);
    }

    public static void Echo(object obj)
    {
        string thing = obj?.ToString();
        program.Echo(thing);
    }
}
public static class ArgusEnums
{
    public static string GetCategoryName(TargetableBlockCategory category)
    {
        switch (category)
        {
            case TargetableBlockCategory.Default:
                return "Default";
            case TargetableBlockCategory.Weapons:
                return "Weapons";
            case TargetableBlockCategory.Propulsion:
                return "Propulsion";
            case TargetableBlockCategory.PowerSystems:
                return "PowerSystems";
            default:
                return "Default";
        }
    }

    public static TargetableBlockCategory GetCategoryFromName(string name)
    {
        switch (name)
        {
            case "Default":
                return TargetableBlockCategory.Default;
            case "Weapons":
                return TargetableBlockCategory.Weapons;
            case "Propulsion":
                return TargetableBlockCategory.Propulsion;
            case "PowerSystems":
                return TargetableBlockCategory.PowerSystems;
            default:
                return TargetableBlockCategory.Default;
        }
    }
}
public static class ArgusExtensions
{
    // Create a metamethod for assigning an int to a Vector3D
    public static Vector3D NormalizeTest(this Vector3D storage, Vector3D value)
    {
        var num = 1.0 / Math.Sqrt(value.X * value.X + value.Y * value.Y + value.Z * value.Z);

        storage.X = value.X * num;
        storage.Y = value.Y * num;
        storage.Z = value.Z * num;
        return storage;
    }


    public static double ALength(this Vector3D storage)
    {
        return storage.Length();
    }

    public static double ALengthSquared(this Vector3D storage)
    {
        return storage.LengthSquared();
    }


    public static StringBuilder TrimTrailingWhitespace(this StringBuilder sb)
    {
        //int num = sb.Length;
        //while (num > 0 && (sb[num - 1] == ' ' || sb[num - 1] == '\r' || sb[num - 1] == '\n'))
        //{
        //    num--;
        //}

        //sb.Length = num;
        //return sb;
        var n = sb.Length - 1;
        while (n >= 0 && char.IsWhiteSpace(sb[n]))
            n--;
        sb.Length = n + 1;
        return sb;
    }

    public static StringBuilder RemoveNonNumberChars(this StringBuilder sb)
    {
        // Compare to NumberCharWhitelist

        for (var i = 0; i < sb.Length; i++)
            if (!char.IsDigit(sb[i]))
            {
                sb.Remove(i, 1);
                i--;
            }

        return sb;
    }
}
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
    Vector2 scanResultLabelSize = new Vector2(150, 25);
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

    int keenWorkaround;
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

    void Draw(HUDSurfaceData data, ArgusHUDInfo info, List<KratosMissileManager.MissileData> missileData,
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

    void DrawEnemyFloorPlan(ref MySpriteDrawFrame frame, ArgusTargetableShip targetedShip, HUDSurfaceData data)
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


    void DrawVersionText(ref MySpriteDrawFrame frame, HUDSurfaceData data)
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

    void DrawMissilePositions(ref MySpriteDrawFrame frame, HUDSurfaceData data,
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

    void DrawScanInfo(ref MySpriteDrawFrame frame, HUDSurfaceData data, ArgusHUDInfo info)
    {
        // Scanning text
        if (info.IsScanning)
            ScanningLines(ref frame, data, info);
        else
            ScanResults(ref frame, data, info);
    }


    void ScanningLines(ref MySpriteDrawFrame frame, HUDSurfaceData data, ArgusHUDInfo info)
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


    void ScanResults(ref MySpriteDrawFrame frame, HUDSurfaceData data, ArgusHUDInfo info)
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


    void DrawBools(ref MySpriteDrawFrame frame, HUDSurfaceData data, ArgusSwitches switches)
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


    void PointerLine(ref MySpriteDrawFrame frame, Vector2 positionA, Vector2 positionB)
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

    void BoolIndicator(ref MySpriteDrawFrame frame, Vector2 position, bool state, string text)
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

    void ProgressBar(ref MySpriteDrawFrame frame, Vector2 position, bool state, float progress)
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

    void ProgressSquare(ref MySpriteDrawFrame frame, Vector2 position, Vector2 size, float progress, Color fullColour, Color emptyColour)
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
    void ProgressBar(ref MySpriteDrawFrame frame, Vector2 position, Vector2 minSize, Vector2 maxSize,
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
    bool WorldPositionToScreenPosition(Vector3D worldPosition, cameraData cameraData, IMyCameraBlock cam,
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


    

    struct cameraData
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
public class ArgusShip
{
    public Vector3D Position;
    public Vector3D Velocity;
    public Vector3D Acceleration;
    Vector3D _previousVelocity;
        
    public bool DataIsCurrent = false;
    public MatrixD ToWorldCoordinatesMatrix;
    public long EntityId;
    // TODO: Update this from within program.cs
    public IMyShipController Controller;

    public ArgusShip(long entityId, MatrixD toWorldCoordinatesMatrix, Vector3D position, IMyShipController controller)
    {
        EntityId = entityId;
        ToWorldCoordinatesMatrix = toWorldCoordinatesMatrix;
        Position = position;
        Controller = controller;
            
        _masterShipList.Add(this);
    }
        
        
    // TODO: Call me
    public virtual void Update(Vector3D position, Vector3D velocity, MatrixD toWorldCoordinatesMatrix, IMyShipController controller)
    {
        Position = position;
        Velocity = velocity;
        Acceleration = (velocity - _previousVelocity) * Program.TimeStep;
        Controller = controller;
            
        _previousVelocity = velocity;
        ToWorldCoordinatesMatrix = toWorldCoordinatesMatrix;
        DataIsCurrent = true;
    }




    private static List<ArgusShip> _masterShipList = new List<ArgusShip>();

    public static void SetAllStatusNonCurrent()
    {
        foreach (var ship in _masterShipList)
        {
            ship.DataIsCurrent = false;
        }
    }
}
internal class ArgusTargetableShip : ArgusShip
{
    public static readonly ArgusTargetableShip Default = new ArgusTargetableShip(0, MatrixD.Identity, Vector3D.Zero, 0);
        
    bool _calculationRequired = true;
        
    public ulong ScanFrame;
        
        
    public readonly List<TargetableBlock> Blocks;
    public int AllBlocksCount;
    Vector3D _allBlocksTargetPosition;
        
    public readonly List<TargetableBlock> WeaponBlocks;
    public int WeaponBlocksCount;
    Vector3D _weaponTargetPosition;
        
    public readonly List<TargetableBlock> PropulsionBlocks;
    public int PropulsionBlocksCount;
    Vector3D _propulsionTargetPosition;
        
    public readonly List<TargetableBlock> PowerSystemsBlocks;
    public int PowerSystemsBlocksCount;
    Vector3D _powerSystemsTargetPosition;
        
    // TODO: add ability to aim at uncategorized
    public readonly List<TargetableBlock> UncategorizedBlocks;
    public int UncategorizedBlocksCount;
    Vector3D _uncategorizedTargetPosition;
        
    public ArgusTargetableShip(long entityId, MatrixD toWorldCoordinatesMatrix, Vector3D position, ulong scanFrame) : base(entityId, toWorldCoordinatesMatrix, position, null)
    {
        Blocks = new List<TargetableBlock>();
        WeaponBlocks = new List<TargetableBlock>();
        PropulsionBlocks = new List<TargetableBlock>();
        PowerSystemsBlocks = new List<TargetableBlock>();
        UncategorizedBlocks = new List<TargetableBlock>();

        _allBlocksTargetPosition = Vector3D.Zero;
        _weaponTargetPosition = Vector3D.Zero;
        _propulsionTargetPosition = Vector3D.Zero;
        _powerSystemsTargetPosition = Vector3D.Zero;
        this.ScanFrame = scanFrame;
    }


    public void CalculateTargetPositions()
    {
        if (!_calculationRequired) return;
        _calculationRequired = false;
        AllBlocksCount = 0;
        WeaponBlocksCount = 0;
        PropulsionBlocksCount = 0;
        PowerSystemsBlocksCount = 0;
        UncategorizedBlocksCount = 0;

        var allBlocks = new List<Vector3D>();
        var weaponBlocks = new List<Vector3D>();
        var propulsionBlocks = new List<Vector3D>();
        var powerSystemsBlocks = new List<Vector3D>();
        var uncategorizedBlocks = new List<Vector3D>();


        foreach (var block in Blocks)
        {
            var pos = block.positionInGrid * 2.5;
            var cat = block.category;
            switch (cat)
            {
                case TargetableBlockCategory.Default:
                    uncategorizedBlocks.Add(pos);
                    UncategorizedBlocksCount++;
                    allBlocks.Add(pos);
                    AllBlocksCount++;
                    UncategorizedBlocks.Add(block);
                    break;
                case TargetableBlockCategory.Weapons:
                    weaponBlocks.Add(pos);
                    WeaponBlocksCount++;
                    allBlocks.Add(pos);
                    AllBlocksCount++;
                    WeaponBlocks.Add(block);
                    break;
                case TargetableBlockCategory.Propulsion:
                    propulsionBlocks.Add(pos);
                    PropulsionBlocksCount++;
                    allBlocks.Add(pos);
                    AllBlocksCount++;
                    PropulsionBlocks.Add(block);
                    break;
                case TargetableBlockCategory.PowerSystems:
                    powerSystemsBlocks.Add(pos);
                    PowerSystemsBlocksCount++;
                    allBlocks.Add(pos);
                    AllBlocksCount++;
                    PowerSystemsBlocks.Add(block);
                    break;
            }
        }


        _allBlocksTargetPosition = CalculateClusteredPosition(allBlocks);
        _weaponTargetPosition = CalculateClusteredPosition(weaponBlocks);
        _propulsionTargetPosition = CalculateClusteredPosition(propulsionBlocks);
        _powerSystemsTargetPosition = CalculateClusteredPosition(powerSystemsBlocks);
        _uncategorizedTargetPosition = CalculateClusteredPosition(uncategorizedBlocks);
    }
    Vector3D CalculateClusteredPosition(List<Vector3D> positions)
    {
        double maxWeight = 0;
        var weights = new Dictionary<Vector3D, double>();
        for (var i = 0; i < positions.Count; i++)
        {
            var pos = positions[i];
            double weight = 0;
            for (var i1 = 0; i1 < positions.Count; i1++)
            {
                if (i == i1) continue;
                var posB = positions[i1];
                weight += (posB - pos).ALengthSquared();
            }

            if (weight > maxWeight) maxWeight = weight;
            weights.Add(pos, weight);
        }

        double totalWeight = 0;
        var averagePos = Vector3D.Zero;
        foreach (var kvp in weights)
        {
            var weight = maxWeight - kvp.Value;
            totalWeight += weight;
            averagePos += kvp.Key * weight;
        }

        if (totalWeight == 0) return averagePos;
        averagePos /= totalWeight;

        return averagePos;
    }

    // to be done later
    //public void CalculateTargetPositions()
    //{

    //    List<Vector3D> weaponBlocks = new List<Vector3D>();
    //    allBlocksCount = 0;
    //    weaponBlocksCount = 0;
    //    propulsionBlocksCount = 0;
    //    powerSystemsBlocksCount = 0;
    //    uncategorizedBlocksCount = 0;
    //    foreach (var block in Blocks)
    //    {
    //        Vector3D pos = block.positionInGrid * 2.5;
    //        TargetableBlockCategory cat = block.category;
    //        switch (cat)
    //        {
    //            case TargetableBlockCategory.Default:
    //                UncategorizedTargetPosition += pos;
    //                uncategorizedBlocksCount++;
    //                AllBlocksTargetPosition += pos;
    //                allBlocksCount++;
    //                break;
    //            case TargetableBlockCategory.Weapons:
    //                //WeaponTargetPosition += pos;
    //                weaponBlocksCount++;
    //                weaponBlocks.Add(pos);
    //                AllBlocksTargetPosition += pos;
    //                allBlocksCount++;
    //                break;
    //            case TargetableBlockCategory.Propulsion:
    //                PropulsionTargetPosition += pos;
    //                propulsionBlocksCount++;
    //                AllBlocksTargetPosition += pos;
    //                allBlocksCount++;
    //                break;
    //            case TargetableBlockCategory.PowerSystems:
    //                PowerSystemsTargetPosition += pos;
    //                powerSystemsBlocksCount++;
    //                AllBlocksTargetPosition += pos;
    //                allBlocksCount++;
    //                break;
    //        }
    //    }

    //    Dictionary<int, KeyValuePair<List<Vector3D>[], double>> kMeansResults = new Dictionary<int, KeyValuePair<List<Vector3D>[], double>>();
    //    for (int k = 1; k < 15; k++)
    //    {
    //        var kMeans = new KMeans(weaponBlocks, k, 5);
    //        var clusters = kMeans.Cluster();
    //        //Program.Echo(clusters.Count().ToString());
    //        //var e = 0 / k; // intentional division by zero
    //        double wcss = kMeans.ComputeWCSS(clusters);
    //        kMeansResults.Add(k, new KeyValuePair<List<Vector3D>[], double>(clusters, wcss));
    //    }

    //    int optimalK = FindElbowPoint(kMeansResults);
    //    Program.Echo("Optimal key: " + optimalK);
    //    List<Vector3D> optimalCluster = kMeansResults[optimalK].Key[0];
    //    //WeaponTargetPosition = optimalCluster.Aggregate((acc, cur) => acc + cur) / optimalCluster.Count;
    //    Program.Echo("All block count: " + allBlocksCount);
    //    Program.Echo("Weapon block count: " + weaponBlocksCount);
    //    Program.Echo("Propulsion block count: " + propulsionBlocksCount);
    //    Program.Echo("Power systems block count: " + powerSystemsBlocksCount);
    //    Program.Echo("Uncategorized block count: " + uncategorizedBlocksCount);
    //    if (allBlocksCount > 0) AllBlocksTargetPosition /= allBlocksCount;
    //    //if (weaponBlocksCount > 0) WeaponTargetPosition /= weaponBlocksCount;
    //    if (propulsionBlocksCount > 0) PropulsionTargetPosition /= propulsionBlocksCount;
    //    if (powerSystemsBlocksCount > 0) PowerSystemsTargetPosition /= powerSystemsBlocksCount;
    //    if (uncategorizedBlocksCount > 0) UncategorizedTargetPosition /= uncategorizedBlocksCount;

    //    // Todo: k-means clustering
    //}
    private static int FindElbowPoint(Dictionary<int, KeyValuePair<List<Vector3D>[], double>> kMeansResults)
    {
        var wcssValues = new double[kMeansResults.Count];
        var i = 0;
        foreach (var kvp in kMeansResults)
        {
            wcssValues[i] = kvp.Value.Value;
            i++;
        }

        // Find the elbow point
        var optimalK = 0;
        double maxChange = 0;
        for (i = 1; i < wcssValues.Length; i++)
        {
            var change = Math.Abs(wcssValues[i] - wcssValues[i - 1]);
            if (change > maxChange)
            {
                maxChange = change;
                optimalK = i + 1; // K starts from 1, so increment by 1
            }
        }

        return optimalK;
    }

    public Vector3D GetTargetPosition(TargetableBlockCategory cat)
    {
        switch (cat)
        {
            case TargetableBlockCategory.Default:
                return Vector3D.Transform(_allBlocksTargetPosition, ToWorldCoordinatesMatrix);
            case TargetableBlockCategory.Weapons:
                return Vector3D.Transform(_weaponTargetPosition, ToWorldCoordinatesMatrix);
            case TargetableBlockCategory.Propulsion:
                return Vector3D.Transform(_propulsionTargetPosition, ToWorldCoordinatesMatrix);
            case TargetableBlockCategory.PowerSystems:
                return Vector3D.Transform(_powerSystemsTargetPosition, ToWorldCoordinatesMatrix);
            default:
                return Vector3D.Zero;
        }
    }

    public bool UpdateBlockCategoryAtPosition(Vector3I positionInGrid, TargetableBlockCategory cat)
    {
        for (var i = 0; i < Blocks.Count; i++)
        {
            var block = Blocks[i];
            if (block.positionInGrid == positionInGrid)
            {
                if (block.category == TargetableBlockCategory.Default) block.category = cat;

                return true;
            }
        }

        return false;
    }


    public static MatrixD GetToWorldCoordinatesMatrix(MyDetectedEntityInfo targetInfo)
    {
        var toWorldMatrix = targetInfo.Orientation;
        var targetShipPosition = targetInfo.Position;
        toWorldMatrix.Translation = targetShipPosition;


        return toWorldMatrix;
    }

    public Vector3D GetBlockPositionAtIndex(int index)
    {
        if (index == -1) return Position;
        return Vector3D.Transform(2.5 * Blocks[index].positionInGrid, ToWorldCoordinatesMatrix);
    }

    public Vector3D GetBlockPositionOfCategoryAtIndex(TargetableBlockCategory cat, int index)
    {
        if (index == -1) return Position;
        switch (cat)
        {
            case TargetableBlockCategory.Default:
                if (index >= Blocks.Count) return Vector3D.Zero;
                return Vector3D.Transform(2.5 * Blocks[index].positionInGrid, ToWorldCoordinatesMatrix);
            case TargetableBlockCategory.Weapons:
                if (index >= WeaponBlocks.Count) return Vector3D.Zero;
                return Vector3D.Transform(2.5 * WeaponBlocks[index].positionInGrid, ToWorldCoordinatesMatrix);
            case TargetableBlockCategory.Propulsion:
                if (index >= PropulsionBlocks.Count) return Vector3D.Zero;
                return Vector3D.Transform(2.5 * PropulsionBlocks[index].positionInGrid, ToWorldCoordinatesMatrix);
            case TargetableBlockCategory.PowerSystems:
                if (index >= PowerSystemsBlocks.Count) return Vector3D.Zero;
                return Vector3D.Transform(2.5 * PowerSystemsBlocks[index].positionInGrid, ToWorldCoordinatesMatrix);
            default:
                return Vector3D.Zero;
        }
    }

    public int GetBlocksCountOfCategory(TargetableBlockCategory cat)
    {
        switch (cat)
        {
            case TargetableBlockCategory.Default:
                return AllBlocksCount;
            case TargetableBlockCategory.Weapons:
                return WeaponBlocksCount;
            case TargetableBlockCategory.Propulsion:
                return PropulsionBlocksCount;
            case TargetableBlockCategory.PowerSystems:
                return PowerSystemsBlocksCount;
            default:
                return 0;
        }
    }
}
public class ArgusSwitches
{

        
    public bool DoAim { get; set; } = true;
    public bool DoAutoFire { get; set; } = true;
    public bool DoGravityDrive { get; set; } = true;
    public bool DoTurretAim { get; set; } = true;
    public bool DoRepulse { get; set; }
    public bool PrecisionMode { get; set; }
    public bool LeadAcceleration { get; set; } = true;
    public bool DoBalance { get; set; } = true;
    public bool DoAutoScan { get; set; } = true;
    public bool DoVolley { get; set; }
        
    public bool WaitForAll { get; set; } = true;
    public bool HighlightTarget { get; set; } = true;
    public bool HighlightMissiles { get; set; } = true;
    public bool MissileInterdictionMode { get; set; } = true;


    public bool[] GetBools()
    {
        var bools = new bool[14];
        bools[0] = DoAim;
        bools[1] = DoAutoFire;
        bools[2] = DoGravityDrive;
        bools[3] = DoTurretAim;
        bools[4] = DoRepulse;
        bools[5] = PrecisionMode;
        bools[6] = LeadAcceleration;
        bools[7] = DoBalance;
        bools[8] = DoAutoScan;
        bools[9] = DoVolley;
        bools[10] = WaitForAll;
        bools[11] = HighlightTarget;
        bools[12] = HighlightMissiles;
        bools[13] = MissileInterdictionMode;
        return bools;
    }

    public void SetBools(bool[] bools)
    {
        DoAim = bools[0];
        DoAutoFire = bools[1];
        DoGravityDrive = bools[2];
        DoTurretAim = bools[3];
        DoRepulse = bools[4];
        PrecisionMode = bools[5];
        LeadAcceleration = bools[6];
        DoBalance = bools[7];
        DoAutoScan = bools[8];
        DoVolley = bools[9];
        WaitForAll = bools[10];
        HighlightTarget = bools[11];
        HighlightMissiles = bools[12];
        MissileInterdictionMode = bools[13];
    }


    public Dictionary<string, bool> GetBoolNames()
    {
        var boolNames = new Dictionary<string, bool>();
        boolNames.Add("AimAsst", DoAim);
        boolNames.Add("AutFire", DoAutoFire);
        boolNames.Add("GDrive", DoGravityDrive);
        boolNames.Add("TurretAI", DoTurretAim);
        boolNames.Add("Repulse", DoRepulse);
        boolNames.Add("Precise", PrecisionMode);
        boolNames.Add("LdAccel", LeadAcceleration);
        boolNames.Add("Balance", DoBalance);
        boolNames.Add("AutoScn", DoAutoScan);
        boolNames.Add("Volley", DoVolley);
        boolNames.Add("Wt4All", WaitForAll);
        boolNames.Add("H.Targt", HighlightTarget);
        boolNames.Add("H.Mssle", HighlightMissiles);
        boolNames.Add("M.Inter", MissileInterdictionMode);
        return boolNames;
    }

    public void SetBoolNames(Dictionary<string, bool> boolNames)
    {
        DoAim = boolNames["AimAsst"];
        DoAutoFire = boolNames["AutFire"];
        DoGravityDrive = boolNames["GDrive"];
        DoTurretAim = boolNames["TurretAI"];
        DoRepulse = boolNames["Repulse"];
        PrecisionMode = boolNames["Precise"];
        LeadAcceleration = boolNames["LdAccel"];
        DoBalance = boolNames["Balance"];
        DoAutoScan = boolNames["AutoScn"];
        DoVolley = boolNames["Volley"];
        WaitForAll = boolNames["Wt4All"];
        HighlightTarget = boolNames["H.Targt"];
        HighlightMissiles = boolNames["H.Mssle"];
        MissileInterdictionMode = boolNames["M.Inter"];
    }
}
public static class AutoDodge
{
        
    private static float _threshold = 25;
    private static double _maxSpeed = 102.65;

    private static Vector3D _ourPreviousVelocity;
        
    private static MatrixD _theirPreviousOrientation;
    private static Vector3D _theirPreviousVelocity;
    private static Vector3D _theirPreviousAcceleration;
        
    private static Vector3D _lastJerkEventPosition;
    private static Vector3D _lastInterceptEventPosition;
    private static float _jerkEventPositionValidFor = 0f;
    private static float _interceptEventPositionValidFor = 0f;

    private static double _jerkLastFrame = 0;
        
    public static Vector3D Check(MyDetectedEntityInfo theirGrid, Vector3D theirPos, Vector3D ourPos, Vector3D ourVelocity, IMyCubeGrid ourGrid, Vector3D ourAngularVelocity)
    {
        if (theirGrid.IsEmpty()) return Vector3D.Zero;
        var theirOrientation = theirGrid.Orientation;
        var ourOrientation = ourGrid.WorldMatrix;
        var theirVelocity = theirGrid.Velocity;

        var theirAcceleration = theirVelocity - _theirPreviousVelocity;
            
            
            
        Vector3D ourAcceleration = ourVelocity - _ourPreviousVelocity;
        Vector3D ourRelativeAcceleration = Vector3D.Transform(ourAcceleration, MatrixD.Transpose(ourOrientation.GetOrientation()));
        ourAngularVelocity /= 60;
        var ourAngularVelocityQuaternion = Quaternion.CreateFromYawPitchRoll((float)ourAngularVelocity.Y, (float)ourAngularVelocity.X, (float)ourAngularVelocity.Z);

        var themToUs = (ourPos - theirPos).Normalized();
            
            
        // Step 1: We want to find the direction of the ship (forward, backward, left, right, up, down) that corresponds closest with the vector from them to us

        var theirForward = GetClosestDirectionVector(theirOrientation, themToUs);

        var dotOfThemToUs = theirForward.Dot((themToUs));
            




        Vector3D theirClosestLeadPoint = Vector3D.Zero; // The point where their shooting position has the least error to us
        Vector3D ourClostestLeadPoint = Vector3D.Zero; // The point where our position is predicted to be
        double closestDistanceSqr = Double.MaxValue;
        float closestT = 0;
            
        float velocity = 2000;
        Vector3D projectileRayStart = theirPos;
        Vector3D ourAdjustedVelocity = ourVelocity;
        MatrixD adjustedOrientation = ourOrientation;

        Vector3D projectileHeading = (theirVelocity + theirForward * velocity).Normalized() * velocity;
            
        for (float t = 1f/60f; t < 1; t += 1f / 60f)
        {
            Vector3D projectileRayEnd = theirPos + projectileHeading * t;
            Vector3D ourPredictedPos = ourPos + ourAdjustedVelocity * t;
            ourAdjustedVelocity += Vector3D.Transform(ourRelativeAcceleration, adjustedOrientation.GetOrientation());
            if (ourAdjustedVelocity.LengthSquared() > _maxSpeed * _maxSpeed)
            {
                // Scale the velocity down to max speed without changing direction
                Vector3D dir = ourAdjustedVelocity.Normalized();
                ourAdjustedVelocity = dir * _maxSpeed;
            }
                
            adjustedOrientation = MatrixD.Transform(adjustedOrientation, ourAngularVelocityQuaternion);

            Vector3D closestPointOnLine =
                ClosestPointOnSegment(projectileRayStart, projectileRayEnd, ourPredictedPos);

            // Color color = Color.Lerp(Color.Red, Color.Blue, t);
            // Program.I.DebugApi.DrawLine(projectileRayStart, projectileRayEnd, color, 1f, 0.016f);
            // Program.I.DebugApi.DrawPoint(ourPredictedPos, color, 5f, 0.016f, true);
            // Program.I.DebugApi.DrawLine(ourPredictedPos, closestPointOnLine, color, 0.25f, 0.016f);

            double distSqr = (closestPointOnLine - ourPredictedPos).LengthSquared();

            if (distSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distSqr;
                theirClosestLeadPoint = closestPointOnLine;
                ourClostestLeadPoint = ourPredictedPos;
                closestT = t;
            }
                
            projectileRayStart = projectileRayEnd;
        }

        // So now we have:
            
        // Their closest intercept point to us
        // Where we will be at that point
        // Our goal is to separate that by as much as possible
        // Maybe we just move our current position directly away from the intercept position?

            
        double jerk = CheckForTheirJerk(themToUs, theirAcceleration);
            

        Vector3D realToGhost = theirClosestLeadPoint - ourClostestLeadPoint;
        //if (realToGhost.LengthSquared() < _threshold * _threshold && didFire)
        if (jerk != 0 && _jerkLastFrame != 0 && Math.Sign(_jerkLastFrame) != Math.Sign(jerk))
        {
            _lastJerkEventPosition = theirClosestLeadPoint;
            _jerkEventPositionValidFor = closestT;
        }

        if (realToGhost.LengthSquared() < _threshold * _threshold)
        {
            _lastInterceptEventPosition = theirClosestLeadPoint;
            _interceptEventPositionValidFor = closestT;
        }
            
        // Program.I.DebugApi.DrawLine(theirClosestLeadPoint, ourClostestLeadPoint, Color.Green, 0.25f, 0.016f);
        // 
        //
            
        _theirPreviousOrientation = theirOrientation;
        _ourPreviousVelocity = ourVelocity;
        _theirPreviousVelocity = theirVelocity;
        _theirPreviousAcceleration = theirAcceleration;
        _interceptEventPositionValidFor -= 1f / 60f;
        _jerkEventPositionValidFor -= 1f / 60f;
        _jerkLastFrame = jerk;
            
        Program.I.DebugApi.DrawPoint(_lastJerkEventPosition, Color.Yellow, 5f, 1f, true);
        Program.I.DebugApi.DrawPoint(_lastInterceptEventPosition, Color.Green, 5f, 1f, true);
            
            
        var dodgeVal = ourPos - _lastJerkEventPosition;
        dodgeVal -= (themToUs * dodgeVal.Dot(themToUs));
        if (_jerkEventPositionValidFor > 0 && dodgeVal.LengthSquared() < 100 * 100)
            return dodgeVal.Normalized();


        dodgeVal = ourPos - _lastInterceptEventPosition;
        dodgeVal -= (themToUs * dodgeVal.Dot(themToUs));
        if (_interceptEventPositionValidFor >= 0 && dodgeVal.LengthSquared() < 100 * 100)
            return dodgeVal.Normalized();
            
            
            
        return Vector3D.Zero;
    }


    private static List<double> previousJerks = new List<double>();
    private const int windowSize = 180;
    private const double outlierThresholdMultiplier = 4.0; // 4 std devs

    private static double CheckForTheirJerk(Vector3D themToUs, Vector3D theirAcceleration)
    {
        Vector3D theirJerk = theirAcceleration - _theirPreviousAcceleration;
        double projectedJerk = theirJerk.Dot(themToUs.Normalized());

        previousJerks.Add(projectedJerk);
        if (previousJerks.Count > windowSize)
            previousJerks.RemoveAt(0);

        if (previousJerks.Count < 10) 
        {
            // Not enough data to decide yet
            return 0;
        }

        double mean = previousJerks.Average();
        double variance = previousJerks.Average(v => Math.Pow(v - mean, 2));
        double stdDev = Math.Sqrt(variance);

        // Outlier detection: current projected jerk > mean + k*stdDev
        bool isOutlier = Math.Abs(projectedJerk) > mean + outlierThresholdMultiplier * stdDev;
            
        // if (isOutlier)
        //     ALDebug.AddText($"Jerk: {projectedJerk:F3}, Mean: {mean:F3}, StdDev: {stdDev:F3}, Outlier: {isOutlier}");

        if (!isOutlier) return 0;
            
        return projectedJerk;
    }
        
        
    private static Vector3D GetClosestDirectionVector(MatrixD orientation, Vector3D fromThemToUs)
    {
        Vector3D bestVector = orientation.Forward;
        double bestDot = Vector3D.Dot(fromThemToUs, bestVector);

        Vector3D candidate = -orientation.Forward;
        double dot = Vector3D.Dot(fromThemToUs, candidate);
        if (dot > bestDot) { bestDot = dot; bestVector = candidate; }

        candidate = orientation.Right;
        dot = Vector3D.Dot(fromThemToUs, candidate);
        if (dot > bestDot) { bestDot = dot; bestVector = candidate; }

        candidate = -orientation.Right;
        dot = Vector3D.Dot(fromThemToUs, candidate);
        if (dot > bestDot) { bestDot = dot; bestVector = candidate; }

        candidate = orientation.Up;
        dot = Vector3D.Dot(fromThemToUs, candidate);
        if (dot > bestDot) { bestDot = dot; bestVector = candidate; }

        candidate = -orientation.Up;
        dot = Vector3D.Dot(fromThemToUs, candidate);
        if (dot > bestDot) { bestDot = dot; bestVector = candidate; }

        return bestVector;
    }
        
    private static Vector3D ClosestPointOnSegment(Vector3D p0, Vector3D p1, Vector3D point)
    {
        Vector3D line = p1 - p0;
        double lineLengthSq = line.LengthSquared();

        if (lineLengthSq == 0)
            return p0; // Degenerate line

        double t = Vector3D.Dot(point - p0, line) / lineLengthSq;
        t = Math.Max(0.0, Math.Min(1.0, t)); // Clamp to segment

        return p0 + line * t;
    }
}
internal static class BlueScreenOfDeath
{
    private const int MAX_BSOD_WIDTH = 50;

    private const string BSOD_TEMPLATE =
        "{0} - v{1}\n\n" +
        "A fatal exception has occured at\n" +
        "{2}. The current\n" +
        "program will be terminated.\n" +
        "\n" +
        "EXCEPTION:\n" +
        "{3}\n" +
        "\n" +
        "* Please screenshot this crash screen and\n" +
        "  DM to DeltaWing or\n" +
        "  Post in the #script-babble channel\n" +
        "\n" +
        "* Press RECOMPILE to restart the program";

    private static readonly StringBuilder bsodBuilder = new StringBuilder(256);

    public static void Show(IMyTextSurface surface, string scriptName, string version, Exception e)
    {
        if (surface == null) return;
        surface.ContentType = ContentType.TEXT_AND_IMAGE;
        surface.Alignment = TextAlignment.LEFT;
        var scaleFactor = 512f / Math.Min(surface.TextureSize.X, surface.TextureSize.Y);
        surface.FontSize = scaleFactor * surface.TextureSize.X / (19.5f * MAX_BSOD_WIDTH);
        surface.FontColor = Color.White;
        surface.BackgroundColor = Color.Blue;
        surface.Font = "Monospace";
        var exceptionStr = e.ToString();
        var exceptionLines = exceptionStr.Split('\n');
        bsodBuilder.Clear();
        foreach (var line in exceptionLines)
            if (line.Length <= MAX_BSOD_WIDTH)
            {
                bsodBuilder.Append(line).Append("\n");
            }
            else
            {
                var words = line.Split(' ');
                var lineLength = 0;
                foreach (var word in words)
                {
                    lineLength += word.Length;
                    if (lineLength >= MAX_BSOD_WIDTH)
                    {
                        bsodBuilder.Append("\n");
                        lineLength = word.Length;
                    }

                    bsodBuilder.Append(word).Append(" ");
                    lineLength += 1;
                }

                bsodBuilder.Append("\n");
            }

        surface.WriteText(string.Format(BSOD_TEMPLATE,
            scriptName.ToUpperInvariant(),
            version,
            DateTime.Now,
            bsodBuilder));
    }
}
public class DebugAPI
    {
        public readonly bool ModDetected;

        public void RemoveDraw() => _rmd?.Invoke(_pb);
        Action<IMyProgrammableBlock> _rmd;

        public void RemoveAll() => _rma?.Invoke(_pb);
        Action<IMyProgrammableBlock> _rma;

        public void Remove(int id) => _rm?.Invoke(_pb, id);
        Action<IMyProgrammableBlock, int> _rm;

        public int DrawPoint(Vector3D origin, Color color, float radius = 0.2f, float seconds = DefaultSeconds, bool? onTop = null) => _p?.Invoke(_pb, origin, color, radius, seconds, onTop ?? _defTop) ?? -1;
        Func<IMyProgrammableBlock, Vector3D, Color, float, float, bool, int> _p;

        public int DrawLine(Vector3D start, Vector3D end, Color color, float thickness = DefaultThickness, float seconds = DefaultSeconds, bool? onTop = null) => _ln?.Invoke(_pb, start, end, color, thickness, seconds, onTop ?? _defTop) ?? -1;
        Func<IMyProgrammableBlock, Vector3D, Vector3D, Color, float, float, bool, int> _ln;

        public int DrawAABB(BoundingBoxD bb, Color color, Style style = Style.Wireframe, float thickness = DefaultThickness, float seconds = DefaultSeconds, bool? onTop = null) => _bb?.Invoke(_pb, bb, color, (int)style, thickness, seconds, onTop ?? _defTop) ?? -1;
        Func<IMyProgrammableBlock, BoundingBoxD, Color, int, float, float, bool, int> _bb;

        public int DrawOBB(MyOrientedBoundingBoxD obb, Color color, Style style = Style.Wireframe, float thickness = DefaultThickness, float seconds = DefaultSeconds, bool? onTop = null) => _obb?.Invoke(_pb, obb, color, (int)style, thickness, seconds, onTop ?? _defTop) ?? -1;
        Func<IMyProgrammableBlock, MyOrientedBoundingBoxD, Color, int, float, float, bool, int> _obb;

        public int DrawSphere(BoundingSphereD sphere, Color color, Style style = Style.Wireframe, float thickness = DefaultThickness, int lineEveryDegrees = 15, float seconds = DefaultSeconds, bool? onTop = null) => _sph?.Invoke(_pb, sphere, color, (int)style, thickness, lineEveryDegrees, seconds, onTop ?? _defTop) ?? -1;
        Func<IMyProgrammableBlock, BoundingSphereD, Color, int, float, int, float, bool, int> _sph;

        public int DrawMatrix(MatrixD matrix, float length = 1f, float thickness = DefaultThickness, float seconds = DefaultSeconds, bool? onTop = null) => _m?.Invoke(_pb, matrix, length, thickness, seconds, onTop ?? _defTop) ?? -1;
        Func<IMyProgrammableBlock, MatrixD, float, float, float, bool, int> _m;

        public int DrawGPS(string name, Vector3D origin, Color? color = null, float seconds = DefaultSeconds) => _gps?.Invoke(_pb, name, origin, color, seconds) ?? -1;
        Func<IMyProgrammableBlock, string, Vector3D, Color?, float, int> _gps;

        public int PrintHUD(string message, Font font = Font.Debug, float seconds = 2) => _hud?.Invoke(_pb, message, font.ToString(), seconds) ?? -1;
        Func<IMyProgrammableBlock, string, string, float, int> _hud;

        public void PrintChat(string message, string sender = null, Color? senderColor = null, Font font = Font.Debug) => _chat?.Invoke(_pb, message, sender, senderColor, font.ToString());
        Action<IMyProgrammableBlock, string, string, Color?, string> _chat;

        public void DeclareAdjustNumber(out int id, double initial, double step = 0.05, Input modifier = Input.Control, string label = null) => id = _adj?.Invoke(_pb, initial, step, modifier.ToString(), label) ?? -1;
        Func<IMyProgrammableBlock, double, double, string, string, int> _adj;

        public double GetAdjustNumber(int id, double noModDefault = 1) => _getAdj?.Invoke(_pb, id) ?? noModDefault;
        Func<IMyProgrammableBlock, int, double> _getAdj;

        public int GetTick() => _tk?.Invoke() ?? -1;
        Func<int> _tk;

        public TimeSpan GetTimestamp() => _ts?.Invoke() ?? TimeSpan.Zero;
        Func<TimeSpan> _ts;

        public MeasureToken Measure(Action<TimeSpan> call) => new MeasureToken(this, call);
        public struct MeasureToken : IDisposable
        {
            DebugAPI A; TimeSpan S; Action<TimeSpan> C;
            public MeasureToken(DebugAPI api, Action<TimeSpan> call) { A = api; C = call; S = A.GetTimestamp(); }
            public void Dispose() { C?.Invoke(A.GetTimestamp() - S); }
        }

        public enum Style { Solid, Wireframe, SolidAndWireframe }
        public enum Input { MouseLeftButton, MouseRightButton, MouseMiddleButton, MouseExtraButton1, MouseExtraButton2, LeftShift, RightShift, LeftControl, RightControl, LeftAlt, RightAlt, Tab, Shift, Control, Alt, Space, PageUp, PageDown, End, Home, Insert, Delete, Left, Up, Right, Down, D0, D1, D2, D3, D4, D5, D6, D7, D8, D9, A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z, NumPad0, NumPad1, NumPad2, NumPad3, NumPad4, NumPad5, NumPad6, NumPad7, NumPad8, NumPad9, Multiply, Add, Separator, Subtract, Decimal, Divide, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12 }
        public enum Font { Debug, White, Red, Green, Blue, DarkBlue }
        const float DefaultThickness = 0.02f;
        const float DefaultSeconds = -1;
        IMyProgrammableBlock _pb;
        bool _defTop;
        public DebugAPI(MyGridProgram program, bool drawOnTopDefault = false)
        {
            if(program == null) throw new Exception("Pass `this` into the API, not null.");
            _defTop = drawOnTopDefault;
            _pb = program.Me;
            var methods = _pb.GetProperty("DebugAPI")?.As<IReadOnlyDictionary<string, Delegate>>()?.GetValue(_pb);
            if(methods != null)
            {
                Assign(out _rma, methods["RemoveAll"]);
                Assign(out _rmd, methods["RemoveDraw"]);
                Assign(out _rm, methods["Remove"]);
                Assign(out _p, methods["Point"]);
                Assign(out _ln, methods["Line"]);
                Assign(out _bb, methods["AABB"]);
                Assign(out _obb, methods["OBB"]);
                Assign(out _sph, methods["Sphere"]);
                Assign(out _m, methods["Matrix"]);
                Assign(out _gps, methods["GPS"]);
                Assign(out _hud, methods["HUDNotification"]);
                Assign(out _chat, methods["Chat"]);
                Assign(out _adj, methods["DeclareAdjustNumber"]);
                Assign(out _getAdj, methods["GetAdjustNumber"]);
                Assign(out _tk, methods["Tick"]);
                Assign(out _ts, methods["Timestamp"]);
                RemoveAll();
                ModDetected = true;
            }
        }
        void Assign<T>(out T field, object method) => field = (T)method;
    }
public static class FlightDataRecorder
{
    public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    public static uint Frame;
    public static uint LastFrame;

    private static readonly StringBuilder FlightDataSB = new StringBuilder();


    public static IMyShipController FlightDataBlock;

    private static bool loggedTimeThisFrame;

    private static readonly ushort instructionReserved = 1024;

    public static void WriteData()
    {
    }

    private static double RoundToDP(double value, int decimalPlaces)
    {
        var multiplier = Math.Pow(10, decimalPlaces);
        return Math.Round(value * multiplier) / multiplier;
    }

    public static void TickFrame()
    {
        Frame++;
        loggedTimeThisFrame = false;
    }

    public static uint ToUnixTimestamp(this DateTime time)
    {
        return (uint)time.Subtract(Epoch).TotalSeconds;
    }


    public static string ushortToString(ushort value)
    {
        var character = (char)value;
        return character.ToString();
    }

    // Create a function to get the seconds since today started
    public static uint GetSecondsSinceToday()
    {
        return (uint)DateTime.Now.TimeOfDay.TotalSeconds;
    }

    // Create a function to get the milliseconds since the hour started
    public static uint GetMillisecondsSinceHour()
    {
        var now = DateTime.Now;
        var startOfHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, 0);
        var timeElapsed = now - startOfHour;
        return (uint)timeElapsed.TotalMilliseconds;
    }


    public static void LogRealTime()
    {
        FlightDataSB.Append("T+").Append(ToUnixTimestamp(DateTime.Now)).AppendLine();
    }

    private static void TryLogTime()
    {
        if (!loggedTimeThisFrame)
        {
            FlightDataSB.Append((char)(ushort)(Frame - LastFrame + instructionReserved));
            LastFrame = Frame;
            loggedTimeThisFrame = true;
        }
    }


    public static void WeaponRechargedEvent(char name)
    {
        TryLogTime();
        FlightDataSB.Append((char)1).Append(name);
    }

    public static void WeaponBeginChargeEvent(char name, double sigma, double range)
    {
        TryLogTime();
        FlightDataSB.Append((char)2).Append(name).Append((char)(Half)sigma).Append((char)(Half)range);
    }

    public static void WeaponCancelChargeEvent(char name, double sigma, double range)
    {
        TryLogTime();
        FlightDataSB.Append((char)3).Append(name).Append((char)(Half)sigma).Append((char)(Half)range);
    }

    public static void WeaponFireEvent(char name, double sigma, double range)
    {
        TryLogTime();
        FlightDataSB.Append((char)4).Append(name).Append((char)(Half)sigma).Append((char)(Half)range);
    }

    public static void WeaponRepairedEvent(char nameShorthand)
    {
        TryLogTime();
        FlightDataSB.Append((char)5).Append(nameShorthand);
    }

    public static void WeaponDamagedEvent(char nameShorthand)
    {
        TryLogTime();
        FlightDataSB.Append((char)6).Append(nameShorthand);
    }

    public static void WeaponCreatedEvent(char nameShorthand)
    {
        TryLogTime();
        FlightDataSB.Append((char)7).Append(nameShorthand);
    }


    internal static void GravityDriveEnabledEvent()
    {
        TryLogTime();
        FlightDataSB.Append((char)8);
    }

    internal static void GravityDriveDisabledEvent()
    {
        TryLogTime();
        FlightDataSB.Append((char)9);
    }

    internal static void GravityDriveRepulseEnabledEvent()
    {
        TryLogTime();
        FlightDataSB.Append((char)10);
    }

    internal static void GravityDriveRepulseDisabledEvent()
    {
        TryLogTime();
        FlightDataSB.Append((char)11);
    }

    public static void GravityDriveMassBalanceEvent(float previousMassError, float massError)
    {
        TryLogTime();
        FlightDataSB.Append((char)12).Append((char)(Half)previousMassError).Append((char)(Half)massError);
    }

    public static void GravityDriveBallBalanceEvent(float previousMassError, float massError)
    {
        TryLogTime();
        FlightDataSB.Append((char)13).Append((char)(Half)previousMassError).Append((char)(Half)massError);
    }

    internal static void GravityDriveDampenersChangedEvent(bool dampeners)
    {
        TryLogTime();
        FlightDataSB.Append((char)14).Append(dampeners ? '1' : '0');
    }

    internal static void GravityDrivePrecisionChangedEvent(bool precision)
    {
        TryLogTime();
        FlightDataSB.Append((char)15).Append(precision ? '1' : '0');
    }


    internal static void PlayerEnteredCockpitEvent()
    {
        TryLogTime();
        FlightDataSB.Append((char)16);
    }

    internal static void PlayerExitedCockpitEvent()
    {
        TryLogTime();
        FlightDataSB.Append((char)17);
    }

    internal static void AutoGetNewBlocksEvent(int count)
    {
        TryLogTime();
        FlightDataSB.Append((char)18).Append((char)(ushort)count);
    }


    internal static string GetAndClearFlightData()
    {
        var data = FlightDataSB.ToString();
        FlightDataSB.Clear();
        return data;
    }

    internal static string GetFlightData()
    {
        return FlightDataSB.ToString();
    }
}
internal class GravityDriveManager
{
    // This is calculated out in relation to how many mass blocks there are
    // The threshold will be higher if there are more mass blocks
    // The threshold will be lower if there are fewer mass blocks


    bool Dampeners;
    double forwardBackwardForce;
    private readonly List<LinearGravityGenerator> forwardBackwardGravity;
    private readonly List<SphericalGravityGenerator> forwardBackwardSphericalGravity;

    int frame;
    int framesIdle;

    bool gravityDriveDisabled = true;
    private readonly int gravityDriveDisableDelay = 600;

    private readonly List<IMyGravityGenerator> gravityGeneratorReferenceList;
    private readonly List<IMyGravityGeneratorSphere> gravityGeneratorSphereReferenceList;

    private readonly IMyGridTerminalSystem GridTerminalSystem;
    int interruptedFor;
    double leftRightForce;
    private readonly List<LinearGravityGenerator> leftRightGravity;

    private readonly int massBlockBalanceDelay = 301; // 5 seconds


    private readonly double
        massBlockMinimumMomentPercentageToDisable =
            1d / 1000; // Change must be greater than this percentage to disable a mass block

    private readonly List<MassBlock> massBlocks;
    private readonly List<IMyArtificialMassBlock> massBlocksReferenceList;

    private readonly float passiveSphericalRadius;

    bool Precision;
    bool PreviousDampeners;


    Vector3D PreviousMoveIndicator = Vector3D.Zero;
    bool PreviousPrecision;

    GravityDriveManagerState
        previousState =
            GravityDriveManagerState.ActiveMove; // Just so that it gets to a deterministic state on the first run

    Vector3D PreviousTransformedVelocity = Vector3D.Zero;

    private readonly MyGridProgram program;
    private readonly float repulseSphericalRadius;


    IMyShipController ShipController;
    double shipMass;
    private readonly int spaceBallBalanceDelay = 1201; // 20 seconds
    private readonly int spaceBallBalanceOffset = 2000000;
    private readonly List<IMySpaceBall> spaceBallReferenceList;

    private readonly List<SpaceBall> spaceBalls;
    GravityDriveManagerState state = GravityDriveManagerState.Idle;
    double upDownForce;

    private readonly List<LinearGravityGenerator> upDownGravity;

    public GravityDriveManager(List<IMyArtificialMassBlock> massBlocks, List<IMySpaceBall> spaceBalls,
        IMyGridTerminalSystem GridTerminalSystem, MyGridProgram program,
        List<IMyGravityGenerator> gravityGenerators, List<IMyGravityGeneratorSphere> sphericalGravityGenerators,
        IMyShipController shipController, float passiveSphericalRadius, float repulseSphericalRadius)
    {
        ShipController = shipController;
        this.massBlocks = new List<MassBlock>();
        massBlocksReferenceList = massBlocks;
        foreach (var block in massBlocks) this.massBlocks.Add(new MassBlock(block));
        this.spaceBalls = new List<SpaceBall>();
        spaceBallReferenceList = spaceBalls;
        foreach (var ball in spaceBalls) this.spaceBalls.Add(new SpaceBall(ball));
        this.GridTerminalSystem = GridTerminalSystem;
        this.program = program;
        gravityGeneratorReferenceList = gravityGenerators;
        gravityGeneratorSphereReferenceList = sphericalGravityGenerators;
        this.passiveSphericalRadius = passiveSphericalRadius;
        this.repulseSphericalRadius = repulseSphericalRadius;

        forwardBackwardSphericalGravity = new List<SphericalGravityGenerator>();
        var forwardReferenceForward = shipController.WorldMatrix.Forward;
        var centerOfMass = shipController.CenterOfMass;
        foreach (var gravityGenerator in sphericalGravityGenerators)
        {
            var pos = gravityGenerator.WorldMatrix.Translation;
            var centerOfMassToBlock = centerOfMass - pos;
            if (Vector3D.Dot(centerOfMassToBlock, forwardReferenceForward) > 0)
                forwardBackwardSphericalGravity.Add(new SphericalGravityGenerator(gravityGenerator, -1, "Rear"));
            else
                forwardBackwardSphericalGravity.Add(new SphericalGravityGenerator(gravityGenerator, 1, "Forward"));
        }

        upDownGravity = new List<LinearGravityGenerator>();
        leftRightGravity = new List<LinearGravityGenerator>();
        forwardBackwardGravity = new List<LinearGravityGenerator>();
        foreach (var generator in gravityGenerators)
            if (generator.WorldMatrix.Up == -shipController.WorldMatrix.Forward)
                forwardBackwardGravity.Add(new LinearGravityGenerator(generator, 1, "Forward"));
            else if (generator.WorldMatrix.Up == shipController.WorldMatrix.Forward)
                forwardBackwardGravity.Add(new LinearGravityGenerator(generator, -1, "Backward"));
            else if (generator.WorldMatrix.Up == -shipController.WorldMatrix.Left)
                leftRightGravity.Add(new LinearGravityGenerator(generator, 1, "Left"));
            else if (generator.WorldMatrix.Up == shipController.WorldMatrix.Left)
                leftRightGravity.Add(new LinearGravityGenerator(generator, -1, "Right"));
            else if (generator.WorldMatrix.Up == -shipController.WorldMatrix.Up)
                upDownGravity.Add(new LinearGravityGenerator(generator, 1, "Up"));
            else if (generator.WorldMatrix.Up == shipController.WorldMatrix.Up)
                upDownGravity.Add(new LinearGravityGenerator(generator, -1, "Down"));
        InitializeMassValues();
        forwardBackwardForce = CalculateForceFromGravityGenerators(forwardBackwardGravity,
            forwardBackwardSphericalGravity, shipController.WorldMatrix.Forward);
        leftRightForce = CalculateForceFromGravityGenerators(leftRightGravity);
        upDownForce = CalculateForceFromGravityGenerators(upDownGravity);
        var shipMass = shipController.CalculateShipMass();
        this.shipMass = shipMass.PhysicalMass;

        CalculateMoments(centerOfMass, this.massBlocks);
        CalculateMoments(centerOfMass, this.spaceBalls);
    }

    public void Interrupt(int frames)
    {
        if (state == GravityDriveManagerState.Repulse) return;
        SetGravityForce(upDownGravity, 0);
        SetGravityForce(leftRightGravity, 0);
        SetGravityForce(forwardBackwardGravity, 0);
        SetGravityForce(forwardBackwardSphericalGravity, 0);
        interruptedFor = frames;
    }

    public void Update(IMyShipController ShipController, bool precision, bool doRepulse, bool doBalance, Vector3D autoDodgeVal)
    {
            
        this.ShipController = ShipController;
        Precision = precision;
        Dampeners = ShipController.DampenersOverride;

        if (Dampeners != PreviousDampeners) FlightDataRecorder.GravityDriveDampenersChangedEvent(Dampeners);
        if (Precision != PreviousPrecision) FlightDataRecorder.GravityDrivePrecisionChangedEvent(Precision);

            
        if (interruptedFor > 0)
        {
            interruptedFor--;
            return;
        }

        frame++;

        if (!gravityDriveDisabled && framesIdle > gravityDriveDisableDelay)
        {
            gravityDriveDisabled = true;
            EnableMassBlocks(false);
            EnableGenerators(false);
            FlightDataRecorder.GravityDriveDisabledEvent();
        }
            
        float baselineGravity = 9.81f;
        var naturalGravity = ShipController.GetNaturalGravity();
        var naturalGravityLength = naturalGravity.Length();
            
            
        var driveEffectiveness = MathHelper.Clamp(baselineGravity - (naturalGravityLength * 2), 0, baselineGravity) / baselineGravity;

        float driveMultiplier = 1;
        if (driveEffectiveness != 0)
        {
            driveMultiplier = (float)(1 / driveEffectiveness);
        }
            
        var velocity = ShipController.GetShipVelocities().LinearVelocity + (naturalGravity / 10);
            
            
        if (frame % 901 == 0) _shipMass = ShipController.CalculateShipMass();
            
        this.shipMass = _shipMass.PhysicalMass;
        velocity *= driveMultiplier;
            
        Vector3 transformedVelocity =
            Vector3D.TransformNormal(velocity, MatrixD.Transpose(ShipController.WorldMatrix));

            
            
        var transformedVelocityNormalized = transformedVelocity;

        if (transformedVelocityNormalized.LengthSquared() > 3)
        {
            transformedVelocityNormalized.X =
                MathHelper.Clamp(MathHelper.RoundOn2(transformedVelocityNormalized.X), -1, 1);
            transformedVelocityNormalized.Y =
                MathHelper.Clamp(MathHelper.RoundOn2(transformedVelocityNormalized.Y), -1, 1);
            transformedVelocityNormalized.Z =
                MathHelper.Clamp(MathHelper.RoundOn2(transformedVelocityNormalized.Z), -1, 1);
        }

        var velocityLengthSquared = velocity.ALengthSquared();

        Vector3D moveIndicator = ShipController.MoveIndicator;
        moveIndicator.X *= -1;
        moveIndicator.Z *= -1;
            

            
        if (autoDodgeVal.LengthSquared() != 0)
        {
            var transformedAutoDodgeVal = Vector3D.TransformNormal(autoDodgeVal, MatrixD.Transpose(ShipController.WorldMatrix));
            transformedAutoDodgeVal.X *= -1;
            moveIndicator += transformedAutoDodgeVal;
        }
            
        var moveIndicatorLengthSquared = moveIndicator.ALengthSquared();

        moveIndicator *= driveMultiplier;   
            
            
            
        previousState = state;
        state = GravityDriveManagerState.Idle;

        // if (we are trying to move) or (we are moving and Dampeners are on)
        if (moveIndicatorLengthSquared > 0 || (velocityLengthSquared > 0.000001 && Dampeners))
            state = GravityDriveManagerState.ActiveMove;
        // if (we are repulsing)
        if (doRepulse) state = GravityDriveManagerState.Repulse;
            

        switch (state)
        {
            // If the gravity drive should be idle


            case GravityDriveManagerState.Idle:

                if (previousState != GravityDriveManagerState.Idle)
                {
                    //EnableGenerators(false);
                    //EnableMassBlocks(false);
                    // I'll do something better here later, turning stuff on and off is expensive
                    SetGravityNoDampeners(Vector3D.Zero, 0);
                    PreviousMoveIndicator = Vector3D.Zero;
                    PreviousTransformedVelocity = Vector3D.Zero;
                }

                framesIdle++;
                break;


            // If the gravity drive should be active


            case GravityDriveManagerState.ActiveMove:

                if (previousState != GravityDriveManagerState.ActiveMove)
                {
                    if (!gravityDriveDisabled) break;
                    gravityDriveDisabled = false;
                    framesIdle = 0;
                    EnableMassBlocks(true);
                    EnableGenerators(true);
                    FlightDataRecorder.GravityDriveEnabledEvent();
                }


                var moveType = MoveType.NoDampenersNoPrecision;
                // Incorperate Dampeners and precision mode
                moveType = moveType + (Precision ? 2 : 0);
                moveType = moveType + (Dampeners ? 1 : 0);
                Move(moveType, moveIndicator, transformedVelocity, transformedVelocityNormalized);
                break;


            // If the mass blocks should be off and the spherical gravity generators should be on at full force


            case GravityDriveManagerState.Repulse:
                if (previousState != GravityDriveManagerState.Repulse)
                {
                    FlightDataRecorder.GravityDriveRepulseEnabledEvent();
                    Repulse();
                }

                break;

        }

            
        // obnoxious edge case
        if (previousState == GravityDriveManagerState.Repulse && state != GravityDriveManagerState.Repulse)
        {
            FlightDataRecorder.GravityDriveRepulseDisabledEvent();
            for (var i = 0; i < forwardBackwardSphericalGravity.Count; i++)
            {
                var spherical = forwardBackwardSphericalGravity[i];
                spherical.SetRadius(passiveSphericalRadius);
                EnableMassBlocks(true); // This should be fine
            }
        }
            
        if (!doBalance) return;
        if (frame % massBlockBalanceDelay == 0)
        {
                
            var centerOfMass = ShipController.CenterOfMass;
            CalculateMoments(centerOfMass, massBlocks);
                
            EnableMassBlockPairs();
            BalanceMassBlocks(massBlocks, spaceBalls, centerOfMass);

            //Temp
            UpdateMassState(state, massBlocks);
            forwardBackwardForce = CalculateForceFromGravityGenerators(forwardBackwardGravity,
                forwardBackwardSphericalGravity, ShipController.WorldMatrix.Forward);
            leftRightForce = CalculateForceFromGravityGenerators(leftRightGravity);
            upDownForce = CalculateForceFromGravityGenerators(upDownGravity);
            double previousMassError;
                
                
            double massError;

            var previousMassErrorVector = Vector3D.Zero;
            var massErrorVector = Vector3D.Zero;
            foreach (var massBlock in massBlocks)
            {
                previousMassErrorVector += massBlock.previousMoment;
                massErrorVector += massBlock.moment;
            }

            foreach (var spaceBall in spaceBalls)
            {
                previousMassErrorVector += spaceBall.previousMoment;
                massErrorVector += spaceBall.moment;
            }

            previousMassErrorVector /= massBlocks.Count + spaceBalls.Count;
            massErrorVector /= massBlocks.Count + spaceBalls.Count;
            previousMassError = previousMassErrorVector.ALength();
            massError = massErrorVector.ALength();

            FlightDataRecorder.GravityDriveMassBalanceEvent(MathHelper.RoundOn2((float)previousMassError / 1000),
                MathHelper.RoundOn2((float)massError / 1000));
                
        }
            
        if ((frame + spaceBallBalanceOffset) % spaceBallBalanceDelay == 0)
        {
            var centerOfMass = ShipController.CenterOfMass;
            CalculateMoments(centerOfMass, spaceBalls);
            BalanceSpaceBalls(massBlocks, spaceBalls, centerOfMass);
            UpdateMassState(state, spaceBalls);
            forwardBackwardForce = CalculateForceFromGravityGenerators(forwardBackwardGravity,
                forwardBackwardSphericalGravity, ShipController.WorldMatrix.Forward);
            leftRightForce = CalculateForceFromGravityGenerators(leftRightGravity);
            upDownForce = CalculateForceFromGravityGenerators(upDownGravity);
            double previousMassError;
            double massError;

            var previousMassErrorVector = Vector3D.Zero;
            var massErrorVector = Vector3D.Zero;
            foreach (var massBlock in massBlocks)
            {
                previousMassErrorVector += massBlock.previousMoment;
                massErrorVector += massBlock.moment;
            }

            foreach (var spaceBall in spaceBalls)
            {
                previousMassErrorVector += spaceBall.previousMoment;
                massErrorVector += spaceBall.moment;
            }

            previousMassErrorVector /= massBlocks.Count + spaceBalls.Count;
            massErrorVector /= massBlocks.Count + spaceBalls.Count;
            previousMassError = previousMassErrorVector.ALength();
            massError = massErrorVector.ALength();

            FlightDataRecorder.GravityDriveBallBalanceEvent(MathHelper.RoundOn2((float)previousMassError / 1000),
                MathHelper.RoundOn2((float)massError / 1000));
        }

        PreviousDampeners = Dampeners;
        PreviousPrecision = Precision;
    }

    void Move(MoveType moveType, Vector3D moveIndicator, Vector3 transformedVelocity,
        Vector3 transformedVelocityNormalized)
    {
        switch (moveType)
        {
            case MoveType.NoDampenersNoPrecision:
                if (moveIndicator != PreviousMoveIndicator) SetGravityNoDampeners(moveIndicator, 1);
                break;
            case MoveType.DampenersNoPrecision:
                if (moveIndicator != PreviousMoveIndicator ||
                    transformedVelocityNormalized != PreviousTransformedVelocity)
                    SetGravityDampeners(moveIndicator, transformedVelocity, 1f);
                break;
            case MoveType.NoDampenersPrecision:
                if (moveIndicator != PreviousMoveIndicator) SetGravityNoDampeners(moveIndicator, 0.1f);
                break;
            case MoveType.DampenersPrecision:
                if (moveIndicator != PreviousMoveIndicator ||
                    transformedVelocityNormalized != PreviousTransformedVelocity)
                    SetGravityDampeners(moveIndicator, transformedVelocity, 0.1f);
                break;
        }

        PreviousMoveIndicator = moveIndicator;
        PreviousTransformedVelocity = transformedVelocityNormalized;
    }

    void SetGravityNoDampeners(Vector3D moveIndicator, float multiplier)
    {
        moveIndicator *= multiplier;
        SetGravityForce(upDownGravity, (float)moveIndicator.Y);
        SetGravityForce(leftRightGravity, (float)moveIndicator.X);
        SetGravityForce(forwardBackwardGravity, (float)moveIndicator.Z);
        SetGravityForce(forwardBackwardSphericalGravity, (float)moveIndicator.Z);
    }

    void SetGravityDampeners(Vector3D moveIndicator, Vector3D transformedVelocity, float multiplier)
    {
        moveIndicator *= multiplier;
        if (moveIndicator.Y == 0)
        {
            var acceleration = shipMass / upDownForce;
            var dampenerForce = transformedVelocity.Y == 0 ? 0 : (float)(transformedVelocity.Y * 10 * acceleration);
            SetGravityForce(upDownGravity, -dampenerForce);
        }
        else
        {
            SetGravityForce(upDownGravity, (float)moveIndicator.Y);
        }

        if (moveIndicator.X == 0)
        {
            var acceleration = shipMass / leftRightForce;
            var dampenerForce = transformedVelocity.X == 0 ? 0 : (float)(transformedVelocity.X * 10 * acceleration);
            SetGravityForce(leftRightGravity, dampenerForce);
        }
        else
        {
            SetGravityForce(leftRightGravity, (float)moveIndicator.X);
        }

        if (moveIndicator.Z == 0)
        {
            var acceleration = shipMass / forwardBackwardForce;
            var dampenerForce = transformedVelocity.Z == 0 ? 0 : (float)(transformedVelocity.Z * 10 * acceleration);
            SetGravityForce(forwardBackwardGravity, dampenerForce);
            SetGravityForce(forwardBackwardSphericalGravity, dampenerForce);
        }
        else
        {
            SetGravityForce(forwardBackwardGravity, (float)moveIndicator.Z);
            SetGravityForce(forwardBackwardSphericalGravity,(float)moveIndicator.Z);
        }
    }

    void Repulse()
    {
        EnableMassBlocks(false);
        foreach (var spherical in forwardBackwardSphericalGravity)
        {
            spherical.actualGravityGenerator.Enabled = true;
            spherical.SetRadius(repulseSphericalRadius);
            spherical.SetGravityOverride(-9.81f);
        }

        SetGravityForce(upDownGravity, 0);
        SetGravityForce(leftRightGravity, 0);
        SetGravityForce(forwardBackwardGravity, 0);
    }


    void InitializeMassValues()
    {
        foreach (var ball in spaceBalls) ball.block.VirtualMass = 20000;
    }


    void EnableGenerators(bool state)
    {
        SetGravityGeneratorsEnabled(upDownGravity, state);
        SetGravityGeneratorsEnabled(leftRightGravity, state);
        SetGravityGeneratorsEnabled(forwardBackwardGravity, state);
        SetGravityGeneratorsEnabled(forwardBackwardSphericalGravity, state);
    }


    void CalculateMoments(Vector3D centerOfMass, List<MassBlock> massBlocks, List<SpaceBall> spaceBalls)
    {
        foreach (var massBlock in massBlocks) massBlock.CalculateMoment(centerOfMass);
        foreach (var spaceBall in spaceBalls) spaceBall.CalculateMoment(centerOfMass);
    }

    void CalculateMoments(Vector3D centerOfMass, List<SpaceBall> spaceBalls)
    {
        foreach (var spaceBall in spaceBalls) spaceBall.CalculateMoment(centerOfMass);
    }

    void CalculateMoments(Vector3D centerOfMass, List<MassBlock> massBlocks)
    {
        for (var i = massBlocks.Count - 1; i >= 0; i--)
        {
            var massBlock = massBlocks[i];
            if (massBlock.block.Closed || !GridTerminalSystem.CanAccess(massBlock.block))
            {
                massBlocks.RemoveAt(i);
                massBlocksReferenceList.Remove(massBlock.block);
            }

            massBlock.CalculateMoment(centerOfMass);
        }
    }

    void UpdateMassState(GravityDriveManagerState state, List<SpaceBall> spaceBalls)
    {
        if (state != GravityDriveManagerState.ActiveMove) return;
        foreach (var spaceBall in spaceBalls) spaceBall.block.Enabled = spaceBall.Enabled;
    }

    void UpdateMassState(GravityDriveManagerState state, List<MassBlock> massBlocks)
    {
        if (state != GravityDriveManagerState.ActiveMove) return;
        foreach (var massBlock in massBlocks) massBlock.block.Enabled = massBlock.Enabled;
    }

        
    int i = 0, j = 1;
    MyShipMass _shipMass;

    void EnableMassBlockPairs()
    {
           
        var disabled = new List<MassBlock>();
        var disabledCount = disabled.Count;
                
        foreach (var mass in massBlocks)
            if (!mass.Enabled) disabled.Add(mass);
        if (disabledCount <= 1) return;
        for (int step = 0; step < 50000; step++)
        {
            if (j >= disabledCount)
            {
                i++;
                if (i >= disabledCount - 1)
                {
                    break;
                }
                j = i+1;
            }
                
            var a = disabled[i];
            var b = disabled[j];

            var added = a.moment + b.moment;
            var lengthSquared = added.X * added.X + added.Y * added.Y + added.Z * added.Z;
            if (lengthSquared < 1) {
                a.Enabled = b.Enabled = true;
            }
            j++;
        }

        if (i >= disabledCount || i + 1 >= disabledCount)
        {
            i = 0;
            j = 1;
        }
    }

    void BalanceMassBlocks(List<MassBlock> blocks, List<SpaceBall> balls, Vector3D centerOfMass)
    {
        var currentMoment = Vector3D.Zero;

        var maximumPossibleMoment = Vector3D.Zero;
            
        // Calculate current total moment
        foreach (var block in blocks)
        {
            currentMoment += block.Enabled ? block.moment : Vector3D.Zero;
            maximumPossibleMoment += Vector3D.Abs(block.moment);
        }
            
        foreach (var ball in balls)
        {
            currentMoment += ball.moment;
            maximumPossibleMoment += Vector3D.Abs(ball.moment);
        }
            

        var maximumPossibleMomentLengthSquared = maximumPossibleMoment.ALengthSquared();
        // Balance the moments by toggling blocks with a preference for turning them on

        //Vector3D largestMomentChange = GetLargestChangingMoment(blocks);
        int count = 0;
        foreach (var block in blocks)
        {
            count++;
                
            if (currentMoment.ALengthSquared() > (long)50000 * 50000)
            {
                var isOn = block.Enabled;

                var momentNoChange = isOn ? block.moment : Vector3D.Zero;
                var momentWithChange = isOn ? Vector3D.Zero : block.moment;


                // Check if the difference between the current moment and the moment with the block toggled is greater than a certain percentage of the total moment
                // var impact = (block.moment / maximumPossibleMoment).ALengthSquared();
                // if (isOn && impact < massBlockMinimumMomentPercentageToDisable *
                //     massBlockMinimumMomentPercentageToDisable) continue;

                currentMoment = currentMoment - momentNoChange;


                // If changing reduces the moment, toggle (multiplied for bias to turning on blocks)
                if ((currentMoment + momentWithChange).ALengthSquared() * (isOn ? 1.03 : 0.97) <
                    (currentMoment + momentNoChange).ALengthSquared())
                {
                    block.Enabled = !isOn;
                    currentMoment = currentMoment + momentWithChange;
                }
                else
                {
                    currentMoment = currentMoment + momentNoChange;
                }
            }
            else
            {
                break; // If the total moment is balanced, exit the loop
            }
                
        }
                
    }

    //private Vector3D GetLargestChangingMoment(List<MassBlock> blocks)
    //{
    //    Vector3D largest = Vector3D.Zero;
    //    foreach (var block in blocks)
    //    {
    //        Vector3D momentChange = block.moment - block.previousMoment;
    //        if (momentChange.ALengthSquared() > largest.ALengthSquared())
    //        {
    //            largest = momentChange;
    //        }
    //    }
    //}

    void BalanceSpaceBalls(List<MassBlock> blocks, List<SpaceBall> balls, Vector3D centerOfMass)
    {
        var totalMoment = Vector3D.Zero;

        // Calculate current total moment
        foreach (var block in blocks) totalMoment += block.Enabled ? block.moment : Vector3D.Zero;
        foreach (var ball in balls) totalMoment += ball.moment;
        foreach (var ball in spaceBalls)
            if (totalMoment.ALengthSquared() > 10 * 10)
            {
                var currentMass = ball.block.VirtualMass;
                var newMass = currentMass + 500;


                float step = 5000;
                // Adjust the mass to try to balance the moment
                // Calculate the current and new moments
                var currentMoment = ball.moment;
                var downMomentChange = ball.momentDown;
                var upMomentChange = ball.momentUp;


                var noAdjustedMoment = totalMoment;
                var downAdjustedMoment = totalMoment - currentMoment + downMomentChange;
                var upAdjustedMoment = totalMoment - currentMoment + upMomentChange;

                var downMomentLength = downAdjustedMoment.ALengthSquared() + 2 * 2;
                var upMomentLength =
                    upAdjustedMoment.ALengthSquared() - 2 * 2; // slight bias towards increasing mass
                var noAdjustedMomentLength = noAdjustedMoment.ALengthSquared();

                var finalMoment = totalMoment;

                var moveUpMul = upMomentChange.ALengthSquared() / currentMoment.ALengthSquared() * step;
                var moveDownMul = downMomentChange.ALengthSquared() / currentMoment.ALengthSquared() * step;

                ball.previousMoveUpMul = moveUpMul;
                ball.previousMoveDownMul = moveDownMul;


                if (downMomentLength < noAdjustedMomentLength && downMomentLength < upMomentLength)
                {
                    newMass = Math.Max(currentMass - (float)moveDownMul * 0.99f, 0);
                    finalMoment = downAdjustedMoment;
                }
                else if (upMomentLength < noAdjustedMomentLength && upMomentLength < downMomentLength)
                {
                    newMass = Math.Min(currentMass + (float)moveUpMul * 1.01f, 20000);
                    finalMoment = upAdjustedMoment;
                }
                // Assuming you want to keep the mass unchanged if momentNoChange is the smallest
                else
                {
                    newMass = currentMass;
                }

                ball.SetMass(newMass);
                totalMoment = finalMoment;
            }
    }

    void SetGravityGeneratorsEnabled(List<LinearGravityGenerator> gravityGeneratorList, bool enabled)
    {
        for (var i = 0; i < gravityGeneratorList.Count; i++)
        {
            var gravityGenerator = gravityGeneratorList[i];
            gravityGenerator.actualGravityGenerator.Enabled = enabled;
        }
    }

    void SetGravityGeneratorsEnabled(List<SphericalGravityGenerator> gravityGeneratorList, bool enabled)
    {
        for (var i = 0; i < gravityGeneratorList.Count; i++)
        {
            var gravityGenerator = gravityGeneratorList[i];
            gravityGenerator.actualGravityGenerator.Enabled = enabled;
        }
    }

    void SetGravityForce(List<LinearGravityGenerator> gravityGeneratorList, float gravity)
    {
        for (var i = 0; i < gravityGeneratorList.Count; i++)
        {
            var gravityGenerator = gravityGeneratorList[i];
            gravityGenerator.SetGravity(gravity);
        }
    }

    // I love overloading
    void SetGravityForce(List<SphericalGravityGenerator> gravityGeneratorList, float gravity)
    {
        for (var i = 0; i < gravityGeneratorList.Count; i++)
        {
            var gravityGenerator = gravityGeneratorList[i];
            gravityGenerator.SetGravity(gravity);
        }
    }

    public void UpdateBlocksList(List<IMyArtificialMassBlock> mass, List<IMySpaceBall> balls,
        List<IMyGravityGenerator> linearGrav, List<IMyGravityGeneratorSphere> sphericalGrav,
        IMyShipController forwardReference)
    {
        foreach (var block in mass)
            if (!massBlocksReferenceList.Contains(block))
            {
                massBlocksReferenceList.Add(block);
                massBlocks.Add(new MassBlock(block));
                block.Enabled = state == GravityDriveManagerState.ActiveMove ? true : false;
            }

        foreach (var block in balls)
            if (!spaceBallReferenceList.Contains(block))
            {
                spaceBallReferenceList.Add(block);
                spaceBalls.Add(new SpaceBall(block));
                block.Enabled = state == GravityDriveManagerState.ActiveMove ? true : false;
            }

        foreach (var block in linearGrav)
            if (!gravityGeneratorReferenceList.Contains(block))
            {
                gravityGeneratorReferenceList.Add(block);
                if (block.WorldMatrix.Up == -ShipController.WorldMatrix.Forward)
                    forwardBackwardGravity.Add(new LinearGravityGenerator(block, 1, "Forward"));
                else if (block.WorldMatrix.Up == ShipController.WorldMatrix.Forward)
                    forwardBackwardGravity.Add(new LinearGravityGenerator(block, -1, "Backward"));
                else if (block.WorldMatrix.Up == -ShipController.WorldMatrix.Left)
                    leftRightGravity.Add(new LinearGravityGenerator(block, -1, "Left"));
                else if (block.WorldMatrix.Up == ShipController.WorldMatrix.Left)
                    leftRightGravity.Add(new LinearGravityGenerator(block, 1, "Right"));
                else if (block.WorldMatrix.Up == -ShipController.WorldMatrix.Up)
                    upDownGravity.Add(new LinearGravityGenerator(block, 1, "Up"));
                else if (block.WorldMatrix.Up == ShipController.WorldMatrix.Up)
                    upDownGravity.Add(new LinearGravityGenerator(block, -1, "Down"));
            }

        // Clear out any gravity generators that are no longer in the list
        for (var i = forwardBackwardGravity.Count - 1; i >= 0; i--)
        {
            var gravityGenerator = forwardBackwardGravity[i];
            if (gravityGenerator.actualGravityGenerator.Closed ||
                !program.GridTerminalSystem.CanAccess(gravityGenerator.actualGravityGenerator))
            {
                forwardBackwardGravity.RemoveAt(i);
                gravityGeneratorReferenceList.Remove(gravityGenerator.actualGravityGenerator);
            }
        }

        for (var i = leftRightGravity.Count - 1; i >= 0; i--)
        {
            var gravityGenerator = leftRightGravity[i];
            if (gravityGenerator.actualGravityGenerator.Closed ||
                !program.GridTerminalSystem.CanAccess(gravityGenerator.actualGravityGenerator))
            {
                leftRightGravity.RemoveAt(i);
                gravityGeneratorReferenceList.Remove(gravityGenerator.actualGravityGenerator);
            }
        }

        for (var i = upDownGravity.Count - 1; i >= 0; i--)
        {
            var gravityGenerator = upDownGravity[i];
            if (gravityGenerator.actualGravityGenerator.Closed ||
                !program.GridTerminalSystem.CanAccess(gravityGenerator.actualGravityGenerator))
            {
                upDownGravity.RemoveAt(i);
                gravityGeneratorReferenceList.Remove(gravityGenerator.actualGravityGenerator);
            }
        }

        for (var i = forwardBackwardSphericalGravity.Count - 1; i >= 0; i--)
        {
            var gravityGenerator = forwardBackwardSphericalGravity[i];
            if (gravityGenerator.actualGravityGenerator.Closed ||
                !program.GridTerminalSystem.CanAccess(gravityGenerator.actualGravityGenerator))
            {
                forwardBackwardSphericalGravity.RemoveAt(i);
                gravityGeneratorSphereReferenceList.Remove(gravityGenerator.actualGravityGenerator);
            }
        }


        var forwardReferenceForward = forwardReference.WorldMatrix.Forward;
        var centerOfMass = forwardReference.CenterOfMass;
        foreach (var block in sphericalGrav)
            if (!gravityGeneratorSphereReferenceList.Contains(block))
            {
                gravityGeneratorSphereReferenceList.Add(block);
                var pos = block.WorldMatrix.Translation;
                var centerOfMassToBlock = centerOfMass - pos;
                if (Vector3D.Dot(centerOfMassToBlock, forwardReferenceForward) > 0)
                    forwardBackwardSphericalGravity.Add(new SphericalGravityGenerator(block, -1, "Rear"));
                else
                    forwardBackwardSphericalGravity.Add(new SphericalGravityGenerator(block, 1, "Forward"));
            }
    }


    public void EnableMassBlocks(bool state)
    {
        for (var i = massBlocks.Count - 1; i >= 0; i--)
        {
            var massBlock = massBlocks[i];
            if (massBlock.block.Closed || !GridTerminalSystem.CanAccess(massBlock.block))
            {
                massBlocks.RemoveAt(i);
                massBlocksReferenceList.Remove(massBlock.block);
                continue;
            }

            massBlock.block.Enabled = state ? massBlock.Enabled : false;
        }

        for (var i = spaceBalls.Count - 1; i >= 0; i--)
        {
            var spaceBall = spaceBalls[i];
            if (spaceBall.block.Closed || !GridTerminalSystem.CanAccess(spaceBall.block))
            {
                spaceBalls.RemoveAt(i);
                spaceBallReferenceList.Remove(spaceBall.block);
                continue;
            }

            spaceBall.block.Enabled = state;
        }
    }


    double CalculateForceFromGravityGenerators(List<LinearGravityGenerator> gravityGenerators)
    {
        // I love how elegant and simple this is
        var acceleration = 9.81 * gravityGenerators.Count;

        double mass = 0;
        foreach (var block in massBlocks) mass += block.Enabled ? 50000 : 0;
        foreach (var ball in spaceBalls) mass += ball.block.VirtualMass;
        return mass * acceleration;
    }

    double CalculateForceFromGravityGenerators(List<LinearGravityGenerator> gravityGenerators,
        List<SphericalGravityGenerator> sphericals, Vector3D forward)
    {
        // I hate how complicated and ugly this is
        var force = CalculateForceFromGravityGenerators(gravityGenerators);


        var allSphericalNetForce = Vector3D.Zero;
        // Because sphericals have a centralized gravity field instead of everything within the radius going in one direction, we need to calculate the force between each spherical and each mass block
        foreach (var spherical in sphericals)
        {
            var sphericalPosition = spherical.actualGravityGenerator.GetPosition();
            var thisSphericalNetForce = Vector3D.Zero;
            foreach (var block in massBlocks)
            {
                if (!block.Enabled) continue;
                var direction = (sphericalPosition - block.block.GetPosition()).Normalized();
                thisSphericalNetForce += direction * 50000 * 9.81 * spherical.sign;
            }

            foreach (var ball in spaceBalls)
            {
                var direction = (sphericalPosition - ball.block.GetPosition()).Normalized();
                thisSphericalNetForce += direction * ball.block.VirtualMass * 9.81 * spherical.sign;
            }

            allSphericalNetForce += thisSphericalNetForce;
        }

        var directionalForce = Vector3D.Dot(forward, allSphericalNetForce);

        return force + directionalForce;
    }


    

    enum GravityDriveManagerState
    {
        Idle,
        ActiveMove,
        Repulse
    }

    
    enum MoveType
    {
        NoDampenersNoPrecision,
        DampenersNoPrecision,
        NoDampenersPrecision,
        DampenersPrecision
    }
}
public delegate void GunFinishedFiringDelegate(Gun gun);
public class Gun
{
    private static readonly MyDefinitionId ElectricityId =
        new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Electricity");

    public IMyUserControllableGun actualGun;
    public bool Available;
    public bool AvailablePrevious;
    public float chargePercent;
    private readonly StringBuilder chargePercentSB = new StringBuilder();
    StringBuilder chargeTimeSB = new StringBuilder();
    private readonly StringBuilder detailedInfoSB = new StringBuilder();
    private readonly float FireDelay;
    public bool Functional;
    public bool FunctionalPrevious;
    public Vector3D gridPosition;
    private readonly GunFinishedFiringDelegate gunFinishedFiringDelegate;
    private readonly MyResourceSinkComponent gunSinkComponent;

    public char nameShorthand = ' ';
    public double PowerDraw;
    public double PowerDrawPrevious;
    public double range = 0;
    public int RemainingChargeSeconds = 0;
    public bool Shoot;

    public bool ShootPrevious;
    public double sigma = 0;
    float TimeSpentFiring;
    public float TimeToFire;

    public Gun(IMyUserControllableGun gun, Dictionary<MyDefinitionId, float> knownFireDelays,
        GunFinishedFiringDelegate gunFinishedFiringDelegate, MyGridProgram program, ushort id)
    {
        actualGun = gun;
        gunSinkComponent = actualGun.Components.Get<MyResourceSinkComponent>();
        if (!knownFireDelays.ContainsKey(gun.BlockDefinition))
            FireDelay = 0f;
        else
            FireDelay = knownFireDelays[gun.BlockDefinition];
        this.gunFinishedFiringDelegate = gunFinishedFiringDelegate;
        gridPosition = actualGun.Position;

        nameShorthand = (char)id;


        Functional = actualGun.IsFunctional;
        PowerDraw = gunSinkComponent.CurrentInputByType(ElectricityId);
        Available = Functional && PowerDraw < 0.002f;

        AvailablePrevious = Available;
        ShootPrevious = Shoot;
        FunctionalPrevious = Functional;
        PowerDrawPrevious = PowerDraw;

        FlightDataRecorder.WeaponCreatedEvent(nameShorthand);
    }

    public bool Enabled
    {
        get { return actualGun.Enabled; }

        set
        {
            if (actualGun.Enabled != value) actualGun.Enabled = value;
        }
    }


    public bool Closed => actualGun.Closed;

    public bool IsFunctional => actualGun.IsFunctional;

    public void Tick()
    {
        if (Available && Shoot)
        {
            actualGun.ShootOnce();
            TimeSpentFiring = Math.Min(TimeSpentFiring + 1f / 60f, FireDelay);
        }
        else
        {
            if (TimeSpentFiring > 0)
            {
                gunFinishedFiringDelegate(this);
                Shoot = false;
                TimeSpentFiring = 0;
            }
        }
    }

    public void SlowTick()
    {
        AvailablePrevious = Available;

        FunctionalPrevious = Functional;
        PowerDrawPrevious = PowerDraw;


        Functional = actualGun.IsFunctional;
        PowerDraw = gunSinkComponent.CurrentInputByType(ElectricityId);
        Available = Functional && PowerDraw < 0.002f;

        EvaluateData(Available, AvailablePrevious, Shoot, ShootPrevious, Functional, FunctionalPrevious, PowerDraw,
            PowerDrawPrevious);
        ShootPrevious = Shoot;
        detailedInfoSB.Clear().Append(actualGun.DetailedInfo);


        var startIndex = actualGun.DetailedInfo.IndexOf("Stored power: ") + 14;
        if (startIndex + 6 > actualGun.DetailedInfo.Length)
        {
            chargePercent = 0;
            return;
        }

        chargePercentSB.Clear().AppendSubstring(detailedInfoSB, startIndex, 6).RemoveNonNumberChars();
        chargePercent = float.Parse(chargePercentSB.ToString()) / 50000f;
    }

    void EvaluateData(bool available, bool availablePrevious, bool shoot, bool shootPrevious,
        bool functional, bool functionalPrevious, double powerDraw, double previousPowerDraw)
    {
        if (shoot != shootPrevious)
        {
            if (shoot)
                FlightDataRecorder.WeaponBeginChargeEvent(nameShorthand, sigma, range);
            else
                FlightDataRecorder.WeaponCancelChargeEvent(nameShorthand, sigma, range);
        }

        if (functional != functionalPrevious)
        {
            if (functional)
                FlightDataRecorder.WeaponRepairedEvent(nameShorthand);
            else
                FlightDataRecorder.WeaponDamagedEvent(nameShorthand);
        }

        if (powerDraw != previousPowerDraw)
        {
            if (powerDraw < 0.002f)
            {
                if (previousPowerDraw > 0.002f && functional && functionalPrevious)
                    FlightDataRecorder.WeaponRechargedEvent(nameShorthand);
            }
            else
            {
                if (previousPowerDraw < 0.002f && functionalPrevious)
                    FlightDataRecorder.WeaponFireEvent(nameShorthand, sigma, range);
            }
        }
    }


    public Vector3D GetPosition()
    {
        return actualGun.GetPosition();
    }

    public float GetTimeToFire()
    {
        TimeToFire = FireDelay - TimeSpentFiring;
        return TimeToFire;
    }
}
public class Guns
{
    private const float IdlePowerDraw = 0.002f;
    Gun activeGun;

    private readonly Dictionary<Gun, bool> availableGuns;

    ushort currentGunIndex = 1;
    bool currentGunIsVolleyFiring;
    Gun currentlyFiringGun;
    int currentVolleyFrame;

    int gunIndex = -1;

    public List<float> gunRechargePercentages = new List<float>();

    public List<int> gunRechargeTimes = new List<int>();

    private readonly List<Gun> guns;
    private readonly List<IMyUserControllableGun> gunsReference;
    private readonly Dictionary<MyDefinitionId, float> knownFireDelays;
    private readonly MyGridProgram program;
    private readonly float secondDifferenceToGroupGunFiring;

    MatrixD ToWorldMatrix;
        
    public int volleyDelayFrames;

    public enum GunMode
    {
        FireWhenReady,
        Volley,
        WaitForAll
    }

    public GunMode _GunMode = GunMode.WaitForAll;

    public Guns(List<IMyUserControllableGun> guns, MyGridProgram program,
        Dictionary<MyDefinitionId, float> knownFireDelays, float framesToGroupGuns,GunMode gunMode,
        int volleyDelayFrames)

    {

        _GunMode = gunMode;
        secondDifferenceToGroupGunFiring = framesToGroupGuns / 60;
        availableGuns = new Dictionary<Gun, bool>();
        this.guns = new List<Gun>();
        gunsReference = new List<IMyUserControllableGun>();
        this.knownFireDelays = knownFireDelays;
        foreach (var gun in guns)
        {
            var isTurret = gun as IMyLargeTurretBase;
            if (isTurret == null)
            {
                this.guns.Add(new Gun(gun, knownFireDelays, GunFinishedFiring, program, currentGunIndex));
                currentGunIndex++;
                gunsReference.Add(gun);
            }
        }

        foreach (var gun in this.guns) availableGuns[gun] = gun.Available;
        this.volleyDelayFrames = volleyDelayFrames;

        this.program = program;

        switch (gunMode)
        {
            case GunMode.FireWhenReady:
                break;
            case GunMode.Volley:
                activeGun = this.guns[0];
                currentlyFiringGun = this.guns[0];
                break;
            case GunMode.WaitForAll:
                break;
        }
            

        ToWorldMatrix = program.Me.CubeGrid.WorldMatrix;
    }

    public void UpdateWeaponsList(List<IMyUserControllableGun> guns)
    {
        foreach (var gun in guns)
        {
            var isTurret = gun as IMyLargeTurretBase;
            if (isTurret == null && !gunsReference.Contains(gun))
            {
                gunsReference.Add(gun);
                this.guns.Add(new Gun(gun, knownFireDelays, GunFinishedFiring, program, currentGunIndex));
                currentGunIndex++;
            }
        }

        foreach (var gun in this.guns) availableGuns[gun] = gun.Available;
    }

    void GunFinishedFiring(Gun gun)
    {
        //if (gun != currentlyFiringGun) return;
        var newGun = GetLowestGunToFire();
        if (newGun != null) currentlyFiringGun = newGun;
    }

    public void Tick()
    {
            
            
        for (var i = guns.Count - 1; i >= 0; i--)
        {
            var gun = guns[i];
            if (gun == null || gun.Closed)
            {
                guns.RemoveAt(i);
                gunsReference.RemoveAt(i);
                //gunRechargeTimes.RemoveAt(i);
                gunRechargePercentages.RemoveAt(i);
                continue;
            }

            gun.Tick();
        }
    }
        
    public void SlowTick(double range, double sigma)
    {

        if (guns.Count == 0) return;
        gunIndex = (gunIndex + 1) % guns.Count;

        var gun = guns[gunIndex];
        if (gun == null || gun.Closed || !program.GridTerminalSystem.CanAccess(gun.actualGun))
        {
            guns.RemoveAt(gunIndex);
            gunsReference.RemoveAt(gunIndex);
            //gunRechargeTimes.RemoveAt(gunIndex);
            gunRechargePercentages.RemoveAt(gunIndex);
            return;
        }

        gun.SlowTick();
        gun.range = range;
        gun.sigma = sigma;
        while (gunRechargePercentages.Count < gunIndex + 1) gunRechargePercentages.Add(0);
        //gunRechargeTimes[gunIndex] = gun.RemainingChargeSeconds;
        gunRechargePercentages[gunIndex] = gun.chargePercent;
    }

    public int AreAvailable()
    {
        var availableGuns = 0;
        for (var i = guns.Count - 1; i >= 0; i--)
        {
            var gun = guns[i];
            var isGunAvailable = gun.Available;
            availableGuns += isGunAvailable ? 1 : 0;
            this.availableGuns[gun] = isGunAvailable;
        }

        if (_GunMode == GunMode.Volley && currentGunIsVolleyFiring) TickActiveGun();
        return availableGuns;
    }


    void TickActiveGun()
    {
        currentVolleyFrame++;
        if (currentVolleyFrame >= volleyDelayFrames)
        {
            currentVolleyFrame = 0;
            currentGunIsVolleyFiring = false;
            IncrementActiveGun();
        }
    }

    void IncrementActiveGun()
    {
        var currentIndex = guns.IndexOf(activeGun);
        activeGun = guns[(currentIndex + 1) % guns.Count];
    }

    public Vector3D GetAimingReferencePos(Vector3D fallback)
    {
        if (_GunMode == GunMode.Volley)
        {
            if (currentlyFiringGun == null) return fallback;
            if (availableGuns[currentlyFiringGun])
                return currentlyFiringGun.GetPosition();
            return fallback;
        }

        var lowestTimeToFire = GetLowestTimeToFire(); // MUST be called to update the time to fire for each gun
        var averagePos = Vector3D.Zero;
        var activeGunCount = 0;
        var chargingGunCount = 0;
        for (var i = 0; i < guns.Count; i++)
        {
            var gun = guns[i];
            if (availableGuns[gun])
            {
                if (gun.TimeToFire - secondDifferenceToGroupGunFiring > lowestTimeToFire)
                {
                    chargingGunCount++;
                    continue;
                }

                ;
                var GunPos = gun.GetPosition();
                averagePos += GunPos;
                activeGunCount++;
            }
        }


        if (activeGunCount == 0) return fallback;
        averagePos /= activeGunCount;
        //averagePos = Vector3D.Transform(averagePos, ToWorldMatrix);

        return averagePos;
    }

    public float GetLowestTimeToFire()
    {
        var lowestTimeToFire = float.MaxValue;
        for (var i = 0; i < guns.Count; i++)
        {
            var gun = guns[i];
            if (gun.Available && gun.Shoot)
            {
                var timeToFire = gun.GetTimeToFire();
                lowestTimeToFire = Math.Min(lowestTimeToFire, timeToFire);
            }
        }

        return lowestTimeToFire;
    }

    Gun GetLowestGunToFire() //returns the gun with the lowest time to fire
    {
        Gun lowestGun = null;
        var lowestTimeToFire = float.MaxValue;
        for (var i = 0; i < guns.Count; i++)
        {
            var gun = guns[i];
            if (gun.Available && gun.Shoot)
            {
                var timeToFire = gun.GetTimeToFire();
                if (timeToFire < lowestTimeToFire)
                {
                    lowestTimeToFire = timeToFire;
                    lowestGun = gun;
                }
            }
        }

        return lowestGun;
    }

    public void Fire()
    {

        switch (_GunMode)
        {
            case GunMode.FireWhenReady:
                for (var i = 0; i < guns.Count; i++)
                {
                    var gun = guns[i];
                    if (availableGuns[gun])
                    {
                        gun.Enabled = true;
                        gun.Shoot = true;
                    }
                }
                break;
            case GunMode.Volley:
                if (availableGuns[activeGun])
                {
                    activeGun.Enabled = true;
                    activeGun.Shoot = true;
                    currentGunIsVolleyFiring = true;
                }
                break;
            case GunMode.WaitForAll:
                var allReady = true;
                    
                for (var i = 0; i < guns.Count; i++)
                {
                    var gun = guns[i];
                    if (gun.PowerDraw > 0.002f) allReady = false;
                }

                if (allReady)
                {
                    for (var i = 0; i < guns.Count; i++)
                    {
                        var gun = guns[i];
                        if (availableGuns[gun])
                        {
                            gun.Enabled = true;
                            gun.Shoot = true;
                        }
                    }
                }
                break;
        }
    }

    public void Cancel()
    {
        for (var i = 0; i < guns.Count; i++)
        {
            var gun = guns[i];
            if (availableGuns[gun])
            {
                gun.Shoot = false;
                gun.Enabled = false;
            }
            else
            {
                gun.Shoot = false;
                gun.Enabled = true; //ensure that uncharged guns will still accumulate power
            }
        }
    }

    public void Standby()
    {
        foreach (var gun in guns)
        {
            gun.Shoot = false;
            gun.Enabled = true;
        }
    }
}
public struct Half
{
    private readonly ushort value;

        
    public Half(float floatValue)
    {
        value = FloatToHalf(floatValue);
    }

    public static explicit operator Half(float floatValue)
    {
        return new Half(floatValue);
    }

    public static explicit operator Half(double doubleValue)
    {
        return new Half((float)doubleValue);
    }

    public static explicit operator char(Half halfValue)
    {
        return (char)halfValue.value;
    }

    private static ushort FloatToHalf(float floatValue)
    {
        // Simple conversion (for illustration purposes)
        var floatBits = BitConverter.ToInt32(BitConverter.GetBytes(floatValue), 0);
        var sign = (floatBits >> 16) & 0x8000;
        var exponent = ((floatBits >> 23) & 0xFF) - 127 + 15;
        var mantissa = floatBits & 0x007FFFFF;

        if (exponent <= 0)
        {
            mantissa = (mantissa | 0x00800000) >> (1 - exponent);
            return (ushort)(sign | (mantissa >> 13));
        }

        if (exponent == 0xFF - (127 - 15))
            return (ushort)(sign | 0x7C00); // Infinity or NaN
        if (exponent > 30)
            return (ushort)(sign | 0x7C00); // Overflow to Infinity
        return (ushort)(sign | (exponent << 10) | (mantissa >> 13));
    }
}
public class KMeans
{
    public static MyGridProgram program;
    private readonly List<Vector3D> centroids;

    private readonly List<Vector3D> dataPoints;
    private readonly int k;
    private readonly int maxIterations;

    public KMeans(List<Vector3D> dataPoints, int k, int maxIterations)
    {
        this.dataPoints = dataPoints;
        this.k = k;
        this.maxIterations = maxIterations;
        centroids = new List<Vector3D>();
    }

    public double ComputeWCSS(List<Vector3D>[] clusters)
    {
        double wcss = 0;
        for (var i = 0; i < k; i++)
            foreach (var point in clusters[i])
                wcss += Math.Pow(Vector3D.Distance(point, centroids[i]), 2);
        return wcss;
    }

    public List<Vector3D>[] Cluster()
    {
        InitializeCentroids();
        var clusters = new List<Vector3D>[k];
        for (var i = 0; i < k; i++) clusters[i] = new List<Vector3D>();

        for (var iter = 0; iter < maxIterations; iter++)
        {
            // Assign each hudSurfaceDataList point to the nearest centroid
            for (var i = 0; i < dataPoints.Count; i++)
            {
                var nearestCentroidIndex = FindNearestCentroidIndex(dataPoints[i]);
                clusters[nearestCentroidIndex].Add(dataPoints[i]);
            }

            // Update centroids
            for (var i = 0; i < k; i++)
            {
                if (clusters[i].Count == 0)
                    continue;
                centroids[i] = ComputeCentroid(clusters[i]);
            }

            // Clear clusters
            foreach (var cluster in clusters) cluster.Clear();
        }

        return clusters;
    }

    void InitializeCentroids()
    {
        var rand = new Random();
        var chosenIndices = new HashSet<int>();

        while (centroids.Count < k)
        {
            var index = rand.Next(dataPoints.Count);

            if (!chosenIndices.Contains(index))
            {
                chosenIndices.Add(index);
                centroids.Add(dataPoints[index]);
            }
        }
    }

    int FindNearestCentroidIndex(Vector3D point)
    {
        var nearestIndex = 0;
        var minDistance = double.MaxValue;

        for (var i = 0; i < k; i++)
        {
            var distance = Vector3D.Distance(point, centroids[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    Vector3D ComputeCentroid(List<Vector3D> cluster)
    {
        if (cluster.Count == 0)
            return Vector3D.Zero;

        var sum = Vector3D.Zero;
        foreach (var point in cluster) sum += point;

        return sum / cluster.Count;
    }
}
public enum MissileState
{
    StandingByForLaunch,
    MaintainLaunchTrajectory,
    InterceptWaypoint,
    InterceptTarget,
    CancelVelocity,
    HoldPosition,
}
public enum MissilePropulsionType
{
    Ion,
    Hydrogen,
    Atmospheric
}
internal class KratosMissile
{
    public MissileState State = MissileState.StandingByForLaunch;
        
    private readonly double _accel;
        
    double _serverMaxSpeed = 150; // BAD
    int _maxDistanceFromLoiterPosition = 50;
    int _frame;
    int _minFramesAliveToDetonate = 60;
    double _hitSurroundPointDist = 7;
    double _fuelRatio = 0.5;
    double _minFuelRatio = 0.1;
        
        
        
        
        
       
    public readonly bool ConstructedSuccessfully;
        
    int _currentFrameMaintainingTrajectory;
    int _detIndex;
    Vector3D _distanceToTarget;
    Vector3D _position;
    Vector3D _previousPosition;
    Vector3D _velocity;
    Vector3D _previousVelocity;
    Vector3D _previousWaypointPosition;

    public readonly IMyGyro Gyro;
        
    
    public readonly IMyGasTank[] GasTanks;
        
    IMyTerminalBlock[] _blocks;
    private readonly IMyBatteryBlock[] _batteries;
    private readonly IMyShipConnector[] _connectors;
    private readonly IMyThrust[] _forwardThrusters;
    private readonly IMyWarhead[] _kaboom;
    private readonly IMyShipMergeBlock[] _mergers; 
    IMyCubeGrid _missileGrid;

    public bool Functional;
        


    private readonly int _framesToMaintainTrajectory;
        
        

        
        


    double _gyroPreviousPitch;
    double _gyroPreviousRoll;
    double _gyroPreviousYaw;
    public bool HasLaunched;
        

    private readonly double _maxAngularVelocityRpm = 60;
        

        
    public readonly MissilePropulsionType MissilePropulsionType = MissilePropulsionType.Ion;
    public readonly string Name = "";
    private readonly PID _pitch;


    // Navigation constant, typically between 3 and 5
    private readonly double _pnGain = 5.0;
        

    double _previousPitchSpeed;
    double _previousYawSpeed;

    private readonly IMyThrust _referenceThruster;
        
    public Vector3D SurroundDir = Vector3D.Zero;
    public double SurroundDist = 0;
        
    // Definitions for the target and host ships
        
    public ArgusShip Host; 
    public ArgusTargetableShip Target;

    // TODO: Regions
    public TargetableBlockCategory _targetedBlockCategory = TargetableBlockCategory.Default;
    public Dictionary<ArgusTargetableShip, Dictionary<TargetableBlockCategory, int>> TargetedCategoryBlockIndices = new Dictionary<ArgusTargetableShip, Dictionary<TargetableBlockCategory, int>>();
       
        

    private readonly double _timeStep = 1 / 60.0;


    private readonly List<Vector3D> _waypoints = new List<Vector3D>();
    private readonly PID _yaw;

    public KratosMissile(ArgusShip host, List<IMyTerminalBlock> blocks, KratosMissileBehavior behavior, double pnGain, string name, bool hasDetachmentGroup)
    {
        this.Name = name;
        Host = host;
        this._blocks = blocks.ToArray();
        if (blocks.Count == 0) return;
        var forwardThrustersTempList = new List<IMyThrust>();
        var batteriesTempList = new List<IMyBatteryBlock>();
        var gasTanksTempList = new List<IMyGasTank>();
        var kaboomTempList = new List<IMyWarhead>();
        var mergersTempList = new List<IMyShipMergeBlock>();
        var connectorsTempList = new List<IMyShipConnector>();

        double mass = 0;
        double force = 0;
        foreach (var block in blocks)
        {
            mass += block.Mass;
            if (block is IMyThrust)
            {
                forwardThrustersTempList.Add(block as IMyThrust);
                // Check the thruster type

                switch (block.BlockDefinition.SubtypeName)
                {
                    case "SmallBlockSmallAtmosphericThrust":
                        MissilePropulsionType = MissilePropulsionType.Atmospheric;
                        break;
                    case "SmallBlockSmallHydrogenThrust":
                        MissilePropulsionType = MissilePropulsionType.Hydrogen;
                        break;
                }

                force += (block as IMyThrust).MaxEffectiveThrust;
            }
            else if (block is IMyBatteryBlock)
            {
                batteriesTempList.Add(block as IMyBatteryBlock);
            }
            else if (block is IMyGyro)
            {
                Gyro = block as IMyGyro;
                if (Gyro.CustomData == "KMM") return;
                 _missileGrid = Gyro.CubeGrid;
            }
            else if (block is IMyGasTank)
            {
                var gasTank = block as IMyGasTank;
                if (gasTank.FilledRatio > _fuelRatio) gasTank.Stockpile = false;
                else gasTank.Stockpile = true;
                gasTanksTempList.Add(gasTank);
            }
            else if (block is IMyWarhead)
            {
                kaboomTempList.Add(block as IMyWarhead);
            }
            else if (block is IMyShipMergeBlock)
            {
                mergersTempList.Add(block as IMyShipMergeBlock);
            }
            else if (block is IMyShipConnector)
            {
                var connector = block as IMyShipConnector;
                connector.Connect();
                connectorsTempList.Add(connector);
            }
        }

        _forwardThrusters = forwardThrustersTempList.ToArray();
        _batteries = batteriesTempList.ToArray();
        GasTanks = gasTanksTempList.ToArray();
        _kaboom = kaboomTempList.ToArray();
        _mergers = mergersTempList.ToArray();
        _connectors = connectorsTempList.ToArray();

        _pitch = new PID(behavior.KP, behavior.KI, behavior.KD, Program.TimeStep);
        _yaw = new PID(behavior.KP, behavior.KI, behavior.KD, Program.TimeStep);

        _accel = force / mass;


        foreach (var tank in GasTanks)
        {
            if (tank.FilledRatio > _fuelRatio)
            {
                tank.Stockpile = false;
            }
        }
            
        ConstructedSuccessfully =
            (MissilePropulsionType == MissilePropulsionType.Ion && _forwardThrusters.Length > 0 && _batteries.Length > 0 &&
             _kaboom.Length > 0 && _mergers.Length > 0 && Gyro != null)
            || (MissilePropulsionType == MissilePropulsionType.Hydrogen && _forwardThrusters.Length > 0 && _batteries.Length > 0 &&
                GasTanks.Length > 0 && _kaboom.Length > 0 && (_mergers.Length > 0 || hasDetachmentGroup || _connectors.Length > 0) &&
                Gyro != null)
            || (MissilePropulsionType == MissilePropulsionType.Atmospheric && _forwardThrusters.Length > 0 && _batteries.Length > 0 &&
                _kaboom.Length > 0 && _mergers.Length > 0 && Gyro != null);

            

        if (!ConstructedSuccessfully) return;
        _referenceThruster = _forwardThrusters[0];
        this._pnGain = pnGain;
        this._framesToMaintainTrajectory = behavior.MaintainTrajectorAfterLaunchFrames;

        //foreach (var gyro in gyroTempList)
        //{
        //    gyro.CustomData = "KMM";
        //}
    }



    public void Refuel()
    {
        foreach (var tank in GasTanks)
        {
            if (tank.FilledRatio > _fuelRatio)
            {
                tank.Stockpile = false;
            }
        }
        foreach (var connector in this._connectors)
            if (!connector.IsConnected)
                connector.Connect();
    }

    public void UpdateMotion(Vector3D targetPosition)
    {
        // _missileGrid = Gyro.CubeGrid;
        // var boundingSphere = _missileGrid.WorldVolume;
        _previousPosition = _position;
        // _position = boundingSphere.Center;
        _previousVelocity = _velocity;
        _velocity = (_position - _previousPosition) / Program.TimeStep;
        _distanceToTarget = targetPosition - _position;
    }
    public void Update(Vector3D gravity, Vector3D gravityNormal)
    {
        ALDebug.Echo("Missile updating");

            
        _frame++;
            
            

            
        var forwardThrusters = this._forwardThrusters;
        var batteries = this._batteries;
        var gasTanks = this.GasTanks;
        var kaboom = this._kaboom;
        var mergers = this._mergers;
        var connectors = this._connectors;
        Functional = CheckBlocks(forwardThrusters, batteries, gasTanks, kaboom, mergers, connectors, Gyro);


        var missileToTargetDistanceSquared = _distanceToTarget.ALengthSquared();

        ALDebug.Echo(State);
        switch (State)
        {
            case MissileState.StandingByForLaunch:
                break;
            case MissileState.MaintainLaunchTrajectory:
                if (_currentFrameMaintainingTrajectory < _framesToMaintainTrajectory)
                {
                    Gyro.GyroOverride = true;
                    _currentFrameMaintainingTrajectory++;
                    if (Gyro.Pitch != 0 || Gyro.Yaw != 0 || Gyro.Roll != 0)
                    {
                        Gyro.Pitch = 0;
                        Gyro.Yaw = 0;
                        Gyro.Roll = 0;
                    }

                    foreach (var thruster in forwardThrusters)
                    {
                        thruster.Enabled = true;
                        thruster.ThrustOverride = 1000000;
                    }
                }
                else
                {
                    State = MissileState.InterceptWaypoint;
                }

                break;
            case MissileState.InterceptWaypoint:

                STD_GUIDANCE(_waypoints[0],(_waypoints[0] - _previousWaypointPosition) / Program.TimeStep,Vector3D.Zero);
                _previousWaypointPosition = _waypoints[0];
                TryTargetNextWaypoint(missileToTargetDistanceSquared);
                break;
            case MissileState.InterceptTarget:
                    
                STD_GUIDANCE(Target.GetBlockPositionOfCategoryAtIndex(_targetedBlockCategory,
                    TargetedCategoryBlockIndices[Target][_targetedBlockCategory]), Target.Velocity, Target.Acceleration);

                TryDetonate(missileToTargetDistanceSquared);
                break;
            case MissileState.CancelVelocity:
                CancelVelocity(gravity);

                break;
            case MissileState.HoldPosition:
                HoldPosition(gravity, gravityNormal);
                break;
            // case MissileState.LoiterAroundTarget:
            //     InterceptSurroundPos(Target);
            //     break;
            // case MissileState.LoiterAroundHost:
            //     InterceptSurroundPos(Host);
            //     break;
        }
    }

        

    public void ForceDetonate()
    {
        if (!HasLaunched || _frame < _minFramesAliveToDetonate)
        {
            var kaboom2 = this._kaboom;
            for (var i = 0; i < kaboom2.Length; i++)
            {
                var item = kaboom2[i];
                item.IsArmed = false;
            }
            return;
        }
        var kaboom = this._kaboom;
        for (var i = 0; i < kaboom.Length; i++)
        {
            var item = kaboom[i];
            item.IsArmed = true;
            item.Detonate();
        }
    }

    // TODO: Use me
    bool CheckBlocks(IMyTerminalBlock[] blocks)
    {
        var length = blocks.Length;
        for (var i = 0; i < length; i++)
        {
            var block = blocks[i];
            if (block.Closed)
            {
                ForceDetonate();
                return false;
            }
        }

        return true;
    }

    bool CheckBlocks(IMyThrust[] forwardThrusters, IMyBatteryBlock[] batteries, IMyGasTank[] gasTanks,
        IMyWarhead[] kaboom, IMyShipMergeBlock[] mergers, IMyShipConnector[] connectors, IMyGyro gyro)
    {
        var forwardThrusters2 = forwardThrusters; // hehe cache misses be like
        var length = forwardThrusters2.Length;
        for (var i = 0; i < length; i++)
            if (forwardThrusters2[i].Closed)
            {
                ForceDetonate();
                return false;
            }

        var batteries2 = batteries;
        length = batteries2.Length;
        for (var i = 0; i < length; i++)
            if (batteries2[i].Closed)
            {
                ForceDetonate();
                return false;
            }

        var gasTanks2 = gasTanks;
        length = gasTanks2.Length;
        for (var i = 0; i < length; i++)
            if (gasTanks2[i].Closed || gasTanks2[i].FilledRatio == 0)
            {
                ForceDetonate();
                return false;
            }

        var kaboom2 = kaboom;
        length = kaboom2.Length;
        for (var i = 0; i < length; i++)
            if (kaboom2[i].Closed)
            {
                ForceDetonate();
                return false;
            }

        var mergers2 = mergers;
        length = mergers2.Length;
        for (var i = 0; i < length; i++)
            if (mergers2[i].Closed)
            {
                ForceDetonate();
                return false;
            }

        var connectors2 = connectors;
        length = connectors2.Length;
        for (var i = 0; i < length; i++)
            if (connectors2[i].Closed)
            {
                ForceDetonate();
                return false;
            }

        var gyro2 = gyro;
        if (!gyro2.IsFunctional || gyro2.Closed)
        {
            ForceDetonate();
            return false;
        }

        return true;
    }

    void InterceptSurroundPos(ArgusShip ship)
    {
        Vector3D target = ship.Position + SurroundDir * SurroundDist;
        STD_GUIDANCE(target, ship.Velocity, ship.Acceleration);
        var missileToTargetDistanceSquared = (target - _position).ALengthSquared();
        if (missileToTargetDistanceSquared < _hitSurroundPointDist * _hitSurroundPointDist) State = MissileState.CancelVelocity;
    }

    void HoldPosition(Vector3D gravity, Vector3D gravityNormal)
    {
            
        Disarm();
        if (gravity == Vector3D.Zero)
        {
            DisableThrusters();
        }
        else
        {
            Gyro.GyroOverride = true;
            Rotate(-gravityNormal, _maxAngularVelocityRpm, false);
        }
            
            
    }

    void DisableThrusters()
    {
        foreach (var thruster in _forwardThrusters) thruster.ThrustOverride = 0;
        Gyro.GyroOverride = false;
    }

    public int Launch()
    {
        if (MissilePropulsionType == MissilePropulsionType.Hydrogen)
        {

            bool fuelFail = false;
            foreach (var tank in GasTanks)
            {
                if (tank.FilledRatio < _minFuelRatio)
                {
                    tank.Stockpile = true;
                    fuelFail = true;
                }
                    
            }
            if (fuelFail) return 1;
        }
            
        HasLaunched = true;
        foreach (var thruster in _forwardThrusters)
        {
            thruster.Enabled = true;
            thruster.ThrustOverridePercentage = 1;
        }

        foreach (var battery in _batteries) battery.Enabled = true;
        foreach (var merger in _mergers) merger.Enabled = false;
        foreach (var connector in _connectors)
        {
            connector.Disconnect();
            connector.Enabled = false;
        }

        foreach (var tank in GasTanks) tank.Stockpile = false;
        State = MissileState.MaintainLaunchTrajectory;
        return 0;
    }

    Vector3D _savedTargetVelocity = Vector3D.Zero; // TODO: Assign me

    public void SetTargetVelocity(Vector3D targetVelocity)
    {
        //if (guidanceMode == MissileGuidanceMode.CancelVelocity) return;
        _savedTargetVelocity = targetVelocity;
    }

    public void AddWaypoint(Vector3D waypoint)
    {
        _waypoints.Add(waypoint);
    }

    void CancelVelocity(Vector3D gravity)
    {
            

        var missileVelocityNormalized = Vector3D.Normalize(_velocity);


        Rotate(-missileVelocityNormalized, _maxAngularVelocityRpm, true);

        double thrustDot = Math.Pow(missileVelocityNormalized.Dot(_forwardThrusters[0].WorldMatrix.Forward), _serverMaxSpeed);
            
        var thrust = thrustDot == 0 ? 0 : (float)(thrustDot * 10 * _accel);
        /*
        var thrust =
            MathHelper.Clamp(Math.Pow(MissileVelocityNormalized.Dot(forwardThrusters[0].WorldMatrix.Forward), _serverMaxSpeed),
                0, 1) * MissileVelocity.ALength() / 10;
                */

        var cross = _velocity.Cross(_forwardThrusters[0].WorldMatrix.Forward);
        foreach (var thruster in _forwardThrusters) thruster.ThrustOverridePercentage = thrust;


        if (_velocity.ALengthSquared() < 0.5 * 0.5)
        {
            DisableThrusters();
            State = MissileState.HoldPosition;
        }
    }


    void STD_GUIDANCE(Vector3D targetPosition, Vector3D targetVelocity, Vector3D targetAcceleration)
    {
        // Sorts CurrentVelocities

        // Uses RdavNav Navigation APN Guidance System
        //-----------------------------------------------

        // Setup LOS rates and PN system
        var losOld = Vector3D.Normalize((targetPosition - targetVelocity) - _previousPosition);
        var losNew = Vector3D.Normalize(targetPosition - _position);

        // And Assigners
        var am = new Vector3D(1, 0, 0);
        double losRate;
        Vector3D losDelta;
        var missileForwards = _forwardThrusters[0].WorldMatrix.Backward;

        // Vector/Rotation Rates
        if (losOld.ALengthSquared() == 0)
        {
            losDelta = new Vector3D(0, 0, 0);
            losRate = 0.0;
        }
        else
        {
            losDelta = losNew - losOld;
            losRate = Math.Sqrt(losDelta.ALengthSquared()) / _timeStep;
        }

        //-----------------------------------------------

        // Closing Velocity
        var vclosing = Math.Sqrt((targetVelocity - _velocity).ALengthSquared());
        var v = _velocity.ALength();
        // If Under Gravity Use Gravitational Accel
        var gravityComp = -Host.Controller.GetNaturalGravity();
        // Calculate the final lateral acceleration
        var lateralDirection = Vector3D.Cross(Vector3D.Cross(targetVelocity - _velocity, losNew),
            targetVelocity - _velocity);
        var lateralDirectionLengthSquared = lateralDirection.ALengthSquared();
        if (lateralDirectionLengthSquared != 0) lateralDirection /= Math.Sqrt(lateralDirectionLengthSquared);

        var lateralAccelerationComponent =
            lateralDirection * _pnGain * losRate * vclosing + losDelta * 9.8 * (0.5 * _pnGain);

        // If Impossible Solution (ie maxes turn rate) Use Drift Cancelling For Minimum T
        var oversteerReqt = Math.Sqrt(lateralAccelerationComponent.ALengthSquared()) / _accel;
        if (oversteerReqt > 0.98)
            lateralAccelerationComponent = _accel * Vector3D.Normalize(lateralAccelerationComponent +
                                                                      oversteerReqt *
                                                                      Vector3D.Normalize(-_velocity) * 40);

        // Calculates And Applies thrust In Correct Direction (Performs own inequality check)
        var thrustPower = Math.Pow(Vector3D.Dot(missileForwards, Vector3D.Normalize(lateralAccelerationComponent)),
            4); // TESTTESTTEST
        thrustPower =
            MathHelper.Clamp(thrustPower, MathHelper.Clamp(_serverMaxSpeed - v, 0, 1),
                1); // for improved thrust performance on the get-go
        thrustPower = MathHelper.RoundToInt(thrustPower * 10) / 10d;

        for (var i = 0; i < _forwardThrusters.Length; i++)
        {
            var thruster = _forwardThrusters[i];
            if (thruster.ThrustOverridePercentage != (float)thrustPower)
                thruster.ThrustOverridePercentage = (float)thrustPower;
        }

        // Calculates Remaining Force Component And Adds Along LOS
        var lateralAccelerationComponentLengthSquared = lateralAccelerationComponent.ALengthSquared();
        var rejectedAccel = Math.Sqrt(_accel * _accel - lateralAccelerationComponentLengthSquared);
        if (double.IsNaN(rejectedAccel)) rejectedAccel = 0;
        lateralAccelerationComponent += losNew * rejectedAccel;

        //-----------------------------------------------

        // Guides To Target Using Gyros
        am = Vector3D.Normalize(lateralAccelerationComponent + gravityComp);
            
        Rotate(am, _maxAngularVelocityRpm, false);
    }

    void TryDetonate(double missileToTargetDistanceSquared)
    {
        if (_frame < _minFramesAliveToDetonate) return;
        if (missileToTargetDistanceSquared < 50 * 50) //Arms
        {
            foreach (var item in _kaboom) item.IsArmed = true;
            if (_kaboom.Length <= _detIndex) return;
            if (missileToTargetDistanceSquared < 15 * 15) //A mighty earth shattering kaboom
            {
                _kaboom[_detIndex].Detonate();
                _detIndex++;
            }
        }
    }

    void TryTargetNextWaypoint(double missileToTargetDistanceSquared)
    {
        if (missileToTargetDistanceSquared < 10 * 10)
        {
            if (_waypoints.Count == 0) return;
            _waypoints.RemoveAt(0);
        }
    }

    void Disarm()
    {
        for (var i = 0; i < _kaboom.Length; i++)
        {
            var item = _kaboom[i];
            item.IsArmed = false;
        }
    }

    void Rotate(Vector3D desiredGlobalFwdNormalized, double maxAngularVelocityRpm, bool precision)
    {
        // Check if desiredGlobalFwdNormalized is nan
        if (double.IsNaN(desiredGlobalFwdNormalized.X) || double.IsNaN(desiredGlobalFwdNormalized.Y) ||
            double.IsNaN(desiredGlobalFwdNormalized.Z)) return;

        double gp;
        double gy;

        //Rotate Toward forward
        var referenceThruster = this._referenceThruster;
        var referenceThrusterWorldMatrix = referenceThruster.WorldMatrix;

        var waxis = Vector3D.Cross(referenceThrusterWorldMatrix.Backward, desiredGlobalFwdNormalized);
        var axis = Vector3D.TransformNormal(waxis, MatrixD.Transpose(referenceThrusterWorldMatrix));
        var x = _pitch.Control(-axis.X);
        var y = _yaw.Control(-axis.Y);
            
        gp = MathHelper.Clamp(x, -maxAngularVelocityRpm, maxAngularVelocityRpm);
        gy = MathHelper.Clamp(y, -maxAngularVelocityRpm, maxAngularVelocityRpm);

        if (Math.Abs(gy) + Math.Abs(gp) > maxAngularVelocityRpm)
        {
            var adjust = maxAngularVelocityRpm / (Math.Abs(gy) + Math.Abs(gp));
            gy *= adjust;
            gp *= adjust;
        }


        ApplyGyroOverride(gp, gy, Gyro, referenceThruster.WorldMatrix, precision);
    }

    void ApplyGyroOverride(double pitchSpeed, double yawSpeed, IMyGyro gyro, MatrixD worldMatrix, bool precision)
    {
        if (!precision)
        {
            pitchSpeed = MathHelper.RoundToInt(pitchSpeed * 10) / 10d;
            yawSpeed = MathHelper.RoundToInt(yawSpeed * 10) / 10d;
        }
            

        if (pitchSpeed == _previousPitchSpeed && yawSpeed == _previousYawSpeed) return;
        _previousPitchSpeed = pitchSpeed;
        _previousYawSpeed = yawSpeed;

        var rotationVec = new Vector3D(pitchSpeed, yawSpeed, 0);
        var relativeRotationVec = Vector3D.TransformNormal(rotationVec, worldMatrix);


        if (gyro.IsFunctional && gyro.IsWorking && gyro.Enabled && !gyro.Closed)
        {
            var transformedRotationVec =
                Vector3D.TransformNormal(relativeRotationVec, MatrixD.Transpose(gyro.WorldMatrix));
            if (precision)
            {
                gyro.Pitch = (float)transformedRotationVec.X;
                _gyroPreviousPitch = transformedRotationVec.X;
                gyro.Yaw = (float)transformedRotationVec.Y;
                _gyroPreviousYaw = transformedRotationVec.Y;
                gyro.Roll = (float)transformedRotationVec.Z;
                _gyroPreviousRoll = transformedRotationVec.Z;
                return;
            }
                
            if (Math.Abs(transformedRotationVec.X - _gyroPreviousPitch) > 0.05)
            {
                gyro.Pitch = (float)transformedRotationVec.X;
                _gyroPreviousPitch = transformedRotationVec.X;
                //ALDebug.program.Echo("Pitching");
            }

            if (Math.Abs(transformedRotationVec.Y - _gyroPreviousYaw) > 0.05)
            {
                gyro.Yaw = (float)transformedRotationVec.Y;
                _gyroPreviousYaw = transformedRotationVec.Y;
                //ALDebug.program.Echo("Yawing");
            }

            if (Math.Abs(transformedRotationVec.Z - _gyroPreviousRoll) > 0.05)
            {
                gyro.Roll = (float)transformedRotationVec.Z;
                _gyroPreviousRoll = transformedRotationVec.Z;
                //ALDebug.program.Echo("Rolling");
            }

            gyro.GyroOverride = true;
        }
    }



    public Vector3D GetPosition()
    {
        return Gyro.CubeGrid.WorldVolume.Center;
    }

    public Vector3D GetForward()
    {
        return _forwardThrusters[0].WorldMatrix.Backward;
    }

    public KratosMissileManager.MissileData GetData()
    {
        // *10 because LCD updates at 6hz as opposed to 60hz of literally everything else
        switch (MissilePropulsionType)
        {
            case MissilePropulsionType.Ion:
                return new KratosMissileManager.MissileData(_position, _previousPosition, _distanceToTarget,
                    _forwardThrusters[0].WorldMatrix.Backward,
                    _batteries[0].CurrentStoredPower / _batteries[0].MaxStoredPower, Gyro.EntityId, HasLaunched,
                    false);
            case MissilePropulsionType.Hydrogen:
                return new KratosMissileManager.MissileData(_position, _previousPosition, _distanceToTarget,
                    _forwardThrusters[0].WorldMatrix.Backward, Math.Min(1, GasTanks[0].FilledRatio / _fuelRatio), Gyro.EntityId, HasLaunched,
                    true);
            case MissilePropulsionType.Atmospheric:
                return new KratosMissileManager.MissileData(_position, _previousPosition, _distanceToTarget,
                    _forwardThrusters[0].WorldMatrix.Backward,
                    _batteries[0].CurrentStoredPower / _batteries[0].MaxStoredPower, Gyro.EntityId, HasLaunched,
                    false);
        }

        return new KratosMissileManager.MissileData(_position, _previousPosition, _distanceToTarget,
            _forwardThrusters[0].WorldMatrix.Backward, _batteries[0].CurrentStoredPower / _batteries[0].MaxStoredPower,
            Gyro.EntityId, HasLaunched, false);
    }

    public Vector3D GetGyroPosition()
    {
        return Gyro.GetPosition();
    }

    public Vector3D GetThrusterPosition()
    {
        return _forwardThrusters[0].GetPosition();
    }

    public bool IsComplete()
    {
        for (int i = 0; i < _blocks.Length; i++)
        {
            var block = _blocks[i];
            if (!block.IsFunctional) return false;
        }

        if (!Gyro.IsFunctional) return false;
        return true;
    }

    public void LoiterAroundHost()
    {
        Vector3D targetPosition = Host.Position + SurroundDir * SurroundDist;
        UpdateMotion(targetPosition);
        if 
        (
            State == MissileState.InterceptTarget 
            //|| 
            //State == MissileState.LoiterAroundTarget
            || 
            (
                (
                    State == MissileState.HoldPosition 
                    || 
                    State == MissileState.CancelVelocity
                )
                && 
                (targetPosition - _position).LengthSquared() > _maxDistanceFromLoiterPosition * _maxDistanceFromLoiterPosition
            )
            || 
            (
                State == MissileState.InterceptWaypoint 
                && _waypoints.Count == 0
            )
        )
        {
            //State = MissileState.LoiterAroundHost;
        }

    }
    public void LoiterAroundTarget()
    {
        Vector3D targetPosition = Target.Position + SurroundDir * SurroundDist;
        UpdateMotion(targetPosition);
        if 
        (
            State == MissileState.InterceptTarget 
            || 
            //State == MissileState.LoiterAroundHost
            //|| 
            (
                (
                    State == MissileState.HoldPosition 
                    || 
                    State == MissileState.CancelVelocity
                )
                &&
                (targetPosition - _position).LengthSquared() > _maxDistanceFromLoiterPosition * _maxDistanceFromLoiterPosition
            )
            || 
            (
                State == MissileState.InterceptWaypoint 
                && _waypoints.Count == 0
            )
        )
        {
            //State = MissileState.LoiterAroundTarget;
        }
    }

    public void InterceptTarget()
    {
        Vector3D target = Target.GetBlockPositionOfCategoryAtIndex(_targetedBlockCategory,
            TargetedCategoryBlockIndices[Target][_targetedBlockCategory]);
        UpdateMotion(target);
        if 
        (
            State != MissileState.InterceptTarget 
            && 
            State != MissileState.StandingByForLaunch 
            &&
            State != MissileState.MaintainLaunchTrajectory
        )
        {
            State = MissileState.InterceptTarget;
        } 
    }


}
public enum AttackPattern
{
    DirectAttack,
    SurroundAndWaitForAttack,
    LoiterAndPursue
}
public enum LaunchType
{
    Single,
    Alpha,
    Staggered
}
public class KratosMissileBehavior
{
    public static Dictionary<KratosMissileBehaviorType, KratosMissileBehavior> Behaviors =
        new Dictionary<KratosMissileBehaviorType, KratosMissileBehavior>
        {
            { KratosMissileBehaviorType.FighterMissileBehavior, new FighterMissileBehavior() },
            { KratosMissileBehaviorType.SurroundMissileBehavior, new SurroundMissileBehavior() },
            { KratosMissileBehaviorType.InterdictMissileBehavior, new InterdictMissileBehavior() },
        };
    public double MinSurroundDistance;
    public double MaxSurroundDistance;
    public double MaxAttackDistance;
    public double MinAngleGravity;
    public double MaxAngleGravity;
    public double MissileSafetyDistance = 50;
    public double MissileDivergeDistance = 300;
    public double MaxLaunchSpeed = 140;
    public double KP = 12;
    public double KI = 0;
    public double KD = 0;
    public double MinGuidanceFactor = 5;
    public double MaxGuidanceFactor = 5;
    public int MissileLaunchDelayFrames = 5;
    public int MaintainTrajectorAfterLaunchFrames = 60;
        
    public AttackPattern AttackPattern;
    public LaunchType LaunchType;
}
public enum KratosMissileBehaviorType
{
    FighterMissileBehavior,
    SurroundMissileBehavior,
    InterdictMissileBehavior
}
public class FighterMissileBehavior : KratosMissileBehavior
{
    public FighterMissileBehavior()
    {
        MinSurroundDistance = 500;
        MaxSurroundDistance = 500;
        MaxAttackDistance = 0;
        MinAngleGravity = 0;
        MaxAngleGravity = 180;
        AttackPattern = AttackPattern.DirectAttack;
        LaunchType = LaunchType.Single;
    }
}
public class SurroundMissileBehavior : KratosMissileBehavior
{
    public SurroundMissileBehavior()
    {
        MinSurroundDistance = 1500;
        MaxSurroundDistance = 3000;
        MaxAttackDistance = 0;
        MinAngleGravity = 100;
        MaxAngleGravity = 180;
        AttackPattern = AttackPattern.SurroundAndWaitForAttack;
        LaunchType = LaunchType.Staggered;
    }
}
public class InterdictMissileBehavior : KratosMissileBehavior
{
    public InterdictMissileBehavior()
    {
        MinSurroundDistance = 1500;
        MaxSurroundDistance = 11000;
        MaxAttackDistance = 5000;
        MinAngleGravity = 85;
        MaxAngleGravity = 120;
        AttackPattern = AttackPattern.LoiterAndPursue;
        LaunchType = LaunchType.Staggered;
    }
}
public enum MissileControlMode
{
    Loiter,
    Attack
}
internal class KratosMissileManager
{
    // TODO: Clean up fields

    // TODO: Make this part of the config
    private readonly int _missileCount = 16;

    // TODO: Assign lists
    private readonly List<KratosMissile> _readyMissiles;
    private readonly Dictionary<KratosMissile, int> _pendingLaunchMissiles;
    private readonly List<KratosMissile> _launchedMissiles;

    int _frame;
    bool _hasTarget;
    private readonly Random _rng;

    ArgusTargetableShip _currentTargetedShip;
    public List<ArgusTargetableShip> _targetCandidates; 

    ArgusShip _host; // TODO: Construct me

    private readonly List<string> _groupNames = new List<string>();

    // Keeps track of whether missiles for each group has been accquired so duplicate entries aren't created
    private readonly Dictionary<string, bool> _accquiredMissiles = new Dictionary<string, bool>();

    TargetableBlockCategory _targetCategory = TargetableBlockCategory.Default;

    private readonly Dictionary<string, MissileDetachmentGroup> _detachmentGroups =
        new Dictionary<string, MissileDetachmentGroup>();

    KratosMissileBehavior _kratosMissileBehavior =
        KratosMissileBehavior.Behaviors[KratosMissileBehaviorType.SurroundMissileBehavior];

    bool _attack;

    // TODO: Clean up constructor

    public KratosMissileManager(IMyGridTerminalSystem gridTerminalSystem, ArgusShip host)
    {
        _host = host;
        _rng = new Random();
        _targetCandidates = new List<ArgusTargetableShip>();
        _readyMissiles = new List<KratosMissile>();
        _pendingLaunchMissiles = new Dictionary<KratosMissile, int>();
        _launchedMissiles = new List<KratosMissile>();
        for (var i = 1; i <= _missileCount; i++)
        {
            var missileGroupName = "M" + i;
            var detachmentGroupName = "DM" + i;

            _groupNames.Add(missileGroupName);

            _accquiredMissiles.Add(missileGroupName, false);
            var missileGroup = gridTerminalSystem.GetBlockGroupWithName(missileGroupName);
            if (missileGroup != null)
            {
                var gyros = new List<IMyGyro>();
                missileGroup.GetBlocksOfType(gyros);
                foreach (var gyro in gyros) gyro.CustomData = "";
            }

            var detachmentGroup = gridTerminalSystem.GetBlockGroupWithName(detachmentGroupName);
            if (detachmentGroup != null)
            {
                var thrusters = new List<IMyThrust>();
                detachmentGroup.GetBlocksOfType(thrusters);
                _detachmentGroups.Add(missileGroupName, new MissileDetachmentGroup(thrusters));
            }
        }
    }

    // TODO: Call from argument somewhere
    public void SetNewBehaviorType(KratosMissileBehaviorType behaviorType)
    {
        _kratosMissileBehavior = KratosMissileBehavior.Behaviors[behaviorType];


        // Fighter > Fighter: nothing
        // Fighter > Surround: stop attacking and loiter around target if valid else loiter around player
        // Figher > interdict: check for targets in range and attack else loiter

        // Surround > Fighter: attack
        // Surround > Surround: nothing
        // Surround > interdict: check for targets in range and attack else loiter

        // Interdict > Fighter: attack
        // Interdict > Surround: stop attacking and loiter around target if valid else loiter around player
        // Interdict > Interdict: nothing

        // So it's symmetrical regardless of what the switch is doing then? Cool
        // TODO: Implement specific changes when switching behaviors
        switch (behaviorType)
        {
            case KratosMissileBehaviorType.FighterMissileBehavior:
                break;
            case KratosMissileBehaviorType.InterdictMissileBehavior:
                break;
            case KratosMissileBehaviorType.SurroundMissileBehavior:
                break;
        }
    }

    public void UpdateTargetedShip(ArgusTargetableShip newTargetedShip)
    {
        _currentTargetedShip = newTargetedShip;
    }
    // TODO: Call me
    public void CycleTargetCategory()
    {
        _CycleTargetCategory();
    }


    void _CycleTargetCategory()
    {
        switch (_targetCategory)
        {
            case TargetableBlockCategory.Default:
                _targetCategory = TargetableBlockCategory.Propulsion;
                break;
            case TargetableBlockCategory.Propulsion:
                _targetCategory = TargetableBlockCategory.Weapons;
                break;
            case TargetableBlockCategory.Weapons:
                _targetCategory = TargetableBlockCategory.PowerSystems;
                break;
            case TargetableBlockCategory.PowerSystems:
                _targetCategory = TargetableBlockCategory.Default;
                break;
        }
    }

    // TODO: Select indexes for missiles when a new target is scanned (done?)
    // TODO: Call from Program.cs
    public void TargetJustScanned(ArgusTargetableShip newShip)
    {
        foreach (var missile in _launchedMissiles) SetTargetedBlockIndices(missile, newShip);
    }

    // TODO: Call this on new missile launch
    void SetTargetingIndicesForNewMissile(KratosMissile missile, List<ArgusTargetableShip> ships)
    {
        foreach (var ship in ships) SetTargetedBlockIndices(missile, ship);
    }
    // ^^^^^
    // These two together should cover all bases?
    // If a missile is launched, it needs to discover all existing ships and their blocks to select a target
    // If a ship is detected, it needs to tell all the missiles to select a target


    // Should this be moved to another class?
    // Either KratosMissile or TargetableShip, which one does it "belong" to more?
    // I guess targetable ship because it's getting data from that specifically?
    // Or I could just keep it here because this is neutral ground
    void SetTargetedBlockIndices(KratosMissile missile, ArgusTargetableShip ship)
    {
        if (ship == null)
        {
            return;
        }


        var targetedIndices = new Dictionary<TargetableBlockCategory, int>
        {
            {
                TargetableBlockCategory.Default,
                GetTargetedBlockIndexForCategory(ship, TargetableBlockCategory.Default)
            },
            {
                TargetableBlockCategory.Weapons,
                GetTargetedBlockIndexForCategory(ship, TargetableBlockCategory.Weapons)
            },
            {
                TargetableBlockCategory.Propulsion,
                GetTargetedBlockIndexForCategory(ship, TargetableBlockCategory.Propulsion)
            },
            {
                TargetableBlockCategory.PowerSystems,
                GetTargetedBlockIndexForCategory(ship, TargetableBlockCategory.PowerSystems)
            }
        };

        // TODO: Is this check redundant?
        // Add the kvp for this ship to the missile if it doesn't exist

        //if (!missile.targetedCategoryBlockIndices.ContainsKey(ship)) missile.targetedCategoryBlockIndices.Add(ship, targetedIndices);
        missile.TargetedCategoryBlockIndices[ship] = targetedIndices;
        // Update the index for the targeted block
    }


    int GetTargetedBlockIndexForCategory(ArgusTargetableShip ship, TargetableBlockCategory cat)
    {
        var index = -1;
        switch (cat)
        {
            case TargetableBlockCategory.Default:
                if (ship.AllBlocksCount == 0) break;
                index = _rng.Next(0, ship.AllBlocksCount - 1);
                break;
            case TargetableBlockCategory.Propulsion:
                if (ship.PropulsionBlocksCount == 0) break;
                index = _rng.Next(0, ship.PropulsionBlocksCount - 1);
                break;
            case TargetableBlockCategory.Weapons:
                if (ship.WeaponBlocksCount == 0) break;
                index = _rng.Next(0, ship.WeaponBlocksCount - 1);
                break;
            case TargetableBlockCategory.PowerSystems:
                if (ship.PowerSystemsBlocksCount == 0) break;
                index = _rng.Next(0, ship.PowerSystemsBlocksCount - 1);
                break;
        }

        return index;
    }

    // TODO: Tackle this absolute monstrosity
    public void Update(ArgusTargetableShip primaryEnemyShip)
    {
        _frame++;

        var gravity = _host.Controller.GetNaturalGravity();
        var gravityNormal = gravity.Normalized();
            

        HandleAttackPatterns(primaryEnemyShip, gravity, gravityNormal);
        FindNewMissiles();
        HandleLaunchQueue();
    }

    void FindNewMissiles()
    {
        if ((_frame + 32) % 9 == 0)
            foreach (var name in _groupNames)
            {
                if (_accquiredMissiles[name]) continue;
                var group = Program._gridTerminalSystem.GetBlockGroupWithName(name);
                if (group == null) continue;
                var blocks = new List<IMyTerminalBlock>();
                group.GetBlocksOfType(blocks);
                    
                var missile = new KratosMissile(_host, blocks, _kratosMissileBehavior,
                    _rng.NextDouble() * (_kratosMissileBehavior.MaxGuidanceFactor -
                                         _kratosMissileBehavior.MinGuidanceFactor) +
                    _kratosMissileBehavior.MinGuidanceFactor, name, false);
                if (missile == null || !missile.ConstructedSuccessfully) continue;
                _accquiredMissiles[name] = true;
                _readyMissiles.Add(missile);
            }
    }


        
    void HandleAttackPatterns(ArgusTargetableShip ship, Vector3D gravity, Vector3D gravityNormal)
    {
        // TODO: Set up params
        switch (_kratosMissileBehavior.AttackPattern)
        {
            case AttackPattern.DirectAttack:
                // Guide missile to target if it has one or guide to position
                DirectAttack(ship, gravity, gravityNormal);
                break;

            case AttackPattern.SurroundAndWaitForAttack:
                // Guide missile to surround the host ship if it does not have a target, or surround the target if it has one, or attack the target if it has one and attack is true
                SurroundAndWaitForAttack(ship, gravity, gravityNormal);
                break;

            case AttackPattern.LoiterAndPursue:
                // Guide missile to position and intercept nearby targets
                LoiterAndPursue(gravity, gravityNormal);
                break;
        }
    }
        
        
    // The most basic mode. Simply loiters around the target in a sphere and automatically goes for a direct attack when a target is detected.
    void DirectAttack(ArgusTargetableShip ship, Vector3D gravity, Vector3D gravityNormal)
    {
            
        if (ship == ArgusTargetableShip.Default)
            foreach (var missile in _launchedMissiles) // Ship is invalid, loiter around host
            {
                missile.LoiterAroundHost();
                missile.Target = ship;
                missile.Update(gravity, gravityNormal);
            }
        else
            foreach (var missile in _launchedMissiles) // Ship is valid, loiter aruond target
            {
                missile.InterceptTarget();
                missile.Target = ship;
                missile.Update(gravity, gravityNormal);
            }

        // TODO: get ship pos and gravity, rework update to take only what is required
    }

    // Surrounds the host ship in a tight sphere, which translates itself directly to the position of the primary enemy if detected
    void SurroundAndWaitForAttack(ArgusTargetableShip ship, Vector3D gravity, Vector3D gravityNormal)
    {
        if (ship == ArgusTargetableShip.Default)
            foreach (var missile in _launchedMissiles) // Ship is invalid, loiter around host
            {
                missile.LoiterAroundHost();
                missile.Target = ship;
                missile.Update(gravity, gravityNormal);
                ALDebug.Echo("Loitering around host?");
            }
        else if (_attack)
            // Attack target
            foreach (var missile in _launchedMissiles) // Ship is valid and attack is true, intercept target
            {
                missile.InterceptTarget();
                missile.Target = ship;
                missile.Update(gravity, gravityNormal);
                ALDebug.Echo("Attacking target?");
            }
        else
            // Surround target
            foreach (var missile in _launchedMissiles) // Ship is valid and attack is false, loiter around target
            {
                missile.LoiterAroundTarget();
                missile.Target = ship;
                missile.Update(gravity, gravityNormal);
                ALDebug.Echo("Surrounding target?");
            }
    }


    // Takes a range of targets and figures out per missile which ship would be the best to atack
    void LoiterAndPursue(Vector3D gravity, Vector3D gravityNormal)
    {
        // TODO: Make target finding once every 10 frames
        foreach (var missile in _launchedMissiles)
        {
            var target = GetClosestTarget(_targetCandidates, missile);
            missile.Target = target;
            if (target == ArgusTargetableShip.Default || !_attack) // No targets in range or attack is turned off
                // We go to loiter position
                missile.LoiterAroundHost();
            else
                missile.InterceptTarget(); // Target in range and attack is turned on
            // TODO: get ship pos and gravity, rework update to take only what is required
            missile.Update(gravity, gravityNormal);
        }
    }

    ArgusTargetableShip GetClosestTarget(List<ArgusTargetableShip> ships, KratosMissile missile)
    {
        var minDistance = double.MaxValue;
        var targetFinal = ArgusTargetableShip.Default;
        foreach (var targetCandidate in ships)
        {
            var shipDistanceSquared = (targetCandidate.Position - missile.GetPosition()).LengthSquared();

            if (!targetCandidate.DataIsCurrent || !(shipDistanceSquared <
                                                    _kratosMissileBehavior.MaxAttackDistance *
                                                    _kratosMissileBehavior.MaxAttackDistance)) continue;

            if (shipDistanceSquared < minDistance)
            {
                minDistance = shipDistanceSquared;
                targetFinal = targetCandidate;
            }
        }

        return targetFinal;
    }

    // Safety function on recompile, server restart, PB crash, etc.
    public void DetonateAll()
    {
        foreach (var missile in _launchedMissiles)
        {
            if (!missile.HasLaunched) return;
            missile.ForceDetonate();
        }
    }

    // TODO: Add safety checks for launching based on missile direction and velocity (Should this be added to the launch queue instead?)
    // In that case, what should happen when a missile comes up in the queue as invalid?
    // It could:
    // a, keep it in the queue but don't try the next missile
    // b, keep it in the queue and immediately try launch the next missile
    // c, keep it in the queue and reduce the delay for the next missile as normal
    // d, Remove it from the queue but don't try the next missile
    // e, Remove it from the queue and immediately try launch the next missile
    // f, Remove it from the queue and reduce the delay for the next missile as normal


    // upsides of a:
    // Does not present a case where missiles could fire simultaneously
    // Preserves the launch order

    // downsides of a:
    // Queue can become indefinitely stuck if it is always false, leading to no missile launches


    // upsides of b:
    // Does not present a case where missiles could fire simultaneously
    // Cannot become stuck
    // Retains rate of fire up until that point
    // Scalable with multiple waiting missiles

    // downsides of b:
    // Sacrifices the launch order
    // If one missile gets stuck, the rest could launch in quick succession one frame after the other
    // This could be mitigated by storing the initial frame delay of the current missile but that would ignore the frame delay of the next missile


    // upsides of c:
    // Cannot become stuck
    // Retains rate of fire

    // ok that's enough upsides and downsides

    // COUNTERPROPOSAL:
    // g, keep it in the launch queue and don't try the next missile, but set a timer and forcibly launch the waiting missile anyway after a set delay

    // upsides:
    // Cannot become stuck
    // Retains launch order
    // Will never allow simultaneous fire

    // downsides:
    // sacrifices launch speed
    // Might blow a hole in the side of the ship because ignoring safety checks
    public void TryLaunch(MyGridProgram program, Vector3D shipPos, Vector3D velocity, Vector3D forward)
    {
        LaunchBoobyTrap();

        var shouldReturn = false;
        var delay = 0;
        // I could skip this switch entirely and have values for shouldReturn in KratosMissileBehavior and derrived classes
        // But then it would be difficult to refactor the launch type to be separated from the missile behavior
        switch (_kratosMissileBehavior.LaunchType)
        {
            case LaunchType.Single:
                shouldReturn = true;
                break;
            case LaunchType.Alpha:
                delay = 0;
                break;
            case LaunchType.Staggered:
                delay = _kratosMissileBehavior.MissileLaunchDelayFrames;
                break;
        }

            
            
        for (var index = _readyMissiles.Count - 1; index >= 0; index--)
        {
            var missile = _readyMissiles[index];
            if (!missile.IsComplete()) continue;

            _pendingLaunchMissiles.Add(missile, delay);

            if (shouldReturn) return;
        }
    }

    // TODO: test this thoroughly before I am comfortable enabling it
    void LaunchBoobyTrap()
    {
        // if (Program.TrollMode)
        // {
        //     
        //     if (rng.NextDouble() < 0.1)
        //     {
        //         BoobyTrap.DetonateAllTheWarheads(Program._gridTerminalSystem);
        //     }
        // }
    }

    // TODO: Call from update
    // Handles the launch queue
    void HandleLaunchQueue()
    {
        var missilesLaunched = new List<KratosMissile>();

        // In the case that all are 0, all will launch on the same frame, otherwise it will subtract from a given missiles pool
        foreach (var kvp in _pendingLaunchMissiles)
        {
            var missile = kvp.Key;
            var framesRemaining = kvp.Value;
            if (framesRemaining > 0)
            {
                _pendingLaunchMissiles[missile] = framesRemaining - 1;
                return;
            }

                
            // TODO: add params
            var result = ActuallyLaunch(missile, _host);
            // TODO: add result handling
            switch (result)
            {
                case 0: // success
                    missilesLaunched.Add(missile);
                    break;
                case 1: // Cannot launch due to relative velocity
                    break;
                case 2:
                    break;
            }
        }
            
        missilesLaunched.ForEach(x => { _pendingLaunchMissiles.Remove(x); _launchedMissiles.Add(x); });
    }


    int ActuallyLaunch(KratosMissile missile, ArgusShip host)
    {
        var missileForward = missile.GetForward();
        if (missileForward.Dot(host.Velocity) > _kratosMissileBehavior.MaxLaunchSpeed ||
            missileForward.Cross(host.Velocity).ALengthSquared() >
            _kratosMissileBehavior.MaxLaunchSpeed * _kratosMissileBehavior.MaxLaunchSpeed) return 1;


        // TODO: this monstrosity
        //SetTargetingIndicesForNewMissile(missile, _targetCandidates); // Disabled for now
        if (_currentTargetedShip != null) SetTargetedBlockIndices(missile, _currentTargetedShip);
        var missileLaunchResult = missile.Launch();
            
        switch (missileLaunchResult)
        {
            case 1: // fuel fail
                return 2; // fuel fail
        }
            
        _accquiredMissiles[missile.Name] = false;
            
            
        var missilePos = missile.GetThrusterPosition();
        var safetyPos = missilePos + missileForward * _kratosMissileBehavior.MissileSafetyDistance;
        var divergePos = safetyPos + (safetyPos - host.Position).Normalized() * _kratosMissileBehavior.MissileDivergeDistance;
        var surroundDir = GenerateRandomPositionOnSphere(Vector3D.Zero);
            
        var gravity = -_host.Controller.GetNaturalGravity();
        var gravityLength = gravity.Length();
        if (gravityLength > 0)
            surroundDir = GenerateGravityAttackPosition(gravity / gravityLength, _kratosMissileBehavior.MinAngleGravity, _kratosMissileBehavior.MaxAngleGravity);

        missile.SurroundDir = surroundDir;

        missile.SurroundDist =
            _rng.NextDouble() * (_kratosMissileBehavior.MaxSurroundDistance - _kratosMissileBehavior.MinSurroundDistance) + _kratosMissileBehavior.MinSurroundDistance;
        return 0;
    }

    Vector3D GenerateGravityAttackPosition(Vector3D direction, double minAngle, double maxAngle)
    {
        // Convert the angle range from degrees to radians
        var minVarianceRadians = MathHelper.ToRadians(minAngle);
        var maxVarianceRadians = MathHelper.ToRadians(maxAngle);

        // Generate a random variance between the minimum and maximum angle
        var varianceRadians = minVarianceRadians + _rng.NextDouble() * (maxVarianceRadians - minVarianceRadians);

        // Generate a random point on a unit sphere
        var theta = 2 * Math.PI * _rng.NextDouble(); // Random angle between 0 and 2*PI
        var u = 2 * _rng.NextDouble() - 1; // Random value between -1 and 1
        var sqrtOneMinusUSquared = Math.Sqrt(1 - u * u);
        var x = sqrtOneMinusUSquared * Math.Cos(theta);
        var y = sqrtOneMinusUSquared * Math.Sin(theta);
        var z = u;
        var randomPointOnSphere = new Vector3D(x, y, z);

        // Scale the random point by the randomly generated variance
        var perturbation = Vector3D.Normalize(randomPointOnSphere) * varianceRadians;

        // Normalize the original direction vector
        direction.Normalize();

        // Calculate the axis and angle for rotation
        var axis = Vector3D.Cross(Vector3D.Up, direction);
        var angle = Math.Acos(Vector3D.Dot(Vector3D.Up, direction));

        // Apply the rotation to the perturbation
        var rotatedPerturbation = Vector3D.Transform(perturbation, MatrixD.CreateFromAxisAngle(axis, angle));

        // Combine the rotated perturbation with the direction vector
        var deviatedDirection = direction + rotatedPerturbation;
        deviatedDirection.Normalize();

        // Compute the final attack position
        var dir = deviatedDirection;
        return dir;
    }

    public void Attack()
    {
        _attack = !_attack;
    }

    Vector3D GenerateRandomPositionOnSphere(Vector3D center)
    {
        // Generate random spherical coordinates
        var theta = 2 * Math.PI * _rng.NextDouble(); // Random angle in [0, 2*PI]
        var phi = Math.Acos(2 * _rng.NextDouble() - 1); // Random angle in [0, PI]

        // Convert spherical coordinates to Cartesian coordinates
        var x = Math.Sin(phi) * Math.Cos(theta);
        var y = Math.Sin(phi) * Math.Sin(theta);
        var z = Math.Cos(phi);

        // Create the random position vector
        var randomPosition = new Vector3D(x, y, z);

        // Translate the position by the center
        return center + randomPosition;
    }


    public List<MissileData> GetMissileData()
    {
        var missileData = new List<MissileData>();
        foreach (var missile in _launchedMissiles) missileData.Add(missile.GetData());
        return missileData;
    }


    public struct MissileData
    {
        public Vector3D Position;
        public Vector3D PreviousPosition;
        public Vector3D Forward;
        public Vector3D DistanceToTarget;
        public double Fuel;
        public long Id;
        public bool HasLaunched;
        public bool IsHydrogen;

        public MissileData(Vector3D position, Vector3D previousPosition, Vector3D forward,
            Vector3D distanceToTarget, double fuel, long id, bool hasLaunched, bool isHydrogen)
        {
            Position = position;
            PreviousPosition = previousPosition;
            Forward = forward;
            DistanceToTarget = distanceToTarget;
            Fuel = fuel;
            Id = id;
            HasLaunched = hasLaunched;
            IsHydrogen = isHydrogen;
        }
    }
}
internal class LinearGravityGenerator
{
    public IMyGravityGenerator actualGravityGenerator;


    private readonly sbyte sign = 1;

    public LinearGravityGenerator(IMyGravityGenerator gravityGenerator, sbyte sign, string name)
    {
        actualGravityGenerator = gravityGenerator;
        this.sign = sign;
        gravityGenerator.CustomName = $"LGravity Generator [{name}]";
    }

    public void SetGravity(float gravity)
    {
        actualGravityGenerator.GravityAcceleration = gravity * sign * 9.81f;
    }
}
internal class MassBlock
{
    public IMyArtificialMassBlock block;
    public double distanceFromCenterSquared;
    public bool Enabled = true;
    public Vector3D moment;
    public Vector3D previousMoment = new Vector3D(0, 0, 0);

    public MassBlock(IMyArtificialMassBlock massBlock)
    {
        block = massBlock;
        moment = new Vector3D(0, 0, 0);
    }

    public bool Functional => block.IsFunctional;

    public void CalculateMoment(Vector3D centerOfMass)
    {
        previousMoment = moment;
        var blockPosition = block.GetPosition();
        var distanceVector = blockPosition - centerOfMass;
        double mass = Functional ? 50000 : 0; // 50 tonnes in kg
        moment = distanceVector * mass;
        distanceFromCenterSquared = distanceVector.ALengthSquared();
    }
}
enum DetachmentState
{
    DelayEnable,
    Enable,
    DelayDisable,
    Disable
}
public class MissileDetachmentGroup
{
    List<IMyThrust> _thrusters;
    DetachmentState _state = DetachmentState.Enable;

    public MissileDetachmentGroup(List<IMyThrust> thrusters)
    {
        _thrusters = thrusters;
    }

    public void Detach()
    {
        // foreach (var thruster in _thrusters)
        // {
        //     thruster.Enabled = true;
        // }
        //
        // return;
        switch (_state)
        {
            case DetachmentState.DelayEnable:
                _state = DetachmentState.Enable;
                break;
            case DetachmentState.Enable:
                foreach (var thruster in _thrusters)
                {
                    thruster.Enabled = true;
                }
                _state = DetachmentState.DelayDisable;
                break;
            case DetachmentState.DelayDisable:
                _state = DetachmentState.Disable;
                break;
            case DetachmentState.Disable:
                foreach (var thruster in _thrusters)
                {
                    thruster.Enabled = false;
                }
                _state = DetachmentState.DelayEnable;
                break;
                    
        }
    }

    public void Reset()
    {
        _state = DetachmentState.Enable;
        foreach (var thruster in _thrusters)
        {
            thruster.Enabled = false;
        }
    }

}
public class PID
{
    double _errorSum;
    bool _firstRun = true;
    double _inverseTimeStep;
    double _lastError;

    double _timeStep;

    public PID(double kp, double ki, double kd, double timeStep)
    {
        Kp = kp;
        Ki = ki;
        Kd = kd;
        _timeStep = timeStep;
        _inverseTimeStep = 1 / _timeStep;
    }

    public double Kp { get; set; }
    public double Ki { get; set; }
    public double Kd { get; set; }
    public double Value { get; private set; }

    protected virtual double GetIntegral(double currentError, double errorSum, double timeStep)
    {
        return errorSum + currentError * timeStep;
    }

    public double GetErrorSum()
    {
        return _errorSum;
    }


    public double Control(double error)
    {
        //Compute derivative term
        var errorDerivative = (error - _lastError) * _inverseTimeStep;

        if (_firstRun)
        {
            errorDerivative = 0;
            _firstRun = false;
        }

        //Get error sum
        _errorSum = GetIntegral(error, _errorSum, _timeStep);

        //Store this error as last error
        _lastError = error;

        //Construct output
        Value = Kp * error + Ki * _errorSum + Kd * errorDerivative;
        return Value;
    }

    public double Control(double error, double timeStep)
    {
        if (timeStep != _timeStep)
        {
            _timeStep = timeStep;
            _inverseTimeStep = 1 / _timeStep;
        }

        return Control(error);
    }

    public virtual void Reset()
    {
        _errorSum = 0;
        _lastError = 0;
        _firstRun = true;
    }
}

public class PIDController
{
    double integral;
    double lastInput;

    public double gain_p;
    double gain_i;
    public double gain_d;
    double upperLimit_i;
    double lowerLimit_i;
    double second;

    public PIDController(double pGain, double iGain, double dGain, double iUpperLimit = 0, double iLowerLimit = 0, double stepsPerSecond = 60)
    {
        gain_p = pGain;
        gain_i = iGain;
        gain_d = dGain;
        upperLimit_i = iUpperLimit;
        lowerLimit_i = iLowerLimit;
        second = stepsPerSecond;
    }

    public double Filter(double input, int round_d_digits)
    {
        double roundedInput = Math.Round(input, round_d_digits);

        integral = integral + (input / second);
        integral = (upperLimit_i > 0 && integral > upperLimit_i ? upperLimit_i : integral);
        integral = (lowerLimit_i < 0 && integral < lowerLimit_i ? lowerLimit_i : integral);

        double derivative = (roundedInput - lastInput) * second;
        lastInput = roundedInput;

        return (gain_p * input) + (gain_i * integral) + (gain_d * derivative);
    }
    public void Reset()
    {
        integral = lastInput = 0;
    }
}public enum TargetableBlockCategory
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
enum ReloadTrackerCurrentState
{
    Bursting,
    Reloading,
    Reloaded
}
internal class ReloadTracker
{
    private readonly int _burstInterval;
    private readonly int _reloadInterval;
    private readonly int _burstCount;
        
    int _currentFrameInSequence;
    int _currentBurst;

    public double Velocity;
    public double Range;
    ReloadTrackerCurrentState _state = ReloadTrackerCurrentState.Reloaded;

    public ReloadTracker(int burstInterval, int reloadInterval, int burstCount, double velocity, double range)
    {
        _burstInterval = burstInterval;
        _reloadInterval = reloadInterval;
        _burstCount = burstCount;
        Velocity = velocity;
        Range = range;
    }

        

    public ReloadTrackerCurrentState GetState()
    {
        return _state;
    }

    public bool Update(bool doFire)
    {

            
            
        switch (_state)
        {
            case ReloadTrackerCurrentState.Bursting:
                _currentFrameInSequence--;
                // If we are firing and the timer says that it's time for the next shot
                if (doFire && _currentFrameInSequence <= 0)
                {
                    // Take a shot, reset timer
                    _currentFrameInSequence = _burstInterval;
                    _currentBurst++;
                    // If that was the last shot in the clip
                    if (_currentBurst >= _burstCount)
                    {
                        // Switch to the reloading state
                        _state = ReloadTrackerCurrentState.Reloading;
                        _currentFrameInSequence = _reloadInterval;
                        _currentBurst = 0;
                    }

                    return true;
                }
                break;
            case ReloadTrackerCurrentState.Reloading:
                _currentFrameInSequence--;
                // If the reload timer is zero
                if (_currentFrameInSequence <= 0)
                {
                    // Switch to the reloaded state
                    _state = ReloadTrackerCurrentState.Reloaded;
                }
                break;
            case ReloadTrackerCurrentState.Reloaded:
                if (doFire) _state = ReloadTrackerCurrentState.Bursting;
                break;
                
                
        }

        return false;
    }
}
internal class SpaceBall
{
    public IMySpaceBall block;
    public bool Enabled = true;
    public Vector3D moment;
    public Vector3D momentDown;
    public Vector3D momentUp;
    public Vector3D previousMoment;
    public double previousMoveDownMul = 1000;
    public double previousMoveUpMul = 1000;

    public SpaceBall(IMySpaceBall spaceBall)
    {
        block = spaceBall;
        moment = new Vector3D(0, 0, 0);
    }

    public bool Functional => block.IsFunctional;

    public void CalculateMoment(Vector3D centerOfMass)
    {
        previousMoment = moment;
        var blockPosition = block.GetPosition();
        var distanceVector = blockPosition - centerOfMass;

        double mass = block.VirtualMass;
        moment = distanceVector * mass;
        momentDown = distanceVector * Math.Max(mass - previousMoveDownMul, 0);
        momentUp = distanceVector * Math.Min(mass + previousMoveUpMul, 20000);
    }

    public Vector3D CalculateMoment(Vector3D centerOfMass, float mass)
    {
        var blockPosition = block.GetPosition();
        var distanceVector = blockPosition - centerOfMass;
        return distanceVector * mass;
    }

    public void SetMass(float mass)
    {
        block.VirtualMass = mass;
    }
}
internal class SphericalGravityGenerator
{
    public IMyGravityGeneratorSphere actualGravityGenerator;


    public sbyte sign = 1;

    public SphericalGravityGenerator(IMyGravityGeneratorSphere gravityGenerator, sbyte sign, string name)
    {
        actualGravityGenerator = gravityGenerator;
        this.sign = sign;
        gravityGenerator.CustomName = $"SGravity Generator [{name}]";
    }

    public void SetGravity(float gravity)
    {
        actualGravityGenerator.GravityAcceleration = gravity * sign * 9.81f;
    }

    public void SetRadius(float range)
    {
        actualGravityGenerator.Radius = range;
    }

    public void SetGravityOverride(float gravity)
    {
        actualGravityGenerator.GravityAcceleration = gravity;
    }
}
internal class TargetableBlock
{
    public TargetableBlockCategory category;
    public Vector3I positionInGrid;

    public TargetableBlock(TargetableBlockCategory category, Vector3I positionInGrid)
    {
        this.category = category;
        this.positionInGrid = positionInGrid;
    }
}
public static class Targeting
{
    private static double maxVelocity = 104;
    public static IMyShipController Controller;
    public static MyGridProgram program;

    public static Vector3D ComputeInterceptPoint(ref Vector3D targetPosition, Vector3D targetVelocity, Vector3D targetAcceleration, ref Vector3D shipPosition, ref Vector3D shipVelocity, ref Vector3D gravity, ref long lastTargetEntityId, WeaponDefinition weapon)
        {
            Vector3D displacementVector = targetPosition - shipPosition;
        
            double Tmax = 0;
            double Dmax = 0;
            double DAtTmax = 0;
            double TApprox = displacementVector.Length() / (Vector3D.Normalize(displacementVector) * weapon.MaxSpeed + (targetVelocity - shipVelocity)).Length();
            double speedApprox = (Vector3D.Normalize(targetPosition + targetVelocity * TApprox + targetAcceleration * TApprox * TApprox - shipPosition) * weapon.InitialSpeed + shipVelocity).Length();
            if (weapon.IsCappedSpeed)
                speedApprox = Math.Min(speedApprox, weapon.MaxSpeed);
        
            if (weapon.Acceleration != 0)
            {
                Tmax = (weapon.MaxSpeed - speedApprox) / weapon.Acceleration;
        
                Dmax =weapon.InitialSpeed * Tmax + weapon.Acceleration * Tmax * Tmax;
                DAtTmax = (targetPosition + targetVelocity * Tmax + 0.5 * targetAcceleration * Tmax * Tmax - shipPosition).Length();
            }
        
            Vector3D TargetVelocityS = targetVelocity;
            double MissileAccelerationD = weapon.Acceleration;
            double MissileVelocityD = speedApprox;
            double TargetAccelerationD = targetAcceleration.Length();
            double TargetVelocityD = targetVelocity.Length();
            double TOffset = 0;
            Vector3D DOffset = Vector3D.Zero;
            if (DAtTmax > Dmax)
            {
                MissileAccelerationD = 0;
                MissileVelocityD = weapon.MaxSpeed;
                DOffset = -Vector3D.Normalize(displacementVector) * Dmax;
                TOffset = Tmax;
            }
        
            //Target Max Speed Math
            double TmaxT = 0;
            if (TargetAccelerationD > 1)
            {
                TmaxT = (maxVelocity - Math.Min(Vector3D.ProjectOnVector(ref targetVelocity, ref targetAcceleration).Length(), maxVelocity)) / TargetAccelerationD;
                TargetAccelerationD = 0;
                targetVelocity = Vector3D.Normalize(targetVelocity) * maxVelocity;
                TargetVelocityD = maxVelocity;
            }
        
            //Quartic
            double a = (0.25 * (MissileAccelerationD * MissileAccelerationD)) - (0.25 * (TargetAccelerationD * TargetAccelerationD));
            double b = (MissileVelocityD * MissileAccelerationD) - (TargetVelocityD * TargetAccelerationD);
            double c = (MissileVelocityD * MissileVelocityD) - (TargetVelocityD * TargetVelocityD) - displacementVector.Dot(targetAcceleration);
            double d = -2 * targetVelocity.Dot(displacementVector);
            double e = -displacementVector.LengthSquared();
        
            //double time = FastSolver.Solve(a, b, c, d, e) - TOffset;
            double time = 0;
            if (time == double.MaxValue || double.IsNaN(time))
                time = 100;
        
            if (TmaxT > time)
            {
                TmaxT = time;
                time = 0;
            }
            else
                time -= TmaxT;
        
            return targetPosition + (targetVelocity - shipVelocity) * time + (TargetVelocityS - shipVelocity) * TmaxT + 0.5 * targetAcceleration * TmaxT * TmaxT - 0.5 * gravity * (time + TmaxT) * (time + TmaxT) * Convert.ToDouble(weapon.HasGravity) + DOffset;
        }
        
    public static Vector3D GetTargetLeadPosition(Vector3D targetPos, Vector3D targetVel, Vector3D shooterPos,
        Vector3D shooterVel, float projectileSpeed, double timeStep, ref Vector3D previousTargetVelocity,
        bool doEcho, bool leadAcceleration)
    {
        var deltaV = (targetVel - previousTargetVelocity) / timeStep / 2;

        var relativePos = targetPos - shooterPos;
        var relativeVel = targetVel - shooterVel;
        var gravity = Controller.GetNaturalGravity() / 2;

        deltaV = leadAcceleration ? deltaV : Vector3D.Zero;

        var timeToIntercept = CalculateTimeToIntercept(relativePos, relativeVel, deltaV, -gravity, projectileSpeed);
        var targetLeadPos = targetPos + (deltaV + relativeVel) * timeToIntercept + -gravity * timeToIntercept;

        previousTargetVelocity = targetVel;
        return targetLeadPos;
    }

    private static Vector3D RotatePosition(Vector3D position, Vector3D center, MatrixD rotation)
    {
        var relativePosition = position - center;
        var rotatedPosition = Vector3D.Transform(relativePosition, rotation);
        return center + rotatedPosition;
    }

    private static double CalculateTimeToIntercept(Vector3D relativePos, Vector3D relativeVel, Vector3D targetAcc,
        Vector3D gravity, float projectileSpeed)
    {
        var a = targetAcc.ALengthSquared() - projectileSpeed * projectileSpeed;
        var b = 2 * (Vector3D.Dot(relativePos, targetAcc) + Vector3D.Dot(relativeVel, targetAcc));
        var c = relativePos.ALengthSquared();

        // Factor in gravity
        a += gravity.ALengthSquared();
        b += 2 * Vector3D.Dot(relativePos, gravity);

        var discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
            // No real solution, return a default position (e.g., current target position)
            return 0;

        var t1 = (-b + Math.Sqrt(discriminant)) / (2 * a);
        var t2 = (-b - Math.Sqrt(discriminant)) / (2 * a);

        if (t1 < 0 && t2 < 0)
            // Both solutions are negative, return a default position (e.g., current target position)
            return 0;
        if (t1 < 0)
            // t1 is negative, return t2
            return t2;
        if (t2 < 0)
            // t2 is negative, return t1
            return t1;
        // Both solutions are valid, return the minimum positive time
        return Math.Min(t1, t2);
    }
}
public class WeaponDefinition
{
    public double MaxSpeed;
    public double InitialSpeed;
    public bool IsCappedSpeed;
    public double Acceleration;
    public bool HasGravity;


    public WeaponDefinition(double MaxSpeed, double InitialSpeed, bool isCappedSpeed, double Acceleration,
        bool HasGravity)
    {
        this.MaxSpeed = MaxSpeed;
        this.InitialSpeed = InitialSpeed;
        this.IsCappedSpeed = isCappedSpeed;
        this.Acceleration = Acceleration;
        this.HasGravity = HasGravity;
    }
}
