﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [NotMapped]
        public string EncryptedId { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "Name cannot excess 50 characters")]
        public string Name { get; set; }

        [Required]
        [RegularExpression("[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?",
            ErrorMessage = "Invalid Email Format")]
        [Display(Name = "Office Email")]
        public string Email { get; set; }

        [Required]
        public Dept? Department { get; set; }

        public string PhotoPath { get; set; }
    }
}
