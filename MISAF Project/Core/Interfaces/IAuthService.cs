using MISAF_Project.Core.Data;
using System;

namespace MISAF_Project.Core.Interfaces
{
    public interface IAuthService
    {
        UserData Attempt(string idNo, DateTime birthDate);
    }
}