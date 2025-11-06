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

    public A B;
    public static Program C;
    public static bool D = false;
    public static IMyGridTerminalSystem E;
        
    private readonly MyIni F = new MyIni();
        
    Dictionary<string, G> H =
        new Dictionary<string, G>()
        {
            {"MyObjectBuilder_LargeMissileTurret/LargeCalibreTurret", new G(1.3333333333, 12, 2, 500, 2000)},
            {"MyObjectBuilder_LargeMissileTurret/LargeBlockMediumCalibreTurret", new G(3, 6, 2, 500, 1400)},
        };

    int I;
    J K;
    L M;
        
    double
        N =
            0.999;

    private readonly List<IMyTerminalBlock> O = new List<IMyTerminalBlock>();

    private readonly P Q;


    private readonly double R = 0.75 * 60.0;
    private readonly double S = 12.0 * 60.0;


    Vector3D T = Vector3D.Zero;
    float U;
    private readonly List<IMyBeacon> V;
    private readonly StringBuilder W = new StringBuilder();
    private readonly List<IMyTextSurface> X;
    bool Y;
    int Z;

    Vector3D a = Vector3D.Zero;

    double
        b =
            1.6;

    private readonly IMyBroadcastListener c;


    private readonly int d = 0;

    int e;

    float
        f = 5;

    private readonly g h;
    IMyBlockGroup i;



    string j = "Flight Control";
    private readonly k l;
    List<IMyGyro> m;

    bool n;
    bool o;
    double p = 0.05;
        

    private readonly Dictionary<MyDefinitionId, float> q = new Dictionary<MyDefinitionId, float>
    {
        [MyDefinitionId.Parse("SmallMissileLauncherReload/SmallRailgun")] = 0.5f,
        [MyDefinitionId.Parse("SmallMissileLauncherReload/LargeRailgun")] = 2.0f
    };






    double r = 500;
    double s = 30;
    double t;

    private readonly u v;


    double w;
    Vector3D x = Vector3D.Zero;
    float y = 30;





    double
        z = 2.0;




    int ª = 5;

    private readonly List<IMyOffensiveCombatBlock> µ;

    double
        º =
            31.0 * 60.0;


    int À;

    double Á;

        
    bool Â = false;



    float Ã = 80;


    private readonly Ä Å;

    bool Æ;
    Vector3D Ç;

    Vector3D È = Vector3D.Zero;

    Vector3D É = Vector3D.Zero;
    Vector3D Ê = Vector3D.Zero;
    Vector3D Ë = Vector3D.Zero;
    Vector3D Ì = Vector3D.Zero;


    Í Ï = Í.Î;
    Ð Ò = Ð.Ñ;
    float Ó = 2000;

    float Ô = 2000f;
    Vector3D Õ = Vector3D.Zero;
    Vector3D Ö = Vector3D.Zero;

    IMyFunctionalBlock Ø;

    long Ù;
    List<IMyRemoteControl> Ú;
    float Û = 400;


    private readonly Random Ü;
    Ä Ý;
    private readonly List<float> Þ = new List<float>();
    bool ß;
    Í à = Í.Î;
    int á = 10;
    int â;

    bool ã;
    int ä;
    IMyShipController å;
    private readonly List<IMyShipController> æ;


    private readonly ç è = new ç();


    private readonly Dictionary<long, J> é = new Dictionary<long, J>();
    double ê;


    private readonly StringBuilder ë = new StringBuilder();


    private readonly string[] ì = { "Attacking ", "Status: " };
    int í;

    Vector3D î;

    Dictionary<IMyFunctionalBlock, MyDetectedEntityInfo> ï =
        new Dictionary<IMyFunctionalBlock, MyDetectedEntityInfo>();

    Vector3D ð = Vector3D.Zero;

    private readonly int ñ = 180;
    public static readonly double ò = 1.0 / 60.0;

    private readonly List<IMyTurretControlBlock> ó;




    float ô = 500f;
    private readonly Dictionary<IMyLargeTurretBase, õ> ö;
    List<IMyLargeTurretBase> ø;









    private readonly string ù = "0.11.0";

    int ú = 20;
    private readonly Ä û;
    int[] ü = {67, 48, 76};
    public Program()
    {
        C = this;
        B = new A(this);
            
        D = Me.GetOwnerFactionTag() !=
                    "" + (char)ü[0] + (char)ü[1] + (char)ü[2];
        E = GridTerminalSystem;
        ý.þ();
        ÿ();
        â = á;

        Ā.ā = this;
        var Ă = new List<IMyUserControllableGun>();
        æ = new List<IMyShipController>();
        ø = new List<IMyLargeTurretBase>();
        ö = new Dictionary<IMyLargeTurretBase, õ>();
        ó = new List<IMyTurretControlBlock>();
        m = new List<IMyGyro>();
        var ă = new List<IMyTextPanel>();
        X = new List<IMyTextSurface>();
        Ú = new List<IMyRemoteControl>();
        var Ą = new List<IMyArtificialMassBlock>();
        var ą = new List<IMySpaceBall>();
        var Ć = new List<IMyGravityGenerator>();
        var ć = new List<IMyGravityGeneratorSphere>();
        V = new List<IMyBeacon>();
        µ = new List<IMyOffensiveCombatBlock>();
        var Ĉ = new List<IMyCameraBlock>();

        i = GridTerminalSystem.GetBlockGroupWithName(j);
        if (i == null)
        {
            Echo("Group not found");
            return;
        }

        var O = new List<IMyTerminalBlock>();
        i.GetBlocks(O);
        for (var ĉ = O.Count - 1; ĉ >= 0; ĉ--)
        {
            var Ċ = O[ĉ];
            if (!GridTerminalSystem.CanAccess(Ċ, MyTerminalAccessScope.Construct)) O.RemoveAt(ĉ);
        }

        Ă = O.OfType<IMyUserControllableGun>().ToList();
        æ = O.OfType<IMyShipController>().ToList();
        ø = O.OfType<IMyLargeTurretBase>().ToList();
        ó = O.OfType<IMyTurretControlBlock>().ToList();
        m = O.OfType<IMyGyro>().ToList();
        ă = O.OfType<IMyTextPanel>().ToList();
        Ú = O.OfType<IMyRemoteControl>().ToList();
        Ą = O.OfType<IMyArtificialMassBlock>().ToList();
        ą = O.OfType<IMySpaceBall>().ToList();
        Ć = O.OfType<IMyGravityGenerator>().ToList();
        ć = O.OfType<IMyGravityGeneratorSphere>().ToList();
        V = O.OfType<IMyBeacon>().ToList();
        µ = O.OfType<IMyOffensiveCombatBlock>().ToList();
        Ĉ = O.OfType<IMyCameraBlock>().ToList();

        var Đ = è.Č ? k.ċ.Č : è.č ? k.ċ.Ď : k.ċ.ď;
            
        l = new k(Ă, this, q, f, Đ, ú);
        đ();
        h = new g(Ą, ą, GridTerminalSystem, this,
            Ć, ć, å, Ã,
            Û);

        Å = new Ä(r, t, s, -p, p);
        û = new Ä(r, t, s, -p, p);
        Ý = new Ä(r, t, s, -p, p);
        Runtime.UpdateFrequency = UpdateFrequency.Update1;

        foreach (var Ē in æ)
        {
            var ē = Ē as IMyCockpit;
            var Ĕ = ē.GetSurface(0);
            if (Ĕ == null) continue;
            X.Add(Ĕ);
                
        }
            
        bool ĕ = false;
        foreach (var Ė in ø)
            if (!ö.ContainsKey(Ė))
            {
                string ė = Ė.BlockDefinition.ToString();

                if (!H.ContainsKey(ė))
                {
                    H.Add(ė, new G(0, 0, 0, 0, 0));
                        
                    F.Set("ReloadTrackerDefinition." + ė, "ShotsPerSecond", 0);
                    F.Set("ReloadTrackerDefinition." + ė, "ReloadTime", 0);
                    F.Set("ReloadTrackerDefinition." + ė, "BurstCount", 0);
                    F.Set("ReloadTrackerDefinition." + ė, "Velocity", 1);
                    F.Set("ReloadTrackerDefinition." + ė, "Range", 0);
                    ĕ = true;
                }
                    
                G Ę = H[ė];
                ö.Add(Ė, new õ(Ę.ę, Ę.Ě, Ę.ě, Ę.Ĝ, Ę.ĝ));
            }
        if (ĕ) Me.CustomData = F.ToString();
                

        Ğ.ğ(ă);
        Ğ.ā = this;

        var Ġ = new List<IMyTextSurface>();
        foreach (var ġ in ă) Ġ.Add(ġ);
        foreach (var ġ in X) Ġ.Add(ġ);

        foreach (var Ģ in µ) Ģ.UpdateTargetInterval = 5;

        Q = new P(Ġ, ù, Ĉ, ref è);

            
        M = new L(Me.CubeGrid.EntityId, Me.CubeGrid.WorldMatrix, Me.CubeGrid.GetPosition(), å);
        K = J.Î;
        v = new u(GridTerminalSystem, M);


        Ü = new Random();

        ý.ģ = å;


        c = IGC.RegisterBroadcastListener("ArgusLiteDiagnosticRequest");
        IGC.SendBroadcastMessage("ArgusLiteRegisterFlightData", Me.EntityId);
            
            
            
        Echo("Constructed successfully");
    }

    void ÿ()
    {
        var Ĥ = "ArgusLiteCommonConfig";
        var ĥ = "ArgusLiteWeaponConfig";
        var Ħ = "ArgusLiteTurretConfig";
        var ħ = "ArgusLiteMissileConfig";
        var Ĩ = "ArgusLiteGravityDriveConfig";
        var ĩ = "ArgusLiteScanConfig";
        var Ī = "ArgusLiteAimbotConfig";
        var ī = "ArgusLiteHUDConfig";

        F.TryParse(Me.CustomData);


        j = F.Get(Ĥ, "GroupName").ToString(j);
        y = F.Get(Ĥ, "MaxAngularVelocityRPM").ToSingle(y);
        è.Ĭ = F.Get(Ĥ, "LeadAcceleration").ToBoolean(è.Ĭ);
        º = F.Get(Ĥ, "NewBlockCheckPeriod").ToDouble(º);

        F.Set(Ĥ, "GroupName", j);
        F.SetComment(Ĥ, "GroupName", "Group name, please ensure this is unique to your ship");
        F.Set(Ĥ, "MaxAngularVelocityRPM", y);
        F.SetComment(Ĥ, "MaxAngularVelocityRPM", "Should be 30 for large grid, 60 for small grid");
        F.Set(Ĥ, "LeadAcceleration", è.Ĭ);
        F.SetComment(Ĥ, "LeadAcceleration",
            "Should we use acceleration in the lead calculation or only velocity? (both fixed weapons and turrets)");
        F.Set(Ĥ, "NewBlockCheckPeriod", º);
        F.SetComment(Ĥ, "NewBlockCheckPeriod",
            "How often we should check the group for new blocks, try to avoid round numbers otherwise everything could happen at once and burn out the programmable block");

        F.SetSectionComment(Ĥ, "\n\nArgus Common Config.\n\nEDIT HERE:");

        è.ĭ = F.Get(ĥ, "DoAutoFire").ToBoolean(è.ĭ);
        z = F.Get(ĥ, "MaximumSafeRPMToFire").ToDouble(z);
        Ó = F.Get(ĥ, "ProjectileMaxDistance").ToSingle(Ó);
        N = F.Get(ĥ, "AimbotWeaponFireSigma").ToDouble(N);
        Ô = F.Get(ĥ, "ProjectileVelocity").ToSingle(Ô);
        f = F.Get(ĥ, "FramesToGroupGuns").ToSingle(f);
        è.č = F.Get(ĥ, "DoVolley").ToBoolean(è.č);
        è.Č = F.Get(ĥ, "WaitForAll").ToBoolean(è.Č);
        ú = F.Get(ĥ, "VolleyDelayFrames").ToInt32(ú);

        F.Set(ĥ, "DoAutoFire", è.ĭ);
        F.SetComment(ĥ, "DoAutoFire", "Automatically fire and cancel fire?");
        F.Set(ĥ, "MaximumSafeRPMToFire", z);
        F.SetComment(ĥ, "MaximumSafeRPMToFire",
            "Not below this RPM (requires that MaxAngularVelocityRPM is set correctly)");
        F.Set(ĥ, "ProjectileMaxDistance", Ó);
        F.SetComment(ĥ, "ProjectileMaxDistance", "Within this distance");
        F.Set(ĥ, "AimbotWeaponFireSigma", N);
        F.SetComment(ĥ, "AimbotWeaponFireSigma",
            "Dot product of this ships forward vector and the target vector is less than this");
        F.Set(ĥ, "ProjectileVelocity", Ô);
        F.SetComment(ĥ, "ProjectileVelocity", "Default projectile velocity, can be changed by argument");
        F.Set(ĥ, "FramesToGroupGuns", f);
        F.SetComment(ĥ, "FramesToGroupGuns",
            "Maximum frame difference between available guns to aim and fire them together");
        F.Set(ĥ, "DoVolley", è.č);
        F.SetComment(ĥ, "DoVolley", "DO NOT ENABLE NOT FINISHED");
        F.Set(ĥ, "WaitForAll", è.Č);
        F.SetComment(ĥ, "WaitForAll", "Whether to wait for all guns before firing (WARNING: DPS WILL BE LOST)");
        F.Set(ĥ, "VolleyDelayFrames", ú);
        F.SetComment(ĥ, "VolleyDelayFrames", "Spacing between each weapon firing for Volley mode");

        F.SetSectionComment(ĥ, "\n\nArgus Weapon Config.\n\nEDIT HERE:");

        è.Į = F.Get(Ħ, "DoTurretAim").ToBoolean(è.Į);
        ô = F.Get(Ħ, "TurretProjectileVelocity").ToSingle(ô);

        F.Set(Ħ, "DoTurretAim", è.Į);
        F.SetComment(Ħ, "DoTurretAim", "Should we override the turret aim?");
        F.Set(Ħ, "TurretProjectileVelocity", ô);
        F.SetComment(Ħ, "TurretProjectileVelocity",
            "Set as whatever the projectile velocity of the turrets are");

        F.SetSectionComment(Ħ, "\n\nArgus Turret Config.\n\nEDIT HERE:");

        Â = F.Get(ħ, "DoFighterMode").ToBoolean(Â);
            
        F.Set(ħ, "DoFighterMode", Â);
        F.SetComment(ħ, "DoFighterMode", "Should we fire missiles one at a time?");
            
            
        è.į = F.Get(Ĩ, "DoGravityDrive").ToBoolean(è.į);
        è.İ = F.Get(Ĩ, "DoBalance").ToBoolean(è.İ);
        è.ı = F.Get(Ĩ, "DoRepulse").ToBoolean(è.ı);
        è.Ĳ = F.Get(Ĩ, "PrecisionMode").ToBoolean(è.Ĳ);
        Ã = F.Get(Ĩ, "PassiveSphericalRange").ToSingle(Ã);
        Û = F.Get(Ĩ, "RepulseSphericalRange").ToSingle(Û);

        F.Set(Ĩ, "DoGravityDrive", è.į);
        F.SetComment(Ĩ, "DoGravityDrive", "Should we manage gravity at all?");
        F.Set(Ĩ, "DoBalance", è.İ);
        F.SetComment(Ĩ, "DoBalance", "Automatically balance the gravity drive?");
        F.Set(Ĩ, "DoRepulse", è.ı);
        F.SetComment(Ĩ, "DoRepulse", "Default value, changed by argument");
        F.Set(Ĩ, "PrecisionMode", è.Ĳ);
        F.SetComment(Ĩ, "PrecisionMode",
            "Should we reduce authority to 1/10th for out of combat maneuvering?");
        F.Set(Ĩ, "PassiveSphericalRange", Ã);
        F.SetComment(Ĩ, "PassiveSphericalRange", "Passive radius for gravity drive");
        F.Set(Ĩ, "RepulseSphericalRange", Û);
        F.SetComment(Ĩ, "RepulseSphericalRange", "Repulse radius for gravity drive");


        F.SetSectionComment(Ĩ, "\n\nArgus Gravity Drive Config.\n\nEDIT HERE:");

        è.ĳ = F.Get(ĩ, "DoAutoScan").ToBoolean(è.ĳ);
        ª = F.Get(ĩ, "MaxScanFrames").ToInt32(ª);
        á = F.Get(ĩ, "ScanPauseDuration").ToInt32(á);

        F.Set(ĩ, "DoAutoScan", è.ĳ);
        F.SetComment(ĩ, "DoAutoScan", "Automatically scan new detected ships?");
        F.Set(ĩ, "MaxScanFrames", ª);
        F.SetComment(ĩ, "MaxScanFrames", "How many times we should scan each category");
        F.Set(ĩ, "ScanPauseDuration", á);
        F.SetComment(ĩ, "ScanPauseDuration", "Pause time between each scan");

        F.SetSectionComment(ĩ, "\n\nArgus Scan Config.\n\nEDIT HERE:");

        è.Ĵ = F.Get(Ī, "DoAim").ToBoolean(è.Ĵ);
        r = F.Get(Ī, "kP").ToDouble(r);
        t = F.Get(Ī, "kI").ToDouble(t);
        s = F.Get(Ī, "kD").ToDouble(s);
        b = F.Get(Ī, "DerivativeNonLinearity").ToDouble(b);
        p = F.Get(Ī, "integralClamp").ToDouble(p);

        F.Set(Ī, "DoAim", è.Ĵ);
        F.SetComment(Ī, "DoAim", "Should we use the aimbot?");
        F.Set(Ī, "kP", r);
        F.SetComment(Ī, "kP", "Proportional gain");
        F.Set(Ī, "kI", t);
        F.SetComment(Ī, "kI", "Integral gain");
        F.Set(Ī, "kD", s);
        F.SetComment(Ī, "kD", "Derivative gain");
        F.Set(Ī, "DerivativeNonLinearity", b);
        F.SetComment(Ī, "DerivativeNonLinearity",
            "Some random crap I made up to stop the ship overshooting due to gyro ramp up");
        F.Set(Ī, "integralClamp", p);
        F.SetComment(Ī, "integralClamp", "Maximum windup for the integral if you are using it");

        F.SetSectionComment(Ī,
            "\n\nArgus Aimbot Config. See script for instructions on tuning.\n\nEDIT HERE:");

        è.ĵ = F.Get(ī, "HighlightMissiles").ToBoolean(è.ĵ);
        è.Ķ = F.Get(ī, "HighlightTarget").ToBoolean(è.Ķ);

        F.Set(ī, "HighlightMissiles", è.ĵ);
        F.SetComment(ī, "HighlightMissiles", "Highlight missiles on the HUD?");
        F.Set(ī, "HighlightTarget", è.Ķ);
        F.SetComment(ī, "HighlightTarget", "Highlight the target blocks on the HUD?");

        F.SetSectionComment(ī, "\n\nArgus HUD Config.\n\nEDIT HERE:");

        List<string> ķ = new List<string>();
        F.GetSections(ķ);

        foreach (var ĸ in ķ)
        {
            if (ĸ.StartsWith("ReloadTrackerDefinition."))
            {
                string Ĺ = ĸ.Replace("ReloadTrackerDefinition.", "");
                    
                double ĺ = F.Get(ĸ, "ShotsPerSecond").ToDouble();
                double Ļ = F.Get(ĸ, "ReloadTime").ToDouble();
                int ļ = F.Get(ĸ, "BurstCount").ToInt32();
                double Ľ = F.Get(ĸ, "Velocity").ToDouble();
                double ľ = F.Get(ĸ, "Range").ToDouble();
                    
                G Ŀ = new G(ĺ, Ļ, ļ, Ľ, ľ);
                if (H.ContainsKey(Ĺ))
                {
                    H[Ĺ] = Ŀ;
                }
                else
                {
                    H.Add(Ĺ, Ŀ);
                }
            }
        }
            
        foreach (var ŀ in H)
        {
            string Ĺ = ŀ.Key;
            G Ł = ŀ.Value;
                
            F.Set("ReloadTrackerDefinition." + Ĺ, "ShotsPerSecond", Ł.ł);
            F.Set("ReloadTrackerDefinition." + Ĺ, "ReloadTime", Ł.Ń);
            F.Set("ReloadTrackerDefinition." + Ĺ, "BurstCount", Ł.ě);
            F.Set("ReloadTrackerDefinition." + Ĺ, "Velocity", Ł.Ĝ);
            F.Set("ReloadTrackerDefinition." + Ĺ, "Range", Ł.ĝ);
        }
            
        Me.CustomData = F.ToString();
    }

    DateTime ń = DateTime.Now;


        
    public void Save()
    {

        string Ņ = Me.GetOwnerFactionTag();
        if (D)
        {
        }
    
    }

    public void Main(string ņ, UpdateType Ň)
    {
        try
        {
            if ((Ň & UpdateType.Update1) != 0) ň();
            if ((Ň & (UpdateType.Trigger | UpdateType.Terminal)) != 0) ŉ(ņ);
        }
        catch (Exception Ŋ)
        {
            ŋ();
            Ō.ō(Me.GetSurface(0), "ArgusLite", ù, Ŋ);
            foreach (var Ĕ in X) Ō.ō(Ĕ, "ArgusLite", ù, Ŋ);
            foreach (var Ŏ in Ğ.ă) Ō.ō(Ŏ, "ArgusLite", ù, Ŋ);
            throw;
        }
    }

    void ŉ(string ņ)
    {
        ņ = ņ.ToLower();
        switch (ņ)
        {
            case "toggle ship aim":
                è.Ĵ = !è.Ĵ;
                Ğ.ŏ($"Ship aim set to {è.Ĵ}");
                break;
            case "toggle acceleration lead":
                è.Ĭ = !è.Ĭ;
                Ğ.ŏ($"Acceleration lead set to {è.Ĭ}");
                break;
            case "toggle auto fire":
                è.ĭ = !è.ĭ;
                Ğ.ŏ($"Auto-fire set to {è.ĭ}");
                break;
            case "toggle volley":
                è.č = !è.č;
                Ğ.ŏ($"Volley set to {è.č}");
                break;
            case "toggle turret aim":
                è.Į = !è.Į;
                if (è.Į == false) Ő(ø);
                Ğ.ŏ($"Turret aim override set to {è.Į}");
                break;
            case "toggle gravity drive":
                è.į = !è.į;
                Ğ.ŏ($"Gravity Drive set to {è.į}");
                break;
            case "toggle auto balance":
                è.İ = !è.İ;
                Ğ.ŏ($"Auto balance set to {è.İ}");
                break;
            case "repulse":
                è.ı = !è.ı;
                Ğ.ŏ($"Repulse set to {è.ı}");
                break;
            case "toggle precision mode":
                è.Ĳ = !è.Ĳ;
                Ğ.ŏ($"Precision mode set to {è.Ĳ}");
                break;
            case "toggle auto scan":
                è.ĳ = !è.ĳ;
                Ğ.ŏ($"Auto scan set to {è.ĳ}");
                break;
            case "toggle full tilt":
                break;
            case "cycle target category":
                Ï++;
                if ((int)Ï == 4) Ï = 0;
                í = Ü.Next(0,
                    K.ő(Ï));
                break;
            case "cycle target type":
                Ò++;
                if ((int)Ò == 2) Ò = 0;
                break;
            case "set random block":
                í = Ü.Next(0,
                    K.ő(Ï));
                Ğ.ŏ(K
                    .Œ(Ï, í).ToString());
                break;
            case "scan":
                Y = true;
                ã = true;
                Ğ.ŏ("Starting scan..");
                break;
            case "unfuck turrets":
                Ő(ø);
                Ğ.ŏ("Attemptnig to unfuck turrets");
                break;
            case "emergency performance":
                è.ĭ = false;
                è.Į = false;
                è.ĳ = false;
                è.İ = false;
                Ğ.ŏ("Performance mode enabled");
                break;
            case "event locked":
                À++;
                è.Ĳ = false;
                break;
            case "event unlocked":
                À = Math.Max(0, À - 1);
                break;
            case "missile launch":
                h.œ(60);
                v.Ŕ(this, å.CenterOfMass, a,
                    å.WorldMatrix.Forward);
                Ğ.ŏ("Missiles launching");
                break;
            case "missile attack":
                v.ŕ();
                Ğ.ŏ("Missiles Attacking");
                break;
            case "hud up":
                Q.Ŗ();
                Ğ.ŏ("Hud up");
                break;
            case "hud down":
                Q.ŗ();
                Ğ.ŏ("Hud down");
                break;
            case "hud select":
                Q.Ř();
                Ğ.ŏ("Hud element selected");
                break;
        }

        if (ņ.Length > 12 && ņ.Substring(0, 12) == "set velocity") ř(ņ);
    }

    void ř(string Ś)
    {
        for (var ĉ = 0; ĉ < Ś.Length; ĉ++)
            if (char.IsDigit(Ś[ĉ]))
            {
                try
                {
                    Ô = float.Parse(Ś.Substring(ĉ));
                }
                catch
                {
                }

                break;
            }
    }


    void ŋ()
    {
        v.ś();
        Ŝ(m);
        h.ŝ(false);
    }


    void Š()
    {
        var Ş = (float)(Runtime.LastRunTimeMs * 1000);
        if (Ş > w) w = Ş;
        Þ.Add(Ş);
        if (Þ.Count > ñ) Þ.RemoveAt(0);
        for (var ĉ = 0;
             ĉ < Þ.Count;
             ĉ++)
        {
            var ş = Þ[ĉ];
            U += ş;
        }

        U /= Þ.Count;
    }

    void Ų()
    {
        var š = 0;
        var Ţ = 0;
        var ţ = 0;
        var Ť = 0;
        var ť = 0;

        if (é.ContainsKey(Ù))
        {
            š = é[Ù].Ŧ;
            Ţ = é[Ù].ŧ;
            ţ = é[Ù].Ũ;
            Ť = é[Ù].ũ;
            ť = é[Ù].Ū;
        }

        ë.Clear().Append("Scan Results");


        for (var ĉ = µ.Count - 1; ĉ >= 0; ĉ--)
        {
            var Ģ = µ[ĉ];
            if (Ģ.Closed)
            {
                µ.RemoveAt(ĉ);
                continue;
            }

            if (!Ģ.DetailedInfo.Contains("Attacking")) continue;
            ë.Clear()
                .Append(
                    Ģ.DetailedInfo.Split(ì, StringSplitOptions.None)[2]);
            break;
        }

        var ŭ = new ū(
            Y,
            U,
            À,
            Ï,
            à,
            Z,
            ª,
            š,
            Ţ,
            ţ,
            Ť,
            ť,
            l.Ŭ,
            ë.ToString(),
            î
        );
        Q.Ů(ŭ, v.ů(), K);

        W.Clear()
            .Append("Argus Info");

        if (À > 0)
            W.Append("\n<<WARN>> Enemy Lock x")
                .Append(À)
                .Append(" <<WARN>>");
        else
            W.Append("\n");
        if (o)
            W.Append("\n").Append(I).Append(" Guns Ready");
        else
            W.Append("\nNo Target");
        W.Append("\n");
        var Ű = W.ToString();
        foreach (var ű in V) ű.HudText = Ű;
    }

    bool ų = false;

    void ň()
    {
            
        L.Ŵ();
            
        M.ŵ(Me.CubeGrid.GetPosition(), Me.CubeGrid.LinearVelocity, Me.CubeGrid.WorldMatrix, å);
            
        Ğ.Ŷ();
            
        ä++;
            
            
        if (ä % º == 0) ŷ();
        if ((ä - 300) % º == 0) Ÿ();


        Š();
        đ();
            
            
        var ź = Ź(ø, ó, ref ï);

        var Ż = ź.EntityId;
        if (Ż != 0)
        {
            Ù = Ż;
            è.Ĳ =
                false;
        }
            

            

        if (Ż != 0 && !Y && !é.ContainsKey(Ż) && è.ĳ)
        {
            ã = true;
            Y = true;
            for (var ĉ = 0; ĉ < ó.Count; ĉ++)
            {
                var Ė = ó[ĉ];
                Ė.SetTargetingGroup("");
            }

            for (var ĉ = 0; ĉ < ø.Count; ĉ++)
            {
                var Ė = ø[ĉ];
                Ė.SetTargetingGroup("");
            }
        }
        else if (Ż == 0 && Y)
        {
            e++;


        }

        ð = ź.Velocity;
        a = å.GetShipVelocities().LinearVelocity;

            
            
            
            
        ż(Ż);
            
        if (!Y && è.Į)
        {
            if (o)
            {
                n = false;
                Ž(ø, î);
            }
            else
            {
                if (!n)
                {
                    Ő(ø);
                    n = true;
                }

                foreach (var Ė in ø) ö[Ė].ŵ(false);
            }
        }

        if (é.ContainsKey(Ż))
        {
            é[Ż].ž = ź.Position;
            é[Ż].ſ =
                J.ƀ(ź);
            switch (Ò)
            {
                case Ð.Ñ:
                    î = é[Ż].Ɓ(Ï);
                    break;
                case Ð.Ƃ:
                    î = é[Ż]
                        .Œ(Ï, í);
                    break;
            }

            K = é[Ż];
        }

        else
        {
            î = ź.Position;
        }
            
        var ƅ = ƃ.Ƅ(ź, î, Me.CubeGrid.WorldVolume.Center, Me.CubeGrid.LinearVelocity,
            Me.CubeGrid, å.GetShipVelocities().AngularVelocity);
            
        l.Ɔ =  è.Č ? k.ċ.Č : è.č ? k.ċ.Ď : k.ċ.ď;
        l.Ƈ(ê, Á);
            
        o = ź.EntityId != 0;
        if (o && è.Ĵ)
        {
            T = Vector3D.Zero;
            l.ƈ();
            I = l.Ɖ();
            T = l.Ɗ(å.CenterOfMass);

            É = x;
            Ê = Õ;
            Ë = Ö;
                
            double Ƌ = a.Dot(å.WorldMatrix.Forward);
            if (Ƌ < 0) Ƌ = 0;

            float ƌ = (float)(Ô - Ƌ);

            x = ƍ.Ǝ(î, ð, T,
                a, ƌ, ò, ref Ì, false,
                è.Ĭ);
            Õ = x - T;
            ê = Õ.Ə();
            Ö =
                Õ / ê;
            Á = Vector3D.Dot(Ö, å.WorldMatrix.Forward);
        }
        else
        {
            Ŝ(m);
        }
            
        Ɛ();
        Ƒ();
            
    
        if ((ä + 2) % 10 == 0) Ų();
        if (è.į == false) return;
            
        if (!o) ƅ = Vector3D.Zero;
            
        h.ŵ(å, è.Ĳ, è.ı, è.İ, ƅ);
            


    }

    void Ɩ()
    {
        while (c.HasPendingMessage)
        {
            Ğ.ŏ("Diagnostic request Received");
            var ƒ = c.AcceptMessage();
            if (ƒ.Data is string)
            {
                var Ɠ = ƒ.Data.ToString();
                switch (Ɠ)
                {
                    case "FlightData":
                        IGC.SendUnicastMessage(ƒ.Source, Me.EntityId.ToString(),
                            ý.Ɣ());
                        break;
                    case "FlightDataReset":
                        IGC.SendUnicastMessage(ƒ.Source, Me.EntityId + "Reset",
                            ý.ƕ());
                        break;
                    case "SWDFlightData":
                        IGC.SendUnicastMessage(ƒ.Source, "SWDFlightData", ý.Ɣ());
                        Ğ.ŏ($"flight data sent to {ƒ.Source} via SWD");
                        break;
                    case "SWDFlightDataReset":
                        IGC.SendUnicastMessage(ƒ.Source, "SWDFlightData",
                            ý.ƕ());
                        break;
                    case "Status":
                        break;
                }
            }
        }
    }

    void ż(long Ż)
    {
        if (ã)
        {
            í = -1;
            Ő(ø);
            é.Remove(Ż);
            ã = false;
            à = Í.Î;
        }

        if (Y) Ɨ();

        if (ß)
        {
            foreach (J Ƙ in é.Values) Ƙ.ƙ();
            foreach (var Ė in ó)
            {
                Ė.SetTargetingGroup("");
                Ė.AIEnabled = true;
            }
            foreach (var Ė in ø) Ė.SetTargetingGroup("");
            Ő(ø);
            ß = false;
            if (é.ContainsKey(Ù))
            {
                v.ƚ(é[Ù]);
                í =
                    ƛ(Ï, é[Ù]);
            }
        }
    }

    int ƛ(Í Ɯ, J Ɲ)
    {
        switch (Ɯ)
        {
            case Í.Î:
                if (Ɲ.Ŧ == 0) return -1;
                return Ü.Next(0, Ɲ.Ŧ - 1);
            case Í.ƞ:
                if (Ɲ.ũ == 0) return -1;
                return Ü.Next(0, Ɲ.ũ - 1);
            case Í.Ɵ:
                if (Ɲ.ŧ == 0) return -1;
                return Ü.Next(0, Ɲ.ŧ - 1);
            case Í.Ơ:
                if (Ɲ.Ũ == 0) return -1;
                return Ü.Next(0, Ɲ.Ũ - 1);
        }

        return -1;
    }

    void ŷ()
    {
        i = GridTerminalSystem.GetBlockGroupWithName(j);

        O.Clear();
        i.GetBlocks(O);
        for (var ĉ = O.Count - 1; ĉ >= 0; ĉ--)
        {
            var Ċ = O[ĉ];
            if (!GridTerminalSystem.CanAccess(Ċ, MyTerminalAccessScope.Construct)) O.RemoveAt(ĉ);
        }

        ø = O.OfType<IMyLargeTurretBase>().ToList();
        bool ĕ = false;
        foreach (var Ė in ø)
            if (!ö.ContainsKey(Ė))
            {
                string ė = Ė.BlockDefinition.ToString();

                if (!H.ContainsKey(ė))
                {
                    H.Add(ė, new G(0, 0, 0, 0, 0));
                        
                    F.Set("ReloadTrackerDefinition." + ė, "ShotsPerSecond", 0);
                    F.Set("ReloadTrackerDefinition." + ė, "ReloadTime", 0);
                    F.Set("ReloadTrackerDefinition." + ė, "BurstCount", 0);
                    F.Set("ReloadTrackerDefinition." + ė, "Velocity", 1);
                    F.Set("ReloadTrackerDefinition." + ė, "Range", 0);
                    ĕ = true;
                }
                    
                G Ę = H[ė];
                ö.Add(Ė, new õ(Ę.ę, Ę.Ě, Ę.ě, Ę.Ĝ, Ę.ĝ));
            }
        if (ĕ) Me.CustomData = F.ToString();

            

        m = O.OfType<IMyGyro>().ToList();
        var Ă = O.OfType<IMyUserControllableGun>().ToList();
        l.ơ(Ă);


    }

    void Ÿ()
    {
        var Ƣ = O.OfType<IMyArtificialMassBlock>().ToList();
        var ƣ = O.OfType<IMySpaceBall>().ToList();
        var Ƥ = O.OfType<IMyGravityGenerator>().ToList();
        var ƥ = O.OfType<IMyGravityGeneratorSphere>().ToList();
        h.Ʀ(Ƣ, ƣ, Ƥ,
            ƥ, å);
    }

    void Ɨ()
    {
        if (â <= á)
        {
            for (var ĉ = ó.Count - 1; ĉ >= 0; ĉ--)
            {
                var Ė = ó[ĉ];
                if (Ė == null)
                {
                    ó.RemoveAt(ĉ);
                    continue;
                }

                Ė.AIEnabled = true;
            }

            â++;
            return;
        }

        â = 1;
        e = 0;
        Ƨ(ø, ó, à);

        Z++;
        Ő(ø);
        for (var ĉ = ó.Count - 1; ĉ >= 0; ĉ--)
        {
            var Ė = ó[ĉ];
            if (Ė == null)
            {
                ó.RemoveAt(ĉ);
                continue;
            }

            Ė.AIEnabled = false;
        }

        if (Z >= ª)
        {
            Z = 0;
            à++;

            if ((int)à >= 4)
            {
                Y = false;
                ß = true;
                    
                    
            }
            else
            {
                var ƪ = ƨ.Ʃ(à);
                if (ƪ == "Default") ƪ = "";
                for (var ĉ = ó.Count - 1; ĉ >= 0; ĉ--)
                {
                    var Ė = ó[ĉ];
                    Ė.SetTargetingGroup(ƪ);
                }

                for (var ĉ = ø.Count - 1; ĉ >= 0; ĉ--)
                {
                    var Ė = ø[ĉ];
                    if (Ė == null)
                    {
                        ø.RemoveAt(ĉ);
                        continue;
                    }

                    Ė.SetTargetingGroup(ƪ);
                }
            }
        }
    }

    MyDetectedEntityInfo Ź(List<IMyLargeTurretBase> ƫ,
        List<IMyTurretControlBlock> Ƭ,
        ref Dictionary<IMyFunctionalBlock, MyDetectedEntityInfo> ƭ)
    {
        for (var ĉ = Ƭ.Count - 1; ĉ >= 0; ĉ--)
        {
            var Ė = Ƭ[ĉ];
            if (Ė == null)
            {
                Ƭ.RemoveAt(ĉ);
                continue;
            }

            var ź = Ė.GetTargetedEntity();
            if (ź.EntityId == 0) continue;
            Ø = Ė;

            return ź;
        }

        for (var ĉ = ƫ.Count - 1; ĉ >= 0; ĉ--)
        {
            var Ė = ƫ[ĉ];
            if (Ė == null)
            {
                ƫ.RemoveAt(ĉ);
                continue;
            }

            var ź = Ė.GetTargetedEntity();
            if (ź.EntityId == 0) continue;
            Ø = Ė;
            if ((ź.Position - å.CenterOfMass).Ʈ() > 2100 * 2100)
                Ư(Ė);
            return ź;
        }

        return new MyDetectedEntityInfo();
    }

    void Ž(List<IMyLargeTurretBase> ƫ, Vector3D î)
    {
        for (var ĉ = 0; ĉ < ƫ.Count; ĉ++)
        {
            var Ė = ƫ[ĉ];
            if (Ė == Ø) continue;
            bool ư = ö[Ė].ŵ(true) && ö[Ė].ĝ > ê;

            Ė.Shoot = ư;
            if (!ư) continue;
            var Ʋ = ö[Ė].Ʊ();


            var ž = Ė.GetPosition();
                
            Echo((Ì - ð).Length().ToString());
            var Ƴ = new Vector3D(Ì.X, Ì.Y,
                Ì.Z);
            var ƴ = ƍ.Ǝ(î, ð, ž, a,
                (float)ö[Ė].Ĝ, ò, ref Ƴ, false, è.Ĭ);
            var Ƶ = (ƴ - ž).Normalized();


            var ƶ = Ė.WorldMatrix.Forward;
            var Ʒ = Ė.WorldMatrix.Up;
            var Ƹ = Ė.WorldMatrix.Right;
            var Å = Math.Asin(Ƶ.Dot(Ʒ));
            var û = -Math.Atan2(Ƶ.Dot(Ƹ), Ƶ.Dot(ƶ));
            Ė.SetManualAzimuthAndElevation((float)û, (float)Å);
            Ė.SyncAzimuth();
            Ė.SyncElevation();
        }
    }

    void Ƨ(List<IMyLargeTurretBase> ƫ, List<IMyTurretControlBlock> Ƭ,
        Í ƹ)
    {
        for (var ĉ = Ƭ.Count - 1; ĉ >= 0; ĉ--)
        {
            var ź = Ƭ[ĉ].GetTargetedEntity();
            ƺ(ź, Ƭ[ĉ], ƹ);
        }

        for (var ĉ = ƫ.Count - 1; ĉ >= 0; ĉ--)
        {
            var ź = ƫ[ĉ].GetTargetedEntity();
            ƺ(ź, ƫ[ĉ], ƹ);
        }
    }

    void ƺ(MyDetectedEntityInfo ź, IMyFunctionalBlock Ė,
        Í ƹ)
    {
        if (ź.EntityId == 0) return;
        if (é.ContainsKey(ź.EntityId))
        {
            var Ƙ = é[ź.EntityId];
            var ƻ = Ƙ.ſ;
            Ƽ(Ƙ, ź, ƻ, Ė,
                ƹ);
        }
        else
        {
            var ƻ = J.ƀ(ź);
            var Ƙ = new J(ź.EntityId, ƻ,
                ź.Position, (ulong)d);
            é.Add(ź.EntityId, Ƙ);
            Ƽ(Ƙ, ź, ƻ, Ė,
                ƹ);
        }
    }


    void Ƽ(J ƽ, MyDetectedEntityInfo ƾ,
        MatrixD ƿ, IMyFunctionalBlock Ė, Í ƪ)
    {
        var ǀ = ƾ.Position;

        var ǁ = (Vector3D)ƾ.HitPosition;

        var ǂ = ǁ - ǀ;

        var ǃ = Vector3D.TransformNormal(ǂ, MatrixD.Transpose(ƿ));
        var Ǆ = new Vector3I(
            (int)((ǃ.X + 0.5) / 2.5),
            (int)((ǃ.Y + 0.5) / 2.5),
            (int)((ǃ.Z + 0.5) / 2.5)
        );

        if (ƽ.ǅ(Ǆ, ƪ))
            return;
        var Ǉ = new ǆ(ƪ, Ǆ);
        ƽ.ǈ.Add(Ǉ);
    }

    void đ()
    {
        if (æ.Count == 0) return;
        å = æ.First();
        for (var ĉ = æ.Count - 1; ĉ >= 0; ĉ--)
        {
            if (æ[ĉ] == null)
            {
                æ.RemoveAt(ĉ);
                continue;
            }

            if (æ[ĉ].IsUnderControl)
            {
                if (!Æ) ý.ǉ();
                Æ = true;
                å = æ[ĉ];
                break;
            }

            if (Æ) ý.Ǌ();
            Æ = false;
        }

        ƍ.ǋ = å;
    }

    void Ɛ()
    {
        var ǌ = å.GetShipVelocities().AngularVelocity;
        ǌ = Vector3D.TransformNormal(ǌ, MatrixD.Transpose(å.WorldMatrix));
        ǌ.Z = 0;
        var Ǎ = ǌ.Ə() / Math.PI * y;
        if (è.ĭ)
        {
            if (o)
            {

                var ǎ = å.WorldMatrix.Forward;
                if (ê < Ó && Á > N &&
                    !Y && Ǎ < z)
                    l.Ǐ();
                else
                    l.ǐ();
            }
            else
            {
                l.Ǒ();
            }
        }
    }


    void Ƒ()
    {
        if (o && è.Ĵ)
        {
            double Ý = å.RollIndicator;
            ǒ();
            Ǔ(Ö, Ý, Á);
        }
    }

    void ǒ()
    {
        if (å == null) return;
        var ǔ = å.GetShipVelocities().AngularVelocity;
        ǔ =
            Vector3D.TransformNormal(ǔ, MatrixD.Transpose(å.WorldMatrix));
        ǔ.Z = 0;
        ǔ *= y;
        var ǖ = Ǖ() * y;
        ǖ.Z = 0;
        var Ǘ = (1 - ǔ.Normalized().Dot(ǖ.Normalized())) *
                  Math.Abs((ǔ + ǖ).Ə());
        var ǘ = Math.Pow(Ǘ, b);
        if (double.IsNaN(ǘ) || double.IsInfinity(ǘ) || o == false) ǘ = 10;


        Å.Ǚ = s + ǘ * s;
        û.Ǚ = s + ǘ * s;
    }

    Vector3D Ǖ()
    {
        var ǚ = Vector3D.Cross(Ö, Ë);
        var Ǜ = ǚ.Ə();
        var ǜ = Ǜ;
        var ǝ = ǜ / ò;
        var Ǟ = (Ö - Ë).Normalized();
        var ǟ = Ǟ * ǝ;
        ǟ = Vector3D.TransformNormal(ǟ, MatrixD.Transpose(å.WorldMatrix));
        var Ǡ = ǟ.X;
        ǟ.X = ǟ.Y;
        ǟ.Y = -Ǡ;
        ǟ = (ǟ + Ç) / 2;
        Ç = ǟ;
        return ǟ;
    }


    void Ǔ(Vector3D ǡ, double Ý, double Ǣ)
    {
        int ǣ = 7;
        double Ǥ = 1.0;
        if (Ǣ > 0.9999)
        {
            Ǥ *= 0.8;
            ǣ = 4;
        }

        if (Ǣ > 0.99999)
        {
            Ǥ *= 0.8;
            ǣ = 3;
        }
        if (Ǣ > 0.999999)
        {
            Ǥ *= 0.8;
            ǣ = 2;
        }
        if (Ǣ > 0.9999999)
        {
            Ǥ *= 0.8;
            ǣ = 1;
        }
            
        double ǥ;
        double Ǧ;
        var ǧ = Ý;


        var Ǩ = Vector3D.Cross(å.WorldMatrix.Forward, ǡ);
        var ǩ = Vector3D.TransformNormal(Ǩ, MatrixD.Transpose(å.WorldMatrix));
        var ǫ = Å.Ǫ(-ǩ.X, ǣ);
        var Ǭ = û.Ǫ(-ǩ.Y, ǣ);

        ǥ = MathHelper.Clamp(ǫ, -y, y);
        Ǧ = MathHelper.Clamp(Ǭ, -y, y);

        if (Math.Abs(Ǧ) + Math.Abs(ǥ) > y)
        {
            var ǭ = y / (Math.Abs(Ǧ) + Math.Abs(ǥ));
            Ǧ *= ǭ;
            ǥ *= ǭ;
        }
        ǥ *= Ǥ;
        Ǧ *= Ǥ;
        Ǯ(ǥ, Ǧ, ǧ, m, å.WorldMatrix);
    }


    void Ǯ(double ǯ, double ǰ, double Ǳ, List<IMyGyro> ǲ,
        MatrixD ǳ)
    {
        var Ǵ = new Vector3D(ǯ, ǰ, Ǳ);
        var ǵ = Vector3D.TransformNormal(Ǵ, ǳ);

        foreach (var Ƕ in ǲ)
            if (Ƕ.IsFunctional && Ƕ.IsWorking && Ƕ.Enabled && !Ƕ.Closed)
            {
                var Ƿ =
                    Vector3D.TransformNormal(ǵ, MatrixD.Transpose(Ƕ.WorldMatrix));
                Ƕ.Pitch = (float)Ƿ.X;
                Ƕ.Yaw = (float)Ƿ.Y;
                Ƕ.Roll = (float)Ƿ.Z;
                Ƕ.GyroOverride = true;
                return;
            }
    }

    void Ŝ(List<IMyGyro> ǲ)
    {
        foreach (var Ƕ in ǲ)
            if (Ƕ.IsFunctional && Ƕ.IsWorking && Ƕ.Enabled && !Ƕ.Closed)
            {
                Ƕ.GyroOverride = false;
                return;
            }
    }


    public void Ő(List<IMyLargeTurretBase> ƫ)
    {
        foreach (var Ė in ƫ) Ư(Ė);
    }

    public static void Ư(IMyLargeTurretBase Ė)
    {
        var Ǹ = Ė.Enabled;
        var ǹ = Ė.TargetMeteors;
        var Ǻ = Ė.TargetMissiles;
        var ǻ = Ė.TargetCharacters;
        var Ǽ = Ė.TargetSmallGrids;
        var ǽ = Ė.TargetLargeGrids;
        var Ǿ = Ė.TargetStations;
        var ľ = Ė.Range;
        var ǿ = Ė.EnableIdleRotation;

        Ė.ResetTargetingToDefault();

        Ė.Enabled = Ǹ;
        Ė.TargetMeteors = ǹ;
        Ė.TargetMissiles = Ǻ;
        Ė.TargetCharacters = ǻ;
        Ė.TargetSmallGrids = Ǽ;
        Ė.TargetLargeGrids = ǽ;
        Ė.TargetStations = Ǿ;
        Ė.Range = ľ;
        Ė.EnableIdleRotation = ǿ;
    }
}
public static class Ğ
{
    public static List<IMyTextPanel> ă;
    public static MyGridProgram ā;

