using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MISAF_Project.ViewModels
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