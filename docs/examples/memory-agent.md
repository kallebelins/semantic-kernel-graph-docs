# Memory Agent Example

This example demonstrates how to implement memory-enabled agents in Semantic Kernel Graph workflows. It shows how to create agents that can remember, learn from, and build upon previous interactions and experiences.

## Objective

Learn how to implement memory-enabled agents in graph-based workflows to:
* Create agents with persistent memory and learning capabilities
* Implement memory storage, retrieval, and management
* Enable agents to learn from past interactions and experiences
* Build context-aware and adaptive agent behaviors
* Implement memory-based decision making and reasoning

## Prerequisites

* **.NET 8.0** or later
* **OpenAI API Key** configured in `appsettings.json`
* **Semantic Kernel Graph package** installed
* Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [Memory Patterns](../concepts/memory.md)

## Key Components

### Concepts and Techniques

* **Memory Storage**: Persistent storage of agent experiences and knowledge
* **Memory Retrieval**: Intelligent retrieval of relevant memories
* **Learning Integration**: Incorporating new experiences into memory
* **Context Awareness**: Using memory for context-aware decision making
* **Memory Management**: Efficient memory organization and cleanup

### Core Classes

* `MemoryAgent`: Base memory-enabled agent implementation
* `MemoryStore`: Memory storage and retrieval system
* `MemoryRetriever`: Intelligent memory search and retrieval
* `LearningIntegrator`: Learning from new experiences
* `MemoryManager`: Memory lifecycle and optimization

## Running the Example

### Getting Started

This example demonstrates memory-enabled agents with the Semantic Kernel Graph package. The code snippets below show you how to implement this pattern in your own applications.

## Step-by-Step Implementation

### 1. Basic Memory Agent Implementation

This example demonstrates basic memory agent creation and operation.

```csharp
// Minimal runnable memory agent example (documentation mirrors the runnable file in examples/MemoryAgentExample.cs)
// Create a local kernel for the demo
var kernel = Kernel.CreateBuilder().Build();

// Simple in-memory store used by the example (see examples/MemoryAgentExample.cs for full implementation)
var memoryStore = new InMemoryMemoryStore();

// Create a graph executor backed by the kernel
var workflow = new GraphExecutor(kernel);

// Create a kernel function that performs retrieval, processing and storage
var memoryFn = kernel.CreateFunctionFromMethod(async (KernelArguments args) =>
{
    var userInput = args.GetValueOrDefault("user_input")?.ToString() ?? "";
    var sessionId = args.GetValueOrDefault("session_id")?.ToString() ?? Guid.NewGuid().ToString();

    // Retrieve relevant memories (token-overlap heuristic)
    var relevant = await memoryStore.RetrieveAsync(userInput, sessionId);

    // Build a simple response
    var response = $"Echo: {userInput} (found {relevant.Count} memories)";

    // Store a new memory entry
    var entry = new MemoryEntry
    {
        Id = Guid.NewGuid().ToString(),
        Content = userInput,
        Response = response,
        SessionId = sessionId,
        Timestamp = DateTime.UtcNow,
        Tags = new List<string> { "doc-example" },
        Importance = 0.5
    };

    await memoryStore.StoreAsync(entry);

    // Persist results in graph state
    var state = args.GetOrCreateGraphState();
    state.SetValue("agent_response", response);
    state.SetValue("memories_retrieved", relevant.Count);
    state.SetValue("new_memory_stored", true);
    state.SetValue("memory_entry_id", entry.Id);

    // Return a compact payload (response + diagnostics)
    const char DELIM = '\u0001';
    return $"{response}{DELIM}{relevant.Count}{DELIM}true";
}, functionName: "doc_memory_agent_fn", description: "Documentation memory agent function");

var memoryAgent = new FunctionGraphNode(memoryFn, "memory-agent");
workflow.AddNode(memoryAgent);
workflow.SetStartNode(memoryAgent.NodeId);

Console.WriteLine("[doc] 🧠 Testing memory agent...\n");

var testInputs = new[] { "What is machine learning?", "Tell me about neural networks", "How does deep learning work?" };
foreach (var input in testInputs)
{
    var args = new KernelArguments { ["user_input"] = input, ["session_id"] = "doc-session-001" };
    Console.WriteLine($"[doc] Input: {input}");

    var result = await workflow.ExecuteAsync(kernel, args);
    var raw = result.GetValue<string>() ?? string.Empty;
    const char DELIM = '\u0001';
    var parts = raw.Split(DELIM);
    var agentResponse = parts.Length > 0 ? parts[0] : string.Empty;
    var memoriesRetrieved = parts.Length > 1 && int.TryParse(parts[1], out var m) ? m : 0;
    var newMemoryStored = parts.Length > 2 && bool.TryParse(parts[2], out var b) ? b : false;

    Console.WriteLine($"[doc] Response: {agentResponse}");
    Console.WriteLine($"[doc] Memories Retrieved: {memoriesRetrieved}");
    Console.WriteLine($"[doc] New Memory Stored: {newMemoryStored}\n");
}
```

