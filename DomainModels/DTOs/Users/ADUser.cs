using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModels.DTOs.Users
{
    public class ADUser
    {
        public string Name { get; set; }
        public string DomainMail { get; set; }
        public List<string> Roles { get; set; }
    }
}
