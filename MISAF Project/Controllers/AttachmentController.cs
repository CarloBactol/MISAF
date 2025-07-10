using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MISAF_Project.Controllers
{
    public class AttachmentController : BaseController
    {
        public ActionResult GetAttachment(string fileName)
        {
            // Validate fileName to prevent directory traversal
            if (string.IsNullOrEmpty(fileName) || fileName.Contains("..") || fileName.Contains("/"))
            {
                return HttpNotFound();
            }

            // Construct the file path
            string filePath = Server.MapPath("~/App_Data/Attachments/" + fileName);

            // Check if the file exists
            if (!System.IO.File.Exists(filePath))
            {
                return HttpNotFound();
            }

            // Determine the content type based on file extension
            string contentType = MimeMapping.GetMimeMapping(fileName);

            // Serve the file
            return File(filePath, contentType, fileName);
        }
    }
}