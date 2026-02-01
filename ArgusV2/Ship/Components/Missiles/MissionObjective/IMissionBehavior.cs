using System;
using IngameScript.Ship.Components.Missiles.LaunchMechanisms;

namespace IngameScript.Ship.Components.Missiles.GuidanceObjective
{
    public interface IMissionBehavior
    {
        GuidanceCommand Evaluate(); // thrust / rotation output
        bool IsComplete();         // tell missile when to move to next objective
        void OnComplete();         // optional cleanup or next objective

        void BindToMissile(Missile missile);         // Behavior implementations use this to bind properties to data needed from missile in flight
    }
    
    
    
}