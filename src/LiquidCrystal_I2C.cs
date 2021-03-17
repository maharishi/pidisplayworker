using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.RaspberryIO;
using Unosquare.WiringPi;

namespace pidisplayworker
{
    public class LiquidCrystal_I2C
    {
        // Define some device constants
        public const int LCD_CHR = 1; // Mode - Sending data
        public const int LCD_CMD = 0; // Mode - Sending command

        public const int LINE1 = 0x80; // 1st line
        public const int LINE2 = 0xC0; // 2nd line
        public const int LINE3 = 0x94; // 3nd line
        public const int LINE4 = 0xD4; // 4nd line

        public const int LCD_BACKLIGHT_ON = 0x08;  // On
        public const int LCD_BACKLIGHT_OFF = 0x00;  // Off

        public const int ENABLE = 0b00000100; // Enable bit

        private int LCD_BackLight { get; set; }

        I2CDevice Device { get; set; }

        private LiquidCrystal_I2C()
        {

        }

        public static LiquidCrystal_I2C Instance(int deviceid)
        {
            var lcd = new LiquidCrystal_I2C();
            try
            {
                Pi.Init<BootstrapWiringPi>();
                Pi.I2C.AddDevice(deviceid);
                lcd.Device = (I2CDevice)Pi.I2C.Devices[0];
                lcd.LCD_BackLight = LCD_BACKLIGHT_ON;
                lcd.LCDInit();
            }
            catch { }
            return lcd;
        }

        private void LCDInit()
        {
            // Initialise display
            WriteByte(0x33, LCD_CMD); // Initialise
            WriteByte(0x32, LCD_CMD); // Initialise
            WriteByte(0x06, LCD_CMD); // Cursor move direction
            WriteByte(0x0C, LCD_CMD); // 0x0F On, Blink Off
            WriteByte(0x28, LCD_CMD); // Data length, number of lines, font size
            WriteByte(0x01, LCD_CMD); // Clear display
            WriteByte(0x02, LCD_CMD); // Clear display
            DelayMicroseconds(500);
        }

        void WriteByte(int bits, int mode)
        {
            //Send byte to data pins
            // bits = the data
            // mode = 1 for data, 0 for command
            int bits_high;
            int bits_low;
            // uses the two half byte writes to LCD
            bits_high = mode | (bits & 0xF0) | LCD_BackLight;
            bits_low = mode | ((bits << 4) & 0xF0) | LCD_BackLight;

            // High bits
            Device.ReadAddressByte(bits_high);
            ToggleEnable(bits_high);

            // Low bits
            Device.ReadAddressByte(bits_low);
            ToggleEnable(bits_low);
        }

        void ToggleEnable(int bits)
        {
            // Toggle enable pin on LCD display
            DelayMicroseconds(500);
            Device.ReadAddressByte((bits | ENABLE));
            DelayMicroseconds(500);
            Device.ReadAddressByte((bits & ~ENABLE));
            DelayMicroseconds(500);
        }

        public void DelayMicroseconds(int secs)
        {
            Thread.Sleep(secs / 100);
        }

        // clr lcd go home loc 0x80
        public void ClearLCD()
        {
            WriteByte(0x01, LCD_CMD);
            WriteByte(0x02, LCD_CMD);
        }

        // go to location on LCD
        public void CursorLine(int line)
        {
            WriteByte(line, LCD_CMD);
        }

        public void PrintLine(string s)
        {
            char[] b = s.ToCharArray();
            foreach (var item in b)
            {
                WriteByte((int)item, LCD_CHR);
            }
        }
    }
}
