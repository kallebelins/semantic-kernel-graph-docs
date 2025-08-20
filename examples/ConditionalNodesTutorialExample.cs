using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;
using System.Text.Json;

namespace Examples
{
    /// <summary>
    /// Example demonstrating conditional nodes and edges in SemanticKernel.Graph
    /// This example shows how to create dynamic workflows that make decisions
    /// and route execution based on state and AI responses.
    /// </summary>
    public static class ConditionalNodesTutorialExample
    {
        private static string? openAiApiKey;
        private static string? openAiModel;

        static ConditionalNodesTutorialExample()
        {
            // Load configuration from appsettings.json (optional)
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
                .Build();

            openAiApiKey = configuration["OpenAI:ApiKey"];
            openAiModel = configuration["OpenAI:Model"] ?? "gpt-3.5-turbo";
        }

        /// <summary>
        /// Demonstrates a complete conditional workflow with complexity-based routing
        /// and urgency checking
        /// </summary>
        /// <param name="kernel">The configured semantic kernel instance</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public static async Task RunConditionalWorkflowExample(Kernel kernel)
        {
            Console.WriteLine("\n=== Conditional Workflow Example ===\n");

            // 1. Input analysis node - analyzes customer requests and provides structured output
            var inputNode = new FunctionGraphNode(
                kernel.CreateFunctionFromPrompt(@"
                    Analyze this customer request: {{$customerRequest}}
                    
                    Provide analysis in JSON format:
                    {
                        ""complexity"": ""simple|moderate|complex"",
                        ""urgency"": ""low|medium|high"",
                        ""category"": ""technical|billing|general"",
                        ""estimatedTime"": ""minutes|hours|days""
                    }
                ")
            ).StoreResultAs("analysis");

            // 2. Complexity-based routing - routes requests based on complexity level
            var isSimpleNode = new ConditionalGraphNode(
                state => 
                {
                    var analysis = state.GetValue<string>("analysis") ?? "";
                    return analysis.Contains("\"complexity\": \"simple\"");
                },
                "IsSimple"
            );

            var isModerateNode = new ConditionalGraphNode(
                state => 
                {
                    var analysis = state.GetValue<string>("analysis") ?? "";
                    return analysis.Contains("\"complexity\": \"moderate\"");
                },
                "IsModerate"
            );

            var isComplexNode = new ConditionalGraphNode(
                state => 
                {
                    var analysis = state.GetValue<string>("analysis") ?? "";
                    return analysis.Contains("\"complexity\": \"complex\"");
                },
                "IsComplex"
            );

            // 3. Simple request handler - handles straightforward requests
            var simpleHandler = new FunctionGraphNode(
                kernel.CreateFunctionFromPrompt(@"
                    Handle this simple request: {{$customerRequest}}
                    Analysis: {{$analysis}}
                    
                    Provide a direct, helpful response that resolves the issue.
                ")
            ).StoreResultAs("response");

            // 4. Moderate request handler - handles moderately complex requests
            var moderateHandler = new FunctionGraphNode(
                kernel.CreateFunctionFromPrompt(@"
                    Handle this moderate complexity request: {{$customerRequest}}
                    Analysis: {{$analysis}}
                    
                    Provide a detailed response with steps and resources.
                    Include any necessary follow-up actions.
                ")
            ).StoreResultAs("response");

            // 5. Complex request handler - handles complex requests requiring detailed analysis
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

            // 6. Urgency check - determines if a request requires immediate attention
            var urgencyCheck = new ConditionalGraphNode(
                state => 
                {
                    var analysis = state.GetValue<string>("analysis") ?? "";
                    return analysis.Contains("\"urgency\": \"high\"");
                },
                "CheckUrgency"
            );

            // 7. High urgency handler - enhances responses for urgent requests
            var highUrgencyHandler = new FunctionGraphNode(
                kernel.CreateFunctionFromPrompt(@"
                    This is a HIGH URGENCY request: {{$customerRequest}}
                    Current response: {{$response}}
                    
                    Enhance the response to emphasize urgency and provide immediate action items.
                    Include escalation contact information.
                ")
            ).StoreResultAs("finalResponse");

            // 8. Final response formatter - creates the final formatted response
            var responseFormatter = new FunctionGraphNode(
                kernel.CreateFunctionFromPrompt(@"
                    Format the final response for the customer:
                    Request: {{$customerRequest}}
                    Response: {{$finalResponse}}
                    Analysis: {{$analysis}}
                    
                    Create a professional, well-structured response that addresses all aspects.
                ")
            ).StoreResultAs("formattedResponse");

            // Build the graph with all nodes
            var graph = new GraphExecutor("ConditionalWorkflowExample");

            // Add all nodes to the graph
            graph.AddNode(inputNode);
            graph.AddNode(isSimpleNode);
            graph.AddNode(isModerateNode);
            graph.AddNode(isComplexNode);
            graph.AddNode(simpleHandler);
            graph.AddNode(moderateHandler);
            graph.AddNode(complexHandler);
            graph.AddNode(urgencyCheck);
            graph.AddNode(highUrgencyHandler);
            graph.AddNode(responseFormatter);

            // Connect the workflow with conditional routing
            graph.Connect(inputNode, isSimpleNode);
            graph.Connect(inputNode, isModerateNode);
            graph.Connect(inputNode, isComplexNode);
            
            // Complexity-based routing connections
            graph.ConnectWhen(isSimpleNode.NodeId, simpleHandler.NodeId, 
                args => isSimpleNode.Condition(args.GetOrCreateGraphState()));
            graph.ConnectWhen(isModerateNode.NodeId, moderateHandler.NodeId, 
                args => isModerateNode.Condition(args.GetOrCreateGraphState()));
            graph.ConnectWhen(isComplexNode.NodeId, complexHandler.NodeId, 
                args => isComplexNode.Condition(args.GetOrCreateGraphState()));
            
            // All paths converge to urgency check
            graph.Connect(simpleHandler, urgencyCheck);
            graph.Connect(moderateHandler, urgencyCheck);
            graph.Connect(complexHandler, urgencyCheck);
            
            // Urgency-based routing
            graph.ConnectWhen(urgencyCheck.NodeId, highUrgencyHandler.NodeId, 
                args => urgencyCheck.Condition(args.GetOrCreateGraphState()));
            graph.ConnectWhen(urgencyCheck.NodeId, responseFormatter.NodeId, 
                args => !urgencyCheck.Condition(args.GetOrCreateGraphState()));
            
            // High urgency path continues to response formatter
            graph.Connect(highUrgencyHandler, responseFormatter);
            
            // Set the starting node
            graph.SetStartNode(inputNode.NodeId);

            // Test with different types of requests
            var testRequests = new[]
            {
                "I can't log into my account", // Simple request
                "I need help setting up advanced security features", // Moderate complexity
                "My entire system is down and I have a critical presentation in 2 hours" // Complex + High urgency
            };

            // Execute the workflow for each test request
            foreach (var request in testRequests)
            {
                Console.WriteLine($"\n--- Testing: {request} ---");
                
                var state = new KernelArguments { ["customerRequest"] = request };
                var result = await graph.ExecuteAsync(kernel, state);
                
                // Access the result values from the arguments
                var analysis = state.GetValueOrDefault("analysis", "No analysis");
                var formattedResponse = state.GetValueOrDefault("formattedResponse", "No response");
                
                Console.WriteLine($"Analysis: {analysis}");
                Console.WriteLine($"Response: {formattedResponse}");
            }
        }

        /// <summary>
        /// Demonstrates basic conditional node patterns and simple routing logic
        /// </summary>
        /// <param name="kernel">The configured semantic kernel instance</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public static async Task RunBasicConditionalPatterns(Kernel kernel)
        {
            Console.WriteLine("\n=== Basic Conditional Patterns ===\n");

            // Create a simple conditional node that checks sentiment
            var isPositiveNode = new ConditionalGraphNode(
                state => 
                {
                    var sentiment = state.GetValue<string>("sentiment") ?? "neutral";
                    return sentiment.ToLower().Contains("positive");
                },
                "IsPositive"
            );

            // Create response nodes
            var positiveResponseNode = new FunctionGraphNode(
                kernel.CreateFunctionFromPrompt("Generate a positive response for: {{$input}}")
            ).StoreResultAs("output");

            var negativeResponseNode = new FunctionGraphNode(
                kernel.CreateFunctionFromPrompt("Generate a helpful response for: {{$input}}")
            ).StoreResultAs("output");

            // Create input node
            var inputNode = new FunctionGraphNode(
                kernel.CreateFunctionFromPrompt("Analyze the sentiment of: {{$input}}")
            ).StoreResultAs("sentiment");

            // Build the basic graph
            var graph = new GraphExecutor("BasicConditionalExample");

            // Add nodes
            graph.AddNode(inputNode);
            graph.AddNode(isPositiveNode);
            graph.AddNode(positiveResponseNode);
            graph.AddNode(negativeResponseNode);

            // Connect with conditional routing
            graph.Connect(inputNode, isPositiveNode);
            graph.ConnectWhen(isPositiveNode.NodeId, positiveResponseNode.NodeId, 
                args => isPositiveNode.Condition(args.GetOrCreateGraphState()));
            graph.ConnectWhen(isPositiveNode.NodeId, negativeResponseNode.NodeId, 
                args => !isPositiveNode.Condition(args.GetOrCreateGraphState()));

            graph.SetStartNode(inputNode.NodeId);

            // Test the basic conditional logic
            var testInputs = new[] { "I love this product!", "This is terrible", "It's okay" };
            
            foreach (var input in testInputs)
            {
                Console.WriteLine($"\nInput: {input}");
                var state = new KernelArguments { ["input"] = input };
                var result = await graph.ExecuteAsync(kernel, state);
                
                // Access the result values from the arguments
                var output = state.GetValueOrDefault("output", "No output");
                Console.WriteLine($"Output: {output}");
            }
        }

        /// <summary>
        /// Demonstrates advanced conditional edge patterns with complex logic
        /// </summary>
        /// <param name="kernel">The configured semantic kernel instance</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public static async Task RunAdvancedConditionalEdgePatterns(Kernel kernel)
        {
            Console.WriteLine("\n=== Advanced Conditional Edge Patterns ===\n");

            // Create a decision node that analyzes the situation
            var decisionNode = new FunctionGraphNode(
                kernel.CreateFunctionFromPrompt(@"
                    Based on this situation: {{$context}}
                    And the current state: {{$currentState}}
                    
                    Decide what action to take next. Return only one of these options:
                    - ""continue"" - if we should proceed normally
                    - ""escalate"" - if this needs human intervention
                    - ""retry"" - if we should try again with different parameters
                    - ""abort"" - if we should stop the workflow
                    
                    Explain your reasoning briefly.
                ")
            ).StoreResultAs("decision");

            // Create conditional nodes for different actions
            var shouldContinueNode = new ConditionalGraphNode(
                state => 
                {
                    var decision = state.GetValue<string>("decision") ?? "";
                    return decision.Contains("continue");
                },
                "ShouldContinue"
            );

            var shouldEscalateNode = new ConditionalGraphNode(
                state => 
                {
                    var decision = state.GetValue<string>("decision") ?? "";
                    return decision.Contains("escalate");
                },
                "ShouldEscalate"
            );

            var shouldRetryNode = new ConditionalGraphNode(
                state => 
                {
                    var decision = state.GetValue<string>("decision") ?? "";
                    return decision.Contains("retry");
                },
                "ShouldRetry"
            );

            var shouldAbortNode = new ConditionalGraphNode(
                state => 
                {
                    var decision = state.GetValue<string>("decision") ?? "";
                    return decision.Contains("abort");
                },
                "ShouldAbort"
            );

            // Create handler nodes for different actions
            var continueHandler = new FunctionGraphNode(
                kernel.CreateFunctionFromPrompt("Continue processing: {{$context}}")
            ).StoreResultAs("action");

            var escalateHandler = new FunctionGraphNode(
                kernel.CreateFunctionFromPrompt("Escalate this issue: {{$context}}")
            ).StoreResultAs("action");

            var retryHandler = new FunctionGraphNode(
                kernel.CreateFunctionFromPrompt("Retry with different approach: {{$context}}")
            ).StoreResultAs("action");

            var abortHandler = new FunctionGraphNode(
                kernel.CreateFunctionFromPrompt("Abort workflow: {{$context}}")
            ).StoreResultAs("action");

            // Build the advanced conditional graph
            var graph = new GraphExecutor("AdvancedConditionalExample");

            // Add all nodes
            graph.AddNode(decisionNode);
            graph.AddNode(shouldContinueNode);
            graph.AddNode(shouldEscalateNode);
            graph.AddNode(shouldRetryNode);
            graph.AddNode(shouldAbortNode);
            graph.AddNode(continueHandler);
            graph.AddNode(escalateHandler);
            graph.AddNode(retryHandler);
            graph.AddNode(abortHandler);

            // Connect with complex conditional routing
            graph.Connect(decisionNode, shouldContinueNode);
            graph.Connect(decisionNode, shouldEscalateNode);
            graph.Connect(decisionNode, shouldRetryNode);
            graph.Connect(decisionNode, shouldAbortNode);
            
            // Route to appropriate handlers based on decision
            graph.ConnectWhen(shouldContinueNode.NodeId, continueHandler.NodeId, 
                args => shouldContinueNode.Condition(args.GetOrCreateGraphState()));
            graph.ConnectWhen(shouldEscalateNode.NodeId, escalateHandler.NodeId, 
                args => shouldEscalateNode.Condition(args.GetOrCreateGraphState()));
            graph.ConnectWhen(shouldRetryNode.NodeId, retryHandler.NodeId, 
                args => shouldRetryNode.Condition(args.GetOrCreateGraphState()));
            graph.ConnectWhen(shouldAbortNode.NodeId, abortHandler.NodeId, 
                args => shouldAbortNode.Condition(args.GetOrCreateGraphState()));

            graph.SetStartNode(decisionNode.NodeId);

            // Test with different scenarios
            var testScenarios = new[]
            {
                new { context = "Simple task completed successfully", currentState = "normal" },
                new { context = "Critical error occurred", currentState = "error" },
                new { context = "Timeout on external service", currentState = "retry" },
                new { context = "Invalid input data", currentState = "invalid" }
            };

            foreach (var scenario in testScenarios)
            {
                Console.WriteLine($"\n--- Scenario: {scenario.context} ---");
                var state = new KernelArguments 
                { 
                    ["context"] = scenario.context,
                    ["currentState"] = scenario.currentState
                };
                
                var result = await graph.ExecuteAsync(kernel, state);
                
                // Access the result values from the arguments
                var decision = state.GetValueOrDefault("decision", "No decision");
                var action = state.GetValueOrDefault("action", "No action");
                
                Console.WriteLine($"Decision: {decision}");
                Console.WriteLine($"Action: {action}");
            }
        }

        /// <summary>
        /// Runs all conditional nodes examples
        /// </summary>
        /// <param name="kernel">The configured semantic kernel instance</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public static async Task RunAllExamples()
        {
            // Create kernel with basic configuration
            var kernel = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion(openAiModel!, openAiApiKey)
                .AddGraphSupport()
                .Build();

            // Create a comprehensive conditional workflow example
            await RunConditionalWorkflowExample(kernel);
            
            // Demonstrate basic conditional patterns
            await RunBasicConditionalPatterns(kernel);
            
            // Show advanced conditional edge patterns
            await RunAdvancedConditionalEdgePatterns(kernel);
        }
    }
}
