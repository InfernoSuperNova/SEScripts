using System;
using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    #region PID Class

    /// <summary>
    ///     Discrete time PID controller class.
    ///     Last edited: 2022/08/11 - Whiplash141
    /// </summary>
    public class PID
    {
        private double _errorSum;
        private bool _firstRun = true;
        private double _inverseTimeStep;
        private double _lastError;

        private double _timeStep;

        public PID(double kp, double ki, double kd, double timeStep)
        {
            Kp = kp;
            Ki = ki;
            Kd = kd;
            _timeStep = timeStep;
            _inverseTimeStep = 1 / _timeStep;
        }

        public double Kp { get; set; }
        public double Ki { get; set; }
        public double Kd { get; set; }
        public double Value { get; private set; }

        protected virtual double GetIntegral(double currentError, double errorSum, double timeStep)
        {
            return errorSum + currentError * timeStep;
        }

        public double GetErrorSum()
        {
            return _errorSum;
        }


        public double Control(double error)
        {
            //Compute derivative term
            var errorDerivative = (error - _lastError) * _inverseTimeStep;

            if (_firstRun)
            {
                errorDerivative = 0;
                _firstRun = false;
            }

            //Get error sum
            _errorSum = GetIntegral(error, _errorSum, _timeStep);

            //Store this error as last error
            _lastError = error;

            //Construct output
            Value = Kp * error + Ki * _errorSum + Kd * errorDerivative;
            return Value;
        }

        public double Control(double error, double timeStep)
        {
            if (timeStep != _timeStep)
            {
                _timeStep = timeStep;
                _inverseTimeStep = 1 / _timeStep;
            }

            return Control(error);
        }

        public virtual void Reset()
        {
            _errorSum = 0;
            _lastError = 0;
            _firstRun = true;
        }
    }

    public class DecayingIntegralPID : PID
    {
        public DecayingIntegralPID(double kp, double ki, double kd, double timeStep, double decayRatio) : base(kp, ki,
            kd, timeStep)
        {
            IntegralDecayRatio = decayRatio;
        }

        public double IntegralDecayRatio { get; set; }

        protected override double GetIntegral(double currentError, double errorSum, double timeStep)
        {
            return errorSum * (1.0 - IntegralDecayRatio) + currentError * timeStep;
        }
    }

    public class ClampedIntegralPID : PID
    {
        public ClampedIntegralPID(double kp, double ki, double kd, double timeStep, double lowerBound,
            double upperBound) : base(kp, ki, kd, timeStep)
        {
            IntegralUpperBound = upperBound;
            IntegralLowerBound = lowerBound;
        }

        public double IntegralUpperBound { get; set; }
        public double IntegralLowerBound { get; set; }

        protected override double GetIntegral(double currentError, double errorSum, double timeStep)
        {
            errorSum = errorSum + currentError * timeStep;
            return Math.Min(IntegralUpperBound, Math.Max(errorSum, IntegralLowerBound));
        }
    }

    public class BufferedIntegralPID : PID
    {
        private readonly Queue<double> _integralBuffer = new Queue<double>();

        public BufferedIntegralPID(double kp, double ki, double kd, double timeStep, int bufferSize) : base(kp, ki, kd,
            timeStep)
        {
            IntegralBufferSize = bufferSize;
        }

        public int IntegralBufferSize { get; set; }

        protected override double GetIntegral(double currentError, double errorSum, double timeStep)
        {
            if (_integralBuffer.Count == IntegralBufferSize)
                _integralBuffer.Dequeue();
            _integralBuffer.Enqueue(currentError * timeStep);
            return _integralBuffer.Sum();
        }

        public override void Reset()
        {
            base.Reset();
            _integralBuffer.Clear();
        }
    }

    #endregion
}