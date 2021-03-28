using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace pidisplayworker.Mock
{
    public class DummyDHT11 : IDHT11
    {
        public DHT11Data RetrieveSensorData()
        {
            return new DHT11Data(_humid: 30.0, _temp: 30.0);
        }
    }
}
