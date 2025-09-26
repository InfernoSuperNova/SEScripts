using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmptyKeys.UserInterface.Controls;
using Sandbox.Game.World;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;
using System.Reflection.Emit;

namespace IngameScript
{
    public static class AutoDodge
    {
        
        private static float _threshold = 25;
        private static double _maxSpeed = 102.65;

        private static Vector3D _ourPreviousVelocity;
        
        private static MatrixD _theirPreviousOrientation;
        private static Vector3D _theirPreviousVelocity;
        private static Vector3D _theirPreviousAcceleration;
        
        private static Vector3D _lastJerkEventPosition;
        private static Vector3D _lastInterceptEventPosition;
        private static float _jerkEventPositionValidFor = 0f;
        private static float _interceptEventPositionValidFor = 0f;

        private static double _jerkLastFrame = 0;
        
        public static Vector3D Check(MyDetectedEntityInfo theirGrid, Vector3D theirPos, Vector3D ourPos, Vector3D ourVelocity, IMyCubeGrid ourGrid, Vector3D ourAngularVelocity)
        {
            
            var theirOrientation = theirGrid.Orientation;
            var ourOrientation = ourGrid.WorldMatrix;
            var theirVelocity = theirGrid.Velocity;

            var theirAcceleration = theirVelocity - _theirPreviousVelocity;
            
            
            
            Vector3D ourAcceleration = ourVelocity - _ourPreviousVelocity;
            Vector3D ourRelativeAcceleration = Vector3D.Transform(ourAcceleration, MatrixD.Transpose(ourOrientation.GetOrientation()));
            ourAngularVelocity /= 60;
            var ourAngularVelocityQuaternion = Quaternion.CreateFromYawPitchRoll((float)ourAngularVelocity.Y, (float)ourAngularVelocity.X, (float)ourAngularVelocity.Z);

            var themToUs = (ourPos - theirPos).Normalized();
            
            
            // Step 1: We want to find the direction of the ship (forward, backward, left, right, up, down) that corresponds closest with the vector from them to us

            var theirForward = GetClosestDirectionVector(theirOrientation, themToUs);

            var dotOfThemToUs = theirForward.Dot((themToUs));
            




            Vector3D theirClosestLeadPoint = Vector3D.Zero; // The point where their shooting position has the least error to us
            Vector3D ourClostestLeadPoint = Vector3D.Zero; // The point where our position is predicted to be
            double closestDistanceSqr = Double.MaxValue;
            float closestT = 0;
            
            float velocity = 2000;
            Vector3D projectileRayStart = theirPos;
            Vector3D ourAdjustedVelocity = ourVelocity;
            MatrixD adjustedOrientation = ourOrientation;

            Vector3D projectileHeading = (theirVelocity + theirForward * velocity).Normalized() * velocity;
            
            for (float t = 1f/60f; t < 1; t += 1f / 60f)
            {
                Vector3D projectileRayEnd = theirPos + projectileHeading * t;
                Vector3D ourPredictedPos = ourPos + ourAdjustedVelocity * t;
                ourAdjustedVelocity += Vector3D.Transform(ourRelativeAcceleration, adjustedOrientation.GetOrientation());
                if (ourAdjustedVelocity.LengthSquared() > _maxSpeed * _maxSpeed)
                {
                    // Scale the velocity down to max speed without changing direction
                    Vector3D dir = ourAdjustedVelocity.Normalized();
                    ourAdjustedVelocity = dir * _maxSpeed;
                }
                
                adjustedOrientation = MatrixD.Transform(adjustedOrientation, ourAngularVelocityQuaternion);

                Vector3D closestPointOnLine =
                    ClosestPointOnSegment(projectileRayStart, projectileRayEnd, ourPredictedPos);

                // Color color = Color.Lerp(Color.Red, Color.Blue, t);
                // Program.I.DebugApi.DrawLine(projectileRayStart, projectileRayEnd, color, 1f, 0.016f);
                // Program.I.DebugApi.DrawPoint(ourPredictedPos, color, 5f, 0.016f, true);
                // Program.I.DebugApi.DrawLine(ourPredictedPos, closestPointOnLine, color, 0.25f, 0.016f);

                double distSqr = (closestPointOnLine - ourPredictedPos).LengthSquared();

                if (distSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distSqr;
                    theirClosestLeadPoint = closestPointOnLine;
                    ourClostestLeadPoint = ourPredictedPos;
                    closestT = t;
                }
                
                projectileRayStart = projectileRayEnd;
            }

            // So now we have:
            
            // Their closest intercept point to us
            // Where we will be at that point
            // Our goal is to separate that by as much as possible
            // Maybe we just move our current position directly away from the intercept position?

            
            double jerk = CheckForTheirJerk(themToUs, theirAcceleration);
            

            Vector3D realToGhost = theirClosestLeadPoint - ourClostestLeadPoint;
            //if (realToGhost.LengthSquared() < _threshold * _threshold && didFire)
            if (jerk != 0 && _jerkLastFrame != 0 && Math.Sign(_jerkLastFrame) != Math.Sign(jerk))
            {
                _lastJerkEventPosition = theirClosestLeadPoint;
                _jerkEventPositionValidFor = closestT;
            }

            if (realToGhost.LengthSquared() < _threshold * _threshold)
            {
                _lastInterceptEventPosition = theirClosestLeadPoint;
                _interceptEventPositionValidFor = closestT;
            }
            
            // Program.I.DebugApi.DrawLine(theirClosestLeadPoint, ourClostestLeadPoint, Color.Green, 0.25f, 0.016f);
            // 
            //
            
            _theirPreviousOrientation = theirOrientation;
            _ourPreviousVelocity = ourVelocity;
            _theirPreviousVelocity = theirVelocity;
            _theirPreviousAcceleration = theirAcceleration;
            _interceptEventPositionValidFor -= 1f / 60f;
            _jerkEventPositionValidFor -= 1f / 60f;
            _jerkLastFrame = jerk;
            
            Program.I.DebugApi.DrawPoint(_lastJerkEventPosition, Color.Yellow, 5f, 1f, true);
            Program.I.DebugApi.DrawPoint(_lastInterceptEventPosition, Color.Green, 5f, 1f, true);
            
            
            var dodgeVal = ourPos - _lastJerkEventPosition;
            dodgeVal -= (themToUs * dodgeVal.Dot(themToUs));
            if (_jerkEventPositionValidFor > 0 && dodgeVal.LengthSquared() < 100 * 100)
                return dodgeVal.Normalized();


            dodgeVal = ourPos - _lastInterceptEventPosition;
            dodgeVal -= (themToUs * dodgeVal.Dot(themToUs));
            if (_interceptEventPositionValidFor >= 0 && dodgeVal.LengthSquared() < 100 * 100)
                return dodgeVal.Normalized();
            
            
            
            return Vector3D.Zero;
        }


