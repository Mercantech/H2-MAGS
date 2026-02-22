using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using DomainModels;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BrunoTestResultsController : ControllerBase
{
    private readonly ILogger<BrunoTestResultsController> _logger;
    private readonly string _testResultsPath;

    public BrunoTestResultsController(ILogger<BrunoTestResultsController> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _testResultsPath = Path.Combine(environment.ContentRootPath, "test-results");
        
        // Debug logging
        _logger.LogInformation("BrunoTestResultsController initialized");
        _logger.LogInformation("ContentRootPath: {ContentRootPath}", environment.ContentRootPath);
        _logger.LogInformation("TestResultsPath: {TestResultsPath}", _testResultsPath);
        _logger.LogInformation("TestResultsPath exists: {Exists}", Directory.Exists(_testResultsPath));
        
        if (Directory.Exists(_testResultsPath))
        {
            var files = Directory.GetFiles(_testResultsPath);
            _logger.LogInformation("Found {FileCount} files in test-results directory", files.Length);
            foreach (var file in files.Take(5)) // Log first 5 files
            {
                _logger.LogInformation("File: {FileName}, Size: {Size} bytes, Modified: {Modified}", 
                    Path.GetFileName(file), new FileInfo(file).Length, System.IO.File.GetLastWriteTime(file));
            }
        }
    }

    /// <summary>
    /// Henter oversigt over alle tilgængelige test resultater
    /// </summary>
    [HttpGet("overview")]
    public ActionResult<TestResultsOverview> GetOverview()
    {
        try
        {
            _logger.LogInformation("GetOverview called - checking path: {TestResultsPath}", _testResultsPath);
            
            if (!Directory.Exists(_testResultsPath))
            {
                _logger.LogWarning("Test results directory does not exist: {TestResultsPath}", _testResultsPath);
                return Ok(new TestResultsOverview
                {
                    AvailableResults = new List<TestFileInfo>()
                });
            }
            
            _logger.LogInformation("Test results directory exists, scanning for files...");

            var allFiles = Directory.GetFiles(_testResultsPath);
            _logger.LogInformation("Found {TotalFiles} total files in directory", allFiles.Length);
            
            var files = allFiles
                .Where(f => f.EndsWith(".json") || f.EndsWith(".html"))
                .Select(f => new TestFileInfo
                {
                    Filename = Path.GetFileName(f),
                    LastModified = System.IO.File.GetLastWriteTime(f),
                    Size = new FileInfo(f).Length,
                    Type = Path.GetExtension(f).ToLower() == ".json" ? "json" : "html"
                })
                .OrderByDescending(f => f.LastModified)
                .ToList();
                
            _logger.LogInformation("Found {FilteredFiles} JSON/HTML files after filtering", files.Count);

            var overview = new TestResultsOverview
            {
                AvailableResults = files,
                LatestJsonResult = files.FirstOrDefault(f => f.Type == "json"),
                LatestHtmlResult = files.FirstOrDefault(f => f.Type == "html")
            };

            // Hent data fra seneste JSON fil hvis den eksisterer
            if (overview.LatestJsonResult != null)
            {
                var jsonPath = Path.Combine(_testResultsPath, overview.LatestJsonResult.Filename);
                if (System.IO.File.Exists(jsonPath))
                {
                    try
                    {
                        var jsonContent = System.IO.File.ReadAllText(jsonPath);
                        overview.LatestTestData = JsonSerializer.Deserialize<BrunoTestResult>(jsonContent);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Kunne ikke parse JSON test resultat: {Error}", ex.Message);
                    }
                }
            }

            return Ok(overview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl ved hentning af test resultat oversigt");
            return StatusCode(500, "Intern server fejl ved hentning af test resultater");
        }
    }

    /// <summary>
    /// Henter det seneste test resultat i JSON format
    /// </summary>
    [HttpGet("latest")]
    public ActionResult<BrunoTestResult> GetLatestResult()
    {
        try
        {
            if (!Directory.Exists(_testResultsPath))
            {
                return NotFound("Ingen test resultater fundet");
            }

            var latestJsonFile = Directory.GetFiles(_testResultsPath, "*.json")
                .OrderByDescending(f => System.IO.File.GetLastWriteTime(f))
                .FirstOrDefault();

            if (latestJsonFile == null)
            {
                return NotFound("Ingen JSON test resultater fundet");
            }

            var jsonContent = System.IO.File.ReadAllText(latestJsonFile);
            var result = JsonSerializer.Deserialize<BrunoTestResult>(jsonContent);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl ved hentning af seneste test resultat");
            return StatusCode(500, "Intern server fejl ved hentning af test resultat");
        }
    }

    /// <summary>
    /// Henter et specifikt test resultat baseret på filnavn
    /// </summary>
    [HttpGet("file/{filename}")]
    public ActionResult<BrunoTestResult> GetResultByFilename(string filename)
    {
        try
        {
            if (!Directory.Exists(_testResultsPath))
            {
                return NotFound("Test resultater mappe ikke fundet");
            }

            var filePath = Path.Combine(_testResultsPath, filename);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound($"Test resultat fil '{filename}' ikke fundet");
            }

            if (!filename.EndsWith(".json"))
            {
                return BadRequest("Kun JSON filer kan hentes som data");
            }

            var jsonContent = System.IO.File.ReadAllText(filePath);
            var result = JsonSerializer.Deserialize<BrunoTestResult>(jsonContent);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl ved hentning af test resultat fil: {Filename}", filename);
            return StatusCode(500, "Intern server fejl ved hentning af test resultat");
        }
    }

    /// <summary>
    /// Henter HTML rapport som indhold (til iframe visning)
    /// </summary>
    [HttpGet("report/{filename}")]
    public ActionResult GetHtmlReport(string filename)
    {
        try
        {
            _logger.LogInformation("GetHtmlReport called for filename: {Filename}", filename);
            _logger.LogInformation("Looking in path: {TestResultsPath}", _testResultsPath);
            
            if (!Directory.Exists(_testResultsPath))
            {
                _logger.LogWarning("Test results directory does not exist: {TestResultsPath}", _testResultsPath);
                return NotFound("Test resultater mappe ikke fundet");
            }

            var filePath = Path.Combine(_testResultsPath, filename);
            _logger.LogInformation("Full file path: {FilePath}", filePath);
            _logger.LogInformation("File exists: {Exists}", System.IO.File.Exists(filePath));
            
            if (!System.IO.File.Exists(filePath))
            {
                // List available files for debugging
                var availableFiles = Directory.GetFiles(_testResultsPath, "*.html");
                _logger.LogWarning("HTML file not found. Available HTML files: {Files}", string.Join(", ", availableFiles.Select(Path.GetFileName)));
                return NotFound($"HTML rapport '{filename}' ikke fundet. Tilgængelige filer: {string.Join(", ", availableFiles.Select(Path.GetFileName))}");
            }

            if (!filename.EndsWith(".html"))
            {
                return BadRequest("Kun HTML filer kan hentes som rapporter");
            }

            var htmlContent = System.IO.File.ReadAllText(filePath);
            _logger.LogInformation("Successfully read HTML file, content length: {Length}", htmlContent.Length);
            
            // Return HTML content directly for iframe display
            return Content(htmlContent, "text/html");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl ved hentning af HTML rapport: {Filename}", filename);
            return StatusCode(500, "Intern server fejl ved hentning af rapport");
        }
    }

    /// <summary>
    /// Downloader HTML rapport som fil
    /// </summary>
    [HttpGet("download/{filename}")]
    public ActionResult DownloadHtmlReport(string filename)
    {
        try
        {
            if (!Directory.Exists(_testResultsPath))
            {
                return NotFound("Test resultater mappe ikke fundet");
            }

            var filePath = Path.Combine(_testResultsPath, filename);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound($"HTML rapport '{filename}' ikke fundet");
            }

            if (!filename.EndsWith(".html"))
            {
                return BadRequest("Kun HTML filer kan downloades");
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "text/html", filename);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl ved download af HTML rapport: {Filename}", filename);
            return StatusCode(500, "Intern server fejl ved download af rapport");
        }
    }

    /// <summary>
    /// Henter test statistikker
    /// </summary>
    [HttpGet("stats")]
    public ActionResult<object> GetTestStats()
    {
        try
        {
            if (!Directory.Exists(_testResultsPath))
            {
                return Ok(new
                {
                    totalFiles = 0,
                    jsonFiles = 0,
                    htmlFiles = 0,
                    latestRun = (DateTime?)null,
                    totalSize = 0
                });
            }

            var files = Directory.GetFiles(_testResultsPath)
                .Where(f => f.EndsWith(".json") || f.EndsWith(".html"))
                .ToList();

            var jsonFiles = files.Count(f => f.EndsWith(".json"));
            var htmlFiles = files.Count(f => f.EndsWith(".html"));
            var latestRun = files.Any() ? files.Max(f => System.IO.File.GetLastWriteTime(f)) : (DateTime?)null;
            var totalSize = files.Sum(f => new FileInfo(f).Length);

            return Ok(new
            {
                totalFiles = files.Count,
                jsonFiles,
                htmlFiles,
                latestRun,
                totalSize,
                totalSizeMB = Math.Round(totalSize / 1024.0 / 1024.0, 2)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl ved hentning af test statistikker");
            return StatusCode(500, "Intern server fejl ved hentning af statistikker");
        }
    }

    /// <summary>
    /// Sletter gamle test resultater (beholder kun de seneste 10)
    /// </summary>
    [HttpDelete("cleanup")]
    public ActionResult CleanupOldResults()
    {
        try
        {
            if (!Directory.Exists(_testResultsPath))
            {
                return Ok("Ingen test resultater at rydde op i");
            }

            var files = Directory.GetFiles(_testResultsPath)
                .Where(f => f.EndsWith(".json") || f.EndsWith(".html"))
                .OrderByDescending(f => System.IO.File.GetLastWriteTime(f))
                .ToList();

            var filesToDelete = files.Skip(10).ToList();
            var deletedCount = 0;

            foreach (var file in filesToDelete)
            {
                try
                {
                    System.IO.File.Delete(file);
                    deletedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Kunne ikke slette fil {File}: {Error}", file, ex.Message);
                }
            }

            return Ok(new
            {
                message = $"Ryddede op i {deletedCount} gamle test resultater",
                deletedCount,
                remainingCount = files.Count - deletedCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl ved oprydning af test resultater");
            return StatusCode(500, "Intern server fejl ved oprydning");
        }
    }
}
