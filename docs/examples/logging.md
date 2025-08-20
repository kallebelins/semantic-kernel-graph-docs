# Logging Example

This example demonstrates comprehensive logging and structured logging capabilities in Semantic Kernel Graph workflows. It shows how to implement different logging levels, structured logging, log aggregation, and integration with various logging systems.

## Objective

Learn how to implement comprehensive logging in graph-based workflows to:
* Configure different logging levels and categories
* Implement structured logging with semantic information
* Aggregate and analyze logs across graph execution
* Integrate with external logging systems and dashboards
* Monitor and debug graph execution through logs

## Prerequisites

* **.NET 8.0** or later
* **OpenAI API Key** configured in `appsettings.json`
* **Semantic Kernel Graph package** installed
* Basic understanding of [Graph Concepts](../concepts/graph-concepts.md) and [Logging Concepts](../concepts/logging.md)

## Key Components

### Concepts and Techniques

* **Structured Logging**: Logging with structured data and semantic information
* **Log Levels**: Different levels of logging detail (Debug, Info, Warning, Error)
* **Log Aggregation**: Collecting and analyzing logs across executions
* **Log Correlation**: Correlating logs with execution context and node IDs
* **Log Export**: Exporting logs to external systems and dashboards

### Core Classes

* `SemanticKernelGraphLogger`: Core logging implementation
* `GraphExecutionLogger`: Execution-specific logging
* `NodeExecutionLogger`: Node-level logging
* `LogAggregator`: Log collection and analysis

## Running the Example

### Getting Started

This example demonstrates comprehensive logging and tracing with the Semantic Kernel Graph package. The code snippets below show you how to implement this pattern in your own applications.

## Step-by-Step Implementation

### 1. Basic Logging Configuration

This example demonstrates basic logging setup and configuration.

```csharp
// Create kernel with mock configuration
var kernel = CreateKernel();

// Create logging-enabled workflow
var loggingWorkflow = new GraphExecutor("LoggingWorkflow", "Basic logging configuration", logger);

// Configure logging options
var loggingOptions = new GraphLoggingOptions
{
    EnableStructuredLogging = true,
    EnableExecutionLogging = true,
    EnableNodeLogging = true,
    EnablePerformanceLogging = true,
    EnableErrorLogging = true,
    LogLevel = LogLevel.Information,
    EnableLogCorrelation = true,
    EnableLogAggregation = true,
    LogStoragePath = "./logs"
};

loggingWorkflow.ConfigureLogging(loggingOptions);

// Sample processing node with logging
var loggingProcessor = new FunctionGraphNode(
    "logging-processor",
    "Process data with comprehensive logging",
    async (context) =>
    {
        var inputData = context.GetValue<string>("input_data");
        var startTime = DateTime.UtcNow;
        
        // Log processing start
        context.Logger.LogInformation("Starting data processing", new
        {
            NodeId = "logging-processor",
            InputData = inputData,
            StartTime = startTime,
            ExecutionId = context.ExecutionId
        });
        
        // Simulate processing
        await Task.Delay(Random.Shared.Next(100, 300));
        
        var processedData = $"Processed: {inputData}";
        var processingTime = DateTime.UtcNow - startTime;
        
        // Log processing completion
        context.Logger.LogInformation("Data processing completed", new
        {
            NodeId = "logging-processor",
            InputData = inputData,
            ProcessedData = processedData,
            ProcessingTimeMs = processingTime.TotalMilliseconds,
            ExecutionId = context.ExecutionId
        });
        
        context.SetValue("processed_data", processedData);
        context.SetValue("processing_time_ms", processingTime.TotalMilliseconds);
        context.SetValue("processing_step", "logged_processed");
        
        return processedData;
    });

// Log aggregation node
var logAggregator = new FunctionGraphNode(
    "log-aggregator",
    "Aggregate and analyze logs",
    async (context) =>
    {
        var processedData = context.GetValue<string>("processed_data");
        var processingTime = context.GetValue<double>("processing_time_ms");
        
        // Aggregate log information
        var logSummary = new Dictionary<string, object>
        {
            ["total_logs"] = 2, // Start + completion logs
            ["processing_time_ms"] = processingTime,
            ["input_data"] = context.GetValue<string>("input_data"),
            ["processed_data"] = processedData,
            ["execution_id"] = context.ExecutionId,
            ["aggregation_timestamp"] = DateTime.UtcNow
        };
        
        context.SetValue("log_summary", logSummary);
        
        // Log aggregation completion
        context.Logger.LogInformation("Log aggregation completed", logSummary);
        
        return $"Log aggregation completed: {logSummary["total_logs"]} logs processed";
    });

// Add nodes to workflow
loggingWorkflow.AddNode(loggingProcessor);
loggingWorkflow.AddNode(logAggregator);

// Set start node
loggingWorkflow.SetStartNode(loggingProcessor.NodeId);

// Test basic logging
var testData = new[]
{
    "Sample data 1",
    "Sample data 2",
    "Sample data 3"
};

foreach (var data in testData)
{
    var arguments = new KernelArguments
    {
        ["input_data"] = data
    };

    Console.WriteLine($"📝 Testing basic logging: {data}");
    var result = await loggingWorkflow.ExecuteAsync(kernel, arguments);
    
    var processingTime = result.GetValue<double>("processing_time_ms");
    var logSummary = result.GetValue<Dictionary<string, object>>("log_summary");
    
    Console.WriteLine($"   Processing Time: {processingTime:F2} ms");
    Console.WriteLine($"   Logs Generated: {logSummary["total_logs"]}");
    Console.WriteLine();
}
```

