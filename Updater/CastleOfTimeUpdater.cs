using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
#if WINDOWS
using System.Drawing;
using System.Windows.Forms;
#endif

namespace CastleOfTimeUpdater
{
    class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
#if WINDOWS
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new UpdaterForm());
                return 0;
            }
#endif
            // Linux console version
            return new ConsoleUpdater().Run().GetAwaiter().GetResult();
        }
    }

    // Base class with shared logic
    public abstract class UpdaterBase
    {
        protected const string REPO_OWNER = "Last-Minute-Games";
        protected const string REPO_NAME = "game";
        protected const string GAME_EXECUTABLE_WINDOWS = "CastleOfTime.exe";
        protected const string GAME_EXECUTABLE_LINUX = "CastleOfTime.x86_64";
        protected const string VERSION_FILE = "version.txt";
        
        protected readonly string InstallDir = AppDomain.CurrentDomain.BaseDirectory;
        protected readonly HttpClient httpClient = new HttpClient();

        protected abstract void Log(string message);
        protected abstract void SetProgress(int value);

        protected string ReadLocalVersion()
        {
            string versionPath = Path.Combine(InstallDir, VERSION_FILE);
            if (File.Exists(versionPath))
            {
                return File.ReadAllText(versionPath).Trim();
            }
            return "unknown";
        }

        protected void WriteLocalVersion(string version)
        {
            string versionPath = Path.Combine(InstallDir, VERSION_FILE);
            File.WriteAllText(versionPath, version);
        }

        protected async Task<GitHubRelease?> FetchLatestRelease()
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

        protected bool IsNewer(string remoteVersion, string localVersion)
        {
            if (localVersion == "unknown") return true;

            if (Version.TryParse(remoteVersion.TrimStart('v'), out var remote) &&
                Version.TryParse(localVersion.TrimStart('v'), out var local))
            {
                return remote > local;
            }

            return string.CompareOrdinal(remoteVersion, localVersion) > 0;
        }

        protected async Task<bool> DownloadAndInstallUpdate(GitHubRelease release)
        {
            try
            {
                string platform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" : "Linux";
                
                string versionNumber = release.TagName.TrimStart('v');
                var parts = versionNumber.Split('.');
                string buildNumber = parts.Length > 0 ? parts[parts.Length - 1] : versionNumber;
                
                string expectedFileName = $"CastleOfTime-{buildNumber}-{platform}.zip";
                
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

                string tempDir = Path.Combine(Path.GetTempPath(), $"CastleOfTime_Update_{Guid.NewGuid()}");
                Directory.CreateDirectory(tempDir);

                try
                {
                    string zipPath = Path.Combine(tempDir, asset.Name);
                    Log($"Downloading from GitHub...");
                    SetProgress(50);
                    
                    await DownloadFileWithProgress(asset.BrowserDownloadUrl, zipPath);

                    string extractDir = Path.Combine(tempDir, "extracted");
                    Log("Extracting update...");
                    SetProgress(80);
                    ZipFile.ExtractToDirectory(zipPath, extractDir);

                    Log("Installing update...");
                    SetProgress(90);

                    // Clean install directory to avoid corruption: delete everything except the updater itself
                    try
                    {
                        CleanInstallDirectoryExceptUpdater(InstallDir);
                    }
                    catch (Exception ex)
                    {
                        Log($"Warning: failed to fully clean install dir: {ex.Message}");
                    }

                    ReplaceGameFiles(extractDir, InstallDir);

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
                    try
                    {
                        Directory.Delete(tempDir, true);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Log($"Update failed: {ex.Message}");
                return false;
            }
        }

        protected async Task DownloadFileWithProgress(string url, string destPath)
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
                        int progressValue = 50 + (percent / 3);
                        SetProgress(progressValue);
                        lastPercent = percent;
                    }
                }
            }
        }

        protected void ReplaceGameFiles(string sourceDir, string targetDir)
        {
            foreach (string sourceFile in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(sourceDir, sourceFile);
                string targetFile = Path.Combine(targetDir, relativePath);

                string? targetDirPath = Path.GetDirectoryName(targetFile);
                if (targetDirPath != null && !Directory.Exists(targetDirPath))
                {
                    Directory.CreateDirectory(targetDirPath);
                }

                string fileName = Path.GetFileName(targetFile).ToLower();
                if (fileName.Contains("updater"))
                {
                    continue;
                }

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

        /// <summary>
        /// Deletes all files and directories inside the install directory except the updater executable.
        /// This helps ensure a clean install in case of corruption. Non-fatal: logs failures.
        /// </summary>
        /// <param name="installDir">The installation directory (InstallDir)</param>
        protected void CleanInstallDirectoryExceptUpdater(string installDir)
        {
            // Find the running updater executable path
            string? updaterFullPath = null;
            try
            {
                updaterFullPath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
            }
            catch { }

            if (string.IsNullOrEmpty(updaterFullPath))
            {
                updaterFullPath = Path.Combine(installDir, "CastleOfTimeUpdater.exe");
            }

            updaterFullPath = Path.GetFullPath(updaterFullPath);

            // Delete all files and directories under installDir except the updaterFullPath file itself.
            // If updater is in a subfolder, we will still delete other files in that folder, but not the updater file.

            // Delete files recursively
            foreach (var file in Directory.GetFiles(installDir, "*", SearchOption.AllDirectories))
            {
                try
                {
                    string fileFull = Path.GetFullPath(file);
                    if (string.Equals(fileFull, updaterFullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    File.SetAttributes(fileFull, FileAttributes.Normal);
                    int tries = 3;
                    while (tries > 0)
                    {
                        try
                        {
                            File.Delete(fileFull);
                            break;
                        }
                        catch
                        {
                            tries--;
                            System.Threading.Thread.Sleep(200);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log($"  ⚠️ Could not delete file {file}: {ex.Message}");
                }
            }

            // Delete empty directories (bottom-up)
            var allDirs = Directory.GetDirectories(installDir, "*", SearchOption.AllDirectories);
            // Sort by path length descending so children are deleted before parents
            Array.Sort(allDirs, (a, b) => b.Length.CompareTo(a.Length));

            foreach (var dir in allDirs)
            {
                try
                {
                    // If the updater file resides somewhere under this directory, we must not delete the file itself; but we can delete other files already removed above.
                    // Attempt to delete the directory; it will fail if updater file still exists inside.
                    if (Directory.Exists(dir) && Directory.GetFileSystemEntries(dir).Length == 0)
                    {
                        int tries = 3;
                        while (tries > 0)
                        {
                            try
                            {
                                Directory.Delete(dir, false);
                                break;
                            }
                            catch
                            {
                                tries--;
                                System.Threading.Thread.Sleep(200);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log($"  ⚠️ Could not delete directory {dir}: {ex.Message}");
                }
            }
        }

        public void LaunchGame()
        {
            string gameExe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
                ? Path.Combine(InstallDir, GAME_EXECUTABLE_WINDOWS)
                : Path.Combine(InstallDir, GAME_EXECUTABLE_LINUX);

            if (!File.Exists(gameExe))
            {
                Log($"Game executable not found: {gameExe}");
                return;
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

    // Console version for Linux
    public class ConsoleUpdater : UpdaterBase
    {
        protected override void Log(string message)
        {
            Console.WriteLine(message);
        }

        protected override void SetProgress(int value)
        {
            // Simple progress indicator for console
        }

        public async Task<int> Run()
        {
            try
            {
                Log("=== Castle of Time Updater ===");
                Log("");

                string currentVersion = ReadLocalVersion();
                Log($"Current version: {currentVersion}");

                Log("Checking for updates...");
                var latestRelease = await FetchLatestRelease();
                
                if (latestRelease == null)
                {
                    Log("⚠️ Unable to check for updates.");
                    Log("Starting game with current version...");
                    LaunchGame();
                    return 0;
                }

                string latestVersion = latestRelease.TagName;
                Log($"Latest version: {latestVersion}");

                if (IsNewer(latestVersion, currentVersion))
                {
                    Log("");
                    Log($"🎮 New version available: {latestVersion}");
                    Log("Downloading update...");
                    Log("");

                    bool success = await DownloadAndInstallUpdate(latestRelease);
                    
                    if (success)
                    {
                        WriteLocalVersion(latestVersion);
                        Log("");
                        Log("✅ Update installed successfully!");
                    }
                    else
                    {
                        Log("");
                        Log("⚠️ Update failed. Launching current version...");
                    }
                }
                else
                {
                    Log("✅ You're up to date!");
                }

                Log("");
                Log("Launching Castle of Time...");
                LaunchGame();
                return 0;
            }
            catch (Exception ex)
            {
                Log("");
                Log($"❌ Error: {ex.Message}");
                Log("");
                Log("Attempting to launch game anyway...");
                
                try
                {
                    LaunchGame();
                    return 0;
                }
                catch
                {
                    Log("Failed to launch game.");
                    return 1;
                }
            }
        }
    }

#if WINDOWS
    // GUI version for Windows
    public class UpdaterForm : Form
    {
        private UpdaterBase? updater;
        private TextBox? logTextBox;
        private ProgressBar? progressBar;
        private Button? launchButton;

        public UpdaterForm()
        {
            InitializeUI();
            updater = new GUIUpdater(this);
            _ = ((GUIUpdater)updater).CheckAndUpdate();
        }

        private void InitializeUI()
        {
            this.Text = "Castle of Time - Updater";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var titleLabel = new Label
            {
                Text = "Castle of Time",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(560, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(titleLabel);

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

            progressBar = new ProgressBar
            {
                Location = new Point(20, 280),
                Size = new Size(540, 25),
                Style = ProgressBarStyle.Continuous
            };
            this.Controls.Add(progressBar);

            launchButton = new Button
            {
                Text = "Launch Game",
                Location = new Point(220, 315),
                Size = new Size(160, 35),
                Enabled = false,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            launchButton.Click += (s, e) => LaunchGameAndExit();
            this.Controls.Add(launchButton);
        }

        public void AppendLog(string message)
        {
            if (logTextBox!.InvokeRequired)
            {
                logTextBox.Invoke(new Action(() => AppendLog(message)));
                return;
            }
            logTextBox.AppendText(message + Environment.NewLine);
        }

        public void UpdateProgress(int value)
        {
            if (progressBar!.InvokeRequired)
            {
                progressBar.Invoke(new Action(() => UpdateProgress(value)));
                return;
            }
            progressBar.Value = Math.Min(Math.Max(value, 0), 100);
        }

        public void EnableLaunch()
        {
            if (launchButton!.InvokeRequired)
            {
                launchButton.Invoke(new Action(EnableLaunch));
                return;
            }
            launchButton.Enabled = true;
        }

        private void LaunchGameAndExit()
        {
            try
            {
                updater?.LaunchGame();
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to launch game: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class GUIUpdater : UpdaterBase
    {
        private readonly UpdaterForm form;

        public GUIUpdater(UpdaterForm form)
        {
            this.form = form;
        }

        protected override void Log(string message)
        {
            form.AppendLog(message);
        }

        protected override void SetProgress(int value)
        {
            form.UpdateProgress(value);
        }

        public async Task CheckAndUpdate()
        {
            try
            {
                Log("=== Castle of Time Updater ===");
                Log("");
                SetProgress(10);

                string currentVersion = ReadLocalVersion();
                Log($"Current version: {currentVersion}");
                SetProgress(20);

                Log("Checking for updates...");
                var latestRelease = await FetchLatestRelease();
                
                if (latestRelease == null)
                {
                    Log("⚠️ Unable to check for updates.");
                    Log("Starting game with current version...");
                    SetProgress(100);
                    form.EnableLaunch();
                    return;
                }

                string latestVersion = latestRelease.TagName;
                Log($"Latest version: {latestVersion}");
                SetProgress(30);

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
                form.EnableLaunch();
            }
            catch (Exception ex)
            {
                Log("");
                Log($"❌ Error: {ex.Message}");
                Log("");
                Log("You can still try to launch the game.");
                SetProgress(100);
                form.EnableLaunch();
            }
        }
    }
#endif

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
