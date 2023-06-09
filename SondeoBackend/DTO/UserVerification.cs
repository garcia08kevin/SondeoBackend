﻿using System.ComponentModel.DataAnnotations;

namespace SondeoBackend.DTO
{
    public class UserVerification
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
