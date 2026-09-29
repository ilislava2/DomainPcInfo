using Microsoft.Win32;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace DomainPcInfo;

internal sealed record DomainControllerStatus(
    string ControllerName,
    bool IsAvailable,
    bool IsManual);

internal static class DomainInfo
{
    private const string RegistryPath = @"SOFTWARE\DomainPcInfo";
    private const string RegistryValueName = "DomainController";

    private const uint DsDirectoryServiceRequired = 0x00000010;
    private const uint DsForceRediscovery = 0x00000001;
    private const uint DsReturnDnsName = 0x40000000;

    [StructLayout(LayoutKind.Sequential)]
    private struct DomainControllerInfoNative
    {
        public IntPtr DomainControllerName;
        public IntPtr DomainControllerAddress;
        public uint DomainControllerAddressType;
        public Guid DomainGuid;
        public IntPtr DomainName;
        public IntPtr DnsForestName;
        public uint Flags;
        public IntPtr DcSiteName;
        public IntPtr ClientSiteName;
    }

    [DllImport("Netapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "DsGetDcNameW")]
    private static extern int DsGetDcName(
        string? computerName,
        string? domainName,
        IntPtr domainGuid,
        string? siteName,
        uint flags,
        out IntPtr domainControllerInfo);

    [DllImport("Netapi32.dll")]
    private static extern int NetApiBufferFree(IntPtr buffer);

    public static async Task<DomainControllerStatus> GetStatusAsync()
    {
        string? configuredDc = GetConfiguredDomainController();
        bool manual = !string.IsNullOrWhiteSpace(configuredDc);

        string? controller = manual
            ? NormalizeDcName(configuredDc!)
            : DiscoverDomainController(forceRediscovery: false);

        if (string.IsNullOrWhiteSpace(controller))
            return new DomainControllerStatus("Не определён", false, manual);

        bool available = await PingAsync(controller);

        // Если Windows вернула закэшированный недоступный DC, один раз просим DC Locator
        // выполнить новое обнаружение. Для ручного DC этого не делаем.
        if (!available && !manual)
        {
            string? rediscovered = DiscoverDomainController(forceRediscovery: true);
            if (!string.IsNullOrWhiteSpace(rediscovered))
            {
                controller = rediscovered;
                available = await PingAsync(controller);
            }
        }

        return new DomainControllerStatus(controller, available, manual);
    }

    private static string? GetConfiguredDomainController()
    {
        try
        {
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(RegistryPath, writable: false);
            string? value = key?.GetValue(RegistryValueName) as string;
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        catch (Exception ex)
        {
            AppLog.Write(ex.ToString());
            return null;
        }
    }

    private static string? DiscoverDomainController(bool forceRediscovery)
    {
        IntPtr buffer = IntPtr.Zero;

        try
        {
            uint flags = DsDirectoryServiceRequired | DsReturnDnsName;
            if (forceRediscovery)
                flags |= DsForceRediscovery;

            // domainName = null: DC Locator использует основной домен локального компьютера.
            int result = DsGetDcName(
                null,
                null,
                IntPtr.Zero,
                null,
                flags,
                out buffer);

            if (result != 0 || buffer == IntPtr.Zero)
            {
                AppLog.Write($"DsGetDcName returned {result}");
                return null;
            }

            var info = Marshal.PtrToStructure<DomainControllerInfoNative>(buffer);
            string? name = Marshal.PtrToStringUni(info.DomainControllerName);
            return string.IsNullOrWhiteSpace(name) ? null : NormalizeDcName(name);
        }
        catch (Exception ex)
        {
            AppLog.Write(ex.ToString());
            return null;
        }
        finally
        {
            if (buffer != IntPtr.Zero)
                NetApiBufferFree(buffer);
        }
    }

    private static async Task<bool> PingAsync(string host)
    {
        try
        {
            using var ping = new Ping();
            PingReply reply = await ping.SendPingAsync(host, 2000);
            return reply.Status == IPStatus.Success;
        }
        catch (Exception ex)
        {
            AppLog.Write(ex.ToString());
            return false;
        }
    }

    private static string NormalizeDcName(string value) =>
        value.Trim().TrimStart('\\');
}
