using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ImageSorter.Helpers
{
    public static class DeletionExtensions
    {
        public static int DeleteAllFiles(this List<FileInfo> collection)
        {
            var deletedItemsCount = 0;
            foreach (var v in collection.AsParallel())
            {
                try
                {
                    v.Delete();
                    deletedItemsCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DeleteFiles - Could not delete [{v.Name}] -> {ex.Message}");
                }
            }

            return deletedItemsCount;
        }
    }
}