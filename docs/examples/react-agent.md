# ReAct Agent Example

This example demonstrates a simple, extensible ReAct (Reasoning → Acting → Observing) agent that can be flexibly extended with many tools.

## Objective

Learn how to implement ReAct agent patterns in graph-based workflows to:
- Create a minimal Reason → Act → Observe loop
- Implement extensible tool registration and discovery
- Demonstrate intelligent action selection and execution
- Show how to add new tools without modifying the agent structure
- Implement parameter validation and intelligent tool matching

## Prerequisites

- **.NET 8.0** or later
- **OpenAI API Key** configured in `appsettings.json`
- **Semantic Kernel Graph package** installed
- Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [ReAct Patterns](../patterns/react.md)
- Familiarity with [Action Nodes](../concepts/node-types.md)

## Key Components

### Concepts and Techniques

- **ReAct Pattern**: Reasoning → Acting → Observing loop for intelligent problem solving
- **Tool Discovery**: Automatic discovery and registration of available tools
- **Action Selection**: Intelligent selection of appropriate tools based on context
- **Parameter Validation**: Validation of tool parameters before execution
- **Extensibility**: Adding new tools without modifying agent structure

### Core Classes

- `GraphExecutor`: Executor for ReAct agent workflows
- `FunctionGraphNode`: Nodes for reasoning and observation
- `ActionGraphNode`: Node for tool execution with auto-discovery
- `ActionSelectionCriteria`: Criteria for tool selection and filtering
- `ConditionalEdge`: Graph edges for workflow control

## Running the Example

### Getting Started

This example demonstrates ReAct (Reasoning + Acting) patterns with the Semantic Kernel Graph package. The code snippets below show you how to implement this pattern in your own applications.

## Step-by-Step Implementation

### 1. Tool Registration

The example starts by registering basic tools that the agent can use.

```csharp
// Register a small set of mock tools (can be replaced/extended freely)
RegisterBasicTools(kernel);

private static void RegisterBasicTools(Kernel kernel)
{
    // Weather tool
    kernel.ImportPluginFromObject(new WeatherTool());
    
    // Calculator tool
    kernel.ImportPluginFromObject(new CalculatorTool());
    
    // Search tool
    kernel.ImportPluginFromObject(new SearchTool());
}

// Example tool implementations
public class WeatherTool
{
    [KernelFunction, Description("Get current weather for a location")]
    public string GetWeather([Description("City name")] string city)
    {
        // Simulate weather data
        var weather = city.ToLowerInvariant() switch
        {
            "lisbon" => "Sunny, 22°C, light breeze",
            "london" => "Cloudy, 15°C, light rain",
            "paris" => "Partly cloudy, 18°C, calm",
            _ => $"Weather data unavailable for {city}"
        };
        
        return $"Current weather in {city}: {weather}";
    }
}

public class CalculatorTool
{
    [KernelFunction, Description("Perform mathematical calculations")]
    public string Calculate([Description("Mathematical expression")] string expression)
    {
        try
        {
            // Simple calculation evaluation (in production, use proper expression parser)
            var result = EvaluateExpression(expression);
            return $"Result of {expression} = {result}";
        }
        catch (Exception ex)
        {
            return $"Error calculating {expression}: {ex.Message}";
        }
    }
    
    private static double EvaluateExpression(string expression)
    {
        // Simplified expression evaluation
        if (expression.Contains("*"))
        {
            var parts = expression.Split('*');
            if (parts.Length == 2 && double.TryParse(parts[0], out var a) && double.TryParse(parts[1], out var b))
                return a * b;
        }
        throw new ArgumentException("Unsupported expression format");
    }
}

public class SearchTool
{
    [KernelFunction, Description("Search for information on a topic")]
    public string Search([Description("Search query")] string query)
    {
        // Simulate search results
        var results = query.ToLowerInvariant() switch
        {
            var q when q.Contains("c#") && q.Contains("logging") => 
                "C# logging best practices: Use ILogger<T>, structured logging, log levels, and centralized configuration.",
            var q when q.Contains("best practices") => 
                "General best practices: Follow established patterns, document code, test thoroughly, and maintain consistency.",
            _ => $"Search results for '{query}': Multiple relevant sources found with comprehensive information."
        };
        
        return results;
    }
}
```

