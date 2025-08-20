# Security and Data Handling

This guide explains how to implement comprehensive security measures and data handling policies in SemanticKernel.Graph, including data sanitization, sensitive key management, retention policies, and encryption capabilities.

## Overview

SemanticKernel.Graph provides enterprise-grade security features that ensure sensitive data protection, secure execution, and compliance with data governance requirements. The system includes:

* **Data Sanitization**: Automatic redaction of sensitive information in logs, events, and exports
* **Authentication & Authorization**: Bearer token validation and scope-based access control
* **Encryption**: Support for encrypted checkpoints and secure data transmission
* **Retention Policies**: Configurable data lifecycle management and automatic cleanup
* **Multi-tenant Isolation**: Secure boundaries between different tenant contexts

## Data Sanitization

### Core Sanitization Components

The sanitization system consists of several key components:

* **`SensitiveDataSanitizer`**: Main utility for sanitizing objects and dictionaries
* **`SensitiveDataPolicy`**: Configuration policy defining sanitization behavior
* **`SanitizationLevel`**: Enumeration controlling sanitization aggressiveness

### Sanitization Levels

```csharp
public enum SanitizationLevel
{
    None = 0,      // No sanitization applied
    Basic = 1,     // Redact only when key suggests sensitivity
    Strict = 2     // Redact all string values regardless of key
}
```

### Basic Sanitization Configuration

Configure sanitization with default sensitive key patterns:

```csharp
using SemanticKernel.Graph.Integration;

var policy = new SensitiveDataPolicy
{
    Enabled = true,
    Level = SanitizationLevel.Basic,
    RedactionText = "***REDACTED***",
    MaskAuthorizationBearerToken = true
};

var sanitizer = new SensitiveDataSanitizer(policy);
```

### Custom Sensitive Key Patterns

Define custom patterns for your domain-specific sensitive data:

```csharp
var customPolicy = new SensitiveDataPolicy
{
    Enabled = true,
    Level = SanitizationLevel.Basic,
    RedactionText = "[SENSITIVE]",
    SensitiveKeySubstrings = new[]
    {
        // Standard patterns
        "password", "secret", "token", "key", "auth",
        
        // Domain-specific patterns
        "ssn", "credit_card", "bank_account", "social_security",
        "medical_record", "patient_id", "diagnosis",
        
        // Custom business patterns
        "internal_note", "confidential", "restricted"
    },
    MaskAuthorizationBearerToken = true
};

var sanitizer = new SensitiveDataSanitizer(customPolicy);
```

### Sanitizing Different Data Types

The sanitizer handles various data structures automatically:

```csharp
// Sanitize dictionary data
var sensitiveData = new Dictionary<string, object?>
{
    ["username"] = "john_doe",
    ["password"] = "secret123",
    ["api_key"] = "sk-1234567890abcdef",
    ["authorization"] = "Bearer sk-1234567890abcdef",
    ["connection_string"] = "Server=localhost;Database=test;Password=secret;",
    ["normal_data"] = "This is not sensitive"
};

var sanitizedData = sanitizer.Sanitize(sensitiveData);

// Output: sensitive values are redacted
foreach (var kvp in sanitizedData)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}
// username: john_doe
// password: ***REDACTED***
// api_key: ***REDACTED***
// authorization: Bearer ***REDACTED***
// connection_string: ***REDACTED***
// normal_data: This is not sensitive
```

### Integration with Logging

Apply sanitization to logging output to prevent sensitive data exposure:

```csharp
// Configure logging with sanitization
var loggingOptions = new GraphLoggingOptions
{
    LogSensitiveData = false,
    Sanitization = new SensitiveDataPolicy
    {
        Enabled = true,
        Level = SanitizationLevel.Basic,
        RedactionText = "[REDACTED]"
    }
};

// Apply to kernel builder
builder.AddGraphSupport(opts =>
{
    opts.EnableLogging = true;
    opts.Logging = loggingOptions;
});
```

