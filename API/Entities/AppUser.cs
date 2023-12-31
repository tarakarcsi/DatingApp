﻿using System.ComponentModel.DataAnnotations;

namespace API;

public class AppUser
{
    public int Id { get; set; }
    
    [Required]
    public string Username { get; set; }
    public byte[] PasswordHash {get; set; }
    public byte[] PasswordSalt { get; set; }
}
