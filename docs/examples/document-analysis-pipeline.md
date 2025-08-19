# Document Analysis Pipeline Example

This example demonstrates a comprehensive document processing workflow using the Semantic Kernel Graph. It shows how to build a multi-stage pipeline for document ingestion, analysis, classification, and information extraction with parallel processing and error handling.

## Objective

Learn how to implement a document analysis pipeline in graph-based workflows to:
- Process documents through multiple analysis stages
- Implement parallel processing for performance optimization
- Handle different document types and formats
- Extract structured information from unstructured text
- Build resilient pipelines with error handling and recovery
- Scale document processing across multiple workers

## Prerequisites

- **.NET 8.0** or later
- **OpenAI API Key** configured in `appsettings.json`
- **Semantic Kernel Graph package** installed
- Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [Node Types](../concepts/node-types.md)

## Key Components

### Concepts and Techniques

- **Pipeline Processing**: Sequential execution of document analysis stages
- **Parallel Processing**: Concurrent execution of independent analysis tasks
- **Document Classification**: Automatic categorization of documents by type and content
- **Information Extraction**: Structured data extraction from unstructured text
- **Error Handling**: Graceful handling of processing failures and recovery

### Core Classes

- `FunctionGraphNode`: Executes document processing functions
- `ConditionalGraphNode`: Routes documents based on type and content
- `GraphExecutor`: Orchestrates the document analysis pipeline
- `GraphState`: Manages document state and metadata throughout processing

## Running the Example

### Getting Started

This example demonstrates document analysis pipeline workflows with the Semantic Kernel Graph package. The code snippets below show you how to implement this pattern in your own applications.

## Step-by-Step Implementation

### 1. Basic Document Analysis Pipeline

This example demonstrates a simple three-stage document processing workflow.

```csharp
// Create kernel with mock configuration
var kernel = CreateKernel();

// Create document analysis pipeline
var pipeline = new GraphExecutor("DocumentAnalysisPipeline", "Basic document analysis", logger);

// Stage 1: Document Ingestion
var documentIngestion = new FunctionGraphNode(
    "document-ingestion",
    "Ingest and validate document",
    async (context) =>
    {
        var documentPath = context.GetValue<string>("document_path");
        var documentContent = await File.ReadAllTextAsync(documentPath);
        
        // Extract basic metadata
        var fileInfo = new FileInfo(documentPath);
        var metadata = new Dictionary<string, object>
        {
            ["file_name"] = fileInfo.Name,
            ["file_size"] = fileInfo.Length,
            ["file_extension"] = fileInfo.Extension,
            ["ingestion_timestamp"] = DateTime.UtcNow,
            ["content_length"] = documentContent.Length
        };
        
        context.SetValue("document_content", documentContent);
        context.SetValue("document_metadata", metadata);
        context.SetValue("processing_status", "ingested");
        
        return $"Document ingested: {fileInfo.Name} ({fileInfo.Length} bytes)";
    });

// Stage 2: Document Classification
var documentClassification = new FunctionGraphNode(
    "document-classification",
    "Classify document by type and content",
    async (context) =>
    {
        var content = context.GetValue<string>("document_content");
        var extension = context.GetValue<string>("file_extension");
        
        // Simple classification logic
        var documentType = extension.ToLower() switch
        {
            ".txt" => "text",
            ".md" => "markdown",
            ".pdf" => "pdf",
            ".doc" => "word",
            ".docx" => "word",
            _ => "unknown"
        };
        
        // Content-based classification
        var contentCategory = content.ToLower() switch
        {
            var c when c.Contains("invoice") || c.Contains("bill") => "financial",
            var c when c.Contains("contract") || c.Contains("agreement") => "legal",
            var c when c.Contains("report") || c.Contains("analysis") => "report",
            var c when c.Contains("email") || c.Contains("correspondence") => "communication",
            _ => "general"
        };
        
        context.SetValue("document_type", documentType);
        context.SetValue("content_category", contentCategory);
        context.SetValue("processing_status", "classified");
        
        return $"Document classified as {documentType} ({content_category})";
    });

// Stage 3: Content Analysis
var contentAnalysis = new FunctionGraphNode(
    "content-analysis",
    "Analyze document content and extract key information",
    async (context) =>
    {
        var content = context.GetValue<string>("document_content");
        var documentType = context.GetValue<string>("document_type");
        var contentCategory = context.GetValue<string>("content_category");
        
        // Extract key information based on document type
        var analysis = new Dictionary<string, object>();
        
        switch (contentCategory)
        {
            case "financial":
                analysis["key_amounts"] = ExtractAmounts(content);
                analysis["dates"] = ExtractDates(content);
                analysis["parties"] = ExtractParties(content);
                break;
            case "legal":
                analysis["contract_parties"] = ExtractParties(content);
                analysis["effective_dates"] = ExtractDates(content);
                analysis["key_terms"] = ExtractKeyTerms(content);
                break;
            case "report":
                analysis["summary"] = GenerateSummary(content);
                analysis["key_findings"] = ExtractFindings(content);
                analysis["recommendations"] = ExtractRecommendations(content);
                break;
            default:
                analysis["word_count"] = content.Split(' ').Length;
                analysis["paragraph_count"] = content.Split('\n').Length;
                analysis["key_topics"] = ExtractTopics(content);
                break;
        }
        
        context.SetValue("content_analysis", analysis);
        context.SetValue("processing_status", "analyzed");
        context.SetValue("analysis_timestamp"] = DateTime.UtcNow);
        
        return $"Content analysis completed for {documentType} document";
    });

// Add nodes to pipeline
pipeline.AddNode(documentIngestion);
pipeline.AddNode(documentClassification);
pipeline.AddNode(contentAnalysis);

// Set start node
pipeline.SetStartNode(documentIngestion.NodeId);

// Test with sample documents
var testDocuments = new[]
{
    "sample_invoice.txt",
    "contract_agreement.md",
    "quarterly_report.txt"
};

foreach (var docPath in testDocuments)
{
    var arguments = new KernelArguments
    {
        ["document_path"] = docPath
    };

    Console.WriteLine($"📄 Processing document: {docPath}");
    var result = await pipeline.ExecuteAsync(kernel, arguments);
    
    var status = result.GetValue<string>("processing_status");
    var docType = result.GetValue<string>("document_type");
    var category = result.GetValue<string>("content_category");
    
    Console.WriteLine($"   Status: {status}");
    Console.WriteLine($"   Type: {docType}");
    Console.WriteLine($"   Category: {category}");
    
    if (result.TryGetValue("content_analysis", out var analysis))
    {
        Console.WriteLine($"   Analysis: {analysis}");
    }
    Console.WriteLine();
}
```

