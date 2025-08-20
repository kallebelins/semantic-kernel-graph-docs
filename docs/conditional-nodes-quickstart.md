# Conditional Nodes Quickstart

This quick tutorial will teach you how to use conditional nodes and edges in SemanticKernel.Graph to create dynamic workflows that can make decisions and route execution based on state.

## What You'll Learn

* Creating `ConditionalGraphNode` for if/else logic
* Using `ConditionalEdge` for dynamic routing
* Building workflows that adapt to different scenarios
* Template-based and function-based conditions

## Concepts and Techniques

**ConditionalGraphNode**: A specialized node type that evaluates conditions and routes execution to different paths based on the result.

**ConditionalEdge**: Edges that connect conditional nodes to their true/false execution paths, enabling dynamic workflow routing.

**Dynamic Routing**: The ability of a graph to change its execution path based on runtime conditions and state values.

**Template Expressions**: String-based conditions that can reference state variables and be evaluated at runtime.

## Prerequisites

* [First Graph Tutorial](first-graph-5-minutes.md) completed
* [State Quickstart](state-quickstart.md) completed
* Basic understanding of SemanticKernel.Graph concepts

## Step 1: Basic Conditional Node

### Simple If/Else Logic

```csharp
using SemanticKernel.Graph.Nodes;

// Create a conditional node that checks user age
var ageCheckNode = new ConditionalGraphNode(
    state => state.GetValue<int>("userAge") >= 18,
    "age_check",
    "AgeVerification",
    "Checks if user is 18 or older"
);
```

### Using Template-Based Conditions

```csharp
// Alternative: Use template expression
var templateAgeCheck = new ConditionalGraphNode(
    "{{userAge}} >= 18",
    "template_age_check",
    "TemplateAgeCheck",
    "Template-based age verification"
);
```

## Step 2: Connecting Conditional Paths

### Add True/False Nodes

```csharp
// Create function nodes for different paths
var adultNode = new FunctionGraphNode(
    KernelFunctionFactory.CreateFromMethod(
        () => "Welcome! You have access to adult content.",
        "AdultAccess",
        "Provides adult content access"
    ),
    "adult_node"
);

var minorNode = new FunctionGraphNode(
    KernelFunctionFactory.CreateFromMethod(
        () => "Sorry, you must be 18 or older to access this content.",
        "MinorAccess",
        "Informs user about age restriction"
    ),
    "minor_node"
);

// Connect conditional paths
ageCheckNode
    .AddTrueNode(adultNode)    // Execute if condition is true
    .AddFalseNode(minorNode);  // Execute if condition is false
```

## Step 3: Building a Complete Conditional Workflow

### Create the Graph

