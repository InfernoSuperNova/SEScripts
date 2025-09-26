using System.Collections.Generic;

namespace IngameScript
{
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
}