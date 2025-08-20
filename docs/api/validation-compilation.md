# Validation and Compilation

This document covers the comprehensive validation and compilation system in SemanticKernel.Graph, including workflow validation, type inference, state validation, and merge conflict resolution. The system provides robust validation at multiple levels, from compile-time schema checking to runtime state integrity verification, ensuring reliable graph execution and data consistency.

## WorkflowValidator

The `WorkflowValidator` validates multi-agent workflows for structural integrity, capability compatibility, and resource configuration sanity before execution, providing comprehensive validation with detailed error reporting.

### Overview

This validator performs non-throwing checks and aggregates issues into a `ValidationResult`, enabling early detection of problems such as invalid dependencies, missing capabilities, and configuration issues. It supports validation of workflow structure, agent capabilities, capacity constraints, and resource governance settings.

### Key Features

* **Structural Validation**: ID uniqueness, dependency integrity, and cycle detection
* **Agent Capability Validation**: Ensures required capabilities are available
* **Capacity Validation**: Validates agent capacity configurations
* **Resource Governance Validation**: Checks resource configuration sanity
* **Comprehensive Reporting**: Aggregates errors, warnings, and recommendations
* **Non-Throwing Design**: Safe validation without exceptions

### Configuration

```csharp
var validator = new WorkflowValidator(logger);

// Validate workflow with full context
var result = validator.Validate(
    workflow: workflow,
    agentRoles: agentRoles,
    agentCapacities: agentCapacities,
    resourceOptions: resourceOptions
);
```

### Workflow Validation

```csharp
// Create a workflow to validate
var workflow = new MultiAgentWorkflow
{
    Id = "document-analysis-001",
    Name = "Document Analysis Pipeline",
    RequiredAgents = new List<string> { "analysis-agent", "processing-agent" },
    Tasks = new List<WorkflowTask>
    {
        new WorkflowTask
        {
            Id = "extract-text",
            Name = "Text Extraction",
            AgentId = "analysis-agent",
            RequiredCapabilities = { "text_extraction", "ocr" },
            DependsOn = new List<string>()
        },
        new WorkflowTask
        {
            Id = "process-content",
            Name = "Content Processing",
            AgentId = "processing-agent",
            RequiredCapabilities = { "text_processing", "nlp" },
            DependsOn = new List<string> { "extract-text" }
        }
    }
};

// Validate the workflow
var validationResult = validator.Validate(workflow);

if (validationResult.IsValid)
{
    Console.WriteLine("Workflow validation passed");
}
else
{
    Console.WriteLine($"Validation failed with {validationResult.ErrorCount} errors:");
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"  Error: {error.Message} (Property: {error.PropertyName})");
    }
    
    if (validationResult.HasWarnings)
    {
        Console.WriteLine($"Warnings: {validationResult.WarningCount}");
        foreach (var warning in validationResult.Warnings)
        {
            Console.WriteLine($"  Warning: {warning.Message}");
        }
    }
}
```

### Agent Capability Validation

```csharp
// Define agent roles with capabilities
var agentRoles = new Dictionary<string, AgentRole>
{
    ["analysis-agent"] = new AgentRole
    {
        Name = "Document Analyst",
        Capabilities = { "text_extraction", "ocr", "image_analysis" }
    },
    ["processing-agent"] = new AgentRole
    {
        Name = "Content Processor",
        Capabilities = { "text_processing", "nlp", "classification" }
    }
};

// Validate with agent roles
var result = validator.Validate(workflow, agentRoles: agentRoles);

// Check capability compatibility
if (!result.IsValid)
{
    var capabilityErrors = result.Errors
        .Where(e => e.Message.Contains("capabilities"))
        .ToList();
    
    foreach (var error in capabilityErrors)
    {
        Console.WriteLine($"Capability error: {error.Message}");
    }
}
```

### Capacity and Resource Validation

```csharp
// Define agent capacities
var agentCapacities = new Dictionary<string, AgentCapacity>
{
    ["analysis-agent"] = new AgentCapacity
    {
        AgentId = "analysis-agent",
        MaxCapacity = 5,
        CurrentLoad = 2
    },
    ["processing-agent"] = new AgentCapacity
    {
        AgentId = "processing-agent",
        MaxCapacity = 3,
        CurrentLoad = 1
    }
};

// Define resource governance options
var resourceOptions = new GraphResourceOptions
{
    EnableResourceGovernance = true,
    CpuHighWatermarkPercent = 80,
    CpuSoftLimitPercent = 70,
    MinAvailableMemoryMB = 512,
    BasePermitsPerSecond = 10,
    MaxBurstSize = 20
};

// Validate with full context
var result = validator.Validate(
    workflow, 
    agentRoles, 
    agentCapacities, 
    resourceOptions
);
```

