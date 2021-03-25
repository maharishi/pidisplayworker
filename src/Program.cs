using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;
using Unosquare.WiringPi;

namespace pidisplayworker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSystemd()
                .ConfigureServices((hostContext, services) =>
                {
                    Pi.Init<BootstrapWiringPi>();
                    Pi.I2C.AddDevice(0x3f);

                    //services.AddTransient<string>(url => "http://192.168.1.27/admin/api.php?summary");
                    //services.AddTransient<ILiquidCrystal_I2C>(lcd => new Mock.DummyLiquidCrystal_I2C());
                    //services.AddTransient<IDHT11>(dht => new Mock.DummyDHT11());

                    services.AddTransient<string>(url => Properties.Resources.PiHoleURL);
                    services.AddTransient<ILiquidCrystal_I2C>(lcd => new LiquidCrystal_I2C((I2CDevice)Pi.I2C.Devices[0]));
                    services.AddTransient<IDHT11>(dht => new DHT11(Pi.Gpio[BcmPin.Gpio05], SensorTypes.DHT11));
                    services.AddHostedService<Worker>();
                });
    }
}
