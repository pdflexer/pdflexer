using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLexer.Tests
{
    public static class PathUtil
    {
        public static string GetPathFromSegmentOfCurrent(string segment)
        {
            return GetPathFromSegment(segment, Environment.CurrentDirectory);
        }
        public static string GetPathFromSegment(string segment, string path)
        {
            var split = path.Split(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            int index = Array.FindLastIndex(split, t => t.Equals(segment, StringComparison.InvariantCultureIgnoreCase));
            if (index == -1)
            {
                throw new FileNotFoundException("Folder to set relative to not found");
            }
            return string.Join(Path.DirectorySeparatorChar.ToString(), split.Take(index + 1));

        }
    }

    public class TemporaryWorkspace : IDisposable
    {
        private string basePath;
        private string copyBasePath;
        public string TempPath { get; set; }

        public TemporaryWorkspace() : this(Environment.CurrentDirectory)
        {
        }

        public TemporaryWorkspace(string basePath)
        {
            this.basePath = basePath;
            TempPath = Path.Combine(basePath, "__temp_" + Path.GetRandomFileName());
            Directory.CreateDirectory(TempPath);
        }

        public TemporaryWorkspace WithCopiesRelativeToAssembly()
        {
            //copyBasePath = PathUtil.GetPathFromSegmentOfCurrent(pathSegment);
            copyBasePath = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location))
                .LocalPath;
            return this;
        }

        public TemporaryWorkspace WithCopiesRelativeTo(string pathSegment)
        {
            //copyBasePath = PathUtil.GetPathFromSegmentOfCurrent(pathSegment);
            copyBasePath = PathUtil.GetPathFromSegment(pathSegment,
                new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location)).LocalPath);
            return this;
        }

        public string AddToTempFolder(string sourcePath, bool sourceRelative, string relativeDestPath = "")
        {
            if (sourceRelative)
                sourcePath = Path.GetFullPath(Path.Combine(copyBasePath, sourcePath));

            if (Directory.Exists(sourcePath))
            {
                string destDir = "";
                if (string.IsNullOrEmpty(relativeDestPath))
                    destDir = TempPath;
                else
                {
                    destDir = Path.GetFullPath(Path.Combine(TempPath, relativeDestPath));
                    Directory.CreateDirectory(destDir);
                }

                foreach (var file in Directory.EnumerateFiles(sourcePath))
                {
                    string fileName = Path.GetFileName(file);

                    string destFile = Path.Combine(destDir, fileName);
                    File.Copy(file, destFile);
                }

                return destDir;
            }
            else if (File.Exists(sourcePath))
            {
                var destDir = TempPath;
                if (!string.IsNullOrEmpty(relativeDestPath))
                {
                    destDir = Path.GetFullPath(Path.Combine(TempPath, relativeDestPath));
                    Directory.CreateDirectory(destDir);
                }

                string dest = Path.Combine(destDir, Path.GetFileName(sourcePath));
                File.Copy(sourcePath, dest);
                return dest;
            }
            else
            {
                throw new FileNotFoundException("Path not found to be copied.");
            }
        }

        public string AddFileWithContents(string fileName, string contents)
        {
            string file = Path.GetFullPath(Path.Combine(TempPath, fileName));
            Directory.CreateDirectory(Path.GetDirectoryName(file));
            File.WriteAllText(file, contents);
            return file;
        }

        public void Dispose()
        {
            Directory.SetCurrentDirectory(basePath);
            Directory.Delete(TempPath, true);
        }
    }
}
