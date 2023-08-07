using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cafe_management.Models
{
    public class ChangePassword
    {
        public string oldPassword { get; set; }

        public string NewPassword { get; set; }
    }
}