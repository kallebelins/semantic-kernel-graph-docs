using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.Execution;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SemanticKernel.Graph.Examples
{
    /// <summary>
    /// Example demonstrating ReAct (Reasoning + Acting) loops and Chain of Thought reasoning
    /// using SemanticKernel.Graph. This example shows how to build intelligent agents
    /// that can think, act, and learn from their actions.
    /// </summary>
    public class ReactCotQuickstartExample
    {
        private readonly Kernel _kernel;
        private readonly GraphExecutor _simpleReActExecutor;
        private readonly GraphExecutor _advancedReActExecutor;
        private readonly GraphExecutor _chainOfThoughtExecutor;

        /// <summary>
        /// Initializes the example with a configured kernel and creates the graph executors
        /// </summary>
        /// <param name="kernel">The configured Semantic Kernel instance</param>
        public ReactCotQuickstartExample(Kernel kernel)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
            
            // Add mock actions to the kernel first, before creating any ActionGraphNodes
            AddMockActionsToKernel();
            
            // Create the executors for different examples
            _simpleReActExecutor = CreateSimpleReActExecutor();
            _advancedReActExecutor = CreateAdvancedReActExecutor();
            _chainOfThoughtExecutor = CreateChainOfThoughtExecutor();
        }

        /// <summary>
        /// Creates a simple ReAct loop with three core components: reasoning, action, and observation
        /// </summary>
        /// <returns>A configured GraphExecutor with a basic ReAct reasoning loop</returns>
        private GraphExecutor CreateSimpleReActExecutor()
        {
            // Create the reasoning node that analyzes user queries and suggests actions
            var reasoningNode = new FunctionGraphNode(
                CreateReasoningFunction(_kernel),
                "reasoning",
                "Problem Analysis");

            // Create the action execution node using CreateWithActions to auto-discover kernel functions
            var actionNode = ActionGraphNode.CreateWithActions(
                _kernel,
                new ActionSelectionCriteria
                {
                    // Keep it open to discover all available functions
                    MinRequiredParameters = 0,
                    MaxRequiredParameters = 5
                },
                "action");
            actionNode.ConfigureExecution(ActionSelectionStrategy.Intelligent, enableParameterValidation: true);

            // ActionGraphNode configured with available actions

            // Create the observation node that analyzes results and provides final answers
            var observationNode = new FunctionGraphNode(
                CreateObservationFunction(_kernel),
                "observation",
                "Result Analysis");

            // Configure nodes to store results for downstream consumption
            reasoningNode.StoreResultAs("reasoning_result");
            // ActionGraphNode automatically stores result as "action_result"
            observationNode.StoreResultAs("final_answer");

            // Build the ReAct loop by connecting the nodes in sequence
            var executor = new GraphExecutor("SimpleReAct", "Basic ReAct reasoning loop");
            executor.AddNode(reasoningNode)
                    .AddNode(actionNode)
                    .AddNode(observationNode)
                    .Connect("reasoning", "action")
                    .Connect("action", "observation")
                    .SetStartNode("reasoning");

            return executor;
        }

        /// <summary>
        /// Creates an advanced ReAct loop using specialized reasoning nodes and loop control
        /// </summary>
        /// <returns>A configured GraphExecutor with advanced ReAct capabilities</returns>
        private GraphExecutor CreateAdvancedReActExecutor()
        {
            // Use a function-based reasoning node to avoid LLM dependency
            // Create advanced reasoning node that provides more sophisticated analysis
            var reasoningNode = new FunctionGraphNode(
                CreateAdvancedReasoningFunction(_kernel),
                "advanced_reasoning",
                "Advanced Problem Analysis");

            // Create action execution node using CreateWithActions to auto-discover kernel functions
            var actionNode = ActionGraphNode.CreateWithActions(
                _kernel,
                new ActionSelectionCriteria
                {
                    MinRequiredParameters = 0,
                    MaxRequiredParameters = 5
                },
                "advanced_action");
            actionNode.ConfigureExecution(ActionSelectionStrategy.Intelligent, enableParameterValidation: true);

            // Create advanced observation node using a function to avoid LLM dependency
            var observationNode = new FunctionGraphNode(
                CreateAdvancedObservationFunction(_kernel),
                "advanced_observation",
                "Advanced Result Analysis");

            // Configure nodes to store results for downstream consumption
            reasoningNode.StoreResultAs("advanced_reasoning_result");
            observationNode.StoreResultAs("advanced_final_answer");

            // Build the advanced ReAct loop by connecting the nodes in sequence
            var executor = new GraphExecutor("AdvancedReAct", "Advanced ReAct reasoning agent");
            executor.AddNode(reasoningNode)
                    .AddNode(actionNode)
                    .AddNode(observationNode)
                    .Connect("advanced_reasoning", "advanced_action")
                    .Connect("advanced_action", "advanced_observation")
                    .SetStartNode("advanced_reasoning");

            return executor;
        }

        /// <summary>
        /// Creates a Chain of Thought executor for step-by-step reasoning
        /// </summary>
        /// <returns>A configured GraphExecutor with Chain of Thought capabilities</returns>
        private GraphExecutor CreateChainOfThoughtExecutor()
        {
            // Create a Chain of Thought function that performs step-by-step reasoning
            var cotNode = new FunctionGraphNode(
                CreateChainOfThoughtFunction(_kernel),
                "chain_of_thought",
                "Step-by-step Problem Solving");

            // Configure to store the result
            cotNode.StoreResultAs("chain_of_thought_result");

            // Build the executor with the Chain of Thought node
            var executor = new GraphExecutor("ChainOfThought", "Chain-of-Thought reasoning example");
            executor.AddNode(cotNode);
            executor.SetStartNode(cotNode.NodeId);

            return executor;
        }

        /// <summary>
        /// Creates a Chain of Thought function for step-by-step problem solving
        /// </summary>
        /// <param name="kernel">The kernel instance for function creation</param>
        /// <returns>A configured KernelFunction for Chain of Thought reasoning</returns>
        private static KernelFunction CreateChainOfThoughtFunction(Kernel kernel)
        {
            return kernel.CreateFunctionFromMethod(
                (KernelArguments args) =>
                {
                    var problemStatement = args.GetValueOrDefault("problem_statement", "A company needs to reduce operational costs by 20% while maintaining employee satisfaction.")?.ToString() ?? "No problem provided";
                    var context = args.GetValueOrDefault("context", "The company operates in a competitive tech market with high talent retention challenges.")?.ToString() ?? "No context provided";
                    var constraints = args.GetValueOrDefault("constraints", "Cannot reduce headcount by more than 5%, must maintain current benefit levels.")?.ToString() ?? "No constraints provided";
                    
                    // Step-by-step Chain of Thought reasoning
                    var step1 = $"Step 1 - Problem Analysis:\nThe core challenge is: {problemStatement}\nThis requires balancing cost reduction with employee satisfaction.";
                    
                    var step2 = $"Step 2 - Context Evaluation:\nGiven context: {context}\nKey insight: In a competitive market, employee retention is crucial for long-term success.";
                    
                    var step3 = $"Step 3 - Constraint Assessment:\nOperational constraints: {constraints}\nThis means we need creative solutions beyond simple cost-cutting.";
                    
                    var step4 = $"Step 4 - Solution Framework:\nPotential approaches:\n- Process automation to reduce operational overhead\n- Renegotiate vendor contracts for better rates\n- Implement energy-saving initiatives\n- Optimize office space utilization\n- Streamline workflows to improve efficiency";
                    
                    var step5 = $"Step 5 - Final Recommendation:\nBased on the analysis, I recommend a three-pronged approach:\n1. Automate repetitive processes (15% cost reduction)\n2. Renegotiate key vendor contracts (3% cost reduction)\n3. Implement energy efficiency measures (2% cost reduction)\nThis achieves the 20% target while preserving jobs and benefits.";
                    
                    var fullReasoning = $"{step1}\n\n{step2}\n\n{step3}\n\n{step4}\n\n{step5}";
                    
                    // Store reasoning steps for inspection
                    args["reasoning_steps"] = new List<string> { step1, step2, step3, step4, step5 };
                    args["final_answer"] = step5;
                    args["reasoning_quality"] = 0.95; // High quality reasoning
                    args["steps_taken"] = 5;
                    
                    return fullReasoning;
                },
                functionName: "chain_of_thought_reasoning",
                description: "Performs step-by-step Chain of Thought reasoning for complex problem solving"
            );
        }

        /// <summary>
        /// Creates a reasoning function that analyzes user queries and suggests appropriate actions
        /// </summary>
        /// <param name="kernel">The kernel instance for function creation</param>
        /// <returns>A configured KernelFunction for reasoning</returns>
        private static KernelFunction CreateReasoningFunction(Kernel kernel)
        {
            return kernel.CreateFunctionFromMethod(
                (KernelArguments args) =>
                {
                    var query = args["user_query"]?.ToString() ?? string.Empty;
                    
                    // Simple reasoning logic based on query content
                    var suggestedAction = query.ToLowerInvariant() switch
                    {
                        var q when q.Contains("weather") => "get_weather",
                        var q when q.Contains("calculate") => "calculate",
                        var q when q.Contains("search") => "search",
                        _ => "search"
                    };

                    // Store the reasoning results in the arguments for later use
                    args["suggested_action"] = suggestedAction;
                    args["reasoning_result"] = $"Selected action '{suggestedAction}' based on query analysis.";
                    return $"Reasoning completed. Proposed action: {suggestedAction}";
                },
                functionName: "simple_reasoning",
                description: "Analyzes user query and suggests appropriate actions"
            );
        }

        /// <summary>
        /// Creates an advanced reasoning function for business problem analysis
        /// </summary>
        /// <param name="kernel">The kernel instance for function creation</param>
        /// <returns>A configured KernelFunction for advanced reasoning</returns>
        private static KernelFunction CreateAdvancedReasoningFunction(Kernel kernel)
        {
            return kernel.CreateFunctionFromMethod(
                (KernelArguments args) =>
                {
                    var problemTitle = args.GetValueOrDefault("problem_title", "Unknown Problem")?.ToString() ?? "Unknown Problem";
                    var taskDescription = args.GetValueOrDefault("task_description", "No description provided")?.ToString() ?? "No description provided";
                    
                    // Advanced reasoning logic for business problems
                    var suggestedAction = taskDescription.ToLowerInvariant() switch
                    {
                        var desc when desc.Contains("cost") && desc.Contains("reduce") => "analyze_problem",
                        var desc when desc.Contains("budget") => "analyze_problem", 
                        var desc when desc.Contains("performance") => "analyze_problem",
                        var desc when desc.Contains("efficiency") => "analyze_problem",
                        _ => "analyze_problem"
                    };

                    // Generate comprehensive reasoning result
                    var reasoning = $"Advanced Analysis of '{problemTitle}':\n" +
                                  $"1. Problem Context: {taskDescription}\n" +
                                  $"2. Strategic Assessment: This appears to be a {(taskDescription.Contains("cost") ? "cost optimization" : "operational improvement")} challenge.\n" +
                                  $"3. Recommended Approach: Systematic analysis with stakeholder consideration.\n" +
                                  $"4. Next Action: {suggestedAction} - Deep dive into root causes and impact analysis.";

                    // Store the reasoning results in the arguments for later use
                    args["suggested_action"] = suggestedAction;
                    args["reasoning_result"] = reasoning;
                    args["problem_title"] = problemTitle;
                    args["task_description"] = taskDescription;
                    
                    return reasoning;
                },
                functionName: "advanced_reasoning",
                description: "Performs advanced business problem analysis and strategic reasoning"
            );
        }

        /// <summary>
        /// Creates an observation function that analyzes action results and provides final answers
        /// </summary>
        /// <param name="kernel">The kernel instance for function creation</param>
        /// <returns>A configured KernelFunction for observation</returns>
        private static KernelFunction CreateObservationFunction(Kernel kernel)
        {
            return kernel.CreateFunctionFromMethod(
                (KernelArguments args) =>
                {
                    var actionResult = args.GetValueOrDefault("action_result", "No result")?.ToString() ?? "No result";
                    var reasoningResult = args.GetValueOrDefault("reasoning_result", "No reasoning")?.ToString() ?? "No reasoning";
                    
                    // Also try alternative keys that might be used
                    if (actionResult == "No result")
                    {
                        actionResult = args.GetValueOrDefault("result", "No result")?.ToString() ?? "No result";
                    }
                    
                    // Combine reasoning and action results into a comprehensive observation
                    var observation = $"Based on reasoning: {reasoningResult}\n" +
                                    $"Action executed with result: {actionResult}\n" +
                                    $"Task completed successfully.";
                    
                    args["final_answer"] = observation;
                    return observation;
                },
                functionName: "simple_observation",
                description: "Analyzes action results and provides final answer"
            );
        }

        /// <summary>
        /// Creates an advanced observation function with enhanced result analysis
        /// </summary>
        /// <param name="kernel">The kernel instance for function creation</param>
        /// <returns>A configured KernelFunction for advanced observation</returns>
        private static KernelFunction CreateAdvancedObservationFunction(Kernel kernel)
        {
            return kernel.CreateFunctionFromMethod(
                (KernelArguments args) =>
                {
                    var actionResult = args["action_result"]?.ToString() ?? "No result";
                    var reasoningResult = args["reasoning_result"]?.ToString() ?? "No reasoning";
                    var iteration = args.GetValueOrDefault("iteration", 1);
                    
                    // Enhanced observation with iteration tracking and quality assessment
                    var observation = $"Iteration {iteration}:\n" +
                                    $"Reasoning: {reasoningResult}\n" +
                                    $"Action Result: {actionResult}\n" +
                                    $"Quality Assessment: Analyzing result completeness and accuracy...";
                    
                    // Simulate quality scoring
                    var qualityScore = Math.Min(0.9, 0.5 + (Convert.ToInt32(iteration) * 0.1));
                    args["quality_score"] = qualityScore;
                    args["iteration"] = iteration;
                    
                    return observation;
                },
                functionName: "advanced_observation",
                description: "Enhanced observation with quality assessment and iteration tracking"
            );
        }

        /// <summary>
        /// Adds mock actions to the kernel for the ActionGraphNode to discover and execute
        /// </summary>
        private void AddMockActionsToKernel()
        {
            // Check if plugin already exists to avoid duplicates
            if (_kernel.Plugins.Any(p => p.Name == "react_actions"))
            {
                return;
            }

            // Import all functions as a plugin so ActionGraphNode can discover them
            _kernel.ImportPluginFromFunctions("react_actions", "Mock actions for ReAct examples", new[]
            {
                // Weather action
                _kernel.CreateFunctionFromMethod(
                    (KernelArguments args) =>
                    {
                        var location = args.GetValueOrDefault("location", "unknown location");
                        return $"The weather in {location} is sunny with 22°C temperature and light breeze.";
                    },
                    functionName: "get_weather",
                    description: "Gets weather information for a specified location"
                ),

                // Calculator action
                _kernel.CreateFunctionFromMethod(
                    (KernelArguments args) =>
                    {
                        var expression = args.GetValueOrDefault("expression", "0");
                        return $"Calculation result for '{expression}': 42 (mock result)";
                    },
                    functionName: "calculate",
                    description: "Performs mathematical calculations"
                ),

                // Search action
                _kernel.CreateFunctionFromMethod(
                    (KernelArguments args) =>
                    {
                        var query = args.GetValueOrDefault("query", "unknown query");
                        return $"Search results for '{query}': Found 5 relevant articles about the topic.";
                    },
                    functionName: "search",
                    description: "Searches for information on the internet"
                ),

                // Generic action for business problems
                _kernel.CreateFunctionFromMethod(
                    (KernelArguments args) =>
                    {
                        var problem = args.GetValueOrDefault("problem", "unknown problem");
                        return $"Analysis of '{problem}': Identified 3 key areas for improvement with cost reduction potential.";
                    },
                    functionName: "analyze_problem",
                    description: "Analyzes business problems and provides insights"
                ),

                // Solution evaluation action
                _kernel.CreateFunctionFromMethod(
                    (KernelArguments args) =>
                    {
                        var solution = args.GetValueOrDefault("solution", "unknown solution");
                        return $"Evaluation of '{solution}': Feasible solution with 85% success probability and moderate implementation complexity.";
                    },
                    functionName: "evaluate_solution",
                    description: "Evaluates proposed solutions for feasibility and impact"
                )
            });

            // Plugin successfully added
        }

        /// <summary>
        /// Demonstrates the simple ReAct loop execution
        /// </summary>
        /// <param name="userQuery">The user's query to process</param>
        /// <returns>The result of the ReAct loop execution</returns>
        public async Task<string> RunSimpleReActExampleAsync(string userQuery)
        {
            Console.WriteLine($"🤖 Running Simple ReAct Example with query: {userQuery}");
            
            var arguments = new KernelArguments
            {
                ["user_query"] = userQuery,
                ["max_steps"] = 3
            };

            try
            {
                var result = await _simpleReActExecutor.ExecuteAsync(_kernel, arguments);
                var answer = result.GetValue<string>() ?? "No answer produced";
                Console.WriteLine($"✅ Simple ReAct Result: {answer}");
                return answer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Simple ReAct Error: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Demonstrates the advanced ReAct loop execution with business problem solving
        /// </summary>
        /// <returns>The result of the advanced ReAct execution</returns>
        public async Task<string> RunAdvancedReActExampleAsync()
        {
            Console.WriteLine("🚀 Running Advanced ReAct Example");
            
            var arguments = new KernelArguments
            {
                ["problem_title"] = "Budget Planning",
                ["task_description"] = "Our team needs to reduce operational costs by 20% while maintaining service quality. Current monthly spending is $50,000 across 5 departments.",
                ["max_iterations"] = 3,
                ["solver_mode"] = "systematic",
                ["domain"] = "business"
            };

            try
            {
                var result = await _advancedReActExecutor.ExecuteAsync(_kernel, arguments);
                var solution = result.GetValue<string>() ?? "No solution generated";
                Console.WriteLine($"✅ Advanced ReAct Solution: {solution}");
                return solution;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Advanced ReAct Error: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Demonstrates Chain of Thought reasoning for complex problem solving
        /// </summary>
        /// <returns>The result of the Chain of Thought execution</returns>
        public async Task<string> RunChainOfThoughtExampleAsync()
        {
            Console.WriteLine("🧠 Running Chain of Thought Example");
            
            var arguments = new KernelArguments
            {
                ["problem_statement"] = "A company needs to reduce operational costs by 20% while maintaining employee satisfaction.",
                ["context"] = "The company operates in a competitive tech market with high talent retention challenges.",
                ["constraints"] = "Cannot reduce headcount by more than 5%, must maintain current benefit levels.",
                ["expected_outcome"] = "A comprehensive cost reduction plan with specific actionable steps",
                ["reasoning_depth"] = 4
            };

            try
            {
                var result = await _chainOfThoughtExecutor.ExecuteAsync(_kernel, arguments);
                var finalAnswer = result.GetValue<string>() ?? "(no result)";
                Console.WriteLine($"✅ Chain of Thought Result: {finalAnswer}");
                return finalAnswer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Chain of Thought Error: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Runs all examples to demonstrate the different reasoning approaches
        /// </summary>
        public async Task RunAllExamplesAsync()
        {
            Console.WriteLine("🎯 React and Chain of Thought Quickstart Examples");
            Console.WriteLine(new string('=', 60));
            
            // Run simple ReAct example
            await RunSimpleReActExampleAsync("What's the weather like today?");
            Console.WriteLine();
            
            // Run advanced ReAct example
            await RunAdvancedReActExampleAsync();
            Console.WriteLine();
            
            // Run Chain of Thought example
            await RunChainOfThoughtExampleAsync();
            Console.WriteLine();
            
            Console.WriteLine("🎉 All examples completed!");
        }
    }
}