## GraphTypeInferenceEngine

The `GraphTypeInferenceEngine` performs lightweight type inference over graphs using available typed schemas, propagating known output types from source nodes to targets that don't declare typed schemas.

### Overview

This static engine provides conservative, name-based type inference that examines upstream nodes to determine input requirements for untyped target nodes. It supports both primitive types and .NET runtime types, enabling gradual adoption of typed schemas across graph components.

### Key Features

* **Conservative Inference**: Only infers types when confident about relationships
* **Name-based Propagation**: Maps parameters by name across node boundaries
* **Upstream Analysis**: Examines predecessor nodes for type information
* **Fallback Handling**: Provides untyped fallbacks when type information unavailable
* **Static Analysis**: Compile-time type inference without runtime overhead

### Type Inference

```csharp
var graph = new GraphExecutor("type-inference-example");

// Add typed source node with known output schema
var sourceNode = new TypedSourceNode();
graph.AddNode(sourceNode);

// Add untyped target node
var targetNode = new UntypedTargetNode();
graph.AddNode(targetNode);

// Connect nodes
graph.Connect(sourceNode.NodeId, targetNode.NodeId);

// Infer input schemas for untyped nodes
var inferredSchemas = GraphTypeInferenceEngine.InferInputSchemas(graph);

// Check inferred schema for target node
if (inferredSchemas.TryGetValue(targetNode.NodeId, out var targetSchema))
{
    Console.WriteLine($"Target node input schema:");
    foreach (var input in targetSchema.Inputs)
    {
        Console.WriteLine($"  {input.Key}: {input.Value.Type} (Required: {input.Value.Required})");
        Console.WriteLine($"    Description: {input.Value.Description}");
    }
}
```

### Schema Propagation

```csharp
// Create a graph with multiple typed nodes
var graph = new GraphExecutor("schema-propagation");

// Source node with typed outputs
var dataSource = new DataSourceNode();
graph.AddNode(dataSource);

// Intermediate processing node
var processor = new DataProcessorNode();
graph.AddNode(processor);

// Final output node
var output = new OutputNode();
graph.AddNode(output);

// Connect the pipeline
graph.Connect(dataSource.NodeId, processor.NodeId);
graph.Connect(processor.NodeId, output.NodeId);

// Infer schemas throughout the pipeline
var inferredSchemas = GraphTypeInferenceEngine.InferInputSchemas(graph);

// Check schema propagation
foreach (var kvp in inferredSchemas)
{
    Console.WriteLine($"Node {kvp.Key} inferred schema:");
    foreach (var input in kvp.Value.Inputs)
    {
        Console.WriteLine($"  Input: {input.Key} -> {input.Value.Type}");
    }
}
```

## StateValidator

The `StateValidator` provides comprehensive integrity checks for graph states, ensuring data consistency and identifying potential issues early in the execution lifecycle.

### Overview

This static validator performs multiple validation categories including basic properties, data validation, execution history, version compatibility, and size constraints. It provides both comprehensive validation and critical property validation for performance-sensitive scenarios.

### Key Features

* **Comprehensive Validation**: Full state integrity checking
* **Critical Property Validation**: Fast validation of essential properties
* **Size and Memory Validation**: Prevents excessive resource usage
* **History Validation**: Ensures execution history integrity
* **Version Compatibility**: Checks state version requirements

### State Validation

```csharp
var state = new GraphState("validation-test");

// Add data to the state
state.SetValue("user_id", 123);
state.SetValue("user_name", "John Doe");
state.SetValue("settings", new Dictionary<string, object>
{
    ["theme"] = "dark",
    ["language"] = "en"
});

// Comprehensive validation
var validationResult = StateValidator.ValidateState(state);

if (validationResult.IsValid)
{
    Console.WriteLine("State validation passed");
}
else
{
    Console.WriteLine($"State validation failed with {validationResult.ErrorCount} errors:");
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"  Error: {error.Message}");
    }
    
    if (validationResult.HasWarnings)
    {
        Console.WriteLine($"Warnings: {validationResult.WarningCount}");
        foreach (var warning in validationResult.Warnings)
        {
            Console.WriteLine($"  Warning: {warning.Message}");
        }
    }
}
```

### Critical Property Validation

```csharp
// Fast validation of critical properties only
var isCriticalValid = StateValidator.ValidateCriticalProperties(state);

if (isCriticalValid)
{
    Console.WriteLine("Critical state properties are valid");
}
else
{
    Console.WriteLine("Critical state properties validation failed");
}
```

### Validation Categories

