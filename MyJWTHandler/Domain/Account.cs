﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyJWTHandler.Domain
{
    class Account
    {
        public int ID { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Role { get; set; }
        public bool Status { get; set; }
    }
}
