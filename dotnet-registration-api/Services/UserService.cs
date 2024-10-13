using dotnet_registration_api.Data;
using dotnet_registration_api.Data.Entities;
using dotnet_registration_api.Data.Models;
using dotnet_registration_api.Data.Repositories;
using dotnet_registration_api.Helpers;
using Mapster;
using Microsoft.Win32;
using System.Text;
using System.Security.Cryptography;
using System.Text;

namespace dotnet_registration_api.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;
        private readonly UserDataContext _context;
        public UserService(UserRepository userRepository , UserDataContext context)
        {
            _userRepository = userRepository;
            _context = context;
        }
        public async Task<List<User>> GetAll()
        {
            return await _userRepository.GetAllUsers();
        }
        public async Task<User> GetById(int id)
        {
            var user = await _userRepository.GetUserById(id);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            return user;
        }

        public async Task<User> Login(LoginRequest login)
        {

            var hashedPassword = HashPassword(login.Password);

            var user = new User
            {
                Username = login.Username,
                PasswordHash = hashedPassword
            };


           var userLog =  await _userRepository.GetUserByUsernameAndPassword(user.Username , user.PasswordHash);

            if (userLog != null)
            {
                return userLog;
            }
            else
            {
                throw new UnauthorizedAccessException("Invalid username or password");
            }

        }
        public async Task<User> Register(RegisterRequest register)
        {
            var user = new User
            {
                Username = register.Username,
                PasswordHash = HashPassword(register.Password), 
                LastName = register.LastName,
                FirstName = register.FirstName
            };

          
            await _context.Users.AddAsync(user);

        
            await _context.SaveChangesAsync();

            return user; 
        }
        public async Task<User> Update(int id, UpdateRequest updateRequest)
        {
            var user = await _userRepository.GetUserById(id);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            if (!string.IsNullOrWhiteSpace(updateRequest.FirstName))
            {
                user.FirstName = updateRequest.FirstName;
            }

            if (!string.IsNullOrWhiteSpace(updateRequest.LastName))
            {
                user.LastName = updateRequest.LastName;
            }

            if (!string.IsNullOrWhiteSpace(updateRequest.Username))
            {
                user.Username = updateRequest.Username;
            }

            if (!string.IsNullOrWhiteSpace(updateRequest.NewPassword))
            {
                if (user.PasswordHash != HashPassword(updateRequest.OldPassword))
                {
                    throw new UnauthorizedAccessException("Old password is incorrect");
                }

                user.PasswordHash = HashPassword(updateRequest.NewPassword);
            }

       
            await _userRepository.UpdateUser(user);

            return user;
        }



        private string HashPassword(string password)
    {
        
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));
        }

        using (var sha256 = SHA256.Create())
        {
           
            var passwordBytes = Encoding.UTF8.GetBytes(password);

            var hashedBytes = sha256.ComputeHash(passwordBytes);

            var hashedPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();

            return hashedPassword; 
        }
    }


    public async Task Delete(int id)
        {
            var user = await _userRepository.GetUserById(id);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            await _userRepository.DeleteUser(id);
    
        }

    }
}