```csharp
// Validate specific aspects of the state
var state = new GraphState("detailed-validation");

// Basic properties validation
if (string.IsNullOrWhiteSpace(state.StateId))
{
    Console.WriteLine("State ID is missing");
}

// Data validation
foreach (var kvp in state.Parameters)
{
    var key = kvp.Key;
    var value = kvp.Value;
    
    // Check parameter name length
    if (key.Length > 256)
    {
        Console.WriteLine($"Parameter name too long: {key}");
    }
    
    // Check string value length
    if (value is string strValue && strValue.Length > 1024 * 1024)
    {
        Console.WriteLine($"String value too long for parameter: {key}");
    }
}

// Size validation
var stateSize = state.EstimateSize();
if (stateSize > 10 * 1024 * 1024) // 10MB limit
{
    Console.WriteLine($"State size exceeds limit: {stateSize} bytes");
}

// History validation
if (state.ExecutionHistory.Count > 10000)
{
    Console.WriteLine("Execution history too large");
}
```

## StateMergeConflictPolicy

The `StateMergeConflictPolicy` defines conflict resolution strategies for state merges during parallel branch joins, providing flexible and configurable merge behavior.

### Overview

This enum and configuration system supports multiple merge strategies from simple precedence rules to complex CRDT-like semantics. It enables fine-grained control over how conflicting state values are resolved during graph execution, particularly in parallel execution scenarios.

### Merge Policies

```csharp
// Available merge conflict policies
public enum StateMergeConflictPolicy
{
    PreferFirst = 0,        // Use value from base state
    PreferSecond = 1,       // Use value from overlay state
    ThrowOnConflict = 2,    // Throw exception on conflict
    LastWriteWins = 3,      // Use timestamp-based resolution
    FirstWriteWins = 4,     // Use first write timestamp
    Reduce = 5,             // Apply reduce function
    CrdtLike = 6,           // CRDT-like merge semantics
    Custom = 7              // Custom merge function
}
```

### Basic State Merging

```csharp
var baseState = new GraphState("base");
baseState.SetValue("count", 5);
baseState.SetValue("settings", new Dictionary<string, object>
{
    ["theme"] = "light",
    ["language"] = "en"
});

var overlayState = new GraphState("overlay");
overlayState.SetValue("count", 10);
overlayState.SetValue("settings", new Dictionary<string, object>
{
    ["theme"] = "dark",
    ["language"] = "pt"
});

// Simple merge with policy
var mergedState = StateHelpers.MergeStates(
    baseState, 
    overlayState, 
    StateMergeConflictPolicy.PreferSecond
);

Console.WriteLine($"Count: {mergedState.GetValue<int>("count")}"); // 10 (overlay)
Console.WriteLine($"Theme: {mergedState.GetValue<Dictionary<string, object>>("settings")["theme"]}"); // "dark"
```

### Advanced Merge Configuration

```csharp
// Create merge configuration with specific policies
var config = new StateMergeConfiguration
{
    DefaultPolicy = StateMergeConflictPolicy.PreferSecond
};

// Configure specific policies per key
config.SetKeyPolicy("count", StateMergeConflictPolicy.Reduce);
config.SetKeyPolicy("settings", StateMergeConflictPolicy.Reduce);

// Configure policies per type
config.SetTypePolicy(typeof(Dictionary<string, object>), StateMergeConflictPolicy.Reduce);
config.SetTypePolicy(typeof(int), StateMergeConflictPolicy.Reduce);

// Custom merge functions
config.SetCustomKeyMerger("count", (baseVal, overlayVal) =>
{
    if (baseVal is int baseInt && overlayVal is int overlayInt)
    {
        return baseInt + overlayInt; // Sum the values
    }
    return overlayVal;
});

config.SetCustomKeyMerger("settings", (baseVal, overlayVal) =>
{
    if (baseVal is Dictionary<string, object> baseDict && 
        overlayVal is Dictionary<string, object> overlayDict)
    {
        var merged = new Dictionary<string, object>(baseDict);
        foreach (var kvp in overlayDict)
        {
            merged[kvp.Key] = kvp.Value;
        }
        return merged;
    }
    return overlayVal;
});

// Perform advanced merge
var advancedMergedState = StateHelpers.MergeStates(baseState, overlayState, config);

Console.WriteLine($"Count: {advancedMergedState.GetValue<int>("count")}"); // 15 (5 + 10)
var settings = advancedMergedState.GetValue<Dictionary<string, object>>("settings");
Console.WriteLine($"Theme: {settings["theme"]}"); // "dark"
Console.WriteLine($"Language: {settings["language"]}"); // "pt"
```

### Reduce Functions

