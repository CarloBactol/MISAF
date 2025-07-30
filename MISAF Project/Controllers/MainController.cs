using MISAF_Project.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MISAF_Project.Controllers
{
    public class MainController : BaseController
    {
        private readonly IMainService _mainService;

        public MainController(IMainService mainService) { 
            _mainService = mainService;
        }

        public JsonResult All(string type = "")
        {
            try
            {
                var types = new List<string>() { 
                    "endorse",
                    "request",
                    "approve",
                    "acknowledge"
                };

                if (! types.Any(x => x == type))
                {
                    return JsonError(new { message = $"AJAX ERROR: '{type}' is invalid." });
                }

                var user = GetAuthUser();
                var mains = _mainService.GetAllByUser(user, type);

                return JsonSuccess(mains);
            }
            catch (Exception ex)
            {
                return JsonError(new { message = ex.Message });
            }
        }
    }
}