using IngameScript.Helper;
using IngameScript.TruncationWrappers;
using VRageMath;

namespace IngameScript.Ship.Components
{
    public struct FiringSolution
    {
        public readonly AT_Vector3D DesiredForward;
        public readonly AT_Vector3D TargetPosition;
        public readonly AT_Vector3D ShooterPosition;
        public readonly AT_Vector3D CurrentForward;
        public readonly double Dot;
        public readonly double Range;
        public MatrixD WorldMatrix;

        public FiringSolution(AT_Vector3D desiredForward, AT_Vector3D targetPosition, AT_Vector3D shooterPosition, AT_Vector3D currentForward, double dot, double range, MatrixD worldMatrix)
        {
            DesiredForward = desiredForward;
            TargetPosition = targetPosition;
            ShooterPosition = shooterPosition;
            CurrentForward = currentForward;
            Dot = dot;
            Range = range;
            WorldMatrix = worldMatrix;
        }
    }
    // Responsible for gathering targeting information from both ControllableShip and GunManager and deciding on a course of action
    public class FireController
    {
        private ControllableShip _ship;
        private GunManager _guns;

        public FireController(ControllableShip ship, GunManager guns)
        {
            Program.LogLine($"Setting up FCS", LogLevel.Info);
            _ship = ship;
            _guns = guns;
        }
        /// <summary>
        /// What a cool function name, that's how you know it's cool
        /// </summary>
        public FiringSolution ArbitrateFiringSolution()
        {

            _guns.SelectFiringGroup();
            
            int count = _guns.FireGroupCount;
            PositionValidity validity = _guns.FireGroupValidity;
            var pos = _guns.FireRefPos;

            var enemyPos = _ship.GetTargetPosition();
            
            
            var displacement = enemyPos - pos;

            var dist = displacement.Length();

            var dir = displacement / dist;

            var dot = dir.Dot(_ship.WorldForward);

            var solvedPos = _guns.GetBallisticSolution();
            var solvedForward = (solvedPos - pos).Normalized();
            Program.Log(solvedForward);

            if (dot > Config.Behavior.MinFireDot && enemyPos != AT_Vector3D.Zero) _guns.TryFire();
            else _guns.TryCancel();

            return new FiringSolution(solvedForward, enemyPos, pos, _ship.WorldForward, dot, dist, _ship.WorldMatrix);
        }
    }
}