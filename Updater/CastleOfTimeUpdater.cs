using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CastleOfTimeUpdater
{
    /// <summary>
    /// Auto-updater launcher for Castle of Time
    /// Checks GitHub Releases for updates, downloads, verifies, and launches the game
    /// </summary>
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UpdaterForm());
        }
    }

    public class UpdaterForm : Form
    {
        private const string REPO_OWNER = "Last-Minute-Games";
        private const string REPO_NAME = "game";
        private const string GAME_EXECUTABLE_WINDOWS = "CastleOfTime.exe";
        private const string GAME_EXECUTABLE_LINUX = "CastleOfTime.x86_64";
        private const string VERSION_FILE = "version.txt";
        
        private readonly string InstallDir = AppDomain.CurrentDomain.BaseDirectory;
        private readonly HttpClient httpClient = new HttpClient();

        private TextBox logTextBox;
        private ProgressBar progressBar;
        private Button launchButton;

        public UpdaterForm()
        {
            InitializeUI();
            _ = CheckAndUpdate();
        }

        private void InitializeUI()
        {
            this.Text = "Castle of Time - Updater";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Logo/Title
            var titleLabel = new Label
            {
                Text = "Castle of Time",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(560, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(titleLabel);

            // Log text box
            logTextBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(20, 70),
                Size = new Size(540, 200),
                Font = new Font("Consolas", 9)
            };
            this.Controls.Add(logTextBox);

            // Progress bar
            progressBar = new ProgressBar
            {
                Location = new Point(20, 280),
                Size = new Size(540, 25),
                Style = ProgressBarStyle.Continuous
            };
            this.Controls.Add(progressBar);

            // Launch button
            launchButton = new Button
            {
                Text = "Launch Game",
                Location = new Point(220, 315),
                Size = new Size(160, 35),
                Enabled = false,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            launchButton.Click += (s, e) => LaunchGame();
            this.Controls.Add(launchButton);
        }

        private void Log(string message)
        {
            if (logTextBox.InvokeRequired)
            {
                logTextBox.Invoke(new Action(() => Log(message)));
                return;
            }
            logTextBox.AppendText(message + Environment.NewLine);
        }

        private void SetProgress(int value)
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new Action(() => SetProgress(value)));
                return;
            }
            progressBar.Value = Math.Min(Math.Max(value, 0), 100);
        }

        private void EnableLaunchButton()
        {
            if (launchButton.InvokeRequired)
            {
                launchButton.Invoke(new Action(EnableLaunchButton));
                return;
            }
            launchButton.Enabled = true;
        }

        private async Task CheckAndUpdate()
        {
            try
            {
                Log("=== Castle of Time Updater ===");
                Log("");
                SetProgress(10);

                // Read current version
                string currentVersion = ReadLocalVersion();
                Log($"Current version: {currentVersion}");
                SetProgress(20);

                // Check for updates
                Log("Checking for updates...");
                var latestRelease = await FetchLatestRelease();
                
                if (latestRelease == null)
                {
                    Log("⚠️ Unable to check for updates.");
                    Log("Starting game with current version...");
                    SetProgress(100);
                    EnableLaunchButton();
                    return;
                }

                string latestVersion = latestRelease.TagName;
                Log($"Latest version: {latestVersion}");
                SetProgress(30);

                // Compare versions
                if (IsNewer(latestVersion, currentVersion))
                {
                    Log("");
                    Log($"🎮 New version available: {latestVersion}");
                    Log("Downloading update...");
                    Log("");
                    SetProgress(40);

                    bool success = await DownloadAndInstallUpdate(latestRelease);
                    
                    if (success)
                    {
                        WriteLocalVersion(latestVersion);
                        Log("");
                        Log("✅ Update installed successfully!");
                        SetProgress(100);
                    }
                    else
                    {
                        Log("");
                        Log("⚠️ Update failed. Launching current version...");
                        SetProgress(100);
                    }
                }
                else
                {
                    Log("✅ You're up to date!");
                    SetProgress(100);
                }

                Log("");
                Log("Ready to launch Castle of Time!");
                EnableLaunchButton();
            }
            catch (Exception ex)
            {
                Log("");
                Log($"❌ Error: {ex.Message}");
                Log("");
                Log("You can still try to launch the game.");
                SetProgress(100);
                EnableLaunchButton();
            }
        }

        private string ReadLocalVersion()
        {
            string versionPath = Path.Combine(InstallDir, VERSION_FILE);
            if (File.Exists(versionPath))
            {
                return File.ReadAllText(versionPath).Trim();
            }
            return "unknown";
        }

        private void WriteLocalVersion(string version)
        {
            string versionPath = Path.Combine(InstallDir, VERSION_FILE);
            File.WriteAllText(versionPath, version);
        }

        private async Task<GitHubRelease?> FetchLatestRelease()
        {
            try
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("CastleOfTime-Updater/1.0");
                httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
                
                string apiUrl = $"https://api.github.com/repos/{REPO_OWNER}/{REPO_NAME}/releases/latest";
                var response = await httpClient.GetStringAsync(apiUrl);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                return JsonSerializer.Deserialize<GitHubRelease>(response, options);
            }
            catch (Exception ex)
            {
                Log($"Warning: {ex.Message}");
                return null;
            }
        }

        private bool IsNewer(string remoteVersion, string localVersion)
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

        private async Task<bool> DownloadAndInstallUpdate(GitHubRelease release)
        {
            try
            {
                // Determine platform
                string platform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" : "Linux";
                
                // Extract just the number from the tag (e.g., "v1.0.100" -> "100")
                string versionNumber = release.TagName.TrimStart('v');
                var parts = versionNumber.Split('.');
                string buildNumber = parts.Length > 0 ? parts[parts.Length - 1] : versionNumber;
                
                string expectedFileName = $"CastleOfTime-{buildNumber}-{platform}.zip";
                
                // Find the asset for our platform
                var asset = release.Assets.Find(a => a.Name == expectedFileName);
                
                if (asset == null)
                {
                    Log($"❌ No update available for platform: {platform}");
                    Log($"Looking for: {expectedFileName}");
                    Log($"Available assets:");
                    foreach (var a in release.Assets)
                    {
                        Log($"  - {a.Name}");
                    }
                    return false;
                }

                // Create temp directory
                string tempDir = Path.Combine(Path.GetTempPath(), $"CastleOfTime_Update_{Guid.NewGuid()}");
                Directory.CreateDirectory(tempDir);

                try
                {
                    // Download zip
                    string zipPath = Path.Combine(tempDir, asset.Name);
                    Log($"Downloading from GitHub...");
                    SetProgress(50);
                    
                    await DownloadFileWithProgress(asset.BrowserDownloadUrl, zipPath);

                    // Extract to temp location
                    string extractDir = Path.Combine(tempDir, "extracted");
                    Log("Extracting update...");
                    SetProgress(80);
                    ZipFile.ExtractToDirectory(zipPath, extractDir);

                    // Replace game files
                    Log("Installing update...");
                    SetProgress(90);
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
                Log($"Update failed: {ex.Message}");
                return false;
            }
        }

        private async Task DownloadFileWithProgress(string url, string destPath)
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
                        int progressValue = 50 + (percent / 3); // Map to 50-83% range
                        SetProgress(progressValue);
                        lastPercent = percent;
                    }
                }
            }
        }

        private void ReplaceGameFiles(string sourceDir, string targetDir)
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

        private void LaunchGame()
        {
            try
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
                    MessageBox.Show($"Game executable not found: {gameExe}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = gameExe,
                    WorkingDirectory = InstallDir,
                    UseShellExecute = true
                };

                Process.Start(startInfo);
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to launch game: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // JSON Models for GitHub API response
    public class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("assets")]
        public List<GitHubAsset> Assets { get; set; } = new();
    }

    public class GitHubAsset
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; set; } = "";

        [JsonPropertyName("size")]
        public long Size { get; set; }
    }
}
