using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace pidisplayworker
{
    public interface IDHT11
    {
        public DHT11Data RetrieveSensorData();
    }
}
