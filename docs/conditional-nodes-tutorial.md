# Conditional Nodes Tutorial

This tutorial will teach you how to use conditional nodes and edges in SemanticKernel.Graph to create dynamic, intelligent workflows that can make decisions and route execution based on state and AI responses.

## What You'll Learn

- How to create conditional nodes that make routing decisions
- Using conditional edges for dynamic workflow paths
- Building intelligent workflows that adapt to different scenarios
- Best practices for conditional logic in graphs

## Prerequisites

Before starting, ensure you have:
- Completed the [First Graph Tutorial](first-graph.md)
- Understanding of [State Management](state-tutorial.md)
- Basic knowledge of SemanticKernel.Graph concepts

## Understanding Conditional Nodes

### What are Conditional Nodes?

Conditional nodes are special nodes that evaluate conditions and decide which path the workflow should take next. They act like traffic controllers, directing execution flow based on the current state.

### Types of Conditions

- **Simple Boolean Conditions**: Basic true/false decisions
- **State-Based Conditions**: Decisions based on data in the state
- **AI-Generated Conditions**: Complex decisions made by AI analysis
- **Template-Based Conditions**: Conditions expressed in natural language

## Basic Conditional Node

### Creating a Simple Conditional Node

```csharp
using SemanticKernel.Graph.Nodes;

// Basic conditional node with a simple boolean function
var isPositiveNode = new ConditionalGraphNode("IsPositive",
    state => 
    {
        var sentiment = state.GetValueOrDefault("sentiment", "neutral");
        return sentiment.ToString().ToLower().Contains("positive");
    }
);
```

### Using the Conditional Node

```csharp
var graph = new GraphExecutor("ConditionalExample");

// Add nodes
graph.AddNode(inputNode);
graph.AddNode(isPositiveNode);
graph.AddNode(positiveResponseNode);
graph.AddNode(negativeResponseNode);

// Connect with conditional routing
graph.Connect(inputNode, isPositiveNode);
graph.Connect(isPositiveNode, positiveResponseNode, 
    edge => edge.When(state => state.GetValueOrDefault("sentiment", "").ToString().Contains("positive")));
graph.Connect(isPositiveNode, negativeResponseNode, 
    edge => edge.When(state => !state.GetValueOrDefault("sentiment", "").ToString().Contains("positive")));

graph.SetStartNode(inputNode);
```

## Advanced Conditional Patterns

### Multi-Path Routing

```csharp
var analysisNode = new FunctionGraphNode(
    kernel.CreateFunctionFromPrompt(@"
        Analyze this text: {{$input}}
        Return JSON with:
        {
            "sentiment": "positive|negative|neutral",
            "urgency": "high|medium|low",
            "category": "technical|personal|business"
        }
    ")
).StoreResultAs("analysis");

var routingNode = new ConditionalGraphNode("RouteByAnalysis",
    state => 
    {
        var analysis = state.GetValueOrDefault("analysis", "");
        if (analysis.ToString().Contains("\"urgency\": \"high\""))
            return "high";
        if (analysis.ToString().Contains("\"sentiment\": \"negative\""))
            return "negative";
        return "normal";
    }
);

// Connect to different handlers
graph.Connect(routingNode, highPriorityNode, 
    edge => edge.When(state => routingNode.EvaluateCondition(state) == "high"));
graph.Connect(routingNode, negativeHandlerNode, 
    edge => edge.When(state => routingNode.EvaluateCondition(state) == "negative"));
graph.Connect(routingNode, normalHandlerNode, 
    edge => edge.When(state => routingNode.EvaluateCondition(state) == "normal"));
```

### AI-Powered Decision Making

```csharp
var decisionNode = new FunctionGraphNode(
    kernel.CreateFunctionFromPrompt(@"
        Based on this situation: {{$context}}
        And the current state: {{$currentState}}
        
        Decide what action to take next. Return only one of these options:
        - "continue" - if we should proceed normally
        - "escalate" - if this needs human intervention
        - "retry" - if we should try again with different parameters
        - "abort" - if we should stop the workflow
        
        Explain your reasoning briefly.
    ")
).StoreResultAs("decision");

var actionRouter = new ConditionalGraphNode("RouteByDecision",
    state => 
    {
        var decision = state.GetValueOrDefault("decision", "").ToString().ToLower();
        if (decision.Contains("continue")) return "continue";
        if (decision.Contains("escalate")) return "escalate";
        if (decision.Contains("retry")) return "retry";
        if (decision.Contains("abort")) return "abort";
        return "default";
    }
);
```

## Complete Conditional Workflow Example

Let's build a complete example that demonstrates conditional routing:

