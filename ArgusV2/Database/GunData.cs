using System.Collections.Generic;
using System.Linq;
using IngameScript.Helper;
using IngameScript.Helper;
using IngameScript.SConfig;
using IngameScript.Ship.Components;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript.Database
{
    public class ProjectileData
    {
        private static readonly ConfigTool Config = new ConfigTool("Projectile Data",
            "The main list of known projectiles. Gun Data should reference these by name.")
        {
            Get = GetConfig,
            Set = SetConfig
        };
        
        
        public static Dictionary<string, ProjectileData> LookupTable = new Dictionary<string, ProjectileData>();
        public static readonly ProjectileData DefaultProjectile = new ProjectileData(0, 0, 0, 0);
        
        static ProjectileData()
        {
            LookupTable.Add("Default", DefaultProjectile);
            
            LookupTable.Add("LargeRailgun", new ProjectileData(2000, 2000, 2000, 0));
            LookupTable.Add("Artillery", new ProjectileData(500, 500, 2000, 0));
            LookupTable.Add("SmallRailgun", new ProjectileData(1000, 1000, 1400, 0));
            LookupTable.Add("Gatling", new ProjectileData(400, 400, 800, 0));
            LookupTable.Add("AssaultCannon", new ProjectileData(500, 500, 1400, 0));
            LookupTable.Add("Rocket", new ProjectileData(100, 200, 800, 1000));
        }

        public static void Init()
        {
            
        }
        
        
        public static ProjectileData Get(string nameThing)
        {
            ProjectileData gun;
            return LookupTable.TryGetValue(nameThing, out gun) ? gun : DefaultProjectile;
        }
        public float ProjectileVelocity { get; private set; }
        public float MaxVelocity { get; private set;}
        public float MaxRange { get; private set; }
        public float Acceleration { get; private set; }

        public ProjectileData(float projectileVelocity, float maxVelocity, float maxRange, float acceleration)
        {
            ProjectileVelocity = projectileVelocity;
            MaxVelocity = maxVelocity;
            MaxRange = maxRange;
            Acceleration = acceleration;
        }
        
        private static Dictionary<string, object> GetConfig()
        {
            var root = new Dictionary<string, object>();

            foreach (var kv in LookupTable)
            {
                var name = kv.Key;
                var values = kv.Value;

                root[name] = new Dictionary<string, object>
                {
                    ["ProjectileVelocity"] = values.ProjectileVelocity,
                    ["MaxVelocity"] = values.MaxVelocity,
                    ["MaxRange"] = values.MaxRange,
                    ["Acceleration"] = values.Acceleration
                };
            }

            return root;
        }
        private static void SetConfig(Dictionary<string, object> config)
        {
            
        }
    }
    
    public class GunData
    {
        private static readonly ConfigTool Config = new ConfigTool("Gun Data",
            "The main list of known gun types and their definition names. Should reference a known projectile type.")
        {
            Get = GetConfig,
            Set = SetConfig
        };
        public static Dictionary<string, GunData> LookupTable = new Dictionary<string, GunData>();
        public static readonly GunData DefaultGun = new GunData("Default", 0, 0, 0f, 0f);
        
        static GunData()
        {
            LookupTable.Add("Default", DefaultGun);
            
            // Large grid
            LookupTable.Add("LargeRailgun", new GunData("LargeRailgun", GunReloadType.NeedsCharging, GunFireType.Delay, 1.5f, 0f));
            LookupTable.Add("LargeBlockLargeCalibreGun", new GunData("Artillery", 0, 0, 0, 12));
            LookupTable.Add("LargeMissileLauncher", new GunData("Rocket", 0, 0, 0, 0.5f));
            // Small grid
            LookupTable.Add("SmallRailgun", new GunData("SmallRailgun", GunReloadType.NeedsCharging, GunFireType.Delay, 0.5f, 0));
            LookupTable.Add("SmallBlockAutocannon", new GunData("Gatling", 0, 0, 0.0f, 0.4f));
            LookupTable.Add("SmallBlockMediumCalibreGun", new GunData("AssaultCannon", 0, 0, 0.0f, 6f));
            LookupTable.Add("MyObjectBuilder_SmallGatlingGun", new GunData("Gatling", 0, 0, 0.0f, 0.1f));
            LookupTable.Add("MyObjectBuilder_SmallMissileLauncher", new GunData("Rocket", 0, 0, 0.0f, 1f));
            
            LookupTable.Add("SmallRocketLauncherReload", Get("MyObjectBuilder_SmallMissileLauncher"));
            LookupTable.Add("SmallGatlingGunWarfare2", Get("MyObjectBuilder_SmallGatlingGun"));
            LookupTable.Add("SmallMissileLauncherWarfare2", Get("MyObjectBuilder_SmallMissileLauncher"));
            
        }
        
        public static void Init()
        {
            
        }
        
        public static GunData Get(string nameThing)
        {
            GunData gun;
            return LookupTable.TryGetValue(nameThing, out gun) ? gun : DefaultGun;
        }


        private string _projectileDataString;
        public ProjectileData ProjectileData { get; }
        public GunReloadType ReloadType { get; }
        public GunFireType FireType { get; }
        public int FireTimeFrames { get; }
        public float FireTime => FireTimeFrames / 60.0f;
        public int ReloadTimeFrames { get; }
        public float ReloadTime => ReloadTimeFrames / 60.0f;

        public GunData(string projectileDataName, GunReloadType reloadType, GunFireType fireType, float fireTime, float reloadTime)
        {
            _projectileDataString = projectileDataName;
            ProjectileData = ProjectileData.Get(projectileDataName);
            ReloadType = reloadType;
            FireType = fireType;
            FireTimeFrames = (int)(fireTime * 60);
            ReloadTimeFrames = (int)(reloadTime * 60);
        }
        
        
        private static Dictionary<string, object> GetConfig()
        {
            var root = new Dictionary<string, object>();

            foreach (var kv in LookupTable)
            {
                var name = kv.Key;
                var values = kv.Value;

                var gunData = new Dictionary<string, object>();
                gunData["Projectile"] = values._projectileDataString;
                gunData["ReloadType"] = new Dwon.Comment((int)values.ReloadType, "0 = normal, 1 = charged");
                gunData["FireType"] = new Dwon.Comment((int)values.FireType,"0 = normal, 1 = delay before firing") ;
                gunData["FireTimeFrames"] = values.FireTimeFrames;
                gunData["ReloadTimeFrames"] = values.ReloadTimeFrames;

                root[name] = gunData;
            }

            return root;
        }
        
        private static void SetConfig(Dictionary<string, object> config)
        {
            
        }
        
    }
}