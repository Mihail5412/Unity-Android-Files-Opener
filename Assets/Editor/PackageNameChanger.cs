/*MIT License

Copyright(c) 2020 Mikhail5412

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using System;

#if UNITY_EDITOR
namespace UnityAndroidOpenUrl.EditorScripts
{
    /// <summary>
    /// Static class whose task is to update the package name in AndroidManifest.xml if it has been changed
    /// </summary>
    [InitializeOnLoad]
    public static class PackageNameChanger
    {
        private const string PLUGINS_DIR = "Plugins";
        private const string TEMP_DIR = "Temp";
        private const string AAR_NAME = "release.aar";
        private const string MANIFEST_NAME = "AndroidManifest.xml";
        private const string PROVIDER_PATHS_NAME = "res/xml/filepaths.xml";

        private static string pathToPluginsFolder;
        private static string pathToTempFolder;
        private static string pathToBinary;

        private static string lastPackageName = "com.company.product";

        private static bool stopedByError;
        static PackageNameChanger()
        {
            pathToPluginsFolder = Path.Combine(Application.dataPath, PLUGINS_DIR);
            if(!Directory.Exists(pathToPluginsFolder))
            {
                Debug.LogError("Plugins folder not found. Please re-import asset. See README.md for details...");
                return;
            }
            pathToTempFolder = Path.Combine(pathToPluginsFolder, TEMP_DIR);
            pathToBinary = Path.Combine(pathToPluginsFolder, AAR_NAME);
            if (!File.Exists(pathToBinary))
            {
                Debug.LogError("File release.aar not found. Please re-import asset. See README.md for details...");
                return;
            }

            EditorApplication.update += Update;
            TryUpdatePackageName();
        }

        static void Update()
        {
            if (stopedByError)
                return;

            if (lastPackageName != PlayerSettings.applicationIdentifier)
            {
                TryUpdatePackageName();
            }
        }

        private static void TryUpdatePackageName()
        {
            FileInfo fileInfo = new FileInfo(pathToBinary);
            if(!IsFileAlreadyOpen(fileInfo))
            {
                RepackBinary();
            }
        }

        private static void RepackBinary()
        {
            try
            {
                ExtractBinary();
            }
            catch (Exception e)
            {
                Debug.LogError("Extract release.aar error: " + e.Message);
                stopedByError = true;
                return;
            }

            ChangePackageName();

            try
            {
                ZippingBinary();
            }
            catch (Exception e)
            {
                Debug.LogError("Zipping release.aar error: " + e.Message);
                stopedByError = true;
                return;
            }

            Directory.Delete(pathToTempFolder, true);
        }

        private static void ExtractBinary()
        {
            if (!File.Exists(pathToBinary))
            {
                throw new Exception("File release.aar not found. Please reimport asset. See README.md for details...");
            }

            if (!Directory.Exists(pathToTempFolder))
            {
                Directory.CreateDirectory(pathToTempFolder);
            }

            using (FileStream fs = new FileStream(pathToBinary, FileMode.Open))
            {
                using (ZipFile zf = new ZipFile(fs))
                {

                    for (int i = 0; i < zf.Count; ++i)
                    {
                        ZipEntry zipEntry = zf[i];
                        string fileName = zipEntry.Name;

                        if (zipEntry.IsDirectory)
                        {
                            Directory.CreateDirectory(Path.Combine(pathToTempFolder, fileName));
                            continue;
                        }

                        byte[] buffer = new byte[4096];
                        using (Stream zipStream = zf.GetInputStream(zipEntry))
                        {
                            using (FileStream streamWriter = File.Create(Path.Combine(pathToTempFolder, fileName)))
                            {
                                StreamUtils.Copy(zipStream, streamWriter, buffer);
                            }
                        }
                    }

                    if (zf != null)
                    {
                        zf.IsStreamOwner = true;
                        zf.Close();
                    }
                }
            }
        }

        private static void ChangePackageName()
        {
            string manifestPath = Path.Combine(pathToTempFolder, MANIFEST_NAME);
            string manifestText = File.ReadAllText(manifestPath);

            int manifestPackageNameStartIndex = manifestText.IndexOf("package=\"") + 9;
            int manifestPackageNameEndIndex = manifestText.IndexOf("\">", manifestPackageNameStartIndex);
            string manifestPackageName = manifestText.Substring(manifestPackageNameStartIndex, manifestPackageNameEndIndex - manifestPackageNameStartIndex);

            manifestText = manifestText.Replace("package=\"" + manifestPackageName, "package=\"" + PlayerSettings.applicationIdentifier);
            manifestText = manifestText.Replace("android:authorities=\"" + manifestPackageName, "android:authorities=\"" + PlayerSettings.applicationIdentifier);
            File.WriteAllText(manifestPath, manifestText);

            string filepathsPath = Path.Combine(pathToTempFolder, PROVIDER_PATHS_NAME);
            string filepathsText = File.ReadAllText(filepathsPath);

            int filepathsPackageNameStartIndex = filepathsText.IndexOf("data/") + 5;
            int filepathsPackageNameEndIndex = filepathsText.IndexOf("\" name", filepathsPackageNameStartIndex);
            string filepathsPackageName = filepathsText.Substring(filepathsPackageNameStartIndex, filepathsPackageNameEndIndex - filepathsPackageNameStartIndex);

            filepathsText = filepathsText.Replace("data/" + filepathsPackageName, "data/" + PlayerSettings.applicationIdentifier);
            File.WriteAllText(filepathsPath, filepathsText);

            lastPackageName = PlayerSettings.applicationIdentifier;
        }

        private static void ZippingBinary() // используется ДОзапись, обычным методом .Add(filePath, entryName), для перезаписи с нуля нужно использовать ZipOutputStream zipToWrite = new ZipOutputStream(zipStream) и FileStream targetFile
        {
            if (!File.Exists(pathToBinary))
            {
                throw new Exception("File release.aar not found. Please reimport asset. See README.md for details...");
            }

            if (!Directory.Exists(pathToTempFolder))
            {
                throw new Exception("Temp folder not found. See README.pdf for details...");
            }

            using (FileStream zipStream = new FileStream(pathToBinary, FileMode.Open))
            {
                using (ZipFile zipFile = new ZipFile(zipStream))
                {
                    zipFile.BeginUpdate();
                    zipFile.Add(Path.Combine(pathToTempFolder, MANIFEST_NAME), MANIFEST_NAME);
                    zipFile.Add(Path.Combine(pathToTempFolder, PROVIDER_PATHS_NAME), PROVIDER_PATHS_NAME);
                    zipFile.CommitUpdate();
                }
            }
        }

        private static bool IsFileAlreadyOpen(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString() + ": " + e.Message);
                stopedByError = true;
                return true;
            }

            return false;
        }
    }
}
#endif