### 2. Advanced Structured Logging

Demonstrates advanced structured logging with semantic information and context.

```csharp
// Create advanced logging workflow
var advancedLoggingWorkflow = new GraphExecutor("AdvancedLoggingWorkflow", "Advanced structured logging", logger);

// Configure advanced logging
var advancedLoggingOptions = new GraphLoggingOptions
{
    EnableStructuredLogging = true,
    EnableExecutionLogging = true,
    EnableNodeLogging = true,
    EnablePerformanceLogging = true,
    EnableErrorLogging = true,
    EnableLogCorrelation = true,
    EnableLogAggregation = true,
    EnableSemanticLogging = true,
    EnableContextLogging = true,
    LogLevel = LogLevel.Debug,
    LogStoragePath = "./advanced-logs",
    StructuredLogFormat = "json",
    EnableLogCompression = true,
    MaxLogHistory = 10000
};

advancedLoggingWorkflow.ConfigureLogging(advancedLoggingOptions);

// Advanced processing node with semantic logging
var advancedProcessor = new FunctionGraphNode(
    "advanced-processor",
    "Advanced processing with semantic logging",
    async (context) =>
    {
        var inputData = context.GetValue<string>("input_data");
        var processingType = context.GetValue<string>("processing_type", "standard");
        var startTime = DateTime.UtcNow;
        
        // Semantic logging with context
        context.Logger.LogInformation("Advanced processing initiated", new
        {
            NodeId = "advanced-processor",
            ProcessingType = processingType,
            InputData = inputData,
            StartTime = startTime,
            ExecutionId = context.ExecutionId,
            Context = new
            {
                UserId = context.GetValue<string>("user_id", "anonymous"),
                SessionId = context.GetValue<string>("session_id", "default"),
                RequestId = context.GetValue<string>("request_id", Guid.NewGuid().ToString())
            },
            Metadata = new
            {
                Version = "1.0.0",
                Environment = "development",
                Component = "advanced-processor"
            }
        });
        
        // Simulate complex processing
        var iterations = processingType == "complex" ? 1000 : 100;
        var result = 0;
        
        for (int i = 0; i < iterations; i++)
        {
            result += i * i;
            if (i % 100 == 0)
            {
                // Progress logging
                context.Logger.LogDebug("Processing progress", new
                {
                    NodeId = "advanced-processor",
                    Iteration = i,
                    Progress = (double)i / iterations,
                    CurrentResult = result
                });
                
                await Task.Delay(1);
            }
        }
        
        var processingTime = DateTime.UtcNow - startTime;
        var processedData = $"Advanced processed: {inputData} (result: {result})";
        
        // Completion logging with performance metrics
        context.Logger.LogInformation("Advanced processing completed", new
        {
            NodeId = "advanced-processor",
            ProcessingType = processingType,
            InputData = inputData,
            ProcessedData = processedData,
            ProcessingTimeMs = processingTime.TotalMilliseconds,
            Iterations = iterations,
            FinalResult = result,
            PerformanceMetrics = new
            {
                Throughput = iterations / (processingTime.TotalMilliseconds / 1000.0),
                Efficiency = processingTime.TotalMilliseconds / iterations,
                CpuUsage = GetCurrentCpuUsage(),
                MemoryUsage = GetCurrentMemoryUsage()
            },
            ExecutionId = context.ExecutionId,
            Context = new
            {
                UserId = context.GetValue<string>("user_id", "anonymous"),
                SessionId = context.GetValue<string>("session_id", "default"),
                RequestId = context.GetValue<string>("request_id", Guid.NewGuid().ToString())
            }
        });
        
        context.SetValue("processed_data", processedData);
        context.SetValue("processing_time_ms", processingTime.TotalMilliseconds);
        context.SetValue("iterations", iterations);
        context.SetValue("final_result", result);
        context.SetValue("processing_step", "advanced_logged_processed");
        
        return processedData;
    });

// Semantic log analyzer
var semanticLogAnalyzer = new FunctionGraphNode(
    "semantic-log-analyzer",
    "Analyze semantic logs and extract insights",
    async (context) =>
    {
        var processedData = context.GetValue<string>("processed_data");
        var processingTime = context.GetValue<double>("processing_time_ms");
        var iterations = context.GetValue<int>("iterations");
        var finalResult = context.GetValue<int>("final_result");
        
        // Analyze log patterns and extract insights
        var logAnalysis = new Dictionary<string, object>
        {
            ["processing_summary"] = new
            {
                TotalTime = processingTime,
                Iterations = iterations,
                FinalResult = finalResult,
                Throughput = iterations / (processingTime / 1000.0)
            },
            ["performance_insights"] = new
            {
                IsEfficient = processingTime < 1000, // Less than 1 second
                Complexity = iterations > 500 ? "high" : "medium",
                OptimizationOpportunity = processingTime > 500 ? "yes" : "no"
            },
            ["semantic_patterns"] = new
            {
                ProcessingType = context.GetValue<string>("processing_type"),
                UserContext = context.GetValue<string>("user_id"),
                SessionContext = context.GetValue<string>("session_id")
            },
            ["analysis_timestamp"] = DateTime.UtcNow,
            ["execution_id"] = context.ExecutionId
        };
        
        context.SetValue("log_analysis", logAnalysis);
        
        // Log analysis completion
        context.Logger.LogInformation("Semantic log analysis completed", logAnalysis);
        
        return $"Semantic log analysis completed with {logAnalysis.Count} insights";
    });

// Add nodes to advanced workflow
advancedLoggingWorkflow.AddNode(advancedProcessor);
advancedLoggingWorkflow.AddNode(semanticLogAnalyzer);

// Set start node
advancedLoggingWorkflow.SetStartNode(advancedProcessor.NodeId);

// Test advanced logging
var advancedTestScenarios = new[]
{
    new { Data = "Simple processing", Type = "simple" },
    new { Data = "Complex processing", Type = "complex" },
    new { Data = "Standard processing", Type = "standard" }
};

foreach (var scenario in advancedTestScenarios)
{
    var arguments = new KernelArguments
    {
        ["input_data"] = scenario.Data,
        ["processing_type"] = scenario.Type,
        ["user_id"] = "user123",
        ["session_id"] = "session456",
        ["request_id"] = Guid.NewGuid().ToString()
    };

    Console.WriteLine($"🔍 Testing advanced logging: {scenario.Data}");
    Console.WriteLine($"   Processing Type: {scenario.Type}");
    
    var result = await advancedLoggingWorkflow.ExecuteAsync(kernel, arguments);
    
    var processingTime = result.GetValue<double>("processing_time_ms");
    var iterations = result.GetValue<int>("iterations");
    var logAnalysis = result.GetValue<Dictionary<string, object>>("log_analysis");
    
    Console.WriteLine($"   Processing Time: {processingTime:F2} ms");
    Console.WriteLine($"   Iterations: {iterations:N0}");
    Console.WriteLine($"   Insights Generated: {logAnalysis.Count}");
    Console.WriteLine();
}
```

