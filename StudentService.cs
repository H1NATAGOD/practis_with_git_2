namespace ConsoleApp2;

public class StudentService
{
           private const string ConnectionString = "Host=localhost;Username=postgres;Password=1234;Database=postgres";

        public List<Student> GetStudents(string sortBy = "grade")
        {
            List<Student> students = new List<Student>();
            string orderBy = sortBy switch
            {
                "name" => "s.full_name",
                "course" => "s.course",
                _ => "average_grade DESC"
            };

            using var conn = new NpgsqlConnection(ConnectionString);
            conn.Open();
            string query = $@"
                SELECT s.id, s.full_name, s.birth_date, s.enrollment_year, s.course, s.group_name,
                       COALESCE(AVG(g.grade), 0) AS average_grade
                FROM students s
                LEFT JOIN grades g ON s.id = g.student_id
                GROUP BY s.id
                ORDER BY {orderBy}";

            using var cmd = new NpgsqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                students.Add(new Student
                {
                    Id = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    BirthDate = reader.GetDateTime(2),
                    EnrollmentYear = reader.GetInt32(3),
                    Course = reader.GetInt32(4),
                    GroupName = reader.GetString(5),
                    AverageGrade = reader.GetDecimal(6)
                });
            }
            return students;
        }

        public void AddStudent(string fullName, DateTime birthDate, int enrollmentYear, int course, string groupName)
        {
            using var conn = new NpgsqlConnection(ConnectionString);
            conn.Open();
            string query = @"INSERT INTO students (full_name, birth_date, enrollment_year, course, group_name)
                             VALUES (@name, @birth, @enrollment, @course, @group)";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("name", fullName);
            cmd.Parameters.AddWithValue("birth", birthDate);
            cmd.Parameters.AddWithValue("enrollment", enrollmentYear);
            cmd.Parameters.AddWithValue("course", course);
            cmd.Parameters.AddWithValue("group", groupName);
            cmd.ExecuteNonQuery();
        }
 
}