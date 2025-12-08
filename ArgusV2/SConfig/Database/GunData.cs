using System;
using System.Collections.Generic;
using IngameScript.Helper;
using IngameScript.SConfig.Helper;
using IngameScript.Ship.Components;

namespace IngameScript.SConfig.Database
{
    public class ProjectileData
    {
        private static readonly ConfigTool Config = new ConfigTool("Projectile Data",
            "The main list of known projectiles. Gun Data should reference these by name.")
        {
            Sync = SyncConfig,
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
            Program.LogLine("Projectile data loaded", LogLevel.Debug);
        }


        public static ProjectileData Get(string nameThing)
        {
            ProjectileData gun;
            return LookupTable.TryGetValue(nameThing, out gun) ? gun : DefaultProjectile;
        }

        public float ProjectileVelocity { get; private set; }
        public float MaxVelocity { get; private set; }
        public float MaxRange { get; private set; }
        public float Acceleration { get; private set; }

        public ProjectileData(float projectileVelocity, float maxVelocity, float maxRange, float acceleration)
        {
            ProjectileVelocity = projectileVelocity;
            MaxVelocity = maxVelocity;
            MaxRange = maxRange;
            Acceleration = acceleration;
        }

        private static void SyncConfig(Dictionary<string, object> root)
        {
            var copy = new Dictionary<string, ProjectileData>(LookupTable);

            foreach (var kv in copy)
            {
                var name = kv.Key;
                var existing = kv.Value;

                // Get or create dictionary for this projectile
                var category = ConfigCategory.From(root, name);

                // Sync each property
                var projectileVelocity = existing.ProjectileVelocity;
                var maxVelocity = existing.MaxVelocity;
                var maxRange = existing.MaxRange;
                var acceleration = existing.Acceleration;

                category.Sync("ProjectileVelocity", ref projectileVelocity);
                category.Sync("MaxVelocity", ref maxVelocity);
                category.Sync("MaxRange", ref maxRange);
                category.Sync("Acceleration", ref acceleration);

                // Update the LookupTable with the synced values
                LookupTable[name] = new ProjectileData(projectileVelocity, maxVelocity, maxRange, acceleration);
            }

            // Handle any new entries in the dictionary that aren't in LookupTable yet
            foreach (var kv in root)
            {
                string empty = "";
                if (!LookupTable.ContainsKey(kv.Key))
                {
                    var data = kv.Value as Dictionary<string, object>;
                    if (data == null) continue;

                    var projectileVelocity = Dwon.GetValue(data, "ProjectileVelocity", ref empty, 0.0f);
                    var maxVelocity = Dwon.GetValue(data, "MaxVelocity", ref empty, 0.0f);
                    var maxRange = Dwon.GetValue(data, "MaxRange", ref empty, 0.0f);
                    var acceleration = Dwon.GetValue(data, "Acceleration", ref empty, 0.0f);

                    LookupTable[kv.Key] = new ProjectileData(projectileVelocity, maxVelocity, maxRange, acceleration);
                }
            }
        }
    }
    
    public class GunData
    {
        private static readonly ConfigTool Config = new ConfigTool("Gun Data",
            "The main list of known gun types and their definition names. Should reference a known projectile type.")
        {
            Sync = SyncConfig
        };
        public static Dictionary<string, GunData> LookupTable = new Dictionary<string, GunData>();
        public static readonly GunData DefaultGun = new GunData("Default", 0, 0, 0f, 0f);
        
        static GunData()
        {
            LookupTable.Add("Default", DefaultGun);
            
            // Large grid
            LookupTable.Add("LargeRailgun", new GunData("LargeRailgun", GunReloadType.NeedsCharging, GunFireType.Delay, 2.0f, 4.0f));
            LookupTable.Add("LargeBlockLargeCalibreGun", new GunData("Artillery", 0, 0, 0, 12));
            LookupTable.Add("LargeMissileLauncher", new GunData("Rocket", 0, 0, 0, 0.5f));
            // Small grid
            LookupTable.Add("SmallRailgun", new GunData("SmallRailgun", GunReloadType.NeedsCharging, GunFireType.Delay, 0.5f, 4.0f));
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
            Program.LogLine("Gun data loaded", LogLevel.Debug);
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
        
        
        private static void SyncConfig(Dictionary<string, object> root)
        {


            var copy = new Dictionary<string, GunData>(LookupTable);
            foreach (var kv in copy)
            {
                var name = kv.Key;
                var existing = kv.Value;

                // Get or create dictionary for this gun
                var category = ConfigCategory.From(root, name);

                // Sync each property
                var projectile = existing._projectileDataString;
                var reloadType = existing.ReloadType;
                var fireType = existing.FireType;
                var fireTime = existing.FireTime;
                var reloadTime = existing.ReloadTime;

                category.Sync("Projectile", ref projectile);
                category.SyncEnum("ReloadType", ref reloadType, "0 = normal, 1 = charged");
                category.SyncEnum("FireType", ref fireType, "0 = normal, 1 = delay before firing");
                category.Sync("FireTime", ref fireTime);
                category.Sync("ReloadTime", ref reloadTime);

                // Update the LookupTable with the synced values
                LookupTable[name] = new GunData(projectile, reloadType, fireType, fireTime, reloadTime);
            }

            // Handle any new entries in the dictionary not in LookupTable yet
            foreach (var kv in root)
            {
                if (!LookupTable.ContainsKey(kv.Key))
                {
                    var data = kv.Value as Dictionary<string, object>;
                    if (data == null) continue;

                    var emptyStr = "";
                    var projectile = Dwon.GetValue(data, "Projectile", ref emptyStr, "");
                    var reloadType = Dwon.GetValue(data, "ReloadType", ref emptyStr, 0);
                    var fireType = Dwon.GetValue(data, "FireType", ref emptyStr, 0);
                    var fireTime = Dwon.GetValue(data, "FireTime", ref emptyStr, 0.0f);
                    var reloadTime = Dwon.GetValue(data, "ReloadTime", ref emptyStr, 0.0f);

                    LookupTable[kv.Key] = new GunData(projectile, (GunReloadType)reloadType, (GunFireType)fireType, fireTime, reloadTime);
                }
            }
        }

        
    }
}