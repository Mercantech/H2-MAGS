using System.Text.Json.Serialization;

namespace DomainModels;

public class BrunoTestResult
{
    [JsonPropertyName("iterationIndex")]
    public int IterationIndex { get; set; }

    [JsonPropertyName("summary")]
    public TestSummary Summary { get; set; } = new();

    [JsonPropertyName("results")]
    public List<TestResult> Results { get; set; } = new();
}

public class TestSummary
{
    [JsonPropertyName("totalRequests")]
    public int TotalRequests { get; set; }

    [JsonPropertyName("passedRequests")]
    public int PassedRequests { get; set; }

    [JsonPropertyName("failedRequests")]
    public int FailedRequests { get; set; }

    [JsonPropertyName("errorRequests")]
    public int ErrorRequests { get; set; }

    [JsonPropertyName("skippedRequests")]
    public int SkippedRequests { get; set; }

    [JsonPropertyName("totalAssertions")]
    public int TotalAssertions { get; set; }

    [JsonPropertyName("passedAssertions")]
    public int PassedAssertions { get; set; }

    [JsonPropertyName("failedAssertions")]
    public int FailedAssertions { get; set; }

    [JsonPropertyName("totalTests")]
    public int TotalTests { get; set; }

    [JsonPropertyName("passedTests")]
    public int PassedTests { get; set; }

    [JsonPropertyName("failedTests")]
    public int FailedTests { get; set; }
}

public class TestResult
{
    [JsonPropertyName("test")]
    public TestInfo Test { get; set; } = new();

    [JsonPropertyName("request")]
    public RequestInfo Request { get; set; } = new();

    [JsonPropertyName("response")]
    public ResponseInfo Response { get; set; } = new();

    [JsonPropertyName("tests")]
    public List<TestAssertion> Tests { get; set; } = new();

    [JsonPropertyName("duration")]
    public int Duration { get; set; }
}

public class TestInfo
{
    [JsonPropertyName("filename")]
    public string Filename { get; set; } = string.Empty;
}

public class RequestInfo
{
    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("headers")]
    public Dictionary<string, string> Headers { get; set; } = new();
}

public class ResponseInfo
{
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("statusText")]
    public string StatusText { get; set; } = string.Empty;

    [JsonPropertyName("headers")]
    public Dictionary<string, string> Headers { get; set; } = new();

    [JsonPropertyName("data")]
    public object? Data { get; set; }
}

public class TestAssertion
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("passed")]
    public bool Passed { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

public class TestFileInfo
{
    public string Filename { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public long Size { get; set; }
    public string Type { get; set; } = string.Empty; // "json" or "html"
}

public class TestResultsOverview
{
    public List<TestFileInfo> AvailableResults { get; set; } = new();
    public TestFileInfo? LatestJsonResult { get; set; }
    public TestFileInfo? LatestHtmlResult { get; set; }
    public BrunoTestResult? LatestTestData { get; set; }
}
