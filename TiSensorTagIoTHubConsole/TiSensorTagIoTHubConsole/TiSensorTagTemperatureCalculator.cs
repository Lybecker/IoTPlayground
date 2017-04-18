using System;

namespace TiSensorTagIoTHubConsole
{
    // http://processors.wiki.ti.com/index.php/SensorTag_User_Guide
    public class TiSensorTagTemperatureCalculator
    {
        public double CalculateAmbientTemperature(byte[] sensorData, TemperatureScale scale)
        {
            int offset = 2;

            if (scale == TemperatureScale.Celsius)
                return BitConverter.ToUInt16(sensorData, offset) / 128.0;
            else
                return (BitConverter.ToUInt16(sensorData, offset) / 128.0) * 1.8 + 32;
        }

        public double CalculateTargetTemperature(byte[] sensorData, double ambientTemperature, TemperatureScale scale)
        {
            if (scale == TemperatureScale.Celsius)
                return CalculateTargetTemperature(sensorData, ambientTemperature);
            else
                return CalculateTargetTemperature(sensorData, ambientTemperature) * 1.8 + 32;
        }



        // info about the calculation: http://www.ti.com/lit/ug/sbou107/sbou107.pdf
        private double CalculateTargetTemperature(byte[] sensorData, double ambientTemperature)
        {
            double Vobj2 = BitConverter.ToInt16(sensorData, 0);
            Vobj2 *= 0.00000015625;

            double Tdie = ambientTemperature + 273.15;

            double S0 = 5.593E-14;  // Calibration factor
            double a1 = 1.75E-3;
            double a2 = -1.678E-5;
            double b0 = -2.94E-5;
            double b1 = -5.7E-7;
            double b2 = 4.63E-9;
            double c2 = 13.4;
            double Tref = 298.15;
            double S = S0 * (1 + a1 * (Tdie - Tref) + a2 * Math.Pow((Tdie - Tref), 2));
            double Vos = b0 + b1 * (Tdie - Tref) + b2 * Math.Pow((Tdie - Tref), 2);
            double fObj = (Vobj2 - Vos) + c2 * Math.Pow((Vobj2 - Vos), 2);
            double tObj = Math.Pow(Math.Pow(Tdie, 4) + (fObj / S), .25);

            return tObj - 273.15;
        }
    }

    public enum TemperatureScale
    {
        Celsius,
        Farenheit
    }
}
