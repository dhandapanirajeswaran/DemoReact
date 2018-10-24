using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReactApi.Model;
using ReactApi.Repository;
using ReactApi.Repository.Query;

namespace ReactApi.Services
{
    public class StudentService : IStudentService
    {
        private readonly IDatabase _database;

        public StudentService(IDatabase database)
        {
            _database = database;
        }

        public async Task<IEnumerable<Student>> GetStudents()
        {
            IEnumerable<Student> products = await _database.QueryAsync(new GetStudent());
            return products.Select(x => new Student
            {
                Id = x.Id,
                Experiences = x.Experiences,
                Country = x.Country,
                FirstName = x.FirstName,
                LastName = x.LastName
            }).ToList();
        }
    }
}
