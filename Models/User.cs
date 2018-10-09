using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleMvcApp.Models
{
    public class User
    {
        public string User_id { get; set; }
        public string Connection { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Phone_number { get; set; }
        public string User_metadata { get; set; }
        public string Email_verified { get; set; }
        public string Verify_email { get; set; }
        public string Phone_verified { get; set; }
        public string Name { get; set; }
    }
}