### 2. Creating the ReAct Agent

The agent is built with a minimal three-node structure: reasoning, action, and observation.

```csharp
private static GraphExecutor CreateSimpleReActAgent(Kernel kernel)
{
    var executor = new GraphExecutor("SimpleReActAgent", "Minimal ReAct agent with extensible tools");

    var reasoning = new FunctionGraphNode(
        CreateReasoningFunction(kernel),
        "react_reason",
        "Analyze the user query and suggest an action"
    );

    // Auto-discover actions from all plugins; keep it simple and let the node pick best matching
    var actions = ActionGraphNode.CreateWithActions(
        kernel,
        new ActionSelectionCriteria
        {
            // Keep open by default; can be restricted via IncludedPlugins/FunctionNamePattern
        },
        "react_act");
    actions.ConfigureExecution(ActionSelectionStrategy.Intelligent, enableParameterValidation: true);

    var observe = new FunctionGraphNode(
        CreateObservationFunction(kernel),
        "react_observe",
        "Summarize action result as a final answer"
    ).StoreResultAs("final_answer");

    executor.AddNode(reasoning);
    executor.AddNode(actions);
    executor.AddNode(observe);

    executor.SetStartNode(reasoning.NodeId);
    executor.AddEdge(ConditionalEdge.CreateUnconditional(reasoning, actions));
    executor.AddEdge(ConditionalEdge.CreateUnconditional(actions, observe));

    return executor;
}
```

### 3. Reasoning Function

The reasoning function analyzes user queries and suggests appropriate actions.

```csharp
private static KernelFunction CreateReasoningFunction(Kernel kernel)
{
    return KernelFunctionFactory.CreateFromMethod(
        (KernelArguments args) =>
        {
            var query = args.TryGetValue("user_query", out var q) ? q?.ToString() ?? string.Empty : string.Empty;
            
            // Analyze the query and determine the appropriate action
            var action = query.ToLowerInvariant() switch
            {
                var s when s.Contains("weather") => "get_weather",
                var s when s.Contains("calculate") || s.Contains("*") || s.Contains("+") || s.Contains("-") => "calculate",
                var s when s.Contains("search") || s.Contains("best practices") => "search",
                var s when s.Contains("convert") && s.Contains("currency") => "currency_convert",
                _ => "search" // Default to search for general queries
            };
            
            // Extract parameters based on the action
            var parameters = new Dictionary<string, object>();
            
            switch (action)
            {
                case "get_weather":
                    var cityMatch = System.Text.RegularExpressions.Regex.Match(query, @"weather in (\w+)");
                    if (cityMatch.Success)
                        parameters["city"] = cityMatch.Groups[1].Value;
                    break;
                    
                case "calculate":
                    var calcMatch = System.Text.RegularExpressions.Regex.Match(query, @"(\d+\s*[\*\+\-]\s*\d+)");
                    if (calcMatch.Success)
                        parameters["expression"] = calcMatch.Groups[1].Value;
                    break;
                    
                case "search":
                    parameters["query"] = query.Replace("search:", "").Trim();
                    break;
                    
                case "currency_convert":
                    var currencyMatch = System.Text.RegularExpressions.Regex.Match(query, @"(\d+)\s+(\w+)\s+to\s+(\w+)");
                    if (currencyMatch.Success)
                    {
                        parameters["amount"] = currencyMatch.Groups[1].Value;
                        parameters["from"] = currencyMatch.Groups[1].Value;
                        parameters["to"] = currencyMatch.Groups[2].Value;
                    }
                    break;
            }
            
            args["suggested_action"] = action;
            args["action_parameters"] = parameters;
            
            return $"Reasoning: Query '{query}' suggests action '{action}' with parameters: {string.Join(", ", parameters.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";
        },
        functionName: "react_reasoning",
        description: "Analyzes user queries and suggests appropriate actions"
    );
}
```

