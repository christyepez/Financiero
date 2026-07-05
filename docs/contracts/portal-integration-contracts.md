# Portal Integration Contracts

## Audit Publisher

```csharp
public interface IPortalAuditPublisher
{
    Task PublishAsync(PortalAuditEvent auditEvent, CancellationToken cancellationToken);
}
```

## Notification Publisher

```csharp
public interface IPortalNotificationPublisher
{
    Task PublishAsync(PortalNotificationRequest notification, CancellationToken cancellationToken);
}
```

## Menu Registration

```csharp
public interface IPortalMenuRegistrationService
{
    Task RegisterFinanceMenusAsync(CancellationToken cancellationToken);
}
```

## Permission Registration

```csharp
public interface IPortalPermissionRegistrationService
{
    Task RegisterFinancePermissionsAsync(CancellationToken cancellationToken);
}
```

## Content Storage

```csharp
public interface IPortalContentStorage
{
    Task<string> StoreAsync(DocumentStorageRequest request, CancellationToken cancellationToken);
    Task<Stream> GetAsync(string documentId, CancellationToken cancellationToken);
}
```

## Configuration Reader

```csharp
public interface IPortalConfigurationReader
{
    Task<T?> GetAsync<T>(string group, string key, CancellationToken cancellationToken);
}
```

## Integration Registry

```csharp
public interface IPortalIntegrationRegistry
{
    Task RegisterConnectorAsync(ConnectorRegistration connector, CancellationToken cancellationToken);
}
```