## Authentication and Authorization

### Bearer Token Validation

SemanticKernel.Graph provides flexible bearer token validation through the `IBearerTokenValidator` interface:

```csharp
using SemanticKernel.Graph.Integration;

// Azure AD JWT validation
var validator = new AzureAdBearerTokenValidator();

// Validate token with required scopes
var isValid = await validator.ValidateAsync(
    bearerToken: "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1Ni...",
    requiredScopes: new[] { "graph.read", "graph.write" },
    requiredAppRoles: new[] { "GraphUser", "GraphAdmin" }
);
```

### Custom Token Validators

Implement custom validation logic for your authentication system:

```csharp
public sealed class CustomBearerTokenValidator : IBearerTokenValidator
{
    public async Task<bool> ValidateAsync(string bearerToken, 
        IEnumerable<string>? requiredScopes = null, 
        IEnumerable<string>? requiredAppRoles = null, 
        CancellationToken cancellationToken = default)
    {
        // Implement your custom validation logic
        // e.g., validate against your identity provider
        
        if (string.IsNullOrWhiteSpace(bearerToken))
            return false;
            
        try
        {
            // Parse and validate token
            // Check scopes and roles
            // Verify expiration and other claims
            
            return true; // or false based on validation
        }
        catch
        {
            return false;
        }
    }
}
```

### REST API Security

Configure security for the Graph REST API:

```csharp
var restApiOptions = new GraphRestApiOptions
{
    RequireAuthentication = true,
    EnableBearerTokenAuth = true,
    RequiredScopes = new[] { "graph.execute", "graph.read" },
    RequiredAppRoles = new[] { "GraphUser" },
    ApiKey = null, // Use bearer token instead
    RateLimitRequestsPerMinute = 100
};

// Register in DI container
builder.Services.AddSingleton(restApiOptions);
builder.Services.AddSingleton<IBearerTokenValidator, AzureAdBearerTokenValidator>();
```

## Encryption and Secure Storage

### Checkpoint Encryption

Enable encryption for sensitive checkpoint data:

```csharp
var checkpointOptions = new CheckpointOptions
{
    EnableCompression = true,
    EnableEncryption = true,
    EncryptionKey = "your-encryption-key", // Use secure key management
    MaxCacheSize = 1000,
    EnableAutoCleanup = true
};

// Configure with backup encryption
var backupOptions = new CheckpointBackupOptions
{
    CompressBackup = true,
    EncryptBackup = true,
    ReplicationFactor = 3,
    VerifyAfterBackup = true
};

checkpointOptions.DefaultBackupOptions = backupOptions;
```

### Secure Key Management

Implement secure key resolution through the `ISecretResolver` interface:

```csharp
public sealed class AzureKeyVaultSecretResolver : ISecretResolver
{
    private readonly SecretClient _secretClient;
    
    public AzureKeyVaultSecretResolver(SecretClient secretClient)
    {
        _secretClient = secretClient;
    }
    
    public async Task<string?> ResolveAsync(string secretName, CancellationToken cancellationToken = default)
    {
        try
        {
            var secret = await _secretClient.GetSecretAsync(secretName, cancellationToken: cancellationToken);
            return secret.Value.Value;
        }
        catch
        {
            return null;
        }
    }
}

// Register in DI container
builder.Services.AddSingleton<ISecretResolver, AzureKeyVaultSecretResolver>();
```

### Memory Encryption

Encrypt sensitive data in memory when required:

```csharp
var memoryOptions = new GraphMemoryOptions
{
    EnableVectorSearch = true,
    EnableSemanticSearch = true,
    EnableEncryption = true,
    EncryptionKey = "memory-encryption-key",
    DefaultCollectionName = "encrypted-graph-memory"
};
```

## Data Retention and Lifecycle Management

### Checkpoint Retention Policies

Configure automatic cleanup and retention policies:

