using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeddedDevice_FAN
{
    public class DeviceSettings
    {
        public string DeviceId { get; set; } = "Fan01";
        public int ApiPort { get; set; } = 5000;
        public decimal DefaultSpeed { get; set; } = 1.0m;
    }
}
