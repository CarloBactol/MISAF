using System.Web;


namespace MISAF_Project.Services
{
    public class UserContextService : IUserContextService
    {
        public string GetApprover()
        {
            return HttpContext.Current.Session["Approver"]?.ToString();
        }

        public string GetEndorser()
        {
            return HttpContext.Current.Session["Endorser"]?.ToString();
        }

        public string GetMIS()
        {
            return HttpContext.Current.Session["MIS"]?.ToString();
        }

        public string GetRequestor()
        {
            return HttpContext.Current.Session["Requestor"]?.ToString();
        }

        public string GetUserIDLogin()
        {
            return HttpContext.Current.Session["EmployeeID"]?.ToString();
        }

        public string GetUserLogin()
        {
            return HttpContext.Current.Session["EmployeeName"]?.ToString();
        }
    }
}