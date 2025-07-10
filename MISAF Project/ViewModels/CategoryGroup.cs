using System;
using System.Collections.Generic;


namespace MISAF_Project.ViewModels
{
    public class CategoryGroup
    {
        public int CategoryId { get; set; }
        public string GroupName { get; set; }
        public List<string> Categories { get; set; }
    }

}