```csharp
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("gpt-3.5-turbo", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
        builder.AddGraphSupport();
        var kernel = builder.Build();

        // Node 1: Input processing
        var inputNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    var age = args.GetValue<int>("userAge");
                    var name = args.GetValue<string>("userName");
                    
                    // Add some analysis
                    var isVip = age > 25 && name.Length > 5;
                    args["isVip"] = isVip;
                    args["ageGroup"] = age < 18 ? "minor" : age < 65 ? "adult" : "senior";
                    
                    return $"Processed user: {name}, Age: {age}, VIP: {isVip}";
                },
                "ProcessUser",
                "Processes user input and determines characteristics"
            ),
            "input_node"
        ).StoreResultAs("inputResult");

        // Node 2: Age verification
        var ageCheckNode = new ConditionalGraphNode(
            state => state.GetValue<int>("userAge") >= 18,
            "age_check",
            "AgeVerification",
            "Verifies user is 18 or older"
        );

        // Node 3: VIP status check
        var vipCheckNode = new ConditionalGraphNode(
            "{{isVip}} == true",
            "vip_check",
            "VipStatusCheck",
            "Checks if user has VIP status"
        );

        // Node 4: Adult content access
        var adultNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    var name = args.GetValue<string>("userName");
                    var age = args.GetValue<int>("userAge");
                    var isVip = args.GetValue<bool>("isVip");
                    
                    var access = isVip ? "Premium" : "Standard";
                    args["accessLevel"] = access;
                    args["contentType"] = "adult";
                    
                    return $"Welcome {name}! You have {access} access to adult content.";
                },
                "AdultAccess",
                "Provides adult content access"
            ),
            "adult_node"
        ).StoreResultAs("adultResult");

        // Node 5: VIP upgrade suggestion
        var vipUpgradeNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    var name = args.GetValue<string>("userName");
                    var age = args.GetValue<int>("userAge");
                    
                    args["upgradeSuggested"] = true;
                    args["upgradeReason"] = "Age and activity qualify for VIP";
                    
                    return $"Hello {name}! Based on your age ({age}), you might qualify for VIP status.";
                },
                "VipUpgrade",
                "Suggests VIP upgrade for eligible users"
            ),
            "vip_upgrade_node"
        ).StoreResultAs("upgradeResult");

        // Node 6: Minor access restriction
        var minorNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    var name = args.GetValue<string>("userName");
                    var age = args.GetValue<int>("userAge");
                    
                    args["accessLevel"] = "restricted";
                    args["contentType"] = "family";
                    args["restrictionReason"] = "Age requirement not met";
                    
                    return $"Hello {name}! You're {age} years old. This content requires you to be 18 or older.";
                },
                "MinorAccess",
                "Handles access for users under 18"
            ),
            "minor_node"
        ).StoreResultAs("minorResult");

        // Node 7: Final summary
        var summaryNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    var name = args.GetValue<string>("userName");
                    var age = args.GetValue<int>("userAge");
                    var accessLevel = args.GetValue<string>("accessLevel");
                    var contentType = args.GetValue<string>("contentType");
                    
                    var summary = $"User: {name}, Age: {age}, Access: {accessLevel}, Content: {contentType}";
                    args["finalSummary"] = summary;
                    
                    return summary;
                },
                "CreateSummary",
                "Creates final summary of user access"
            ),
            "summary_node"
        ).StoreResultAs("summaryResult");

        // Build and configure the graph
        var graph = new GraphExecutor("ConditionalWorkflowExample", "Demonstrates conditional routing based on user characteristics");
        
        graph.AddNode(inputNode);
        graph.AddNode(ageCheckNode);
        graph.AddNode(vipCheckNode);
        graph.AddNode(adultNode);
        graph.AddNode(vipUpgradeNode);
        graph.AddNode(minorNode);
        graph.AddNode(summaryNode);
        
        // Connect nodes with conditional logic
        graph.Connect(inputNode, ageCheckNode);
        
        // Age check paths
        graph.Connect(ageCheckNode, vipCheckNode, 
            edge => edge.When(state => state.GetValue<int>("userAge") >= 18));
        graph.Connect(ageCheckNode, minorNode, 
            edge => edge.When(state => state.GetValue<int>("userAge") < 18));
        
        // VIP check paths
        graph.Connect(vipCheckNode, adultNode, 
            edge => edge.When(state => state.GetValue<bool>("isVip") == true));
        graph.Connect(vipCheckNode, vipUpgradeNode, 
            edge => edge.When(state => state.GetValue<bool>("isVip") == false));
        
        // Connect all paths to summary
        graph.Connect(adultNode, summaryNode);
        graph.Connect(vipUpgradeNode, summaryNode);
        graph.Connect(minorNode, summaryNode);
        
        graph.SetStartNode(inputNode);

        // Execute with different user scenarios
        var scenarios = new[]
        {
            new { Name = "Alice", Age = 25, ExpectedPath = "VIP Adult" },
            new { Name = "Bob", Age = 17, ExpectedPath = "Minor" },
            new { Name = "Charlie", Age = 30, ExpectedPath = "Standard Adult" }
        };

        foreach (var scenario in scenarios)
        {
            Console.WriteLine($"\n=== Testing: {scenario.Name}, Age {scenario.Age} ===");
            
            var initialState = new KernelArguments
            {
                ["userName"] = scenario.Name,
                ["userAge"] = scenario.Age
            };
            
            var result = await graph.ExecuteAsync(initialState);
            
            Console.WriteLine($"Path taken: {scenario.ExpectedPath}");
            Console.WriteLine($"Final summary: {result.GetValue<string>("finalSummary")}");
            Console.WriteLine($"Access level: {result.GetValue<string>("accessLevel")}");
        }
        
        Console.WriteLine("\n✅ Conditional workflow completed successfully!");
    }
}
```

## Step 4: Run Your Conditional Example

### Set Up Environment Variables

```bash
# Windows
setx OPENAI_API_KEY "your-api-key-here"

# macOS/Linux
export OPENAI_API_KEY="your-api-key-here"
```

### Execute the Graph

```bash
dotnet run
```

You should see output like:

```
=== Testing: Alice, Age 25 ===
Path taken: VIP Adult
Final summary: User: Alice, Age: 25, Access: Premium, Content: adult
Access level: Premium

=== Testing: Bob, Age 17 ===
Path taken: Minor
Final summary: User: Bob, Age: 17, Access: restricted, Content: family
Access level: restricted

=== Testing: Charlie, Age 30 ===
Path taken: Standard Adult
Final summary: User: Charlie, Age: 30, Access: Standard, Content: adult
Access level: Standard

✅ Conditional workflow completed successfully!
```

## What Just Happened?

### 1. **Conditional Node Creation**
```csharp
var ageCheckNode = new ConditionalGraphNode(
    state => state.GetValue<int>("userAge") >= 18,
    "age_check"
);
```
Creates a node that evaluates a condition and routes execution based on the result.

### 2. **Conditional Edge Routing**
```csharp
graph.Connect(ageCheckNode, vipCheckNode, 
    edge => edge.When(state => state.GetValue<int>("userAge") >= 18));
```
Uses `ConditionalEdge` to create dynamic routing based on state conditions.

