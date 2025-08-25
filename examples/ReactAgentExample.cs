using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.Extensions;

namespace Examples
{
    /// <summary>
    /// Simple ReAct (Reason -> Act -> Observe) agent example adapted from documentation.
    /// The example is deterministic (no external LLM calls) and uses method-based
    /// kernel functions so it can run locally for testing and documentation purposes.
    /// </summary>
    public static class ReactAgentExample
    {
        /// <summary>
        /// Entry point to run the React agent example. Creates a kernel, registers
        /// basic tools, builds the agent executor and runs a few sample queries.
        /// </summary>
        public static async Task RunAsync()
        {
            Console.WriteLine("🎯 Running ReAct Agent Example (documentation runnable)\n");

            // Create a minimal kernel with graph support. No LLM provider is configured
            // so the example only uses method-based functions (deterministic and offline).
            var kernel = Kernel.CreateBuilder()
                .AddGraphSupport()
                .Build();

            // Register a small set of mock tools that the ActionGraphNode can discover
            RegisterBasicTools(kernel);

            // Build the simple ReAct agent executor
            var executor = CreateSimpleReActAgent(kernel);

            // Sample queries to demonstrate reasoning -> action -> observation flow
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

                try
                {
                    var result = await executor.ExecuteAsync(kernel, args);
                    var answer = result.GetValue<string>() ?? "No answer produced";
                    Console.WriteLine($"🤖 Agent: {answer}\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Execution error: {ex.Message}\n");
                }

                await Task.Delay(200);
            }

            // Demonstrate extensibility by adding a currency conversion tool at runtime
            Console.WriteLine("➕ Adding currency conversion tool and running an extended query...\n");
            AddCurrencyConversionTool(kernel);

            var extendedQuery = "Convert 100 USD to EUR";
            Console.WriteLine($"🧑‍💻 User: {extendedQuery}");
            var extendedArgs = new KernelArguments { ["user_query"] = extendedQuery };

            try
            {
                var extendedResult = await executor.ExecuteAsync(kernel, extendedArgs);
                Console.WriteLine($"🤖 Agent: {extendedResult.GetValue<string>() ?? "No answer produced"}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Extended execution error: {ex.Message}\n");
            }

            Console.WriteLine("✅ ReAct Agent Example finished.");
        }

        /// <summary>
        /// Registers a few simple tools as plugin objects. Methods are intentionally
        /// simple and well-commented for documentation and testing.
        /// </summary>
        private static void RegisterBasicTools(Kernel kernel)
        {
            // Importing plugin objects enables ActionGraphNode to discover their public methods
            kernel.ImportPluginFromObject(new WeatherTool());
            kernel.ImportPluginFromObject(new CalculatorTool());
            kernel.ImportPluginFromObject(new SearchTool());
        }

        /// <summary>
        /// Adds a currency conversion utility at runtime to demonstrate extensibility.
        /// </summary>
        private static void AddCurrencyConversionTool(Kernel kernel)
        {
            kernel.ImportPluginFromObject(new CurrencyConversionTool());
        }

        /// <summary>
        /// Creates a minimal GraphExecutor implementing a Reason -> Act -> Observe flow.
        /// The ActionGraphNode is configured to auto-discover available tool functions.
        /// </summary>
        private static GraphExecutor CreateSimpleReActAgent(Kernel kernel)
        {
            if (kernel is null) throw new ArgumentNullException(nameof(kernel));

            var reasoningNode = new FunctionGraphNode(
                CreateReasoningFunction(kernel),
                "react_reason",
                "Analyze the user query and suggest an action"
            );

            var actionsNode = ActionGraphNode.CreateWithActions(
                kernel,
                new ActionSelectionCriteria
                {
                    MinRequiredParameters = 0,
                    MaxRequiredParameters = 5
                },
                "react_act");

            actionsNode.ConfigureExecution(ActionSelectionStrategy.Intelligent, enableParameterValidation: true);

            var observeNode = new FunctionGraphNode(
                CreateObservationFunction(kernel),
                "react_observe",
                "Summarize action result as a final answer"
            ).StoreResultAs("final_answer");

            var executor = new GraphExecutor("SimpleReActAgent", "Minimal ReAct agent with extensible tools");
            executor.AddNode(reasoningNode);
            executor.AddNode(actionsNode);
            executor.AddNode(observeNode);

            executor.SetStartNode(reasoningNode.NodeId);
            executor.AddEdge(ConditionalEdge.CreateUnconditional(reasoningNode, actionsNode));
            executor.AddEdge(ConditionalEdge.CreateUnconditional(actionsNode, observeNode));

            return executor;
        }