### 2. Advanced Pipeline with Parallel Processing

Demonstrates parallel execution of independent analysis tasks for improved performance.

```csharp
// Create advanced pipeline with parallel processing
var advancedPipeline = new GraphExecutor("AdvancedDocumentPipeline", "Parallel document analysis", logger);

// Document ingestion (sequential)
var advancedIngestion = new FunctionGraphNode(
    "advanced-ingestion",
    "Advanced document ingestion with validation",
    async (context) =>
    {
        var documentPath = context.GetValue<string>("document_path");
        var documentContent = await File.ReadAllTextAsync(documentPath);
        
        // Validate document
        if (string.IsNullOrWhiteSpace(documentContent))
        {
            throw new InvalidOperationException("Document content is empty");
        }
        
        // Extract comprehensive metadata
        var fileInfo = new FileInfo(documentPath);
        var metadata = new Dictionary<string, object>
        {
            ["file_name"] = fileInfo.Name,
            ["file_size"] = fileInfo.Length,
            ["file_extension"] = fileInfo.Extension,
            ["ingestion_timestamp"] = DateTime.UtcNow,
            ["content_length"] = documentContent.Length,
            ["line_count"] = documentContent.Split('\n').Length,
            ["word_count"] = documentContent.Split(' ').Length,
            ["character_count"] = documentContent.Length
        };
        
        context.SetValue("document_content", documentContent);
        context.SetValue("document_metadata", metadata);
        context.SetValue("processing_status", "ingested");
        
        return $"Advanced ingestion completed: {fileInfo.Name}";
    });

// Parallel analysis tasks
var textAnalysis = new FunctionGraphNode(
    "text-analysis",
    "Analyze text content and structure",
    async (context) =>
    {
        var content = context.GetValue<string>("document_content");
        
        // Text analysis
        var analysis = new Dictionary<string, object>
        {
            ["readability_score"] = CalculateReadabilityScore(content),
            ["sentiment_score"] = AnalyzeSentiment(content),
            ["language_detected"] = DetectLanguage(content),
            ["key_phrases"] = ExtractKeyPhrases(content),
            ["named_entities"] = ExtractNamedEntities(content)
        };
        
        context.SetValue("text_analysis", analysis);
        return "Text analysis completed";
    });

var structureAnalysis = new FunctionGraphNode(
    "structure-analysis",
    "Analyze document structure and formatting",
    async (context) =>
    {
        var content = context.GetValue<string>("document_content");
        
        // Structure analysis
        var structure = new Dictionary<string, object>
        {
            ["sections"] = IdentifySections(content),
            ["headers"] = ExtractHeaders(content),
            ["lists"] = CountLists(content),
            ["tables"] = CountTables(content),
            ["formatting"] = AnalyzeFormatting(content)
        };
        
        context.SetValue("structure_analysis", structure);
        return "Structure analysis completed";
    });

var semanticAnalysis = new FunctionGraphNode(
    "semantic-analysis",
    "Perform semantic analysis of content",
    async (context) =>
    {
        var content = context.GetValue<string>("document_content");
        
        // Semantic analysis
        var semantic = new Dictionary<string, object>
        {
            ["topics"] = ExtractTopics(content),
            ["themes"] = IdentifyThemes(content),
            ["relationships"] = FindRelationships(content),
            ["summary"] = GenerateSemanticSummary(content),
            ["key_insights"] = ExtractKeyInsights(content)
        };
        
        context.SetValue("semantic_analysis", semantic);
        return "Semantic analysis completed";
    });

// Results aggregation
var resultsAggregation = new FunctionGraphNode(
    "results-aggregation",
    "Aggregate all analysis results",
    async (context) =>
    {
        var textAnalysis = context.GetValue<Dictionary<string, object>>("text_analysis");
        var structureAnalysis = context.GetValue<Dictionary<string, object>>("structure_analysis");
        var semanticAnalysis = context.GetValue<Dictionary<string, object>>("semantic_analysis");
        var metadata = context.GetValue<Dictionary<string, object>>("document_metadata");
        
        // Combine all results
        var comprehensiveAnalysis = new Dictionary<string, object>
        {
            ["metadata"] = metadata,
            ["text_analysis"] = textAnalysis,
            ["structure_analysis"] = structureAnalysis,
            ["semantic_analysis"] = semanticAnalysis,
            ["analysis_timestamp"] = DateTime.UtcNow,
            ["processing_status"] = "completed"
        };
        
        context.SetValue("comprehensive_analysis", comprehensiveAnalysis);
        
        return "Results aggregation completed";
    });

// Add nodes to pipeline
advancedPipeline.AddNode(advancedIngestion);
advancedPipeline.AddNode(textAnalysis);
advancedPipeline.AddNode(structureAnalysis);
advancedPipeline.AddNode(semanticAnalysis);
advancedPipeline.AddNode(resultsAggregation);

// Set start node
advancedPipeline.SetStartNode(advancedIngestion.NodeId);

// Test advanced pipeline
var advancedArgs = new KernelArguments
{
    ["document_path"] = "complex_document.txt"
};

Console.WriteLine("🚀 Starting advanced document analysis pipeline...");
var advancedResult = await advancedPipeline.ExecuteAsync(kernel, advancedArgs);

var comprehensiveAnalysis = advancedResult.GetValue<Dictionary<string, object>>("comprehensive_analysis");
Console.WriteLine($"✅ Advanced analysis completed");
Console.WriteLine($"   Text Analysis: {comprehensiveAnalysis["text_analysis"]}");
Console.WriteLine($"   Structure Analysis: {comprehensiveAnalysis["structure_analysis"]}");
Console.WriteLine($"   Semantic Analysis: {comprehensiveAnalysis["semantic_analysis"]}");
```