    public static StringBuilder Ȁ = new StringBuilder();

    public static void ğ(List<IMyTextPanel> ă)
    {
        Ğ.ă = ă;
        foreach (var Ŏ in ă) Ŏ.ContentType = ContentType.TEXT_AND_IMAGE;
    }

    public static void ŏ(string ȁ)
    {
        Ȁ.AppendLine(ȁ);
    }

    public static void Ŷ()
    {
        var ȁ = Ȁ.ToString();
        ā.Echo(ȁ);
    }

    public static void Ȅ(object Ȃ)
    {
        string ȃ = Ȃ?.ToString();
        ā.Echo(ȃ);
    }
}
public static class ƨ
{
    public static string Ʃ(Í ƪ)
    {
        switch (ƪ)
        {
            case Í.Î:
                return "Default";
            case Í.Ɵ:
                return "Weapons";
            case Í.ƞ:
                return "Propulsion";
            case Í.Ơ:
                return "PowerSystems";
            default:
                return "Default";
        }
    }

    public static Í ȅ(string Ĺ)
    {
        switch (Ĺ)
        {
            case "Default":
                return Í.Î;
            case "Weapons":
                return Í.Ɵ;
            case "Propulsion":
                return Í.ƞ;
            case "PowerSystems":
                return Í.Ơ;
            default:
                return Í.Î;
        }
    }
}
public static class ȍ
{
    public static Vector3D ȉ(this Vector3D Ȇ, Vector3D ȇ)
    {
        var Ȉ = 1.0 / Math.Sqrt(ȇ.X * ȇ.X + ȇ.Y * ȇ.Y + ȇ.Z * ȇ.Z);

        Ȇ.X = ȇ.X * Ȉ;
        Ȇ.Y = ȇ.Y * Ȉ;
        Ȇ.Z = ȇ.Z * Ȉ;
        return Ȇ;
    }


    public static double Ə(this Vector3D Ȇ)
    {
        return Ȇ.Length();
    }

    public static double Ʈ(this Vector3D Ȇ)
    {
        return Ȇ.LengthSquared();
    }


    public static StringBuilder ȋ(this StringBuilder Ȁ)
    {

        var Ȋ = Ȁ.Length - 1;
        while (Ȋ >= 0 && char.IsWhiteSpace(Ȁ[Ȋ]))
            Ȋ--;
        Ȁ.Length = Ȋ + 1;
        return Ȁ;
    }

    public static StringBuilder Ȍ(this StringBuilder Ȁ)
    {

        for (var ĉ = 0; ĉ < Ȁ.Length; ĉ++)
            if (!char.IsDigit(Ȁ[ĉ]))
            {
                Ȁ.Remove(ĉ, 1);
                ĉ--;
            }

        return Ȁ;
    }
}
public struct ū
{
    public bool Ȏ;
    public float ȏ;
    public int À;
    public Í Ȑ;
    public Í ȑ;


    public int Ȓ;
    public int ȓ;
    public int Ȕ;
    public int ȕ;
    public int Ȗ;
    public int ȗ;
    public int Ș;