### 4. Action Execution

The action node automatically discovers and executes the appropriate tools.

```csharp
// The ActionGraphNode automatically handles:
// - Tool discovery from registered plugins
// - Parameter mapping and validation
// - Tool execution with proper error handling
// - Result formatting for the observation step

// Configuration for intelligent action selection
actions.ConfigureExecution(
    ActionSelectionStrategy.Intelligent, 
    enableParameterValidation: true
);
```

### 5. Observation Function

The observation function summarizes action results into final answers.

```csharp
private static KernelFunction CreateObservationFunction(Kernel kernel)
{
    return KernelFunctionFactory.CreateFromMethod(
        (KernelArguments args) =>
        {
            var query = args.TryGetValue("user_query", out var q) ? q?.ToString() ?? string.Empty : string.Empty;
            var action = args.TryGetValue("suggested_action", out var a) ? a?.ToString() ?? string.Empty : string.Empty;
            var result = args.TryGetValue("action_result", out var r) ? r?.ToString() ?? string.Empty : string.Empty;
            
            // Format the final answer based on the action and result
            var answer = action switch
            {
                "get_weather" => $"Based on your query about weather, I found: {result}",
                "calculate" => $"I calculated the result for you: {result}",
                "search" => $"Here's what I found when searching: {result}",
                "currency_convert" => $"I converted the currency for you: {result}",
                _ => $"I processed your request '{query}' and here's what I found: {result}"
            };
            
            args["final_answer"] = answer;
            return answer;
        },
        functionName: "react_observation",
        description: "Summarizes action results into final answers"
    );
}
```

### 6. Sample Query Processing

The example processes multiple sample queries to demonstrate the agent's capabilities.

```csharp
// Run a few sample queries and show how adding a tool still works transparently
var sampleQueries = new[]
{
    "What's the weather in Lisbon today?",
    "Calculate: 42 * 7",
    "Search: best practices for C# logging"
};

foreach (var query in sampleQueries)
{
    Console.WriteLine($"🧑‍💻 User: {query}");
    var args = new KernelArguments
    {
        ["user_query"] = query,
        ["max_steps"] = 3
    };

    var result = await executor.ExecuteAsync(kernel, args);
    var answer = result.GetValue<string>() ?? "No answer produced";
    Console.WriteLine($"🤖 Agent: {answer}\n");
    await Task.Delay(250);
}
```

### 7. Tool Extensibility

The example demonstrates how to add new tools without modifying the agent structure.

```csharp
// Demonstrate extensibility: add a new tool and reuse the same agent
AddCurrencyConversionTool(kernel);
Console.WriteLine("➕ Added new tool: currency_convert(amount, from, to)\n");

var extendedQuery = "Convert 100 USD to EUR";
Console.WriteLine($"🧑‍💻 User: {extendedQuery}");
var extendedArgs = new KernelArguments { ["user_query"] = extendedQuery };
var extendedResult = await executor.ExecuteAsync(kernel, extendedArgs);
Console.WriteLine($"🤖 Agent: {extendedResult.GetValue<string>() ?? "No answer produced"}\n");

private static void AddCurrencyConversionTool(Kernel kernel)
{
    kernel.ImportPluginFromObject(new CurrencyConversionTool());
}

public class CurrencyConversionTool
{
    [KernelFunction, Description("Convert currency between different units")]
    public string ConvertCurrency(
        [Description("Amount to convert")] double amount,
        [Description("Source currency code")] string from,
        [Description("Target currency code")] string to)
    {
        // Simulate currency conversion rates
        var rates = new Dictionary<string, double>
        {
            ["USD"] = 1.0,
            ["EUR"] = 0.85,
            ["GBP"] = 0.73,
            ["JPY"] = 110.0
        };
        
        if (rates.TryGetValue(from.ToUpper(), out var fromRate) && rates.TryGetValue(to.ToUpper(), out var toRate))
        {
            var convertedAmount = amount * (toRate / fromRate);
            return $"{amount} {from.ToUpper()} = {convertedAmount:F2} {to.ToUpper()}";
        }
        
        return $"Unable to convert {amount} {from} to {to} - unsupported currency pair";
    }
}
```

