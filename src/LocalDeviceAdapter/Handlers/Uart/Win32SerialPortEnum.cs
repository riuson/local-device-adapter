using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace LocalDeviceAdapter.Handlers.Uart
{
    internal class Win32SerialPortEnum
    {
        private const uint DIGCF_PRESENT = 2;
        private const uint DIGCF_DEVICEINTERFACE = 16;
        private const uint DEVICEDESC = 0;
        private const uint DICS_FLAG_GLOBAL = 1;
        private const uint DIREG_DEV = 1;
        private const uint KEY_QUERY_VALUE = 1;
        private const string GUID_DEVINTERFACE_COMPORT = "86E0D1E0-8089-11D0-9CE4-08003E301F73";

        [DllImport("setupapi.dll")]
        private static extern int SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport("setupapi.dll")]
        private static extern bool SetupDiEnumDeviceInfo(
            IntPtr DeviceInfoSet,
            int MemberIndex,
            ref SP_DEVINFO_DATA DeviceInterfaceData);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiGetDeviceRegistryProperty(
            IntPtr deviceInfoSet,
            ref SP_DEVINFO_DATA deviceInfoData,
            uint property,
            out uint propertyRegDataType,
            byte[] propertyBuffer,
            uint propertyBufferSize,
            out uint requiredSize);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiGetDeviceRegistryProperty(
            IntPtr deviceInfoSet,
            ref SP_DEVINFO_DATA deviceInfoData,
            uint property,
            out uint propertyRegDataType,
            IntPtr pPropertyBuffer,
            uint propertyBufferSize,
            out uint requiredSize);

        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern IntPtr SetupDiGetClassDevs(
            ref Guid gClass,
            uint iEnumerator,
            IntPtr hParent,
            uint nFlags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetupDiOpenDevRegKey(
            IntPtr hDeviceInfoSet,
            ref SP_DEVINFO_DATA deviceInfoData,
            uint scope,
            uint hwProfile,
            uint parameterRegistryValueKind,
            uint samDesired);

        [DllImport("kernel32.dll")]
        private static extern int GetLastError();

        public static IEnumerable<DeviceInfo> GetAllCOMPorts()
        {
            var gClass = new Guid("86E0D1E0-8089-11D0-9CE4-08003E301F73");
            var classDevs = SetupDiGetClassDevs(ref gClass, 0U, IntPtr.Zero, 18U);
            if (classDevs == IntPtr.Zero)
                throw new Exception("Не удалось получить набор информации по COM портам");
            try
            {
                var allComPorts = new List<DeviceInfo>();
                var MemberIndex = 0;
                while (true)
                {
                    var DeviceInterfaceData = new SP_DEVINFO_DATA();
                    DeviceInterfaceData.cbSize = Marshal.SizeOf(typeof(SP_DEVINFO_DATA));
                    if (SetupDiEnumDeviceInfo(classDevs, MemberIndex, ref DeviceInterfaceData))
                    {
                        var deviceInfo = new DeviceInfo
                        {
                            Name = GetDeviceName(classDevs, DeviceInterfaceData),
                            Description = GetDevicePropertyString(classDevs, DeviceInterfaceData, SDRP.SDRP_DEVICEDESC)
                        };
                        allComPorts.Add(deviceInfo);
                        ++MemberIndex;
                    }
                    else
                    {
                        break;
                    }
                }

                return allComPorts;
            }
            finally
            {
                SetupDiDestroyDeviceInfoList(classDevs);
            }
        }

        private static string GetDeviceName(
            IntPtr pDevInfoSet,
            SP_DEVINFO_DATA deviceInfoData)
        {
            var empty = string.Empty;
            var preexistingHandle = SetupDiOpenDevRegKey(pDevInfoSet, ref deviceInfoData, 1U, 0U, 1U, 1U);
            if (preexistingHandle == IntPtr.Zero)
                throw new Exception("Не удалось открыть ключ реестра с информацией о конфигурации устройства");
            try
            {
                using (var handle = new SafeRegistryHandle(preexistingHandle, true))
                {
                    using (var registryKey = RegistryKey.FromHandle(handle))
                    {
                        return Convert.ToString(registryKey.GetValue("PortName"));
                    }
                }
            }
            catch
            {
                throw new Exception("Не удалось считать из реестра значение PortName для устройства " +
                                    deviceInfoData.ClassGuid);
            }
        }

        private static byte[] GetDeviceProperty(
            IntPtr hDeviceInfoSet,
            SP_DEVINFO_DATA deviceInfoData,
            SDRP property,
            out RegistryDataType propertyRegistryDataType)
        {
            propertyRegistryDataType = RegistryDataType.REG_NONE;
            uint propertyRegDataType;
            uint requiredSize;
            SetupDiGetDeviceRegistryProperty(hDeviceInfoSet, ref deviceInfoData, (uint)property,
                out propertyRegDataType, IntPtr.Zero, 0U, out requiredSize);
            var propertyBuffer = requiredSize > 0U
                ? new byte[(int)requiredSize]
                : throw new Exception("Can not read registry value PortName for device " + deviceInfoData.ClassGuid);
            if (SetupDiGetDeviceRegistryProperty(hDeviceInfoSet, ref deviceInfoData, (uint)property,
                    out propertyRegDataType, propertyBuffer, requiredSize, out requiredSize))
                propertyRegistryDataType = (RegistryDataType)propertyRegDataType;
            return propertyBuffer;
        }

        private static string GetDevicePropertyString(
            IntPtr hDeviceInfoSet,
            SP_DEVINFO_DATA deviceInfoData,
            SDRP property)
        {
            RegistryDataType propertyRegistryDataType;
            var devicePropertyString = Encoding.Unicode.GetString(GetDeviceProperty(hDeviceInfoSet, deviceInfoData,
                property, out propertyRegistryDataType));
            switch (propertyRegistryDataType)
            {
                case RegistryDataType.REG_SZ:
                    devicePropertyString = devicePropertyString.Replace("\0", "").Trim();
                    break;
                case RegistryDataType.REG_EXPAND_SZ:
                    devicePropertyString = devicePropertyString.Replace("\0", "").Trim();
                    break;
                case RegistryDataType.REG_MULTI_SZ:
                    devicePropertyString = devicePropertyString.Replace("\0\0", "").Replace("\0", "").Trim();
                    break;
            }

            return devicePropertyString;
        }

        private enum SDRP : uint
        {
            SDRP_DEVICEDESC,
            SDRP_HARDWAREID,
            SDRP_COMPATIBLEIDS,
            SDRP_UNUSED0,
            SDRP_SERVICE,
            SDRP_UNUSED1,
            SDRP_UNUSED2,
            SDRP_CLASS,
            SDRP_CLASSGUID,
            SDRP_DRIVER,
            SDRP_CONFIGFLAGS,
            SDRP_MFG,
            SDRP_FRIENDLYNAME,
            SDRP_LOCATION_INFORMATION,
            SDRP_PHYSICAL_DEVICE_OBJECT_NAME,
            SDRP_CAPABILITIES,
            SDRP_UI_NUMBER,
            SDRP_UPPERFILTERS,
            SDRP_LOWERFILTERS,
            SDRP_BUSTYPEGUID,
            SDRP_LEGACYBUSTYPE,
            SDRP_BUSNUMBER,
            SDRP_ENUMERATOR_NAME,
            SDRP_SECURITY,
            SDRP_SECURITY_SDS,
            SDRP_DEVTYPE,
            SDRP_EXCLUSIVE,
            SDRP_CHARACTERISTICS,
            SDRP_ADDRESS,
            SDRP_UI_NUMBER_DESC_FORMAT,
            SDRP_DEVICE_POWER_DATA,
            SDRP_REMOVAL_POLICY,
            SDRP_REMOVAL_POLICY_HW_DEFAULT,
            SDRP_REMOVAL_POLICY_OVERRIDE,
            SDRP_INSTALL_STATE
        }

        private enum RegistryDataType : uint
        {
            REG_NONE = 0,
            REG_SZ = 1,
            REG_EXPAND_SZ = 2,
            REG_BINARY = 3,
            REG_DWORD = 4,
            REG_DWORD_LITTLE_ENDIAN = 4,
            REG_DWORD_BIG_ENDIAN = 5,
            REG_LINK = 6,
            REG_MULTI_SZ = 7,
            REG_RESOURCE_LIST = 8,
            REG_FULL_RESOURCE_DESCRIPTOR = 9,
            REG_RESOURCE_REQUIREMENTS_LIST = 10, // 0x0000000A
            REG_QWORD = 11, // 0x0000000B
            REG_QWORD_LITTLE_ENDIAN = 11 // 0x0000000B
        }

        private struct SP_DEVINFO_DATA
        {
            public int cbSize;
            public Guid ClassGuid;
            public int DevInst;
            public UIntPtr Reserved;
        }

        public struct DeviceInfo
        {
            public string Name;
            public string Description;
        }
    }
}