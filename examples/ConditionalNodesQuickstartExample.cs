using Microsoft.SemanticKernel;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;
using SemanticKernel.Graph.State;

namespace ConditionalNodesQuickstartExample;

/// <summary>
/// Example demonstrating conditional nodes and edges in SemanticKernel.Graph.
/// This example shows how to create dynamic workflows that can make decisions
/// and route execution based on state conditions.
/// </summary>
public static class ConditionalNodesQuickstartExample
{
    /// <summary>
    /// Helper method to safely get typed values from KernelArguments.
    /// </summary>
    /// <typeparam name="T">Expected value type</typeparam>
    /// <param name="args">Kernel arguments</param>
    /// <param name="key">Parameter key</param>
    /// <param name="defaultValue">Default value if not found</param>
    /// <returns>Typed value or default</returns>
    private static T GetValue<T>(this KernelArguments args, string key, T defaultValue = default!)
    {
        if (args.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }

    /// <summary>
    /// Demonstrates a complete conditional workflow with age verification and VIP status checks.
    /// </summary>
    /// <param name="kernel">The semantic kernel instance</param>
    /// <returns>Task representing the execution</returns>
    public static async Task RunConditionalWorkflowExample(Kernel kernel)
    {
        // Node 1: Input processing
        var inputNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    var age = args.GetValue<int>("userAge");
                    var name = args.GetValue<string>("userName");
                    
                    // Add some analysis
                    var isVip = age > 25 && name.Length > 5;
                    args["isVip"] = isVip;
                    args["ageGroup"] = age < 18 ? "minor" : age < 65 ? "adult" : "senior";
                    
                    return $"Processed user: {name}, Age: {age}, VIP: {isVip}";
                },
                "ProcessUser",
                "Processes user input and determines characteristics"
            ),
            "input_node"
        ).StoreResultAs("inputResult");

        // Node 2: Age verification
        var ageCheckNode = new ConditionalGraphNode(
            state => state.GetValue<int>("userAge") >= 18,
            "age_check",
            "AgeVerification",
            "Verifies user is 18 or older"
        );

        // Node 3: VIP status check
        var vipCheckNode = new ConditionalGraphNode(
            "{{isVip}} == true",
            "vip_check",
            "VipStatusCheck",
            "Checks if user has VIP status"
        );

        // Node 4: Adult content access
        var adultNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    var name = args.GetValue<string>("userName");
                    var age = args.GetValue<int>("userAge");
                    var isVip = args.GetValue<bool>("isVip");
                    
                    var access = isVip ? "Premium" : "Standard";
                    args["accessLevel"] = access;
                    args["contentType"] = "adult";
                    
                    return $"Welcome {name}! You have {access} access to adult content.";
                },
                "AdultAccess",
                "Provides adult content access"
            ),
            "adult_node"
        ).StoreResultAs("adultResult");

        // Node 5: VIP upgrade suggestion
        var vipUpgradeNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    var name = args.GetValue<string>("userName");
                    var age = args.GetValue<int>("userAge");
                    
                    args["upgradeSuggested"] = true;
                    args["upgradeReason"] = "Age and activity qualify for VIP";
                    
                    return $"Hello {name}! Based on your age ({age}), you might qualify for VIP status.";
                },
                "VipUpgrade",
                "Suggests VIP upgrade for eligible users"
            ),
            "vip_upgrade_node"
        ).StoreResultAs("upgradeResult");

        // Node 6: Minor access restriction
        var minorNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    var name = args.GetValue<string>("userName");
                    var age = args.GetValue<int>("userAge");
                    
                    args["accessLevel"] = "restricted";
                    args["contentType"] = "family";
                    args["restrictionReason"] = "Age requirement not met";
                    
                    return $"Hello {name}! You're {age} years old. This content requires you to be 18 or older.";
                },
                "MinorAccess",
                "Handles access for users under 18"
            ),
            "minor_node"
        ).StoreResultAs("minorResult");

        // Node 7: Final summary
        var summaryNode = new FunctionGraphNode(
            KernelFunctionFactory.CreateFromMethod(
                (KernelArguments args) =>
                {
                    var name = args.GetValue<string>("userName");
                    var age = args.GetValue<int>("userAge");
                    var accessLevel = args.GetValue<string>("accessLevel");
                    var contentType = args.GetValue<string>("contentType");
                    
                    var summary = $"User: {name}, Age: {age}, Access: {accessLevel}, Content: {contentType}";
                    args["finalSummary"] = summary;
                    
                    return summary;
                },
                "CreateSummary",
                "Creates final summary of user access"
            ),
            "summary_node"
        ).StoreResultAs("summaryResult");

        // Build and configure the graph
        var graph = new GraphExecutor("ConditionalWorkflowExample", "Demonstrates conditional routing based on user characteristics");
        
        graph.AddNode(inputNode);
        graph.AddNode(ageCheckNode);
        graph.AddNode(vipCheckNode);
        graph.AddNode(adultNode);
        graph.AddNode(vipUpgradeNode);
        graph.AddNode(minorNode);
        graph.AddNode(summaryNode);
        
        // Connect nodes with conditional logic
        graph.Connect(inputNode.NodeId, ageCheckNode.NodeId);
        
        // Age check paths - use ConnectWhen for conditional routing
        graph.ConnectWhen(ageCheckNode.NodeId, vipCheckNode.NodeId, 
            args => args.GetValue<int>("userAge") >= 18);
        graph.ConnectWhen(ageCheckNode.NodeId, minorNode.NodeId, 
            args => args.GetValue<int>("userAge") < 18);
        
        // VIP check paths
        graph.ConnectWhen(vipCheckNode.NodeId, adultNode.NodeId, 
            args => args.GetValue<bool>("isVip") == true);
        graph.ConnectWhen(vipCheckNode.NodeId, vipUpgradeNode.NodeId, 
            args => args.GetValue<bool>("isVip") == false);
        
        // Connect all paths to summary
        graph.Connect(adultNode.NodeId, summaryNode.NodeId);
        graph.Connect(vipUpgradeNode.NodeId, summaryNode.NodeId);
        graph.Connect(minorNode.NodeId, summaryNode.NodeId);
        
        graph.SetStartNode(inputNode.NodeId);

        // Execute with different user scenarios
        var scenarios = new[]
        {
            new { Name = "Alice", Age = 25, ExpectedPath = "VIP Adult" },
            new { Name = "Bob", Age = 17, ExpectedPath = "Minor" },
            new { Name = "Charlie", Age = 30, ExpectedPath = "Standard Adult" }
        };

        foreach (var scenario in scenarios)
        {
            Console.WriteLine($"\n=== Testing: {scenario.Name}, Age {scenario.Age} ===");
            
            var initialState = new KernelArguments
            {
                ["userName"] = scenario.Name,
                ["userAge"] = scenario.Age
            };
            
            var result = await graph.ExecuteAsync(kernel, initialState);
            
            Console.WriteLine($"Path taken: {scenario.ExpectedPath}");
            Console.WriteLine($"Final summary: {initialState.GetValue<string>("finalSummary")}");
            Console.WriteLine($"Access level: {initialState.GetValue<string>("accessLevel")}");
        }
    }
}
