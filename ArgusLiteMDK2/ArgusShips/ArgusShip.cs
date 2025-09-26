using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    public class ArgusShip
    {
    #region motion
        public Vector3D Position;
        public Vector3D Velocity;
        public Vector3D Acceleration;
        private Vector3D _previousVelocity;
    #endregion
        
        public bool DataIsCurrent = false;
        public MatrixD ToWorldCoordinatesMatrix;
        public long EntityId;
        // TODO: Update this from within program.cs
        public IMyShipController Controller;

        public ArgusShip(long entityId, MatrixD toWorldCoordinatesMatrix, Vector3D position, IMyShipController controller)
        {
            EntityId = entityId;
            ToWorldCoordinatesMatrix = toWorldCoordinatesMatrix;
            Position = position;
            Controller = controller;
            
            _masterShipList.Add(this);
        }
        
        
        // TODO: Call me
        public virtual void Update(Vector3D position, Vector3D velocity, MatrixD toWorldCoordinatesMatrix, IMyShipController controller)
        {
            Position = position;
            Velocity = velocity;
            Acceleration = (velocity - _previousVelocity) * Program.TimeStep;
            Controller = controller;
            
            _previousVelocity = velocity;
            ToWorldCoordinatesMatrix = toWorldCoordinatesMatrix;
            DataIsCurrent = true;
        }




        private static List<ArgusShip> _masterShipList = new List<ArgusShip>();

        public static void SetAllStatusNonCurrent()
        {
            foreach (var ship in _masterShipList)
            {
                ship.DataIsCurrent = false;
            }
        }
    }
}