    public List<float> ș;
    public string Ț;
    public Vector3D î;

    public ū(
        bool ț,
        float Ş,
        int Ȝ,
        Í ȝ,
        Í Ȟ,
        int ȟ,
        int Ƞ,
        int ȡ,
        int Ȣ,
        int ȣ,
        int Ȥ,
        int ȥ,
        List<float> Ȧ,
        string ȧ,
        Vector3D Ȩ
    )
    {
        Ȏ = ț;
        ȏ = Ş;
        À = Ȝ;
        Ȑ = ȝ;
        ȑ = Ȟ;


        Ȓ = ȟ;
        ȓ = Ƞ;
        Ȕ = ȡ;
        ȕ = Ȣ;
        Ȗ = ȣ;
        ȗ = Ȥ;
        Ș = ȥ;

        ș = Ȧ;
        Ț = ȧ;
        î = Ȩ;
    }
}
internal class ɧ
{
    private readonly Vector2 ȩ = new Vector2(-70, 0);
    private readonly int Ȫ = 1;
    private readonly int ȫ = 15;

    public Vector2[] Ȭ;
    public Vector2 ȭ = new Vector2(-20, 0);

    private readonly Vector2 Ȯ = new Vector2(0, -20);
    private readonly Vector2 ȯ = new Vector2(0, 20);
    public IMyCameraBlock Ȱ;
    public Vector2 ȱ = new Vector2(11, 11);
    public Vector2 Ȳ = new Vector2(8, 8);


    public Vector2 ȳ = new Vector2(15, 15);
    public bool ȴ;


    public Vector2 ȵ;
    public Vector2 ȶ;

    private readonly Vector2 ȷ = new Vector2(70, 50);
    public Color ȸ = new Color(80, 0, 0);

    public Vector2 ȹ = new Vector2(-180, 20);

    public Vector2 Ⱥ;
    public Color Ȼ = Color.Orange;
    public Vector2 ȼ = new Vector2(100, 3);

    public Vector2 Ƚ = new Vector2(0, 3);
    public Vector2 Ⱦ = new Vector2(3, 3);
    public int ȿ = 16;
    private readonly Vector2 ɀ = new Vector2(-100, 20);
    public Vector2 Ɂ = new Vector2(0, 20);
    public Vector2 ɂ;
    public float Ƀ = 3;


    public Color Ʉ = Color.CornflowerBlue;
    public Color Ʌ = new Color(0, 80, 0);
    public Vector2 Ɇ = new Vector2(1, 1);

    public Vector2 ɇ = new Vector2(3, 3);
    public Vector2 Ɉ = new Vector2(11, 11);

    private readonly Vector2 ɉ = new Vector2(0, 20);
    public Vector2 Ɋ;

    private readonly Vector2 ɋ = new Vector2(-10, -20);
    public Vector2 Ɍ;
    public Vector2 ɍ;


    private readonly Vector2 Ɏ = new Vector2(20, -100);


    public Vector2 ɏ;
    public Vector2 ɐ = new Vector2(200, 30);
    public Vector2 ɑ;
    public float ɒ = 0.8f;
    private readonly Vector2 ɓ = new Vector2(0, 30);
    public Vector2 ɔ;


    private readonly Vector2 ɕ = new Vector2(20, -100);
    Vector2 ɖ = new Vector2(150, 25);
    public float ɗ = 0.6f;
    private readonly Vector2 ɘ = new Vector2(0, 15);
    public Vector2 ə;
    public Vector2 ɚ;
    private readonly Vector2 ɛ = new Vector2(10, -20);

    public float ɜ = 0.7f;
    public Vector2 ɝ;
    public Vector2 ɞ;
    private readonly Vector2 ɟ = new Vector2(22, -20);

    public float ɠ = 1.2f;
    public IMyTextSurface ɡ;

    public Vector2 ɢ;

    public Vector2 ɣ;

    public string ù;


    public Vector2 ɤ;
    public RectangleF ɥ;


    public ɧ(IMyTextSurface ɡ, RectangleF ɥ, IMyCameraBlock Ȱ, string ù)
    {
        this.ɡ = ɡ;
        this.ɥ = ɥ;


        if (Ȱ != null)
        {
            ȴ = true;
            this.Ȱ = Ȱ;
        }

        ɤ = new Vector2(ɥ.Position.X + ɥ.Width / 2,
            ɥ.Position.Y + ɥ.Height - 20);
        this.ù = ù;

        Ɍ = ɥ.Size + ɋ + ɥ.Position;
        ȶ = ɥ.Position + ȷ;
        Ɋ = ɥ.Position + ɉ;

        ɏ = new Vector2(0, ɥ.Height) + Ɏ;
        ɑ = ɏ + ɟ;
        ɍ = ɑ + ɓ * 2;
        ɞ = ɍ + ɓ;


        ɣ = new Vector2(ɥ.Position.X + ɥ.Width / 2,
            ɥ.Position.Y + ɥ.Height - 50);


        ɢ = new Vector2(0, ɥ.Height) + ɕ + ɛ;
        ɔ = ɢ + ɘ * 2;
        ɝ = ɔ + ɘ;
        ə = ɝ + ɘ;
        ɚ = ə + ɘ;

        ȵ = ɥ.Size + Ȯ + ɥ.Position +
            ȩ * Ȫ - ȫ * ȯ;

        Ȭ = new Vector2[ȫ * Ȫ];
        for (var ĉ = 0; ĉ < Ȫ; ĉ++)
        for (var ɦ = 0; ɦ < ȫ; ɦ++)
            Ȭ[ĉ * ȫ + ɦ] =
                ȵ + (ɦ + 1) * ȯ - ĉ * ȩ;

        ɂ = new Vector2(ɥ.Width, 0) + ɀ + ɥ.Position;

        Ⱥ = new Vector2(ɥ.Width, 0) + ȹ + ɥ.Position;
    }
}
internal class P
{
    private static readonly string ɨ = "White";
    private readonly float ɩ = 90;

    public ushort ɪ;
    private readonly float ɫ = 0.6f;

    private readonly Dictionary<IMyTextSurface, Dictionary<long, Vector2>> ɬ =
        new Dictionary<IMyTextSurface, Dictionary<long, Vector2>>();

    private readonly float ɭ = 14f;

    public static readonly ū ɮ = new ū(true, 0f, 0, Í.Î,
        Í.Î, 0, 0, 0, 0, 0, 0, 0, new List<float>(), "", Vector3D.Zero);

    private readonly Color ɯ = new Color(80, 0, 0);


    private readonly List<ɧ> ɰ;

    int ɱ;
    private readonly int ɲ = 30;

    private readonly float ɳ = 1;

    private readonly Dictionary<IMyTextSurface, Dictionary<long, Vector2>> ɴ =
        new Dictionary<IMyTextSurface, Dictionary<long, Vector2>>();

    private readonly Vector2 ɵ = new Vector2(200, 1);

    private readonly Vector2 ɶ = new Vector2(0, 1);
        
    private readonly int ɷ = 1;
    public ç è;


    private readonly Color ɸ = new Color(0, 80, 0);

    public P(List<IMyTextSurface> ɹ, string ù, List<IMyCameraBlock> ɺ,
        ref ç è)
    {
        this.è = è;
        ɰ = new List<ɧ>();
        foreach (var ɡ in ɹ)
        {
            var ɥ = new RectangleF(
                (ɡ.TextureSize - ɡ.SurfaceSize) / 2f,
                ɡ.SurfaceSize
            );
            ɡ.ContentType = ContentType.SCRIPT;
            ɡ.BackgroundColor = Color.Black;


            IMyCameraBlock Ȱ = null;
            var Ŏ = ɡ as IMyTextPanel;
            if (Ŏ != null)
                foreach (var ɻ in ɺ)
                {
                    if (ɻ.CustomData == "") continue;
                    if (ɻ.CustomData == Ŏ.CustomData) Ȱ = ɻ;
                }
                

            var ɼ = new ɧ(ɡ, ɥ, Ȱ, ù);
            ɰ.Add(ɼ);

            ɴ.Add(ɡ, new Dictionary<long, Vector2>());
            ɬ.Add(ɡ, new Dictionary<long, Vector2>());

            ɽ(ɼ, ɮ, new List<u.ɾ>(), J.Î);
        }
    }

    public void Ŗ()
    {
        ɪ--;
        if (ɪ >= ɰ.ElementAt(0).Ȭ.Length)
            ɪ = (ushort)(ɰ.ElementAt(0).Ȭ.Length - 1);
    }

    public void ŗ()
    {
        ɪ++;
        if (ɪ >= ɰ.ElementAt(0).Ȭ.Length) ɪ = 0;
    }

    public void Ř()
    {
        var ʀ = è.ɿ();

        if (ʀ.Length <= ɪ) return;
        ʀ[ɪ] = !ʀ[ɪ];
        è.ʁ(ʀ);
    }

    public void Ů(ū ŭ, List<u.ɾ> ʂ,
        J ʃ)
    {
        ɱ++;
        for (var ĉ = 0; ĉ < ɰ.Count; ĉ++)
        {
            var ɼ = ɰ.ElementAt(ĉ);
            ɽ(ɼ, ŭ, ʂ, ʃ);
        }
    }

    void ɽ(ɧ ɼ, ū ŭ, List<u.ɾ> ʂ,
        J ʃ)
    {
        var d = ɼ.ɡ.DrawFrame();
        ʄ(ref d, ɼ, ŭ, ʂ, ʃ);
        d.Dispose();
    }


    public string ʅ(Í ƪ)
    {
        switch (ƪ)
        {
            case Í.Î:
                return "All Blocks";
            case Í.Ɵ:
                return "Weapons";
            case Í.ƞ:
                return "Propulsion";
            case Í.Ơ:
                return "Power Systems";
        }

        return "fuck you";
    }

    public void ʄ(ref MySpriteDrawFrame d, ɧ ɼ, ū ŭ,
        List<u.ɾ> ʂ, J ʃ)
    {
        if (ɱ > ɲ)
        {
            ɱ = 0;
            var ʆ = new MySprite();
            d.Add(ʆ);
        }

        ʇ(ref d, ɼ, è);
        ʈ(ref d, ɼ, ŭ);


        var ʉ = new MySprite
        {
            Type = SpriteType.TEXTURE,
            Data = "SquareSimple",
            Position = ɼ.Ɍ,
            Size = new Vector2(200, 30),
            Color = Color.White,
            Alignment = TextAlignment.RIGHT
        };
        var ʊ = new MySprite
        {
            Type = SpriteType.TEXT,
            Data = ŭ.ȏ.ToString("0.0"),
            Position = ɼ.Ɍ,
            RotationOrScale = 0.8f,
            Color = Color.OrangeRed,
            Alignment = TextAlignment.RIGHT,
            FontId = ɨ
        };
        d.Add(ʊ);


        if (ŭ.À > 0)
        {
            var ʋ = new MySprite
            {
                Type = SpriteType.TEXT,
                Data = "<<Warning: Enemy lock x" + ŭ.À + ">>",
                Position = ɼ.ȶ,
                RotationOrScale = 1.0f,
                Color = Color.Orange,
                Alignment = TextAlignment.LEFT,
                FontId = ɨ
            };
            d.Add(ʋ);
        }

        var Ɋ = ɼ.Ɋ;
            
            
        for (var ĉ = 0; ĉ < ŭ.ș.Count; ĉ++)
        {
            ʌ(ref d, Ɋ, true, ŭ.ș[ĉ]);
            Ɋ.Y += ɷ;
        }

        ʍ(ref d, ɼ);

        if (è.Ķ) ʎ(ref d, ʃ, ɼ);


        int ʏ = 0;
        int ʐ = 0;
        foreach (u.ɾ ʑ in ʂ)
        {
            if (ʑ.ʒ) 
                ʐ++;
            else ʏ++;
        }
            
            
        if (è.ĵ)
        {
            if (ʂ.Count == 0)
            {
                var ʓ = new MySprite
                {
                    Type = SpriteType.TEXT,
                    Data = "0 / 0",
                    Position = ɼ.ɂ,
                    RotationOrScale = 0.8f,
                    Color = Color.White,
                    Alignment = TextAlignment.LEFT,
                    FontId = ɨ
                };
                d.Add(ʓ);
            }
            else
            {
                var ʓ = new MySprite
                {
                    Type = SpriteType.TEXT,
                    Data = ʐ + " / " + ʏ,
                    Position = ɼ.Ⱥ,
                    RotationOrScale = 0.8f,
                    Color = Color.White,
                    Alignment = TextAlignment.LEFT,
                    FontId = ɨ
                };
                d.Add(ʓ);
            }

            ʔ(ref d, ɼ, ʂ, ŭ.î, ŭ.Ȑ);
        }
        else
        {
            var ʓ = new MySprite
            {
                Type = SpriteType.TEXT,
                Data = ʂ.Count + " / 20",
                Position = ɼ.ɂ,
                RotationOrScale = 0.8f,
                Color = Color.White,
                Alignment = TextAlignment.LEFT,
                FontId = ɨ
            };
            d.Add(ʓ);
        }
    }

    void ʎ(ref MySpriteDrawFrame d, J ʃ, ɧ ɼ)
    {
        var ʕ = new Vector2(200, 200);

        for (var ĉ = 0; ĉ < ʃ.ǈ.Count; ĉ++)
        {
            var Ċ = ʃ.ǈ[ĉ];

            var Ŏ = ɼ.ɡ as IMyTextPanel;
            if (Ŏ == null) return;
            if (ɼ.Ȱ == null) return;
            var ʖ = new ʖ(ɼ.Ȱ, Ŏ);

            var ʘ = ʃ.ʗ(ĉ);
            var ʚ = Vector3D.Distance(ʘ, ʖ.ʙ);
            var ʛ = (float)MathHelper.Clamp(
                MathHelper.InterpLog((float)(2000 - ʚ) / 2000, 0.5f, 4),
                1,
                double.MaxValue
            );
            Vector2 ʜ;

            var ʞ =
                ʝ(ʘ, ʖ, ɼ.Ȱ, Ŏ, out ʜ);
            if (!ʞ) continue;

            var ʟ = Color.White;
            switch (Ċ.ƪ)
            {
                case Í.Î:
                    ʟ = Color.White;
                    break;
                case Í.Ɵ:
                    ʟ = Color.Red;
                    break;
                case Í.ƞ:
                    ʟ = Color.Yellow;
                    break;
                case Í.Ơ:
                    ʟ = Color.Blue;
                    break;
            }


            var ʠ = new MySprite
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Position = ʜ,
                Size = new Vector2(ʛ, ʛ),
                Color = ʟ,
                Alignment = TextAlignment.LEFT
            };
            d.Add(ʠ);
        }
    }


    void ʍ(ref MySpriteDrawFrame d, ɧ ɼ)
    {
        var ʡ = new MySprite
        {
            Type = SpriteType.TEXT,
            Data = ɼ.ù,
            Position = ɼ.ɤ,
            RotationOrScale = 0.7f,
            Color = Color.White,
            Alignment = TextAlignment.CENTER,
            FontId = ɨ
        };
        d.Add(ʡ);
    }

    void ʔ(ref MySpriteDrawFrame d, ɧ ɼ,
        List<u.ɾ> ʂ, Vector3D ʢ, Í ƪ)
    {
        var Ŏ = ɼ.ɡ as IMyTextPanel;
        if (Ŏ == null) return;
        if (ɼ.Ȱ == null) return;
        var ʖ = new ʖ(ɼ.Ȱ, Ŏ);


        Vector2 ʜ;

        var ʞ
            = ʝ(ʢ, ʖ, ɼ.Ȱ, Ŏ, out ʜ)
              && ʜ.X > ɼ.ɥ.Position.X
              && ʜ.Y > ɼ.ɥ.Position.Y
              && ʜ.X < ɼ.ɥ.Position.X + ɼ.ɥ.Width
              && ʜ.Y < ɼ.ɥ.Position.Y + ɼ.ɥ.Height;



        bool ʣ = ʂ.Count > 60;
        var ɂ = ɼ.ɂ;
        for (var ĉ = 0; ĉ < ʂ.Count; ĉ++)
        {
            ɂ.Y += ɼ.Ƀ;
            var ʑ = ʂ[ĉ];
            Vector2 ʤ;

            var ʥ = ɼ.ȸ;

            var ʧ = ʑ.ʒ
                ? Color.Lerp(ɼ.Ȼ, ɼ.Ʌ, (float)ʑ.ʦ)
                : ɼ.Ʉ;
                


            if (ʝ(ʑ.ž, ʖ, ɼ.Ȱ, Ŏ,
                    out ʤ))
            {
                ɬ[ɼ.ɡ].Add(ʑ.ʨ, ʤ);
                var ʩ = ʤ;
                    

                if (ɴ[ɼ.ɡ].ContainsKey(ʑ.ʨ))
                    ʩ = Vector2.Lerp(ɴ[ɼ.ɡ][ʑ.ʨ], ʤ, 1.5f);



                var ʪ = new MySprite
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Position = ʩ,
                    Size = ɼ.ɇ,
                    Color = Color.Lime,
                    Alignment = TextAlignment.CENTER
                };
                d.Add(ʪ);
                var ʫ = new MySprite
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Position = ʩ,
                    Size = ɼ.Ɇ,
                    Color = Color.Black,
                    Alignment = TextAlignment.CENTER
                };
                d.Add(ʫ);
                    
            }

            if (!ʣ)
                    
                ʌ(ref d, ɂ, ɼ.Ƚ,
                    ɼ.ȼ, ʧ, ʥ, true, (float)ʑ.ʦ, true);
        }

        if (ʣ)
        {
            Vector2 ʬ = ɼ.ɂ;
            int ʭ = 0;
            int ʮ = 0;
            for (var ĉ = 0; ĉ < ʂ.Count; ĉ++)
            {
                var ʑ = ʂ[ĉ];
                var ʯ = ʑ.ʦ;

                var ʰ = Color.White;
                var ʱ = Color.Blue;
                if (ʑ.ʒ)
                {
                    ʰ = ɸ;
                    ʱ = ɯ;
                }
                Vector2 ʲ = ʬ + new Vector2(ʭ * ɼ.Ƀ * 2, ʮ * ɼ.Ƀ * 2);
                    
                ʳ(ref d, ʲ, new Vector2(3, 3), (float)ʯ, ʰ, ʱ);
                ʭ++;
                if (ʭ >= ɼ.ȿ)
                {
                    ʭ = 0; 
                    ʮ++;
                }
            }
        }


        var Ǡ = ɬ[ɼ.ɡ];
        ɬ[ɼ.ɡ] = ɴ[ɼ.ɡ];
        ɴ[ɼ.ɡ] = Ǡ;
        ɬ[ɼ.ɡ].Clear();
    }

    void ʈ(ref MySpriteDrawFrame d, ɧ ɼ, ū ŭ)
    {
        if (ŭ.Ȏ)
            ʴ(ref d, ɼ, ŭ);
        else
            ʵ(ref d, ɼ, ŭ);
    }


    void ʴ(ref MySpriteDrawFrame d, ɧ ɼ, ū ŭ)
    {
        var ʶ = new MySprite
        {
            Type = SpriteType.TEXTURE,
            Data = "SquareSimple",
            Position = ɼ.ɏ,
            Size = ɼ.ɐ,
            Color = Color.LightBlue,
            Alignment = TextAlignment.LEFT
        };
        d.Add(ʶ);
        var ʷ = new MySprite
        {
            Type = SpriteType.TEXT,
            Data = "Scanning...",
            RotationOrScale = ɼ.ɠ,
            Position = ɼ.ɑ,

            Color = Color.Black,
            Alignment = TextAlignment.LEFT,
            FontId = ɨ
        };
        d.Add(ʷ);
        var ʸ = new MySprite
        {
            Type = SpriteType.TEXT,
            Data = "Category: " + ʅ(ŭ.ȑ),
            RotationOrScale = ɼ.ɒ,
            Position = ɼ.ɍ,
            Color = Color.White,
            Alignment = TextAlignment.LEFT,
            FontId = ɨ
        };
        d.Add(ʸ);
        var ʹ = new MySprite
        {
            Type = SpriteType.TEXT,
            Data = "Step " + (ŭ.Ȓ + 1) + "/" + ŭ.ȓ,
            RotationOrScale = ɼ.ɒ,
            Position = ɼ.ɞ,
            Color = Color.White,
            Alignment = TextAlignment.LEFT,
            FontId = ɨ
        };
        d.Add(ʹ);
    }


    void ʵ(ref MySpriteDrawFrame d, ɧ ɼ, ū ŭ)
    {
        var ʺ = new MySprite
        {
            Type = SpriteType.TEXT,
            Data = "Targeting: " + ʅ(ŭ.Ȑ),
            RotationOrScale = ɼ.ɜ,
            Position = ɼ.ɣ,
            Color = Color.CornflowerBlue,
            Alignment = TextAlignment.CENTER,
            FontId = ɨ
        };
        d.Add(ʺ);


        var ʻ = new MySprite
        {
            Type = SpriteType.TEXT,
            Data = ŭ.Ț,
            RotationOrScale = ɼ.ɜ,
            Position = ɼ.ɢ,

            Color = Color.Red,
            Alignment = TextAlignment.LEFT,
            FontId = ɨ
        };
        d.Add(ʻ);
        var ʼ = new MySprite
        {
            Type = SpriteType.TEXT,
            Data = "All: " + ŭ.Ȕ,
            RotationOrScale = ɼ.ɗ,
            Position = ɼ.ɔ,
            Color = Color.White,
            Alignment = TextAlignment.LEFT,
            FontId = ɨ
        };
        d.Add(ʼ);
        var ʽ = new MySprite
        {
            Type = SpriteType.TEXT,
            Data = "Weapons: " + ŭ.ȕ,
            RotationOrScale = ɼ.ɗ,
            Position = ɼ.ɝ,
            Color = Color.White,
            Alignment = TextAlignment.LEFT,
            FontId = ɨ
        };
        d.Add(ʽ);
        var ʾ = new MySprite
        {
            Type = SpriteType.TEXT,
            Data = "Power: " + ŭ.Ȗ,
            RotationOrScale = ɼ.ɗ,
            Position = ɼ.ə,
            Color = Color.White,
            Alignment = TextAlignment.LEFT,
            FontId = ɨ
        };
        d.Add(ʾ);
        var ʿ = new MySprite
        {
            Type = SpriteType.TEXT,
            Data = "Propulsion: " + ŭ.ȗ,
            RotationOrScale = ɼ.ɗ,
            Position = ɼ.ɚ,
            Color = Color.White,
            Alignment = TextAlignment.LEFT,
            FontId = ɨ
        };
        d.Add(ʿ);
    }


    void ʇ(ref MySpriteDrawFrame d, ɧ ɼ, ç è)
    {
        var ʀ = è.ˀ();

        for (var ĉ = 0; ĉ < ʀ.Count; ĉ++)
            ˁ(ref d, ɼ.Ȭ[ĉ], ʀ.ElementAt(ĉ).Value, ʀ.ElementAt(ĉ).Key);

        var ˆ = new MySprite
        {
            Type = SpriteType.TEXTURE,
            Data = "Arrow",
            Position = ɼ.Ȭ[ɪ] + ɼ.ȭ,
            Size = new Vector2(20, 20),
            RotationOrScale = 1.570796f,
            Color = Color.White,
            Alignment = TextAlignment.LEFT
        };
        d.Add(ˆ);
    }


    void ˑ(ref MySpriteDrawFrame d, Vector2 ˇ, Vector2 ˈ)
    {
        var ʟ = Color.CornflowerBlue;

        var ˉ = new Vector2(ˇ.X, ˈ.Y);

        var ˊ = ˈ.X < ˉ.X ? ˈ : ˉ;
        var ˋ = new Vector2(Math.Abs(ˈ.X - ˉ.X), ɳ);

        var ˌ = ˉ - ˇ;

        var ˍ = ˇ.Y < ˉ.Y ? ˇ : ˉ;
        ˍ.Y += Math.Abs(ˌ.Y / 2);
        var ˎ = new Vector2(ɳ, Math.Abs(ˌ.Y));

        var ˏ = new MySprite
        {
            Type = SpriteType.TEXTURE,
            Data = "SquareSimple",
            Position = ˊ,
            Size = ˋ,
            Color = ʟ,
            Alignment = TextAlignment.LEFT
        };

        var ː = new MySprite
        {
            Type = SpriteType.TEXTURE,
            Data = "SquareSimple",
            Position = ˍ,
            Size = ˎ,
            Color = ʟ,
            Alignment = TextAlignment.LEFT
        };


        d.Add(ˏ);
        d.Add(ː);
    }

    void ˁ(ref MySpriteDrawFrame d, Vector2 ʬ, bool Ʋ, string ȁ)
    {
        var ʟ = Ʋ ? ɸ : ɯ;

        var ˠ = Math.Max(ɫ * ɭ * ȁ.Length, ɩ);
        var ʶ = new MySprite
        {
            Type = SpriteType.TEXTURE,
            Data = "SquareSimple",
            Position = ʬ,
            Size = new Vector2(ˠ, 20),
            Color = ʟ,
            Alignment = TextAlignment.LEFT
        };
        var ˡ = new MySprite
        {
            Type = SpriteType.TEXT,
            Data = ȁ,
            RotationOrScale = ɫ,
            Position = ʬ + new Vector2(2, -10),
            Color = Color.White,
            Alignment = TextAlignment.LEFT,
            FontId = ɨ
        };
        d.Add(ʶ);
        d.Add(ˡ);
    }

    void ʌ(ref MySpriteDrawFrame d, Vector2 ʬ, bool Ʋ, float ˢ)
    {
        var ˣ = new MySprite
        {
            Type = SpriteType.TEXTURE,
            Data = "SquareSimple",
            Position = ʬ,
            Size = ɵ,
            Color = ɯ,
            Alignment = TextAlignment.LEFT
        };
        d.Add(ˣ);
        var ˤ = Vector2.Lerp(ɶ, ɵ, ˢ);

        if (ˢ == 1)
        {
            var ˬ = new MySprite
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Position = ʬ,
                Size = ˤ,
                Color = ɸ,
                Alignment = TextAlignment.LEFT
            };
            d.Add(ˬ);
        }
        else
        {
            var ˬ = new MySprite
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Position = ʬ,
                Size = ˤ,
                Color = Color.CornflowerBlue,
                Alignment = TextAlignment.LEFT
            };
            d.Add(ˬ);
        }
    }

    void ʳ(ref MySpriteDrawFrame d, Vector2 ʬ, Vector2 ʛ, float ˢ, Color ʰ, Color ʱ)
    {
        var ˬ = new MySprite
        {
            Type = SpriteType.TEXTURE,
            Data = "SquareSimple",
            Position = ʬ,
            Size = new Vector2(5, 5),
            Color = Color.Lerp(ʱ, ʰ, ˢ),
            Alignment = TextAlignment.LEFT
        };
        d.Add(ˬ);
    }
    void ʌ(ref MySpriteDrawFrame d, Vector2 ʬ, Vector2 ˮ, Vector2 Ͱ,
        Color ͱ, Color Ͳ, bool Ʋ, float ˢ, bool ͳ)
    {
        var ˣ = new MySprite
        {
            Type = SpriteType.TEXTURE,
            Data = "SquareSimple",
            Position = ʬ,
            Size = Ͱ,
            Color = Ͳ,
            Alignment = TextAlignment.LEFT
        };
        d.Add(ˣ);
        var ˤ = Vector2.Lerp(ˮ, Ͱ, ˢ);


        ʬ = ͳ ? ʬ + new Vector2(Ͱ.X - ˤ.X, 0) : ʬ;

        var ˬ = new MySprite
        {
            Type = SpriteType.TEXTURE,
            Data = "SquareSimple",
            Position = ʬ,
            Size = ˤ,
            Color = ͱ,
            Alignment = TextAlignment.LEFT
        };
        d.Add(ˬ);
    }



