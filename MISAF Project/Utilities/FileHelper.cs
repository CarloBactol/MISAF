using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace MISAF_Project.Utilities
{
    public static class FileHelper
    {
        public static void CleanupTempUploads()
        {
            var tempPath = HttpContext.Current.Server.MapPath("~/App_Data/TempUploads/");
            if (!Directory.Exists(tempPath)) return;

            var files = Directory.GetFiles(tempPath);
            foreach (var file in files)
            {
                var creationTime = System.IO.File.GetCreationTime(file);
                if ((DateTime.Now - creationTime).TotalSeconds > 5)
                {
                    System.IO.File.Delete(file);
                }
            }
        }

    }
}