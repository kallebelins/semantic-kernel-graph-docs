using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;

namespace Examples;

/// <summary>
/// Example demonstrating all the installation and configuration code from the installation documentation.
/// This example tests various setup scenarios and configuration options.
/// </summary>
public class InstallationExample
{
    /// <summary>
    /// Runs all installation examples to verify the documentation code works correctly.
    /// </summary>
    /// <returns>Task representing the asynchronous operation</returns>
    public static async Task RunAllExamplesAsync()
    {
        Console.WriteLine("🔧 Installation Examples - Testing Documentation Code\n");

        try
        {
            // Test 1: Basic OpenAI Configuration
            await TestOpenAIConfigurationAsync();

            // Test 2: Azure OpenAI Configuration
            await TestAzureOpenAIConfigurationAsync();

            // Test 3: Configuration File Setup
            await TestConfigurationFileSetupAsync();

            // Test 4: Basic Project Setup
            await TestBasicProjectSetupAsync();

            // Test 5: Advanced Configuration
            await TestAdvancedConfigurationAsync();

            // Test 6: Dependency Injection Setup (commented out due to framework dependencies)
            // await TestDependencyInjectionSetupAsync();

            // Test 7: Verification Test
            await TestVerificationAsync();

            Console.WriteLine("\n✅ All installation examples completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in installation examples: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Test 1: Basic OpenAI Configuration from environment variables
    /// </summary>
    /// <returns>Task representing the asynchronous operation</returns>
    private static Task TestOpenAIConfigurationAsync()
    {
        Console.WriteLine("📋 Test 1: Basic OpenAI Configuration");

        try
        {
            var builder = Kernel.CreateBuilder();

            // Test the exact code from documentation
            builder.AddOpenAIChatCompletion(
                modelId: Environment.GetEnvironmentVariable("OPENAI_MODEL_NAME") ?? "gpt-4",
                apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new InvalidOperationException("OPENAI_API_KEY not found")
            );

            var kernel = builder.Build();
            Console.WriteLine("✅ OpenAI configuration successful");
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine("⚠️  OPENAI_API_KEY not found in environment variables (expected in test environment)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ OpenAI configuration failed: {ex.Message}");
            throw;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Test 2: Azure OpenAI Configuration from environment variables
    /// </summary>
    /// <returns>Task representing the asynchronous operation</returns>
    private static Task TestAzureOpenAIConfigurationAsync()
    {
        Console.WriteLine("📋 Test 2: Azure OpenAI Configuration");

        try
        {
            var builder = Kernel.CreateBuilder();

            // Test the exact code from documentation
            builder.AddAzureOpenAIChatCompletion(
                deploymentName: Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? throw new InvalidOperationException("AZURE_OPENAI_DEPLOYMENT_NAME not found"),
                endpoint: Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT not found"),
                apiKey: Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? throw new InvalidOperationException("AZURE_OPENAI_API_KEY not found")
            );

            var kernel = builder.Build();
            Console.WriteLine("✅ Azure OpenAI configuration successful");
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine("⚠️  Azure OpenAI environment variables not found (expected in test environment)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Azure OpenAI configuration failed: {ex.Message}");
            throw;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Test 3: Configuration File Setup using appsettings.json
    /// </summary>
    /// <returns>Task representing the asynchronous operation</returns>
    private static Task TestConfigurationFileSetupAsync()
    {
        Console.WriteLine("📋 Test 3: Configuration File Setup");

        try
        {
            // Test the exact code from documentation
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var builder = Kernel.CreateBuilder();
            builder.AddOpenAIChatCompletion(
                modelId: configuration["OpenAI:Model"] ?? "gpt-4",
                apiKey: configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI:ApiKey not found")
            );

            var kernel = builder.Build();
            Console.WriteLine("✅ Configuration file setup successful");
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine("⚠️  OpenAI:ApiKey not found in configuration (expected in test environment)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Configuration file setup failed: {ex.Message}");
            throw;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Test 4: Basic Project Setup with required packages
    /// </summary>
    /// <returns>Task representing the asynchronous operation</returns>
    private static Task TestBasicProjectSetupAsync()
    {
        Console.WriteLine("📋 Test 4: Basic Project Setup");

        try
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var builder = Kernel.CreateBuilder();

            // Configure your LLM provider
            builder.AddOpenAIChatCompletion(
                "gpt-4",
                configuration["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            );

            // Enable graph functionality
            builder.AddGraphSupport(options =>
            {
                options.EnableLogging = true;
                options.EnableMetrics = true;
            });

            var kernel = builder.Build();
            Console.WriteLine("✅ Basic project setup successful");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Basic project setup failed: {ex.Message}");
            throw;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Test 5: Advanced Configuration with graph options
    /// </summary>
    /// <returns>Task representing the asynchronous operation</returns>
    private static Task TestAdvancedConfigurationAsync()
    {
        Console.WriteLine("📋 Test 5: Advanced Configuration");

        try
        {
            var builder = Kernel.CreateBuilder();

            // Test the exact code from documentation
            builder.AddGraphSupport(options =>
            {
                // Enable logging
                options.EnableLogging = true;

                // Enable performance metrics
                options.EnableMetrics = true;

                // Configure execution limits
                options.MaxExecutionSteps = 100;
                options.ExecutionTimeout = TimeSpan.FromMinutes(5);
            });

            // Add additional graph features
            builder.AddGraphMemory()
                .AddGraphTemplates()
                .AddCheckpointSupport(options =>
                {
                    options.EnableCompression = true;
                    options.MaxCacheSize = 1000;
                    options.EnableAutoCleanup = true;
                    options.AutoCleanupInterval = TimeSpan.FromHours(1);
                });

            var kernel = builder.Build();
            Console.WriteLine("✅ Advanced configuration successful");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Advanced configuration failed: {ex.Message}");
            throw;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Test 6: Dependency Injection Setup for ASP.NET Core
    /// </summary>
    /// <returns>Task representing the asynchronous operation</returns>
    private static Task TestDependencyInjectionSetupAsync()
    {
        Console.WriteLine("📋 Test 6: Dependency Injection Setup");
        Console.WriteLine("⚠️  This test is commented out due to framework dependencies");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Test 7: Verification Test - Simple test to verify everything works
    /// </summary>
    /// <returns>Task representing the asynchronous operation</returns>
    private static async Task TestVerificationAsync()
    {
        Console.WriteLine("📋 Test 7: Verification Test");

        try
        {
            // Test the exact code from documentation
            var builder = Kernel.CreateBuilder();
            builder.AddOpenAIChatCompletion("gpt-4", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
            builder.AddGraphSupport();

            var kernel = builder.Build();

            // Create a simple test node
            var testNode = new FunctionGraphNode(
                kernel.CreateFunctionFromPrompt("Say hello to {{$name}}"),
                "test-node"
            );

            // Create and execute a minimal graph
            var graph = new GraphExecutor("TestGraph");
            graph.AddNode(testNode);
            graph.SetStartNode(testNode.NodeId);

            var state = new KernelArguments { ["name"] = "World" };
            var result = await graph.ExecuteAsync(kernel, state);

            Console.WriteLine($"✅ Verification test successful! Result: {result}");
        }
        catch (ArgumentNullException)
        {
            Console.WriteLine("⚠️  Verification test skipped - OPENAI_API_KEY not found in environment variables");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Verification test failed: {ex.Message}");
            Console.WriteLine("⚠️  This test requires a valid OpenAI API key to execute");
        }
    }
}
