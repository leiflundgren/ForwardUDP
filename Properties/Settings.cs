using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForwardUDP.Properties
{
    public class Settings
    {
        public Settings() { }

        public string? Local;
        public string[]? Targets;
        public int LogLevel = 3;
        public string? LogPath;
    }
}
