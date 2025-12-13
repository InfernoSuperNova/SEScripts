namespace IngameScript.Helper
{
    public static class MemorableName
    {
        public static string[] adjectives = new string[]
        {
            "Blazing",
            "Screaming",
            "Reckless",
            "Furious",
            "Vengeful",
            "Merciless",
            "Relentless",
            "Unstoppable",
            "Fearless",
            "Savage",
            "Brutal",
            "Vicious",
            "Lethal",
            "Deadly",
            "Explosive",
            "Thunderous",
            "Infernal",
            "Celestial",
            "Phantom",
            "Shadow",
            "Silent",
            "Swift",
            "Mighty",
            "Titanic",
            "Colossal",
            "Adorable"
        };

        public static string[] nouns = new string[]
        {
            "Reaper",
            "Viper",
            "Falcon",
            "Hammer",
            "Blade",
            "Storm",
            "Thunder",
            "Inferno",
            "Cyclone",
            "Avalanche",
            "Meteor",
            "Phoenix",
            "Dragon",
            "Kraken",
            "Leviathan",
            "Specter",
            "Wraith",
            "Banshee",
            "Valkyrie",
            "Sentinel",
            "Titan",
            "Goliath",
            "Behemoth",
            "Juggernaut",
            "Colossus",
            "Kitten"
        };

        public static string[] suffixes = new string[]
        {
            "Death",
            "Destruction",
            "Ruin",
            "Chaos",
            "Vengeance",
            "Wrath",
            "Doom",
            "Oblivion",
            "Annihilation",
            "The Apocalypse",
            "Perdition",
            "Damnation",
            "Judgment",
            "Retribution",
            "Calamity",
            "Cataclysm",
            "Devastation",
            "Extinction",
            "Armageddon",
            "Twilight",
            "Nightfall",
            "Reckoning",
            "Fury",
            "Malice",
            "Torment",
            "Cuddles"
        };

        public static string Get()
        {
            var adjective = adjectives[Program.RNG.Next(adjectives.Length)];
            var noun = nouns[Program.RNG.Next(nouns.Length)];
            var suffix = suffixes[Program.RNG.Next(suffixes.Length)];
            return $"{adjective} {noun} of {suffix}";
        }
    }
}