bool ʝ(Vector3D ʹ, ʖ ʖ, IMyCameraBlock Ͷ,
        IMyTextPanel Ĕ, out Vector2 ͷ)
    {
        ͷ = Vector2.Zero;


        var ͺ = ʹ - ʖ.ʙ;
        var ͼ = ͺ.Dot(ʖ.ͻ) * ʖ.ͻ;
        var Ά = ʖ.ͽ / ͼ.Ə();

        var Έ = Ά * ͺ;

        if (Έ.Dot(Ĕ.WorldMatrix.Forward) < 0) return false;

        var Ί = ʖ.Ή -
                                   Vector3D.Dot(ʖ.Ή, ʖ.ͻ) * ʖ.ͻ;
        Έ -= Ί;

        var Ό = new Vector2(
            (float)Έ.Dot(Ĕ.WorldMatrix.Right),
            (float)Έ.Dot(Ĕ.WorldMatrix.Down));

        double Ύ = Ĕ.CubeGrid.GridSize * 0.855f;
        var Ώ = (float)(Ĕ.TextureSize.X / Ύ);

        Ό *= Ώ;

        var ΐ = Ĕ.TextureSize * 0.5f;
        ͷ = ΐ + Ό;
        return true;
    }


    

    struct ʖ
    {
        public readonly Vector3D ʙ;
        public readonly Vector3D Α;
        public readonly Vector3D ͻ;
        public readonly Vector3D Ή;
        public readonly double ͽ;
        public Vector3D Β;

        public ʖ(IMyCameraBlock Ͷ, IMyTextPanel Ĕ)
        {
            ʙ = Ͷ.GetPosition() +
                        Ͷ.WorldMatrix.Forward *
                        0.25;
            Α = Ĕ.GetPosition() + Ĕ.WorldMatrix.Forward * 0.5 * Ĕ.CubeGrid.GridSize;
            ͻ = Ĕ.WorldMatrix.Forward;
            Ή = Α - ʙ;
            ͽ = Math.Abs(Vector3D.Dot(Ή, ͻ));

            Β = ͽ * Ͷ.WorldMatrix.Forward;
        }
    }
}
public class L
{
    public Vector3D ž;
    public Vector3D Ĝ;
    public Vector3D Γ;
    Vector3D Δ;
        
    public bool Ε = false;
    public MatrixD ſ;
    public long Ζ;
    public IMyShipController ǋ;

    public L(long Ż, MatrixD ƻ, Vector3D ʬ, IMyShipController Η)
    {
        Ζ = Ż;
        ſ = ƻ;
        ž = ʬ;
        ǋ = Η;
            
        Θ.Add(this);
    }
        
        
    public virtual void ŵ(Vector3D ʬ, Vector3D Ľ, MatrixD ƻ, IMyShipController Η)
    {
        ž = ʬ;
        Ĝ = Ľ;
        Γ = (Ľ - Δ) * Program.ò;
        ǋ = Η;
            
        Δ = Ľ;
        ſ = ƻ;
        Ε = true;
    }




    private static List<L> Θ = new List<L>();

    public static void Ŵ()
    {
        foreach (L Ι in Θ)
        {
            Ι.Ε = false;
        }
    }
}
internal class J : L
{
    public static readonly J Î = new J(0, MatrixD.Identity, Vector3D.Zero, 0);
        
    bool Κ = true;
        
    public ulong Λ;
        
        
    public readonly List<ǆ> ǈ;
    public int Ŧ;
    Vector3D Μ;
        
    public readonly List<ǆ> Ν;
    public int ŧ;
    Vector3D Ξ;
        
    public readonly List<ǆ> Ο;
    public int ũ;
    Vector3D Π;
        
    public readonly List<ǆ> Ρ;
    public int Ũ;
    Vector3D Σ;
        
    public readonly List<ǆ> Τ;
    public int Ū;
    Vector3D Υ;
        
    public J(long Ż, MatrixD ƻ, Vector3D ʬ, ulong Φ) : base(Ż, ƻ, ʬ, null)
    {
        ǈ = new List<ǆ>();
        Ν = new List<ǆ>();
        Ο = new List<ǆ>();
        Ρ = new List<ǆ>();
        Τ = new List<ǆ>();

        Μ = Vector3D.Zero;
        Ξ = Vector3D.Zero;
        Π = Vector3D.Zero;
        Σ = Vector3D.Zero;
        this.Λ = Φ;
    }


    public void ƙ()
    {
        if (!Κ) return;
        Κ = false;
        Ŧ = 0;
        ŧ = 0;
        ũ = 0;
        Ũ = 0;
        Ū = 0;

        var O = new List<Vector3D>();
        var Χ = new List<Vector3D>();
        var Ψ = new List<Vector3D>();
        var Ω = new List<Vector3D>();
        var Ϊ = new List<Vector3D>();


        foreach (ǆ Ċ in ǈ)
        {
            var ʘ = Ċ.Ϋ * 2.5;
            var ά = Ċ.ƪ;
            switch (ά)
            {
                case Í.Î:
                    Ϊ.Add(ʘ);
                    Ū++;
                    O.Add(ʘ);
                    Ŧ++;
                    Τ.Add(Ċ);
                    break;
                case Í.Ɵ:
                    Χ.Add(ʘ);
                    ŧ++;
                    O.Add(ʘ);
                    Ŧ++;
                    Ν.Add(Ċ);
                    break;
                case Í.ƞ:
                    Ψ.Add(ʘ);
                    ũ++;
                    O.Add(ʘ);
                    Ŧ++;
                    Ο.Add(Ċ);
                    break;
                case Í.Ơ:
                    Ω.Add(ʘ);
                    Ũ++;
                    O.Add(ʘ);
                    Ŧ++;
                    Ρ.Add(Ċ);
                    break;
            }
        }


        Μ = έ(O);
        Ξ = έ(Χ);
        Π = έ(Ψ);
        Σ = έ(Ω);
        Υ = έ(Ϊ);
    }
    Vector3D έ(List<Vector3D> ή)
    {
        double ί = 0;
        var ΰ = new Dictionary<Vector3D, double>();
        for (var ĉ = 0; ĉ < ή.Count; ĉ++)
        {
            var ʘ = ή[ĉ];
            double α = 0;
            for (var β = 0; β < ή.Count; β++)
            {
                if (ĉ == β) continue;
                var γ = ή[β];
                α += (γ - ʘ).Ʈ();
            }

            if (α > ί) ί = α;
            ΰ.Add(ʘ, α);
        }

        double δ = 0;
        var ε = Vector3D.Zero;
        foreach (var ŀ in ΰ)
        {
            var α = ί - ŀ.Value;
            δ += α;
            ε += ŀ.Key * α;
        }

        if (δ == 0) return ε;
        ε /= δ;

        return ε;
    }





    private static int λ(Dictionary<int, KeyValuePair<List<Vector3D>[], double>> ζ)
    {
        var η = new double[ζ.Count];
        var ĉ = 0;
        foreach (var ŀ in ζ)
        {
            η[ĉ] = ŀ.Value.Value;
            ĉ++;
        }

        var θ = 0;
        double ι = 0;
        for (ĉ = 1; ĉ < η.Length; ĉ++)
        {
            var κ = Math.Abs(η[ĉ] - η[ĉ - 1]);
            if (κ > ι)
            {
                ι = κ;
                θ = ĉ + 1;
            }
        }

        return θ;
    }

    public Vector3D Ɓ(Í ά)
    {
        switch (ά)
        {
            case Í.Î:
                return Vector3D.Transform(Μ, ſ);
            case Í.Ɵ:
                return Vector3D.Transform(Ξ, ſ);
            case Í.ƞ:
                return Vector3D.Transform(Π, ſ);
            case Í.Ơ:
                return Vector3D.Transform(Σ, ſ);
            default:
                return Vector3D.Zero;
        }
    }

    public bool ǅ(Vector3I Ϋ, Í ά)
    {
        for (var ĉ = 0; ĉ < ǈ.Count; ĉ++)
        {
            var Ċ = ǈ[ĉ];
            if (Ċ.Ϋ == Ϋ)
            {
                if (Ċ.ƪ == Í.Î) Ċ.ƪ = ά;

                return true;
            }
        }

        return false;
    }


    public static MatrixD ƀ(MyDetectedEntityInfo ƾ)
    {
        var μ = ƾ.Orientation;
        var ν = ƾ.Position;
        μ.Translation = ν;


        return μ;
    }

    public Vector3D ʗ(int ξ)
    {
        if (ξ == -1) return ž;
        return Vector3D.Transform(2.5 * ǈ[ξ].Ϋ, ſ);
    }

    public Vector3D Œ(Í ά, int ξ)
    {
        if (ξ == -1) return ž;
        switch (ά)
        {
            case Í.Î:
                if (ξ >= ǈ.Count) return Vector3D.Zero;
                return Vector3D.Transform(2.5 * ǈ[ξ].Ϋ, ſ);
            case Í.Ɵ:
                if (ξ >= Ν.Count) return Vector3D.Zero;
                return Vector3D.Transform(2.5 * Ν[ξ].Ϋ, ſ);
            case Í.ƞ:
                if (ξ >= Ο.Count) return Vector3D.Zero;
                return Vector3D.Transform(2.5 * Ο[ξ].Ϋ, ſ);
            case Í.Ơ:
                if (ξ >= Ρ.Count) return Vector3D.Zero;
                return Vector3D.Transform(2.5 * Ρ[ξ].Ϋ, ſ);
            default:
                return Vector3D.Zero;
        }
    }

    public int ő(Í ά)
    {
        switch (ά)
        {
            case Í.Î:
                return Ŧ;
            case Í.Ɵ:
                return ŧ;
            case Í.ƞ:
                return ũ;
            case Í.Ơ:
                return Ũ;
            default:
                return 0;
        }
    }
}
public class ç
{

        
    public bool Ĵ { get; set; } = true;
    public bool ĭ { get; set; } = true;
    public bool į { get; set; } = true;
    public bool Į { get; set; } = true;
    public bool ı { get; set; }
    public bool Ĳ { get; set; }
    public bool Ĭ { get; set; } = true;
    public bool İ { get; set; } = true;
    public bool ĳ { get; set; } = true;
    public bool č { get; set; }
        
    public bool Č { get; set; } = true;
    public bool Ķ { get; set; } = true;
    public bool ĵ { get; set; } = true;
    public bool ο { get; set; } = true;


    public bool[] ɿ()
    {
        var π = new bool[14];
        π[0] = Ĵ;
        π[1] = ĭ;
        π[2] = į;
        π[3] = Į;
        π[4] = ı;
        π[5] = Ĳ;
        π[6] = Ĭ;
        π[7] = İ;
        π[8] = ĳ;
        π[9] = č;
        π[10] = Č;
        π[11] = Ķ;
        π[12] = ĵ;
        π[13] = ο;
        return π;
    }

    public void ʁ(bool[] π)
    {
        Ĵ = π[0];
        ĭ = π[1];
        į = π[2];
        Į = π[3];
        ı = π[4];
        Ĳ = π[5];
        Ĭ = π[6];
        İ = π[7];
        ĳ = π[8];
        č = π[9];
        Č = π[10];
        Ķ = π[11];
        ĵ = π[12];
        ο = π[13];
    }


    public Dictionary<string, bool> ˀ()
    {
        var ρ = new Dictionary<string, bool>();
        ρ.Add("AimAsst", Ĵ);
        ρ.Add("AutFire", ĭ);
        ρ.Add("GDrive", į);
        ρ.Add("TurretAI", Į);
        ρ.Add("Repulse", ı);
        ρ.Add("Precise", Ĳ);
        ρ.Add("LdAccel", Ĭ);
        ρ.Add("Balance", İ);
        ρ.Add("AutoScn", ĳ);
        ρ.Add("Volley", č);
        ρ.Add("Wt4All", Č);
        ρ.Add("H.Targt", Ķ);
        ρ.Add("H.Mssle", ĵ);
        ρ.Add("M.Inter", ο);
        return ρ;
    }

    public void ς(Dictionary<string, bool> ρ)
    {
        Ĵ = ρ["AimAsst"];
        ĭ = ρ["AutFire"];
        į = ρ["GDrive"];
        Į = ρ["TurretAI"];
        ı = ρ["Repulse"];
        Ĳ = ρ["Precise"];
        Ĭ = ρ["LdAccel"];
        İ = ρ["Balance"];
        ĳ = ρ["AutoScn"];
        č = ρ["Volley"];
        Č = ρ["Wt4All"];
        Ķ = ρ["H.Targt"];
        ĵ = ρ["H.Mssle"];
        ο = ρ["M.Inter"];
    }
}
public static class ƃ
{
        
    private static float σ = 25;
    private static double τ = 102.65;

    private static Vector3D υ;
        
    private static MatrixD φ;
    private static Vector3D χ;
    private static Vector3D ψ;
        
    private static Vector3D ω;
    private static Vector3D ϊ;
    private static float ϋ = 0f;
    private static float ό = 0f;

    private static double ύ = 0;
        
    public static Vector3D Ƅ(MyDetectedEntityInfo ώ, Vector3D Ϗ, Vector3D ϐ, Vector3D ϑ, IMyCubeGrid ϒ, Vector3D ϓ)
    {
        if (ώ.IsEmpty()) return Vector3D.Zero;
        var ϔ = ώ.Orientation;
        var ϕ = ϒ.WorldMatrix;
        var ϖ = ώ.Velocity;

        var ϗ = ϖ - χ;
            
            
            
        Vector3D Ϙ = ϑ - υ;
        Vector3D ϙ = Vector3D.Transform(Ϙ, MatrixD.Transpose(ϕ.GetOrientation()));
        ϓ /= 60;
        var Ϛ = Quaternion.CreateFromYawPitchRoll((float)ϓ.Y, (float)ϓ.X, (float)ϓ.Z);

        var ϛ = (ϐ - Ϗ).Normalized();
            
            

        var ϝ = Ϝ(ϔ, ϛ);

        var Ϟ = ϝ.Dot((ϛ));
            




        Vector3D ϟ = Vector3D.Zero;
        Vector3D Ϡ = Vector3D.Zero;
        double ϡ = Double.MaxValue;
        float Ϣ = 0;
            
        float Ľ = 2000;
        Vector3D ϣ = Ϗ;
        Vector3D Ϥ = ϑ;
        MatrixD ϥ = ϕ;

        Vector3D Ϧ = (ϖ + ϝ * Ľ).Normalized() * Ľ;
            
        for (float ϧ = 1f/60f; ϧ < 1; ϧ += 1f / 60f)
        {
            Vector3D Ϩ = Ϗ + Ϧ * ϧ;
            Vector3D ϩ = ϐ + Ϥ * ϧ;
            Ϥ += Vector3D.Transform(ϙ, ϥ.GetOrientation());
            if (Ϥ.LengthSquared() > τ * τ)
            {
                Vector3D Ϫ = Ϥ.Normalized();
                Ϥ = Ϫ * τ;
            }
                
            ϥ = MatrixD.Transform(ϥ, Ϛ);

            Vector3D Ϭ =
                ϫ(ϣ, Ϩ, ϩ);


            double ϭ = (Ϭ - ϩ).LengthSquared();

            if (ϭ < ϡ)
            {
                ϡ = ϭ;
                ϟ = Ϭ;
                Ϡ = ϩ;
                Ϣ = ϧ;
            }
                
            ϣ = Ϩ;
        }

            

            
        double ϯ = Ϯ(ϛ, ϗ);
            

        Vector3D ϰ = ϟ - Ϡ;
        if (ϯ != 0 && ύ != 0 && Math.Sign(ύ) != Math.Sign(ϯ))
        {
            ω = ϟ;
            ϋ = Ϣ;
        }

        if (ϰ.LengthSquared() < σ * σ)
        {
            ϊ = ϟ;
            ό = Ϣ;
        }
            
            
        φ = ϔ;
        υ = ϑ;
        χ = ϖ;
        ψ = ϗ;
        ό -= 1f / 60f;
        ϋ -= 1f / 60f;
        ύ = ϯ;
            
        Program.C.B.ϱ(ω, Color.Yellow, 5f, 1f, true);
        Program.C.B.ϱ(ϊ, Color.Green, 5f, 1f, true);
            
            
        var ϲ = ϐ - ω;
        ϲ -= (ϛ * ϲ.Dot(ϛ));
        if (ϋ > 0 && ϲ.LengthSquared() < 100 * 100)
            return ϲ.Normalized();


        ϲ = ϐ - ϊ;
        ϲ -= (ϛ * ϲ.Dot(ϛ));
        if (ό >= 0 && ϲ.LengthSquared() < 100 * 100)
            return ϲ.Normalized();
            
            
            
        return Vector3D.Zero;
    }


    private static List<double> ϳ = new List<double>();
    private const int ϴ = 180;
    private const double ϵ = 4.0;

    private static double Ϯ(Vector3D ϛ, Vector3D ϗ)
    {
        Vector3D Ϸ = ϗ - ψ;
        double ϸ = Ϸ.Dot(ϛ.Normalized());

        ϳ.Add(ϸ);
        if (ϳ.Count > ϴ)
            ϳ.RemoveAt(0);

        if (ϳ.Count < 10) 
        {
            return 0;
        }

        double Ϲ = ϳ.Average();
        double ϻ = ϳ.Average(Ϻ => Math.Pow(Ϻ - Ϲ, 2));
        double ϼ = Math.Sqrt(ϻ);

        bool Ͻ = Math.Abs(ϸ) > Ϲ + ϵ * ϼ;
            

        if (!Ͻ) return 0;
            
        return ϸ;
    }
        
        
    private static Vector3D Ϝ(MatrixD Ͼ, Vector3D Ͽ)
    {
        Vector3D Ѐ = Ͼ.Forward;
        double Ё = Vector3D.Dot(Ͽ, Ѐ);

        Vector3D Ђ = -Ͼ.Forward;
        double Ǘ = Vector3D.Dot(Ͽ, Ђ);
        if (Ǘ > Ё) { Ё = Ǘ; Ѐ = Ђ; }

        Ђ = Ͼ.Right;
        Ǘ = Vector3D.Dot(Ͽ, Ђ);
        if (Ǘ > Ё) { Ё = Ǘ; Ѐ = Ђ; }

        Ђ = -Ͼ.Right;
        Ǘ = Vector3D.Dot(Ͽ, Ђ);
        if (Ǘ > Ё) { Ё = Ǘ; Ѐ = Ђ; }

        Ђ = Ͼ.Up;
        Ǘ = Vector3D.Dot(Ͽ, Ђ);
        if (Ǘ > Ё) { Ё = Ǘ; Ѐ = Ђ; }

        Ђ = -Ͼ.Up;
        Ǘ = Vector3D.Dot(Ͽ, Ђ);
        if (Ǘ > Ё) { Ё = Ǘ; Ѐ = Ђ; }

        return Ѐ;
    }
        
    private static Vector3D ϫ(Vector3D Ѓ, Vector3D Є, Vector3D Ѕ)
    {
        Vector3D І = Є - Ѓ;
        double Ї = І.LengthSquared();

        if (Ї == 0)
            return Ѓ;

        double ϧ = Vector3D.Dot(Ѕ - Ѓ, І) / Ї;
        ϧ = Math.Max(0.0, Math.Min(1.0, ϧ));

        return Ѓ + І * ϧ;
    }
}
internal static class Ō
{
    private const int Ј = 50;

    private const string Љ =
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

    private static readonly StringBuilder Њ = new StringBuilder(256);

