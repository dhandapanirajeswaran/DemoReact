using ReactApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReactApi.Services
{
   public interface IStudentService
    {
        Task<IEnumerable<Student>> GetStudents();
    }
}
