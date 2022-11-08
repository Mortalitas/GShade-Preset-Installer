using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows;

namespace GShadePresetInstaller
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static int _language = 1; // For future use. Currently only English is supported. 1 = English, 2 = Japanese, 3 = Korean, 4 = German, 5 = French, 6 = Italian
        public static readonly string _instVer = System.Windows.Forms.Application.ProductVersion;
        public static readonly string _instName = System.Windows.Forms.Application.ProductName;
        public static readonly string _windowTitle = _instName + " " + _instVer;
        public static readonly string _instPath = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles"), "GShade");
        public static readonly string _zipTempPath = Path.Combine(_instPath, "PresetsTemp");
        public static readonly string _presetPathName = System.Windows.Forms.Application.CompanyName; // You can change this to override using the Company Name in AssemblyInfo.cs as the name of the preset folder.
        public static string _instLog = "Starting preset installation...";
        public static string _instState = "Starting preset installation...";
        public static List<string> _gamePaths = new List<string>();
        public static System.Text.UTF8Encoding utf8NoBOM = new System.Text.UTF8Encoding(false);

        public static void AddLog(string log)
        {
            _instLog = _instLog + "\r\n" + log;
            _instState = log;

            try
            {
                using (StreamWriter logFile = System.IO.File.AppendText(Path.Combine(_instPath, "PresetLog.txt")))
                {
                    logFile.WriteLine("\r\n" + log);
                    logFile.Close();
                }
            }
            catch
            {
            }
        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            // Check if the Directory/Link exists.
            Debug.Assert(source.Exists);

            if (!Directory.Exists(target.FullName))
            {
                try
                {
                    Directory.CreateDirectory(target.FullName);
                    AddLog("Created: " + target.FullName);
                }
                catch
                {
                    AddLog("Failed to create: " + target.FullName);
                }
            }

            FileSystemInfo[] sourceFiles;
            try
            {
                sourceFiles = source.GetFileSystemInfos("*", SearchOption.AllDirectories);
            }
            catch
            {
                // Reference does not exist.
                sourceFiles = new FileSystemInfo[0];
            }

            foreach (var fsi in sourceFiles)
            {
                var sourceName = fsi.FullName.Remove(0, source.FullName.Length + 1);
                var targetPath = Path.Combine(target.FullName, sourceName);

                if (fsi.Attributes.HasFlag(System.IO.FileAttributes.Directory))
                {
                    CreateFolder(targetPath);
                }
                else
                {
                    CopyFile(fsi.FullName, targetPath, true);
                }
            }
        }

        private static void CopyDirectory(string sourceDirectory, string targetDirectory)
        {
            try
            {
                DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
                DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

                CopyAll(diSource, diTarget);
                AddLog("Copied: " + sourceDirectory + " To: " + targetDirectory);
            }
            catch
            {
                AddLog("Failed to copy: " + sourceDirectory + " To: " + targetDirectory);
            }
        }

        private static void CopyFile(string source, string target, bool overwrite)
        {
            try
            {
                System.IO.File.Copy(source, target, overwrite);
                AddLog("Copied: " + source + " To: " + target);
            }
            catch
            {
                AddLog("Failed to copy: " + source + " To: " + target);
            }
        }

        private static void CreateFolder(string path)
        {
            if (Directory.Exists(path))
                AddLog("Folder already exists: " + path);
            else
            {
                try
                {
                    Directory.CreateDirectory(path);
                    AddLog("Created: " + path);
                }
                catch
                {
                    AddLog("Failed to create: " + path);
                }
            }
        }

        private static void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                    AddLog("Removed: " + path);
                }
                catch (IOException)
                {
                    try
                    {
                        Directory.Delete(path, true);
                        AddLog("Removed: " + path);
                    }
                    catch
                    {
                        AddLog("Failed to remove (IOException): " + path);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    try
                    {
                        Directory.Delete(path, true);
                        AddLog("Removed: " + path);
                    }
                    catch
                    {
                        AddLog("Failed to remove (UnauthorizedAccessException): " + path);
                    }
                }
                catch
                {
                    AddLog("Failed to remove: " + path);
                }
            }
        }

        private static void DeleteFile(string path)
        {
            if (System.IO.File.Exists(path))
            {
                try
                {
                    FileInfo pathInfo = new FileInfo(path);
                    if (pathInfo.IsReadOnly)
                        pathInfo.IsReadOnly = false;
                }
                catch
                {
                }

                try
                {
                    System.IO.File.Delete(path);
                    AddLog("Removed: " + path);
                }
                catch (IOException)
                {
                    try
                    {
                        System.IO.File.Delete(path);
                        AddLog("Removed: " + path);
                    }
                    catch
                    {
                        AddLog("Failed to remove (IOException): " + path);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    try
                    {
                        System.IO.File.Delete(path);
                        AddLog("Removed: " + path);
                    }
                    catch
                    {
                        AddLog("Failed to remove (UnauthorizedAccessException): " + path);
                    }
                }
                catch
                {
                    AddLog("Failed to remove: " + path);
                }
            }
            else
            {
                AddLog("File does not exist: " + path);
            }
        }

        private static bool KeyExists(RegistryKey baseKey, string subKeyName)
        {
            RegistryKey ret = baseKey.OpenSubKey(subKeyName);

            return ret != null;
        }

        public static bool BuildGamePaths()
        {
            try
            {
                // Delete any previous logs.
                if (File.Exists(Path.Combine(_instPath, "PresetLog.txt")))
                    DeleteFile(Path.Combine(_instPath, "PresetLog.txt"));                    

                if (KeyExists(Registry.LocalMachine, "Software\\GShade"))
                {
                    if (KeyExists(Registry.LocalMachine, "Software\\GShade\\Installations"))
                    {
                        RegistryKey instKey = Registry.LocalMachine.OpenSubKey("Software\\GShade\\Installations", false);
                        string[] instKeys = instKey.GetSubKeyNames();
                        instKey.Close();

                        // Build a list of installation paths.
                        foreach (string regKey in instKeys)
                        {
                            RegistryKey tempKey = Registry.LocalMachine.OpenSubKey("Software\\GShade\\Installations\\" + regKey, false);
                            // GUID length to make sure only relevant keys are parsed. Check to see if the exe path key exists to ensure that only valid installations are listed.
                            if (regKey.Length > 30 && tempKey.GetValue("exedir") != null)
                            {
                                // Only add paths that currently exist to the list.
                                if (System.IO.Directory.Exists((string)tempKey.GetValue("exedir")))
                                {
                                    _gamePaths.Add((string)tempKey.GetValue("exedir"));
                                }
                            }

                            tempKey.Close();
                        }
                    }
                }

                return true;
            }
            catch
            {
                AddLog("Failed to build a list of GShade installations. Aborting installation");
                return false;
            }
        }

        public static bool ZipExtractionProcess()
        {
            // Check if ?:\Program Files\GShade exists.
            if (!System.IO.Directory.Exists(_instPath))
            {
                AddLog("GShade installation not found. Aborting preset installtion.");
                return false;
            }

            // If a previous PresetsTemp folder exists, remove it.
            if (System.IO.Directory.Exists(_zipTempPath))
                DeleteDirectory(_zipTempPath);

            CreateFolder(_zipTempPath);

            string zipPath = Path.Combine(_instPath, "PresetsTemp.zip");
            // Begin file extraction.
            try
            {
                System.IO.File.WriteAllBytes(zipPath, GShadePresetInstaller.Properties.Resources.PresetPayload);
            }
            catch
            {
                AddLog("Failed to unzip: " + zipPath + " To: " + _zipTempPath);
                return false;
            }

            try
            {
                ZipFile.ExtractToDirectory(zipPath, _zipTempPath);
                AddLog("Unzipped: " + zipPath + " To: " + _zipTempPath);
            }
            catch
            {
                AddLog("Failed to unzip: " + zipPath + " To: " + _zipTempPath);
                return false;
            }
            // Delete payload zip after extraction.
            if (System.IO.File.Exists(zipPath))
                DeleteFile(zipPath);

            return true;
        }

        public static void FileDeploymentProcess()
        {
            string[] fileList = Directory.GetFiles(_zipTempPath);
            List<string> imageList = new List<string> {};

            // Build a list of textures from our extracted zip.
            foreach (string file in fileList)
            {
                if (file.Length >= 5)
                {
                    if (file.Substring(file.Length - 4) == ".png" || file.Substring(file.Length - 4) == ".dds" || file.Substring(file.Length - 4) == ".jpg" || file.Substring(file.Length - 5) == ".jpeg")
                    {
                        imageList.Add(file);
                    }
                }
            }
            // Move the textures into the global GShade Textures folder.
            foreach (string imagePath in imageList)
            {
                CopyFile(imagePath, Path.Combine(_instPath, "gshade-shaders\\Textures", Path.GetFileName(imagePath)), true);
                DeleteFile(imagePath);
            }

            // Copy presets to various GShade installations.
            foreach (string directory in _gamePaths)
            {
                CopyDirectory(_zipTempPath, Path.Combine(directory, "gshade-presets", _presetPathName));
            }

            DeleteDirectory(_zipTempPath);
        }

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += (ls, le) => MessageBox.Show(le.ExceptionObject.ToString(), _windowTitle, MessageBoxButton.OK, MessageBoxImage.Error);

            // Set default font size
            {
                var style = new Style(typeof(Window));
                {
                    style.Setters.Add(new Setter(Window.FontSizeProperty, 12.0));
                }
                FrameworkElement.StyleProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(style));
            }
        }
    }
}
