﻿namespace UserService.Domain.Entities
{
    public class Customer
    {
        public int ID { get; set; }
        public int? AccountID { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
    }
}
