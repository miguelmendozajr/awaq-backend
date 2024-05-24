using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace awaq_backend.Controllers
{
    public class User
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        public string? JoinedAt { get; set; }
    }

    public class UsersController : Controller
    {
        private readonly IConfiguration _configuration;

        public UsersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("api/users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                List<User> users = new List<User>();
                string? connectionString = _configuration.GetConnectionString("myDb1");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = connection;
                    cmd.CommandText = "Select user_id, email, name, password, r.role AS role, joinedAt from users u JOIN roles r ON u.role_id = r.role_id;";

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            User usr = new User();
                            usr.Name = reader["name"].ToString();
                            DateTime joinedAt = Convert.ToDateTime(reader["joinedAt"]);
                            usr.Password = reader["password"].ToString();
                            usr.JoinedAt = joinedAt.ToString("dd/MM/yyyy");
                            usr.Role = reader["role"].ToString();
                            usr.Email = reader["email"].ToString();
                            users.Add(usr);
                        }
                    }
                    connection.Dispose();
                }
               

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        public class CreateUserRequest
        {
            public required string Name { get; set; }
            public required string Email { get; set; }
            public required string Password { get; set; }
            public required int Role { get; set; }
        }

        [HttpPost]
        [Route("api/user")]
        public async Task<IActionResult> CreateUser([FromBody]CreateUserRequest request)
        {
            try
            {
                List<User> users = new List<User>();
                string? connectionString = _configuration.GetConnectionString("myDb1");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = connection;
                    cmd.CommandText = "INSERT INTO users(name, email, password, role_id, joinedAt) VALUES(@Name, @Email, @Password, @Role, @JoinedAt)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@Name", request.Name);
                    cmd.Parameters.AddWithValue("@Email", request.Email);
                    cmd.Parameters.AddWithValue("@Password", request.Password);
                    cmd.Parameters.AddWithValue("@Role", request.Role);
                    cmd.Parameters.AddWithValue("@JoinedAt", DateTime.Now);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    connection.Dispose();

                    if (rowsAffected > 0)
                    {
                        return Ok(new { Message = "Usuario creado exitosamente." });
                    }
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }

}

