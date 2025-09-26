using System;
using System.Text;
using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    public static class FlightDataRecorder
    {
        public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        public static uint Frame;
        public static uint LastFrame;

        private static readonly StringBuilder FlightDataSB = new StringBuilder();


        public static IMyShipController FlightDataBlock;

        private static bool loggedTimeThisFrame;

        private static readonly ushort instructionReserved = 1024;

        public static void WriteData()
        {
        }

        private static double RoundToDP(double value, int decimalPlaces)
        {
            var multiplier = Math.Pow(10, decimalPlaces);
            return Math.Round(value * multiplier) / multiplier;
        }

        public static void TickFrame()
        {
            Frame++;
            loggedTimeThisFrame = false;
        }

        public static uint ToUnixTimestamp(this DateTime time)
        {
            return (uint)time.Subtract(Epoch).TotalSeconds;
        }


        public static string ushortToString(ushort value)
        {
            var character = (char)value;
            return character.ToString();
        }

        // Create a function to get the seconds since today started
        public static uint GetSecondsSinceToday()
        {
            return (uint)DateTime.Now.TimeOfDay.TotalSeconds;
        }

        // Create a function to get the milliseconds since the hour started
        public static uint GetMillisecondsSinceHour()
        {
            var now = DateTime.Now;
            var startOfHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, 0);
            var timeElapsed = now - startOfHour;
            return (uint)timeElapsed.TotalMilliseconds;
        }


        public static void LogRealTime()
        {
            FlightDataSB.Append("T+").Append(ToUnixTimestamp(DateTime.Now)).AppendLine();
        }

        private static void TryLogTime()
        {
            if (!loggedTimeThisFrame)
            {
                FlightDataSB.Append((char)(ushort)(Frame - LastFrame + instructionReserved));
                LastFrame = Frame;
                loggedTimeThisFrame = true;
            }
        }


        public static void WeaponRechargedEvent(char name)
        {
            TryLogTime();
            FlightDataSB.Append((char)1).Append(name);
        }

        public static void WeaponBeginChargeEvent(char name, double sigma, double range)
        {
            TryLogTime();
            FlightDataSB.Append((char)2).Append(name).Append((char)(Half)sigma).Append((char)(Half)range);
        }

        public static void WeaponCancelChargeEvent(char name, double sigma, double range)
        {
            TryLogTime();
            FlightDataSB.Append((char)3).Append(name).Append((char)(Half)sigma).Append((char)(Half)range);
        }

        public static void WeaponFireEvent(char name, double sigma, double range)
        {
            TryLogTime();
            FlightDataSB.Append((char)4).Append(name).Append((char)(Half)sigma).Append((char)(Half)range);
        }

        public static void WeaponRepairedEvent(char nameShorthand)
        {
            TryLogTime();
            FlightDataSB.Append((char)5).Append(nameShorthand);
        }

        public static void WeaponDamagedEvent(char nameShorthand)
        {
            TryLogTime();
            FlightDataSB.Append((char)6).Append(nameShorthand);
        }

        public static void WeaponCreatedEvent(char nameShorthand)
        {
            TryLogTime();
            FlightDataSB.Append((char)7).Append(nameShorthand);
        }


        internal static void GravityDriveEnabledEvent()
        {
            TryLogTime();
            FlightDataSB.Append((char)8);
        }

        internal static void GravityDriveDisabledEvent()
        {
            TryLogTime();
            FlightDataSB.Append((char)9);
        }

        internal static void GravityDriveRepulseEnabledEvent()
        {
            TryLogTime();
            FlightDataSB.Append((char)10);
        }

        internal static void GravityDriveRepulseDisabledEvent()
        {
            TryLogTime();
            FlightDataSB.Append((char)11);
        }

        public static void GravityDriveMassBalanceEvent(float previousMassError, float massError)
        {
            TryLogTime();
            FlightDataSB.Append((char)12).Append((char)(Half)previousMassError).Append((char)(Half)massError);
        }

        public static void GravityDriveBallBalanceEvent(float previousMassError, float massError)
        {
            TryLogTime();
            FlightDataSB.Append((char)13).Append((char)(Half)previousMassError).Append((char)(Half)massError);
        }

        internal static void GravityDriveDampenersChangedEvent(bool dampeners)
        {
            TryLogTime();
            FlightDataSB.Append((char)14).Append(dampeners ? '1' : '0');
        }

        internal static void GravityDrivePrecisionChangedEvent(bool precision)
        {
            TryLogTime();
            FlightDataSB.Append((char)15).Append(precision ? '1' : '0');
        }


        internal static void PlayerEnteredCockpitEvent()
        {
            TryLogTime();
            FlightDataSB.Append((char)16);
        }

        internal static void PlayerExitedCockpitEvent()
        {
            TryLogTime();
            FlightDataSB.Append((char)17);
        }

        internal static void AutoGetNewBlocksEvent(int count)
        {
            TryLogTime();
            FlightDataSB.Append((char)18).Append((char)(ushort)count);
        }


        internal static string GetAndClearFlightData()
        {
            var data = FlightDataSB.ToString();
            FlightDataSB.Clear();
            return data;
        }

        internal static string GetFlightData()
        {
            return FlightDataSB.ToString();
        }
    }
}