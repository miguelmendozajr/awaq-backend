using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace awaq_backend.Controllers
{
    public class SignInController : Controller
    {
        private readonly IConfiguration _configuration;

        public SignInController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("api/signIn")]
        public async Task<IActionResult> SendPasscode([FromBody] SignInRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("Email is required.");
            }

            if (string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Password is required.");
            }

            string email = string.Empty;
            string password = string.Empty;
            string role = string.Empty;
            string connectionString = _configuration.GetConnectionString("myDb1");
            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                await conexion.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "signIn";
                    cmd.Parameters.AddWithValue("@email_in", request.Email);
                    cmd.Connection = conexion;
                    using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            email = reader["email"].ToString();
                            password = reader["password"].ToString();
                            role = reader["role"].ToString();
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email does not exist.");
            }

            if (string.IsNullOrEmpty(password))
            {
                return BadRequest("Password does not exist.");
            }

            if (string.IsNullOrEmpty(role))
            {
                return BadRequest("User does not have a role.");
            }


            if(password != request.Password)
            {
                return BadRequest("Password doesn't match.");
            }


            return Ok(new { Message = "User logged in succesfully", Role = role });

        }

        public class SignInRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }
}