```csharp
using Microsoft.SemanticKernel;
using SemanticKernel.Graph;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("gpt-4", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
        builder.AddGraphSupport();
        var kernel = builder.Build();

        // 1. Input analysis node
        var inputNode = new FunctionGraphNode(
            kernel.CreateFunctionFromPrompt(@"
                Analyze this customer request: {{$customerRequest}}
                
                Provide analysis in JSON format:
                {
                    "complexity": "simple|moderate|complex",
                    "urgency": "low|medium|high",
                    "category": "technical|billing|general",
                    "estimatedTime": "minutes|hours|days"
                }
            ")
        ).StoreResultAs("analysis");

        // 2. Complexity-based routing
        var complexityRouter = new ConditionalGraphNode("RouteByComplexity",
            state => 
            {
                var analysis = state.GetValueOrDefault("analysis", "");
                if (analysis.ToString().Contains("\"complexity\": \"simple\""))
                    return "simple";
                if (analysis.ToString().Contains("\"complexity\": \"moderate\""))
                    return "moderate";
                return "complex";
            }
        );

        // 3. Simple request handler
        var simpleHandler = new FunctionGraphNode(
            kernel.CreateFunctionFromPrompt(@"
                Handle this simple request: {{$customerRequest}}
                Analysis: {{$analysis}}
                
                Provide a direct, helpful response that resolves the issue.
            ")
        ).StoreResultAs("response");

        // 4. Moderate request handler
        var moderateHandler = new FunctionGraphNode(
            kernel.CreateFunctionFromPrompt(@"
                Handle this moderate complexity request: {{$customerRequest}}
                Analysis: {{$analysis}}
                
                Provide a detailed response with steps and resources.
                Include any necessary follow-up actions.
            ")
        ).StoreResultAs("response");

        // 5. Complex request handler
        var complexHandler = new FunctionGraphNode(
            kernel.CreateFunctionFromPrompt(@"
                This is a complex request: {{$customerRequest}}
                Analysis: {{$analysis}}
                
                Provide a comprehensive response that:
                1. Acknowledges the complexity
                2. Outlines a step-by-step approach
                3. Suggests escalation if needed
                4. Sets clear expectations
            ")
        ).StoreResultAs("response");

        // 6. Urgency check
        var urgencyCheck = new ConditionalGraphNode("CheckUrgency",
            state => 
            {
                var analysis = state.GetValueOrDefault("analysis", "");
                return analysis.ToString().Contains("\"urgency\": \"high\"");
            }
        );

        // 7. High urgency handler
        var highUrgencyHandler = new FunctionGraphNode(
            kernel.CreateFunctionFromPrompt(@"
                This is a HIGH URGENCY request: {{$customerRequest}}
                Current response: {{$response}}
                
                Enhance the response to emphasize urgency and provide immediate action items.
                Include escalation contact information.
            ")
        ).StoreResultAs("finalResponse");

        // 8. Final response formatter
        var responseFormatter = new FunctionGraphNode(
            kernel.CreateFunctionFromPrompt(@"
                Format the final response for the customer:
                Request: {{$customerRequest}}
                Response: {{$finalResponse}}
                Analysis: {{$analysis}}
                
                Create a professional, well-structured response that addresses all aspects.
            ")
        ).StoreResultAs("formattedResponse");

        // Build the graph
        var graph = new GraphExecutor("ConditionalWorkflowExample");

        // Add all nodes
        graph.AddNode(inputNode);
        graph.AddNode(complexityRouter);
        graph.AddNode(simpleHandler);
        graph.AddNode(moderateHandler);
        graph.AddNode(complexHandler);
        graph.AddNode(urgencyCheck);
        graph.AddNode(highUrgencyHandler);
        graph.AddNode(responseFormatter);

        // Connect the workflow
        graph.Connect(inputNode, complexityRouter);
        
        // Complexity routing
        graph.Connect(complexityRouter, simpleHandler, 
            edge => edge.When(state => complexityRouter.EvaluateCondition(state) == "simple"));
        graph.Connect(complexityRouter, moderateHandler, 
            edge => edge.When(state => complexityRouter.EvaluateCondition(state) == "moderate"));
        graph.Connect(complexityRouter, complexHandler, 
            edge => edge.When(state => complexityRouter.EvaluateCondition(state) == "complex"));
        
        // All paths go to urgency check
        graph.Connect(simpleHandler, urgencyCheck);
        graph.Connect(moderateHandler, urgencyCheck);
        graph.Connect(complexHandler, urgencyCheck);
        
        // Urgency routing
        graph.Connect(urgencyCheck, highUrgencyHandler, 
            edge => edge.When(state => urgencyCheck.EvaluateCondition(state)));
        graph.Connect(urgencyCheck, responseFormatter, 
            edge => edge.When(state => !urgencyCheck.EvaluateCondition(state)));
        
        // High urgency path
        graph.Connect(highUrgencyHandler, responseFormatter);
        
        graph.SetStartNode(inputNode);

        // Test with different inputs
        var testRequests = new[]
        {
            "I can't log into my account", // Simple
            "I need help setting up advanced security features", // Moderate
            "My entire system is down and I have a critical presentation in 2 hours" // Complex + High urgency
        };

        foreach (var request in testRequests)
        {
            Console.WriteLine($"\n=== Testing: {request} ===");
            
            var state = new KernelArguments { ["customerRequest"] = request };
            var result = await graph.ExecuteAsync(state);
            
            Console.WriteLine($"Complexity: {result.GetValueOrDefault("analysis", "No analysis")}");
            Console.WriteLine($"Response: {result.GetValueOrDefault("formattedResponse", "No response")}");
        }
    }
}
```