```csharp
var retentionPolicy = new CheckpointRetentionPolicy
{
    MaxAge = TimeSpan.FromDays(30),           // Keep for 30 days
    MaxCheckpointsPerExecution = 100,         // Max 100 per execution
    MaxTotalStorageBytes = 5L * 1024 * 1024 * 1024,  // 5GB total storage
    KeepCriticalCheckpoints = true,           // Always keep critical checkpoints
    CriticalCheckpointInterval = 10           // Critical checkpoints every 10 regular ones
};

var checkpointOptions = new CheckpointOptions
{
    DefaultRetentionPolicy = retentionPolicy,
    EnableAutoCleanup = true,
    AutoCleanupInterval = TimeSpan.FromHours(6)
};
```

### Log Retention and Cleanup

Configure log retention and automatic cleanup:

```csharp
var loggingOptions = new GraphLoggingOptions
{
    LogRetentionPeriod = TimeSpan.FromDays(90),
    EnableLogRotation = true,
    MaxLogFileSize = 100 * 1024 * 1024, // 100MB
    MaxLogFiles = 10,
    EnableCompression = true,
    EnableEncryption = false // Enable for sensitive logs
};
```

### Memory Cleanup Policies

Configure memory cleanup and retention:

```csharp
var memoryCleanupOptions = new MemoryCleanupOptions
{
    CleanupInterval = TimeSpan.FromHours(12),
    MaxMemoryAge = TimeSpan.FromDays(7),
    MaxMemoryEntries = 10000,
    EnableCompression = true,
    EnableEncryption = false
};
```

## Multi-Tenant Isolation

### Tenant Context Management

Implement tenant isolation for multi-tenant applications:

```csharp
public sealed class TenantContext
{
    public string TenantId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> Claims { get; init; } = new Dictionary<string, string>();
    public TimeSpan SessionTimeout { get; init; } = TimeSpan.FromHours(8);
}

// Tenant-aware graph execution
public sealed class TenantAwareGraphExecutor
{
    private readonly GraphExecutor _baseExecutor;
    private readonly TenantContext _tenantContext;
    
    public async Task<GraphExecutionResult> ExecuteAsync(
        KernelArguments arguments, 
        CancellationToken cancellationToken = default)
    {
        // Inject tenant context into arguments
        arguments["tenant_id"] = _tenantContext.TenantId;
        arguments["user_id"] = _tenantContext.UserId;
        
        // Execute with tenant isolation
        return await _baseExecutor.ExecuteAsync(arguments, cancellationToken);
    }
}
```

### Subgraph Isolation

Use subgraph isolation modes for secure execution boundaries:

```csharp
var subgraphConfig = new SubgraphConfiguration
{
    IsolationMode = SubgraphIsolationMode.IsolatedClone,
    ScopedPrefix = "tenant_data",
    InputMappings = new Dictionary<string, string>
    {
        ["user_input"] = "input",
        ["tenant_context"] = "context"
    },
    OutputMappings = new Dictionary<string, string>
    {
        ["result"] = "tenant_result",
        ["metadata"] = "tenant_metadata"
    }
};

var subgraphNode = new SubgraphGraphNode(subgraphExecutor, subgraphConfig);
```

### API-Level Tenant Isolation

Implement tenant isolation at the REST API level:

```csharp
var restApiOptions = new GraphRestApiOptions
{
    RequireAuthentication = true,
    EnableTenantIsolation = true,
    TenantHeaderName = "X-Tenant-ID",
    TenantValidationRequired = true,
    MaxTenantsPerRequest = 1
};

// Custom tenant validator
public sealed class TenantValidator : ITenantValidator
{
    public async Task<bool> ValidateTenantAsync(string tenantId, string userId, CancellationToken cancellationToken = default)
    {
        // Implement tenant validation logic
        // Check if user has access to specified tenant
        return await ValidateUserTenantAccessAsync(tenantId, userId, cancellationToken);
    }
}
```

## Audit and Compliance

### Execution Auditing

