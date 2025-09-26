namespace IngameScript
{
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
}