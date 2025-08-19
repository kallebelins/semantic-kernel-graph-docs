# Subgraph Examples

This example demonstrates subgraph composition capabilities in Semantic Kernel Graph, showing different isolation modes and input/output mappings.

## Objective

Learn how to implement subgraph composition in graph-based workflows to:
- Create reusable subgraphs with isolated execution contexts
- Implement input/output mapping between parent and child graphs
- Use different isolation modes (IsolatedClone, ScopedPrefix)
- Handle state merging and conflict resolution
- Build modular and composable graph architectures

## Prerequisites

- **.NET 8.0** or later
- **OpenAI API Key** configured in `appsettings.json`
- **Semantic Kernel Graph package** installed
- Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [Subgraph Composition](../concepts/subgraphs.md)
- Familiarity with [State Management](../concepts/state.md)

## Key Components

### Concepts and Techniques

- **Subgraph Composition**: Building complex graphs from simpler, reusable components
- **Isolation Modes**: Different strategies for isolating subgraph execution contexts
- **Input/Output Mapping**: Transforming data between parent and child graph contexts
- **State Merging**: Combining execution results from subgraphs with parent state
- **Conflict Resolution**: Handling state conflicts during subgraph execution

### Core Classes

- `GraphExecutor`: Base executor for both parent and child graphs
- `SubgraphGraphNode`: Node that executes subgraphs with configuration
- `SubgraphConfiguration`: Configuration for subgraph behavior and mappings
- `FunctionGraphNode`: Nodes within subgraphs for specific functionality
- `SubgraphIsolationMode`: Enumeration of isolation strategies

## Running the Example

### Getting Started

This example demonstrates subgraph composition and isolation with the Semantic Kernel Graph package. The code snippets below show you how to implement this pattern in your own applications.

## Step-by-Step Implementation

### 1. IsolatedClone Subgraph Example

The first example demonstrates subgraph execution with complete isolation and explicit mappings.

```csharp
public static async Task RunIsolatedCloneSubgraphExampleAsync(Kernel kernel)
{
    ArgumentNullException.ThrowIfNull(kernel);

    // 1) Define subgraph that calculates the sum of x and y
    var child = new GraphExecutor("Subgraph_Sum", "Calculates sum of x and y");

    var sumFunction = KernelFunctionFactory.CreateFromMethod(
        (KernelArguments args) =>
        {
            var x = args.TryGetValue("x", out var xv) && xv is IConvertible ? Convert.ToDouble(xv) : 0.0;
            var y = args.TryGetValue("y", out var yv) && yv is IConvertible ? Convert.ToDouble(yv) : 0.0;
            var sum = x + y;
            args["sum"] = sum;
            return sum.ToString("F2");
        },
        functionName: "compute_sum",
        description: "Sums x and y and stores in 'sum'"
    );

    var sumNode = new FunctionGraphNode(sumFunction, nodeId: "sum_node", description: "Calculates sum");
    // Optionally store the result using supported metadata
    sumNode.SetMetadata("StoreResultAs", "sum");

    child.AddNode(sumNode)
         .SetStartNode(sumNode.NodeId);

    // 2) Subgraph node in parent graph with isolation and mappings
    var config = new SubgraphConfiguration
    {
        IsolationMode = SubgraphIsolationMode.IsolatedClone,
        MergeConflictPolicy = SemanticKernel.Graph.State.StateMergeConflictPolicy.PreferSecond,
        InputMappings =
        {
            ["a"] = "x",
            ["b"] = "y"
        },
        OutputMappings =
        {
            ["sum"] = "total"
        }
    };

    var parent = new GraphExecutor("Parent_IsolatedClone", "Uses subgraph for summation");
    var subgraphNode = new SubgraphGraphNode(child, name: "Subgraph(Sum)", description: "Executes sum subgraph", config: config);

    // Final node to demonstrate continuation after subgraph
    var finalizeFunction = KernelFunctionFactory.CreateFromMethod(
        (KernelArguments args) =>
        {
            var total = args.TryGetValue("total", out var tv) ? tv : 0;
            return $"Total (mapped from subgraph): {total}";
        },
        functionName: "finalize",
        description: "Returns total"
    );
    var finalizeNode = new FunctionGraphNode(finalizeFunction, nodeId: "finalize_node", description: "Displays total");

    parent.AddNode(subgraphNode)
          .AddNode(finalizeNode)
          .SetStartNode(subgraphNode.NodeId)
          .Connect(subgraphNode.NodeId, finalizeNode.NodeId);

    // 3) Execute with initial state (a, b)
    var args = new KernelArguments
    {
        ["a"] = 3,
        ["b"] = 7
    };

    var result = await parent.ExecuteAsync(kernel, args, CancellationToken.None);

    Console.WriteLine("[IsolatedClone] expected total = 10");
    var totalOk = args.TryGetValue("total", out var totalVal);
    Console.WriteLine($"[IsolatedClone] obtained total = {(totalOk ? totalVal : "(not mapped)")}");
    Console.WriteLine($"[IsolatedClone] final message = {result.GetValue<object>()}");
}
```