### 3. **Multiple Execution Paths**
The graph creates different execution paths:
* **Alice (25)**: Input → Age Check (true) → VIP Check (true) → Adult Access → Summary
* **Bob (17)**: Input → Age Check (false) → Minor Access → Summary  
* **Charlie (30)**: Input → Age Check (true) → VIP Check (false) → VIP Upgrade → Summary

### 4. **State-Based Decision Making**
Each conditional node reads from the current state to make routing decisions, creating a dynamic workflow that adapts to different inputs.

## Key Concepts

* **ConditionalGraphNode**: Evaluates conditions and routes execution to different paths
* **ConditionalEdge**: Creates guarded transitions between nodes based on state conditions
* **Template Conditions**: Use Handlebars-like syntax for simple conditions
* **Function Conditions**: Use C# functions for complex conditional logic
* **Dynamic Routing**: Execution flow changes based on the current state

## Common Patterns

### Simple Boolean Conditions
```csharp
var simpleCheck = new ConditionalGraphNode(
    state => state.GetValue<bool>("isEnabled"),
    "simple_check"
);
```

### Complex State Analysis
```csharp
var complexCheck = new ConditionalGraphNode(
    state => 
    {
        var score = state.GetValue<int>("score");
        var attempts = state.GetValue<int>("attempts");
        var isVip = state.GetValue<bool>("isVip");
        
        return score > 80 && attempts < 3 || isVip;
    },
    "complex_check"
);
```

### Template-Based Conditions
```csharp
var templateCheck = new ConditionalGraphNode(
    "{{score}} > 80 && {{attempts}} < 3 || {{isVip}} == true",
    "template_check"
);
```

### Multiple Conditional Paths
```csharp
// Create a switch-like structure
var primaryCheck = new ConditionalGraphNode("{{priority}} == 'high'");
var secondaryCheck = new ConditionalGraphNode("{{priority}} == 'medium'");

graph.Connect(inputNode, primaryCheck, edge => edge.When(state => state.GetValue<string>("priority") == "high"));
graph.Connect(inputNode, secondaryCheck, edge => edge.When(state => state.GetValue<string>("priority") == "medium"));
```

## Troubleshooting

### **Condition Always Evaluates to False**
```
Condition never evaluates to true, always taking false path
```
**Solution**: Check your condition logic and verify state values are as expected.

### **Multiple Paths Executing**
```
Multiple conditional paths are executing when only one should
```
**Solution**: Ensure your conditions are mutually exclusive or use proper conditional edges.

### **State Values Not Available**
```
System.Collections.Generic.KeyNotFoundException: The given key 'missingKey' was not present
```
**Solution**: Use `GetValueOrDefault()` or check with `ContainsKey()` before reading state values.

### **Template Condition Syntax Errors**
```
Template condition fails to evaluate
```
**Solution**: Verify template syntax and ensure all referenced variables exist in state.

## Next Steps

* **[Conditional Nodes Tutorial](conditional-nodes-tutorial.md)**: Advanced conditional patterns and best practices
* **[State Management](state-tutorial.md)**: Advanced state manipulation and validation
* **[Switch Nodes](how-to/switch-nodes.md)**: Multi-branch conditional logic
* **[Core Concepts](concepts/index.md)**: Understanding graphs, nodes, and execution

## Concepts and Techniques

This tutorial introduces several key concepts:

* **Conditional Routing**: The ability to direct execution flow based on state conditions
* **ConditionalGraphNode**: A node that evaluates conditions and routes execution
* **ConditionalEdge**: An edge that only allows traversal when specific conditions are met
* **Template Conditions**: Natural language expressions for simple conditional logic
* **Function Conditions**: C# functions for complex conditional evaluation
* **Dynamic Workflows**: Graphs that adapt their execution path based on runtime state

## Prerequisites and Minimum Configuration

To complete this tutorial, you need:
* **.NET 8.0+** runtime and SDK
* **SemanticKernel.Graph** package installed
* **LLM Provider** configured with valid API keys
* **Environment Variables** set up for your API credentials

## See Also

* **[First Graph Tutorial](first-graph-5-minutes.md)**: Create your first graph workflow
* **[State Quickstart](state-quickstart.md)**: Manage data flow between nodes
* **[Conditional Nodes Tutorial](conditional-nodes-tutorial.md)**: Advanced conditional patterns
* **[Core Concepts](concepts/index.md)**: Understanding graphs, nodes, and execution
* **[API Reference](api/nodes.md)**: Complete node API documentation

## Reference APIs

* **[ConditionalGraphNode](../api/nodes.md#conditional-graph-node)**: Conditional routing node
* **[ConditionalEdge](../api/core.md#conditional-edge)**: Conditional connection edge
* **[IGraphNode](../api/core.md#igraph-node)**: Base node interface
* **[GraphExecutor](../api/core.md#graph-executor)**: Graph execution engine
