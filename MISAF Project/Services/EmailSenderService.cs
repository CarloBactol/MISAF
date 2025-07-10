using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Web;
using MISAF_Project.EDMX;
using MISAF_Project.ViewModels;
using Newtonsoft.Json;
using System.Web.Mvc;
using MISAF_Project.DTO;

namespace MISAF_Project.Services
{
    public class EmailSenderService : IEmailSenderService
    {
        private const string V = "N/A";
        private readonly string _fromEmail;
        private readonly string _toEmail;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly bool _enableSsl;
        private readonly string _smtpUserName;
        private readonly string _smtpPassword;

        public EmailSenderService()
        {
            _fromEmail = System.Configuration.ConfigurationManager.AppSettings["SmtpFromEmail"] ?? "noreply@filpet.com.ph";
            _toEmail = System.Configuration.ConfigurationManager.AppSettings["SmtpToEmail"] ?? "west-carlo_b@manlyplastics.com,west-edwin_l@manlyplastics.com";
            _smtpHost = System.Configuration.ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(System.Configuration.ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
            _enableSsl = bool.Parse(System.Configuration.ConfigurationManager.AppSettings["SmtpEnableSsl"] ?? "true");
            _smtpUserName = System.Configuration.ConfigurationManager.AppSettings["SmtpUserName"] ?? "noreply@filpet.com.ph";
            _smtpPassword = System.Configuration.ConfigurationManager.AppSettings["SmtpPassword"] ?? "uybc asrv kqwn nrhn";
        }

        public void SendErrorEmail(string errorMessage, string additionalDetails)
        {
            try
            {
                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(_fromEmail, "MISAF Application");
                    mailMessage.To.Add(_toEmail);
                    mailMessage.Subject = "MISAF Application Error - Urgent";
                    mailMessage.Body = BuildEmailBody(errorMessage, additionalDetails);
                    mailMessage.IsBodyHtml = true;

                    using (var smtpClient = new SmtpClient(_smtpHost, _smtpPort))
                    {
                        smtpClient.EnableSsl = _enableSsl;
                        smtpClient.Credentials = new System.Net.NetworkCredential(_smtpUserName, _smtpPassword);
                        smtpClient.Send(mailMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to send email: {ex.Message}");
            }
        }

        public void SendEmail(string emailApprover, string subject, MAFMainDto main, List<MAF_Detail> details, List<MAF_Attachment> attachments, string MapPath, bool IsAll, string options)
        {
            try
            {
                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(_fromEmail, "MISAF Application");
                    mailMessage.To.Add(emailApprover);
                    mailMessage.Subject = subject;
                    if(main.Status == "For Acknowledgement MIS" || main.Status == "Rejected" || main.Status == "Approved" || main.Status == "Done" || main.Status == "On Hold")
                    {
                        if (IsAll)
                        {
                            mailMessage.Body = NotifyUsersAllRequest(main, details, options);
                        }
                        else
                        {
                            mailMessage.Body = NotifyUsersPerDetails(main, details, options);
                        }
                       
                    }
                    mailMessage.IsBodyHtml = true;

                    // Add attachments
                    foreach (var attachment in attachments)
                    {
                        // Option 1: If attachments are stored as file paths
                        var filePath = $"{MapPath}/{attachment.MAF_No}-{attachment.Record_ID}-{attachment.Filename}";
                        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                        {
                            var mailAttachment = new System.Net.Mail.Attachment(filePath);
                            mailAttachment.Name = attachment.Filename; // Ensure the file name is set
                            mailMessage.Attachments.Add(mailAttachment);
                        }
                        // Option 2: If attachments are stored as binary data (byte[])
                        //else if (attachment.FileData != null && attachment.FileData.Length > 0)
                        //{
                        //    var stream = new MemoryStream(attachment.FileData);
                        //    var mailAttachment = new  System.Net.Mail.Attachment(stream, attachment.Filename, GetMimeType(attachment.Filename));
                        //    mailMessage.Attachments.Add(mailAttachment);
                        //}
                    }

                    using (var smtpClient = new SmtpClient(_smtpHost, _smtpPort))
                    {
                        smtpClient.EnableSsl = _enableSsl;
                        smtpClient.Credentials = new System.Net.NetworkCredential(_smtpUserName, _smtpPassword);
                        smtpClient.Send(mailMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to send email: {ex.Message}");
            }
        }

        // Helper method to determine MIME type based on file extension
        //private string GetMimeType(string filename)
        //{
        //    var extension = Path.GetExtension(filename).ToLowerInvariant();
        //    return extension switch
        //    {
        //        ".pdf" => "application/pdf",
        //        ".jpg" => "image/jpeg",
        //        ".jpeg" => "image/jpeg",
        //        ".png" => "image/png",
        //        ".gif" => "image/gif",
        //        _ => "application/octet-stream" // Default MIME type for unknown files
        //    };
        //}



        private string BuildEmailBody(string errorMessage, string additionalDetails)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<h2>MISAF Application Error Report</h2>");
            sb.AppendLine("<p>An error occurred in the MISAF application. Please investigate and resolve the issue.</p>");
            sb.AppendLine("<h3>Error Details</h3>");
            sb.AppendLine($"<p><strong>Error Message:</strong> {errorMessage}</p>");

            // Format Additional Details with line breaks
            sb.AppendLine("<h3>Additional Details</h3>");
            if (!string.IsNullOrEmpty(additionalDetails))
            {
                // Split the additional details into parts (e.g., Input Data and Stack Trace)
                var parts = additionalDetails.Split(new[] { "Stack Trace:" }, StringSplitOptions.None);
                if (parts.Length > 0)
                {
                    // Handle Input Data
                    string inputDataPart = parts[0].Trim();
                    if (inputDataPart.StartsWith("Input Data:"))
                    {
                        // Extract JSON and format it
                        var jsonStartIndex = inputDataPart.IndexOf("{");
                        if (jsonStartIndex >= 0)
                        {
                            var jsonString = inputDataPart.Substring(jsonStartIndex);
                            try
                            {
                                // Pretty-print the JSON
                                var jsonObject = JsonConvert.DeserializeObject(jsonString);
                                var formattedJson = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
                                sb.AppendLine("<p><strong>Input Data:</strong></p>");
                                sb.AppendLine($"<pre>{formattedJson}</pre>");
                            }
                            catch
                            {
                                // If JSON parsing fails, fall back to raw string with line breaks
                                sb.AppendLine("<p><strong>Input Data:</strong></p>");
                                sb.AppendLine($"<pre>{inputDataPart.Replace("\n", "<br/>")}</pre>");
                            }
                        }
                        else
                        {
                            sb.AppendLine($"<p><strong>Input Data:</strong> {inputDataPart.Replace("\n", "<br/>")}</p>");
                        }
                    }
                    else
                    {
                        sb.AppendLine($"<p>{inputDataPart.Replace("\n", "<br/>")}</p>");
                    }
                }

                // Handle Stack Trace
                if (parts.Length > 1)
                {
                    string stackTrace = parts[1].Trim();
                    sb.AppendLine("<p><strong>Stack Trace:</strong></p>");
                    sb.AppendLine($"<pre>{stackTrace.Replace("\n", "<br/>")}</pre>");
                }
            }
            else
            {
                sb.AppendLine("<p>No additional details available.</p>");
            }

            sb.AppendLine("<h3>Environment</h3>");
            sb.AppendLine($"<p><strong>Timestamp:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
            sb.AppendLine($"<p><strong>Application:</strong> ASP.NET MVC 5</p>");
            //sb.AppendLine("<h3>Next Steps</h3>");
            //sb.AppendLine("<p>Please review the error and take appropriate action. Contact the developer if further assistance is needed.</p>");
            return sb.ToString();
        }

        private string NotifyUsersAllRequest(MAFMainDto main, List<MAF_Detail> details, string options)
        {
            string dateEndorse;
            string endorserStatus;
            if (main.Endorsed_Status == "Y")
            {
                endorserStatus = "APPROVED";
                dateEndorse = main.DateTime_Endorsed.ToString();
            }
            else
            {
                endorserStatus = V;
                dateEndorse = V;
            }

            string approverStatus = main.Status == "Rejected" ? "REJECTED" : "APPROVED";

            StringBuilder emailContent = new StringBuilder();
            emailContent.AppendLine($"<h3>This is to notify you that this request is already [{main.Status}].</h3>");
            emailContent.AppendLine($"<p><strong>Ticket Number:</strong> <span style='text-decoration: underline;'>{main.MAF_No}</span></p>");
            emailContent.AppendLine($"<p><strong>Date and Time Requested:</strong> <span style='text-decoration: underline;'>{main.DateTime_Requested}</span></p>");
            emailContent.AppendLine($"<p><strong>Pre-Approved:</strong> <span style='text-decoration: underline;'>{main.Pre_Approved}</span></p>");
            emailContent.AppendLine($"<p><strong>Requested By:</strong> <span style='text-decoration: underline;'>{main.Requestor_Name}</span></p>");
            emailContent.AppendLine("<hr>");

            if(options == "Endorser")
            {
                if(main.Endorsed_Status == "N")
                {
                    endorserStatus = approverStatus;
                    dateEndorse = main.DateTime_Endorsed.ToString();
                }
                emailContent.AppendLine($"<p><strong>Endorsed / Acknowledged By:</strong> <span style='text-decoration: underline;'>{main.Endorsed_By}</span></p>");
                emailContent.AppendLine($"<p><strong>Status:</strong> <span style='text-decoration: underline;'>{endorserStatus}</span></p>");
                emailContent.AppendLine($"<p><strong>Date and Time:</strong> <span style='text-decoration: underline;'>{dateEndorse}</span></p>");
                emailContent.AppendLine($"<p><strong>Remarks:</strong> <span style='text-decoration: underline;'>{main.Endorser_Remarks}</span></p>");
                emailContent.AppendLine("<hr>");
            }

           if(options == "Approver")
            {
                emailContent.AppendLine($"<p><strong>Approver:</strong> <span style='text-decoration: underline;'>{main.Final_Approver}</span></p>");
                emailContent.AppendLine($"<p><strong>Status:</strong> <span style='text-decoration: underline;'>{approverStatus}</span></p>");
                emailContent.AppendLine($"<p><strong>Date and Time:</strong> <span style='text-decoration: underline;'>{main.DateTime_Approved}</span></p>");
                emailContent.AppendLine($"<p><strong>Remarks:</strong> <span style='text-decoration: underline;'>{main.Final_Approver_Remarks}</span></p>");
                emailContent.AppendLine("<hr>");
            }

            if (options == "Acknowledge")
            {
                emailContent.AppendLine($"<p><strong>MIS Personel:</strong> <span style='text-decoration: underline;'>{main.Status_Updated_By}</span></p>");
                emailContent.AppendLine($"<p><strong>Status:</strong> <span style='text-decoration: underline;'>{main.Status}</span></p>");
                emailContent.AppendLine($"<p><strong>Date and Time:</strong> <span style='text-decoration: underline;'>{main.Status_DateTime}</span></p>");
                emailContent.AppendLine($"<p><strong>Remarks:</strong> <span style='text-decoration: underline;'>{main.Status_Remarks}</span></p>");
                emailContent.AppendLine("<hr>");
            }

            emailContent.AppendLine("<h4>Request Details</h4>");
            emailContent.AppendLine("<table border='1' style='border-collapse: collapse; width: 100%;'>");
            emailContent.AppendLine("<thead>");
            emailContent.AppendLine("<tr style='background-color: #f2f2f2;'>");
            emailContent.AppendLine("<th style='padding: 8px;'>Category</th>");
            emailContent.AppendLine("<th style='padding: 8px;'>Request/Problem Recommendation</th>");
            emailContent.AppendLine("<th style='padding: 8px;'>Reason</th>");
            emailContent.AppendLine("<th style='padding: 8px;'>Status</th>");
            emailContent.AppendLine("<th style='padding: 8px;'>Status Date</th>");
            emailContent.AppendLine("<th style='padding: 8px;'>Remarks</th>");
            emailContent.AppendLine("<th style='padding: 8px;'>Status Updated By</th>");
            emailContent.AppendLine("</tr>");
            emailContent.AppendLine("</thead>");
            emailContent.AppendLine("<tbody>");

            foreach (var item in details)
            {
                var rowClass = "";
                if (item.Status == "Approved" || item.Status == "Done")
                {
                    rowClass += "style='background-color: #d1e7dd; color: #0f5132; border-color: #a3cfbb;'";
                }
                else if (item.Status == "Disapproved" || item.Status == "Rejected")
                {
                    rowClass += "style='background-color: #f8d7da; color: #842029; border-color: #f5c2c7;'";
                }
                else if (item.Status == "On Going" || item.Status == "For Acknowledgement MIS")
                {
                    rowClass += "style='background-color: #fff3cd; color: #664d03; border-color: #ffec99;'";
                }
                else if (item.Status == "On Hold")
                {
                    rowClass += "style='background-color: #cff4fc; color: #055160; border-color: #9eeaf9;'";
                }

                emailContent.AppendLine($"<tr {rowClass}>");
                emailContent.AppendLine("<td style='padding: 8px;'>" + item.Category + "</td>");
                emailContent.AppendLine("<td style='padding: 8px;'>" + item.Request + "</td>");
                emailContent.AppendLine("<td style='padding: 8px;'>" + item.Reason + "</td>");
                emailContent.AppendLine("<td style='padding: 8px;'>" + item.Status + "</td>");
                emailContent.AppendLine("<td style='padding: 8px;'>" + item.Status_DateTime + "</td>");
                emailContent.AppendLine("<td style='padding: 8px;'>" + item.Status_Remarks + "</td>");
                emailContent.AppendLine("<td style='padding: 8px;'>" + item.Status_Updated_By + "</td>");
                emailContent.AppendLine("</tr>");
            }
            emailContent.AppendLine("</tbody>");
            emailContent.AppendLine("</table>");
            return emailContent.ToString();
        }


        private string NotifyUsersPerDetails(MAFMainDto main, List<MAF_Detail> details, string options)
        {
            string dateEndorse;
            string endorserStatus;
            if (main.Endorsed_Status == "Y")
            {
                endorserStatus = "APPROVED";
                dateEndorse = main.DateTime_Endorsed.ToString();
            }
            else
            {
                endorserStatus = V;
                dateEndorse = V;
            }

            string approverStatus = main.Status == "Rejected" ? "REJECTED" : "APPROVED";

            StringBuilder emailContent = new StringBuilder();
            emailContent.AppendLine($"<h3>This is to notify you that this request is already [{main.Status}].</h3>");
            emailContent.AppendLine($"<p><strong>Ticket Number:</strong> <span style='text-decoration: underline;'>{main.MAF_No}</span></p>");
            emailContent.AppendLine($"<p><strong>Date and Time Requested:</strong> <span style='text-decoration: underline;'>{main.DateTime_Requested}</span></p>");
            emailContent.AppendLine($"<p><strong>Pre-Approved:</strong> <span style='text-decoration: underline;'>{main.Pre_Approved}</span></p>");
            emailContent.AppendLine($"<p><strong>Requested By:</strong> <span style='text-decoration: underline;'>{main.Requestor_Name}</span></p>");
            emailContent.AppendLine("<hr>");

            if (options == "Endorser")
            {
                if (main.Endorsed_Status == "N")
                {
                    endorserStatus = approverStatus;
                    dateEndorse = main.DateTime_Endorsed.ToString();
                }
                emailContent.AppendLine($"<p><strong>Endorsed / Acknowledged By:</strong> <span style='text-decoration: underline;'>{main.Endorsed_By}</span></p>");
                emailContent.AppendLine($"<p><strong>Status:</strong> <span style='text-decoration: underline;'>{endorserStatus}</span></p>");
                emailContent.AppendLine($"<p><strong>Date and Time:</strong> <span style='text-decoration: underline;'>{dateEndorse}</span></p>");
                emailContent.AppendLine($"<p><strong>Remarks:</strong> <span style='text-decoration: underline;'>{main.Endorser_Remarks}</span></p>");
                emailContent.AppendLine("<hr>");
            }

            if (options == "Approver")
            {
                emailContent.AppendLine($"<p><strong>Approver:</strong> <span style='text-decoration: underline;'>{main.Final_Approver}</span></p>");
                emailContent.AppendLine($"<p><strong>Status:</strong> <span style='text-decoration: underline;'>{approverStatus}</span></p>");
                emailContent.AppendLine($"<p><strong>Date and Time:</strong> <span style='text-decoration: underline;'>{main.DateTime_Approved}</span></p>");
                emailContent.AppendLine($"<p><strong>Approver:</strong> <span style='text-decoration: underline;'>{main.Final_Approver}</span></p>");
                emailContent.AppendLine($"<p><strong>Remarks:</strong> <span style='text-decoration: underline;'>{main.Final_Approver_Remarks}</span></p>");
                emailContent.AppendLine("<hr>");
            }

            emailContent.AppendLine("<h4>Request Details</h4>");
            emailContent.AppendLine("<table border='1' style='border-collapse: collapse; width: 100%;'>");
            emailContent.AppendLine("<thead>");
            emailContent.AppendLine("<tr style='background-color: #f2f2f2;'>");
            emailContent.AppendLine("<th style='padding: 8px;'>Category</th>");
            emailContent.AppendLine("<th style='padding: 8px;'>Request/Problem Recommendation</th>");
            emailContent.AppendLine("<th style='padding: 8px;'>Reason</th>");
            emailContent.AppendLine("<th style='padding: 8px;'>Status</th>");
            emailContent.AppendLine("<th style='padding: 8px;'>Status Date</th>");
            emailContent.AppendLine("<th style='padding: 8px;'>Remarks</th>");
            emailContent.AppendLine("<th style='padding: 8px;'>Status Updated By</th>");
            emailContent.AppendLine("</tr>");
            emailContent.AppendLine("</thead>");
            emailContent.AppendLine("<tbody>");

            foreach (var item in details)
            {
                var rowClass = "";
                if (item.Status == "Approved" || item.Status == "Done")
                {
                    rowClass += "style='background-color: #d1e7dd; color: #0f5132; border-color: #a3cfbb;'";
                }
                else if (item.Status == "Disapproved" || item.Status == "Rejected")
                {
                    rowClass += "style='background-color: #f8d7da; color: #842029; border-color: #f5c2c7;'";
                }
                else if (item.Status == "On Going" || item.Status == "For Acknowledgement MIS")
                {
                    rowClass += "style='background-color: #fff3cd; color: #664d03; border-color: #ffec99;'";
                }
                else if (item.Status == "On Hold")
                {
                    rowClass += "style='background-color: #cff4fc; color: #055160; border-color: #9eeaf9;'";
                }

                emailContent.AppendLine($"<tr {rowClass}>");
                emailContent.AppendLine("<td style='padding: 8px;'>" + item.Category + "</td>");
                emailContent.AppendLine("<td style='padding: 8px;'>" + item.Request + "</td>");
                emailContent.AppendLine("<td style='padding: 8px;'>" + item.Reason + "</td>");
                emailContent.AppendLine("<td style='padding: 8px;'>" + item.Status + "</td>");
                emailContent.AppendLine("<td style='padding: 8px;'>" + item.Status_DateTime + "</td>");
                emailContent.AppendLine("<td style='padding: 8px;'>" + item.Status_Remarks + "</td>");
                emailContent.AppendLine("<td style='padding: 8px;'>" + item.Status_Updated_By + "</td>");
                emailContent.AppendLine("</tr>");
            }
            emailContent.AppendLine("</tbody>");
            emailContent.AppendLine("</table>");
            return emailContent.ToString();
        }
    }
}