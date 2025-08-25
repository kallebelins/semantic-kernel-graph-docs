// This file mirrors the runnable example in the Examples project.
// It is intended for the documentation to show code that has been executed and validated.

using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.State;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Examples;

/// <summary>
/// Documentation copy of the SchemaTypingAndValidationExample used in the Examples project.
/// Keep this file synchronized with the runnable example to ensure documented code works.
/// </summary>
public static class SchemaTypingAndValidationExample
{
    public static async Task RunAsync()
    {
        var kernel = Kernel.CreateBuilder().AddGraphSupport().Build();

        var input = new GraphParameterSchema
        {
            Name = "user_id",
            Description = "Unique identifier for user",
            Required = true,
            Type = GraphType.FromPrimitive(GraphPrimitiveType.Integer)
        };

        var state = new GraphState();
        state.SetValue("user_id", 100);

        var validation = StateValidator.ValidateState(state);
        Console.WriteLine(validation.CreateSummary(includeDetails: true));

        await Task.CompletedTask;
    }
}


