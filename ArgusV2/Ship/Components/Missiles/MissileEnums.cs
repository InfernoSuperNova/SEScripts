using System;

namespace IngameScript
{
    
    
    // TODO
    // Missiles:
    // Payload type: Warhead
    // Payload type: Nuke
    // Payload type: Kinetic
    // CIWS missiles (Anti missile missile)
    // Delivery type: Hydrogen/ion/atmos
    // Launch type: Staggered
    // Launch type: Simultaneous
    // Launch type: OAAT
    // Launch mechanism: Pulsed thruster detach
    // Launch mechanism: Mechanical (rotor/hinge/connector/merge block)
    // Launch mechanism: Gun
    // Behavior: Interdict
    // Behavior: Direct attack
    // Behavior: Surround and close
    // Expandable missile config system
    // Detect missiles via direct grid position lookup, starting from projectors and storing custom data in them to store offset etc
    
    
    // Note: A missile qualifies for CIWS if it's:
    // Warhead payload type
    // Delivery type matches environment
    // CIWS missiles are switched on
    // The platform 
    public enum PayloadType
    {
        Kinetic,            // Effectively: No payload
        Warhead,            // Versatile
        Nuke                // Ammo bomb
    }

    public enum LaunchType
    {
        OneAtATime,         // One missile launch per button press
        Simultaneous,       // All launchers fire on button press
        Staggered           // All launchers fire on button press, but with a delay between them
    }

    public enum LaunchControl
    {
        Automatic,          // Launch to a quota whenever any target is tracked. See refuel state for further launch conditions
        Manual,             // Launch only when commanded to by the controlling player. See refuel state for further launch conditions
        ManualCiwsOverride  // Launch only when commanded to by the controlling player, or when CIWS system requests a missile and none are nearby. See refuel state for further launch conditions
    }

    // Only relevant for hydrogen missiles
    // Maybe we have one RefuelPriority instance for each launch type so it's less arbitrary how each system works together
    public enum RefuelPriority
    {
        Low,                // Allows: Automatic launch @ 10% fuel, manual launch @ any%, CIWS launch @ any%
        Medium,             // Allows: Automatic launch @ 100% fuel, manual launch @ 10%, CIWS launch @ any%
        High                // Allows: Automatic launch @ 100% fuel, manual launch @ 100% out of combat, 10% with a tracked target, CIWS launch @ 1%
    }

    [Flags]
    public enum DeliveryType
    {
        None = 0,
        Hydrogen = 1 << 0,
        Ion = 1 << 1,
        Atmospheric = 1 << 2
    }

    [Flags]
    public enum LaunchMechanism
    {
        None = 0,
        Mechanical = 1 << 0,
        Connector = 1 << 1,
        MergeBlock = 1 << 2,
        PulsedThruster = 1 << 3,
        Weapon = 1 << 4
    }

    public enum Behavior
    {
        Interdict,          // Hang out at a random point far away from the ship and chase down any detected target that gets too close
        DirectAttack,       // Go straight for the primary detected target on launch. As a fallback, either beamride or idle near the launching ship
        SurroundAndClose,   // Surround an enemy target and wait for the signal to attack. As a fallback, surround the controlling ship instead
        Defend              // Effectively the same as Interdict, but at a much much tighter area, just a couple hundred meters
    }
}