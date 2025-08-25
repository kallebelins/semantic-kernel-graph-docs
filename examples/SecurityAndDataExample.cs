using System;
using System.Collections.Generic;
using System.Text.Json;
using SemanticKernel.Graph.Integration;

namespace Examples;

/// <summary>
/// Documentation example for Security and Data Handling.
/// Demonstrates how to use <see cref="SensitiveDataSanitizer"/> to redact sensitive values.
/// All comments are in English and aimed to be accessible for all levels.
/// </summary>
public static class SecurityAndDataExample
{
    /// <summary>
    /// Async wrapper so examples can call this method consistently.
    /// </summary>
    public static Task RunAsync()
    {
        RunExample();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns a small example output that the documentation can show.
    /// This mirror the code used in the documentation and is safe to run.
    /// </summary>
    public static void RunExample()
    {
        // Create a policy that redacts sensitive keys and preserves the "Bearer " prefix
        var policy = new SensitiveDataPolicy
        {
            Enabled = true,
            Level = SanitizationLevel.Basic,
            RedactionText = "[REDACTED]",
            MaskAuthorizationBearerToken = true
        };

        var sanitizer = new SensitiveDataSanitizer(policy);

        // Sample dictionary that contains sensitive keys
        var sample = new Dictionary<string, object?>()
        {
            ["username"] = "alice",
            ["password"] = "s3cr3t!",
            ["authorization"] = "Bearer sk-12345",
            ["notes"] = "This value is not sensitive"
        };

        // Sanitize the dictionary (cast to IDictionary to disambiguate overloads)
        var sanitized = sanitizer.Sanitize((IDictionary<string, object?>)sample);

        Console.WriteLine("Original values:");
        foreach (var kv in sample)
        {
            Console.WriteLine($" - {kv.Key}: {kv.Value}");
        }

        Console.WriteLine('\n' + "Sanitized values:");
        foreach (var kv in sanitized)
        {
            Console.WriteLine($" - {kv.Key}: {kv.Value}");
        }

        // Demonstrate sanitizing a JSON payload
        var json = "{\"api_key\":\"sk-abcdef\",\"nested\":{\"password\":\"p@ss\"}}";
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var sanitizedJson = sanitizer.Sanitize(root);

        Console.WriteLine('\n' + "Original JSON: " + json);
        Console.WriteLine("Sanitized JSON object:");
        if (sanitizedJson is IDictionary<string, object?> dict)
        {
            foreach (var kv in dict)
            {
                Console.WriteLine($" - {kv.Key}: {kv.Value}");
            }
        }
    }
}