    public static void ō(IMyTextSurface ɡ, string Ћ, string ù, Exception Ŋ)
    {
        if (ɡ == null) return;
        ɡ.ContentType = ContentType.TEXT_AND_IMAGE;
        ɡ.Alignment = TextAlignment.LEFT;
        var Ќ = 512f / Math.Min(ɡ.TextureSize.X, ɡ.TextureSize.Y);
        ɡ.FontSize = Ќ * ɡ.TextureSize.X / (19.5f * Ј);
        ɡ.FontColor = Color.White;
        ɡ.BackgroundColor = Color.Blue;
        ɡ.Font = "Monospace";
        var Ѝ = Ŋ.ToString();
        var Ў = Ѝ.Split('\n');
        Њ.Clear();
        foreach (var І in Ў)
            if (І.Length <= Ј)
            {
                Њ.Append(І).Append("\n");
            }
            else
            {
                var Џ = І.Split(' ');
                var А = 0;
                foreach (var Б in Џ)
                {
                    А += Б.Length;
                    if (А >= Ј)
                    {
                        Њ.Append("\n");
                        А = Б.Length;
                    }

                    Њ.Append(Б).Append(" ");
                    А += 1;
                }

                Њ.Append("\n");
            }

        ɡ.WriteText(string.Format(Љ,
            Ћ.ToUpperInvariant(),
            ù,
            DateTime.Now,
            Њ));
    }
}
public class A
    {
        public readonly bool В;

        public void Е() => Г?.Invoke(Д);
        Action<IMyProgrammableBlock> Г;

        public void З() => Ж?.Invoke(Д);
        Action<IMyProgrammableBlock> Ж;

        public void Й(int ė) => И?.Invoke(Д, ė);
        Action<IMyProgrammableBlock, int> И;

        public int ϱ(Vector3D К, Color ʟ, float Л = 0.2f, float Н = М, bool? О = null) => П?.Invoke(Д, К, ʟ, Л, Н, О ?? Р) ?? -1;
        Func<IMyProgrammableBlock, Vector3D, Color, float, float, bool, int> П;

        public int Ц(Vector3D С, Vector3D Т, Color ʟ, float Ф = У, float Н = М, bool? О = null) => Х?.Invoke(Д, С, Т, ʟ, Ф, Н, О ?? Р) ?? -1;
        Func<IMyProgrammableBlock, Vector3D, Vector3D, Color, float, float, bool, int> Х;

        public int Ь(BoundingBoxD Ч, Color ʟ, Ш Ъ = Ш.Щ, float Ф = У, float Н = М, bool? О = null) => Ы?.Invoke(Д, Ч, ʟ, (int)Ъ, Ф, Н, О ?? Р) ?? -1;
        Func<IMyProgrammableBlock, BoundingBoxD, Color, int, float, float, bool, int> Ы;

        public int Я(MyOrientedBoundingBoxD Э, Color ʟ, Ш Ъ = Ш.Щ, float Ф = У, float Н = М, bool? О = null) => Ю?.Invoke(Д, Э, ʟ, (int)Ъ, Ф, Н, О ?? Р) ?? -1;
        Func<IMyProgrammableBlock, MyOrientedBoundingBoxD, Color, int, float, float, bool, int> Ю;

        public int г(BoundingSphereD а, Color ʟ, Ш Ъ = Ш.Щ, float Ф = У, int б = 15, float Н = М, bool? О = null) => в?.Invoke(Д, а, ʟ, (int)Ъ, Ф, б, Н, О ?? Р) ?? -1;
        Func<IMyProgrammableBlock, BoundingSphereD, Color, int, float, int, float, bool, int> в;

        public int з(MatrixD д, float е = 1f, float Ф = У, float Н = М, bool? О = null) => ж?.Invoke(Д, д, е, Ф, Н, О ?? Р) ?? -1;
        Func<IMyProgrammableBlock, MatrixD, float, float, float, bool, int> ж;

        public int й(string Ĺ, Vector3D К, Color? ʟ = null, float Н = М) => и?.Invoke(Д, Ĺ, К, ʟ, Н) ?? -1;
        Func<IMyProgrammableBlock, string, Vector3D, Color?, float, int> и;

        public int о(string ƒ, к м = к.л, float Н = 2) => н?.Invoke(Д, ƒ, м.ToString(), Н) ?? -1;
        Func<IMyProgrammableBlock, string, string, float, int> н;

        public void т(string ƒ, string п = null, Color? р = null, к м = к.л) => с?.Invoke(Д, ƒ, п, р, м.ToString());
        Action<IMyProgrammableBlock, string, string, Color?, string> с;

        public void ъ(out int ė, double у, double ф = 0.05, х ч = х.ц, string ш = null) => ė = щ?.Invoke(Д, у, ф, ч.ToString(), ш) ?? -1;
        Func<IMyProgrammableBlock, double, double, string, string, int> щ;

        public double э(int ė, double ы = 1) => ь?.Invoke(Д, ė) ?? ы;
        Func<IMyProgrammableBlock, int, double> ь;

        public int я() => ю?.Invoke() ?? -1;
        Func<int> ю;

        public TimeSpan ё() => ѐ?.Invoke() ?? TimeSpan.Zero;
        Func<TimeSpan> ѐ;

        public ђ є(Action<TimeSpan> ѓ) => new ђ(this, ѓ);
        public struct ђ : IDisposable
        {
            A ѕ; TimeSpan і; Action<TimeSpan> ї;
            public ђ(A ј, Action<TimeSpan> ѓ) { ѕ = ј; ї = ѓ; і = ѕ.ё(); }
            public void Dispose() { ї?.Invoke(ѕ.ё() - і); }
        }

        public enum Ш { љ, Щ, њ }
        public enum х { ћ, ќ, ѝ, ў, џ, Ѡ, ѡ, Ѣ, ѣ, Ѥ, ѥ, Ѧ, ѧ, ц, Ѩ, ѩ, Ѫ, ѫ, Ѭ, ѭ, Ѯ, ѯ, Ѱ, Ʒ, Ƹ, ѱ, Ѳ, ѳ, Ѵ, ѵ, Ѷ, ѷ, Ѹ, ѹ, Ѻ, ѻ, ѕ, Ѽ, ї, ѽ, Ѿ, ѿ, Ҁ, ҁ, C, Ҋ, ҋ, Ҍ, ҍ, Ҏ, ҏ, Ґ, ґ, Ғ, і, ғ, Ҕ, ҕ, Җ, җ, Ҙ, ҙ, Қ, қ, Ҝ, ҝ, Ҟ, ҟ, Ҡ, ҡ, Ң, ң, Ҥ, ҥ, Ҧ, ҧ, Ҩ, ҩ, Ҫ, ҫ, Ҭ, ҭ, Ү, ү, Ұ, ұ, Ҳ, ҳ, Ҵ, ҵ }
        public enum к { л, Ҷ, ҷ, Ҹ, ҹ, Һ }
        const float У = 0.02f;
        const float М = -1;
        IMyProgrammableBlock Д;
        bool Р;
        public A(MyGridProgram ā, bool һ = false)
        {
            if(ā == null) throw new Exception("Pass `this` into the API, not null.");
            Р = һ;
            Д = ā.Me;
            var Ҽ = Д.GetProperty("DebugAPI")?.As<IReadOnlyDictionary<string, Delegate>>()?.GetValue(Д);
            if(Ҽ != null)
            {
                ҽ(out Ж, Ҽ["RemoveAll"]);
                ҽ(out Г, Ҽ["RemoveDraw"]);
                ҽ(out И, Ҽ["Remove"]);
                ҽ(out П, Ҽ["Point"]);
                ҽ(out Х, Ҽ["Line"]);
                ҽ(out Ы, Ҽ["AABB"]);
                ҽ(out Ю, Ҽ["OBB"]);
                ҽ(out в, Ҽ["Sphere"]);
                ҽ(out ж, Ҽ["Matrix"]);
                ҽ(out и, Ҽ["GPS"]);
                ҽ(out н, Ҽ["HUDNotification"]);
                ҽ(out с, Ҽ["Chat"]);
                ҽ(out щ, Ҽ["DeclareAdjustNumber"]);
                ҽ(out ь, Ҽ["GetAdjustNumber"]);
                ҽ(out ю, Ҽ["Tick"]);
                ҽ(out ѐ, Ҽ["Timestamp"]);
                З();
                В = true;
            }
        }
        void ҽ<ғ>(out ғ Ҿ, object ҿ) => Ҿ = (ғ)ҿ;
    }
public static class ý
{
    public static readonly DateTime Ӏ = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    public static uint Ӂ;
    public static uint ӂ;

    private static readonly StringBuilder Ӄ = new StringBuilder();


    public static IMyShipController ģ;

    private static bool ӄ;

    private static readonly ushort Ӆ = 1024;

    public static void ӆ()
    {
    }

    private static double Ӊ(double ȇ, int Ӈ)
    {
        var ӈ = Math.Pow(10, Ӈ);
        return Math.Round(ȇ * ӈ) / ӈ;
    }

    public static void ӊ()
    {
        Ӂ++;
        ӄ = false;
    }

    public static uint ӌ(this DateTime Ӌ)
    {
        return (uint)Ӌ.Subtract(Ӏ).TotalSeconds;
    }


    public static string ӎ(ushort ȇ)
    {
        var Ӎ = (char)ȇ;
        return Ӎ.ToString();
    }

    public static uint ӏ()
    {
        return (uint)DateTime.Now.TimeOfDay.TotalSeconds;
    }

    public static uint ӓ()
    {
        var Ӑ = DateTime.Now;
        var ӑ = new DateTime(Ӑ.Year, Ӑ.Month, Ӑ.Day, Ӑ.Hour, 0, 0, 0);
        var Ӓ = Ӑ - ӑ;
        return (uint)Ӓ.TotalMilliseconds;
    }


    public static void þ()
    {
        Ӄ.Append("T+").Append(ӌ(DateTime.Now)).AppendLine();
    }

    private static void Ӕ()
    {
        if (!ӄ)
        {
            Ӄ.Append((char)(ushort)(Ӂ - ӂ + Ӆ));
            ӂ = Ӂ;
            ӄ = true;
        }
    }


    public static void ӕ(char Ĺ)
    {
        Ӕ();
        Ӄ.Append((char)1).Append(Ĺ);
    }

    public static void Ә(char Ĺ, double Ӗ, double ľ)
    {
        Ӕ();
        Ӄ.Append((char)2).Append(Ĺ).Append((char)(ӗ)Ӗ).Append((char)(ӗ)ľ);
    }

    public static void ә(char Ĺ, double Ӗ, double ľ)
    {
        Ӕ();
        Ӄ.Append((char)3).Append(Ĺ).Append((char)(ӗ)Ӗ).Append((char)(ӗ)ľ);
    }

    public static void Ӛ(char Ĺ, double Ӗ, double ľ)
    {
        Ӕ();
        Ӄ.Append((char)4).Append(Ĺ).Append((char)(ӗ)Ӗ).Append((char)(ӗ)ľ);
    }

    public static void Ӝ(char ӛ)
    {
        Ӕ();
        Ӄ.Append((char)5).Append(ӛ);
    }

    public static void ӝ(char ӛ)
    {
        Ӕ();
        Ӄ.Append((char)6).Append(ӛ);
    }

    public static void Ӟ(char ӛ)
    {
        Ӕ();
        Ӄ.Append((char)7).Append(ӛ);
    }


    internal static void ӟ()
    {
        Ӕ();
        Ӄ.Append((char)8);
    }

    internal static void Ӡ()
    {
        Ӕ();
        Ӄ.Append((char)9);
    }

    internal static void ӡ()
    {
        Ӕ();
        Ӄ.Append((char)10);
    }

    internal static void Ӣ()
    {
        Ӕ();
        Ӄ.Append((char)11);
    }

    public static void ӥ(float ӣ, float Ӥ)
    {
        Ӕ();
        Ӄ.Append((char)12).Append((char)(ӗ)ӣ).Append((char)(ӗ)Ӥ);
    }

    public static void Ӧ(float ӣ, float Ӥ)
    {
        Ӕ();
        Ӄ.Append((char)13).Append((char)(ӗ)ӣ).Append((char)(ӗ)Ӥ);
    }

    internal static void Ө(bool ӧ)
    {
        Ӕ();
        Ӄ.Append((char)14).Append(ӧ ? '1' : '0');
    }

    internal static void Ӫ(bool ө)
    {
        Ӕ();
        Ӄ.Append((char)15).Append(ө ? '1' : '0');
    }


    internal static void ǉ()
    {
        Ӕ();
        Ӄ.Append((char)16);
    }

    internal static void Ǌ()
    {
        Ӕ();
        Ӄ.Append((char)17);
    }

    internal static void Ӭ(int ӫ)
    {
        Ӕ();
        Ӄ.Append((char)18).Append((char)(ushort)ӫ);
    }


    internal static string ƕ()
    {
        var ɼ = Ӄ.ToString();
        Ӄ.Clear();
        return ɼ;
    }

    internal static string Ɣ()
    {
        return Ӄ.ToString();
    }
}
internal class g
{


    bool ӭ;
    double Ӯ;
    private readonly List<ӯ> Ӱ;
    private readonly List<ӱ> Ӳ;

    int d;
    int ӳ;

    bool Ӵ = true;
    private readonly int ӵ = 600;

    private readonly List<IMyGravityGenerator> Ӷ;
    private readonly List<IMyGravityGeneratorSphere> ӷ;

    private readonly IMyGridTerminalSystem Ӹ;
    int ӹ;
    double Ӻ;
    private readonly List<ӯ> ӻ;

    private readonly int Ӽ = 301;


    private readonly double
        ӽ =
            1d / 1000;

    private readonly List<Ӿ> ӿ;
    private readonly List<IMyArtificialMassBlock> Ԁ;

    private readonly float ԁ;

    bool Ԃ;
    bool ԃ;


    Vector3D Ԅ = Vector3D.Zero;
    bool ԅ;

    Ԇ
        Ԉ =
            Ԇ.ԇ;

    Vector3D ԉ = Vector3D.Zero;

    private readonly MyGridProgram ā;
    private readonly float Ԋ;


    IMyShipController å;
    double ԋ;
    private readonly int Ԍ = 1201;
    private readonly int ԍ = 2000000;
    private readonly List<IMySpaceBall> Ԏ;

    private readonly List<ԏ> ą;
    Ԇ Ʋ = Ԇ.Ԑ;
    double ԑ;

    private readonly List<ӯ> Ԓ;

    public g(List<IMyArtificialMassBlock> ӿ, List<IMySpaceBall> ą,
        IMyGridTerminalSystem Ӹ, MyGridProgram ā,
        List<IMyGravityGenerator> Ć, List<IMyGravityGeneratorSphere> ć,
        IMyShipController Ē, float ԁ, float Ԋ)
    {
        å = Ē;
        this.ӿ = new List<Ӿ>();
        Ԁ = ӿ;
        foreach (var Ċ in ӿ) this.ӿ.Add(new Ӿ(Ċ));
        this.ą = new List<ԏ>();
        Ԏ = ą;
        foreach (var ԓ in ą) this.ą.Add(new ԏ(ԓ));
        this.Ӹ = Ӹ;
        this.ā = ā;
        Ӷ = Ć;
        ӷ = ć;
        this.ԁ = ԁ;
        this.Ԋ = Ԋ;

        Ӳ = new List<ӱ>();
        var Ԕ = Ē.WorldMatrix.Forward;
        var ԕ = Ē.CenterOfMass;
        foreach (var Ԗ in ć)
        {
            var ʘ = Ԗ.WorldMatrix.Translation;
            var ԗ = ԕ - ʘ;
            if (Vector3D.Dot(ԗ, Ԕ) > 0)
                Ӳ.Add(new ӱ(Ԗ, -1, "Rear"));
            else
                Ӳ.Add(new ӱ(Ԗ, 1, "Forward"));
        }

        Ԓ = new List<ӯ>();
        ӻ = new List<ӯ>();
        Ӱ = new List<ӯ>();
        foreach (var Ԙ in Ć)
            if (Ԙ.WorldMatrix.Up == -Ē.WorldMatrix.Forward)
                Ӱ.Add(new ӯ(Ԙ, 1, "Forward"));
            else if (Ԙ.WorldMatrix.Up == Ē.WorldMatrix.Forward)
                Ӱ.Add(new ӯ(Ԙ, -1, "Backward"));
            else if (Ԙ.WorldMatrix.Up == -Ē.WorldMatrix.Left)
                ӻ.Add(new ӯ(Ԙ, 1, "Left"));
            else if (Ԙ.WorldMatrix.Up == Ē.WorldMatrix.Left)
                ӻ.Add(new ӯ(Ԙ, -1, "Right"));
            else if (Ԙ.WorldMatrix.Up == -Ē.WorldMatrix.Up)
                Ԓ.Add(new ӯ(Ԙ, 1, "Up"));
            else if (Ԙ.WorldMatrix.Up == Ē.WorldMatrix.Up)
                Ԓ.Add(new ӯ(Ԙ, -1, "Down"));
        ԙ();
        Ӯ = Ԛ(Ӱ,
            Ӳ, Ē.WorldMatrix.Forward);
        Ӻ = Ԛ(ӻ);
        ԑ = Ԛ(Ԓ);
        var ԋ = Ē.CalculateShipMass();
        this.ԋ = ԋ.PhysicalMass;

        ԛ(ԕ, this.ӿ);
        ԛ(ԕ, this.ą);
    }

    public void œ(int Ԝ)
    {
        if (Ʋ == Ԇ.ԝ) return;
        Ԟ(Ԓ, 0);
        Ԟ(ӻ, 0);
        Ԟ(Ӱ, 0);
        Ԟ(Ӳ, 0);
        ӹ = Ԝ;
    }

    public void ŵ(IMyShipController å, bool ө, bool ԟ, bool Ԡ, Vector3D ƅ)
    {
            
        this.å = å;
        Ԃ = ө;
        ӭ = å.DampenersOverride;

        if (ӭ != ԃ) ý.Ө(ӭ);
        if (Ԃ != ԅ) ý.Ӫ(Ԃ);

            
        if (ӹ > 0)
        {
            ӹ--;
            return;
        }

        d++;

        if (!Ӵ && ӳ > ӵ)
        {
            Ӵ = true;
            ŝ(false);
            ԡ(false);
            ý.Ӡ();
        }
            
        float Ԣ = 9.81f;
        var ԣ = å.GetNaturalGravity();
        var Ԥ = ԣ.Length();
            
            
        var ԥ = MathHelper.Clamp(Ԣ - (Ԥ * 2), 0, Ԣ) / Ԣ;

        float Ԧ = 1;
        if (ԥ != 0)
        {
            Ԧ = (float)(1 / ԥ);
        }
            
        var Ľ = å.GetShipVelocities().LinearVelocity + (ԣ / 10);
            
            
        if (d % 901 == 0) ԧ = å.CalculateShipMass();
            
        this.ԋ = ԧ.PhysicalMass;
        Ľ *= Ԧ;
            
        Vector3 Ա =
            Vector3D.TransformNormal(Ľ, MatrixD.Transpose(å.WorldMatrix));

            
            
        var Բ = Ա;

        if (Բ.LengthSquared() > 3)
        {
            Բ.X =
                MathHelper.Clamp(MathHelper.RoundOn2(Բ.X), -1, 1);
            Բ.Y =
                MathHelper.Clamp(MathHelper.RoundOn2(Բ.Y), -1, 1);
            Բ.Z =
                MathHelper.Clamp(MathHelper.RoundOn2(Բ.Z), -1, 1);
        }

        var Գ = Ľ.Ʈ();

        Vector3D Դ = å.MoveIndicator;
        Դ.X *= -1;
        Դ.Z *= -1;
            

            
        if (ƅ.LengthSquared() != 0)
        {
            var Ե = Vector3D.TransformNormal(ƅ, MatrixD.Transpose(å.WorldMatrix));
            Ե.X *= -1;
            Դ += Ե;
        }
            
        var Զ = Դ.Ʈ();

        Դ *= Ԧ;   
            
            
            
        Ԉ = Ʋ;
        Ʋ = Ԇ.Ԑ;

        if (Զ > 0 || (Գ > 0.000001 && ӭ))
            Ʋ = Ԇ.ԇ;
        if (ԟ) Ʋ = Ԇ.ԝ;
            

        switch (Ʋ)
        {


            case Ԇ.Ԑ:

                if (Ԉ != Ԇ.Ԑ)
                {
                    Է(Vector3D.Zero, 0);
                    Ԅ = Vector3D.Zero;
                    ԉ = Vector3D.Zero;
                }

                ӳ++;
                break;




            case Ԇ.ԇ:

                if (Ԉ != Ԇ.ԇ)
                {
                    if (!Ӵ) break;
                    Ӵ = false;
                    ӳ = 0;
                    ŝ(true);
                    ԡ(true);
                    ý.ӟ();
                }


                var Ժ = Ը.Թ;
                Ժ = Ժ + (Ԃ ? 2 : 0);
                Ժ = Ժ + (ӭ ? 1 : 0);
                Ի(Ժ, Դ, Ա, Բ);
                break;




            case Ԇ.ԝ:
                if (Ԉ != Ԇ.ԝ)
                {
                    ý.ӡ();
                    ԝ();
                }

                break;

        }

            
        if (Ԉ == Ԇ.ԝ && Ʋ != Ԇ.ԝ)
        {
            ý.Ӣ();
            for (var ĉ = 0; ĉ < Ӳ.Count; ĉ++)
            {
                var Լ = Ӳ[ĉ];
                Լ.Խ(ԁ);
                ŝ(true);
            }
        }
            
        if (!Ԡ) return;
        if (d % Ӽ == 0)
        {
                
            var ԕ = å.CenterOfMass;
            ԛ(ԕ, ӿ);
                
            Ծ();
            Կ(ӿ, ą, ԕ);

            Հ(Ʋ, ӿ);
            Ӯ = Ԛ(Ӱ,
                Ӳ, å.WorldMatrix.Forward);
            Ӻ = Ԛ(ӻ);
            ԑ = Ԛ(Ԓ);
            double ӣ;
                
                
            double Ӥ;

            var Ձ = Vector3D.Zero;
            var Ղ = Vector3D.Zero;
            foreach (Ӿ Ճ in ӿ)
            {
                Ձ += Ճ.Մ;
                Ղ += Ճ.Յ;
            }

            foreach (ԏ Ն in ą)
            {
                Ձ += Ն.Մ;
                Ղ += Ն.Յ;
            }

            Ձ /= ӿ.Count + ą.Count;
            Ղ /= ӿ.Count + ą.Count;
            ӣ = Ձ.Ə();
            Ӥ = Ղ.Ə();

            ý.ӥ(MathHelper.RoundOn2((float)ӣ / 1000),
                MathHelper.RoundOn2((float)Ӥ / 1000));
                
        }
            
        if ((d + ԍ) % Ԍ == 0)
        {
            var ԕ = å.CenterOfMass;
            ԛ(ԕ, ą);
            Շ(ӿ, ą, ԕ);
            Հ(Ʋ, ą);
            Ӯ = Ԛ(Ӱ,
                Ӳ, å.WorldMatrix.Forward);
            Ӻ = Ԛ(ӻ);
            ԑ = Ԛ(Ԓ);
            double ӣ;
            double Ӥ;

            var Ձ = Vector3D.Zero;
            var Ղ = Vector3D.Zero;
            foreach (Ӿ Ճ in ӿ)
            {
                Ձ += Ճ.Մ;
                Ղ += Ճ.Յ;
            }

            foreach (ԏ Ն in ą)
            {
                Ձ += Ն.Մ;
                Ղ += Ն.Յ;
            }

            Ձ /= ӿ.Count + ą.Count;
            Ղ /= ӿ.Count + ą.Count;
            ӣ = Ձ.Ə();
            Ӥ = Ղ.Ə();

            ý.Ӧ(MathHelper.RoundOn2((float)ӣ / 1000),
                MathHelper.RoundOn2((float)Ӥ / 1000));
        }

        ԃ = ӭ;
        ԅ = Ԃ;
    }

    void Ի(Ը Ժ, Vector3D Դ, Vector3 Ա,
        Vector3 Բ)
    {
        switch (Ժ)
        {
            case Ը.Թ:
                if (Դ != Ԅ) Է(Դ, 1);
                break;
            case Ը.Ո:
                if (Դ != Ԅ ||
                    Բ != ԉ)
                    Չ(Դ, Ա, 1f);
                break;
            case Ը.Պ:
                if (Դ != Ԅ) Է(Դ, 0.1f);
                break;
            case Ը.Ջ:
                if (Դ != Ԅ ||
                    Բ != ԉ)
                    Չ(Դ, Ա, 0.1f);
                break;
        }

        Ԅ = Դ;
        ԉ = Բ;
    }

    void Է(Vector3D Դ, float ӈ)
    {
        Դ *= ӈ;
        Ԟ(Ԓ, (float)Դ.Y);
        Ԟ(ӻ, (float)Դ.X);
        Ԟ(Ӱ, (float)Դ.Z);
        Ԟ(Ӳ, (float)Դ.Z);
    }

    void Չ(Vector3D Դ, Vector3D Ա, float ӈ)
    {
        Դ *= ӈ;
        if (Դ.Y == 0)
        {
            var Ռ = ԋ / ԑ;
            var Ս = Ա.Y == 0 ? 0 : (float)(Ա.Y * 10 * Ռ);
            Ԟ(Ԓ, -Ս);
        }
        else
        {
            Ԟ(Ԓ, (float)Դ.Y);
        }

        if (Դ.X == 0)
        {
            var Ռ = ԋ / Ӻ;
            var Ս = Ա.X == 0 ? 0 : (float)(Ա.X * 10 * Ռ);
            Ԟ(ӻ, Ս);
        }
        else
        {
            Ԟ(ӻ, (float)Դ.X);
        }

        if (Դ.Z == 0)
        {
            var Ռ = ԋ / Ӯ;
            var Ս = Ա.Z == 0 ? 0 : (float)(Ա.Z * 10 * Ռ);
            Ԟ(Ӱ, Ս);
            Ԟ(Ӳ, Ս);
        }
        else
        {
            Ԟ(Ӱ, (float)Դ.Z);
            Ԟ(Ӳ,(float)Դ.Z);
        }
    }

    void ԝ()
    {
        ŝ(false);
        foreach (ӱ Լ in Ӳ)
        {
            Լ.Վ.Enabled = true;
            Լ.Խ(Ԋ);
            Լ.Տ(-9.81f);
        }

        Ԟ(Ԓ, 0);
        Ԟ(ӻ, 0);
        Ԟ(Ӱ, 0);
    }


    void ԙ()
    {
        foreach (ԏ ԓ in ą) ԓ.Ċ.VirtualMass = 20000;
    }


    void ԡ(bool Ʋ)
    {
        Ր(Ԓ, Ʋ);
        Ր(ӻ, Ʋ);
        Ր(Ӱ, Ʋ);
        Ր(Ӳ, Ʋ);
    }


    void ԛ(Vector3D ԕ, List<Ӿ> ӿ, List<ԏ> ą)
    {
        foreach (Ӿ Ճ in ӿ) Ճ.Ց(ԕ);
        foreach (ԏ Ն in ą) Ն.Ց(ԕ);
    }

    void ԛ(Vector3D ԕ, List<ԏ> ą)
    {
        foreach (ԏ Ն in ą) Ն.Ց(ԕ);
    }

    void ԛ(Vector3D ԕ, List<Ӿ> ӿ)
    {
        for (var ĉ = ӿ.Count - 1; ĉ >= 0; ĉ--)
        {
            var Ճ = ӿ[ĉ];
            if (Ճ.Ċ.Closed || !Ӹ.CanAccess(Ճ.Ċ))
            {
                ӿ.RemoveAt(ĉ);
                Ԁ.Remove(Ճ.Ċ);
            }

            Ճ.Ց(ԕ);
        }
    }

    void Հ(Ԇ Ʋ, List<ԏ> ą)
    {
        if (Ʋ != Ԇ.ԇ) return;
        foreach (ԏ Ն in ą) Ն.Ċ.Enabled = Ն.Ւ;
    }

    void Հ(Ԇ Ʋ, List<Ӿ> ӿ)
    {
        if (Ʋ != Ԇ.ԇ) return;
        foreach (Ӿ Ճ in ӿ) Ճ.Ċ.Enabled = Ճ.Ւ;
    }

        
    int ĉ = 0, ɦ = 1;
    MyShipMass ԧ;

    void Ծ()
    {
           
        var Փ = new List<Ӿ>();
        var Ք = Փ.Count;
                
        foreach (Ӿ Օ in ӿ)
            if (!Օ.Ւ) Փ.Add(Օ);
        if (Ք <= 1) return;
        for (int ф = 0; ф < 50000; ф++)
        {
            if (ɦ >= Ք)
            {
                ĉ++;
                if (ĉ >= Ք - 1)
                {
                    break;
                }
                ɦ = ĉ+1;
            }
                
            var Ֆ = Փ[ĉ];
            var ՙ = Փ[ɦ];

            var ա = Ֆ.Յ + ՙ.Յ;
            var բ = ա.X * ա.X + ա.Y * ա.Y + ա.Z * ա.Z;
            if (բ < 1) {
                Ֆ.Ւ = ՙ.Ւ = true;
            }
            ɦ++;
        }

        if (ĉ >= Ք || ĉ + 1 >= Ք)
        {
            ĉ = 0;
            ɦ = 1;
        }
    }