        /// <summary>
        /// Creates a reasoning KernelFunction that inspects the user query and suggests
        /// a tool/action name plus a dictionary of parameters to run the action with.
        /// </summary>
        private static KernelFunction CreateReasoningFunction(Kernel kernel)
        {
            return kernel.CreateFunctionFromMethod(
                (KernelArguments args) =>
                {
                    var query = args.TryGetValue("user_query", out var q) ? q?.ToString() ?? string.Empty : string.Empty;

                    // Determine suggested action based on simple heuristics over the query text
                    var action = query.ToLowerInvariant() switch
                    {
                        var s when s.Contains("weather") => "GetWeather",
                        var s when s.Contains("calculate") || s.Contains("*") || s.Contains("+") || s.Contains("-") => "Calculate",
                        var s when s.Contains("search") || s.Contains("best practices") => "Search",
                        var s when s.Contains("convert") && s.Contains("currency") => "ConvertCurrency",
                        _ => "Search"
                    };

                    // Basic parameter extraction for demo purposes
                    var parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                    if (action == "GetWeather")
                    {
                        var cityMatch = System.Text.RegularExpressions.Regex.Match(query, @"in ([A-Za-z]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        if (cityMatch.Success) parameters["city"] = cityMatch.Groups[1].Value;
                    }

                    if (action == "Calculate")
                    {
                        var calcMatch = System.Text.RegularExpressions.Regex.Match(query, @"(\d+\s*[\*\+\-]\s*\d+)");
                        if (calcMatch.Success) parameters["expression"] = calcMatch.Groups[1].Value;
                    }

                    if (action == "Search")
                    {
                        parameters["query"] = query.Replace("Search:", "", StringComparison.OrdinalIgnoreCase).Trim();
                    }

                    // Store suggested action and parameters into args for downstream nodes
                    args["suggested_action"] = action;
                    args["action_parameters"] = parameters;

                    return $"Reasoning: suggested action='{action}' parameters=[{string.Join(',', parameters.Select(kv => kv.Key + "=" + kv.Value))}]";
                },
                functionName: "react_reasoning",
                description: "Analyzes user queries and suggests appropriate actions"
            );
        }

        /// <summary>
        /// Creates an observation KernelFunction that formats the action result into the
        /// final answer returned to the caller.
        /// </summary>
        private static KernelFunction CreateObservationFunction(Kernel kernel)
        {
            return kernel.CreateFunctionFromMethod(
                (KernelArguments args) =>
                {
                    var action = args.TryGetValue("suggested_action", out var a) ? a?.ToString() ?? string.Empty : string.Empty;
                    var result = args.TryGetValue("action_result", out var r) ? r?.ToString() ?? string.Empty : string.Empty;

                    var answer = action switch
                    {
                        "GetWeather" => $"Based on your query about weather, I found: {result}",
                        "Calculate" => $"I calculated the result for you: {result}",
                        "Search" => $"Here's what I found when searching: {result}",
                        "ConvertCurrency" => $"I converted the currency for you: {result}",
                        _ => $"I processed your request and here's what I found: {result}"
                    };

                    args["final_answer"] = answer;
                    return answer;
                },
                functionName: "react_observation",
                description: "Summarizes action results into final answers"
            );
        }

        #region Tool Implementations

        /// <summary>
        /// Simple weather tool for documentation/testing. Public methods are discoverable
        /// by the kernel when the object is imported as a plugin.
        /// </summary>
        public class WeatherTool
        {
            [KernelFunction]
            public string GetWeather(string city)
            {
                var weather = city?.ToLowerInvariant() switch
                {
                    "lisbon" => "Sunny, 22°C, light breeze",
                    "london" => "Cloudy, 15°C, light rain",
                    "paris" => "Partly cloudy, 18°C, calm",
                    _ => $"Weather data unavailable for {city}"
                };

                return $"Current weather in {city}: {weather}";
            }
        }

        /// <summary>
        /// Minimal calculator tool. Only supports simple multiplication expressions like "42 * 7".
        /// </summary>
        public class CalculatorTool
        {
            [KernelFunction]
            public string Calculate(string expression)
            {
                try
                {
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
                if (string.IsNullOrWhiteSpace(expression)) throw new ArgumentException("Expression is empty", nameof(expression));
                if (expression.Contains("*"))
                {
                    var parts = expression.Split('*');
                    if (parts.Length == 2 && double.TryParse(parts[0], out var a) && double.TryParse(parts[1], out var b))
                        return a * b;
                }

                throw new ArgumentException("Unsupported expression format. Supported: A * B");
            }
        }

        /// <summary>
        /// Simple search tool that returns canned responses for demo queries.
        /// </summary>
        public class SearchTool
        {
            [KernelFunction]
            public string Search(string query)
            {
                var q = query?.ToLowerInvariant() ?? string.Empty;

                if (q.Contains("c#") && q.Contains("logging"))
                {
                    return "C# logging best practices: Use ILogger<T>, structured logging, log levels, and centralized configuration.";
                }

                if (q.Contains("best practices"))
                {
                    return "General best practices: Follow established patterns, document code, test thoroughly, and maintain consistency.";
                }

                return $"Search results for '{query}': Multiple relevant sources found.";
            }
        }

        /// <summary>
        /// Currency conversion tool used to demonstrate adding new tools at runtime.
        /// </summary>
        public class CurrencyConversionTool
        {
            [KernelFunction]
            public string ConvertCurrency(double amount, string from, string to)
            {
                var rates = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
                {
                    ["USD"] = 1.0,
                    ["EUR"] = 0.85,
                    ["GBP"] = 0.73,
                    ["JPY"] = 110.0
                };

                if (rates.TryGetValue(from, out var fromRate) && rates.TryGetValue(to, out var toRate))
                {
                    var converted = amount * (toRate / fromRate);
                    return $"{amount} {from.ToUpper()} = {converted:F2} {to.ToUpper()}";
                }

                return $"Unable to convert {amount} {from} to {to} - unsupported currency pair";
            }
        }

        #endregion
    }
}


