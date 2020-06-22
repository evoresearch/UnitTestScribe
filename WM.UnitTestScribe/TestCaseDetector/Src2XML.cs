﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WM.UnitTestScribe.TestCaseDetector {
    public class Src2XML {

        public void SourceFolderToXml(String sourceLoc, string tempDir, String srcmlExe) {
            srcmlExe = srcmlExe.EndsWith("\\")? srcmlExe + "src2srcml.exe": srcmlExe + "\\src2srcml.exe";
            //foreach (var fileName in Directory.GetFiles(tempDir)) {
            //    if (fileName.EndsWith(".cs")) {
            //        //Console.WriteLine(fileName);
            //        SourceFileToXml(fileName, tempDir +
            //                @"\" + Util.RandomString(5) + fileName.Substring(fileName.LastIndexOf('\\') + 1) + ".xml", srcmlExe);
            //    }
            //}
            //foreach (string dirFile in Directory.GetDirectories(sourceLoc)) {
            //    foreach (string fileName in Directory.GetFiles(dirFile)) {
            //        // fileName  is the file name
            //        if (fileName.EndsWith(".cs")) {
            //            //Console.WriteLine(fileName);
            //            SourceFileToXml(fileName, tempDir +
            //                    @"\" + Util.RandomString(5) + fileName.Substring(fileName.LastIndexOf('\\') + 1) + ".xml", srcmlExe);
            //        }
            //    }
            //}
            
            string[] alloweExtensions = ConfigurationManager.AppSettings["allowedFileExtensions"].ToString().Split(',');

            foreach (string file in Directory.EnumerateFiles(sourceLoc, "*.*", SearchOption.AllDirectories))
            {
                if (alloweExtensions.Contains(Path.GetExtension(file)))
                {
                    //Console.WriteLine(fileName);
                    SourceFileToXml(file, tempDir +
                            // @"\" + Util.RandomString(5) + file.Substring(file.LastIndexOf('\\') + 1) + ".xml", srcmlExe);
                            @"\" + Path.GetFileName(file) + ".xml", srcmlExe);
                }
            }
        }


     
        public void SourceFileToXml(String source, String xMLOutput, String srcmlExe) {
            string language = ConfigurationManager.AppSettings["projectLanguage"];
            //Console.WriteLine("source : " + source);
            //Console.WriteLine("output : " + xMLOutput);
            String cmd = srcmlExe + " " + string.Format(" --language={0} ",language) + source + " -o " + xMLOutput;

            Process myProcess = new Process();

            try {
                myProcess.StartInfo.UseShellExecute = false;
                // You can start any process, HelloWorld is a do-nothing example.
                myProcess.StartInfo.FileName = srcmlExe;
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.StartInfo.Arguments = string.Format(" --language={0} ", language) + source + " -o " + xMLOutput;
                myProcess.Start();
                // This code assumes the process you are starting will terminate itself.
                // Given that is is started without a window so you cannot terminate it
                // on the desktop, it must terminate itself or you can do it programmatically
                // from this application using the Kill method.
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
    }
}