### 2. Advanced Memory Retrieval

Demonstrates advanced memory retrieval with semantic search and context awareness.

```csharp
// Advanced retrieval — this snippet shows an orchestration pattern. Replace these placeholder functions
// with real semantic search / ranking implementations when integrating with a vector store or LLM.
var advancedMemoryWorkflow = new GraphExecutor("AdvancedMemoryWorkflow", logger: ConsoleLogger.Instance);

// Semantic memory searcher (placeholder)
var semanticMemorySearcher = new FunctionGraphNode(
    "semantic-memory-searcher",
    "Perform semantic search in memory",
    async (context) =>
    {
        var query = context.GetValue<string>("query") ?? string.Empty;
        var contextInfo = context.GetValue<Dictionary<string, object>>("context_info") ?? new Dictionary<string, object>();
        var searchDepth = context.GetValue<int>("search_depth", 3);

        // NOTE: Replace PerformSemanticSearch with your vector-store or embedding-based search.
        var semanticResults = await PerformSemanticSearch(query, contextInfo, searchDepth);

        // Rank and cluster (placeholder helpers)
        var rankedResults = await RankMemoryResults(semanticResults, contextInfo);
        var clusteredResults = await ClusterRelatedMemories(rankedResults);

        context.SetValue("semantic_results", semanticResults);
        context.SetValue("ranked_results", rankedResults);
        context.SetValue("clustered_results", clusteredResults);
        context.SetValue("search_completed", true);

        return $"Semantic search completed: {semanticResults.Count} results found";
    });

var contextAwareAnalyzer = new FunctionGraphNode(
    "context-aware-analyzer",
    "Analyze memories with context awareness",
    async (context) =>
    {
        var rankedResults = context.GetValue<List<RankedMemory>>("ranked_results") ?? new List<RankedMemory>();
        var contextInfo = context.GetValue<Dictionary<string, object>>("context_info") ?? new Dictionary<string, object>();

        var contextAnalysis = await AnalyzeContextRelevance(rankedResults, contextInfo);

        var contextualInsights = new Dictionary<string, object>
        {
            ["context_relevance_scores"] = contextAnalysis.RelevanceScores,
            ["context_patterns"] = contextAnalysis.Patterns,
            ["contextual_recommendations"] = contextAnalysis.Recommendations,
            ["context_confidence"] = contextAnalysis.Confidence
        };

        context.SetValue("context_analysis", contextAnalysis);
        context.SetValue("contextual_insights", contextualInsights);

        return $"Context analysis completed: Confidence {contextAnalysis.Confidence:F2}";
    });

var memorySynthesizer = new FunctionGraphNode(
    "memory-synthesizer",
    "Synthesize memories into coherent response",
    async (context) =>
    {
        var query = context.GetValue<string>("query") ?? string.Empty;
        var clusteredResults = context.GetValue<List<MemoryCluster>>("clustered_results") ?? new List<MemoryCluster>();
        var contextualInsights = context.GetValue<Dictionary<string, object>>("contextual_insights") ?? new Dictionary<string, object>();

        var synthesizedResponse = await SynthesizeMemories(query, clusteredResults, contextualInsights);

        var memorySummary = new Dictionary<string, object>
        {
            ["synthesized_response"] = synthesizedResponse.Response,
            ["memory_sources"] = synthesizedResponse.MemorySources,
            ["confidence_score"] = synthesizedResponse.Confidence,
            ["synthesis_method"] = synthesizedResponse.Method,
            ["synthesis_timestamp"] = DateTime.UtcNow
        };

        context.SetValue("synthesized_response", synthesizedResponse);
        context.SetValue("memory_summary", memorySummary);

        return $"Memory synthesis completed: {synthesizedResponse.MemorySources.Count} sources used";
    });

advancedMemoryWorkflow.AddNode(semanticMemorySearcher);
advancedMemoryWorkflow.AddNode(contextAwareAnalyzer);
advancedMemoryWorkflow.AddNode(memorySynthesizer);
advancedMemoryWorkflow.SetStartNode(semanticMemorySearcher.NodeId);

Console.WriteLine("🔍 Testing advanced memory retrieval...");

var advancedQueries = new[] { "Explain the relationship between AI and machine learning", "What are the latest developments in neural networks?", "How do different learning algorithms compare?" };
foreach (var q in advancedQueries)
{
    var arguments = new KernelArguments
    {
        ["query"] = q,
        ["context_info"] = new Dictionary<string, object> { ["user_level"] = "intermediate", ["domain"] = "artificial_intelligence" },
        ["search_depth"] = 5
    };

    Console.WriteLine($"   Query: {q}");
    var result = await advancedMemoryWorkflow.ExecuteAsync(kernel, arguments);

    var searchCompleted = result.GetValue<bool>("search_completed");
    var memorySummary = result.GetValue<Dictionary<string, object>>("memory_summary");
    if (searchCompleted && memorySummary != null)
    {
        Console.WriteLine($"   Search Completed: {searchCompleted}");
        Console.WriteLine($"   Confidence Score: {memorySummary["confidence_score"]}");
        Console.WriteLine($"   Memory Sources: {memorySummary["memory_sources"]}");
        Console.WriteLine($"   Synthesis Method: {memorySummary["synthesis_method"]}");
    }
    Console.WriteLine();
}
```

