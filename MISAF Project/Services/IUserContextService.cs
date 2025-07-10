using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MISAF_Project.Services
{
    public interface IUserContextService
    {
        string GetApprover();
        string GetEndorser();
        string GetMIS();
        string GetRequestor();
        string GetUserLogin();
        string GetUserIDLogin();
    }
}