### 3. Error Logging and Monitoring

Shows how to implement comprehensive error logging and monitoring.

```csharp
// Create error logging workflow
var errorLoggingWorkflow = new GraphExecutor("ErrorLoggingWorkflow", "Error logging and monitoring", logger);

// Configure error logging
var errorLoggingOptions = new GraphLoggingOptions
{
    EnableStructuredLogging = true,
    EnableExecutionLogging = true,
    EnableNodeLogging = true,
    EnableErrorLogging = true,
    EnableErrorAggregation = true,
    EnableErrorCorrelation = true,
    EnableErrorReporting = true,
    LogLevel = LogLevel.Warning,
    LogStoragePath = "./error-logs",
    ErrorLogRetention = TimeSpan.FromDays(30),
    EnableErrorMetrics = true
};

errorLoggingWorkflow.ConfigureLogging(errorLoggingOptions);

// Error-prone processing node
var errorProneProcessor = new FunctionGraphNode(
    "error-prone-processor",
    "Process data with potential errors",
    async (context) =>
    {
        var inputData = context.GetValue<string>("input_data");
        var errorProbability = context.GetValue<double>("error_probability", 0.3);
        var startTime = DateTime.UtcNow;
        
        try
        {
            // Log processing start
            context.Logger.LogInformation("Error-prone processing started", new
            {
                NodeId = "error-prone-processor",
                InputData = inputData,
                ErrorProbability = errorProbability,
                StartTime = startTime,
                ExecutionId = context.ExecutionId
            });
            
            // Simulate processing with potential errors
            var random = Random.Shared.NextDouble();
            if (random < errorProbability)
            {
                // Simulate an error
                var errorMessage = $"Processing failed for input: {inputData}";
                var exception = new InvalidOperationException(errorMessage);
                
                // Log error with context
                context.Logger.LogError(exception, "Processing error occurred", new
                {
                    NodeId = "error-prone-processor",
                    InputData = inputData,
                    ErrorType = exception.GetType().Name,
                    ErrorMessage = errorMessage,
                    ErrorProbability = errorProbability,
                    RandomValue = random,
                    ProcessingTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds,
                    ExecutionId = context.ExecutionId,
                    Context = new
                    {
                        UserId = context.GetValue<string>("user_id", "anonymous"),
                        SessionId = context.GetValue<string>("session_id", "default"),
                        RequestId = context.GetValue<string>("request_id", Guid.NewGuid().ToString())
                    }
                });
                
                // Set error state
                context.SetValue("error_occurred", true);
                context.SetValue("error_message", errorMessage);
                context.SetValue("error_type", exception.GetType().Name);
                context.SetValue("processing_step", "error_logged");
                
                throw exception;
            }
            
            // Successful processing
            await Task.Delay(Random.Shared.Next(100, 300));
            var processedData = $"Successfully processed: {inputData}";
            var processingTime = DateTime.UtcNow - startTime;
            
            // Log success
            context.Logger.LogInformation("Processing completed successfully", new
            {
                NodeId = "error-prone-processor",
                InputData = inputData,
                ProcessedData = processedData,
                ProcessingTimeMs = processingTime.TotalMilliseconds,
                ErrorProbability = errorProbability,
                RandomValue = random,
                ExecutionId = context.ExecutionId
            });
            
            context.SetValue("processed_data", processedData);
            context.SetValue("processing_time_ms", processingTime.TotalMilliseconds);
            context.SetValue("error_occurred", false);
            context.SetValue("processing_step", "success_logged");
            
            return processedData;
        }
        catch (Exception ex)
        {
            // Additional error logging for unhandled exceptions
            context.Logger.LogCritical(ex, "Unhandled exception in error-prone processor", new
            {
                NodeId = "error-prone-processor",
                InputData = inputData,
                ExceptionType = ex.GetType().Name,
                ExceptionMessage = ex.Message,
                StackTrace = ex.StackTrace,
                ExecutionId = context.ExecutionId
            });
            
            throw;
        }
    });

// Error monitor and aggregator
var errorMonitor = new FunctionGraphNode(
    "error-monitor",
    "Monitor and aggregate error logs",
    async (context) =>
    {
        var errorOccurred = context.GetValue<bool>("error_occurred", false);
        var errorMessage = context.GetValue<string>("error_message", "");
        var errorType = context.GetValue<string>("error_type", "");
        var processingTime = context.GetValue<double>("processing_time_ms", 0.0);
        
        // Aggregate error information
        var errorSummary = new Dictionary<string, object>
        {
            ["error_summary"] = new
            {
                ErrorOccurred = errorOccurred,
                ErrorMessage = errorMessage,
                ErrorType = errorType,
                ProcessingTimeMs = processingTime,
                Timestamp = DateTime.UtcNow
            },
            ["execution_metrics"] = new
            {
                TotalExecutions = 1,
                SuccessfulExecutions = errorOccurred ? 0 : 1,
                FailedExecutions = errorOccurred ? 1 : 0,
                SuccessRate = errorOccurred ? 0.0 : 1.0,
                AverageProcessingTime = processingTime
            },
            ["monitoring_data"] = new
            {
                ExecutionId = context.ExecutionId,
                NodeId = "error-prone-processor",
                MonitoringTimestamp = DateTime.UtcNow,
                AlertLevel = errorOccurred ? "warning" : "info"
            }
        };
        
        context.SetValue("error_summary", errorSummary);
        
        // Log monitoring results
        if (errorOccurred)
        {
            context.Logger.LogWarning("Error monitoring alert", errorSummary);
        }
        else
        {
            context.Logger.LogInformation("Error monitoring completed", errorSummary);
        }
        
        return $"Error monitoring completed. Errors: {(errorOccurred ? 1 : 0)}";
    });

// Add nodes to error workflow
errorLoggingWorkflow.AddNode(errorProneProcessor);
errorLoggingWorkflow.AddNode(errorMonitor);

// Set start node
errorLoggingWorkflow.SetStartNode(errorProneProcessor.NodeId);

// Test error logging
var errorTestScenarios = new[]
{
    new { Data = "Low error probability", Probability = 0.1 },
    new { Data = "Medium error probability", Probability = 0.5 },
    new { Data = "High error probability", Probability = 0.8 }
};

foreach (var scenario in errorTestScenarios)
{
    var arguments = new KernelArguments
    {
        ["input_data"] = scenario.Data,
        ["error_probability"] = scenario.Probability,
        ["user_id"] = "user123",
        ["session_id"] = "session456",
        ["request_id"] = Guid.NewGuid().ToString()
    };

    Console.WriteLine($"⚠️ Testing error logging: {scenario.Data}");
    Console.WriteLine($"   Error Probability: {scenario.Probability:P0}");
    
    try
    {
        var result = await errorLoggingWorkflow.ExecuteAsync(kernel, arguments);
        
        var errorOccurred = result.GetValue<bool>("error_occurred");
        var processingStep = result.GetValue<string>("processing_step");
        var errorSummary = result.GetValue<Dictionary<string, object>>("error_summary");
        
        Console.WriteLine($"   Error Occurred: {errorOccurred}");
        Console.WriteLine($"   Processing Step: {processingStep}");
        Console.WriteLine($"   Monitoring Data: {errorSummary.Count} metrics");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   Exception Caught: {ex.GetType().Name}: {ex.Message}");
    }
    
    Console.WriteLine();
}
```