## Advanced Patterns

### Multi-Tool Coordination

```csharp
// Implement coordinated tool usage for complex tasks
var coordinatedAgent = new CoordinatedReActAgent
{
    ToolCoordinationStrategy = new SequentialCoordinationStrategy
    {
        MaxParallelTools = 2,
        CoordinationRules = new Dictionary<string, string[]>
        {
            ["data_analysis"] = new[] { "data_clean", "data_transform", "data_analyze" },
            ["report_generation"] = new[] { "data_analyze", "format_report", "validate_report" }
        }
    },
    FallbackStrategy = new FallbackStrategy
    {
        PrimaryTools = new[] { "primary_tool" },
        BackupTools = new[] { "backup_tool" },
        RetryAttempts = 3
    }
};

// Execute coordinated tool usage
var coordinatedResult = await coordinatedAgent.ExecuteAsync(kernel, coordinatedArgs);
```

### Adaptive Reasoning

```csharp
// Implement adaptive reasoning based on task complexity
var adaptiveAgent = new AdaptiveReActAgent
{
    ReasoningStrategies = new Dictionary<string, IReasoningStrategy>
    {
        ["simple"] = new SimpleReasoningStrategy { MaxSteps = 2 },
        ["moderate"] = new ModerateReasoningStrategy { MaxSteps = 4 },
        ["complex"] = new ComplexReasoningStrategy { MaxSteps = 6 }
    },
    ComplexityAnalyzer = new TaskComplexityAnalyzer
    {
        ComplexityMetrics = new[] { "query_length", "tool_count", "domain_specificity" },
        Thresholds = new Dictionary<string, double>
        {
            ["simple"] = 0.3,
            ["moderate"] = 0.7,
            ["complex"] = 1.0
        }
    }
};

// Automatically select reasoning strategy
var strategy = adaptiveAgent.SelectReasoningStrategy(userQuery);
var adaptiveResult = await adaptiveAgent.ExecuteAsync(kernel, args, strategy);
```

### Tool Performance Optimization

```csharp
// Implement tool performance optimization
var optimizedAgent = new OptimizedReActAgent
{
    ToolPerformanceTracker = new ToolPerformanceTracker
    {
        PerformanceMetrics = new Dictionary<string, ToolMetrics>(),
        OptimizationThreshold = TimeSpan.FromSeconds(2)
    },
    ToolSelectionOptimizer = new ToolSelectionOptimizer
    {
        SelectionCriteria = new[] { "accuracy", "speed", "reliability" },
        WeightedScoring = true,
        HistoricalPerformanceWeight = 0.7
    }
};

// Track and optimize tool performance
await optimizedAgent.TrackToolPerformanceAsync("currency_convert", executionTime);
var optimizedTools = await optimizedAgent.GetOptimizedToolSetAsync();
```

## Expected Output

The example produces comprehensive output showing:

- 🧑‍💻 User queries and agent reasoning
- 🤖 Intelligent action selection and tool execution
- 📊 Tool parameter extraction and validation
- 🔄 ReAct loop execution (Reason → Act → Observe)
- ➕ Tool extensibility demonstration
- ✅ Complete ReAct agent workflow execution

## Troubleshooting

### Common Issues

1. **Tool Discovery Failures**: Ensure tools are properly registered with Semantic Kernel
2. **Parameter Validation Errors**: Check tool parameter types and validation rules
3. **Action Selection Issues**: Verify tool descriptions and function attributes
4. **Execution Failures**: Monitor tool execution and error handling

### Debugging Tips

- Enable detailed logging to trace ReAct loop execution
- Verify tool registration and discovery in the kernel
- Check parameter mapping between reasoning and action nodes
- Monitor action selection criteria and tool matching

## See Also

- [ReAct Patterns](../patterns/react.md)
- [Action Nodes](../concepts/node-types.md)
- [Tool Integration](../how-to/tools.md)
- [Agent Patterns](../patterns/agent-patterns.md)
- [Function Nodes](../concepts/node-types.md)