Enable comprehensive execution auditing:

```csharp
var auditOptions = new AuditOptions
{
    EnableExecutionAudit = true,
    EnableDataAccessAudit = true,
    EnableSecurityAudit = true,
    AuditRetentionPeriod = TimeSpan.FromDays(365),
    EnableAuditCompression = true,
    EnableAuditEncryption = true
};

// Configure audit logging
var auditLogger = new AuditLogger(auditOptions);
builder.Services.AddSingleton<IAuditLogger>(auditLogger);
```

### Compliance Reporting

Generate compliance reports for regulatory requirements:

```csharp
public sealed class ComplianceReporter
{
    public async Task<ComplianceReport> GenerateReportAsync(
        DateTimeOffset startDate, 
        DateTimeOffset endDate, 
        string tenantId, 
        CancellationToken cancellationToken = default)
    {
        var report = new ComplianceReport
        {
            Period = new DateRange(startDate, endDate),
            TenantId = tenantId,
            DataAccessLogs = await GetDataAccessLogsAsync(startDate, endDate, tenantId, cancellationToken),
            SecurityEvents = await GetSecurityEventsAsync(startDate, endDate, tenantId, cancellationToken),
            DataRetentionCompliance = await ValidateDataRetentionAsync(tenantId, cancellationToken)
        };
        
        return report;
    }
}
```

## Security Best Practices

### Configuration Security

1. **Environment-based Configuration**: Use environment variables for sensitive configuration
2. **Secret Rotation**: Implement automatic secret rotation for long-lived credentials
3. **Least Privilege**: Grant minimal required permissions to each component
4. **Network Security**: Use HTTPS/TLS for all external communications

### Data Protection

1. **Encryption at Rest**: Encrypt sensitive data in persistent storage
2. **Encryption in Transit**: Use TLS for all data transmission
3. **Key Management**: Use secure key management services (Azure Key Vault, AWS KMS)
4. **Data Classification**: Classify data by sensitivity and apply appropriate protection

### Access Control

1. **Multi-Factor Authentication**: Require MFA for administrative access
2. **Role-Based Access Control**: Implement RBAC for fine-grained permissions
3. **Session Management**: Implement proper session timeouts and management
4. **Audit Logging**: Log all security-relevant events for compliance

### Monitoring and Alerting

1. **Security Monitoring**: Monitor for suspicious activities and security events
2. **Alerting**: Set up alerts for security violations and anomalies
3. **Incident Response**: Have procedures for responding to security incidents
4. **Regular Reviews**: Conduct regular security reviews and penetration testing

## Troubleshooting

### Common Security Issues

**Authentication Failures**
* Verify bearer token format and expiration
* Check required scopes and app roles
* Validate token issuer and audience

**Data Sanitization Problems**
* Ensure sensitive key patterns are correctly configured
* Check sanitization level settings
* Verify custom helper functions don't bypass sanitization

**Encryption Issues**
* Validate encryption key format and length
* Check key rotation schedules
* Verify encryption algorithm compatibility

**Tenant Isolation Problems**
* Confirm tenant context is properly injected
* Check subgraph isolation mode configuration
* Verify tenant validation logic

### Debugging Security

1. **Enable Security Logging**: Use detailed security logging for troubleshooting
2. **Token Inspection**: Use JWT debuggers to inspect token contents
3. **Policy Validation**: Test sanitization policies with sample data
4. **Isolation Testing**: Verify tenant isolation with multi-tenant test scenarios

## See Also

* [Integration and Extensions](../how-to/integration-and-extensions.md) - Core integration patterns and extensions
* [Checkpointing and Recovery](../concepts/checkpointing.md) - State persistence and recovery
* [Multi-Agent Workflows](../how-to/multi-agent-and-shared-state.md) - Secure multi-agent coordination
* [REST API Integration](../how-to/exposing-rest-apis.md) - Secure API endpoints
* [API Reference](../api/) - Complete API documentation for security types
