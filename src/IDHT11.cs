using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace pidisplayworker
{
    public interface IDHT11
    {
        public Task<float> ReadTemperature(bool S);

        public Task<float> ReadHumidity();

    }
}
