using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedSMBIOS {
    public enum SMBIOSWakeUpType {
        Reserved,
        Other,
        Unknown,
        APMTimer,
        ModemRing,
        LANRemote,
        PowerSwitch,
        PCIPME,
        ACPowerRestored
    }

    public class SMBIOS_SystemInformation : ISMBIOSTable {
        public int Type { get; private set; }
        public UInt16 Handle { get; private set; }

        // v2.0+
        public string Manufacturer { get; private set; }
        public string ProductName { get; private set; }
        public string Version { get; private set; }
        public string SerialNumber { get; private set; }

        // v2.1+
        public Guid Uuid { get; private set; }
        public SMBIOSWakeUpType WakeUpType { get; private set; }

        // v2.4+
        public string SKUNumber { get; private set; }
        public string Family { get; private set; }

        internal SMBIOS_SystemInformation(int type, UInt16 handle, byte[] data, IList<string> stringTable) {
            this.Type = type;
            this.Handle = handle;

            int offset = 0;

            this.Manufacturer = stringTable[data[offset++] - 1];
            this.ProductName = stringTable[data[offset++] - 1];
            this.SerialNumber = stringTable[data[offset++] - 1];
            
            // TODO: Complete SystemInformation
        }
    }
}
