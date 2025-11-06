using AlmohandisAPI.Data;
using AlmohandisAPI.DTOs;
using AlmohandisAPI.Entities;
using AlmohandisAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AlmohandisAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly TokenService _tokenService;

        public UsersController(DataContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto userRegisterDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == userRegisterDto.Email.ToLower()))
            {
                return BadRequest("هذا البريد الإلكتروني مستخدم بالفعل.");
            }

            using var hmac = new HMACSHA512();

            var user = new User
            {
                FirstName = userRegisterDto.FirstName,
                LastName = userRegisterDto.LastName,
                Email = userRegisterDto.Email.ToLower(),
                PhoneNumber = userRegisterDto.PhoneNumber,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userRegisterDto.Password)),
                PasswordSalt = hmac.Key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        // ===================================================================
        // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ الدالة المعدلة مع كود التصحيح ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
        // ===================================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            Console.WriteLine($"--- Attempting login for: {userLoginDto.Email} ---");

            // 1. البحث عن المستخدم عن طريق البريد الإلكتروني
            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.Email == userLoginDto.Email.ToLower());

            // إذا لم يتم العثور على المستخدم
            if (user == null)
            {
                Console.WriteLine("Login failed: User not found.");
                return Unauthorized("البريد الإلكتروني أو كلمة المرور غير صحيحة");
            }

            Console.WriteLine("Login check: User found in database.");

            // 2. التحقق من صحة كلمة المرور
            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userLoginDto.Password));

            // طباعة الهاش المخزن والهاش المحسوب للمقارنة
            Console.WriteLine($"Stored Hash (Base64):   {Convert.ToBase64String(user.PasswordHash)}");
            Console.WriteLine($"Computed Hash (Base64): {Convert.ToBase64String(computedHash)}");

            // نقارن الهاش المحسوب مع الهاش المخزن في قاعدة البيانات
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i])
                {
                    Console.WriteLine("Login failed: Password hash does not match.");
                    return Unauthorized("البريد الإلكتروني أو كلمة المرور غير صحيحة");
                }
            }

            Console.WriteLine("Login successful: Password hash matched.");

            // 3. إذا كانت كلمة المرور صحيحة، ننشئ ونرجع التوكن
            return Ok(new
            {
                Email = user.Email,
                Token = _tokenService.CreateToken(user)
            });
        }
    }
}