### 4. Log Export and Integration

Demonstrates exporting logs to external systems and integrating with monitoring platforms.

```csharp
// Create log export workflow
var logExportWorkflow = new GraphExecutor("LogExportWorkflow", "Log export and integration", logger);

// Configure log export
var logExportOptions = new GraphLoggingOptions
{
    EnableStructuredLogging = true,
    EnableExecutionLogging = true,
    EnableNodeLogging = true,
    EnableLogExport = true,
    EnableLogPersistence = true,
    EnableLogCompression = true,
    LogStoragePath = "./export-logs",
    ExportFormats = new[] { "json", "csv", "logstash", "fluentd" },
    ExportInterval = TimeSpan.FromSeconds(5),
    EnableLogRotation = true,
    MaxLogFileSize = 10 * 1024 * 1024, // 10MB
    LogRetentionDays = 7
};

logExportWorkflow.ConfigureLogging(logExportOptions);

// Log generator
var logGenerator = new FunctionGraphNode(
    "log-generator",
    "Generate sample logs for export",
    async (context) =>
    {
        var iteration = context.GetValue<int>("iteration", 0);
        var logCount = context.GetValue<int>("log_count", 10);
        
        // Generate various log types
        var logs = new List<Dictionary<string, object>>();
        
        for (int i = 0; i < logCount; i++)
        {
            var logEntry = new Dictionary<string, object>
            {
                ["timestamp"] = DateTime.UtcNow.AddSeconds(-i),
                ["level"] = GetRandomLogLevel(),
                ["message"] = $"Sample log message {iteration}-{i}",
                ["node_id"] = "log-generator",
                ["execution_id"] = context.ExecutionId,
                ["iteration"] = iteration,
                ["log_index"] = i,
                ["metadata"] = new
                {
                    Source = "log-generator",
                    Version = "1.0.0",
                    Environment = "development"
                }
            };
            
            logs.Add(logEntry);
        }
        
        context.SetValue("generated_logs", logs);
        context.SetValue("log_count", logs.Count);
        context.SetValue("generation_timestamp", DateTime.UtcNow);
        
        // Log generation completion
        context.Logger.LogInformation("Log generation completed", new
        {
            NodeId = "log-generator",
            LogCount = logs.Count,
            Iteration = iteration,
            ExecutionId = context.ExecutionId
        });
        
        return $"Generated {logs.Count} log entries for iteration {iteration}";
    });

// Log exporter
var logExporter = new FunctionGraphNode(
    "log-exporter",
    "Export logs to external systems",
    async (context) =>
    {
        var generatedLogs = context.GetValue<List<Dictionary<string, object>>>("generated_logs");
        var iteration = context.GetValue<int>("iteration");
        var generationTimestamp = context.GetValue<DateTime>("generation_timestamp");
        
        // Export to different formats
        var exportResults = new Dictionary<string, string>();
        
        // JSON export
        var jsonExport = await ExportLogsToJson(generatedLogs);
        exportResults["json"] = jsonExport;
        
        // CSV export
        var csvExport = await ExportLogsToCsv(generatedLogs);
        exportResults["csv"] = csvExport;
        
        // Logstash export
        var logstashExport = await ExportLogsToLogstash(generatedLogs);
        exportResults["logstash"] = logstashExport;
        
        // Fluentd export
        var fluentdExport = await ExportLogsToFluentd(generatedLogs);
        exportResults["fluentd"] = fluentdExport;
        
        // Export to monitoring systems
        var monitoringExport = await ExportLogsToMonitoring(generatedLogs);
        exportResults["monitoring"] = monitoringExport;
        
        // Create export summary
        var exportSummary = new Dictionary<string, object>
        {
            ["export_summary"] = new
            {
                TotalLogs = generatedLogs.Count,
                ExportFormats = exportResults.Count,
                ExportFormatsList = exportResults.Keys.ToArray(),
                Iteration = iteration,
                GenerationTimestamp = generationTimestamp,
                ExportTimestamp = DateTime.UtcNow
            },
            ["export_results"] = exportResults,
            ["export_metadata"] = new
            {
                ExportPath = "./export-logs",
                CompressionEnabled = true,
                RotationEnabled = true,
                RetentionDays = 7
            }
        };
        
        context.SetValue("export_summary", exportSummary);
        
        // Log export completion
        context.Logger.LogInformation("Log export completed", exportSummary);
        
        return $"Logs exported to {exportResults.Count} formats";
    });

// Add nodes to export workflow
logExportWorkflow.AddNode(logGenerator);
logExportWorkflow.AddNode(logExporter);

// Set start node
logExportWorkflow.SetStartNode(logGenerator.NodeId);

// Test log export
Console.WriteLine("📤 Testing log export and integration...");

var exportArguments = new KernelArguments
{
    ["iteration"] = 1,
    ["log_count"] = 25
};

var result = await logExportWorkflow.ExecuteAsync(kernel, exportArguments);

var exportSummary = result.GetValue<Dictionary<string, object>>("export_summary");
var generatedLogs = result.GetValue<List<Dictionary<string, object>>>("generated_logs");

if (exportSummary != null)
{
    var summary = exportSummary["export_summary"] as dynamic;
    Console.WriteLine($"   Total Logs: {summary.TotalLogs}");
    Console.WriteLine($"   Export Formats: {string.Join(", ", summary.ExportFormatsList)}");
    Console.WriteLine($"   Export Path: {exportSummary["export_metadata"].ExportPath}");
}

Console.WriteLine("✅ Log export testing completed");

// Helper methods for log export
async Task<string> ExportLogsToJson(List<Dictionary<string, object>> logs)
{
    var json = System.Text.Json.JsonSerializer.Serialize(logs, new System.Text.Json.JsonSerializerOptions
    {
        WriteIndented = true
    });
    
    var filename = $"./export-logs/logs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
    await File.WriteAllTextAsync(filename, json);
    
    return filename;
}

async Task<string> ExportLogsToCsv(List<Dictionary<string, object>> logs)
{
    var csv = new StringBuilder();
    
    if (logs.Any())
    {
        // Header
        var headers = logs.First().Keys;
        csv.AppendLine(string.Join(",", headers));
        
        // Data
        foreach (var log in logs)
        {
            var values = headers.Select(h => log[h]?.ToString() ?? "").Select(v => $"\"{v}\"");
            csv.AppendLine(string.Join(",", values));
        }
    }
    
    var filename = $"./export-logs/logs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
    await File.WriteAllTextAsync(filename, csv.ToString());
    
    return filename;
}

async Task<string> ExportLogsToLogstash(List<Dictionary<string, object>> logs)
{
    var logstashFormat = new StringBuilder();
    
    foreach (var log in logs)
    {
        var logstashEntry = new
        {
            @timestamp = log["timestamp"],
            level = log["level"],
            message = log["message"],
            node_id = log["node_id"],
            execution_id = log["execution_id"],
            metadata = log["metadata"]
        };
        
        logstashFormat.AppendLine(System.Text.Json.JsonSerializer.Serialize(logstashEntry));
    }
    
    var filename = $"./export-logs/logs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.logstash";
    await File.WriteAllTextAsync(filename, logstashFormat.ToString());
    
    return filename;
}

async Task<string> ExportLogsToFluentd(List<Dictionary<string, object>> logs)
{
    var fluentdFormat = new StringBuilder();
    
    foreach (var log in logs)
    {
        var fluentdEntry = new
        {
            time = log["timestamp"],
            level = log["level"],
            message = log["message"],
            node_id = log["node_id"],
            execution_id = log["execution_id"],
            metadata = log["metadata"]
        };
        
        fluentdFormat.AppendLine(System.Text.Json.JsonSerializer.Serialize(fluentdEntry));
    }
    
    var filename = $"./export-logs/logs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.fluentd";
    await File.WriteAllTextAsync(filename, fluentdFormat.ToString());
    
    return filename;
}

async Task<string> ExportLogsToMonitoring(List<Dictionary<string, object>> logs)
{
    // Simulate export to monitoring system
    var monitoringData = new
    {
        source = "semantic-kernel-graph",
        timestamp = DateTime.UtcNow,
        log_count = logs.Count,
        log_levels = logs.GroupBy(l => l["level"]).ToDictionary(g => g.Key, g => g.Count()),
        execution_ids = logs.Select(l => l["execution_id"]).Distinct().ToArray()
    };
    
    var filename = $"./export-logs/monitoring_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
    await File.WriteAllTextAsync(filename, System.Text.Json.JsonSerializer.Serialize(monitoringData, new System.Text.Json.JsonSerializerOptions
    {
        WriteIndented = true
    }));
    
    return filename;
}

string GetRandomLogLevel()
{
    var levels = new[] { "Debug", "Information", "Warning", "Error" };
    var weights = new[] { 0.4, 0.4, 0.15, 0.05 }; // 40% Debug, 40% Info, 15% Warning, 5% Error
    
    var random = Random.Shared.NextDouble();
    var cumulativeWeight = 0.0;
    
    for (int i = 0; i < levels.Length; i++)
    {
        cumulativeWeight += weights[i];
        if (random <= cumulativeWeight)
        {
            return levels[i];
        }
    }
    
    return levels[0];
}
```

