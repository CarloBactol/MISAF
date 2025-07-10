using FluentValidation.Mvc;
using MISAF_Project.EDMX;
using MISAF_Project.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace MISAF_Project
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information() // Log Information and above (Info, Warning, Error, Fatal)
                .Enrich.FromLogContext() // Add contextual information (e.g., thread ID)
                .WriteTo.Async(a => // Use async logging for better performance
                    a.File(
                        path: Server.MapPath("~/App_Data/ErrorLogs/ErrorLog.txt"),
                        rollingInterval: RollingInterval.Day, // Create a new file each day (e.g., ErrorLog-20250428.txt)
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                        fileSizeLimitBytes: 10 * 1024 * 1024, // 10 MB file size limit
                        retainedFileCountLimit: 31 // Keep logs for 31 days
                    ))
                .CreateLogger();

            // Log application startup
            Log.Information("Application started");


            // Register FluentValidation
            FluentValidationModelValidatorProvider.Configure();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            FileHelper.CleanupTempUploads();
            UnityConfig.RegisterComponents(); // for DI
            
        }

        protected void Application_End()
        {
            // Log application shutdown
            Log.Information("Application shutting down");

            // Close and flush Serilog
            Log.CloseAndFlush();
        }
    }
}
