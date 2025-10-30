using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace CastleOfTimeUpdater
{
    /// <summary>
    /// Auto-updater launcher for Castle of Time
    /// Checks GitHub Releases for updates, downloads, verifies, and launches the game
    /// </summary>
    class Program
    {
        private const string MANIFEST_URL = "https://github.com/Last-Minute-Games/game/releases/latest/download/manifest.json";
        private const string GAME_EXECUTABLE_WINDOWS = "CastleOfTime.exe";
        private const string GAME_EXECUTABLE_LINUX = "CastleOfTime.x86_64";
        private const string VERSION_FILE = "version.txt";
        
        private static readonly string InstallDir = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly HttpClient httpClient = new HttpClient();

        static async Task<int> Main(string[] args)
        {
            Console.WriteLine("=== Castle of Time Updater ===\n");

            try
            {
                // Read current version
                string currentVersion = ReadLocalVersion();
                Console.WriteLine($"Current version: {currentVersion}");

                // Check for updates
                Console.WriteLine("\nChecking for updates...");
                var manifest = await FetchManifest();
                
                if (manifest == null)
                {
                    Console.WriteLine("Unable to check for updates. Starting game with current version...");
                    LaunchGame();
                    return 0;
                }

                Console.WriteLine($"Latest version: {manifest.Version}");

                // Compare versions
                if (IsNewer(manifest.Version, currentVersion))
                {
                    Console.WriteLine($"\n🎮 New version available: {manifest.Version}");
                    Console.WriteLine("Downloading update...\n");

                    bool success = await DownloadAndInstallUpdate(manifest);
                    
                    if (success)
                    {
                        WriteLocalVersion(manifest.Version);
                        Console.WriteLine("\n✅ Update installed successfully!");
                    }
                    else
                    {
                        Console.WriteLine("\n⚠️ Update failed. Launching current version...");
                    }
                }
                else
                {
                    Console.WriteLine("✅ You're up to date!");
                }

                // Launch the game
                Console.WriteLine("\nLaunching Castle of Time...");
                LaunchGame();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error: {ex.Message}");
                Console.WriteLine("\nAttempting to launch game anyway...");
                
                try
                {
                    LaunchGame();
                    return 0;
                }
                catch
                {
                    Console.WriteLine("Failed to launch game. Press any key to exit...");
                    Console.ReadKey();
                    return 1;
                }
            }
        }

        private static string ReadLocalVersion()
        {
            string versionPath = Path.Combine(InstallDir, VERSION_FILE);
            if (File.Exists(versionPath))
            {
                return File.ReadAllText(versionPath).Trim();
            }
            return "unknown";
        }

        private static void WriteLocalVersion(string version)
        {
            string versionPath = Path.Combine(InstallDir, VERSION_FILE);
            File.WriteAllText(versionPath, version);
        }

        private static async Task<UpdateManifest?> FetchManifest()
        {
            try
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("CastleOfTime-Updater/1.0");
                var response = await httpClient.GetStringAsync(MANIFEST_URL);
                return JsonSerializer.Deserialize<UpdateManifest>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Unable to fetch manifest: {ex.Message}");
                return null;
            }
        }

        private static bool IsNewer(string remoteVersion, string localVersion)
        {
            // Handle "unknown" local version
            if (localVersion == "unknown") return true;

            // Try semantic versioning comparison
            if (Version.TryParse(remoteVersion.TrimStart('v'), out var remote) &&
                Version.TryParse(localVersion.TrimStart('v'), out var local))
            {
                return remote > local;
            }

            // Fallback to string comparison
            return string.CompareOrdinal(remoteVersion, localVersion) > 0;
        }

        private static async Task<bool> DownloadAndInstallUpdate(UpdateManifest manifest)
        {
            try
            {
                // Determine platform
                string platform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows" : "linux";
                
                if (!manifest.Platforms.TryGetValue(platform, out var platformInfo))
                {
                    Console.WriteLine($"No update available for platform: {platform}");
                    return false;
                }

                // Create temp directory
                string tempDir = Path.Combine(Path.GetTempPath(), $"CastleOfTime_Update_{Guid.NewGuid()}");
                Directory.CreateDirectory(tempDir);

                try
                {
                    // Download zip
                    string zipPath = Path.Combine(tempDir, platformInfo.Filename);
                    Console.WriteLine($"Downloading from {platformInfo.Url}...");
                    
                    await DownloadFileWithProgress(platformInfo.Url, zipPath);

                    // Verify SHA256
                    Console.WriteLine("\nVerifying download integrity...");
                    string actualHash = ComputeSHA256(zipPath);
                    
                    if (!actualHash.Equals(platformInfo.Sha256, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("❌ Hash mismatch! Update may be corrupted.");
                        Console.WriteLine($"Expected: {platformInfo.Sha256}");
                        Console.WriteLine($"Got:      {actualHash}");
                        return false;
                    }
                    Console.WriteLine("✅ Download verified");

                    // Extract to temp location
                    string extractDir = Path.Combine(tempDir, "extracted");
                    Console.WriteLine("\nExtracting update...");
                    ZipFile.ExtractToDirectory(zipPath, extractDir);

                    // Replace game files
                    Console.WriteLine("Installing update...");
                    ReplaceGameFiles(extractDir, InstallDir);

                    // Fix permissions on Linux
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        string gameExe = Path.Combine(InstallDir, GAME_EXECUTABLE_LINUX);
                        if (File.Exists(gameExe))
                        {
                            Process.Start("chmod", $"+x \"{gameExe}\"")?.WaitForExit();
                        }
                    }

                    return true;
                }
                finally
                {
                    // Cleanup temp directory
                    try
                    {
                        Directory.Delete(tempDir, true);
                    }
                    catch { /* Ignore cleanup errors */ }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update failed: {ex.Message}");
                return false;
            }
        }

        private static async Task DownloadFileWithProgress(string url, string destPath)
        {
            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            long? totalBytes = response.Content.Headers.ContentLength;
            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            long totalRead = 0;
            int bytesRead;
            int lastPercent = -1;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                totalRead += bytesRead;

                if (totalBytes.HasValue)
                {
                    int percent = (int)((totalRead * 100) / totalBytes.Value);
                    if (percent != lastPercent)
                    {
                        Console.Write($"\rProgress: {percent}% ({FormatBytes(totalRead)} / {FormatBytes(totalBytes.Value)})");
                        lastPercent = percent;
                    }
                }
            }
            Console.WriteLine();
        }

        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private static string ComputeSHA256(string filePath)
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hash = sha256.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private static void ReplaceGameFiles(string sourceDir, string targetDir)
        {
            // Get all files from source
            foreach (string sourceFile in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(sourceDir, sourceFile);
                string targetFile = Path.Combine(targetDir, relativePath);

                // Create directory if needed
                string? targetDirPath = Path.GetDirectoryName(targetFile);
                if (targetDirPath != null && !Directory.Exists(targetDirPath))
                {
                    Directory.CreateDirectory(targetDirPath);
                }

                // Don't overwrite the updater itself or version file while running
                string fileName = Path.GetFileName(targetFile).ToLower();
                if (fileName.Contains("updater"))
                {
                    continue;
                }

                // Copy file with retry logic (in case file is briefly locked)
                int retries = 3;
                while (retries > 0)
                {
                    try
                    {
                        File.Copy(sourceFile, targetFile, true);
                        break;
                    }
                    catch
                    {
                        retries--;
                        if (retries == 0) throw;
                        System.Threading.Thread.Sleep(500);
                    }
                }
            }
        }

        private static void LaunchGame()
        {
            string gameExe;
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                gameExe = Path.Combine(InstallDir, GAME_EXECUTABLE_WINDOWS);
            }
            else
            {
                gameExe = Path.Combine(InstallDir, GAME_EXECUTABLE_LINUX);
            }

            if (!File.Exists(gameExe))
            {
                throw new FileNotFoundException($"Game executable not found: {gameExe}");
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = gameExe,
                WorkingDirectory = InstallDir,
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }
    }

    // JSON Models for manifest.json
    public class UpdateManifest
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = "";

        [JsonPropertyName("released")]
        public string Released { get; set; } = "";

        [JsonPropertyName("platforms")]
        public Dictionary<string, PlatformInfo> Platforms { get; set; } = new();
    }

    public class PlatformInfo
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = "";

        [JsonPropertyName("sha256")]
        public string Sha256 { get; set; } = "";

        [JsonPropertyName("filename")]
        public string Filename { get; set; } = "";
    }
}
