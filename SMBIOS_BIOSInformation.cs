using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedSMBIOS {
    public class SMBIOS_BIOSInformation : ISMBIOSTable {
        public int Type { get; private set; }
        public UInt16 Handle { get; private set; }

        // v2.0+
        public string Vendor { get; private set; }
        public string Version { get; private set; }
        public UInt16 StartingAddressSegment { get; private set; }
        public string ReleaseDate { get; private set; }
        public int ROMSize { get; private set; }
        public UInt64 Characteristics { get; private set; }

        // v2.4+
        public byte[] CharacteristicExtensions { get; private set; }
        public int MajorVersion { get; private set; }
        public int MinorVersion { get; private set; }
        public int ControllerMajorVersion { get; private set; }
        public int ControllerMinorVersion { get; private set; }

        // v3.1+
        public UInt16 ROMSizeExtended { get; private set; }

        internal SMBIOS_BIOSInformation(int type, UInt16 handle, byte[] data, IList<string> stringTable) {
            this.Type = type;
            this.Handle = handle;

            int offset = 0;

            this.Vendor = stringTable[data[offset++] - 1];
            this.Version = stringTable[data[offset++] - 1];
            this.StartingAddressSegment = BitConverter.ToUInt16(data, offset);
            offset += 2;
            this.ReleaseDate = stringTable[data[offset++] - 1];
            this.ROMSize = 0x10000 * (data[offset++] + 1);
            this.Characteristics = BitConverter.ToUInt64(data, offset);
            offset += 8;

            // TODO: Complete BIOSInformation
        }
    }
}