### 3. Error Handling and Recovery Pipeline

Shows how to implement resilient document processing with error handling and recovery mechanisms.

```csharp
// Create resilient pipeline with error handling
var resilientPipeline = new GraphExecutor("ResilientDocumentPipeline", "Error handling and recovery", logger);

// Document ingestion with validation
var resilientIngestion = new FunctionGraphNode(
    "resilient-ingestion",
    "Resilient document ingestion",
    async (context) =>
    {
        try
        {
            var documentPath = context.GetValue<string>("document_path");
            
            if (!File.Exists(documentPath))
            {
                context.SetValue("error_type", "file_not_found");
                context.SetValue("error_message", $"Document not found: {documentPath}");
                context.SetValue("processing_status", "failed");
                return "Document not found";
            }
            
            var documentContent = await File.ReadAllTextAsync(documentPath);
            
            if (string.IsNullOrWhiteSpace(documentContent))
            {
                context.SetValue("error_type", "empty_content");
                context.SetValue("error_message", "Document content is empty");
                context.SetValue("processing_status", "failed");
                return "Empty document content";
            }
            
            // Success path
            context.SetValue("document_content", documentContent);
            context.SetValue("processing_status", "ingested");
            context.SetValue("error_type", "none");
            
            return "Document ingested successfully";
        }
        catch (Exception ex)
        {
            context.SetValue("error_type", "ingestion_error");
            context.SetValue("error_message", ex.Message);
            context.SetValue("processing_status", "failed");
            return $"Ingestion failed: {ex.Message}";
        }
    });

// Conditional routing based on ingestion status
var ingestionRouter = new ConditionalGraphNode(
    "ingestion-router",
    "Route based on ingestion status",
    logger)
{
    ConditionExpression = "processing_status == 'ingested'",
    TrueNodeId = "content-processor",
    FalseNodeId = "error-handler"
};

// Content processor for successful ingestion
var contentProcessor = new FunctionGraphNode(
    "content-processor",
    "Process document content",
    async (context) =>
    {
        try
        {
            var content = context.GetValue<string>("document_content");
            
            // Process content
            var processedContent = await ProcessContentAsync(content);
            context.SetValue("processed_content", processedContent);
            context.SetValue("processing_status", "processed");
            
            return "Content processing completed";
        }
        catch (Exception ex)
        {
            context.SetValue("error_type", "processing_error");
            context.SetValue("error_message", ex.Message);
            context.SetValue("processing_status", "failed");
            return $"Content processing failed: {ex.Message}";
        }
    });

// Error handler for failed operations
var errorHandler = new FunctionGraphNode(
    "error-handler",
    "Handle processing errors",
    async (context) =>
    {
        var errorType = context.GetValue<string>("error_type");
        var errorMessage = context.GetValue<string>("error_message");
        
        // Log error
        Console.WriteLine($"❌ Error in document processing: {errorType} - {errorMessage}");
        
        // Attempt recovery based on error type
        var recoveryAction = errorType switch
        {
            "file_not_found" => "Request document resubmission",
            "empty_content" => "Skip processing and notify user",
            "ingestion_error" => "Retry with exponential backoff",
            "processing_error" => "Fall back to basic processing",
            _ => "Unknown error - manual intervention required"
        };
        
        context.SetValue("recovery_action", recoveryAction);
        context.SetValue("processing_status", "error_handled");
        
        return $"Error handled. Recovery action: {recoveryAction}";
    });

// Add nodes to resilient pipeline
resilientPipeline.AddNode(resilientIngestion);
resilientPipeline.AddNode(ingestionRouter);
resilientPipeline.AddNode(contentProcessor);
resilientPipeline.AddNode(errorHandler);

// Set start node
resilientPipeline.SetStartNode(resilientIngestion.NodeId);

// Test error handling scenarios
var errorTestScenarios = new[]
{
    new { Path = "nonexistent_file.txt", ExpectedError = "file_not_found" },
    new { Path = "empty_file.txt", ExpectedError = "empty_content" },
    new { Path = "valid_document.txt", ExpectedError = "none" }
};

foreach (var scenario in errorTestScenarios)
{
    var resilientArgs = new KernelArguments
    {
        ["document_path"] = scenario.Path
    };

    Console.WriteLine($"\n🧪 Testing error handling: {scenario.Path}");
    var resilientResult = await resilientPipeline.ExecuteAsync(kernel, resilientArgs);
    
    var status = resilientResult.GetValue<string>("processing_status");
    var errorType = resilientResult.GetValue<string>("error_type");
    
    Console.WriteLine($"   Status: {status}");
    Console.WriteLine($"   Error Type: {errorType}");
    
    if (status == "error_handled")
    {
        var recoveryAction = resilientResult.GetValue<string>("recovery_action");
        Console.WriteLine($"   Recovery Action: {recoveryAction}");
    }
}
```

