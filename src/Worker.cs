using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ConsoleDump;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LCDLibrary;

namespace pidisplayworker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private string DomainBlocked { get; set; }
        private string DNSQueryToday { get; set;}
        private string AdsBlocked { get; set;}
        private string AdsBlockedPercentage { get; set; }

        private DHT11Data LastResult { get; set; }

        private readonly ILiquidCrystal_I2C _lcd;

        private readonly IDHT11 _dHT; 

        private readonly string PiHoleURL;

        public Worker(ILogger<Worker> logger, ILiquidCrystal_I2C lcd_, IDHT11 dHT_, string url_)
        {
            _logger = logger;
            _lcd = lcd_;
            PiHoleURL = url_;
            _dHT = dHT_;
            DomainBlocked = DNSQueryToday = AdsBlocked = AdsBlockedPercentage = string.Empty;
        }
        

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _lcd.ClearLCD();
            _logger.LogInformation("Worker stopping at: {time}", DateTimeOffset.Now);
            _logger.LogInformation($"Domains Blocked : {DomainBlocked} | DNS Queries Today : {DNSQueryToday} | Ads Blocked : {AdsBlocked} | Ads Percentage {AdsBlockedPercentage}");
            _logger.LogInformation($"Temp C : {LastResult.Temperature} | Temp F : {LastResult.TemperatureFarenheight} | Humidity : {LastResult.Humidity:P0} | Heat Index : {LastResult.HeatIndex}");
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int counter = 0;
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (counter > 0)
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            var res = client.GetStringAsync(PiHoleURL).Result;
                            var jsonDoc = JsonDocument.Parse(Encoding.Default.GetBytes(res.ToString()));
                            DomainBlocked = jsonDoc.RootElement.GetProperty("domains_being_blocked").GetString(); //.Dump("Domains Blocked");
                            DNSQueryToday = jsonDoc.RootElement.GetProperty("dns_queries_today").GetString(); //.Dump("DNS Queries Today");
                            AdsBlocked = jsonDoc.RootElement.GetProperty("ads_blocked_today").GetString(); //.Dump("Ads Blocked");
                            AdsBlockedPercentage = jsonDoc.RootElement.GetProperty("ads_percentage_today").GetString(); //.Dump("Ads Percentage");
                        }

                        if (_logger.IsEnabled(LogLevel.Trace))
                        {
                            _logger.LogTrace($"Ad Domans: {DomainBlocked,9}");
                            _logger.LogTrace($"DNS Qry2D: {DNSQueryToday,9}");
                            _logger.LogTrace($"Ads Blokd: {AdsBlocked,9}");
                            _logger.LogTrace($"Ads Blok%: {$"{AdsBlockedPercentage}%",9}");
                        }

                        _lcd.CursorLine(LiquidCrystal_I2C.LINE1);
                        _lcd.PrintLine($"Ad Domans: {DomainBlocked,9}");
                        _lcd.CursorLine(LiquidCrystal_I2C.LINE2);
                        _lcd.PrintLine($"DNS Qry2D: {DNSQueryToday,9}");
                        _lcd.CursorLine(LiquidCrystal_I2C.LINE3);
                        _lcd.PrintLine($"Ads Blokd: {AdsBlocked,9}");
                        _lcd.CursorLine(LiquidCrystal_I2C.LINE4);
                        _lcd.PrintLine($"Ads Blok%: {$"{AdsBlockedPercentage}%",9}");

                        if (counter != 5)
                        {
                            counter++;
                        }
                        else
                        {
                            counter = -1;
                        }
                        await Task.Delay(1000, stoppingToken);
                    }
                    else
                    {
                        LastResult = _dHT.RetrieveSensorData();
                        //dhtData.Dump("DHData");
                        if (_logger.IsEnabled(LogLevel.Trace))
                        {
                            _logger.LogTrace($"Temp C      : { LastResult.Temperature,4:#0.00}?C");
                            _logger.LogTrace($"Humidity    : { LastResult.Humidity,4:P0}%");
                            _logger.LogTrace($"Temp F      : { LastResult.Temperature,4:#0.00}?F");
                            _logger.LogTrace($"Heat Index  : { LastResult.Humidity,4:#0.00}");
                        }

                        _lcd.CursorLine(LiquidCrystal_I2C.LINE1);
                        _lcd.PrintLine($"Temp C      : { LastResult.Temperature,6:#0.00}");
                        _lcd.CursorLine(LiquidCrystal_I2C.LINE2);
                        _lcd.PrintLine($"Temp F      : { LastResult.TemperatureFarenheight,6:#0.00}");
                        _lcd.CursorLine(LiquidCrystal_I2C.LINE3);
                        _lcd.PrintLine($"Humidity    : { LastResult.Humidity,6:P0}");
                        _lcd.CursorLine(LiquidCrystal_I2C.LINE4);
                        _lcd.PrintLine($"Heat Index  : { LastResult.HeatIndex,6:#0.00}");

                        if (counter != -5)
                        {
                            counter--;
                        }
                        else
                        {
                            counter = 1;
                        }

                        await Task.Delay(2000, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    if (ex is TaskCanceledException)
                    {
                        _logger.LogInformation("Worker stopping at: {time}", DateTimeOffset.Now);
                        _ = $"Domains Blocked : {DomainBlocked} | DNS Queries Today : {DNSQueryToday} | Ads Blocked : {AdsBlocked} | Ads Percentage {AdsBlockedPercentage}".Dump("Last Statistics");
                        _ = $"Temp C : {LastResult.Temperature} | Temp F : {LastResult.TemperatureFarenheight} | Humidity : {LastResult.Humidity:P0} | Heat Index : {LastResult.HeatIndex}".Dump("Last Temp & Humidity");
                    }
                    else
                    {
                        _logger.LogError("Worker error at: {time}", DateTimeOffset.Now);
                        _ = $"Domains Blocked : {DomainBlocked} | DNS Queries Today : {DNSQueryToday} | Ads Blocked : {AdsBlocked} | Ads Percentage {AdsBlockedPercentage}".Dump("Last Statistics");
                        _ = $"Temp C : {LastResult.Temperature} | Temp F : {LastResult.TemperatureFarenheight} | Humidity : {LastResult.Humidity:P0} | Heat Index : {LastResult.HeatIndex}".Dump("Last Temp & Humidity");
                        ex.Dump();
                        await Task.Delay(2000, stoppingToken);
                    }
                }
            }
        }
    }
}
