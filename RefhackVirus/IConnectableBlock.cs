using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    public interface IConnectableBlock
    {
        bool HasNewTarget();
        IMyCubeGrid GetConnectedGrid();

        InfectionType BaseInfectionType { get;}
        Vector3D Position { get;}
        string CustomName { get; }


        IMyCubeBlock GetBlock();
    }

    public class ConnectorBlock : IConnectableBlock
    {
        private readonly IMyShipConnector _connector;
        public ConnectorBlock(IMyShipConnector connector)
        {
            _connector = connector;
            BaseInfectionType = InfectionType.Connector;
        }
        private bool _previousState;
        public bool HasNewTarget()
        {
            bool newState = _connector.IsConnected;
            bool result = newState && !_previousState;
            _previousState = newState;
            return result;
        }
        public IMyCubeGrid GetConnectedGrid()
        {
            return _connector.OtherConnector?.CubeGrid;
        }
        public IMyCubeBlock GetBlock()
        {
            return _connector;
        }

        public InfectionType BaseInfectionType { get; }
        public Vector3D Position => _connector.GetPosition();
        public string CustomName => _connector.CustomName;
    }

    public class MechanicalBottom : IConnectableBlock
    {
        private readonly IMyMechanicalConnectionBlock _connector;

        public MechanicalBottom(IMyMechanicalConnectionBlock connector)
        {
            _connector = connector;
            BaseInfectionType = InfectionType.Mechanical;
        }
        private bool _previousState;
        public bool HasNewTarget()
        {
            bool newState = _connector.IsAttached;
            bool result = newState && !_previousState;
            _previousState = newState;
            return result;

        }
        public IMyCubeGrid GetConnectedGrid()
        {
            return _connector.TopGrid;
        }
        public IMyCubeBlock GetBlock()
        {
            return _connector;
        }
        
        public InfectionType BaseInfectionType { get; }
        public Vector3D Position => _connector.GetPosition();
        
        public string CustomName => _connector.CustomName;
    }

    public class MechanicalTop : IConnectableBlock
    {
        private readonly IMyAttachableTopBlock _connector;

        public MechanicalTop(IMyAttachableTopBlock connector)
        {
            _connector = connector;
            BaseInfectionType = InfectionType.Mechanical;
        }
        private bool _previousState;
        public bool HasNewTarget()
        {
            bool newState = _connector.IsAttached;
            bool result = newState && !_previousState;
            _previousState = newState;
            return result;
        }

        public IMyCubeGrid GetConnectedGrid()
        {
            return _connector.Base?.CubeGrid;
        }
        public IMyCubeBlock GetBlock()
        {
            return _connector;
        }
        public InfectionType BaseInfectionType { get; }
        public Vector3D Position => _connector.GetPosition();
        
        public string CustomName => _connector.DisplayName;
    }
}