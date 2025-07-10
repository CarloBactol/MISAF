using MISAF_Project.DTO;
using MISAF_Project.EDMX;
using MISAF_Project.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace MISAF_Project.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public List<EmployeeDto> GetById(string id)
        {
            return _employeeRepository.GetById(id);
        }

        public List<EmployeeDto> GetByName(string name)
        {
            return _employeeRepository.GetByName(name);
        }

        public IQueryable<Employee> QueryEmployee()
        {
            return _employeeRepository.QueryEmployee();
        }
    }
}