## Expected Output

### Basic Logging Configuration Example

```
📝 Testing basic logging: Sample data 1
   Processing Time: 234.56 ms
   Logs Generated: 2

📝 Testing basic logging: Sample data 2
   Processing Time: 187.23 ms
   Logs Generated: 2
```

### Advanced Structured Logging Example

```
🔍 Testing advanced logging: Simple processing
   Processing Type: simple
   Processing Time: 156.78 ms
   Iterations: 100
   Insights Generated: 4

🔍 Testing advanced logging: Complex processing
   Processing Type: complex
   Processing Time: 1,234.56 ms
   Iterations: 1,000
   Insights Generated: 4
```

### Error Logging and Monitoring Example

```
⚠️ Testing error logging: Low error probability
   Error Probability: 10%
   Error Occurred: False
   Processing Step: success_logged
   Monitoring Data: 3 metrics

⚠️ Testing error logging: High error probability
   Error Probability: 80%
   Error Occurred: True
   Processing Step: error_logged
   Monitoring Data: 3 metrics
```

### Log Export and Integration Example

```
📤 Testing log export and integration...
   Total Logs: 25
   Export Formats: json, csv, logstash, fluentd, monitoring
   Export Path: ./export-logs
✅ Log export testing completed
```

## Configuration Options

