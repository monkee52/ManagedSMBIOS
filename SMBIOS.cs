using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ManagedSMBIOS {
    [StructLayout(LayoutKind.Sequential)]
    internal struct RawSMBIOSData {
        public byte UsedCallingMethod;
        public byte MajorVersion;
        public byte MinorVersion;
        public byte DMIRevision;
        public int Length;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SMBIOSTableHeader {
        public byte Type;
        public byte Length;
        public UInt16 Handle;
    }

    public interface ISMBIOSTable {
        int Type { get; }
        UInt16 Handle { get; }
    }

    public static class SMBIOS {
        public static int MajorVersion { get; private set; }
        public static int MinorVersion { get; private set; }

        private static IDictionary<Type, ISMBIOSTable> tables;

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern int GetSystemFirmwareTable([In]UInt32 firmwareTableProviderSignature, [In]UInt32 firmwareTableId, [Out]IntPtr pFirmwareTableBuffer, [In]int bufferSize);

        static SMBIOS() {
            // Determine size needed for entire SMBIOS table
            int size = GetSystemFirmwareTable(0x52534d42, 0x00000000, IntPtr.Zero, 0);

            SMBIOS.tables = new Dictionary<Type, ISMBIOSTable>();

            if (size == 0) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            IntPtr pSmbios = Marshal.AllocHGlobal(size);

            try {
                // Retrieve SMBIOS
                size = GetSystemFirmwareTable(0x52534d42, 0x00000000, pSmbios, size);

                if (size == 0) {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                // Provide basic information
                RawSMBIOSData smbiosInfo = (RawSMBIOSData)Marshal.PtrToStructure(pSmbios, typeof(RawSMBIOSData));

                SMBIOS.MajorVersion = smbiosInfo.MajorVersion;
                SMBIOS.MinorVersion = smbiosInfo.MinorVersion;

                // Decode tables
                IntPtr smbios = pSmbios + Marshal.SizeOf(typeof(RawSMBIOSData));
                int headerSize = Marshal.SizeOf(typeof(SMBIOSTableHeader));
                int length = smbiosInfo.Length;
                int offset = 0;

                while (offset < length) {
                    SMBIOSTableHeader header = (SMBIOSTableHeader)Marshal.PtrToStructure(smbios + offset, typeof(SMBIOSTableHeader));
                    IList<string> stringTable = new List<string>();

                    // Copy table data
                    byte[] data = new byte[header.Length - headerSize];

                    Marshal.Copy(smbios + offset + headerSize, data, 0, header.Length - headerSize);

                    offset += header.Length;

                    // Skip empty string tables
                    if (Marshal.ReadInt16(smbios + offset) == 0) {
                        offset += 2;
                    } else {
                        while (true) {
                            string str = Marshal.PtrToStringAnsi(smbios + offset);

                            // A zero length string indicates the end of the table
                            if (str.Length == 0) {
                                break;
                            }

                            offset += str.Length + 1;

                            stringTable.Add(str);
                        }

                        offset += 1;
                    }

                    switch (header.Type) {
                        case 0: // BIOS Information
                            SMBIOS.AddTable(new SMBIOS_BIOSInformation(header.Type, header.Handle, data, stringTable));
                            break;
                        case 1: // System Information
                            SMBIOS.AddTable(new SMBIOS_SystemInformation(header.Type, header.Handle, data, stringTable));
                            break;
                        default:
                            Debug.WriteLine("Unknown table type: {0}", header.Type);
                            break;
                    }
                }
            } finally {
                Marshal.FreeHGlobal(pSmbios);
            }
        }

        private static void AddTable(ISMBIOSTable table) {
            SMBIOS.tables.Add(table.GetType(), table);
        }

        public static TTable GetTable<TTable>() where TTable : class, ISMBIOSTable {
            if (SMBIOS.tables.ContainsKey(typeof(TTable))) {
                return (TTable)SMBIOS.tables[typeof(TTable)];
            } else {
                return null;
            }
        }
    }
}
