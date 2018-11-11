/*using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageSorter.Helpers
{
    public class DelectionHelper_Concept
    {
        public ConcurrentQueue<FileInfo> FilesToBeDeleted { get; } = new ConcurrentQueue<FileInfo>();

        private readonly Task _delTask;
        public bool ShouldWork = false;

        #region Singleton

        private static DelectionHelper_Concept _instance;
        public static DelectionHelper_Concept Instance => _instance ?? (_instance = new DelectionHelper_Concept());

        private DelectionHelper_Concept()
        {
            _delTask = new Task(Work);
            _delTask.Start();
        }

        #endregion Singleton

        #region Task

        private void Work()
        {
            Console.WriteLine("DeletionHelper - Work Start");
            while (ShouldWork)
            {
                if (FilesToBeDeleted.IsEmpty) continue;

                FilesToBeDeleted.TryDequeue(out var fileInfo);
                if (fileInfo != null)
                {
                    DeleteFile(fileInfo);
                }
            }
            Console.WriteLine("DeletionHelper - Work Exit");
        }

        private void DeleteFile(FileInfo fi)
        {
            try
            {
                Console.WriteLine("DeletionHelper - Deleting: " + fi.FullName);
                fi.Delete();
            }
            catch (Exception ex)
            {
                Console.WriteLine("DeletionHelper - Error while deleting: " + ex.Message);
            }
        }

        #endregion Task
    }
}*/