### 3. Learning and Adaptation

Shows how to implement learning and adaptation mechanisms for memory agents.

```csharp
// Create learning and adaptation workflow
var learningWorkflow = new GraphExecutor("LearningWorkflow", "Learning and adaptation", logger);

// Configure learning options
var learningOptions = new LearningOptions
{
    EnableExperienceLearning = true,
    EnablePatternRecognition = true,
    EnableAdaptiveBehavior = true,
    EnableKnowledgeExpansion = true,
    LearningRate = 0.1,
    AdaptationThreshold = 0.7,
    PatternRecognitionThreshold = 0.6
};

learningWorkflow.ConfigureLearning(learningOptions);

// Experience learner node
var experienceLearner = new FunctionGraphNode(
    "experience-learner",
    "Learn from new experiences",
    async (context) =>
    {
        var userInput = context.GetValue<string>("user_input");
        var agentResponse = context.GetValue<string>("agent_response");
        var userFeedback = context.GetValue<string>("user_feedback", "positive");
        var interactionQuality = context.GetValue<double>("interaction_quality", 0.8);
        
        // Learn from experience
        var learningOutcome = await LearnFromExperience(userInput, agentResponse, userFeedback, interactionQuality);
        
        // Update learning metrics
        var updatedMetrics = new Dictionary<string, object>
        {
            ["learning_outcome"] = learningOutcome,
            ["knowledge_gained"] = learningOutcome.KnowledgeGained,
            ["skill_improvement"] = learningOutcome.SkillImprovement,
            ["adaptation_level"] = learningOutcome.AdaptationLevel,
            ["learning_timestamp"] = DateTime.UtcNow
        };
        
        context.SetValue("learning_outcome", learningOutcome);
        context.SetValue("updated_metrics", updatedMetrics);
        
        return $"Learning completed: Knowledge gained {learningOutcome.KnowledgeGained:F2}";
    });

// Pattern recognizer node
var patternRecognizer = new FunctionGraphNode(
    "pattern-recognizer",
    "Recognize patterns in interactions",
    async (context) =>
    {
        var sessionId = context.GetValue<string>("session_id");
        var interactionHistory = context.GetValue<List<Interaction>>("interaction_history");
        var learningOutcome = context.GetValue<LearningOutcome>("learning_outcome");
        
        // Recognize patterns
        var patterns = await RecognizePatterns(sessionId, interactionHistory, learningOutcome);
        
        // Generate pattern insights
        var patternInsights = new Dictionary<string, object>
        {
            ["recognized_patterns"] = patterns.IdentifiedPatterns,
            ["pattern_confidence"] = patterns.Confidence,
            ["pattern_recommendations"] = patterns.Recommendations,
            ["pattern_learning_value"] = patterns.LearningValue
        };
        
        context.SetValue("recognized_patterns", patterns);
        context.SetValue("pattern_insights", patternInsights);
        
        return $"Pattern recognition completed: {patterns.IdentifiedPatterns.Count} patterns found";
    });

// Adaptive behavior generator
var adaptiveBehaviorGenerator = new FunctionGraphNode(
    "adaptive-behavior-generator",
    "Generate adaptive behaviors based on learning",
    async (context) =>
    {
        var learningOutcome = context.GetValue<LearningOutcome>("learning_outcome");
        var recognizedPatterns = context.GetValue<PatternRecognition>("recognized_patterns");
        var currentContext = context.GetValue<Dictionary<string, object>>("current_context");
        
        // Generate adaptive behaviors
        var adaptiveBehaviors = await GenerateAdaptiveBehaviors(learningOutcome, recognizedPatterns, currentContext);
        
        // Update behavior state
        var behaviorState = new Dictionary<string, object>
        {
            ["adaptive_behaviors"] = adaptiveBehaviors.Behaviors,
            ["behavior_confidence"] = adaptiveBehaviors.Confidence,
            ["adaptation_reason"] = adaptiveBehaviors.AdaptationReason,
            ["behavior_timestamp"] = DateTime.UtcNow
        };
        
        context.SetValue("adaptive_behaviors", adaptiveBehaviors);
        context.SetValue("behavior_state", behaviorState);
        
        return $"Adaptive behaviors generated: {adaptiveBehaviors.Behaviors.Count} behaviors";
    });

// Add nodes to learning workflow
learningWorkflow.AddNode(experienceLearner);
learningWorkflow.AddNode(patternRecognizer);
learningWorkflow.AddNode(adaptiveBehaviorGenerator);

// Set start node
learningWorkflow.SetStartNode(experienceLearner.NodeId);

// Test learning and adaptation
Console.WriteLine("📚 Testing learning and adaptation...");

var learningScenarios = new[]
{
    new { Input = "Explain quantum computing", Feedback = "positive", Quality = 0.9 },
    new { Input = "What is blockchain?", Feedback = "neutral", Quality = 0.7 },
    new { Input = "How does encryption work?", Feedback = "positive", Quality = 0.8 }
};

foreach (var scenario in learningScenarios)
{
    var arguments = new KernelArguments
    {
        ["user_input"] = scenario.Input,
        ["agent_response"] = $"Response to: {scenario.Input}",
        ["user_feedback"] = scenario.Feedback,
        ["interaction_quality"] = scenario.Quality,
        ["session_id"] = "learning-session-001",
        ["interaction_history"] = new List<Interaction>(),
        ["current_context"] = new Dictionary<string, object>()
    };

    Console.WriteLine($"   Input: {scenario.Input}");
    Console.WriteLine($"   Feedback: {scenario.Feedback}");
    Console.WriteLine($"   Quality: {scenario.Quality}");
    
    var result = await learningWorkflow.ExecuteAsync(kernel, arguments);
    
    var learningOutcome = result.GetValue<LearningOutcome>("learning_outcome");
    var patternInsights = result.GetValue<Dictionary<string, object>>("pattern_insights");
    var behaviorState = result.GetValue<Dictionary<string, object>>("behavior_state");
    
    if (learningOutcome != null)
    {
        Console.WriteLine($"   Knowledge Gained: {learningOutcome.KnowledgeGained:F2}");
        Console.WriteLine($"   Skill Improvement: {learningOutcome.SkillImprovement:F2}");
    }
    
    Console.WriteLine();
}
```

