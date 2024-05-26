using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace awaq_backend.Controllers
{
    public class SignInRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

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
                return BadRequest("El correo electrónico es requerido.");
            }

            if (string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("La contraseña es requerida.");
            }
            int? id = 0;
            string? email = string.Empty;
            string? password = string.Empty;
            string? role = string.Empty;
            string? connectionString = _configuration.GetConnectionString("myDb1");
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "signIn";
                    cmd.Parameters.AddWithValue("@email_in", request.Email);
                    cmd.Connection = connection;
                    using (MySqlDataReader reader = (MySqlDataReader) await cmd.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            id = (int?)reader["id"];
                            email = reader["email"].ToString();
                            password = reader["password"].ToString();
                            role = reader["role"].ToString();
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("El correo electrónico no existe.");
            }

            if (string.IsNullOrEmpty(password))
            {
                return BadRequest("La contraseña no existe.");
            }

            if (string.IsNullOrEmpty(role))
            {
                return BadRequest("El usuario no tiene rol.");
            }


            if(password != request.Password)
            {
                return BadRequest("La contraseña es incorrecta.");
            }


            return Ok( new { Message = "Inicio de sesión exitoso.", Role = role, Id = id });

        }
    }
}
