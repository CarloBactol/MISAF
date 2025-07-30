using MISAF_Project.Core.Interfaces;
using MISAF_Project.Core.Data;
using MISAF_Project.EDMX;
using System;
using System.Linq;

namespace MISAF_Project.Core.Services
{
    public class AuthService : IAuthService
    {
        public UserData Attempt(string idNo, DateTime birthDate)
        {
            Employee employee = null;

            using (var hr_db = new HREntities()) {
                employee = hr_db.Employees
                    .AsNoTracking()
                    .FirstOrDefault(x => x.ID_No == idNo && x.Birthdate == birthDate);
            }

            if (employee == null) {
                return null;
            }

            var user = new UserData()
            {
                IdNo = employee.ID_No,
                FirstName = employee.First_Name,
                MiddleName = employee.Middle_Name,
                LastName = employee.Last_Name,
                FullName = employee.Name,
                Birthdate = employee.Birthdate
            };

            using (var master_db = new Master_FilesEntities())
            {
                user.Email = master_db.Users
                    .AsNoTracking()
                    .FirstOrDefault(x => x.ID_No == user.IdNo).Email;
            }

            using (var mis_db = new MISEntities()) {
                var approver = mis_db.MAF_Approvers
                    .AsNoTracking()
                    .FirstOrDefault(x => x.ID_No == user.IdNo);

                user.IsMIS = approver?.MIS == "Y";
                user.IsEndorser = approver?.Endorser_Only == "Y";
                user.IsApprover = approver?.Endorser_Only == "N";
            }

            return user;
        }
    }
}