### Logging Configuration

```csharp
var loggingOptions = new GraphLoggingOptions
{
    EnableStructuredLogging = true,                    // Enable structured logging
    EnableExecutionLogging = true,                     // Enable execution-level logging
    EnableNodeLogging = true,                          // Enable node-level logging
    EnablePerformanceLogging = true,                   // Enable performance logging
    EnableErrorLogging = true,                         // Enable error logging
    EnableErrorAggregation = true,                     // Enable error aggregation
    EnableErrorCorrelation = true,                     // Enable error correlation
    EnableErrorReporting = true,                       // Enable error reporting
    EnableSemanticLogging = true,                      // Enable semantic logging
    EnableContextLogging = true,                       // Enable context logging
    EnableLogCorrelation = true,                       // Enable log correlation
    EnableLogAggregation = true,                       // Enable log aggregation
    EnableLogExport = true,                            // Enable log export
    EnableLogPersistence = true,                       // Enable log persistence
    EnableLogCompression = true,                       // Enable log compression
    EnableLogRotation = true,                          // Enable log rotation
    EnableErrorMetrics = true,                         // Enable error metrics
    LogLevel = LogLevel.Information,                   // Default log level
    StructuredLogFormat = "json",                      // Structured log format
    LogStoragePath = "./logs",                         // Log storage path
    ExportFormats = new[] { "json", "csv", "logstash", "fluentd" }, // Export formats
    ExportInterval = TimeSpan.FromSeconds(5),          // Export interval
    MaxLogFileSize = 10 * 1024 * 1024,                // Max log file size (10MB)
    LogRetentionDays = 7,                              // Log retention period
    ErrorLogRetention = TimeSpan.FromDays(30),         // Error log retention
    MaxLogHistory = 10000,                             // Maximum log history
    EnableLogCompression = true,                       // Enable log compression
    CompressionLevel = System.IO.Compression.CompressionLevel.Optimal // Compression level
};
```