### 2. ScopedPrefix Subgraph Example

The second example demonstrates subgraph execution with scoped prefix isolation.

```csharp
public static async Task RunScopedPrefixSubgraphExampleAsync(Kernel kernel)
{
    ArgumentNullException.ThrowIfNull(kernel);

    // 1) Define subgraph that processes data with internal state
    var child = new GraphExecutor("Subgraph_Processor", "Processes data with internal state");

    var processFunction = KernelFunctionFactory.CreateFromMethod(
        (KernelArguments args) =>
        {
            var input = args.TryGetValue("input", out var inv) ? inv?.ToString() ?? string.Empty : string.Empty;
            var processed = $"Processed: {input.ToUpperInvariant()}";
            
            // Store internal state with prefix
            args["internal_result"] = processed;
            args["internal_count"] = input.Length;
            
            return processed;
        },
        functionName: "process_data",
        description: "Processes input data and stores internal state"
    );

    var processNode = new FunctionGraphNode(processFunction, nodeId: "process_node", description: "Processes data");
    child.AddNode(processNode).SetStartNode(processNode.NodeId);

    // 2) Subgraph node with scoped prefix isolation
    var config = new SubgraphConfiguration
    {
        IsolationMode = SubgraphIsolationMode.ScopedPrefix,
        Prefix = "subgraph_",
        MergeConflictPolicy = SemanticKernel.Graph.State.StateMergeConflictPolicy.PreferFirst,
        InputMappings =
        {
            ["data"] = "input"
        },
        OutputMappings =
        {
            ["internal_result"] = "result",
            ["internal_count"] = "count"
        }
    };

    var parent = new GraphExecutor("Parent_ScopedPrefix", "Uses subgraph with scoped prefix");
    var subgraphNode = new SubgraphGraphNode(child, name: "Subgraph(Processor)", description: "Executes processing subgraph", config: config);

    // Node to demonstrate state merging
    var mergeFunction = KernelFunctionFactory.CreateFromMethod(
        (KernelArguments args) =>
        {
            var result = args.TryGetValue("result", out var rv) ? rv : "No result";
            var count = args.TryGetValue("count", out var cv) ? cv : 0;
            var originalData = args.TryGetValue("data", out var dv) ? dv : "No data";
            
            return $"Merged state: {result} (count: {count}, original: {originalData})";
        },
        functionName: "merge_state",
        description: "Demonstrates state merging"
    );
    var mergeNode = new FunctionGraphNode(mergeFunction, nodeId: "merge_node", description: "Merges state");

    parent.AddNode(subgraphNode)
          .AddNode(mergeNode)
          .SetStartNode(subgraphNode.NodeId)
          .Connect(subgraphNode.NodeId, mergeNode.NodeId);

    // 3) Execute with data input
    var args = new KernelArguments
    {
        ["data"] = "Hello World"
    };

    var result = await parent.ExecuteAsync(kernel, args, CancellationToken.None);

    Console.WriteLine("[ScopedPrefix] Processing 'Hello World'");
    Console.WriteLine($"[ScopedPrefix] Result: {args.TryGetValue("result", out var r) ? r : "Not found"}");
    Console.WriteLine($"[ScopedPrefix] Count: {args.TryGetValue("count", out var c) ? c : "Not found"}");
    Console.WriteLine($"[ScopedPrefix] Final message: {result.GetValue<object>()}");
}
```

### 3. Subgraph Configuration Options

The examples demonstrate various configuration options for subgraph behavior.

```csharp
// Comprehensive subgraph configuration
var advancedConfig = new SubgraphConfiguration
{
    // Isolation mode determines how the subgraph context is managed
    IsolationMode = SubgraphIsolationMode.IsolatedClone, // or ScopedPrefix
    
    // Prefix for scoped isolation (only used with ScopedPrefix mode)
    Prefix = "my_subgraph_",
    
    // How to handle state conflicts during merging
    MergeConflictPolicy = StateMergeConflictPolicy.PreferSecond, // or PreferFirst, Merge
    
    // Input mappings: parent state -> subgraph state
    InputMappings = new Dictionary<string, string>
    {
        ["parent_input"] = "subgraph_input",
        ["parent_config"] = "subgraph_config"
    },
    
    // Output mappings: subgraph state -> parent state
    OutputMappings = new Dictionary<string, string>
    {
        ["subgraph_result"] = "parent_result",
        ["subgraph_metadata"] = "parent_metadata"
    },
    
    // Additional configuration options
    EnableStateValidation = true,
    MaxExecutionTime = TimeSpan.FromMinutes(5),
    EnableCheckpointing = false
};
```

