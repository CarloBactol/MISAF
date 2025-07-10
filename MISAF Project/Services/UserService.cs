using MISAF_Project.DTO;
using MISAF_Project.EDMX;
using MISAF_Project.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MISAF_Project.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IQueryable<User> QueryUser()
        {
            return _userRepository.QueryUser();
        }
    }
}