### Error Logging Configuration

```csharp
var errorLoggingOptions = new ErrorLoggingOptions
{
    EnableErrorAggregation = true,                     // Enable error aggregation
    EnableErrorCorrelation = true,                     // Enable error correlation
    EnableErrorReporting = true,                       // Enable error reporting
    EnableErrorMetrics = true,                         // Enable error metrics
    EnableErrorAlerts = true,                          // Enable error alerts
    EnableErrorTrends = true,                          // Enable error trend analysis
    ErrorLogRetention = TimeSpan.FromDays(30),         // Error log retention
    MaxErrorHistory = 1000,                            // Maximum error history
    ErrorAlertThreshold = 5,                           // Error alert threshold
    ErrorAlertWindow = TimeSpan.FromMinutes(5),        // Error alert window
    EnableErrorSampling = true,                        // Enable error sampling
    ErrorSamplingRate = 0.1,                           // Error sampling rate (10%)
    EnableErrorDeduplication = true,                   // Enable error deduplication
    ErrorDeduplicationWindow = TimeSpan.FromMinutes(10) // Error deduplication window
};
```

## Troubleshooting

### Common Issues

#### Logs Not Being Generated
```bash
# Problem: Logs are not being generated
# Solution: Check logging configuration and enable required features
EnableStructuredLogging = true;
EnableExecutionLogging = true;
LogLevel = LogLevel.Information;
```

#### Performance Impact
```bash
# Problem: Logging impacts performance
# Solution: Adjust log level and enable compression
LogLevel = LogLevel.Warning;
EnableLogCompression = true;
EnableLogSampling = true;
```

#### Storage Issues
```bash
# Problem: Logs consume too much storage
# Solution: Enable rotation and set retention policies
EnableLogRotation = true;
MaxLogFileSize = 5 * 1024 * 1024; // 5MB
LogRetentionDays = 3;
```

### Debug Mode

Enable detailed logging for troubleshooting:

