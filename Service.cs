using System;
using System.IO;
using System.Linq;
using System.Timers;

namespace SyncDirectoryService
{
    public class Service
    {
        private readonly Timer timer;

        public Service()
        {
            timer = new Timer(10000) { AutoReset = true };
            timer.Elapsed += TimerElapsed;
        }

        public void Start()
        {
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }

        private void TimerElapsed(object sender, EventArgs e)
        {
            var sourceParent = @""; // Specify source directory here!
            var targetParent = @""; // Specify target directory here!
            SyncDirectory(sourceParent, targetParent);
        }

        private static void SyncDirectory(string sourceParentDir, string targetParentDir)
        {
            SyncFiles(sourceParentDir, targetParentDir);

            // Get source directories
            var sourceSubDirs = Directory.GetDirectories(sourceParentDir);
            var sourceDirNames = sourceSubDirs.Select(x => x.Substring(sourceParentDir.Length + 1));

            // Get all target directories
            var targetSubDirs = Directory.GetDirectories(targetParentDir);
            var targetDirNames = targetSubDirs.Select(x => x.Substring(targetParentDir.Length + 1)).ToList();

            foreach (var dirName in sourceDirNames)
            {
                var dirExist = Directory.Exists(Path.Combine(targetParentDir, dirName)); //Kör contains mot targetDirNames
                if (!dirExist)
                {
                    Directory.CreateDirectory(Path.Combine(targetParentDir, dirName));
                    SyncDirectory(Path.Combine(sourceParentDir, dirName), Path.Combine(targetParentDir, dirName));
                }
                else
                {
                    targetDirNames.Remove(dirName);
                    SyncDirectory(Path.Combine(sourceParentDir, dirName), Path.Combine(targetParentDir, dirName));
                }
            }

            if (targetDirNames.Any())
            {
                foreach (var dirName in targetDirNames)
                {
                    Directory.Delete(Path.Combine(targetParentDir, dirName), true);
                }
            }
        }

        private static void SyncFiles(string sourceParentDir, string targetParentDir)
        {
            // Get source file paths
            var sourceFilePaths = Directory.GetFiles(sourceParentDir);
            var sourceFileNames = sourceFilePaths.Select(x => x.Substring(sourceParentDir.Length + 1)).ToList();

            // Get target file names
            var targetFilePaths = Directory.GetFiles(targetParentDir);
            var targetFileNames = targetFilePaths.Select(x => x.Substring(targetParentDir.Length + 1)).ToList();

            foreach (var fileName in sourceFileNames)
            {
                //var fileName = filePath.Substring(sourceParentDir.Length + 1);
                var fileExist = File.Exists(Path.Combine(targetParentDir, fileName));
                if (!fileExist)
                {
                    File.Copy(Path.Combine(sourceParentDir, fileName), Path.Combine(targetParentDir, fileName));
                }
                else
                {
                    targetFileNames.Remove(fileName);
                    var sourceFileDate = File.GetLastWriteTime(Path.Combine(sourceParentDir, fileName));
                    var targetFileDate = File.GetLastWriteTime(Path.Combine(targetParentDir, fileName));
                    if (sourceFileDate > targetFileDate)
                    {
                        File.Copy(Path.Combine(sourceParentDir, fileName), Path.Combine(targetParentDir, fileName), true);
                    }
                }
            }

            if (targetFileNames.Any())
            {
                foreach (var fileName in targetFileNames)
                {
                    File.Delete(Path.Combine(targetParentDir, fileName));
                }
            }
        }
    }
}
