using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace DomainPcInfo;

internal sealed record PhysicalNetworkAdapterInfo(
    string Name,
    IReadOnlyList<string> IPv4Addresses);

internal static class NetworkInfo
{
    public static IReadOnlyList<PhysicalNetworkAdapterInfo> GetActivePhysicalIPv4Adapters()
    {
        try
        {
            Dictionary<string, string> physicalAdapters = GetPhysicalAdapterNamesByGuid();
            var result = new List<PhysicalNetworkAdapterInfo>();

            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus != OperationalStatus.Up)
                    continue;

                string id = NormalizeGuid(networkInterface.Id);
                if (!physicalAdapters.TryGetValue(id, out string? connectionName))
                    continue;

                List<string> addresses = networkInterface
                    .GetIPProperties()
                    .UnicastAddresses
                    .Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(x => x.Address)
                    .Where(x => !IPAddress.IsLoopback(x) && !x.Equals(IPAddress.Any))
                    .Select(x => x.ToString())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (addresses.Count == 0)
                    continue;

                string displayName = !string.IsNullOrWhiteSpace(connectionName)
                    ? connectionName
                    : networkInterface.Name;

                result.Add(new PhysicalNetworkAdapterInfo(displayName, addresses));
            }

            return result
                .OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }
        catch (Exception ex)
        {
            AppLog.Write(ex.ToString());
            return [];
        }
    }

    private static Dictionary<string, string> GetPhysicalAdapterNamesByGuid()
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var scope = new ManagementScope(@"\\.\root\StandardCimv2");
        scope.Connect();

        using var searcher = new ManagementObjectSearcher(
            scope,
            new ObjectQuery(
                "SELECT InterfaceGuid, Name FROM MSFT_NetAdapter " +
                "WHERE HardwareInterface = TRUE " +
                "AND InterfaceOperationalStatus = 1 " +
                "AND MediaConnectState = 1"));

        using ManagementObjectCollection adapters = searcher.Get();

        foreach (ManagementObject adapter in adapters)
        {
            string? guid = adapter["InterfaceGuid"]?.ToString();
            if (string.IsNullOrWhiteSpace(guid))
                continue;

            string? adapterName = adapter["Name"]?.ToString();
            result[NormalizeGuid(guid)] =
                string.IsNullOrWhiteSpace(adapterName)
                    ? "Сетевой интерфейс"
                    : adapterName;
        }

        return result;
    }

    private static string NormalizeGuid(string value) =>
        value.Trim().Trim('{', '}').ToUpperInvariant();
}
