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
        public int? ID { get; set; }
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
                    cmd.CommandText = "Select user_id, email, name, password, r.role AS role, joinedAt from users u JOIN roles r ON u.role_id = r.role_id ORDER BY joinedAt DESC;";

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            User usr = new User();
                            usr.ID = (int?)reader["user_id"];
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

                    MySqlCommand cmd2 = new MySqlCommand();
                    cmd2.Connection = connection;
                    cmd2.CommandText = "SELECT email FROM users where email=@Email";
                    cmd2.Parameters.AddWithValue("@Email", request.Email);
                    using (var reader = await cmd2.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            if (reader["email"].ToString() == request.Email)
                            {
                                return BadRequest("El correo electrónico ya está vinculado a una cuenta.");

                            }
   
                        }
                    }


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

        public class EditUserRoleRequest
        {
            public required int Role { get; set; }
        }

        [HttpPut]
        [Route("api/user/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] EditUserRoleRequest request)
        {
            try
            {
                string? connectionString = _configuration.GetConnectionString("myDb1");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    MySqlCommand cmd2 = new MySqlCommand();
                    cmd2.Connection = connection;
                    cmd2.CommandText = "UPDATE users SET role_id = @Role WHERE user_id = @Id";
                    cmd2.Parameters.AddWithValue("@Id", id);
                    cmd2.Parameters.AddWithValue("@Role", request.Role);
                    await cmd2.ExecuteNonQueryAsync();
                }

                return Ok("Usuario editado exitosamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet]
        [Route("api/user/{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                string? connectionString = _configuration.GetConnectionString("myDb1");

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand cmd2 = new MySqlCommand();
                    cmd2.Connection = connection;
                    cmd2.CommandText = "SELECT name, birthDate, phoneNumber, profile_picture FROM users WHERE user_id = @Id";
                    cmd2.Parameters.AddWithValue("@Id", id);

                    // Execute the command and retrieve data
                    using (MySqlDataReader reader = (MySqlDataReader)await cmd2.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            // Retrieve data from the reader
                            string? name = reader.IsDBNull(0) ? null : reader.GetString(0);
                            DateTime? birthDate = reader.IsDBNull(1) ? null : (DateTime?)reader.GetDateTime(1);
                            string? phoneNumber = reader.IsDBNull(2) ? null : reader.GetString(2);
                            string? profilePicture = reader.IsDBNull(3) ? null : reader.GetString(3);

                            // Construct response object
                            var responseData = new
                            {
                                Name = name,
                                BirthDate = birthDate,
                                PhoneNumber = phoneNumber,
                                ProfilePicture = profilePicture
                            };

                            return Ok(responseData);
                        }
                        else
                        {
                            // User not found
                            return NotFound();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        public class EditUserRequest
        {
            public string? Name { get; set; }
            public string? Phone { get; set; }
            public DateTime? Date { get; set; }
        }

        [HttpPut]
        [Route("api/user/{id}")]
        public async Task<IActionResult> UpdateUserData(int id, [FromBody] EditUserRequest request)
        {
            try
            {
                string? connectionString = _configuration.GetConnectionString("myDb1");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    MySqlCommand cmd2 = new MySqlCommand();
                    cmd2.Connection = connection;

                    // Start building the query
                    List<string> setClauses = new List<string>();
                    if (request.Name != null)
                    {
                        setClauses.Add("name=@Name");
                        cmd2.Parameters.AddWithValue("@Name", request.Name);
                    }
                    if (request.Date != null)
                    {
                        setClauses.Add("birthDate=@BirthDate");
                        cmd2.Parameters.AddWithValue("@BirthDate", request.Date);
                    }
                    if (request.Phone != null)
                    {
                        setClauses.Add("phoneNumber=@PhoneNumber");
                        cmd2.Parameters.AddWithValue("@PhoneNumber", request.Phone);
                    }

                    string setClause = string.Join(", ", setClauses);

                    if (string.IsNullOrEmpty(setClause))
                    {
                        return BadRequest("No valid fields provided to update.");
                    }

                    cmd2.CommandText = $"UPDATE users SET {setClause} WHERE user_id=@Id";
                    cmd2.Parameters.AddWithValue("@Id", id);

                    await cmd2.ExecuteNonQueryAsync();
                }

                return Ok("Usuario editado exitosamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }

}