### 4. State Management and Isolation

The examples show how state is managed across different isolation modes.

```csharp
// State management in IsolatedClone mode
// - Parent state: { "a": 3, "b": 7 }
// - Mapped to subgraph: { "x": 3, "y": 7 }
// - Subgraph execution: { "x": 3, "y": 7, "sum": 10 }
// - Mapped back to parent: { "a": 3, "b": 7, "total": 10 }

// State management in ScopedPrefix mode
// - Parent state: { "data": "Hello World" }
// - Mapped to subgraph: { "input": "Hello World" }
// - Subgraph execution: { "input": "Hello World", "internal_result": "Processed: HELLO WORLD", "internal_count": 11 }
// - Merged with prefix: { "data": "Hello World", "subgraph_internal_result": "Processed: HELLO WORLD", "subgraph_internal_count": 11 }
// - Mapped outputs: { "data": "Hello World", "result": "Processed: HELLO WORLD", "count": 11 }
```

### 5. Error Handling and Validation

The examples include error handling for subgraph execution.

```csharp
// Error handling in subgraph execution
try
{
    var result = await parent.ExecuteAsync(kernel, args, CancellationToken.None);
    
    if (result.Success)
    {
        Console.WriteLine("✅ Subgraph execution completed successfully");
        Console.WriteLine($"Result: {result.GetValue<object>()}");
    }
    else
    {
        Console.WriteLine("❌ Subgraph execution failed");
        Console.WriteLine($"Error: {result.ErrorMessage}");
    }
}
catch (SubgraphExecutionException ex)
{
    Console.WriteLine($"🚨 Subgraph execution error: {ex.Message}");
    Console.WriteLine($"Subgraph: {ex.SubgraphName}");
    Console.WriteLine($"Node: {ex.NodeId}");
}
catch (StateMappingException ex)
{
    Console.WriteLine($"🔀 State mapping error: {ex.Message}");
    Console.WriteLine($"Mapping: {ex.MappingName}");
}
```

### 6. Advanced Subgraph Patterns

The examples demonstrate advanced patterns for subgraph composition.

```csharp
// Nested subgraphs
var nestedChild = new GraphExecutor("Nested_Child", "Nested subgraph example");
// ... configure nested child

var middleChild = new GraphExecutor("Middle_Child", "Middle level subgraph");
var nestedNode = new SubgraphGraphNode(nestedChild, "Nested", "Nested subgraph");
middleChild.AddNode(nestedNode);

var parent = new GraphExecutor("Parent", "Parent with nested subgraphs");
var middleNode = new SubgraphGraphNode(middleChild, "Middle", "Middle subgraph");
parent.AddNode(middleNode);

// Conditional subgraph execution
var conditionalConfig = new SubgraphConfiguration
{
    IsolationMode = SubgraphIsolationMode.IsolatedClone,
    ExecutionCondition = (state) => 
        state.TryGetValue("enable_subgraph", out var enable) && 
        enable is bool b && b
};

// Dynamic subgraph selection
var subgraphSelector = new FunctionGraphNode(
    KernelFunctionFactory.CreateFromMethod(
        (KernelArguments args) =>
        {
            var condition = args.TryGetValue("condition", out var c) ? c?.ToString() ?? string.Empty : string.Empty;
            return condition switch
            {
                "sum" => "Subgraph_Sum",
                "process" => "Subgraph_Processor",
                _ => "Subgraph_Default"
            };
        },
        "select_subgraph",
        "Selects appropriate subgraph based on condition"
    ),
    "selector",
    "Subgraph Selector"
);
```

## Expected Output

The examples produce comprehensive output showing:

- 🔢 IsolatedClone subgraph execution with explicit mappings
- 🔀 ScopedPrefix subgraph execution with automatic prefixing
- 📊 State transformation and mapping results
- 🔄 State merging and conflict resolution
- ✅ Complete subgraph workflow execution
- 📈 Modular graph composition capabilities

## Troubleshooting

### Common Issues

1. **State Mapping Failures**: Verify input/output mapping configurations
2. **Isolation Mode Errors**: Check isolation mode compatibility with use case
3. **State Merge Conflicts**: Configure appropriate conflict resolution policies
4. **Subgraph Execution Failures**: Monitor subgraph execution and error handling

### Debugging Tips

- Enable detailed logging for subgraph execution
- Verify state mapping configurations and transformations
- Monitor state isolation and merging behavior
- Check subgraph configuration and isolation mode settings

## See Also

- [Subgraph Composition](../concepts/subgraphs.md)
- [State Management](../concepts/state.md)
- [Graph Composition](../how-to/graph-composition.md)
- [State Mapping](../concepts/state-mapping.md)
- [Modular Architecture](../patterns/modular-architecture.md)
