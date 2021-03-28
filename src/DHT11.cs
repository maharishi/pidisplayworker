using ConsoleDump;
using Swan.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;
using Unosquare.WiringPi;

namespace pidisplayworker
{
    /// <summary>
    /// Class DHT11.
    /// </summary>
    public class DHT11 : IDHT11
    {
        const long BitPulseMidMicroseconds = 60; // (26 ... 28)µs for false; (29 ... 70)µs for true
        const uint PullDownMicroseconds = 21000;

    
        /// <summary>
        /// The _data pin
        /// </summary>
        private IGpioPin _dataPin;

        private DHT11Data _lastResult = new DHT11Data();

        /// <summary>
        /// Initializes a new instance of the <see cref="DHT11"/> class.
        /// </summary>
        /// <param name="datatPin">The datat pin.</param>
        /// <param name="sensor">The sensor.</param>
        /// <exception cref="System.ArgumentException">Parameter cannot bet null.;dataPin</exception>
        public DHT11(IGpioPin datatPin)
        {
            if (datatPin != null)
            {
                _dataPin = datatPin;
            }
            else
            {
                throw new ArgumentException("Parameter cannot bet null.", "dataPin");
            }
        }

        /// <summary>
        /// Retrieves the sensor data.
        /// </summary>
        /// <returns>The event arguments that will be read from the sensor.</returns>
        public DHT11Data RetrieveSensorData()
        {
            // Prepare buffer to store measure and checksum
            var data = new byte[5];

            // Start to communicate with sensor
            // Inform sensor that must finish last execution and put it's state in idle
            _dataPin.PinMode = GpioPinDriveMode.Output;

            // Send request to transmission from board to sensor
            _dataPin.Write(GpioPinValue.Low);
            Pi.Timing.SleepMicroseconds(PullDownMicroseconds);
            _dataPin.Write(GpioPinValue.High);

            // Wait for sensor response
            _dataPin.PinMode = GpioPinDriveMode.Input;

            try
            {
                // Read acknowledgement from sensor
                _dataPin.WaitForValue(GpioPinValue.Low, 50);

                _dataPin.WaitForValue(GpioPinValue.High, 50);

                // Begins data transmission
                _dataPin.WaitForValue(GpioPinValue.Low, 50);


                // Read 40 bits to acquire:
                //   16 bit -> Humidity
                //   16 bit -> Temperature
                //   8 bit -> Checksum
                var stopwatch = new HighResolutionTimer();

                for (var i = 0; i < 40; i++)
                {
                    stopwatch.Reset();
                    _dataPin.WaitForValue(GpioPinValue.High, 50);

                    stopwatch.Start();
                    _dataPin.WaitForValue(GpioPinValue.Low, 50);

                    stopwatch.Stop();

                    data[i / 8] <<= 1;

                    // Check if signal is 1 or 0
                    if (stopwatch.ElapsedMicroseconds > BitPulseMidMicroseconds)
                        data[i / 8] |= 1;
                }

                // End transmission
                _dataPin.WaitForValue(GpioPinValue.High, 50);
                var _validData = IsDataValid(data)
                    ? new DHT11Data(_humid: ((data[0] + (data[1] * 0.1)) / 100.0), _temp: (data[2] + ((data[3] & 0x0f) * 0.1)))
                    : new DHT11Data();

                if (_validData.IsInitialized) _lastResult = _validData;

                // Compute the checksum
                return _lastResult;
            }
            catch
            {
                return new DHT11Data() { Humidity = 0.0, Temperature = 0.0 };
            }
        }

        private static bool IsDataValid(byte[] data) =>
           ((data[0] + data[1] + data[2] + data[3]) & 0xff) == data[4];

    }
}
