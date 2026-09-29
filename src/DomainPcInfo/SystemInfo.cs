using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace DomainPcInfo;

internal enum ComputerJoinType
{
    Unknown,
    Workgroup,
    Domain
}

internal sealed record ComputerJoinInfo(
    ComputerJoinType JoinType,
    string Name);

internal static class SystemInfo
{
    private enum NetJoinStatus
    {
        NetSetupUnknownStatus = 0,
        NetSetupUnjoined = 1,
        NetSetupWorkgroupName = 2,
        NetSetupDomainName = 3
    }

    [DllImport("Netapi32.dll", CharSet = CharSet.Unicode)]
    private static extern int NetGetJoinInformation(
        string? lpServer,
        out IntPtr lpNameBuffer,
        out NetJoinStatus bufferType);

    [DllImport("Netapi32.dll")]
    private static extern int NetApiBufferFree(IntPtr buffer);

    public static string ComputerName => Environment.MachineName;

    public static ComputerJoinInfo GetJoinInfo()
    {
        IntPtr buffer = IntPtr.Zero;

        try
        {
            int result = NetGetJoinInformation(null, out buffer, out NetJoinStatus status);
            if (result != 0)
            {
                AppLog.Write($"NetGetJoinInformation returned {result}");
                return new ComputerJoinInfo(ComputerJoinType.Unknown, "Не определено");
            }

            string systemName = Marshal.PtrToStringUni(buffer) ?? "Не определено";

            if (status == NetJoinStatus.NetSetupDomainName)
            {
                // Для отображения предпочитаем DNS-имя домена, если Windows его сообщает.
                string dnsDomain = IPGlobalProperties.GetIPGlobalProperties().DomainName;
                string displayName = string.IsNullOrWhiteSpace(dnsDomain) ? systemName : dnsDomain;
                return new ComputerJoinInfo(ComputerJoinType.Domain, displayName);
            }

            if (status is NetJoinStatus.NetSetupWorkgroupName or NetJoinStatus.NetSetupUnjoined)
            {
                return new ComputerJoinInfo(ComputerJoinType.Workgroup, systemName);
            }

            return new ComputerJoinInfo(ComputerJoinType.Unknown, systemName);
        }
        catch (Exception ex)
        {
            AppLog.Write(ex.ToString());
            return new ComputerJoinInfo(ComputerJoinType.Unknown, "Не определено");
        }
        finally
        {
            if (buffer != IntPtr.Zero)
                NetApiBufferFree(buffer);
        }
    }
}
