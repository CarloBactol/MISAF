using FluentValidation;
using MISAF_Project.EDMX;
using MISAF_Project.FluentValidations;
using MISAF_Project.Queue;
using MISAF_Project.Repositories;
using MISAF_Project.Services;
using System.Web.Mvc;
using Unity;
using Unity.Mvc5;

namespace MISAF_Project
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();


            // === ADDED BY Carlo ================

            // Register Entity Framework DbContexts
            container.RegisterType<HREntities>();
            container.RegisterType<Master_FilesEntities>();
            container.RegisterType<MISEntities>();

            // Register validators
            container.RegisterType<IValidator<MAF_Main>, MAF_MainValidator>();
            container.RegisterType<IValidator<MAF_Detail>, MAF_DetailValidator>();
            container.RegisterType<IValidator<MAF_Attachment>, MAF_AttachmentValidator>();

            // Register repositories
            container.RegisterType<IEmployeeRepository, EmployeeRepository>();
            container.RegisterType<IUserRepository, UserRepository>();
            container.RegisterType<IApproverRepository, ApproverRepository>();
            container.RegisterType<IReasonRepository, ReasonRepository>();
            container.RegisterType<ILastSeriesRepository, LastSeriesRepository>();
            container.RegisterType<IMainRepository, MainRepository>();
            container.RegisterType<IDetailsRepository, DetailsRepository>();
            container.RegisterType<IAttachmentsRepository, AttachmentsRepository>();
            container.RegisterType<IHistoryMainRepository, HistoryMainRepository>();
            container.RegisterType<IHistoryDetailsRepository, HistoryDetailsRepository>();
            container.RegisterType<IHistoryAttachmentRepository, HistoryAttachmentRepository>();

            // Register services
            container.RegisterType<IEmployeeService, EmployeeService>();
            container.RegisterType<IUserService, UserService>();
            container.RegisterType<IApproverService, ApproverService>();
            container.RegisterType<IReasonService, ReasonService>();
            container.RegisterType<ILastSeriesService, LastSeriesService>();
            container.RegisterType<IMainService, MainService>();
            container.RegisterType<IDetailsService, DetailsService>();
            container.RegisterType<IAttachmentsService, AttachmentsService>();
            container.RegisterType<IEmailSenderService, EmailSenderService>();
            container.RegisterType<IUserContextService, UserContextService>();
            container.RegisterType<IHistoryMainService, HistoryMainService>();
            container.RegisterType<IHistoryDetailsService, HistoryDetailsService>();
            container.RegisterType<IHistoryAttachmentService, HistoryAttachmentService>();

            // === END-ADDED ================

            // Set the dependency resolver for MVC
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));


            // Initialize the queue with the LastSeriesService
            var lastSeriesService = container.Resolve<ILastSeriesService>();
            SeriesNumberQueue.Initialize(lastSeriesService);
        }
    }
}