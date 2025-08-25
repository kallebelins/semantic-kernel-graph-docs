using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Integration;

namespace SemanticKernel.Graph.DocsExamples
{
    /// <summary>
    /// Documentation-friendly example showing kernel integration and sanitization.
    /// Comments are in English and the file is intended to be included in docs.
    /// </summary>
    public static class IntegrationAndExtensionsExample
    {
        /// <summary>
        /// Run the example used by the documentation. This mirrors the runnable example
        /// provided in the Examples project and is kept small for clarity.
        /// </summary>
        public static async Task RunAsync()
        {
            // Build a kernel with graph support enabled.
            var kernel = Kernel.CreateBuilder()
                .AddGraphSupport(options =>
                {
                    options.EnableLogging = true;
                    options.EnableMetrics = false;
                })
                .Build();

            Console.WriteLine("Integration example (docs): kernel built with graph support.");

            // Demonstrate the SensitiveDataSanitizer usage.
            var sanitizer = new SensitiveDataSanitizer();

            System.Collections.Generic.IDictionary<string, object?> payload = new System.Collections.Generic.Dictionary<string, object?>
            {
                ["username"] = "alice",
                ["password"] = "supersecret",
                ["api_key"] = "sk-test-123",
                ["notes"] = "This is safe"
            };

            var sanitized = sanitizer.Sanitize(payload);

            Console.WriteLine("Sanitized payload:");
            foreach (var kv in sanitized)
            {
                Console.WriteLine($" - {kv.Key}: {kv.Value}");
            }

            await Task.CompletedTask;
        }
    }
}