## Conditional Edge Patterns

### Basic Conditional Edges

```csharp
// Simple boolean condition
graph.Connect(nodeA, nodeB, 
    edge => edge.When(state => state.ContainsKey("success") && (bool)state["success"]));

// String-based condition
graph.Connect(nodeA, nodeB, 
    edge => edge.When(state => state.GetValueOrDefault("status", "").ToString() == "approved"));

// Numeric condition
graph.Connect(nodeA, nodeB, 
    edge => edge.When(state => 
    {
        var count = state.GetValueOrDefault("itemCount", 0);
        return count is int i && i > 10;
    }));
```

### Complex Conditional Logic

```csharp
// Multiple conditions
graph.Connect(nodeA, nodeB, 
    edge => edge.When(state => 
    {
        var hasPermission = state.GetValueOrDefault("userRole", "").ToString() == "admin";
        var isBusinessHours = DateTime.Now.Hour >= 9 && DateTime.Now.Hour <= 17;
        var isUrgent = state.GetValueOrDefault("priority", "").ToString() == "high";
        
        return hasPermission && (isBusinessHours || isUrgent);
    }));

// AI-generated condition
graph.Connect(nodeA, nodeB, 
    edge => edge.When(state => 
    {
        var aiDecision = state.GetValueOrDefault("aiDecision", "").ToString();
        return aiDecision.Contains("proceed") || aiDecision.Contains("continue");
    }));
```

### Fallback and Default Routes

```csharp
// Primary route with fallback
graph.Connect(decisionNode, primaryHandler, 
    edge => edge.When(state => primaryHandler.CanHandle(state)));
graph.Connect(decisionNode, fallbackHandler, 
    edge => edge.When(state => !primaryHandler.CanHandle(state)));

// Multiple fallback levels
graph.Connect(decisionNode, handlerA, 
    edge => edge.When(state => conditionA(state)));
graph.Connect(decisionNode, handlerB, 
    edge => edge.When(state => !conditionA(state) && conditionB(state)));
graph.Connect(decisionNode, defaultHandler, 
    edge => edge.When(state => !conditionA(state) && !conditionB(state)));
```

## Best Practices

### 1. **Keep Conditions Simple and Readable**
```csharp
// Good - clear and readable
var isAdmin = new ConditionalGraphNode("IsAdmin",
    state => state.GetValueOrDefault("userRole", "").ToString() == "admin");

// Avoid - complex inline logic
var complexCondition = new ConditionalGraphNode("Complex",
    state => state.ContainsKey("a") && state.ContainsKey("b") && 
             state.GetValueOrDefault("c", 0) is int i && i > 10 && 
             state.GetValueOrDefault("d", "").ToString().Length > 5);
```

### 2. **Use Descriptive Names**
```csharp
// Good - descriptive names
var shouldEscalate = new ConditionalGraphNode("ShouldEscalate", condition);
var isHighPriority = new ConditionalGraphNode("IsHighPriority", condition);

// Avoid - generic names
var check1 = new ConditionalGraphNode("Check1", condition);
var decision = new ConditionalGraphNode("Decision", condition);
```

### 3. **Handle Edge Cases**
```csharp
var safeCondition = new ConditionalGraphNode("SafeCondition",
    state => 
    {
        try
        {
            var value = state.GetValueOrDefault("key", "");
            return value?.ToString()?.Contains("expected") == true;
        }
        catch
        {
            return false; // Default to safe path
        }
    });
```

