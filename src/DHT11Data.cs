using System;
using System.Collections.Generic;
using System.Text;

namespace pidisplayworker
{
    public class DHT11Data
    {

        public bool IsInitialized { get; private set; } = false;

        public double Temperature { get; set; }
        public double Humidity { get; set; }

        public double TemperatureFarenheight
        {
            get
            {
                return ConvertCtoF(Temperature);
            }
        }

        public double HeatIndex
        {
            get
            {
                return ComputeHeatIndex(Temperature, Humidity, false);
            }
        }

        public DHT11Data()
        {
            IsInitialized = false;
        }

        public DHT11Data(double _humid, double _temp)
        {
            Temperature = _temp;
            Humidity = _humid;
            IsInitialized = true;
        }

        /// <summary>
        /// Converts the cto f.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns>System.Single.</returns>
        public double ConvertCtoF(double c)
        {
            return c * 9 / 5 + 32;
        }

        /// <summary>
        /// Converts the fto c.
        /// </summary>
        /// <param name="f">The f.</param>
        /// <returns>System.Single.</returns>
        public double ConvertFtoC(double f)
        {
            return (f - 32) * 5 / 9;
        }

        public double ComputeHeatIndex(double temperature, double percentHumidity, bool isFahrenheit)
        {
            // Adapted from equation at: https://github.com/adafruit/DHT-sensor-library/issues/9 and
            // Wikipedia: http://en.wikipedia.org/wiki/Heat_index
            if (!isFahrenheit)
            {
                // Celsius heat index calculation.
                return -8.784695 +
                         1.61139411 * temperature +
                         2.338549 * percentHumidity +
                        -0.14611605 * temperature * percentHumidity +
                        -0.01230809 * Math.Pow(temperature, 2) +
                        -0.01642482 * Math.Pow(percentHumidity, 2) +
                         0.00221173 * Math.Pow(temperature, 2) * percentHumidity +
                         0.00072546 * temperature * Math.Pow(percentHumidity, 2) +
                        -0.00000358 * Math.Pow(temperature, 2) * Math.Pow(percentHumidity, 2);
            }
            else
            {
                // Fahrenheit heat index calculation.
                return -42.379 +
                         2.04901523 * temperature +
                        10.14333127 * percentHumidity +
                        -0.22475541 * temperature * percentHumidity +
                        -0.00683783 * Math.Pow(temperature, 2) +
                        -0.05481717 * Math.Pow(percentHumidity, 2) +
                         0.00122874 * Math.Pow(temperature, 2) * percentHumidity +
                         0.00085282 * temperature * Math.Pow(percentHumidity, 2) +
                        -0.00000199 * Math.Pow(temperature, 2) * Math.Pow(percentHumidity, 2);
            }
        }
    }
}
