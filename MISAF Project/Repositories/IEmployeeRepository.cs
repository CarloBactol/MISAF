using MISAF_Project.DTO;
using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISAF_Project.Repositories
{
    public interface IEmployeeRepository
    {
        List<EmployeeDto> GetByName(string name);
        List<EmployeeDto> GetById(string id);

        IQueryable<Employee> QueryEmployee();
    }
}
