using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using SemanticKernel.Graph;
using SemanticKernel.Graph.Core;
using SemanticKernel.Graph.Execution;
using SemanticKernel.Graph.Extensions;
using SemanticKernel.Graph.Nodes;

/// <summary>
/// Example demonstrating a minimal Human-in-the-Loop approval flow using
/// a console interaction channel and a graph executor.
/// All code has explanatory comments and is designed to be runnable from
/// the examples runner without external dependencies.
/// </summary>
public static class HumanInTheLoopExample
{
    /// <summary>
    /// Entry point for the examples runner. Builds a kernel and a graph
    /// that pauses for human approval. The approval node is configured
    /// with a short timeout so the example will not block indefinitely.
    /// </summary>
    public static async Task RunAsync()
    {
        // Build a minimal kernel with graph support. No external services required.
        var kernel = Kernel.CreateBuilder()
            .AddGraphSupport()
            .Build();

        // Create a GraphExecutor that will run our graph using the kernel DI.
        var executor = new GraphExecutor(kernel);

        // Create a simple console-based human interaction channel.
        // This channel writes prompts to the console and reads responses.
        var consoleChannel = new ConsoleHumanInteractionChannel();

        // Create a human approval node that will ask the user to approve or reject.
        // We provide a short timeout with a default action to avoid blocking.
        var approvalNode = new HumanApprovalGraphNode(
            approvalTitle: "Document Review",
            approvalMessage: "Please review and approve the generated document (type 'approve' or 'reject')",
            interactionChannel: consoleChannel,
            nodeId: "doc_review_approval");

        // Add approval options with friendly display text. The API returns
        // a result in `approval_result` that the graph can route on.
        // Add approval options: (optionId, displayText, value, isDefault)
        approvalNode.AddApprovalOption("approve", "Approve and Continue", value: true, isDefault: true)
                    .AddApprovalOption("reject", "Reject and Stop", value: false, isDefault: false);

        // Configure a short timeout so automated runs do not hang waiting for input.
        // When the timeout fires the node will automatically reject (TimeoutAction.Reject).
        approvalNode.WithTimeout(TimeSpan.FromSeconds(5), TimeoutAction.Reject);

        // Create terminal function nodes that represent approved and rejected flows.
        var approvedFunction = KernelFunctionFactory.CreateFromMethod((KernelArguments args) =>
        {
            // Return a simple message for the approved branch.
            return "approved";
        }, "ApprovedFunction");

        var rejectedFunction = KernelFunctionFactory.CreateFromMethod((KernelArguments args) =>
        {
            // Return a simple message for the rejected branch.
            return "rejected";
        }, "RejectedFunction");

        var approvedNode = new FunctionGraphNode(approvedFunction, nodeId: "approved", description: "Approved Node");
        var rejectedNode = new FunctionGraphNode(rejectedFunction, nodeId: "rejected", description: "Rejected Node");

        // Add nodes to the executor and wire conditional routing based on approval result.
        executor.AddNode(approvalNode);
        executor.AddNode(approvedNode);
        executor.AddNode(rejectedNode);

        // Start execution at the approval node.
        executor.SetStartNode("doc_review_approval");

        // Mark the rejected node as the rejection-only path on the approval node
        approvalNode.OnRejection(rejectedNode);

        // Add unconditional edges from the approval node to both outcomes.
        // The approval node itself will select which path to follow based on the approval result.
        executor.Connect(approvalNode.NodeId, approvedNode.NodeId);
        executor.Connect(approvalNode.NodeId, rejectedNode.NodeId);

        // Execute the graph and print the final result value.
        var result = await executor.ExecuteAsync(kernel, new KernelArguments(), CancellationToken.None);

        Console.WriteLine("Human-in-the-Loop example completed. Result: " + (result?.GetValue<string>() ?? "<no-result>"));
    }
}