### 4. Multi-Document Batch Processing

Demonstrates processing multiple documents in parallel with result aggregation.

```csharp
// Create batch processing pipeline
var batchPipeline = new GraphExecutor("BatchDocumentPipeline", "Multi-document batch processing", logger);

// Document batch processor
var batchProcessor = new FunctionGraphNode(
    "batch-processor",
    "Process multiple documents in batch",
    async (context) =>
    {
        var documentPaths = context.GetValue<string[]>("document_paths");
        var batchResults = new List<Dictionary<string, object>>();
        
        // Process documents in parallel
        var processingTasks = documentPaths.Select(async (docPath, index) =>
        {
            try
            {
                var content = await File.ReadAllTextAsync(docPath);
                var fileInfo = new FileInfo(docPath);
                
                var result = new Dictionary<string, object>
                {
                    ["document_id"] = $"doc_{index}",
                    ["file_name"] = fileInfo.Name,
                    ["file_size"] = fileInfo.Length,
                    ["content_length"] = content.Length,
                    ["processing_status"] = "processed",
                    ["processing_timestamp"] = DateTime.UtcNow
                };
                
                // Basic analysis
                result["word_count"] = content.Split(' ').Length;
                result["line_count"] = content.Split('\n').Length;
                result["key_topics"] = ExtractTopics(content);
                
                return result;
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object>
                {
                    ["document_id"] = $"doc_{index}",
                    ["file_name"] = docPath,
                    ["processing_status"] = "failed",
                    ["error_message"] = ex.Message,
                    ["processing_timestamp"] = DateTime.UtcNow
                };
            }
        });
        
        var results = await Task.WhenAll(processingTasks);
        batchResults.AddRange(results);
        
        context.SetValue("batch_results", batchResults);
        context.SetValue("total_documents", documentPaths.Length);
        context.SetValue("successful_documents", batchResults.Count(r => r["processing_status"].ToString() == "processed"));
        context.SetValue("failed_documents", batchResults.Count(r => r["processing_status"].ToString() == "failed"));
        
        return $"Batch processing completed: {batchResults.Count} documents processed";
    });

// Batch results analyzer
var batchAnalyzer = new FunctionGraphNode(
    "batch-analyzer",
    "Analyze batch processing results",
    async (context) =>
    {
        var batchResults = context.GetValue<List<Dictionary<string, object>>>("batch_results");
        var totalDocuments = context.GetValue<int>("total_documents");
        var successfulDocuments = context.GetValue<int>("successful_documents");
        var failedDocuments = context.GetValue<int>("failed_documents");
        
        // Calculate statistics
        var totalSize = batchResults
            .Where(r => r["processing_status"].ToString() == "processed")
            .Sum(r => Convert.ToInt64(r["file_size"]));
        
        var totalWords = batchResults
            .Where(r => r["processing_status"].ToString() == "processed")
            .Sum(r => Convert.ToInt32(r["word_count"]));
        
        var analysis = new Dictionary<string, object>
        {
            ["total_documents"] = totalDocuments,
            ["successful_documents"] = successfulDocuments,
            ["failed_documents"] = failedDocuments,
            ["success_rate"] = (double)successfulDocuments / totalDocuments,
            ["total_size_bytes"] = totalSize,
            ["total_words"] = totalWords,
            ["average_file_size"] = totalSize / Math.Max(successfulDocuments, 1),
            ["average_words_per_document"] = totalWords / Math.Max(successfulDocuments, 1),
            ["processing_summary"] = batchResults
        };
        
        context.SetValue("batch_analysis", analysis);
        context.SetValue("processing_status", "batch_completed");
        
        return "Batch analysis completed";
    });

// Add nodes to batch pipeline
batchPipeline.AddNode(batchProcessor);
batchPipeline.AddNode(batchAnalyzer);

// Set start node
batchPipeline.SetStartNode(batchProcessor.NodeId);

// Test batch processing
var batchArgs = new KernelArguments
{
    ["document_paths"] = new[]
    {
        "document1.txt",
        "document2.md",
        "document3.txt",
        "document4.pdf"
    }
};

Console.WriteLine("📚 Starting batch document processing...");
var batchResult = await batchPipeline.ExecuteAsync(kernel, batchArgs);

var batchAnalysis = batchResult.GetValue<Dictionary<string, object>>("batch_analysis");
Console.WriteLine($"✅ Batch processing completed");
Console.WriteLine($"   Total Documents: {batchAnalysis["total_documents"]}");
Console.WriteLine($"   Successful: {batchAnalysis["successful_documents"]}");
Console.WriteLine($"   Failed: {batchAnalysis["failed_documents"]}");
Console.WriteLine($"   Success Rate: {Convert.ToDouble(batchAnalysis["success_rate"]):P1}");
Console.WriteLine($"   Total Size: {Convert.ToInt64(batchAnalysis["total_size_bytes"]):N0} bytes");
Console.WriteLine($"   Total Words: {Convert.ToInt32(batchAnalysis["total_words"]):N0}");
```