```csharp
// Configure reduce functions for numeric types
config.SetReduceFunction(typeof(int), (baseVal, overlayVal) =>
{
    if (baseVal is int baseInt && overlayVal is int overlayInt)
    {
        return Math.Max(baseInt, overlayVal); // Take maximum
    }
    return overlayVal;
});

config.SetReduceFunction(typeof(double), (baseVal, overlayVal) =>
{
    if (baseVal is double baseDouble && overlayVal is double overlayDouble)
    {
        return (baseDouble + overlayDouble) / 2; // Average
    }
    return overlayVal;
});

// Configure reduce functions for collections
config.SetReduceFunction(typeof(List<string>), (baseVal, overlayVal) =>
{
    if (baseVal is List<string> baseList && overlayVal is List<string> overlayList)
    {
        var merged = new List<string>(baseList);
        foreach (var item in overlayList)
        {
            if (!merged.Contains(item))
            {
                merged.Add(item);
            }
        }
        return merged;
    }
    return overlayVal;
});
```

### Conflict Detection and Resolution

```csharp
// Perform merge with conflict detection
var mergeResult = StateHelpers.MergeStatesWithConflicts(baseState, overlayState, config);

if (mergeResult.HasConflicts)
{
    Console.WriteLine($"Merge completed with {mergeResult.Conflicts.Count} conflicts:");
    
    foreach (var conflict in mergeResult.Conflicts)
    {
        Console.WriteLine($"  Conflict on '{conflict.Key}':");
        Console.WriteLine($"    Base value: {conflict.BaseValue}");
        Console.WriteLine($"    Overlay value: {conflict.OverlayValue}");
        Console.WriteLine($"    Policy: {conflict.Policy}");
        Console.WriteLine($"    Resolved: {conflict.WasResolved}");
        
        if (conflict.ResolvedValue != null)
        {
            Console.WriteLine($"    Final value: {conflict.ResolvedValue}");
        }
    }
}
else
{
    Console.WriteLine("Merge completed without conflicts");
}

// Access the merged state
var finalState = mergeResult.MergedState;
```

## ValidationResult and ValidationIssue

The validation system provides comprehensive result objects that aggregate validation issues with detailed information about errors, warnings, and recommendations.

### ValidationResult Structure

```csharp
public sealed class ValidationResult
{
    public DateTimeOffset Timestamp { get; }
    public bool IsValid { get; }
    public bool HasErrors { get; }
    public bool HasWarnings { get; }
    public IReadOnlyList<ValidationIssue> Issues { get; }
    public IReadOnlyList<ValidationIssue> Errors { get; }
    public IReadOnlyList<ValidationIssue> Warnings { get; }
    public int TotalIssues { get; }
    public int ErrorCount { get; }
    public int WarningCount { get; }
    
    public void AddError(string message, string? propertyName = null);
    public void AddWarning(string message, string? propertyName = null);
    public void Merge(ValidationResult other);
    public string CreateSummary(bool includeWarnings = true);
    public void ThrowIfInvalid();
}
```

### ValidationIssue Structure

```csharp
public sealed class ValidationIssue
{
    public ValidationSeverity Severity { get; }
    public string Message { get; }
    public string? PropertyName { get; }
    public string? Code { get; }
    public Dictionary<string, object> Metadata { get; }
    
    public ValidationIssue(ValidationSeverity severity, string message, 
        string? propertyName = null, string? code = null);
}

public enum ValidationSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2,
    Critical = 3
}
```

### Working with Validation Results

```csharp
// Create validation result
var result = new ValidationResult();

// Add validation issues
result.AddError("Required field 'name' is missing", "name");
result.AddWarning("Field 'age' should be positive", "age");
result.AddError("Invalid email format", "email");

// Check validation status
if (result.IsValid)
{
    Console.WriteLine("Validation passed");
}
else
{
    Console.WriteLine($"Validation failed: {result.ErrorCount} errors, {result.WarningCount} warnings");
    
    // Process errors
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Error: {error.Message}");
        if (!string.IsNullOrEmpty(error.PropertyName))
        {
            Console.WriteLine($"  Property: {error.PropertyName}");
        }
    }
    
    // Process warnings
    foreach (var warning in result.Warnings)
    {
        Console.WriteLine($"Warning: {warning.Message}");
    }
}

// Create summary
var summary = result.CreateSummary(includeWarnings: true);
Console.WriteLine(summary);

// Merge with another result
var otherResult = new ValidationResult();
otherResult.AddError("Additional error");
result.Merge(otherResult);

// Throw if invalid (useful for validation chains)
try
{
    result.ThrowIfInvalid();
}
catch (ValidationException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```

## See Also

* [Schema Typing and Validation Guide](../how-to/schema-typing-and-validation.md) - Comprehensive guide to implementing schema validation
* [State and Serialization Reference](state-and-serialization.md) - State management and merge operations
* [Multi-Agent Reference](multi-agent.md) - Workflow validation in multi-agent systems
* [Graph Executor Reference](graph-executor.md) - Graph integrity validation
* [Integration Reference](integration.md) - External system validation
* [Validation Examples](../examples/validation-examples.md) - Practical examples of validation implementations
