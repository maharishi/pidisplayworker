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

namespace pidisplayworker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private string DomainBlocked { get; set; }
        private string DNSQueryToday { get; set;}
        private string AdsBlocked { get; set;}
        private string AdsBlockedPercentage { get; set; }

        private LiquidCrystal_I2C lcd { get; set; }

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            lcd = LiquidCrystal_I2C.Instance(0x3f);
            DomainBlocked = DNSQueryToday = AdsBlocked = AdsBlockedPercentage = string.Empty;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            lcd.ClearLCD();
            _logger.LogInformation("Worker stopping at: {time}", DateTimeOffset.Now);
            _ = $"Domains Blocked : {DomainBlocked} | DNS Queries Today : {DNSQueryToday} | Ads Blocked : {AdsBlocked} | Ads Percentage {AdsBlockedPercentage}".Dump("Last Statistics");
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var res = client.GetStringAsync(Properties.Resources.PiHoleURL).Result;
                        var jsonDoc = JsonDocument.Parse(Encoding.Default.GetBytes(res.ToString()));
                        DomainBlocked = jsonDoc.RootElement.GetProperty("domains_being_blocked").GetString(); //.Dump("Domains Blocked");
                        DNSQueryToday = jsonDoc.RootElement.GetProperty("dns_queries_today").GetString(); //.Dump("DNS Queries Today");
                        AdsBlocked = jsonDoc.RootElement.GetProperty("ads_blocked_today").GetString(); //.Dump("Ads Blocked");
                        AdsBlockedPercentage = jsonDoc.RootElement.GetProperty("ads_percentage_today").GetString(); //.Dump("Ads Percentage");
                    }

                    lcd.CursorLine(LiquidCrystal_I2C.LINE1);
                    lcd.PrintLine("Ad Domans: " + DomainBlocked.PadLeft(9));
                    lcd.CursorLine(LiquidCrystal_I2C.LINE2);
                    lcd.PrintLine("DNS Qry2D: " + DNSQueryToday.PadLeft(9));
                    lcd.CursorLine(LiquidCrystal_I2C.LINE3);
                    lcd.PrintLine("Ads Blokd: " + AdsBlocked.PadLeft(9));
                    lcd.CursorLine(LiquidCrystal_I2C.LINE4);
                    lcd.PrintLine("Ads Blok%: " + $"{AdsBlockedPercentage}%".PadLeft(9));
                    await Task.Delay(1000, stoppingToken);
                }
                catch (Exception ex)
                {
                    if (ex is TaskCanceledException)
                    {
                        _logger.LogInformation("Worker stopping at: {time}", DateTimeOffset.Now);
                        _ = $"Domains Blocked : {DomainBlocked} | DNS Queries Today : {DNSQueryToday} | Ads Blocked : {AdsBlocked} | Ads Percentage {AdsBlockedPercentage}".Dump("Last Statistics");
                    }
                    else
                    {
                        _logger.LogError("Worker error at: {time}", DateTimeOffset.Now);
                        _ = $"Domains Blocked : {DomainBlocked} | DNS Queries Today : {DNSQueryToday} | Ads Blocked : {AdsBlocked} | Ads Percentage {AdsBlockedPercentage}".Dump("Last Statistics");
                        ex.Dump();
                        await Task.Delay(2000, stoppingToken);
                    }
                }
            }
        }
    }
}