## Expected Output

### Basic Document Analysis Pipeline Example

```
📄 Processing document: sample_invoice.txt
   Status: analyzed
   Type: text
   Category: financial
   Analysis: [key_amounts, dates, parties]

📄 Processing document: contract_agreement.md
   Status: analyzed
   Type: markdown
   Category: legal
   Analysis: [contract_parties, effective_dates, key_terms]

📄 Processing document: quarterly_report.txt
   Status: analyzed
   Type: text
   Category: report
   Analysis: [summary, key_findings, recommendations]
```

### Advanced Pipeline with Parallel Processing Example

```
🚀 Starting advanced document analysis pipeline...
✅ Advanced analysis completed
   Text Analysis: [readability_score, sentiment_score, language_detected, key_phrases, named_entities]
   Structure Analysis: [sections, headers, lists, tables, formatting]
   Semantic Analysis: [topics, themes, relationships, summary, key_insights]
```

### Error Handling and Recovery Pipeline Example

```
🧪 Testing error handling: nonexistent_file.txt
   Status: error_handled
   Error Type: file_not_found
   Recovery Action: Request document resubmission

🧪 Testing error handling: empty_file.txt
   Status: error_handled
   Error Type: empty_content
   Recovery Action: Skip processing and notify user

🧪 Testing error handling: valid_document.txt
   Status: processed
   Error Type: none
```