### 4. **Test Different Scenarios**
```csharp
// Test various input combinations
var testCases = new[]
{
    new { input = "positive", expected = "positive" },
    new { input = "negative", expected = "negative" },
    new { input = "", expected = "default" },
    new { input = null, expected = "default" }
};

foreach (var testCase in testCases)
{
    var state = new KernelArguments { ["input"] = testCase.input };
    var result = await graph.ExecuteAsync(state);
    Console.WriteLine($"Input: {testCase.input}, Result: {result.GetValueOrDefault("output", "default")}");
}
```

## Common Patterns

### **Switch-like Routing**
```csharp
var router = new ConditionalGraphNode("Router",
    state => 
    {
        var action = state.GetValueOrDefault("action", "").ToString().ToLower();
        return action switch
        {
            "create" => "create",
            "read" => "read",
            "update" => "update",
            "delete" => "delete",
            _ => "default"
        };
    });
```

### **Threshold-based Decisions**
```csharp
var thresholdNode = new ConditionalGraphNode("ThresholdCheck",
    state => 
    {
        var score = state.GetValueOrDefault("score", 0);
        return score switch
        {
            >= 90 => "excellent",
            >= 80 => "good",
            >= 70 => "fair",
            >= 60 => "poor",
            _ => "failing"
        };
    });
```

### **Multi-factor Decisions**
```csharp
var multiFactorNode = new ConditionalGraphNode("MultiFactorDecision",
    state => 
    {
        var risk = state.GetValueOrDefault("riskLevel", "low").ToString();
        var amount = state.GetValueOrDefault("amount", 0m);
        var userType = state.GetValueOrDefault("userType", "standard").ToString();
        
        if (risk == "high" || amount > 10000m)
            return "manual_review";
        if (userType == "premium" && amount <= 5000m)
            return "auto_approve";
        return "standard_review";
    });
```

## Troubleshooting

### **Common Issues**

#### Condition Always Returns False
```
System.InvalidOperationException: No next nodes found for node 'ConditionalNode'
```
**Solution**: Check your condition logic and ensure at least one edge condition evaluates to true.

#### Infinite Loops
```
System.InvalidOperationException: Maximum execution steps exceeded
```
**Solution**: Ensure your conditional logic doesn't create circular paths or infinite loops.

#### Unexpected Routing
```
Node 'HandlerA' executed instead of expected 'HandlerB'
```
**Solution**: Debug your condition logic and verify the state values being evaluated.

### **Debugging Tips**

1. **Add Logging to Conditions**
```csharp
var debugNode = new ConditionalGraphNode("DebugCondition",
    state => 
    {
        var result = state.GetValueOrDefault("key", "").ToString().Contains("expected");
        Console.WriteLine($"Condition evaluated: {result} for state: {state["key"]}");
        return result;
    });
```

2. **Test Conditions in Isolation**
```csharp
// Test your condition function directly
var testState = new KernelArguments { ["key"] = "test_value" };
var result = myCondition(testState);
Console.WriteLine($"Condition result: {result}");
```

3. **Validate State at Decision Points**
```csharp
var validationNode = new ConditionalGraphNode("ValidateState",
    state => 
    {
        Console.WriteLine($"State at decision point: {JsonSerializer.Serialize(state)}");
        return myCondition(state);
    });
```

## Next Steps

Now that you understand conditional nodes, explore these advanced topics:

- **[Loops Tutorial](loops-tutorial.md)**: Learn about loop nodes and iterative workflows
- **[Error Handling Guide](how-to/error-handling.md)**: Handle errors and exceptions in your workflows
- **[Advanced Routing](how-to/advanced-routing.md)**: Master complex routing patterns
- **[Patterns](patterns/index.md)**: Discover common workflow patterns

## Concepts and Techniques

This tutorial covers several key concepts:

- **Conditional Node**: A node that evaluates conditions and routes execution based on the result
- **Conditional Edge**: An edge that only allows execution when its condition evaluates to true
- **Routing Logic**: The decision-making process that determines workflow execution paths
- **State Evaluation**: How conditions analyze the current state to make routing decisions
- **Multi-path Workflows**: Creating workflows that can take different paths based on conditions

## Prerequisites and Minimum Configuration

To complete this tutorial, you need:
- **SemanticKernel.Graph** package installed and configured
- **LLM Provider** with valid API keys
- **Understanding** of basic graph concepts and state management
- **.NET 8.0+** runtime environment

## See Also

- **[State Management Tutorial](state-tutorial.md)**: Understanding how data flows through your graph
- **[First Graph Tutorial](first-graph.md)**: Building your first complete graph workflow
- **[How-to Guides](how-to/conditional-nodes.md)**: Advanced conditional node techniques
- **[API Reference](api/nodes.md)**: Complete node API documentation
