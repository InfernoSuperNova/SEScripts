namespace IngameScript.Ship.Components.Missiles.LaunchMechanisms
{
    public class MissileLauncher
    {
        private LaunchMechanism _launchMechanism;
        
        
        
        
        public MissileLauncher(LaunchMechanism launchMechanism)
        {
            _launchMechanism = launchMechanism;
        }
        internal LaunchMechanism LaunchMechanism => _launchMechanism;

        
    }
}