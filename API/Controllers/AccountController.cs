﻿using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API;

public class AccountController : BaseAPIController
{
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;

    public AccountController(DataContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("register")] //POST: api/account/register
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto registerDto) 
    {
        if (await UserExists(registerDto.Username)) 
        {
            return BadRequest("Username is taken!");
        }

        using var hmac = new HMACSHA512();

        var user = new AppUser
        {
            Username = registerDto.Username.ToLower(),
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

         return new UserDto 
        {
            Username = user.Username,
            Token = _tokenService.CreateToken(user)
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto) 
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == loginDto.Username);
        if (user == null) 
        {
            return Unauthorized("Invalid username!");
        }

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
        for (int i = 0; i < computedHash.Length; i++) 
        {
            if (computedHash[i] != user.PasswordHash[i]) 
            {
                return Unauthorized("Invalid password!");
            }
        }

        return new UserDto 
        {
            Username = user.Username,
            Token = _tokenService.CreateToken(user)
        };
    }

    private async Task<bool> UserExists (string username)
    {
        return await _context.Users.AnyAsync(u => u.Username == username.ToLower());
    }
}
