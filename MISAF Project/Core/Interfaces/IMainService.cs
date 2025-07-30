using MISAF_Project.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MISAF_Project.Core.Interfaces
{
    public interface IMainService
    {
        List<MainData> GetAllByUser(UserData user, string type);
        List<MainData> GetAll();
    }
}