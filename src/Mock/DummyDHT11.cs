using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace pidisplayworker.Mock
{
    public class DummyDHT11 : IDHT11
    {
        public Task<float> ReadHumidity()
        {
            return Task.Run(() => { return 50.5944444555f; });
        }

        public Task<float> ReadTemperature(bool S)
        {
            return Task.Run(() => { return 32.33444555f; });
        }
    }
}