### 4. Memory Management and Optimization

Demonstrates memory management, cleanup, and optimization strategies.

```csharp
// Create memory management workflow
var memoryManagementWorkflow = new GraphExecutor("MemoryManagementWorkflow", "Memory management and optimization", logger);

// Configure memory management options
var memoryManagementOptions = new MemoryManagementOptions
{
    EnableMemoryCleanup = true,
    EnableMemoryOptimization = true,
    EnableMemoryCompression = true,
    EnableMemoryArchiving = true,
    CleanupThreshold = 0.8,
    CompressionThreshold = 0.6,
    ArchiveThreshold = 0.3
};

memoryManagementWorkflow.ConfigureMemoryManagement(memoryManagementOptions);

// Memory analyzer node
var memoryAnalyzer = new FunctionGraphNode(
    "memory-analyzer",
    "Analyze memory usage and performance",
    async (context) =>
    {
        var sessionId = context.GetValue<string>("session_id");
        var analysisDepth = context.GetValue<int>("analysis_depth", 5);
        
        // Analyze memory usage
        var memoryAnalysis = await AnalyzeMemoryUsage(sessionId, analysisDepth);
        
        // Generate optimization recommendations
        var optimizationRecommendations = await GenerateOptimizationRecommendations(memoryAnalysis);
        
        // Update analysis state
        context.SetValue("memory_analysis", memoryAnalysis);
        context.SetValue("optimization_recommendations", optimizationRecommendations);
        context.SetValue("analysis_completed", true);
        
        return $"Memory analysis completed: {optimizationRecommendations.Count} recommendations";
    });

// Memory optimizer node
var memoryOptimizer = new FunctionGraphNode(
    "memory-optimizer",
    "Optimize memory storage and retrieval",
    async (context) =>
    {
        var memoryAnalysis = context.GetValue<MemoryUsageAnalysis>("memory_analysis");
        var optimizationRecommendations = context.GetValue<List<OptimizationRecommendation>>("optimization_recommendations");
        
        // Apply optimizations
        var optimizationResults = await ApplyMemoryOptimizations(memoryAnalysis, optimizationRecommendations);
        
        // Update optimization state
        context.SetValue("optimization_results", optimizationResults);
        context.SetValue("optimization_completed", true);
        
        return $"Memory optimization completed: {optimizationResults.AppliedOptimizations.Count} optimizations applied";
    });

// Memory cleanup node
var memoryCleanup = new FunctionGraphNode(
    "memory-cleanup",
    "Clean up and compress memory",
    async (context) =>
    {
        var memoryAnalysis = context.GetValue<MemoryUsageAnalysis>("memory_analysis");
        var optimizationResults = context.GetValue<OptimizationResults>("optimization_results");
        
        // Perform memory cleanup
        var cleanupResults = await PerformMemoryCleanup(memoryAnalysis, optimizationResults);
        
        // Update cleanup state
        context.SetValue("cleanup_results", cleanupResults);
        context.SetValue("cleanup_completed", true);
        
        return $"Memory cleanup completed: {cleanupResults.CleanedEntries} entries cleaned";
    });

// Add nodes to management workflow
memoryManagementWorkflow.AddNode(memoryAnalyzer);
memoryManagementWorkflow.AddNode(memoryOptimizer);
memoryManagementWorkflow.AddNode(memoryCleanup);

// Set start node
memoryManagementWorkflow.SetStartNode(memoryAnalyzer.NodeId);

// Test memory management
Console.WriteLine("🧹 Testing memory management and optimization...");

var managementArguments = new KernelArguments
{
    ["session_id"] = "management-session-001",
    ["analysis_depth"] = 7
};

var result = await memoryManagementWorkflow.ExecuteAsync(kernel, managementArguments);

var analysisCompleted = result.GetValue<bool>("analysis_completed");
var optimizationCompleted = result.GetValue<bool>("optimization_completed");
var cleanupCompleted = result.GetValue<bool>("cleanup_completed");

if (analysisCompleted && optimizationCompleted && cleanupCompleted)
{
    var optimizationResults = result.GetValue<OptimizationResults>("optimization_results");
    var cleanupResults = result.GetValue<CleanupResults>("cleanup_results");
    
    Console.WriteLine($"   Analysis Completed: {analysisCompleted}");
    Console.WriteLine($"   Optimization Completed: {optimizationCompleted}");
    Console.WriteLine($"   Cleanup Completed: {cleanupCompleted}");
    Console.WriteLine($"   Optimizations Applied: {optimizationResults.AppliedOptimizations.Count}");
    Console.WriteLine($"   Entries Cleaned: {cleanupResults.CleanedEntries}");
}
```

