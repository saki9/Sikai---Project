using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace Sikai_Project
{
	public class TeacherDashboardStats
	{
		public int CourseCount { get; set; }
		public int AssignmentCount { get; set; }
		public int PendingSubmissionCount { get; set; }
		public int StudentCount { get; set; }
	}

	public class TeacherClassItem
	{
		public long ClassId { get; set; }
		public string ClassCode { get; set; }
		public string ClassName { get; set; }
		public int StudentCount { get; set; }

		public string Display
		{
			get
			{
				return ClassName + " (" + ClassCode + ") - " + StudentCount +
					(StudentCount == 1 ? " student" : " students");
			}
		}
	}

	public class ClassStudentItem
	{
		public long StudentId { get; set; }
		public string FullName { get; set; }
		public string Email { get; set; }
	}

	public static class TeacherDb
	{
		
		
		

		public static async Task<TeacherDashboardStats> GetDashboardStatsAsync(
			Account teacher)
		{
			if (teacher == null)
				throw new ArgumentNullException("teacher");

			if (teacher.Role != Role.Teacher)
				throw new InvalidOperationException(
					"The current account is not a teacher account.");

			TeacherDashboardStats stats = new TeacherDashboardStats();

			stats.CourseCount =
				await GetCourseCountAsync(teacher);

			stats.AssignmentCount =
				await GetAssignmentCountAsync(teacher);

			stats.PendingSubmissionCount = await GetPendingSubmissionCountAsync(teacher);
			stats.StudentCount = await GetStudentCountAsync(teacher);

			return stats;
		}


		
		
		

		private static async Task<int> GetCourseCountAsync(
			Account teacher)
		{
			await Db.EnsureClassSubjectsAsync();

			

			const string sql = @"
                SELECT COUNT(*)
                FROM SIKAI.SikaiClasses
                WHERE TeacherID = :teacherId
                  AND SubjectID IS NOT NULL";

			using (OracleConnection conn = await Db.OpenAsync())
			using (OracleCommand cmd = new OracleCommand(sql, conn))
			{
				cmd.BindByName = true;

				cmd.Parameters.Add(
					"teacherId",
					teacher.Id);

				object result =
					await cmd.ExecuteScalarAsync();

				return Convert.ToInt32(result);
			}
		}


		
		
		

		private static async Task<int> GetAssignmentCountAsync(
			Account teacher)
		{
			const string sql = @"
                SELECT COUNT(*)
                FROM SIKAI.Tasks
                WHERE OwnerRole = 'TEACHER'
                  AND OwnerID = :teacherId";

			using (OracleConnection conn = await Db.OpenAsync())
			using (OracleCommand cmd = new OracleCommand(sql, conn))
			{
				cmd.BindByName = true;

				cmd.Parameters.Add(
					"teacherId",
					teacher.Id);

				object result =
					await cmd.ExecuteScalarAsync();

				return Convert.ToInt32(result);
			}
		}


		
		
		

		private static async Task<int> GetStudentCountAsync(Account teacher)
		{
			const string sql = @"SELECT COUNT(DISTINCT e.StudentID)
                FROM SIKAI.ClassEnrollments e
                JOIN SIKAI.SikaiClasses c ON c.ClassID=e.ClassID
                WHERE c.TeacherID=:teacherId";
			using (OracleConnection conn = await Db.OpenAsync())
			using (OracleCommand cmd = new OracleCommand(sql, conn))
			{
				cmd.BindByName = true;
				cmd.Parameters.Add("teacherId", teacher.Id);
				return Convert.ToInt32(await cmd.ExecuteScalarAsync());
			}
		}

        private static async Task<int> GetPendingSubmissionCountAsync(Account teacher)
        {
            const string sql = @"SELECT COUNT(*) FROM SIKAI.AssignmentSubmissions sub
                JOIN SIKAI.Tasks t ON t.TaskID=sub.TaskID
                WHERE t.OwnerRole='TEACHER' AND t.OwnerID=:teacherId AND sub.Status='Pending'";
            using (OracleConnection conn = await Db.OpenAsync())
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.BindByName = true;
                cmd.Parameters.Add("teacherId", teacher.Id);
                return Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
        }


		
		
		

		public static async Task<List<SubjectModel>> GetTeacherSubjectsAsync(
			Account teacher)
		{
			if (teacher == null)
				throw new ArgumentNullException("teacher");

			await Db.EnsureClassSubjectsAsync();

			const string sql = @"
                SELECT
                    s.SubjectID,
                    s.Code,
                    s.Name,

                    (
                        SELECT COUNT(*)
                        FROM SIKAI.Notes n
                        WHERE n.SubjectID = s.SubjectID
                          AND n.OwnerRole = 'TEACHER'
                          AND n.OwnerID = :teacherId
                    ) AS NoteCount,

                    (
                        SELECT COUNT(*)
                        FROM SIKAI.Tasks t
                        WHERE t.SubjectID = s.SubjectID
                          AND t.OwnerRole = 'TEACHER'
                          AND t.OwnerID = :teacherId
                    ) AS TaskCount,

                    (
                        SELECT COUNT(*)
                        FROM SIKAI.Resources r
                        WHERE r.SubjectID = s.SubjectID
                          AND r.OwnerRole = 'TEACHER'
                          AND r.OwnerID = :teacherId
                    ) AS ResourceCount

                FROM SIKAI.Subjects s

                WHERE s.SubjectID IN
                (
                    SELECT c.SubjectID
                    FROM SIKAI.SikaiClasses c
                    WHERE c.TeacherID = :teacherId
                      AND c.SubjectID IS NOT NULL
                )

                ORDER BY s.Code";

			List<SubjectModel> list =
				new List<SubjectModel>();

			using (OracleConnection conn = await Db.OpenAsync())
			using (OracleCommand cmd = new OracleCommand(sql, conn))
			{
				cmd.BindByName = true;

				cmd.Parameters.Add(
					"teacherId",
					teacher.Id);

				using (OracleDataReader reader =
					   await cmd.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						list.Add(new SubjectModel
						{
							Id = Convert.ToInt64(
								reader.GetValue(0)),

							Code = reader.GetString(1),

							Name = reader.GetString(2),

							NoteCount = Convert.ToInt32(
								reader.GetValue(3)),

							TaskCount = Convert.ToInt32(
								reader.GetValue(4)),

							ResourceCount = Convert.ToInt32(
								reader.GetValue(5))
						});
					}
				}
			}

			return list;
		}


		
		
		

		public static async Task<List<TaskModel>> GetTeacherAssignmentsAsync(
			Account teacher)
		{
			if (teacher == null)
				throw new ArgumentNullException("teacher");

			const string sql = @"
                SELECT
                    t.TaskID,
                    t.SubjectID,
                    s.Code,
                    s.Name,
                    t.Title,
                    t.Description,
                    t.Status,
                    t.Priority,
                    t.TaskType,
                    t.DueDate

                FROM SIKAI.Tasks t

                JOIN SIKAI.Subjects s
                    ON s.SubjectID = t.SubjectID

                WHERE t.OwnerRole = 'TEACHER'
                  AND t.OwnerID = :teacherId

                ORDER BY t.DueDate";

			List<TaskModel> list =
				new List<TaskModel>();

			using (OracleConnection conn = await Db.OpenAsync())
			using (OracleCommand cmd = new OracleCommand(sql, conn))
			{
				cmd.BindByName = true;

				cmd.Parameters.Add(
					"teacherId",
					teacher.Id);

				using (OracleDataReader reader =
					   await cmd.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						list.Add(new TaskModel
						{
							Id = Convert.ToInt64(
								reader.GetValue(0)),

							SubjectId = Convert.ToInt64(
								reader.GetValue(1)),

							SubjectCode =
								reader.GetString(2),

							SubjectName =
								reader.GetString(3),

							Title =
								reader.GetString(4),

							Description =
								reader.IsDBNull(5)
									? ""
									: reader.GetString(5),

							Status =
								reader.GetString(6),

							Priority =
								reader.GetString(7),

							TaskType =
								reader.GetString(8),

							DueDate =
								reader.GetDateTime(9)
						});
					}
				}
			}

			return list;
		}


		
		
		

		public static async Task<List<ResourceModel>> GetTeacherMaterialsAsync(
			Account teacher)
		{
			if (teacher == null)
				throw new ArgumentNullException("teacher");

			const string sql = @"
                SELECT
                    r.ResourceID,
                    r.SubjectID,
                    s.Code,
                    s.Name,
                    r.Title,
                    r.Description,
                    r.ResourceType,
                    r.Url,
                    r.UploadedAt

                FROM SIKAI.Resources r

                JOIN SIKAI.Subjects s
                    ON s.SubjectID = r.SubjectID

                WHERE r.OwnerRole = 'TEACHER'
                  AND r.OwnerID = :teacherId

                ORDER BY r.UploadedAt DESC";

			List<ResourceModel> list =
				new List<ResourceModel>();

			using (OracleConnection conn = await Db.OpenAsync())
			using (OracleCommand cmd = new OracleCommand(sql, conn))
			{
				cmd.BindByName = true;

				cmd.Parameters.Add(
					"teacherId",
					teacher.Id);

				using (OracleDataReader reader =
					   await cmd.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						list.Add(new ResourceModel
						{
							Id = Convert.ToInt64(
								reader.GetValue(0)),

							SubjectId = Convert.ToInt64(
								reader.GetValue(1)),

							SubjectCode =
								reader.GetString(2),

							SubjectName =
								reader.GetString(3),

							Title =
								reader.GetString(4),

							Description =
								reader.IsDBNull(5)
									? ""
									: reader.GetString(5),

							ResourceType =
								reader.GetString(6),

							Url =
								reader.IsDBNull(7)
									? ""
									: reader.GetString(7),

							UploadedAt =
								reader.GetDateTime(8)
						});
					}
				}
			}

			return list;
		}


		
		
		

		public static async Task<List<string>> GetRecentActivityAsync(
			Account teacher)
		{
			if (teacher == null)
				throw new ArgumentNullException("teacher");

			List<string> activities =
				new List<string>();

			

			const string sql = @"
                SELECT ActivityText
                FROM
                (
                    SELECT
                        'Created assignment: ' || t.Title
                        AS ActivityText,
                        t.DueDate AS ActivityDate
                    FROM SIKAI.Tasks t
                    WHERE t.OwnerRole = 'TEACHER'
                      AND t.OwnerID = :teacherId

                    UNION ALL

                    SELECT
                        'Added learning material: ' || r.Title
                        AS ActivityText,
                        r.UploadedAt AS ActivityDate
                    FROM SIKAI.Resources r
                    WHERE r.OwnerRole = 'TEACHER'
                      AND r.OwnerID = :teacherId

                    UNION ALL

                    SELECT
                        'Created note: ' || n.Title
                        AS ActivityText,
                        n.CreatedAt AS ActivityDate
                    FROM SIKAI.Notes n
                    WHERE n.OwnerRole = 'TEACHER'
                      AND n.OwnerID = :teacherId
                )

                ORDER BY ActivityDate DESC";

			using (OracleConnection conn = await Db.OpenAsync())
			using (OracleCommand cmd = new OracleCommand(sql, conn))
			{
				cmd.BindByName = true;

				cmd.Parameters.Add(
					"teacherId",
					teacher.Id);

				using (OracleDataReader reader =
					   await cmd.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						activities.Add(
							reader.GetString(0));

						if (activities.Count >= 10)
							break;
					}
				}
			}

			return activities;
		}


		
		
		

		public static async Task<Account> GetTeacherAsync(
			long teacherId)
		{
			const string sql = @"
                SELECT
                    TeacherID,
                    FullName,
                    Email,
                    Password,
                    SecurityQuestion,
                    SecurityAnswer

                FROM SIKAI.Teacher

                WHERE TeacherID = :teacherId";

			using (OracleConnection conn = await Db.OpenAsync())
			using (OracleCommand cmd = new OracleCommand(sql, conn))
			{
				cmd.BindByName = true;

				cmd.Parameters.Add(
					"teacherId",
					teacherId);

				using (OracleDataReader reader =
					   await cmd.ExecuteReaderAsync())
				{
					if (!await reader.ReadAsync())
						return null;

					return new Account
					{
						Role = Role.Teacher,

						Id = Convert.ToInt64(
							reader.GetValue(0)),

						FullName =
							reader.GetString(1),

						Email =
							reader.GetString(2),

						PasswordEntry =
							reader.GetString(3),

						SecurityQuestion =
							reader.IsDBNull(4)
								? null
								: reader.GetString(4),

						SecurityAnswerEntry =
							reader.IsDBNull(5)
								? null
								: reader.GetString(5)
					};
				}
			}
		}

		
		
		

		public static async Task<List<TeacherClassItem>> GetTeacherClassesAsync(
			Account teacher)
		{
			if (teacher == null)
				throw new ArgumentNullException("teacher");

			const string sql = @"
				SELECT c.ClassID,
				       c.ClassCode,
				       c.ClassName,
				       (SELECT COUNT(*)
				          FROM SIKAI.ClassEnrollments e
				         WHERE e.ClassID = c.ClassID)
				  FROM SIKAI.SikaiClasses c
				 WHERE c.TeacherID = :teacherId
				 ORDER BY c.ClassName";

			List<TeacherClassItem> list = new List<TeacherClassItem>();

			using (OracleConnection conn = await Db.OpenAsync())
			using (OracleCommand cmd = new OracleCommand(sql, conn))
			{
				cmd.BindByName = true;
				cmd.Parameters.Add("teacherId", teacher.Id);

				using (OracleDataReader r = await cmd.ExecuteReaderAsync())
				{
					while (await r.ReadAsync())
					{
						list.Add(new TeacherClassItem
						{
							ClassId = Convert.ToInt64(r.GetValue(0)),
							ClassCode = r.GetString(1),
							ClassName = r.GetString(2),
							StudentCount = Convert.ToInt32(r.GetValue(3))
						});
					}
				}
			}

			return list;
		}

		public static async Task<List<ClassStudentItem>> GetClassStudentsAsync(
			Account teacher,
			long classId)
		{
			if (teacher == null)
				throw new ArgumentNullException("teacher");

			
			const string sql = @"
				SELECT s.StudentID,
				       s.FullName,
				       s.Email
				  FROM SIKAI.ClassEnrollments e
				  JOIN SIKAI.Student s ON s.StudentID = e.StudentID
				  JOIN SIKAI.SikaiClasses c ON c.ClassID = e.ClassID
				 WHERE e.ClassID = :classId
				   AND c.TeacherID = :teacherId
				 ORDER BY s.FullName";

			List<ClassStudentItem> list = new List<ClassStudentItem>();

			using (OracleConnection conn = await Db.OpenAsync())
			using (OracleCommand cmd = new OracleCommand(sql, conn))
			{
				cmd.BindByName = true;
				cmd.Parameters.Add("classId", classId);
				cmd.Parameters.Add("teacherId", teacher.Id);

				using (OracleDataReader r = await cmd.ExecuteReaderAsync())
				{
					while (await r.ReadAsync())
					{
						list.Add(new ClassStudentItem
						{
							StudentId = Convert.ToInt64(r.GetValue(0)),
							FullName = r.GetString(1),
							Email = r.GetString(2)
						});
					}
				}
			}

			return list;
		}

		
		
		public static async Task<bool> RemoveStudentFromClassAsync(
			Account teacher,
			long classId,
			long studentId)
		{
			if (teacher == null)
				throw new ArgumentNullException("teacher");

			const string sql = @"
				DELETE FROM SIKAI.ClassEnrollments
				 WHERE ClassID = :classId
				   AND StudentID = :studentId
				   AND ClassID IN (SELECT ClassID
				                     FROM SIKAI.SikaiClasses
				                    WHERE TeacherID = :teacherId)";

			using (OracleConnection conn = await Db.OpenAsync())
			using (OracleCommand cmd = new OracleCommand(sql, conn))
			{
				cmd.BindByName = true;
				cmd.Parameters.Add("classId", classId);
				cmd.Parameters.Add("studentId", studentId);
				cmd.Parameters.Add("teacherId", teacher.Id);

				int rows = await cmd.ExecuteNonQueryAsync();
				return rows > 0;
			}
		}

	}
}
