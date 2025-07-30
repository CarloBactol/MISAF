using Microsoft.Ajax.Utilities;
using MISAF_Project.DTO;
using MISAF_Project.EDMX;
using MISAF_Project.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MISAF_Project.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailSenderService _emailSenderService;
        private readonly IAttachmentsService _attachmentsService;

        public UserService(
            IUserRepository userRepository,
            IEmailSenderService emailSenderService,
            IAttachmentsService attachmentsService)
        {
            _userRepository = userRepository;
            _emailSenderService = emailSenderService;
            _attachmentsService = attachmentsService;
        }

        public IQueryable<User> QueryUser()
        {
            return _userRepository.QueryUser();
        }

        public void NotifyUserRequestStatus(MAFMainDto mainDto, string attachmentFilePath, string updatorType)
        {
            if (mainDto == null)
            {
                return;
            }

            var requestorEmail = QueryUser()
                .Where(d => d.ID_No == mainDto.Requestor_ID_No)
                .AsNoTracking()
                .Select(d => d.Email)
                .FirstOrDefault();

            if (requestorEmail.IsNullOrWhiteSpace())
            {
                return;
            }

            mainDto.MAF_Details = mainDto.MAF_Details ?? new List<MAF_Detail>(); // Incase MAF_Details is still null

            var attachments = _attachmentsService
                                .QueryAttachment()
                                .AsNoTracking()
                                .Where(a => a.MAF_No == mainDto.MAF_No)
                                .ToList();

            var subject = $"(MISAF #{mainDto.MAF_No}): MIS ACTION FORM IS {mainDto.Status.ToUpper()}";

            _emailSenderService.SendEmail(requestorEmail, subject, mainDto, mainDto.MAF_Details, attachments, attachmentFilePath, true, updatorType);
        }
    }
}