    void Կ(List<Ӿ> գ, List<ԏ> դ, Vector3D ԕ)
    {
        var ե = Vector3D.Zero;

        var զ = Vector3D.Zero;
            
        foreach (Ӿ Ċ in գ)
        {
            ե += Ċ.Ւ ? Ċ.Յ : Vector3D.Zero;
            զ += Vector3D.Abs(Ċ.Յ);
        }
            
        foreach (ԏ ԓ in դ)
        {
            ե += ԓ.Յ;
            զ += Vector3D.Abs(ԓ.Յ);
        }
            

        var է = զ.Ʈ();

        int ӫ = 0;
        foreach (Ӿ Ċ in գ)
        {
            ӫ++;
                
            if (ե.Ʈ() > (long)50000 * 50000)
            {
                var ը = Ċ.Ւ;

                var թ = ը ? Ċ.Յ : Vector3D.Zero;
                var ժ = ը ? Vector3D.Zero : Ċ.Յ;



                ե = ե - թ;


                if ((ե + ժ).Ʈ() * (ը ? 1.03 : 0.97) <
                    (ե + թ).Ʈ())
                {
                    Ċ.Ւ = !ը;
                    ե = ե + ժ;
                }
                else
                {
                    ե = ե + թ;
                }
            }
            else
            {
                break;
            }
                
        }
                
    }


    void Շ(List<Ӿ> գ, List<ԏ> դ, Vector3D ԕ)
    {
        var ի = Vector3D.Zero;

        foreach (Ӿ Ċ in գ) ի += Ċ.Ւ ? Ċ.Յ : Vector3D.Zero;
        foreach (ԏ ԓ in դ) ի += ԓ.Յ;
        foreach (ԏ ԓ in ą)
            if (ի.Ʈ() > 10 * 10)
            {
                var լ = ԓ.Ċ.VirtualMass;
                var խ = լ + 500;


                float ф = 5000;
                var ե = ԓ.Յ;
                var կ = ԓ.ծ;
                var ձ = ԓ.հ;


                var ղ = ի;
                var ճ = ի - ե + կ;
                var մ = ի - ե + ձ;

                var յ = ճ.Ʈ() + 2 * 2;
                var ն =
                    մ.Ʈ() - 2 * 2;
                var շ = ղ.Ʈ();

                var ո = ի;

                var չ = ձ.Ʈ() / ե.Ʈ() * ф;
                var պ = կ.Ʈ() / ե.Ʈ() * ф;

                ԓ.ջ = չ;
                ԓ.ռ = պ;


                if (յ < շ && յ < ն)
                {
                    խ = Math.Max(լ - (float)պ * 0.99f, 0);
                    ո = ճ;
                }
                else if (ն < շ && ն < յ)
                {
                    խ = Math.Min(լ + (float)չ * 1.01f, 20000);
                    ո = մ;
                }
                else
                {
                    խ = լ;
                }

                ԓ.ս(խ);
                ի = ո;
            }
    }

    void Ր(List<ӯ> Ƥ, bool Ǹ)
    {
        for (var ĉ = 0; ĉ < Ƥ.Count; ĉ++)
        {
            var Ԗ = Ƥ[ĉ];
            Ԗ.Վ.Enabled = Ǹ;
        }
    }

    void Ր(List<ӱ> Ƥ, bool Ǹ)
    {
        for (var ĉ = 0; ĉ < Ƥ.Count; ĉ++)
        {
            var Ԗ = Ƥ[ĉ];
            Ԗ.Վ.Enabled = Ǹ;
        }
    }

    void Ԟ(List<ӯ> Ƥ, float վ)
    {
        for (var ĉ = 0; ĉ < Ƥ.Count; ĉ++)
        {
            var Ԗ = Ƥ[ĉ];
            Ԗ.տ(վ);
        }
    }

    void Ԟ(List<ӱ> Ƥ, float վ)
    {
        for (var ĉ = 0; ĉ < Ƥ.Count; ĉ++)
        {
            var Ԗ = Ƥ[ĉ];
            Ԗ.տ(վ);
        }
    }

    public void Ʀ(List<IMyArtificialMassBlock> Օ, List<IMySpaceBall> դ,
        List<IMyGravityGenerator> ր, List<IMyGravityGeneratorSphere> ց,
        IMyShipController ւ)
    {
        foreach (var Ċ in Օ)
            if (!Ԁ.Contains(Ċ))
            {
                Ԁ.Add(Ċ);
                ӿ.Add(new Ӿ(Ċ));
                Ċ.Enabled = Ʋ == Ԇ.ԇ ? true : false;
            }

        foreach (var Ċ in դ)
            if (!Ԏ.Contains(Ċ))
            {
                Ԏ.Add(Ċ);
                ą.Add(new ԏ(Ċ));
                Ċ.Enabled = Ʋ == Ԇ.ԇ ? true : false;
            }

        foreach (var Ċ in ր)
            if (!Ӷ.Contains(Ċ))
            {
                Ӷ.Add(Ċ);
                if (Ċ.WorldMatrix.Up == -å.WorldMatrix.Forward)
                    Ӱ.Add(new ӯ(Ċ, 1, "Forward"));
                else if (Ċ.WorldMatrix.Up == å.WorldMatrix.Forward)
                    Ӱ.Add(new ӯ(Ċ, -1, "Backward"));
                else if (Ċ.WorldMatrix.Up == -å.WorldMatrix.Left)
                    ӻ.Add(new ӯ(Ċ, -1, "Left"));
                else if (Ċ.WorldMatrix.Up == å.WorldMatrix.Left)
                    ӻ.Add(new ӯ(Ċ, 1, "Right"));
                else if (Ċ.WorldMatrix.Up == -å.WorldMatrix.Up)
                    Ԓ.Add(new ӯ(Ċ, 1, "Up"));
                else if (Ċ.WorldMatrix.Up == å.WorldMatrix.Up)
                    Ԓ.Add(new ӯ(Ċ, -1, "Down"));
            }

        for (var ĉ = Ӱ.Count - 1; ĉ >= 0; ĉ--)
        {
            var Ԗ = Ӱ[ĉ];
            if (Ԗ.Վ.Closed ||
                !ā.GridTerminalSystem.CanAccess(Ԗ.Վ))
            {
                Ӱ.RemoveAt(ĉ);
                Ӷ.Remove(Ԗ.Վ);
            }
        }

        for (var ĉ = ӻ.Count - 1; ĉ >= 0; ĉ--)
        {
            var Ԗ = ӻ[ĉ];
            if (Ԗ.Վ.Closed ||
                !ā.GridTerminalSystem.CanAccess(Ԗ.Վ))
            {
                ӻ.RemoveAt(ĉ);
                Ӷ.Remove(Ԗ.Վ);
            }
        }

        for (var ĉ = Ԓ.Count - 1; ĉ >= 0; ĉ--)
        {
            var Ԗ = Ԓ[ĉ];
            if (Ԗ.Վ.Closed ||
                !ā.GridTerminalSystem.CanAccess(Ԗ.Վ))
            {
                Ԓ.RemoveAt(ĉ);
                Ӷ.Remove(Ԗ.Վ);
            }
        }

        for (var ĉ = Ӳ.Count - 1; ĉ >= 0; ĉ--)
        {
            var Ԗ = Ӳ[ĉ];
            if (Ԗ.Վ.Closed ||
                !ā.GridTerminalSystem.CanAccess(Ԗ.Վ))
            {
                Ӳ.RemoveAt(ĉ);
                ӷ.Remove(Ԗ.Վ);
            }
        }


        var Ԕ = ւ.WorldMatrix.Forward;
        var ԕ = ւ.CenterOfMass;
        foreach (var Ċ in ց)
            if (!ӷ.Contains(Ċ))
            {
                ӷ.Add(Ċ);
                var ʘ = Ċ.WorldMatrix.Translation;
                var ԗ = ԕ - ʘ;
                if (Vector3D.Dot(ԗ, Ԕ) > 0)
                    Ӳ.Add(new ӱ(Ċ, -1, "Rear"));
                else
                    Ӳ.Add(new ӱ(Ċ, 1, "Forward"));
            }
    }


    public void ŝ(bool Ʋ)
    {
        for (var ĉ = ӿ.Count - 1; ĉ >= 0; ĉ--)
        {
            var Ճ = ӿ[ĉ];
            if (Ճ.Ċ.Closed || !Ӹ.CanAccess(Ճ.Ċ))
            {
                ӿ.RemoveAt(ĉ);
                Ԁ.Remove(Ճ.Ċ);
                continue;
            }

            Ճ.Ċ.Enabled = Ʋ ? Ճ.Ւ : false;
        }

        for (var ĉ = ą.Count - 1; ĉ >= 0; ĉ--)
        {
            var Ն = ą[ĉ];
            if (Ն.Ċ.Closed || !Ӹ.CanAccess(Ն.Ċ))
            {
                ą.RemoveAt(ĉ);
                Ԏ.Remove(Ն.Ċ);
                continue;
            }

            Ն.Ċ.Enabled = Ʋ;
        }
    }


    double Ԛ(List<ӯ> Ć)
    {
        var Ռ = 9.81 * Ć.Count;

        double Օ = 0;
        foreach (Ӿ Ċ in ӿ) Օ += Ċ.Ւ ? 50000 : 0;
        foreach (ԏ ԓ in ą) Օ += ԓ.Ċ.VirtualMass;
        return Օ * Ռ;
    }

    double Ԛ(List<ӯ> Ć,
        List<ӱ> փ, Vector3D ǎ)
    {
        var ք = Ԛ(Ć);


        var օ = Vector3D.Zero;
        foreach (ӱ Լ in փ)
        {
            var ֆ = Լ.Վ.GetPosition();
            var և = Vector3D.Zero;
            foreach (Ӿ Ċ in ӿ)
            {
                if (!Ċ.Ւ) continue;
                var ͺ = (ֆ - Ċ.Ċ.GetPosition()).Normalized();
                և += ͺ * 50000 * 9.81 * Լ.א;
            }

            foreach (ԏ ԓ in ą)
            {
                var ͺ = (ֆ - ԓ.Ċ.GetPosition()).Normalized();
                և += ͺ * ԓ.Ċ.VirtualMass * 9.81 * Լ.א;
            }

            օ += և;
        }

        var ב = Vector3D.Dot(ǎ, օ);

        return ք + ב;
    }


    

    enum Ԇ
    {
        Ԑ,
        ԇ,
        ԝ
    }

    
    enum Ը
    {
        Թ,
        Ո,
        Պ,
        Ջ
    }
}
public delegate void ה(ג ד);
public class ג
{
    private static readonly MyDefinitionId ו =
        new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Electricity");

    public IMyUserControllableGun ז;
    public bool ח;
    public bool ט;
    public float י;
    private readonly StringBuilder ך = new StringBuilder();
    StringBuilder כ = new StringBuilder();
    private readonly StringBuilder ל = new StringBuilder();
    private readonly float ם;
    public bool מ;
    public bool ן;
    public Vector3D נ;
    private readonly ה ס;
    private readonly MyResourceSinkComponent ע;

    public char ӛ = ' ';
    public double ף;
    public double פ;
    public double ľ = 0;
    public int ץ = 0;
    public bool צ;

    public bool ק;
    public double Ӗ = 0;
    float ר;
    public float ש;

    public ג(IMyUserControllableGun ד, Dictionary<MyDefinitionId, float> ת,
        ה ס, MyGridProgram ā, ushort ė)
    {
        ז = ד;
        ע = ז.Components.Get<MyResourceSinkComponent>();
        if (!ת.ContainsKey(ד.BlockDefinition))
            ם = 0f;
        else
            ם = ת[ד.BlockDefinition];
        this.ס = ס;
        נ = ז.Position;

        ӛ = (char)ė;


        מ = ז.IsFunctional;
        ף = ע.CurrentInputByType(ו);
        ח = מ && ף < 0.002f;

        ט = ח;
        ק = צ;
        ן = מ;
        פ = ף;

        ý.Ӟ(ӛ);
    }

    public bool Ւ
    {
        get { return ז.Enabled; }

        set
        {
            if (ז.Enabled != value) ז.Enabled = value;
        }
    }


    public bool װ => ז.Closed;

    public bool ױ => ז.IsFunctional;

    public void ƈ()
    {
        if (ח && צ)
        {
            ז.ShootOnce();
            ר = Math.Min(ר + 1f / 60f, ם);
        }
        else
        {
            if (ר > 0)
            {
                ס(this);
                צ = false;
                ר = 0;
            }
        }
    }

    public void Ƈ()
    {
        ט = ח;

        ן = מ;
        פ = ף;


        מ = ז.IsFunctional;
        ף = ע.CurrentInputByType(ו);
        ח = מ && ף < 0.002f;

        ײ(ח, ט, צ, ק, מ, ן, ף,
            פ);
        ק = צ;
        ל.Clear().Append(ז.DetailedInfo);


        var ؠ = ז.DetailedInfo.IndexOf("Stored power: ") + 14;
        if (ؠ + 6 > ז.DetailedInfo.Length)
        {
            י = 0;
            return;
        }

        ך.Clear().AppendSubstring(ל, ؠ, 6).Ȍ();
        י = float.Parse(ך.ToString()) / 50000f;
    }

    void ײ(bool ء, bool آ, bool أ, bool ؤ,
        bool إ, bool ئ, double ا, double ب)
    {
        if (أ != ؤ)
        {
            if (أ)
                ý.Ә(ӛ, Ӗ, ľ);
            else
                ý.ә(ӛ, Ӗ, ľ);
        }

        if (إ != ئ)
        {
            if (إ)
                ý.Ӝ(ӛ);
            else
                ý.ӝ(ӛ);
        }

        if (ا != ب)
        {
            if (ا < 0.002f)
            {
                if (ب > 0.002f && إ && ئ)
                    ý.ӕ(ӛ);
            }
            else
            {
                if (ب < 0.002f && ئ)
                    ý.Ӛ(ӛ, Ӗ, ľ);
            }
        }
    }


    public Vector3D ة()
    {
        return ז.GetPosition();
    }

    public float ت()
    {
        ש = ם - ר;
        return ש;
    }
}
public class k
{
    private const float ث = 0.002f;
    ג ج;

    private readonly Dictionary<ג, bool> ح;

    ushort خ = 1;
    bool د;
    ג ذ;
    int ر;

    int ز = -1;

    public List<float> Ŭ = new List<float>();

    public List<int> س = new List<int>();

    private readonly List<ג> l;
    private readonly List<IMyUserControllableGun> ش;
    private readonly Dictionary<MyDefinitionId, float> ת;
    private readonly MyGridProgram ā;
    private readonly float ص;

    MatrixD ض;
        
    public int ط;

    public enum ċ
    {
        ď,
        Ď,
        Č
    }

    public ċ Ɔ = ċ.Č;

    public k(List<IMyUserControllableGun> l, MyGridProgram ā,
        Dictionary<MyDefinitionId, float> ת, float ظ,ċ Đ,
        int ط)

    {

        Ɔ = Đ;
        ص = ظ / 60;
        ح = new Dictionary<ג, bool>();
        this.l = new List<ג>();
        ش = new List<IMyUserControllableGun>();
        this.ת = ת;
        foreach (var ד in l)
        {
            var ع = ד as IMyLargeTurretBase;
            if (ع == null)
            {
                this.l.Add(new ג(ד, ת, غ, ā, خ));
                خ++;
                ش.Add(ד);
            }
        }

        foreach (ג ד in this.l) ح[ד] = ד.ח;
        this.ط = ط;

        this.ā = ā;

        switch (Đ)
        {
            case ċ.ď:
                break;
            case ċ.Ď:
                ج = this.l[0];
                ذ = this.l[0];
                break;
            case ċ.Č:
                break;
        }
            

        ض = ā.Me.CubeGrid.WorldMatrix;
    }

    public void ơ(List<IMyUserControllableGun> l)
    {
        foreach (var ד in l)
        {
            var ع = ד as IMyLargeTurretBase;
            if (ع == null && !ش.Contains(ד))
            {
                ش.Add(ד);
                this.l.Add(new ג(ד, ת, غ, ā, خ));
                خ++;
            }
        }

        foreach (ג ד in this.l) ح[ד] = ד.ח;
    }

    void غ(ג ד)
    {
        var ؼ = ػ();
        if (ؼ != null) ذ = ؼ;
    }

    public void ƈ()
    {
            
            
        for (var ĉ = l.Count - 1; ĉ >= 0; ĉ--)
        {
            var ד = l[ĉ];
            if (ד == null || ד.װ)
            {
                l.RemoveAt(ĉ);
                ش.RemoveAt(ĉ);
                Ŭ.RemoveAt(ĉ);
                continue;
            }

            ד.ƈ();
        }
    }
        
    public void Ƈ(double ľ, double Ӗ)
    {

        if (l.Count == 0) return;
        ز = (ز + 1) % l.Count;

        var ד = l[ز];
        if (ד == null || ד.װ || !ā.GridTerminalSystem.CanAccess(ד.ז))
        {
            l.RemoveAt(ز);
            ش.RemoveAt(ز);
            Ŭ.RemoveAt(ز);
            return;
        }

        ד.Ƈ();
        ד.ľ = ľ;
        ד.Ӗ = Ӗ;
        while (Ŭ.Count < ز + 1) Ŭ.Add(0);
        Ŭ[ز] = ד.י;
    }

    public int Ɖ()
    {
        var ح = 0;
        for (var ĉ = l.Count - 1; ĉ >= 0; ĉ--)
        {
            var ד = l[ĉ];
            var ؽ = ד.ח;
            ح += ؽ ? 1 : 0;
            this.ح[ד] = ؽ;
        }

        if (Ɔ == ċ.Ď && د) ؾ();
        return ح;
    }


    void ؾ()
    {
        ر++;
        if (ر >= ط)
        {
            ر = 0;
            د = false;
            ؿ();
        }
    }

    void ؿ()
    {
        var ـ = l.IndexOf(ج);
        ج = l[(ـ + 1) % l.Count];
    }

    public Vector3D Ɗ(Vector3D ف)
    {
        if (Ɔ == ċ.Ď)
        {
            if (ذ == null) return ف;
            if (ح[ذ])
                return ذ.ة();
            return ف;
        }

        var ك = ق();
        var ε = Vector3D.Zero;
        var ل = 0;
        var م = 0;
        for (var ĉ = 0; ĉ < l.Count; ĉ++)
        {
            var ד = l[ĉ];
            if (ح[ד])
            {
                if (ד.ש - ص > ك)
                {
                    م++;
                    continue;
                }

                ;
                var ن = ד.ة();
                ε += ن;
                ل++;
            }
        }


        if (ل == 0) return ف;
        ε /= ل;

        return ε;
    }

    public float ق()
    {
        var ك = float.MaxValue;
        for (var ĉ = 0; ĉ < l.Count; ĉ++)
        {
            var ד = l[ĉ];
            if (ד.ח && ד.צ)
            {
                var ه = ד.ت();
                ك = Math.Min(ك, ه);
            }
        }

        return ك;
    }

    ג ػ()
    {
        ג و = null;
        var ك = float.MaxValue;
        for (var ĉ = 0; ĉ < l.Count; ĉ++)
        {
            var ד = l[ĉ];
            if (ד.ח && ד.צ)
            {
                var ه = ד.ت();
                if (ه < ك)
                {
                    ك = ه;
                    و = ד;
                }
            }
        }

        return و;
    }

    public void Ǐ()
    {

        switch (Ɔ)
        {
            case ċ.ď:
                for (var ĉ = 0; ĉ < l.Count; ĉ++)
                {
                    var ד = l[ĉ];
                    if (ح[ד])
                    {
                        ד.Ւ = true;
                        ד.צ = true;
                    }
                }
                break;
            case ċ.Ď:
                if (ح[ج])
                {
                    ج.Ւ = true;
                    ج.צ = true;
                    د = true;
                }
                break;
            case ċ.Č:
                var ى = true;
                    
                for (var ĉ = 0; ĉ < l.Count; ĉ++)
                {
                    var ד = l[ĉ];
                    if (ד.ף > 0.002f) ى = false;
                }

                if (ى)
                {
                    for (var ĉ = 0; ĉ < l.Count; ĉ++)
                    {
                        var ד = l[ĉ];
                        if (ح[ד])
                        {
                            ד.Ւ = true;
                            ד.צ = true;
                        }
                    }
                }
                break;
        }
    }

    public void ǐ()
    {
        for (var ĉ = 0; ĉ < l.Count; ĉ++)
        {
            var ד = l[ĉ];
            if (ح[ד])
            {
                ד.צ = false;
                ד.Ւ = false;
            }
            else
            {
                ד.צ = false;
                ד.Ւ = true;
            }
        }
    }

    public void Ǒ()
    {
        foreach (ג ד in l)
        {
            ד.צ = false;
            ד.Ւ = true;
        }
    }
}
public struct ӗ
{
    private readonly ushort ȇ;

    public ӗ(float ي)
    {
        ȇ = ٮ(ي);
    }

    public static explicit operator ӗ(float ي)
    {
        return new ӗ(ي);
    }

    public static explicit operator ӗ(double ٯ)
    {
        return new ӗ((float)ٯ);
    }

    public static explicit operator char(ӗ ٱ)
    {
        return (char)ٱ.ȇ;
    }

    private static ushort ٮ(float ي)
    {
        var ٲ = BitConverter.ToInt32(BitConverter.GetBytes(ي), 0);
        var א = (ٲ >> 16) & 0x8000;
        var ٳ = ((ٲ >> 23) & 0xFF) - 127 + 15;
        var ٴ = ٲ & 0x007FFFFF;

        if (ٳ <= 0)
        {
            ٴ = (ٴ | 0x00800000) >> (1 - ٳ);
            return (ushort)(א | (ٴ >> 13));
        }

        if (ٳ == 0xFF - (127 - 15))
            return (ushort)(א | 0x7C00);
        if (ٳ > 30)
            return (ushort)(א | 0x7C00);
        return (ushort)(א | (ٳ << 10) | (ٴ >> 13));
    }
}
public class Ā
{
    public static MyGridProgram ā;
    private readonly List<Vector3D> ٵ;

    private readonly List<Vector3D> ٶ;
    private readonly int ٷ;
    private readonly int ٸ;

    public Ā(List<Vector3D> ٶ, int ٷ, int ٸ)
    {
        this.ٶ = ٶ;
        this.ٷ = ٷ;
        this.ٸ = ٸ;
        ٵ = new List<Vector3D>();
    }

    public double ٻ(List<Vector3D>[] ٹ)
    {
        double ٺ = 0;
        for (var ĉ = 0; ĉ < ٷ; ĉ++)
            foreach (var Ѕ in ٹ[ĉ])
                ٺ += Math.Pow(Vector3D.Distance(Ѕ, ٵ[ĉ]), 2);
        return ٺ;
    }

    public List<Vector3D>[] ڂ()
    {
        ټ();
        var ٹ = new List<Vector3D>[ٷ];
        for (var ĉ = 0; ĉ < ٷ; ĉ++) ٹ[ĉ] = new List<Vector3D>();

        for (var ٽ = 0; ٽ < ٸ; ٽ++)
        {
            for (var ĉ = 0; ĉ < ٶ.Count; ĉ++)
            {
                var ٿ = پ(ٶ[ĉ]);
                ٹ[ٿ].Add(ٶ[ĉ]);
            }

            for (var ĉ = 0; ĉ < ٷ; ĉ++)
            {
                if (ٹ[ĉ].Count == 0)
                    continue;
                ٵ[ĉ] = ڀ(ٹ[ĉ]);
            }

            foreach (var ځ in ٹ) ځ.Clear();
        }

        return ٹ;
    }

    void ټ()
    {
        var ڃ = new Random();
        var ڄ = new HashSet<int>();

        while (ٵ.Count < ٷ)
        {
            var ξ = ڃ.Next(ٶ.Count);

            if (!ڄ.Contains(ξ))
            {
                ڄ.Add(ξ);
                ٵ.Add(ٶ[ξ]);
            }
        }
    }

    int پ(Vector3D Ѕ)
    {
        var څ = 0;
        var چ = double.MaxValue;

        for (var ĉ = 0; ĉ < ٷ; ĉ++)
        {
            var ʚ = Vector3D.Distance(Ѕ, ٵ[ĉ]);
            if (ʚ < چ)
            {
                چ = ʚ;
                څ = ĉ;
            }
        }

        return څ;
    }

    Vector3D ڀ(List<Vector3D> ځ)
    {
        if (ځ.Count == 0)
            return Vector3D.Zero;

        var ڇ = Vector3D.Zero;
        foreach (var Ѕ in ځ) ڇ += Ѕ;

        return ڇ / ځ.Count;
    }
}
public enum ڎ
{
    ڈ,
    ډ,
    ڊ,
    ڋ,
    ڌ,
    ڍ,
}
public enum ڒ
{
    ڏ,
    ڐ,
    ڑ
}
internal class ە
{
    public ڎ ړ = ڎ.ڈ;
        
    private readonly double ڔ;
        
    double ڕ = 150;
    int ږ = 50;
    int ڗ;
    int ژ = 60;
    double ڙ = 7;
    double ښ = 0.5;
    double ڛ = 0.1;
        
        
        
        
        
       
    public readonly bool ڜ;
        
    int ڝ;
    int ڞ;
    Vector3D ڟ;
    Vector3D ڠ;
    Vector3D ڡ;
    Vector3D ڢ;
    Vector3D Δ;
    Vector3D ڣ;

    public readonly IMyGyro ڤ;
        
    
    public readonly IMyGasTank[] ڥ;
        
