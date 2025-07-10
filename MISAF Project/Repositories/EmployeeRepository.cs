using MISAF_Project.DTO;
using MISAF_Project.EDMX;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace MISAF_Project.Repositories
{
    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(HREntities context) : base(context)
        {
        }


        public List<EmployeeDto> GetById(string id)
        {
            return _dbSet
            .Where(x =>
                   x.ID_No == id )
               .Select(x => new EmployeeDto
               {
                   ID_No = x.ID_No,
                   First_Name = x.First_Name,
                   Last_Name = x.Last_Name,
                   Name = x.Name
               }).ToList();
        }

        public List<EmployeeDto> GetByName(string name)
        {
            name = name.ToLower();

            return _dbSet
                .Where(x =>
                    x.First_Name.ToLower() == name ||
                    x.Last_Name.ToLower() == name ||
                    x.Name.ToLower().Contains(name))
                .Select(x => new EmployeeDto
                {
                    ID_No = x.ID_No,
                    Name = x.Name
                }).ToList();
        }

        public IQueryable<Employee> QueryEmployee()
        {
            return _dbSet.AsQueryable();
        }
    }
}