        private static List<double> previousJerks = new List<double>();
        private const int windowSize = 180;
        private const double outlierThresholdMultiplier = 4.0; // 4 std devs

        private static double CheckForTheirJerk(Vector3D themToUs, Vector3D theirAcceleration)
        {
            Vector3D theirJerk = theirAcceleration - _theirPreviousAcceleration;
            double projectedJerk = theirJerk.Dot(themToUs.Normalized());

            previousJerks.Add(projectedJerk);
            if (previousJerks.Count > windowSize)
                previousJerks.RemoveAt(0);

            if (previousJerks.Count < 10) 
            {
                // Not enough data to decide yet
                return 0;
            }

            double mean = previousJerks.Average();
            double variance = previousJerks.Average(v => Math.Pow(v - mean, 2));
            double stdDev = Math.Sqrt(variance);

            // Outlier detection: current projected jerk > mean + k*stdDev
            bool isOutlier = Math.Abs(projectedJerk) > mean + outlierThresholdMultiplier * stdDev;
            
            // if (isOutlier)
            //     ALDebug.AddText($"Jerk: {projectedJerk:F3}, Mean: {mean:F3}, StdDev: {stdDev:F3}, Outlier: {isOutlier}");

            if (!isOutlier) return 0;
            
            return projectedJerk;
        }
        
        
        private static Vector3D GetClosestDirectionVector(MatrixD orientation, Vector3D fromThemToUs)
        {
            Vector3D bestVector = orientation.Forward;
            double bestDot = Vector3D.Dot(fromThemToUs, bestVector);

            Vector3D candidate = -orientation.Forward;
            double dot = Vector3D.Dot(fromThemToUs, candidate);
            if (dot > bestDot) { bestDot = dot; bestVector = candidate; }

            candidate = orientation.Right;
            dot = Vector3D.Dot(fromThemToUs, candidate);
            if (dot > bestDot) { bestDot = dot; bestVector = candidate; }

            candidate = -orientation.Right;
            dot = Vector3D.Dot(fromThemToUs, candidate);
            if (dot > bestDot) { bestDot = dot; bestVector = candidate; }

            candidate = orientation.Up;
            dot = Vector3D.Dot(fromThemToUs, candidate);
            if (dot > bestDot) { bestDot = dot; bestVector = candidate; }

            candidate = -orientation.Up;
            dot = Vector3D.Dot(fromThemToUs, candidate);
            if (dot > bestDot) { bestDot = dot; bestVector = candidate; }

            return bestVector;
        }
        
        private static Vector3D ClosestPointOnSegment(Vector3D p0, Vector3D p1, Vector3D point)
        {
            Vector3D line = p1 - p0;
            double lineLengthSq = line.LengthSquared();

            if (lineLengthSq == 0)
                return p0; // Degenerate line

            double t = Vector3D.Dot(point - p0, line) / lineLengthSq;
            t = Math.Max(0.0, Math.Min(1.0, t)); // Clamp to segment

            return p0 + line * t;
        }
    }
    
}