## Expected Output

### Basic Memory Agent Example

```
🧠 Testing basic memory agent...
   Input: What is machine learning?
   Response: Machine learning is a subset of artificial intelligence...
   Memories Retrieved: 3
   New Memory Stored: True

   Input: Tell me about neural networks
   Response: Neural networks are computational models inspired by...
   Memories Retrieved: 2
   New Memory Stored: True
```

### Advanced Memory Retrieval Example

```
🔍 Testing advanced memory retrieval...
   Query: Explain the relationship between AI and machine learning
   Search Completed: True
   Confidence Score: 0.85
   Memory Sources: 5
   Synthesis Method: semantic_clustering

   Query: What are the latest developments in neural networks?
   Search Completed: True
   Confidence Score: 0.78
   Memory Sources: 3
   Synthesis Method: temporal_ranking
```

### Learning and Adaptation Example

```
📚 Testing learning and adaptation...
   Input: Explain quantum computing
   Feedback: positive
   Quality: 0.9
   Knowledge Gained: 0.85
   Skill Improvement: 0.72

   Input: What is blockchain?
   Feedback: neutral
   Quality: 0.7
   Knowledge Gained: 0.62
   Skill Improvement: 0.58
```

### Memory Management Example

```
🧹 Testing memory management and optimization...
   Analysis Completed: True
   Optimization Completed: True
   Cleanup Completed: True
   Optimizations Applied: 4
   Entries Cleaned: 15
```