### Multi-Document Batch Processing Example

```
📚 Starting batch document processing...
✅ Batch processing completed
   Total Documents: 4
   Successful: 3
   Failed: 1
   Success Rate: 75.0%
   Total Size: 45,678 bytes
   Total Words: 12,345
```

## Configuration Options

### Pipeline Configuration

```csharp
var pipelineOptions = new DocumentPipelineOptions
{
    EnableParallelProcessing = true,              // Enable parallel execution
    MaxConcurrency = Environment.ProcessorCount, // Maximum parallel tasks
    EnableErrorHandling = true,                   // Enable error handling
    EnableRecovery = true,                        // Enable automatic recovery
    BatchSize = 100,                              // Documents per batch
    ProcessingTimeout = TimeSpan.FromMinutes(30), // Processing timeout
    EnableProgressTracking = true,                 // Track processing progress
    EnableResultCaching = true,                   // Cache analysis results
    StorageProvider = new FileSystemStorageProvider("./pipeline-results")
};
```

### Document Processing Configuration

```csharp
var processingOptions = new DocumentProcessingOptions
{
    SupportedFormats = new[] { ".txt", ".md", ".pdf", ".doc", ".docx" },
    MaxFileSize = 100 * 1024 * 1024,             // 100MB max file size
    EnableContentValidation = true,                // Validate document content
    EnableMetadataExtraction = true,               // Extract document metadata
    EnableContentAnalysis = true,                  // Perform content analysis
    EnableStructureAnalysis = true,                // Analyze document structure
    EnableSemanticAnalysis = true,                 // Perform semantic analysis
    AnalysisDepth = AnalysisDepth.Comprehensive,   // Analysis depth level
    EnableResultPersistence = true                 // Persist analysis results
};
```

## Troubleshooting

### Common Issues

#### Document Ingestion Fails
```bash
# Problem: Documents fail to ingest
# Solution: Check file permissions and validate file format
EnableContentValidation = true;
SupportedFormats = new[] { ".txt", ".md", ".pdf" };
```

