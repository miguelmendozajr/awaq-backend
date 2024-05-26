using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;
using Google.Protobuf.WellKnownTypes;


namespace awaq_backend.Controllers
{
    public class Dashboard : Controller
    {

        private readonly IConfiguration _configuration;

        public Dashboard(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public class UsersMetrics
        {
            public int CapacitedUsers { get; set; }
            public int TotalUsers { get; set; }
        }

        public async Task<UsersMetrics> GetTotalNumberOfCapacitedUsersHelper()
        {
            string? connectionString = _configuration.GetConnectionString("myDb1");
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("getTotalNumberOfCapacitedUsers", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    var reader = await command.ExecuteReaderAsync();
                    var metrics = new UsersMetrics();
                    if (await reader.ReadAsync())
                    {
                        metrics.CapacitedUsers = reader.GetInt32(0);
                        metrics.TotalUsers = reader.GetInt32(1);
                    }
                    return metrics;
                }
            }
        }

        [HttpGet]
        [Route("api/metrics")]
        public async Task<IActionResult> GetTotalNumberOfCapacitedUsers()
        {
            try
            {
                var userProgress = await GetTotalNumberOfCapacitedUsersHelper();
                return Ok(userProgress);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        public class UserProgress
        {
            public int TEDI { get; set; }
            public int Ciberseguridad { get; set; }
            public int Comunicaciones { get; set; }
            public int PautasDeConducta { get; set; }
        }


        public async Task<UserProgress?> GetUserProgressHelper(int userId)
        {
            string? connectionString = _configuration.GetConnectionString("myDb1");
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("getUserProgress", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@userId", userId);

                    var reader = await command.ExecuteReaderAsync();

                    // Check if there are any results
                    if (await reader.ReadAsync())
                    {
                        var userProgress = new UserProgress
                        {
                            TEDI = Convert.ToInt32(reader.GetDecimal(0)),
                            Ciberseguridad = Convert.ToInt32(reader.GetDecimal(1)),
                            Comunicaciones = Convert.ToInt32(reader.GetDecimal(2)),
                            PautasDeConducta = Convert.ToInt32(reader.GetDecimal(3)),
                        };
                        return userProgress;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        [HttpGet]
        [Route("api/metrics/user/{id}")]
        public async Task<IActionResult> GetUserProgress(int id)
        {
            try
            {
                var userProgress = await GetUserProgressHelper(id);
                if (userProgress == null)
                {
                    return NotFound();
                }
                return Ok(userProgress);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        public class CompleteUserMetrics
        {
            public int ID { get; set; }
            public required string Name { get; set; }
            public required string Email { get; set; }
            public string? PhoneNumber { get; set; }
            public DateTime JoinedAt { get; set; }
            public int TEDI { get; set; }
            public int Ciberseguridad { get; set; }
            public int Comunicaciones { get; set; }
            public int PautasDeConducta { get; set; }
        }


        public async Task<IEnumerable<CompleteUserMetrics>> GetAllUsersProgressHelper()
        {
            string? connectionString = _configuration.GetConnectionString("myDb1");
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("getAllUsersProgress", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    var reader = await command.ExecuteReaderAsync();
                    var results = new List<CompleteUserMetrics>();
                    while (await reader.ReadAsync())
                    {
                        var userProgress = new CompleteUserMetrics
                        {
                            ID = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Email = reader.GetString(2),
                            PhoneNumber = reader.GetString(3),
                            JoinedAt = reader.GetDateTime(4),
                            TEDI = Convert.ToInt32(reader.GetDecimal(5)),
                            Ciberseguridad = Convert.ToInt32(reader.GetDecimal(6)),
                            Comunicaciones = Convert.ToInt32(reader.GetDecimal(7)),
                            PautasDeConducta = Convert.ToInt32(reader.GetDecimal(8)),
                        };
                        results.Add(userProgress);
                    }
                    return results;
                }
            }
        }

        [HttpGet]
        [Route("api/metrics/users")]
        public async Task<IActionResult> GetAllUsersProgress()
        {
            try
            {
                var userProgress = await GetAllUsersProgressHelper();
                if (userProgress == null || !userProgress.Any())
                {
                    return NotFound();
                }
                return Ok(userProgress);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        public class RankingUserMetrics
        {
            public required string Name { get; set; }
            public int TEDI { get; set; }
            public int Ciberseguridad { get; set; }
            public int Comunicaciones { get; set; }
            public int PautasDeConducta { get; set; }
        }


        public async Task<IEnumerable<RankingUserMetrics>> GetTopUsersHelper(int top)
        {
            string? connectionString = _configuration.GetConnectionString("myDb1");
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("topNUsers", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@N", top);
                    var reader = await command.ExecuteReaderAsync();
                    var results = new List<RankingUserMetrics>();
                    while (await reader.ReadAsync())
                    {
                        var topUsers = new RankingUserMetrics
                        {
                            Name = reader.GetString(1),
                            TEDI = Convert.ToInt32(reader.GetDecimal(2)),
                            Ciberseguridad = Convert.ToInt32(reader.GetDecimal(3)),
                            Comunicaciones = Convert.ToInt32(reader.GetDecimal(4)),
                            PautasDeConducta = Convert.ToInt32(reader.GetDecimal(5))
                        };
                        results.Add(topUsers);
                    }
                    return results;
                }
            }
        }

        [HttpGet]
        [Route("api/metrics/users/{top}")]
        public async Task<IActionResult> GetTopUsers(int top)
        {
            try
            {
                var topUsers = await GetTopUsersHelper(top);
                if (topUsers == null || !topUsers.Any())
                {
                    return NotFound();
                }
                return Ok(topUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        public async Task<UserProgress?> GetUserProgressPerSubjectHelper()
        {
            string? connectionString = _configuration.GetConnectionString("myDb1");
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("getOverallProblemSolvingPercentage", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    var reader = await command.ExecuteReaderAsync();

                    // Check if there are any results
                    if (await reader.ReadAsync())
                    {
                        var userProgress = new UserProgress
                        {
                            TEDI = Convert.ToInt32(reader.GetDecimal(0)),
                            Ciberseguridad = Convert.ToInt32(reader.GetDecimal(1)),
                            Comunicaciones = Convert.ToInt32(reader.GetDecimal(2)),
                            PautasDeConducta = Convert.ToInt32(reader.GetDecimal(3))
                        };
                        return userProgress;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        [HttpGet]
        [Route("api/metrics/subjects")]
        public async Task<IActionResult> GetUserProgressPerSubject()
        {
            try
            {
                var userProgress = await GetUserProgressPerSubjectHelper();
                if (userProgress == null)
                {
                    return NotFound();
                }
                return Ok(userProgress);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



    }
}