    IMyTerminalBlock[] ڦ;
    private readonly IMyBatteryBlock[] ڧ;
    private readonly IMyShipConnector[] ڨ;
    private readonly IMyThrust[] ک;
    private readonly IMyWarhead[] ڪ;
    private readonly IMyShipMergeBlock[] ګ; 
    IMyCubeGrid ڬ;

    public bool מ;
        


    private readonly int ڭ;
        
        

        
        


    double ڮ;
    double گ;
    double ڰ;
    public bool ʒ;
        

    private readonly double ڱ = 60;
        

        
    public readonly ڒ ڒ = ڒ.ڏ;
    public readonly string ڲ = "";
    private readonly ڳ ڴ;


    private readonly double ڵ = 5.0;
        

    double ڶ;
    double ڷ;

    private readonly IMyThrust ڸ;
        
    public Vector3D ڹ = Vector3D.Zero;
    public double ں = 0;
        
        
    public L ڻ; 
    public J ڼ;

    public Í ڽ = Í.Î;
    public Dictionary<J, Dictionary<Í, int>> ھ = new Dictionary<J, Dictionary<Í, int>>();
       
        

    private readonly double ڿ = 1 / 60.0;


    private readonly List<Vector3D> ۀ = new List<Vector3D>();
    private readonly ڳ ہ;

    public ە(L ۂ, List<IMyTerminalBlock> գ, ۃ ۄ, double ۅ, string Ĺ, bool ۆ)
    {
        this.ڲ = Ĺ;
        ڻ = ۂ;
        this.ڦ = գ.ToArray();
        if (գ.Count == 0) return;
        var ۇ = new List<IMyThrust>();
        var ۈ = new List<IMyBatteryBlock>();
        var ۉ = new List<IMyGasTank>();
        var ۊ = new List<IMyWarhead>();
        var ۋ = new List<IMyShipMergeBlock>();
        var ی = new List<IMyShipConnector>();

        double Օ = 0;
        double ք = 0;
        foreach (var Ċ in գ)
        {
            Օ += Ċ.Mass;
            if (Ċ is IMyThrust)
            {
                ۇ.Add(Ċ as IMyThrust);

                switch (Ċ.BlockDefinition.SubtypeName)
                {
                    case "SmallBlockSmallAtmosphericThrust":
                        ڒ = ڒ.ڑ;
                        break;
                    case "SmallBlockSmallHydrogenThrust":
                        ڒ = ڒ.ڐ;
                        break;
                }

                ք += (Ċ as IMyThrust).MaxEffectiveThrust;
            }
            else if (Ċ is IMyBatteryBlock)
            {
                ۈ.Add(Ċ as IMyBatteryBlock);
            }
            else if (Ċ is IMyGyro)
            {
                ڤ = Ċ as IMyGyro;
                if (ڤ.CustomData == "KMM") return;
                 ڬ = ڤ.CubeGrid;
            }
            else if (Ċ is IMyGasTank)
            {
                var ۍ = Ċ as IMyGasTank;
                if (ۍ.FilledRatio > ښ) ۍ.Stockpile = false;
                else ۍ.Stockpile = true;
                ۉ.Add(ۍ);
            }
            else if (Ċ is IMyWarhead)
            {
                ۊ.Add(Ċ as IMyWarhead);
            }
            else if (Ċ is IMyShipMergeBlock)
            {
                ۋ.Add(Ċ as IMyShipMergeBlock);
            }
            else if (Ċ is IMyShipConnector)
            {
                var ێ = Ċ as IMyShipConnector;
                ێ.Connect();
                ی.Add(ێ);
            }
        }

        ک = ۇ.ToArray();
        ڧ = ۈ.ToArray();
        ڥ = ۉ.ToArray();
        ڪ = ۊ.ToArray();
        ګ = ۋ.ToArray();
        ڨ = ی.ToArray();

        ڴ = new ڳ(ۄ.ۏ, ۄ.ې, ۄ.ۑ, Program.ò);
        ہ = new ڳ(ۄ.ۏ, ۄ.ې, ۄ.ۑ, Program.ò);

        ڔ = ք / Օ;


        foreach (var ے in ڥ)
        {
            if (ے.FilledRatio > ښ)
            {
                ے.Stockpile = false;
            }
        }
            
        ڜ =
            (ڒ == ڒ.ڏ && ک.Length > 0 && ڧ.Length > 0 &&
             ڪ.Length > 0 && ګ.Length > 0 && ڤ != null)
            || (ڒ == ڒ.ڐ && ک.Length > 0 && ڧ.Length > 0 &&
                ڥ.Length > 0 && ڪ.Length > 0 && (ګ.Length > 0 || ۆ || ڨ.Length > 0) &&
                ڤ != null)
            || (ڒ == ڒ.ڑ && ک.Length > 0 && ڧ.Length > 0 &&
                ڪ.Length > 0 && ګ.Length > 0 && ڤ != null);

            

        if (!ڜ) return;
        ڸ = ک[0];
        this.ڵ = ۅ;
        this.ڭ = ۄ.ۓ;

    }



    public void ۥ()
    {
        foreach (var ے in ڥ)
        {
            if (ے.FilledRatio > ښ)
            {
                ے.Stockpile = false;
            }
        }
        foreach (var ێ in this.ڨ)
            if (!ێ.IsConnected)
                ێ.Connect();
    }

    public void ۦ(Vector3D Ȩ)
    {
        ڡ = ڠ;
        Δ = ڢ;
        ڢ = (ڠ - ڡ) / Program.ò;
        ڟ = Ȩ - ڠ;
    }
    public void ŵ(Vector3D վ, Vector3D ۮ)
    {
        Ğ.Ȅ("Missile updating");

            
        ڗ++;
            
            

            
        var ۯ = this.ک;
        var ۺ = this.ڧ;
        var ۻ = this.ڥ;
        var ۼ = this.ڪ;
        var ۿ = this.ګ;
        var ܐ = this.ڨ;
        מ = ܒ(ۯ, ۺ, ۻ, ۼ, ۿ, ܐ, ڤ);


        var ܓ = ڟ.Ʈ();

        Ğ.Ȅ(ړ);
        switch (ړ)
        {
            case ڎ.ڈ:
                break;
            case ڎ.ډ:
                if (ڝ < ڭ)
                {
                    ڤ.GyroOverride = true;
                    ڝ++;
                    if (ڤ.Pitch != 0 || ڤ.Yaw != 0 || ڤ.Roll != 0)
                    {
                        ڤ.Pitch = 0;
                        ڤ.Yaw = 0;
                        ڤ.Roll = 0;
                    }

                    foreach (var ܔ in ۯ)
                    {
                        ܔ.Enabled = true;
                        ܔ.ThrustOverride = 1000000;
                    }
                }
                else
                {
                    ړ = ڎ.ڊ;
                }

                break;
            case ڎ.ڊ:

                ܕ(ۀ[0],(ۀ[0] - ڣ) / Program.ò,Vector3D.Zero);
                ڣ = ۀ[0];
                ܖ(ܓ);
                break;
            case ڎ.ڋ:
                    
                ܕ(ڼ.Œ(ڽ,
                    ھ[ڼ][ڽ]), ڼ.Ĝ, ڼ.Γ);

                ܗ(ܓ);
                break;
            case ڎ.ڌ:
                ڌ(վ);

                break;
            case ڎ.ڍ:
                ڍ(վ, ۮ);
                break;
        }
    }

        

    public void ܚ()
    {
        if (!ʒ || ڗ < ژ)
        {
            var ܘ = this.ڪ;
            for (var ĉ = 0; ĉ < ܘ.Length; ĉ++)
            {
                var ܙ = ܘ[ĉ];
                ܙ.IsArmed = false;
            }
            return;
        }
        var ۼ = this.ڪ;
        for (var ĉ = 0; ĉ < ۼ.Length; ĉ++)
        {
            var ܙ = ۼ[ĉ];
            ܙ.IsArmed = true;
            ܙ.Detonate();
        }
    }

    bool ܒ(IMyTerminalBlock[] գ)
    {
        var е = գ.Length;
        for (var ĉ = 0; ĉ < е; ĉ++)
        {
            var Ċ = գ[ĉ];
            if (Ċ.Closed)
            {
                ܚ();
                return false;
            }
        }

        return true;
    }

    bool ܒ(IMyThrust[] ۯ, IMyBatteryBlock[] ۺ, IMyGasTank[] ۻ,
        IMyWarhead[] ۼ, IMyShipMergeBlock[] ۿ, IMyShipConnector[] ܐ, IMyGyro Ƕ)
    {
        var ܛ = ۯ;
        var е = ܛ.Length;
        for (var ĉ = 0; ĉ < е; ĉ++)
            if (ܛ[ĉ].Closed)
            {
                ܚ();
                return false;
            }

        var ܜ = ۺ;
        е = ܜ.Length;
        for (var ĉ = 0; ĉ < е; ĉ++)
            if (ܜ[ĉ].Closed)
            {
                ܚ();
                return false;
            }

        var ܝ = ۻ;
        е = ܝ.Length;
        for (var ĉ = 0; ĉ < е; ĉ++)
            if (ܝ[ĉ].Closed || ܝ[ĉ].FilledRatio == 0)
            {
                ܚ();
                return false;
            }

        var ܘ = ۼ;
        е = ܘ.Length;
        for (var ĉ = 0; ĉ < е; ĉ++)
            if (ܘ[ĉ].Closed)
            {
                ܚ();
                return false;
            }

        var ܞ = ۿ;
        е = ܞ.Length;
        for (var ĉ = 0; ĉ < е; ĉ++)
            if (ܞ[ĉ].Closed)
            {
                ܚ();
                return false;
            }

        var ܟ = ܐ;
        е = ܟ.Length;
        for (var ĉ = 0; ĉ < е; ĉ++)
            if (ܟ[ĉ].Closed)
            {
                ܚ();
                return false;
            }

        var ܠ = Ƕ;
        if (!ܠ.IsFunctional || ܠ.Closed)
        {
            ܚ();
            return false;
        }

        return true;
    }

    void ܡ(L Ι)
    {
        Vector3D ʢ = Ι.ž + ڹ * ں;
        ܕ(ʢ, Ι.Ĝ, Ι.Γ);
        var ܓ = (ʢ - ڠ).Ʈ();
        if (ܓ < ڙ * ڙ) ړ = ڎ.ڌ;
    }

    void ڍ(Vector3D վ, Vector3D ۮ)
    {
            
        ܢ();
        if (վ == Vector3D.Zero)
        {
            ܣ();
        }
        else
        {
            ڤ.GyroOverride = true;
            Ǔ(-ۮ, ڱ, false);
        }
            
            
    }

    void ܣ()
    {
        foreach (var ܔ in ک) ܔ.ThrustOverride = 0;
        ڤ.GyroOverride = false;
    }

    public int ܧ()
    {
        if (ڒ == ڒ.ڐ)
        {

            bool ܤ = false;
            foreach (var ے in ڥ)
            {
                if (ے.FilledRatio < ڛ)
                {
                    ے.Stockpile = true;
                    ܤ = true;
                }
                    
            }
            if (ܤ) return 1;
        }
            
        ʒ = true;
        foreach (var ܔ in ک)
        {
            ܔ.Enabled = true;
            ܔ.ThrustOverridePercentage = 1;
        }

        foreach (var ܥ in ڧ) ܥ.Enabled = true;
        foreach (var ܦ in ګ) ܦ.Enabled = false;
        foreach (var ێ in ڨ)
        {
            ێ.Disconnect();
            ێ.Enabled = false;
        }

        foreach (var ے in ڥ) ے.Stockpile = false;
        ړ = ڎ.ډ;
        return 0;
    }

    Vector3D ܨ = Vector3D.Zero;

    public void ܪ(Vector3D ܩ)
    {
        ܨ = ܩ;
    }

    public void ܬ(Vector3D ܫ)
    {
        ۀ.Add(ܫ);
    }

    void ڌ(Vector3D վ)
    {
            

        var ܭ = Vector3D.Normalize(ڢ);


        Ǔ(-ܭ, ڱ, true);

        double ܮ = Math.Pow(ܭ.Dot(ک[0].WorldMatrix.Forward), ڕ);
            
        var ܯ = ܮ == 0 ? 0 : (float)(ܮ * 10 * ڔ);

        var ݍ = ڢ.Cross(ک[0].WorldMatrix.Forward);
        foreach (var ܔ in ک) ܔ.ThrustOverridePercentage = ܯ;


        if (ڢ.Ʈ() < 0.5 * 0.5)
        {
            ܣ();
            ړ = ڎ.ڍ;
        }
    }


    void ܕ(Vector3D Ȩ, Vector3D ܩ, Vector3D ݎ)
    {


        var ݏ = Vector3D.Normalize((Ȩ - ܩ) - ڡ);
        var ݐ = Vector3D.Normalize(Ȩ - ڠ);

        var ݑ = new Vector3D(1, 0, 0);
        double ݒ;
        Vector3D ݓ;
        var ݔ = ک[0].WorldMatrix.Backward;

        if (ݏ.Ʈ() == 0)
        {
            ݓ = new Vector3D(0, 0, 0);
            ݒ = 0.0;
        }
        else
        {
            ݓ = ݐ - ݏ;
            ݒ = Math.Sqrt(ݓ.Ʈ()) / ڿ;
        }


        var ݕ = Math.Sqrt((ܩ - ڢ).Ʈ());
        var Ϻ = ڢ.Ə();
        var ݖ = -ڻ.ǋ.GetNaturalGravity();
        var ݗ = Vector3D.Cross(Vector3D.Cross(ܩ - ڢ, ݐ),
            ܩ - ڢ);
        var ݘ = ݗ.Ʈ();
        if (ݘ != 0) ݗ /= Math.Sqrt(ݘ);

        var ݙ =
            ݗ * ڵ * ݒ * ݕ + ݓ * 9.8 * (0.5 * ڵ);

        var ݚ = Math.Sqrt(ݙ.Ʈ()) / ڔ;
        if (ݚ > 0.98)
            ݙ = ڔ * Vector3D.Normalize(ݙ +
                                                                      ݚ *
                                                                      Vector3D.Normalize(-ڢ) * 40);

        var ݛ = Math.Pow(Vector3D.Dot(ݔ, Vector3D.Normalize(ݙ)),
            4);
        ݛ =
            MathHelper.Clamp(ݛ, MathHelper.Clamp(ڕ - Ϻ, 0, 1),
                1);
        ݛ = MathHelper.RoundToInt(ݛ * 10) / 10d;

        for (var ĉ = 0; ĉ < ک.Length; ĉ++)
        {
            var ܔ = ک[ĉ];
            if (ܔ.ThrustOverridePercentage != (float)ݛ)
                ܔ.ThrustOverridePercentage = (float)ݛ;
        }

        var ݜ = ݙ.Ʈ();
        var ݝ = Math.Sqrt(ڔ * ڔ - ݜ);
        if (double.IsNaN(ݝ)) ݝ = 0;
        ݙ += ݐ * ݝ;


        ݑ = Vector3D.Normalize(ݙ + ݖ);
            
        Ǔ(ݑ, ڱ, false);
    }

    void ܗ(double ܓ)
    {
        if (ڗ < ژ) return;
        if (ܓ < 50 * 50)
        {
            foreach (var ܙ in ڪ) ܙ.IsArmed = true;
            if (ڪ.Length <= ڞ) return;
            if (ܓ < 15 * 15)
            {
                ڪ[ڞ].Detonate();
                ڞ++;
            }
        }
    }

    void ܖ(double ܓ)
    {
        if (ܓ < 10 * 10)
        {
            if (ۀ.Count == 0) return;
            ۀ.RemoveAt(0);
        }
    }

    void ܢ()
    {
        for (var ĉ = 0; ĉ < ڪ.Length; ĉ++)
        {
            var ܙ = ڪ[ĉ];
            ܙ.IsArmed = false;
        }
    }

    void Ǔ(Vector3D ǡ, double ݞ, bool ө)
    {
        if (double.IsNaN(ǡ.X) || double.IsNaN(ǡ.Y) ||
            double.IsNaN(ǡ.Z)) return;

        double ǥ;
        double Ǧ;

        var ݟ = this.ڸ;
        var ݠ = ݟ.WorldMatrix;

        var Ǩ = Vector3D.Cross(ݠ.Backward, ǡ);
        var ǩ = Vector3D.TransformNormal(Ǩ, MatrixD.Transpose(ݠ));
        var ǫ = ڴ.ц(-ǩ.X);
        var Ǭ = ہ.ц(-ǩ.Y);
            
        ǥ = MathHelper.Clamp(ǫ, -ݞ, ݞ);
        Ǧ = MathHelper.Clamp(Ǭ, -ݞ, ݞ);

        if (Math.Abs(Ǧ) + Math.Abs(ǥ) > ݞ)
        {
            var ǭ = ݞ / (Math.Abs(Ǧ) + Math.Abs(ǥ));
            Ǧ *= ǭ;
            ǥ *= ǭ;
        }


        Ǯ(ǥ, Ǧ, ڤ, ݟ.WorldMatrix, ө);
    }

    void Ǯ(double ǯ, double ǰ, IMyGyro Ƕ, MatrixD ǳ, bool ө)
    {
        if (!ө)
        {
            ǯ = MathHelper.RoundToInt(ǯ * 10) / 10d;
            ǰ = MathHelper.RoundToInt(ǰ * 10) / 10d;
        }
            

        if (ǯ == ڶ && ǰ == ڷ) return;
        ڶ = ǯ;
        ڷ = ǰ;

        var Ǵ = new Vector3D(ǯ, ǰ, 0);
        var ǵ = Vector3D.TransformNormal(Ǵ, ǳ);


        if (Ƕ.IsFunctional && Ƕ.IsWorking && Ƕ.Enabled && !Ƕ.Closed)
        {
            var Ƿ =
                Vector3D.TransformNormal(ǵ, MatrixD.Transpose(Ƕ.WorldMatrix));
            if (ө)
            {
                Ƕ.Pitch = (float)Ƿ.X;
                ڮ = Ƿ.X;
                Ƕ.Yaw = (float)Ƿ.Y;
                ڰ = Ƿ.Y;
                Ƕ.Roll = (float)Ƿ.Z;
                گ = Ƿ.Z;
                return;
            }
                
            if (Math.Abs(Ƿ.X - ڮ) > 0.05)
            {
                Ƕ.Pitch = (float)Ƿ.X;
                ڮ = Ƿ.X;
            }

            if (Math.Abs(Ƿ.Y - ڰ) > 0.05)
            {
                Ƕ.Yaw = (float)Ƿ.Y;
                ڰ = Ƿ.Y;
            }

            if (Math.Abs(Ƿ.Z - گ) > 0.05)
            {
                Ƕ.Roll = (float)Ƿ.Z;
                گ = Ƿ.Z;
            }

            Ƕ.GyroOverride = true;
        }
    }



    public Vector3D ة()
    {
        return ڤ.CubeGrid.WorldVolume.Center;
    }

    public Vector3D ݡ()
    {
        return ک[0].WorldMatrix.Backward;
    }

    public u.ɾ ݢ()
    {
        switch (ڒ)
        {
            case ڒ.ڏ:
                return new u.ɾ(ڠ, ڡ, ڟ,
                    ک[0].WorldMatrix.Backward,
                    ڧ[0].CurrentStoredPower / ڧ[0].MaxStoredPower, ڤ.EntityId, ʒ,
                    false);
            case ڒ.ڐ:
                return new u.ɾ(ڠ, ڡ, ڟ,
                    ک[0].WorldMatrix.Backward, Math.Min(1, ڥ[0].FilledRatio / ښ), ڤ.EntityId, ʒ,
                    true);
            case ڒ.ڑ:
                return new u.ɾ(ڠ, ڡ, ڟ,
                    ک[0].WorldMatrix.Backward,
                    ڧ[0].CurrentStoredPower / ڧ[0].MaxStoredPower, ڤ.EntityId, ʒ,
                    false);
        }

        return new u.ɾ(ڠ, ڡ, ڟ,
            ک[0].WorldMatrix.Backward, ڧ[0].CurrentStoredPower / ڧ[0].MaxStoredPower,
            ڤ.EntityId, ʒ, false);
    }

    public Vector3D ݣ()
    {
        return ڤ.GetPosition();
    }

    public Vector3D ݤ()
    {
        return ک[0].GetPosition();
    }

    public bool ݥ()
    {
        for (int ĉ = 0; ĉ < ڦ.Length; ĉ++)
        {
            var Ċ = ڦ[ĉ];
            if (!Ċ.IsFunctional) return false;
        }

        if (!ڤ.IsFunctional) return false;
        return true;
    }

    public void ݦ()
    {
        Vector3D Ȩ = ڻ.ž + ڹ * ں;
        ۦ(Ȩ);
        if 
        (
            ړ == ڎ.ڋ 
            || 
            (
                (
                    ړ == ڎ.ڍ 
                    || 
                    ړ == ڎ.ڌ
                )
                && 
                (Ȩ - ڠ).LengthSquared() > ږ * ږ
            )
            || 
            (
                ړ == ڎ.ڊ 
                && ۀ.Count == 0
            )
        )
        {
        }

    }
    public void ݧ()
    {
        Vector3D Ȩ = ڼ.ž + ڹ * ں;
        ۦ(Ȩ);
        if 
        (
            ړ == ڎ.ڋ 
            || 
            (
                (
                    ړ == ڎ.ڍ 
                    || 
                    ړ == ڎ.ڌ
                )
                &&
                (Ȩ - ڠ).LengthSquared() > ږ * ږ
            )
            || 
            (
                ړ == ڎ.ڊ 
                && ۀ.Count == 0
            )
        )
        {
        }
    }

    public void ڋ()
    {
        Vector3D ʢ = ڼ.Œ(ڽ,
            ھ[ڼ][ڽ]);
        ۦ(ʢ);
        if 
        (
            ړ != ڎ.ڋ 
            && 
            ړ != ڎ.ڈ 
            &&
            ړ != ڎ.ډ
        )
        {
            ړ = ڎ.ڋ;
        } 
    }


}
public enum ݫ
{
    ݨ,
    ݩ,
    ݪ
}
public enum ݯ
{
    ݬ,
    ݭ,
    ݮ
}
public class ۃ
{
    public static Dictionary<ݰ, ۃ> ݴ =
        new Dictionary<ݰ, ۃ>
        {
            { ݰ.ݱ, new ݱ() },
            { ݰ.ݲ, new ݲ() },
            { ݰ.ݳ, new ݳ() },
        };
    public double ݵ;
    public double ݶ;
    public double ݷ;
    public double ݸ;
    public double ݹ;
    public double ݺ = 50;
    public double ݻ = 300;
    public double ݼ = 140;
    public double ۏ = 12;
    public double ې = 0;
    public double ۑ = 0;
    public double ݽ = 5;
    public double ݾ = 5;
    public int ݿ = 5;
    public int ۓ = 60;
        
    public ݫ ݫ;
    public ݯ ݯ;
}
public enum ݰ
{
    ݱ,
    ݲ,
    ݳ
}
public class ݱ : ۃ
{
    public ݱ()
    {
        ݵ = 500;
        ݶ = 500;
        ݷ = 0;
        ݸ = 0;
        ݹ = 180;
        ݫ = ݫ.ݨ;
        ݯ = ݯ.ݬ;
    }
}
public class ݲ : ۃ
{
    public ݲ()
    {
        ݵ = 1500;
        ݶ = 3000;
        ݷ = 0;
        ݸ = 100;
        ݹ = 180;
        ݫ = ݫ.ݩ;
        ݯ = ݯ.ݮ;
    }
}
public class ݳ : ۃ
{
    public ݳ()
    {
        ݵ = 1500;
        ݶ = 11000;
        ݷ = 5000;
        ݸ = 85;
        ݹ = 120;
        ݫ = ݫ.ݪ;
        ݯ = ݯ.ݮ;
    }
}
public enum ށ
{
    ހ,
    ŕ
}
internal class u
{

    private readonly int ނ = 16;

    private readonly List<ە> ރ;
    private readonly Dictionary<ە, int> ބ;
    private readonly List<ە> ޅ;

    int ڗ;
    bool ކ;
    private readonly Random އ;

    J ވ;
    public List<J> މ; 

    L M;

    private readonly List<string> ފ = new List<string>();

    private readonly Dictionary<string, bool> ދ = new Dictionary<string, bool>();

    Í ތ = Í.Î;

    private readonly Dictionary<string, ލ> ގ =
        new Dictionary<string, ލ>();

    ۃ ޏ =
        ۃ.ݴ[ݰ.ݲ];

    bool ސ;


    public u(IMyGridTerminalSystem ޑ, L ۂ)
    {
        M = ۂ;
        އ = new Random();
        މ = new List<J>();
        ރ = new List<ە>();
        ބ = new Dictionary<ە, int>();
        ޅ = new List<ە>();
        for (var ĉ = 1; ĉ <= ނ; ĉ++)
        {
            var ޒ = "M" + ĉ;
            var ޓ = "DM" + ĉ;

            ފ.Add(ޒ);

            ދ.Add(ޒ, false);
            var ޔ = ޑ.GetBlockGroupWithName(ޒ);
            if (ޔ != null)
            {
                var ޕ = new List<IMyGyro>();
                ޔ.GetBlocksOfType(ޕ);
                foreach (var Ƕ in ޕ) Ƕ.CustomData = "";
            }

            var ޖ = ޑ.GetBlockGroupWithName(ޓ);
            if (ޖ != null)
            {
                var ޗ = new List<IMyThrust>();
                ޖ.GetBlocksOfType(ޗ);
                ގ.Add(ޒ, new ލ(ޗ));
            }
        }
    }

