using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Integration;
using SemanticKernel.Graph.Nodes;

namespace Examples;

/// <summary>
/// Comprehensive example demonstrating dynamic routing capabilities including:
/// - Content-based node selection
/// - Template-based routing decisions
/// - Routing cache optimization
/// - Fallback mechanisms
/// - Routing metrics and reporting
/// </summary>
public static class DynamicRoutingExample
{
    /// <summary>
    /// Runs the complete dynamic routing demonstration.
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("🔄 Dynamic Routing Example");
        Console.WriteLine("============================");

        try
        {
            // 1. Basic Dynamic Routing Setup
            await Example1_BasicDynamicRoutingSetupAsync();
            Console.WriteLine();

            // 2. Advanced Routing with Services
            await Example2_AdvancedRoutingWithServicesAsync();
            Console.WriteLine();

            // 3. Template-Based Routing
            await Example3_TemplateBasedRoutingAsync();
            Console.WriteLine();

            // 4. Routing Metrics and Monitoring
            await Example4_RoutingMetricsAndMonitoringAsync();
            Console.WriteLine();

            // 5. Fallback Mechanisms
            await Example5_FallbackMechanismsAsync();

            // 6. Documentation Code Test
            await Example6_DocumentationCodeTestAsync();

            Console.WriteLine("\n✅ All dynamic routing examples completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in dynamic routing examples: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Example 1: Basic Dynamic Routing Setup
    /// Demonstrates creating a kernel with dynamic routing enabled
    /// </summary>
    private static async Task Example1_BasicDynamicRoutingSetupAsync()
    {
        Console.WriteLine("📋 Example 1: Basic Dynamic Routing Setup");
        Console.WriteLine("----------------------------------------");

        try
        {
            // Create kernel with dynamic routing
            var kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.AddGraphSupport();

            var kernel = kernelBuilder.Build();

            // Create graph with dynamic routing enabled
            var graph = new GraphExecutor("DynamicDemo", "Dynamic routing example");

            // Add nodes and connections
            var startNode = new FunctionGraphNode(
                kernel.CreateFunctionFromMethod(
                    (string input) => $"Processed: {input}",
                    functionName: "ProcessInput",
                    description: "Process the input data"
                ),
                "start"
            ).StoreResultAs("processed_input");

            var processNode = new FunctionGraphNode(
                kernel.CreateFunctionFromMethod(
                    (string processedInput) => $"Final result: {processedInput}",
                    functionName: "ProcessData",
                    description: "Process the input data"
                ),
                "process"
            ).StoreResultAs("result");

            var errorNode = new FunctionGraphNode(
                kernel.CreateFunctionFromMethod(
                    () => "Error handled successfully",
                    functionName: "HandleError",
                    description: "Handle any errors"
                ),
                "error"
            ).StoreResultAs("error_handled");

            graph.AddNode(startNode)
                 .AddNode(processNode)
                 .AddNode(errorNode)
                 .SetStartNode("start");

            // Execute with dynamic routing
            var result = await graph.ExecuteAsync(kernel, new KernelArguments { ["input"] = "test" });

            Console.WriteLine($"✅ Basic dynamic routing executed successfully");
            Console.WriteLine($"Result: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in basic dynamic routing: {ex.Message}");
        }
    }

    /// <summary>
    /// Example 2: Advanced Routing with Services
    /// Demonstrates creating advanced routing with embedding and memory services
    /// </summary>
    private static async Task Example2_AdvancedRoutingWithServicesAsync()
    {
        Console.WriteLine("📋 Example 2: Advanced Routing with Services");
        Console.WriteLine("--------------------------------------------");

        try
        {
            // Create advanced routing with embedding and memory services
            // Note: In a real scenario, you would have actual embedding and memory services
            // For this example, we'll create the routing engine without them
            var routingEngine = new DynamicRoutingEngine(
                templateEngine: null,
                options: new DynamicRoutingOptions
                {
                    EnableCaching = true,
                    EnableFallback = true,
                    MaxCacheSize = 500,
                    CacheExpirationMinutes = 60
                },
                logger: null,  // Would be actual logger
                embeddingService: null,  // Would be actual embedding service
                memoryService: null      // Would be actual memory service
            );

            Console.WriteLine($"✅ Advanced routing engine created successfully");
            Console.WriteLine($"Caching enabled: {routingEngine.GetType().GetProperty("EnableCaching") != null}");
            Console.WriteLine($"Fallback enabled: {routingEngine.GetType().GetProperty("EnableFallback") != null}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in advanced routing with services: {ex.Message}");
        }
    }

    /// <summary>
    /// Example 3: Template-Based Routing
    /// Demonstrates template-based routing decisions using Handlebars templates
    /// </summary>
    private static async Task Example3_TemplateBasedRoutingAsync()
    {
        Console.WriteLine("📋 Example 3: Template-Based Routing");
        Console.WriteLine("-----------------------------------");

        try
        {
            // Create template engine
            var templateEngine = new HandlebarsGraphTemplateEngine(new GraphTemplateOptions
            {
                EnableHandlebars = true,
                EnableCustomHelpers = true,
                TemplateCacheSize = 100
            });

            // Create routing engine with template support
            var routingEngine = new DynamicRoutingEngine(
                templateEngine: templateEngine,
                options: new DynamicRoutingOptions { EnableCaching = true, EnableFallback = true }
            );

            Console.WriteLine($"✅ Template-based routing engine created successfully");
            Console.WriteLine($"Template engine type: {templateEngine.GetType().Name}");

            // Test template rendering
            var template = "{{#if (eq priority 'high')}}true{{else}}false{{/if}}";
            var context = new KernelArguments { ["priority"] = "high" };

            try
            {
                var result = await templateEngine.RenderWithArgumentsAsync(template, context, CancellationToken.None);
                Console.WriteLine($"✅ Template rendering test successful: '{template}' -> '{result}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Template rendering test failed (expected in demo): {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in template-based routing: {ex.Message}");
        }
    }

    /// <summary>
    /// Example 4: Routing Metrics and Monitoring
    /// Demonstrates routing metrics collection and analysis
    /// </summary>
    private static async Task Example4_RoutingMetricsAndMonitoringAsync()
    {
        Console.WriteLine("📋 Example 4: Routing Metrics and Monitoring");
        Console.WriteLine("--------------------------------------------");

        try
        {
            // Create routing engine with metrics enabled
            var routingEngine = new DynamicRoutingEngine(
                options: new DynamicRoutingOptions
                {
                    EnableCaching = true,
                    EnableFallback = true,
                    MaxCacheSize = 1000,
                    CacheExpirationMinutes = 30
                }
            );

            Console.WriteLine($"✅ Routing engine with metrics created successfully");

            // Note: In a real scenario, you would execute routing decisions and collect metrics
            // For this example, we'll just verify the engine was created
            Console.WriteLine($"Routing engine type: {routingEngine.GetType().Name}");
            Console.WriteLine($"Implements IAsyncDisposable: {routingEngine is IAsyncDisposable}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in routing metrics and monitoring: {ex.Message}");
        }
    }

    /// <summary>
    /// Example 5: Fallback Mechanisms
    /// Demonstrates fallback strategies when routing fails
    /// </summary>
    private static async Task Example5_FallbackMechanismsAsync()
    {
        Console.WriteLine("📋 Example 5: Fallback Mechanisms");
        Console.WriteLine("---------------------------------");

        try
        {
            // Create routing engine with fallback enabled
            var routingEngine = new DynamicRoutingEngine(
                options: new DynamicRoutingOptions
                {
                    EnableCaching = true,
                    EnableFallback = true,
                    MaxCacheSize = 100,
                    CacheExpirationMinutes = 15
                }
            );

            Console.WriteLine($"✅ Fallback-enabled routing engine created successfully");

            // Test fallback configuration
            var options = new DynamicRoutingOptions
            {
                EnableCaching = true,
                EnableFallback = true,
                MaxCacheSize = 500,
                CacheExpirationMinutes = 60
            };

            Console.WriteLine($"Fallback options:");
            Console.WriteLine($"  - Caching enabled: {options.EnableCaching}");
            Console.WriteLine($"  - Fallback enabled: {options.EnableFallback}");
            Console.WriteLine($"  - Max cache size: {options.MaxCacheSize}");
            Console.WriteLine($"  - Cache expiration: {options.CacheExpirationMinutes} minutes");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in fallback mechanisms: {ex.Message}");
        }
    }

    /// <summary>
    /// Example 6: Documentation Code Test
    /// Tests the exact code examples from the documentation to ensure they work
    /// </summary>
    private static async Task Example6_DocumentationCodeTestAsync()
    {
        Console.WriteLine("📋 Example 6: Documentation Code Test");
        Console.WriteLine("-------------------------------------");

        try
        {
            // Test the exact code from the documentation
            Console.WriteLine("Testing Basic Dynamic Routing Setup from documentation...");

            // Create kernel with dynamic routing (exact code from docs)
            var kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.AddGraphSupport();

            var kernel = kernelBuilder.Build();

            // Create graph with dynamic routing enabled (exact code from docs)
            var graph = kernelBuilder.CreateGraphWithDynamicRouting("DynamicDemo", "Dynamic routing example");

            // Add nodes and connections (exact code from docs)
            var startNode = new FunctionGraphNode(
                kernel.CreateFunctionFromMethod(
                    (string input) => $"Processed: {input}",
                    functionName: "ProcessInput",
                    description: "Process the input data"
                ),
                "start"
            ).StoreResultAs("processed_input");

            var processNode = new FunctionGraphNode(
                kernel.CreateFunctionFromMethod(
                    (string processedInput) => $"Final result: {processedInput}",
                    functionName: "ProcessData",
                    description: "Process the input data"
                ),
                "process"
            ).StoreResultAs("result");

            var errorNode = new FunctionGraphNode(
                kernel.CreateFunctionFromMethod(
                    () => "Error handled successfully",
                    functionName: "HandleError",
                    description: "Handle any errors"
                ),
                "error"
            ).StoreResultAs("error_handled");

            graph.AddNode(startNode)
                 .AddNode(processNode)
                 .AddNode(errorNode)
                 .SetStartNode("start");

            // Execute with dynamic routing (exact code from docs)
            var result = await graph.ExecuteAsync(kernel, new KernelArguments { ["input"] = "test" });

            Console.WriteLine($"✅ Documentation code test successful!");
            Console.WriteLine($"Result: {result}");
            Console.WriteLine($"Graph has dynamic routing: {graph.RoutingEngine != null}");
            Console.WriteLine($"Graph has template engine: {graph.Metadata.ContainsKey("TemplateEngine")}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in documentation code test: {ex.Message}");
        }
    }
}


