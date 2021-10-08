using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;

namespace FunctionAppwithsqldb1457
{
    public static class CRUDFunction
    {
        [FunctionName("CreateStu")]
        public static async Task<IActionResult> CreateStu(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "student")] HttpRequest req, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<StudentModel>(requestBody);
            try
            {
                using (SqlConnection connection = new
                    SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
                {
                    connection.Open();
                    if (!String.IsNullOrEmpty(input.Name))
                    {
                        var query = $"INSERT INTO [Student] (Name,Age,Class) VALUES('{input.Name}', '{input.Age}','{input.Class}')";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
                return new BadRequestResult();
            }
            return new OkResult();
        }

        [FunctionName("GetStu")]
        public static async Task<IActionResult> GetEmp(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "student")] HttpRequest req, ILogger log)
        {
            List<StudentModel> StuList = new List<StudentModel>();
            try
            {
                using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
                {
                    connection.Open();
                    var query = @"Select * from Student";
                    SqlCommand command = new SqlCommand(query, connection);
                    var reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        StudentModel stu = new StudentModel()
                        {
                            Id = (int)reader["Id"],
                            Name = reader["Name"].ToString(),
                            Age = reader["Age"].ToString(),
                            Class = reader["Class"].ToString()

                        };
                        StuList.Add(stu);
                    }
                }
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
            }
            if (StuList.Count > 0)
            {
                return new OkObjectResult(StuList);
            }
            else
            {
                return new NotFoundResult();
            }
        }

    }
    public class StudentModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Age { get; set; }
        public string Class { get; set; }

    }
}
