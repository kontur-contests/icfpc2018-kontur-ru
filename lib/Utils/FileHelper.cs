using System;
using System.IO;

namespace lib.Utils
{
    public static class FileHelper
    {
        public static string DataDir => PatchDirectoryName("data");
        public static string ProblemsDir => Path.Combine(DataDir, "problemsF");
        public static string SolutionsDir => Path.Combine(DataDir, "solutions");
        public static string DefaultTracesDir => Path.Combine(DataDir, "dfltTracesF");

        public static string PatchFilename(string filename, string baseDirectoryPath = null)
        {
            return Path.GetFullPath(Path.IsPathRooted(filename) ? filename : WalkDirectoryTree(filename, File.Exists, baseDirectoryPath));
        }

        public static string PatchDirectoryName(string dirName, string baseDirectoryPath = null)
        {
            return Path.GetFullPath(Path.IsPathRooted(dirName) ? dirName : WalkDirectoryTree(dirName, Directory.Exists, baseDirectoryPath));
        }

        private static string WalkDirectoryTree(string filename, Func<string, bool> fileSystemObjectExists, string baseDirectoryPath = null)
        {
            if (baseDirectoryPath == null)
                baseDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;
            var baseDirectory = new DirectoryInfo(baseDirectoryPath);
            while (baseDirectory != null)
            {
                var candidateFilename = Path.Combine(baseDirectory.FullName, filename);
                if (fileSystemObjectExists(candidateFilename))
                    return candidateFilename;
                baseDirectory = baseDirectory.Parent;
            }
            return filename;
        }
    }
}