```csharp
// Enable debug logging
var logger = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
}).CreateLogger<LoggingExample>();

// Configure logging with debug logging
var debugLoggingOptions = new GraphLoggingOptions
{
    EnableStructuredLogging = true,
    EnableExecutionLogging = true,
    EnableNodeLogging = true,
    EnableDebugLogging = true,
    LogLevel = LogLevel.Debug,
    LogVisualizationUpdates = true,
    LogExportOperations = true
};
```

## Advanced Patterns

### Custom Log Formatters

```csharp
// Implement custom log formatter
public class CustomLogFormatter : ILogFormatter
{
    public async Task<string> FormatLogAsync(LogEntry entry, Dictionary<string, object> context)
    {
        var customFormat = new StringBuilder();
        
        // Custom header
        customFormat.AppendLine("=== CUSTOM LOG ENTRY ===");
        customFormat.AppendLine($"Timestamp: {entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}");
        customFormat.AppendLine($"Level: {entry.Level}");
        customFormat.AppendLine($"Message: {entry.Message}");
        customFormat.AppendLine($"Node: {entry.NodeId}");
        customFormat.AppendLine($"Execution: {entry.ExecutionId}");
        
        // Custom context formatting
        if (context.Any())
        {
            customFormat.AppendLine("Context:");
            foreach (var kvp in context)
            {
                customFormat.AppendLine($"  {kvp.Key}: {kvp.Value}");
            }
        }
        
        customFormat.AppendLine("========================");
        
        return customFormat.ToString();
    }
}
```

### Custom Log Aggregators

```csharp
// Implement custom log aggregator
public class CustomLogAggregator : ILogAggregator
{
    public async Task<LogAggregationResult> AggregateLogsAsync(IEnumerable<LogEntry> logs)
    {
        var aggregation = new LogAggregationResult();
        
        foreach (var log in logs)
        {
            // Aggregate by level
            if (!aggregation.LevelCounts.ContainsKey(log.Level))
                aggregation.LevelCounts[log.Level] = 0;
            aggregation.LevelCounts[log.Level]++;
            
            // Aggregate by node
            if (!aggregation.NodeCounts.ContainsKey(log.NodeId))
                aggregation.NodeCounts[log.NodeId] = 0;
            aggregation.NodeCounts[log.NodeId]++;
            
            // Track execution patterns
            if (!aggregation.ExecutionPatterns.ContainsKey(log.ExecutionId))
                aggregation.ExecutionPatterns[log.ExecutionId] = new List<LogEntry>();
            aggregation.ExecutionPatterns[log.ExecutionId].Add(log);
        }
        
        // Calculate derived metrics
        aggregation.TotalLogs = logs.Count();
        aggregation.AverageLogsPerExecution = aggregation.ExecutionPatterns.Count > 0 
            ? (double)aggregation.TotalLogs / aggregation.ExecutionPatterns.Count 
            : 0;
        
        return aggregation;
    }
}
```

### Real-Time Log Monitoring

```csharp
// Implement real-time log monitoring
public class RealTimeLogMonitor : ILogMonitor
{
    private readonly List<LogEntry> _recentLogs = new();
    private readonly object _lock = new();
    
    public async Task MonitorLogAsync(LogEntry entry)
    {
        lock (_lock)
        {
            _recentLogs.Add(entry);
            
            // Keep only recent logs (last 1000)
            if (_recentLogs.Count > 1000)
            {
                _recentLogs.RemoveRange(0, _recentLogs.Count - 1000);
            }
        }
        
        // Check for alerts
        await CheckAlertsAsync(entry);
    }
    
    private async Task CheckAlertsAsync(LogEntry entry)
    {
        // Check for error rate alerts
        var recentErrors = _recentLogs
            .Where(l => l.Level == LogLevel.Error)
            .Where(l => l.Timestamp > DateTime.UtcNow.AddMinutes(-5))
            .Count();
        
        if (recentErrors > 10)
        {
            await SendAlertAsync($"High error rate detected: {recentErrors} errors in last 5 minutes");
        }
        
        // Check for specific error patterns
        if (entry.Level == LogLevel.Error && entry.Message.Contains("critical"))
        {
            await SendAlertAsync($"Critical error detected: {entry.Message}");
        }
    }
    
    private async Task SendAlertAsync(string message)
    {
        // Implementation for sending alerts
        Console.WriteLine($"🚨 ALERT: {message}");
        await Task.CompletedTask;
    }
}
```

## Related Examples

* [Graph Metrics](./graph-metrics.md): Metrics collection and monitoring
* [Graph Visualization](./graph-visualization.md): Visual log representation
* [Debug and Inspection](./debug-inspection.md): Using logs for debugging
* [Performance Optimization](./performance-optimization.md): Log-based performance analysis

## See Also

* [Logging and Observability](../concepts/logging.md): Understanding logging concepts
* [Debug and Inspection](../how-to/debug-and-inspection.md): Debugging with logs
* [Performance Monitoring](../how-to/performance-monitoring.md): Performance logging
* [API Reference](../api/): Complete API documentation
