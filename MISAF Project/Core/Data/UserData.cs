using System;
using System.ComponentModel.DataAnnotations;

namespace MISAF_Project.Core.Data
{
    public class UserData
    {
        public string IdNo { get; set; }
        public DateTime Birthdate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public bool IsRequestor { get => !IsEndorser & !IsApprover & !IsMIS; }
        public bool IsEndorser { get; set; }
        public bool IsApprover { get; set; }
        public bool IsMIS { get; set; }
    }
}