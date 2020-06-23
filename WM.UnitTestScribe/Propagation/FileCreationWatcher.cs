﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace TeaCap.Propagation
{
   public class FileCreationWatcher
    {
        public static int fileCount;
        public static string folder;
        public static int filesExpected;

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void Run()
        {
            
            // Create a new FileSystemWatcher and set its properties.
            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.Path = folder;

                // Watch for changes in LastAccess and LastWrite times, and
                // the renaming of files or directories.
                watcher.NotifyFilter = //NotifyFilters.LastAccess
                                     NotifyFilters.LastWrite
                                     | NotifyFilters.FileName
                                     | NotifyFilters.DirectoryName;

                // Only watch text files.
                watcher.Filter = "*.xml";

                // Add event handlers.
               // watcher.Changed += OnChanged;
                watcher.Created += OnChanged;
                //watcher.Deleted += OnChanged;
                //watcher.Renamed += OnRenamed;

                // Begin watching.
                watcher.EnableRaisingEvents = true;

                // Wait for the user to quit the program.
                
                while (fileCount<filesExpected) ;
                Console.WriteLine();
            }
        }

        // Define the event handlers.
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            //Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");
            Console.Write(".");
            fileCount++;
        }

        private static void OnRenamed(object source, RenamedEventArgs e) =>
            // Specify what is done when a file is renamed.
            Console.WriteLine($"File: {e.OldFullPath} renamed to {e.FullPath}");
    }
}