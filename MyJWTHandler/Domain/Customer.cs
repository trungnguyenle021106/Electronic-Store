using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyJWTHandler.Domain
{
  public class Customer
    {
        public int ID { get; set; }
        public int? AccountID { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
    }
}
