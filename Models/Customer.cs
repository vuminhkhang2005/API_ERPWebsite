using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebERP.Models
{
    public class Customer
    {
        [Key]
        public int customerId { get; set; }
        public string customerName { get; set; }
        public string city { get; set; }
        public string customerType { get; set; }



        public Customer(int customerId, string customerName, string city, string customerType)
        {
            this.customerId = customerId;
            this.customerName = customerName;
            this.city = city;
            this.customerType = customerType;
        }
        public Customer()
        {

        }
    }
}
