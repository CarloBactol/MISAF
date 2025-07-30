using System;
using System.ComponentModel.DataAnnotations;

namespace MISAF_Project.Core.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "ID No.")]
        public string ID_No { get; set; }
        [Required]
        [Display(Name = "Birth Date")]
        public DateTime Birth_Date { get; set; }
    }
}