using MISAF_Project.DTO;
using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MISAF_Project.Services
{
    public interface IEmployeeService
    {
        List<EmployeeDto> GetByName(string name);
        List<EmployeeDto> GetById(string id);

        IQueryable<Employee> QueryEmployee();
    }
}