# Semantic Kernel Graph Package

A powerful and extensible graph-based execution framework built on top of Microsoft's Semantic Kernel, designed for building sophisticated AI workflows, multi-agent systems, and intelligent applications.

Browse the documentation on the website: [skgraph.dev](https://skgraph.dev/)

## 🚀 Features

### Core capabilities
- **Graph-based execution**: Define complex AI workflows as directed graphs with nodes and edges
- **Stateful orchestration**: Shared, typed state across nodes with validation and serialization
- **Dynamic routing**: Conditional edges and intelligent routing driven by data and model outputs
- **Checkpointing & recovery**: Resume execution from any point with full state preservation
- **Streaming execution**: Real-time events and monitoring for interactive apps
- **Human-in-the-loop**: Built-in approval, intervention, and feedback workflows

### Advanced patterns
- **Chain-of-thought** and **ReAct**: Structured reasoning and tool-use workflows
- **Multi-hop RAG** with retries and guardrails
- **Conditional execution** and **parallel fork-join** patterns

### Enterprise features
- **Observability**: Metrics, logging, and real-time visualization
- **Error policies**: Retries, backoffs, and compensations
- **Resource governance**: Concurrency limits, rate limiting, and cost controls
- **Security**: Data and secret handling, auth and policy integration

## 🧭 Documentation

Online docs: [skgraph.dev](https://skgraph.dev/)

### Getting started
- [Overview](docs/getting-started.md)
- [Installation](docs/installation.md)
- [First Graph](docs/first-graph.md)
- Quickstarts:
  - [First Graph in 5 Minutes](docs/first-graph-5-minutes.md)
  - [State Management](docs/state-quickstart.md)
  - [Conditional Nodes](docs/conditional-nodes-quickstart.md)
  - [Checkpointing](docs/checkpointing-quickstart.md)
  - [Streaming](docs/streaming-quickstart.md)
  - [Metrics & Logging](docs/metrics-logging-quickstart.md)
  - [ReAct & Chain of Thought](docs/react-cot-quickstart.md)
- Tutorials:
  - [State Management](docs/state-tutorial.md)
  - [Conditional Nodes](docs/conditional-nodes-tutorial.md)

### Core concepts
- [Overview](docs/concepts/index.md)
- [Graph concepts](docs/concepts/graph-concepts.md)
- [Graphs](docs/concepts/graphs.md)
- [Nodes](docs/concepts/nodes.md)
- [Node types](docs/concepts/node-types.md)
- [State](docs/concepts/state.md)
- [Edges & routing](docs/concepts/routing.md)
- [Execution](docs/concepts/execution.md)
- [Execution model](docs/concepts/execution-model.md)
- [Checkpointing](docs/concepts/checkpointing.md)
- [Streaming](docs/concepts/streaming.md)
- [Visualization](docs/concepts/visualization.md)

### How-to guides
- Core:
  - [Build a graph](docs/how-to/build-a-graph.md)
  - [Conditional nodes](docs/how-to/conditional-nodes.md)
  - [Loops](docs/how-to/loops.md)
- Advanced:
  - [Advanced routing](docs/how-to/advanced-routing.md)
  - [Parallelism (fork-join)](docs/how-to/parallelism-and-fork-join.md)
  - [Error handling & resilience](docs/how-to/error-handling-and-resilience.md)
- Integration:
  - [Tools](docs/how-to/tools.md)
  - [REST tools integration](docs/how-to/rest-tools-integration.md)
  - [Multi-agent & shared state](docs/how-to/multi-agent-and-shared-state.md)
  - [Integration & extensions](docs/how-to/integration-and-extensions.md)
- Observability:
  - [Metrics](docs/how-to/metrics-and-observability.md)
  - [Debug & inspection](docs/how-to/debug-and-inspection.md)
  - [Real-time visualization](docs/how-to/real-time-visualization-and-highlights.md)
- Security & governance:
  - [Security & data](docs/how-to/security-and-data.md)
  - [Resource governance & concurrency](docs/how-to/resource-governance-and-concurrency.md)
  - [Human-in-the-loop](docs/how-to/human-in-the-loop.md)
- Server & APIs:
  - [Server and APIs](docs/how-to/server-and-apis.md)
  - [Exposing REST APIs](docs/how-to/exposing-rest-apis.md)
- Templates & types:
  - [Templates & memory](docs/how-to/templates-and-memory.md)
  - [Schema typing & validation](docs/how-to/schema-typing-and-validation.md)

## 🛠️ Installation

### Prerequisites
* .NET 8.0 or later
* Semantic Kernel SDK
* Optional: Azure OpenAI, OpenAI, or other AI service credentials

## 🔧 Usage

### Basic Graph Definition
```csharp
var graph = new GraphBuilder()
    .AddNode("start", new ActionNode("Initialize workflow"))
    .AddNode("process", new ActionNode("Process data"))
    .AddNode("decision", new ConditionalNode("Make decision"))
    .AddNode("complete", new ActionNode("Complete workflow"))
    
    .AddEdge("start", "process")
    .AddEdge("process", "decision")
    .AddEdge("decision", "complete", "success")
    
    .Build();

var executor = new GraphExecutor();
var result = await executor.ExecuteAsync(graph, context);
```

### Advanced Multi-Agent Workflow
```csharp
var workflow = new MultiAgentWorkflowBuilder()
    .AddAgent("researcher", new ResearchAgent())
    .AddAgent("analyst", new AnalysisAgent())
    .AddAgent("writer", new WritingAgent())
    
    .DefineWorkflow(workflow => workflow
        .StartWith("researcher")
        .Then("analyst")
        .Then("writer")
        .WithHumanApproval("final_review")
    )
    
    .Build();
```

## 📊 Examples

There is a full .NET examples project in `examples/` that maps to the docs examples.

Run prerequisites:
- .NET 8.0 SDK or later
- Optional: Set `OPENAI_API_KEY` (or your provider env vars) for LLM-powered demos

Run examples from the repository root:

```powershell
cd examples
dotnet run            # runs all examples
dotnet run -- first-graph
dotnet run -- conditional-nodes-quickstart
dotnet run -- streaming-quickstart
dotnet run -- multi-agent
```

Common example names (see `examples/Program.cs` for the full list):
`first-graph`, `first-graph-5-minutes`, `getting-started`, `state-quickstart`, `state-tutorial`, `conditional-nodes-quickstart`, `conditional-nodes-tutorial`, `checkpointing-quickstart`, `streaming-quickstart`, `metrics-logging-quickstart`, `dynamic-routing`, `advanced-routing`, `graph-executor`, `graph-options`, `executors-and-middlewares`, `inspection-visualization`, `debug-inspection`, `logging`, `graph-metrics`, `graph-visualization`, `multi-agent`, `chain-of-thought`, `react-cot-quickstart`, `react-agent`, `rest-api`, `rest-tools`, `plugin-system`, `document-analysis-pipeline`, `memory-agent`, `retrieval-agent`, `multi-hop-rag-retry`, `optimizers-and-few-shot`, `validation-compilation`, `error-policies`, `schema-typing-and-validation`, `resource-governance`, `parallelism-and-fork-join`.

Docs for examples: `docs/examples/`
- Core patterns: [Conditional Nodes](docs/examples/conditional-nodes.md), [Loops](docs/examples/loop-nodes.md), [Checkpointing](docs/examples/checkpointing.md)
- AI patterns: [Chain-of-Thought](docs/examples/chain-of-thought.md), [ReAct](docs/examples/react-agent.md), [Multi-agent](docs/examples/multi-agent.md)
- Advanced features: [Advanced Patterns](docs/examples/advanced-patterns.md), [Advanced Routing](docs/examples/advanced-routing.md), [Dynamic Routing](docs/examples/dynamic-routing.md)
- Integration: [REST API](docs/examples/rest-api.md), [Plugin System](docs/examples/plugin-system.md), [Tools](docs/examples/tools.md)
- Observability: [Metrics](docs/examples/graph-metrics.md), [Visualization](docs/examples/graph-visualization.md), [Logging](docs/examples/logging.md)
- Workflows: [Chatbot](docs/examples/chatbot.md), [Document Analysis](docs/examples/document-analysis-pipeline.md), [Retrieval Agent](docs/examples/retrieval-agent.md)
- Specialized: [Memory Agent](docs/examples/memory-agent.md), [Multi-hop RAG Retry](docs/examples/multi-hop-rag-retry.md), [Optimizers & Few-shot](docs/examples/optimizers-and-few-shot.md), [Assert & Suggest](docs/examples/assert-and-suggest.md), [Subgraphs](docs/examples/subgraph-examples.md), [Streaming Execution](docs/examples/streaming-execution.md)
- Templates: [Template Standard Sections](docs/examples/template-standard-sections.md), [Execution Guide](docs/examples/execution-guide.md)

## 📖 API reference

Key reference pages (see `docs/api/` for all):
- Core: [core.md](docs/api/core.md), [nodes.md](docs/api/nodes.md), [extensions.md](docs/api/extensions.md), [extensions-and-options.md](docs/api/extensions-and-options.md)
- Integration: [integration.md](docs/api/integration.md), [rest-tools.md](docs/api/rest-tools.md), [server-apis.md](docs/api/server-apis.md)
- Execution & tooling: [graph-executor.md](docs/api/graph-executor.md), [executors-and-middlewares.md](docs/api/executors-and-middlewares.md), [execution-context.md](docs/api/execution-context.md)
- Options & policies: [graph-options.md](docs/api/graph-options.md), [error-policies.md](docs/api/error-policies.md)
- Features: [human-in-the-loop.md](docs/api/human-in-the-loop.md), [visualization-realtime.md](docs/api/visualization-realtime.md), [inspection-visualization.md](docs/api/inspection-visualization.md)
- Patterns: [dynamic-routing.md](docs/api/dynamic-routing.md), [conditional-edge.md](docs/api/conditional-edge.md)
- Types & state: [state-and-serialization.md](docs/api/state-and-serialization.md), [streaming.md](docs/api/streaming.md)
- Agents & validation: [multi-agent.md](docs/api/multi-agent.md), [validation-compilation.md](docs/api/validation-compilation.md)
- Additional: [additional-utilities.md](docs/api/additional-utilities.md), [main-node-types.md](docs/api/main-node-types.md), [igraph-executor.md](docs/api/igraph-executor.md), [igraph-node.md](docs/api/igraph-node.md), [metrics.md](docs/api/metrics.md)

## 📦 Install in your app (NuGet)

Install the packages in a .NET application:

```powershell
dotnet add package SemanticKernel.Graph
dotnet add package Microsoft.SemanticKernel
```

Then follow the guides above to build and run graphs.

## 🔍 Troubleshooting and resources
- [Glossary](docs/glossary.md)
- [Migrating](docs/migrations/index.md)
- [Troubleshooting](docs/troubleshooting.md)
- [FAQ](docs/faq.md)
- [Changelog](docs/changelog.md)

## 🤝 Contributing

Contributions to docs and examples are welcome. Please open an issue or pull request on the repository.
- Repository: `https://github.com/kallebelins/semantic-kernel-graph-docs`
- Issues: `https://github.com/kallebelins/semantic-kernel-graph-docs/issues`

## 📄 License

Licensed under the MIT License.

## 🙏 Acknowledgments

* Built on [Microsoft Semantic Kernel](https://github.com/microsoft/semantic-kernel)
* Inspired by modern workflow orchestration patterns
* Community contributions and feedback

## 📞 Support & community

- Website: [skgraph.dev](https://skgraph.dev/)
- Documentation: `docs/`
- Examples: `examples/`
- LinkedIn: `https://www.linkedin.com/company/skgraph-dev/`

---

**Ready to build intelligent workflows?** Start with [First Graph in 5 Minutes](docs/first-graph-5-minutes.md) and explore Semantic Kernel Graph!