## Configuration Options

### Memory Agent Configuration

```csharp
var memoryAgentOptions = new MemoryAgentOptions
{
    EnableMemoryStorage = true,                     // Enable memory storage
    EnableMemoryRetrieval = true,                   // Enable memory retrieval
    EnableLearning = true,                          // Enable learning capabilities
    EnableContextAwareness = true,                  // Enable context awareness
    MemoryRetentionDays = 30,                       // Memory retention period
    MaxMemorySize = 1000,                           // Maximum memory size
    EnableMemoryCompression = true,                 // Enable memory compression
    EnableMemoryEncryption = true,                  // Enable memory encryption
    EnableMemoryBackup = true,                      // Enable memory backup
    BackupInterval = TimeSpan.FromHours(24)         // Backup interval
};
```

### Advanced Memory Configuration

```csharp
var advancedMemoryOptions = new AdvancedMemoryOptions
{
    EnableSemanticSearch = true,                    // Enable semantic search
    EnableContextualRetrieval = true,               // Enable contextual retrieval
    EnableMemoryRanking = true,                     // Enable memory ranking
    EnableMemoryClustering = true,                  // Enable memory clustering
    SemanticSearchThreshold = 0.7,                  // Semantic search threshold
    ContextRelevanceWeight = 0.6,                   // Context relevance weight
    TemporalRelevanceWeight = 0.4,                  // Temporal relevance weight
    EnableFuzzyMatching = true,                     // Enable fuzzy matching
    EnableMemoryDeduplication = true,               // Enable memory deduplication
    MaxSearchResults = 50                           // Maximum search results
};
```

### Learning Configuration

```csharp
var learningOptions = new LearningOptions
{
    EnableExperienceLearning = true,                // Enable experience learning
    EnablePatternRecognition = true,                // Enable pattern recognition
    EnableAdaptiveBehavior = true,                  // Enable adaptive behavior
    EnableKnowledgeExpansion = true,                // Enable knowledge expansion
    LearningRate = 0.1,                             // Learning rate
    AdaptationThreshold = 0.7,                      // Adaptation threshold
    PatternRecognitionThreshold = 0.6,              // Pattern recognition threshold
    EnableIncrementalLearning = true,               // Enable incremental learning
    EnableTransferLearning = true,                  // Enable transfer learning
    MaxLearningIterations = 100                     // Maximum learning iterations
};
```

## Troubleshooting

### Common Issues

#### Memory Not Being Stored
```bash
# Problem: Memories are not being stored
# Solution: Check memory storage configuration
EnableMemoryStorage = true;
MemoryRetentionDays = 30;
MaxMemorySize = 1000;
```

#### Poor Memory Retrieval
```bash
# Problem: Memory retrieval quality is poor
# Solution: Adjust search thresholds and enable semantic search
EnableSemanticSearch = true;
SemanticSearchThreshold = 0.7;
ContextRelevanceWeight = 0.6;
```

#### Learning Not Working
```bash
# Problem: Learning mechanisms are not working
# Solution: Check learning configuration and enable required features
EnableExperienceLearning = true;
EnablePatternRecognition = true;
LearningRate = 0.1;
```

