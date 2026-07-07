namespace Financiero.Application;

public sealed class FinancialConfigurationReader(IPortalConfigurationClient configuration) : IFinancialConfigurationReader
{
    public async Task<bool> GetBoolAsync(string key, bool defaultValue, PortalCallContext context, CancellationToken ct)
    {
        var value = await configuration.GetAsync(key, context, ct);
        return bool.TryParse(value, out var parsed) ? parsed : defaultValue;
    }

    public async Task<int> GetIntAsync(string key, int defaultValue, PortalCallContext context, CancellationToken ct)
    {
        var value = await configuration.GetAsync(key, context, ct);
        return int.TryParse(value, out var parsed) ? parsed : defaultValue;
    }

    public async Task<string> GetStringAsync(string key, string defaultValue, PortalCallContext context, CancellationToken ct)
    {
        var value = await configuration.GetAsync(key, context, ct);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
    }
}

public sealed class StaticFinancialConfigurationReader(IReadOnlyDictionary<string, string>? values = null) : IFinancialConfigurationReader
{
    private readonly IReadOnlyDictionary<string, string> _values = values ?? new Dictionary<string, string>();
    public Task<bool> GetBoolAsync(string key, bool defaultValue, PortalCallContext context, CancellationToken ct) =>
        Task.FromResult(_values.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed) ? parsed : defaultValue);
    public Task<int> GetIntAsync(string key, int defaultValue, PortalCallContext context, CancellationToken ct) =>
        Task.FromResult(_values.TryGetValue(key, out var value) && int.TryParse(value, out var parsed) ? parsed : defaultValue);
    public Task<string> GetStringAsync(string key, string defaultValue, PortalCallContext context, CancellationToken ct) =>
        Task.FromResult(_values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value.Trim() : defaultValue);
}