#### Parallel Processing Issues
```bash
# Problem: Parallel processing causes errors
# Solution: Reduce concurrency and enable error handling
MaxConcurrency = 2;
EnableErrorHandling = true;
```

#### Memory Issues
```bash
# Problem: Large documents cause memory issues
# Solution: Enable streaming and set memory limits
MaxFileSize = 50 * 1024 * 1024; // 50MB limit
EnableStreaming = true;
```

### Debug Mode

Enable detailed logging for troubleshooting:

```csharp
// Enable debug logging
var logger = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
}).CreateLogger<DocumentAnalysisPipelineExample>();

// Configure pipeline with debug logging
var debugPipeline = new GraphExecutor("DebugPipeline", "Debug document analysis", logger);
debugPipeline.EnableDebugMode = true;
debugPipeline.LogProcessingSteps = true;
debugPipeline.LogErrorDetails = true;
```

## Advanced Patterns

### Custom Document Processors

```csharp
// Implement custom document processors
public class CustomDocumentProcessor : IDocumentProcessor
{
    public async Task<DocumentAnalysisResult> ProcessAsync(DocumentContext context)
    {
        var content = context.Content;
        var metadata = context.Metadata;
        
        // Custom processing logic
        var customAnalysis = await PerformCustomAnalysis(content, metadata);
        
        return new DocumentAnalysisResult
        {
            DocumentId = context.DocumentId,
            AnalysisResults = customAnalysis,
            ProcessingTimestamp = DateTime.UtcNow,
            ProcessorVersion = "1.0.0"
        };
    }
    
    private async Task<Dictionary<string, object>> PerformCustomAnalysis(string content, Dictionary<string, object> metadata)
    {
        // Implement custom analysis logic
        await Task.Delay(100); // Simulate processing
        
        return new Dictionary<string, object>
        {
            ["custom_metric"] = CalculateCustomMetric(content),
            ["domain_specific_analysis"] = PerformDomainAnalysis(content, metadata)
        };
    }
}
```

### Pipeline Orchestration

```csharp
// Orchestrate multiple document processing pipelines
var orchestrator = new DocumentPipelineOrchestrator
{
    PipelineDefinitions = new Dictionary<string, PipelineDefinition>
    {
        ["financial_documents"] = new PipelineDefinition
        {
            EntryCondition = "content_category == 'financial'",
            PipelineGraph = financialPipeline,
            Priority = 1,
            ProcessingRules = new FinancialProcessingRules()
        },
        ["legal_documents"] = new PipelineDefinition
        {
            EntryCondition = "content_category == 'legal'",
            PipelineGraph = legalPipeline,
            Priority = 2,
            ProcessingRules = new LegalProcessingRules()
        }
    },
    DefaultPipeline = generalPipeline,
    EnableLoadBalancing = true,
    EnableFailover = true
};

var selectedPipeline = orchestrator.SelectPipeline(documentContext);
```

### Real-time Processing

```csharp
// Implement real-time document processing
var realTimePipeline = new RealTimeDocumentPipeline
{
    InputQueue = new DocumentQueue(),
    ProcessingWorkers = new List<DocumentWorker>(),
    ResultPublisher = new ResultPublisher(),
    EnableStreaming = true,
    ProcessingMode = ProcessingMode.RealTime
};

// Start real-time processing
await realTimePipeline.StartAsync();

// Subscribe to real-time results
realTimePipeline.ResultPublished += (sender, e) =>
{
    Console.WriteLine($"📊 Real-time result: {e.DocumentId} - {e.AnalysisSummary}");
};
```

## Related Examples

- [Conditional Nodes](./conditional-nodes.md): Dynamic routing and decision making
- [Streaming Execution](./streaming-execution.md): Real-time processing and monitoring
- [Multi-Agent](./multi-agent.md): Coordinated document processing
- [Checkpointing](./checkpointing.md): State persistence and recovery

## See Also

- [Document Processing Concepts](../concepts/document-processing.md): Understanding document analysis
- [Pipeline Patterns](../patterns/pipeline.md): Building processing pipelines
- [Performance Optimization](../how-to/performance-optimization.md): Optimizing processing performance
- [API Reference](../api/): Complete API documentation