### Debug Mode

Enable detailed memory monitoring for troubleshooting:

```csharp
// Enable debug memory monitoring
var debugMemoryOptions = new MemoryAgentOptions
{
    EnableMemoryStorage = true,
    EnableMemoryRetrieval = true,
    EnableLearning = true,
    EnableDebugLogging = true,
    EnableMemoryInspection = true,
    EnablePerformanceMonitoring = true,
    LogMemoryOperations = true,
    LogLearningProgress = true
};
```

## Advanced Patterns

### Custom Memory Stores

```csharp
// Implement custom memory store
public class CustomMemoryStore : IMemoryStore
{
    public async Task<bool> StoreMemoryAsync(MemoryEntry entry)
    {
        // Custom storage logic
        var storageResult = await StoreInCustomDatabase(entry);
        
        // Add custom metadata
        entry.Metadata["custom_stored"] = true;
        entry.Metadata["storage_timestamp"] = DateTime.UtcNow;
        
        return storageResult;
    }
    
    public async Task<List<MemoryEntry>> RetrieveMemoriesAsync(string query, Dictionary<string, object> context)
    {
        // Custom retrieval logic
        var memories = await RetrieveFromCustomDatabase(query, context);
        
        // Apply custom filtering
        memories = await ApplyCustomFilters(memories, context);
        
        return memories;
    }
}
```

### Custom Learning Algorithms

```csharp
// Implement custom learning algorithm
public class CustomLearningAlgorithm : ILearningAlgorithm
{
    public async Task<LearningOutcome> LearnFromExperienceAsync(Experience experience)
    {
        var outcome = new LearningOutcome();
        
        // Custom learning logic
        outcome.KnowledgeGained = await CalculateKnowledgeGain(experience);
        outcome.SkillImprovement = await CalculateSkillImprovement(experience);
        outcome.AdaptationLevel = await CalculateAdaptationLevel(experience);
        
        // Apply custom learning rules
        await ApplyCustomLearningRules(experience, outcome);
        
        return outcome;
    }
    
    private async Task<double> CalculateKnowledgeGain(Experience experience)
    {
        // Custom knowledge gain calculation
        var baseGain = experience.Quality * 0.8;
        var feedbackMultiplier = GetFeedbackMultiplier(experience.Feedback);
        var contextBonus = GetContextBonus(experience.Context);
        
        return Math.Min(1.0, baseGain * feedbackMultiplier + contextBonus);
    }
}
```

### Memory Optimization Strategies

```csharp
// Implement memory optimization strategy
public class MemoryOptimizationStrategy : IMemoryOptimizationStrategy
{
    public async Task<OptimizationResult> OptimizeMemoryAsync(MemoryUsageAnalysis analysis)
    {
        var result = new OptimizationResult();
        
        // Apply compression if needed
        if (analysis.CompressionRatio < 0.6)
        {
            result.AppliedOptimizations.Add(await CompressMemories(analysis));
        }
        
        // Apply deduplication if needed
        if (analysis.DuplicationRate > 0.2)
        {
            result.AppliedOptimizations.Add(await DeduplicateMemories(analysis));
        }
        
        // Apply archiving if needed
        if (analysis.AccessFrequency < 0.3)
        {
            result.AppliedOptimizations.Add(await ArchiveMemories(analysis));
        }
        
        return result;
    }
    
    private async Task<Optimization> CompressMemories(MemoryUsageAnalysis analysis)
    {
        // Implement memory compression
        var compressionRatio = await CompressMemoryData(analysis.Memories);
        
        return new Optimization
        {
            Type = "compression",
            Impact = compressionRatio,
            Description = $"Compressed memories with ratio {compressionRatio:F2}"
        };
    }
}
```

## Related Examples

* [Multi-Agent System](./multi-agent.md): Multi-agent coordination with memory
* [Graph Metrics](./graph-metrics.md): Memory performance monitoring
* [State Management](./state-tutorial.md): Memory state persistence
* [Performance Optimization](./performance-optimization.md): Memory optimization techniques

## See Also

* [Memory Patterns](../concepts/memory.md): Understanding memory concepts
* [Learning and Adaptation](../how-to/learning-adaptation.md): Memory-based learning
* [Performance Monitoring](../how-to/performance-monitoring.md): Memory performance analysis
* [API Reference](../api/): Complete API documentation