    public void ޙ(ݰ ޘ)
    {
        ޏ = ۃ.ݴ[ޘ];





        switch (ޘ)
        {
            case ݰ.ݱ:
                break;
            case ݰ.ݳ:
                break;
            case ݰ.ݲ:
                break;
        }
    }

    public void ƚ(J ޚ)
    {
        ވ = ޚ;
    }
    public void ޜ()
    {
        ޛ();
    }


    void ޛ()
    {
        switch (ތ)
        {
            case Í.Î:
                ތ = Í.ƞ;
                break;
            case Í.ƞ:
                ތ = Í.Ɵ;
                break;
            case Í.Ɵ:
                ތ = Í.Ơ;
                break;
            case Í.Ơ:
                ތ = Í.Î;
                break;
        }
    }

    public void ޟ(J ޝ)
    {
        foreach (ە ʑ in ޅ) ޞ(ʑ, ޝ);
    }

    void ޡ(ە ʑ, List<J> ޠ)
    {
        foreach (J Ι in ޠ) ޞ(ʑ, Ι);
    }


    void ޞ(ە ʑ, J Ι)
    {
        if (Ι == null)
        {
            return;
        }


        var ޣ = new Dictionary<Í, int>
        {
            {
                Í.Î,
                ޢ(Ι, Í.Î)
            },
            {
                Í.Ɵ,
                ޢ(Ι, Í.Ɵ)
            },
            {
                Í.ƞ,
                ޢ(Ι, Í.ƞ)
            },
            {
                Í.Ơ,
                ޢ(Ι, Í.Ơ)
            }
        };


        ʑ.ھ[Ι] = ޣ;
    }


    int ޢ(J Ι, Í ά)
    {
        var ξ = -1;
        switch (ά)
        {
            case Í.Î:
                if (Ι.Ŧ == 0) break;
                ξ = އ.Next(0, Ι.Ŧ - 1);
                break;
            case Í.ƞ:
                if (Ι.ũ == 0) break;
                ξ = އ.Next(0, Ι.ũ - 1);
                break;
            case Í.Ɵ:
                if (Ι.ŧ == 0) break;
                ξ = އ.Next(0, Ι.ŧ - 1);
                break;
            case Í.Ơ:
                if (Ι.Ũ == 0) break;
                ξ = އ.Next(0, Ι.Ũ - 1);
                break;
        }

        return ξ;
    }

    public void ŵ(J ޤ)
    {
        ڗ++;

        var վ = M.ǋ.GetNaturalGravity();
        var ۮ = վ.Normalized();
            

        ޥ(ޤ, վ, ۮ);
        ޱ();
        ߊ();
    }

    void ޱ()
    {
        if ((ڗ + 32) % 9 == 0)
            foreach (var Ĺ in ފ)
            {
                if (ދ[Ĺ]) continue;
                var i = Program.E.GetBlockGroupWithName(Ĺ);
                if (i == null) continue;
                var գ = new List<IMyTerminalBlock>();
                i.GetBlocksOfType(գ);
                    
                var ʑ = new ە(M, գ, ޏ,
                    އ.NextDouble() * (ޏ.ݾ -
                                         ޏ.ݽ) +
                    ޏ.ݽ, Ĺ, false);
                if (ʑ == null || !ʑ.ڜ) continue;
                ދ[Ĺ] = true;
                ރ.Add(ʑ);
            }
    }


        
    void ޥ(J Ι, Vector3D վ, Vector3D ۮ)
    {
        switch (ޏ.ݫ)
        {
            case ݫ.ݨ:
                ݨ(Ι, վ, ۮ);
                break;

            case ݫ.ݩ:
                ݩ(Ι, վ, ۮ);
                break;

            case ݫ.ݪ:
                ݪ(վ, ۮ);
                break;
        }
    }
        
        
    void ݨ(J Ι, Vector3D վ, Vector3D ۮ)
    {
            
        if (Ι == J.Î)
            foreach (ە ʑ in ޅ)
            {
                ʑ.ݦ();
                ʑ.ڼ = Ι;
                ʑ.ŵ(վ, ۮ);
            }
        else
            foreach (ە ʑ in ޅ)
            {
                ʑ.ڋ();
                ʑ.ڼ = Ι;
                ʑ.ŵ(վ, ۮ);
            }

    }

    void ݩ(J Ι, Vector3D վ, Vector3D ۮ)
    {
        if (Ι == J.Î)
            foreach (ە ʑ in ޅ)
            {
                ʑ.ݦ();
                ʑ.ڼ = Ι;
                ʑ.ŵ(վ, ۮ);
                Ğ.Ȅ("Loitering around host?");
            }
        else if (ސ)
            foreach (ە ʑ in ޅ)
            {
                ʑ.ڋ();
                ʑ.ڼ = Ι;
                ʑ.ŵ(վ, ۮ);
                Ğ.Ȅ("Attacking target?");
            }
        else
            foreach (ە ʑ in ޅ)
            {
                ʑ.ݧ();
                ʑ.ڼ = Ι;
                ʑ.ŵ(վ, ۮ);
                Ğ.Ȅ("Surrounding target?");
            }
    }


    void ݪ(Vector3D վ, Vector3D ۮ)
    {
        foreach (ە ʑ in ޅ)
        {
            var ʢ = ߋ(މ, ʑ);
            ʑ.ڼ = ʢ;
            if (ʢ == J.Î || !ސ)
                ʑ.ݦ();
            else
                ʑ.ڋ();
            ʑ.ŵ(վ, ۮ);
        }
    }

    J ߋ(List<J> ޠ, ە ʑ)
    {
        var چ = double.MaxValue;
        var ߌ = J.Î;
        foreach (J ߍ in ޠ)
        {
            var ߎ = (ߍ.ž - ʑ.ة()).LengthSquared();

            if (!ߍ.Ε || !(ߎ <
                                                    ޏ.ݷ *
                                                    ޏ.ݷ)) continue;

            if (ߎ < چ)
            {
                چ = ߎ;
                ߌ = ߍ;
            }
        }

        return ߌ;
    }

    public void ś()
    {
        foreach (ە ʑ in ޅ)
        {
            if (!ʑ.ʒ) return;
            ʑ.ܚ();
        }
    }













    public void Ŕ(MyGridProgram ā, Vector3D ߏ, Vector3D Ľ, Vector3D ǎ)
    {
        ߐ();

        var ߑ = false;
        var ߒ = 0;
        switch (ޏ.ݯ)
        {
            case ݯ.ݬ:
                ߑ = true;
                break;
            case ݯ.ݭ:
                ߒ = 0;
                break;
            case ݯ.ݮ:
                ߒ = ޏ.ݿ;
                break;
        }

            
            
        for (var ξ = ރ.Count - 1; ξ >= 0; ξ--)
        {
            var ʑ = ރ[ξ];
            if (!ʑ.ݥ()) continue;

            ބ.Add(ʑ, ߒ);

            if (ߑ) return;
        }
    }

    void ߐ()
    {
    }

    void ߊ()
    {
        var ߓ = new List<ە>();

        foreach (var ŀ in ބ)
        {
            var ʑ = ŀ.Key;
            var ߔ = ŀ.Value;
            if (ߔ > 0)
            {
                ބ[ʑ] = ߔ - 1;
                return;
            }

                
            var ߖ = ߕ(ʑ, M);
            switch (ߖ)
            {
                case 0:
                    ߓ.Add(ʑ);
                    break;
                case 1:
                    break;
                case 2:
                    break;
            }
        }
            
        ߓ.ForEach(ǫ => { ބ.Remove(ǫ); ޅ.Add(ǫ); });
    }


    int ߕ(ە ʑ, L ۂ)
    {
        var ߗ = ʑ.ݡ();
        if (ߗ.Dot(ۂ.Ĝ) > ޏ.ݼ ||
            ߗ.Cross(ۂ.Ĝ).Ʈ() >
            ޏ.ݼ * ޏ.ݼ) return 1;


        if (ވ != null) ޞ(ʑ, ވ);
        var ߘ = ʑ.ܧ();
            
        switch (ߘ)
        {
            case 1:
                return 2;
        }
            
        ދ[ʑ.ڲ] = false;
            
            
        var ߙ = ʑ.ݤ();
        var ߚ = ߙ + ߗ * ޏ.ݺ;
        var ߛ = ߚ + (ߚ - ۂ.ž).Normalized() * ޏ.ݻ;
        var ߝ = ߜ(Vector3D.Zero);
            
        var վ = -M.ǋ.GetNaturalGravity();
        var ߞ = վ.Length();
        if (ߞ > 0)
            ߝ = ߟ(վ / ߞ, ޏ.ݸ, ޏ.ݹ);

        ʑ.ڹ = ߝ;

        ʑ.ں =
            އ.NextDouble() * (ޏ.ݶ - ޏ.ݵ) + ޏ.ݵ;
        return 0;
    }

    Vector3D ߟ(Vector3D ͺ, double ߠ, double ߡ)
    {
        var ߢ = MathHelper.ToRadians(ߠ);
        var ߣ = MathHelper.ToRadians(ߡ);

        var ߤ = ߢ + އ.NextDouble() * (ߣ - ߢ);

        var ߥ = 2 * Math.PI * އ.NextDouble();
        var ߦ = 2 * އ.NextDouble() - 1;
        var ߧ = Math.Sqrt(1 - ߦ * ߦ);
        var ǫ = ߧ * Math.Cos(ߥ);
        var Ǭ = ߧ * Math.Sin(ߥ);
        var ߨ = ߦ;
        var ߩ = new Vector3D(ǫ, Ǭ, ߨ);

        var ߪ = Vector3D.Normalize(ߩ) * ߤ;

        ͺ.Normalize();

        var ǩ = Vector3D.Cross(Vector3D.Up, ͺ);
        var ߴ = Math.Acos(Vector3D.Dot(Vector3D.Up, ͺ));

        var ߵ = Vector3D.Transform(ߪ, MatrixD.CreateFromAxisAngle(ǩ, ߴ));

        var ߺ = ͺ + ߵ;
        ߺ.Normalize();

        var Ϫ = ߺ;
        return Ϫ;
    }

    public void ŕ()
    {
        ސ = !ސ;
    }

    Vector3D ߜ(Vector3D ࠀ)
    {
        var ߥ = 2 * Math.PI * އ.NextDouble();
        var ࠁ = Math.Acos(2 * އ.NextDouble() - 1);

        var ǫ = Math.Sin(ࠁ) * Math.Cos(ߥ);
        var Ǭ = Math.Sin(ࠁ) * Math.Sin(ߥ);
        var ߨ = Math.Cos(ࠁ);

        var ࠂ = new Vector3D(ǫ, Ǭ, ߨ);

        return ࠀ + ࠂ;
    }


    public List<ɾ> ů()
    {
        var ʂ = new List<ɾ>();
        foreach (ە ʑ in ޅ) ʂ.Add(ʑ.ݢ());
        return ʂ;
    }


    public struct ɾ
    {
        public Vector3D ž;
        public Vector3D ࠃ;
        public Vector3D ƶ;
        public Vector3D ࠄ;
        public double ʦ;
        public long ʨ;
        public bool ʒ;
        public bool ࠅ;

        public ɾ(Vector3D ʬ, Vector3D ࠆ, Vector3D ǎ,
            Vector3D ࠇ, double ʯ, long ė, bool ࠈ, bool ࠉ)
        {
            ž = ʬ;
            ࠃ = ࠆ;
            ƶ = ǎ;
            ࠄ = ࠇ;
            ʦ = ʯ;
            ʨ = ė;
            ʒ = ࠈ;
            ࠅ = ࠉ;
        }
    }
}
internal class ӯ
{
    public IMyGravityGenerator Վ;


    private readonly sbyte א = 1;

    public ӯ(IMyGravityGenerator Ԗ, sbyte א, string Ĺ)
    {
        Վ = Ԗ;
        this.א = א;
        Ԗ.CustomName = $"LGravity Generator [{Ĺ}]";
    }

    public void տ(float վ)
    {
        Վ.GravityAcceleration = վ * א * 9.81f;
    }
}
internal class Ӿ
{
    public IMyArtificialMassBlock Ċ;
    public double ࠊ;
    public bool Ւ = true;
    public Vector3D Յ;
    public Vector3D Մ = new Vector3D(0, 0, 0);

    public Ӿ(IMyArtificialMassBlock Ճ)
    {
        Ċ = Ճ;
        Յ = new Vector3D(0, 0, 0);
    }

    public bool מ => Ċ.IsFunctional;

    public void Ց(Vector3D ԕ)
    {
        Մ = Յ;
        var ࠋ = Ċ.GetPosition();
        var ࠌ = ࠋ - ԕ;
        double Օ = מ ? 50000 : 0;
        Յ = ࠌ * Օ;
        ࠊ = ࠌ.Ʈ();
    }
}
enum ࠑ
{
    ࠍ,
    ࠎ,
    ࠏ,
    ࠐ
}
public class ލ
{
    List<IMyThrust> ࠒ;
    ࠑ ࠓ = ࠑ.ࠎ;

    public ލ(List<IMyThrust> ޗ)
    {
        ࠒ = ޗ;
    }

    public void ࠔ()
    {
        switch (ࠓ)
        {
            case ࠑ.ࠍ:
                ࠓ = ࠑ.ࠎ;
                break;
            case ࠑ.ࠎ:
                foreach (var ܔ in ࠒ)
                {
                    ܔ.Enabled = true;
                }
                ࠓ = ࠑ.ࠏ;
                break;
            case ࠑ.ࠏ:
                ࠓ = ࠑ.ࠐ;
                break;
            case ࠑ.ࠐ:
                foreach (var ܔ in ࠒ)
                {
                    ܔ.Enabled = false;
                }
                ࠓ = ࠑ.ࠍ;
                break;
                    
        }
    }

    public void ࠕ()
    {
        ࠓ = ࠑ.ࠎ;
        foreach (var ܔ in ࠒ)
        {
            ܔ.Enabled = false;
        }
    }

}
public class ڳ
{
    double ࠚ;
    bool ࠤ = true;
    double ࠨ;
    double ࡀ;

    double ڿ;

    public ڳ(double ࡁ, double ࡂ, double ࡃ, double ࡄ)
    {
        ࡅ = ࡁ;
        ࡆ = ࡂ;
        ࡇ = ࡃ;
        ڿ = ࡄ;
        ࠨ = 1 / ڿ;
    }

    public double ࡅ { get; set; }
    public double ࡆ { get; set; }
    public double ࡇ { get; set; }
    public double ࡈ { get; private set; }

    protected virtual double ࡋ(double ࡉ, double ࡊ, double ࡄ)
    {
        return ࡊ + ࡉ * ࡄ;
    }

    public double ࡌ()
    {
        return ࠚ;
    }


    public double ц(double ࡍ)
    {
        var ࡎ = (ࡍ - ࡀ) * ࠨ;

        if (ࠤ)
        {
            ࡎ = 0;
            ࠤ = false;
        }

        ࠚ = ࡋ(ࡍ, ࠚ, ڿ);

        ࡀ = ࡍ;

        ࡈ = ࡅ * ࡍ + ࡆ * ࠚ + ࡇ * ࡎ;
        return ࡈ;
    }

    public double ц(double ࡍ, double ࡄ)
    {
        if (ࡄ != ڿ)
        {
            ڿ = ࡄ;
            ࠨ = 1 / ڿ;
        }

        return ц(ࡍ);
    }

    public virtual void ࠕ()
    {
        ࠚ = 0;
        ࡀ = 0;
        ࠤ = true;
    }
}

public class Ä
{
    double ࡏ;
    double ࡐ;

    public double ࡑ;
    double ࡒ;
    public double Ǚ;
    double ࡓ;
    double ࡔ;
    double ࡕ;

    public Ä(double ࡖ, double ࡗ, double ࡘ, double ࢠ = 0, double ࢢ = 0, double ࢣ = 60)
    {
        ࡑ = ࡖ;
        ࡒ = ࡗ;
        Ǚ = ࡘ;
        ࡓ = ࢠ;
        ࡔ = ࢢ;
        ࡕ = ࢣ;
    }

    public double Ǫ(double ࢤ, int ࢥ)
    {
        double ࢦ = Math.Round(ࢤ, ࢥ);

        ࡏ = ࡏ + (ࢤ / ࡕ);
        ࡏ = (ࡓ > 0 && ࡏ > ࡓ ? ࡓ : ࡏ);
        ࡏ = (ࡔ < 0 && ࡏ < ࡔ ? ࡔ : ࡏ);

        double ࢧ = (ࢦ - ࡐ) * ࡕ;
        ࡐ = ࢦ;

        return (ࡑ * ࢤ) + (ࡒ * ࡏ) + (Ǚ * ࢧ);
    }
    public void ࠕ()
    {
        ࡏ = ࡐ = 0;
    }
}public enum Í
{
    Î,
    Ɵ,
    ƞ,
    Ơ
}
public enum Ð
{
    Ñ,
    Ƃ
}
public struct G
{
    public double ł;
    public double Ń;
    public int ě;
    public double Ĝ;
    public double ĝ;
        
    public int ę;
    public int Ě;
        
    public G(double ĺ, double Ļ, int ļ, double Ľ, double ľ)
    {
        Ĝ = Ľ;
        ł = ĺ;
        Ń = Ļ;
        ě = ļ;
        ĝ = ľ;
            
            
        ę = (int)(1.0 / ł* 60.0);
        Ě = (int)(Ń * 60.0);
    }
        
}
enum ࢫ
{
    ࢨ,
    ࢩ,
    ࢪ
}
internal class õ
{
    private readonly int ࢬ;
    private readonly int ऄ;
    private readonly int अ;
        
    int आ;
    int इ;

    public double Ĝ;
    public double ĝ;
    ࢫ ࠓ = ࢫ.ࢪ;

    public õ(int ई, int उ, int ļ, double Ľ, double ľ)
    {
        ࢬ = ई;
        ऄ = उ;
        अ = ļ;
        Ĝ = Ľ;
        ĝ = ľ;
    }

        

    public ࢫ Ʊ()
    {
        return ࠓ;
    }

    public bool ŵ(bool ऊ)
    {

            
            
        switch (ࠓ)
        {
            case ࢫ.ࢨ:
                आ--;
                if (ऊ && आ <= 0)
                {
                    आ = ࢬ;
                    इ++;
                    if (इ >= अ)
                    {
                        ࠓ = ࢫ.ࢩ;
                        आ = ऄ;
                        इ = 0;
                    }

                    return true;
                }
                break;
            case ࢫ.ࢩ:
                आ--;
                if (आ <= 0)
                {
                    ࠓ = ࢫ.ࢪ;
                }
                break;
            case ࢫ.ࢪ:
                if (ऊ) ࠓ = ࢫ.ࢨ;
                break;
                
                
        }

        return false;
    }
}
internal class ԏ
{
    public IMySpaceBall Ċ;
    public bool Ւ = true;
    public Vector3D Յ;
    public Vector3D ծ;
    public Vector3D հ;
    public Vector3D Մ;
    public double ռ = 1000;
    public double ջ = 1000;

    public ԏ(IMySpaceBall Ն)
    {
        Ċ = Ն;
        Յ = new Vector3D(0, 0, 0);
    }

    public bool מ => Ċ.IsFunctional;

    public void Ց(Vector3D ԕ)
    {
        Մ = Յ;
        var ࠋ = Ċ.GetPosition();
        var ࠌ = ࠋ - ԕ;

        double Օ = Ċ.VirtualMass;
        Յ = ࠌ * Օ;
        ծ = ࠌ * Math.Max(Օ - ռ, 0);
        հ = ࠌ * Math.Min(Օ + ջ, 20000);
    }

    public Vector3D Ց(Vector3D ԕ, float Օ)
    {
        var ࠋ = Ċ.GetPosition();
        var ࠌ = ࠋ - ԕ;
        return ࠌ * Օ;
    }

    public void ս(float Օ)
    {
        Ċ.VirtualMass = Օ;
    }
}
internal class ӱ
{
    public IMyGravityGeneratorSphere Վ;


    public sbyte א = 1;

    public ӱ(IMyGravityGeneratorSphere Ԗ, sbyte א, string Ĺ)
    {
        Վ = Ԗ;
        this.א = א;
        Ԗ.CustomName = $"SGravity Generator [{Ĺ}]";
    }

    public void տ(float վ)
    {
        Վ.GravityAcceleration = վ * א * 9.81f;
    }

    public void Խ(float ľ)
    {
        Վ.Radius = ľ;
    }

    public void Տ(float վ)
    {
        Վ.GravityAcceleration = վ;
    }
}
internal class ǆ
{
    public Í ƪ;
    public Vector3I Ϋ;

    public ǆ(Í ƪ, Vector3I Ϋ)
    {
        this.ƪ = ƪ;
        this.Ϋ = Ϋ;
    }
}
public static class ƍ
{
    private static double ऋ = 104;
    public static IMyShipController ǋ;
    public static MyGridProgram ā;

    public static Vector3D थ(ref Vector3D Ȩ, Vector3D ܩ, Vector3D ݎ, ref Vector3D ऌ, ref Vector3D ऍ, ref Vector3D վ, ref long ऎ, ए ऐ)
        {
            Vector3D ऑ = Ȩ - ऌ;
        
            double ऒ = 0;
            double ओ = 0;
            double औ = 0;
            double ख = ऑ.Length() / (Vector3D.Normalize(ऑ) * ऐ.क + (ܩ - ऍ)).Length();
            double घ = (Vector3D.Normalize(Ȩ + ܩ * ख + ݎ * ख * ख - ऌ) * ऐ.ग + ऍ).Length();
            if (ऐ.ङ)
                घ = Math.Min(घ, ऐ.क);
        
            if (ऐ.Γ != 0)
            {
                ऒ = (ऐ.क - घ) / ऐ.Γ;
        
                ओ =ऐ.ग * ऒ + ऐ.Γ * ऒ * ऒ;
                औ = (Ȩ + ܩ * ऒ + 0.5 * ݎ * ऒ * ऒ - ऌ).Length();
            }
        
            Vector3D च = ܩ;
            double छ = ऐ.Γ;
            double ज = घ;
            double झ = ݎ.Length();
            double ञ = ܩ.Length();
            double ट = 0;
            Vector3D ठ = Vector3D.Zero;
            if (औ > ओ)
            {
                छ = 0;
                ज = ऐ.क;
                ठ = -Vector3D.Normalize(ऑ) * ओ;
                ट = ऒ;
            }
        
            double ड = 0;
            if (झ > 1)
            {
                ड = (ऋ - Math.Min(Vector3D.ProjectOnVector(ref ܩ, ref ݎ).Length(), ऋ)) / झ;
                झ = 0;
                ܩ = Vector3D.Normalize(ܩ) * ऋ;
                ञ = ऋ;
            }
        
            double Ֆ = (0.25 * (छ * छ)) - (0.25 * (झ * झ));
            double ՙ = (ज * छ) - (ञ * झ);
            double ढ = (ज * ज) - (ञ * ञ) - ऑ.Dot(ݎ);
            double ण = -2 * ܩ.Dot(ऑ);
            double Ŋ = -ऑ.LengthSquared();
        
            double Ӌ = 0;
            if (Ӌ == double.MaxValue || double.IsNaN(Ӌ))
                Ӌ = 100;
        
            if (ड > Ӌ)
            {
                ड = Ӌ;
                Ӌ = 0;
            }
            else
                Ӌ -= ड;
        
            return Ȩ + (ܩ - ऍ) * Ӌ + (च - ऍ) * ड + 0.5 * ݎ * ड * ड - 0.5 * վ * (Ӌ + ड) * (Ӌ + ड) * Convert.ToDouble(ऐ.त) + ठ;
        }
        
    public static Vector3D Ǝ(Vector3D द, Vector3D ध, Vector3D न,
        Vector3D ऩ, float प, double ࡄ, ref Vector3D Ƴ,
        bool फ, bool ब)
    {
        var भ = (ध - Ƴ) / ࡄ / 2;

        var म = द - न;
        var य = ध - ऩ;
        var վ = ǋ.GetNaturalGravity() / 2;

        भ = ब ? भ : Vector3D.Zero;

        var ऱ = र(म, य, भ, -վ, प);
        var ल = द + (भ + य) * ऱ + -վ * ऱ;

        Ƴ = ध;
        return ल;
    }

    private static Vector3D श(Vector3D ʬ, Vector3D ࠀ, MatrixD ळ)
    {
        var ऴ = ʬ - ࠀ;
        var व = Vector3D.Transform(ऴ, ळ);
        return ࠀ + व;
    }

    private static double र(Vector3D म, Vector3D य, Vector3D ष,
        Vector3D վ, float प)
    {
        var Ֆ = ष.Ʈ() - प * प;
        var ՙ = 2 * (Vector3D.Dot(म, ष) + Vector3D.Dot(य, ष));
        var ढ = म.Ʈ();

        Ֆ += վ.Ʈ();
        ՙ += 2 * Vector3D.Dot(म, վ);

        var स = ՙ * ՙ - 4 * Ֆ * ढ;

        if (स < 0)
            return 0;

        var ह = (-ՙ + Math.Sqrt(स)) / (2 * Ֆ);
        var ऽ = (-ՙ - Math.Sqrt(स)) / (2 * Ֆ);

        if (ह < 0 && ऽ < 0)
            return 0;
        if (ह < 0)
            return ऽ;
        if (ऽ < 0)
            return ह;
        return Math.Min(ह, ऽ);
    }
}
public class ए
{
    public double क;
    public double ग;
    public bool ङ;
    public double Γ;
    public bool त;


    public ए(double क, double ग, bool ॐ, double Γ,
        bool त)
    {
        this.क = क;
        this.ग = ग;
        this.ङ = ॐ;
        this.Γ = Γ;
        this.त = त;
    }
}
