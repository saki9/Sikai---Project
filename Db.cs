using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace Sikai_Project
{
    public enum Role { Student, Teacher }

    public class Account
    {
        public Role Role { get; set; }
        public long Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordEntry { get; set; }
        public string SecurityQuestion { get; set; }
        public string SecurityAnswerEntry { get; set; }
    }


    public class MessageContact
    {
        public long Id { get; set; }
        public Role Role { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Display { get { return FullName + "  ·  " + Email; } }
    }

    public class MessageItem
    {
        public long Id { get; set; }
        public Role SenderRole { get; set; }
        public long SenderId { get; set; }
        public string SenderName { get; set; }
        public string Body { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsMine { get; set; }
    }

    public class NotificationItem
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }

    public class SubmissionItem
    {
        public long Id { get; set; }
        public long TaskId { get; set; }
        public long StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public string TaskTitle { get; set; }
        public string SubjectCode { get; set; }
        public string SubmissionText { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string Status { get; set; }
        public string Feedback { get; set; }
    }

    public class SubjectModel
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int NoteCount { get; set; }
        public int TaskCount { get; set; }
        public int ResourceCount { get; set; }
    }

	public class ClassModel
	{
		public long Id { get; set; }
		public string Code { get; set; }
		public string Name { get; set; }
		public long TeacherId { get; set; }
		public string TeacherName { get; set; }
		public DateTime EnrolledAt { get; set; }
	}
	public class NoteModel
    {
        public long Id { get; set; }
        public long SubjectId { get; set; }
        public string SubjectCode { get; set; }
        public string SubjectName { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TaskModel
    {
        public long Id { get; set; }
        public long SubjectId { get; set; }
        public string SubjectCode { get; set; }
        public string SubjectName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string OwnerRole { get; set; }
        public long OwnerId { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string TaskType { get; set; }
        public DateTime DueDate { get; set; }
    }

    public class ResourceModel
    {
        public long Id { get; set; }
        public long SubjectId { get; set; }
        public string SubjectCode { get; set; }
        public string SubjectName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ResourceType { get; set; }
        public string Url { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class StudyPlanModel
    {
        public long Id { get; set; }
        public long SubjectId { get; set; }
        public string SubjectCode { get; set; }
        public string SubjectName { get; set; }
        public string Description { get; set; }
        public DateTime PlanDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

    public class DashboardData
    {
        public int NoteCount { get; set; }
        public int TaskCount { get; set; }
        public int CompletedTaskCount { get; set; }
        public int SubjectCount { get; set; }
        public decimal StudyHours { get; set; }
        public List<NoteModel> RecentNotes { get; set; }
        public List<TaskModel> TodayTasks { get; set; }
        public List<StudyPlanModel> TodayPlans { get; set; }
    }

    public static class Session
    {
        public static Account CurrentUser { get; set; }
    }

    public static class Db
    {
        private const string Schema = "SIKAI";
        private static string Table(Role role) { return Schema + "." + (role == Role.Teacher ? "Teacher" : "Student"); }
        private static string IdColumn(Role role) { return role == Role.Teacher ? "TeacherID" : "StudentID"; }

        public static async Task<OracleConnection> OpenAsync()
        {
            ConnectionStringSettings setting = ConfigurationManager.ConnectionStrings["OracleDb"];
            if (setting == null)
                throw new ConfigurationErrorsException("Connection string 'OracleDb' was not found in App.config.");

            OracleConnection conn = new OracleConnection(setting.ConnectionString);
            await conn.OpenAsync();

            
            
            
            using (OracleCommand identity = new OracleCommand(
                "SELECT SYS_CONTEXT('USERENV','SESSION_USER') || ' @ ' || SYS_CONTEXT('USERENV','SERVICE_NAME') FROM DUAL", conn))
            {
                string current = Convert.ToString(await identity.ExecuteScalarAsync());
                if (!current.StartsWith("SIKAI @ ORCLPDB", StringComparison.OrdinalIgnoreCase))
                {
                    conn.Dispose();
                    throw new InvalidOperationException(
                        "SIKAI is connected to an unexpected Oracle session: " + current +
                        ". Expected SIKAI @ ORCLPDB. Check App.config and the active Visual Studio build.");
                }
            }

            return conn;
        }

        public static async Task<Account> FindAccountAsync(Role role, string email)
        {
            string sql = "SELECT " + IdColumn(role) + ", FullName, Email, Password, SecurityQuestion, SecurityAnswer " +
                         "FROM " + Table(role) + " WHERE LOWER(Email) = :email";
            using (OracleConnection conn = await OpenAsync())
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.BindByName = true;
                cmd.Parameters.Add("email", email.Trim().ToLowerInvariant());
                using (OracleDataReader r = await cmd.ExecuteReaderAsync())
                {
                    if (!await r.ReadAsync()) return null;
                    return ReadAccount(r, role);
                }
            }
        }

        private static Account ReadAccount(OracleDataReader r, Role role)
        {
            return new Account
            {
                Role = role,
                Id = Convert.ToInt64(r.GetValue(0)),
                FullName = r.GetString(1),
                Email = r.GetString(2),
                PasswordEntry = r.GetString(3),
                SecurityQuestion = r.IsDBNull(4) ? null : r.GetString(4),
                SecurityAnswerEntry = r.IsDBNull(5) ? null : r.GetString(5)
            };
        }

        public static async Task UpdatePasswordAsync(Role role, long id, string newPasswordEntry)
        {
            string sql = "UPDATE " + Table(role) + " SET Password = :pw WHERE " + IdColumn(role) + " = :id";
            using (OracleConnection conn = await OpenAsync())
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.BindByName = true;
                cmd.Parameters.Add("pw", newPasswordEntry);
                cmd.Parameters.Add("id", id);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        
        
        
        private const string MySubjectIds = @"(SELECT c.SubjectID FROM SIKAI.SikaiClasses c
                WHERE c.SubjectID IS NOT NULL
                  AND ((:ownerRole = 'TEACHER' AND c.TeacherID = :ownerId)
                    OR (:ownerRole = 'STUDENT' AND c.ClassID IN
                         (SELECT e.ClassID FROM SIKAI.ClassEnrollments e WHERE e.StudentID = :ownerId))))";

        private static void AddOwner(OracleCommand cmd, Account account)
        {
            cmd.Parameters.Add("ownerRole", account.Role.ToString().ToUpperInvariant());
            cmd.Parameters.Add("ownerId", account.Id);
        }

        public static async Task<List<SubjectModel>> GetSubjectsAsync(Account account)
        {
            await EnsureClassSubjectsAsync();
            string sql = @"SELECT s.SubjectID, s.Code, s.Name,
                (SELECT COUNT(*) FROM SIKAI.Notes n WHERE n.SubjectID=s.SubjectID AND n.OwnerRole=:ownerRole AND n.OwnerID=:ownerId),
                (SELECT COUNT(*) FROM SIKAI.Tasks t WHERE t.SubjectID=s.SubjectID AND t.OwnerRole=:ownerRole AND t.OwnerID=:ownerId),
                (SELECT COUNT(*) FROM SIKAI.Resources r WHERE r.SubjectID=s.SubjectID AND r.OwnerRole=:ownerRole AND r.OwnerID=:ownerId)
                FROM SIKAI.Subjects s
                WHERE s.SubjectID IN " + MySubjectIds + @"
                ORDER BY s.Code";
            List<SubjectModel> list = new List<SubjectModel>();
            using (OracleConnection conn = await OpenAsync())
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.BindByName = true;
                AddOwner(cmd, account);
                using (OracleDataReader r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                    {
                        list.Add(new SubjectModel {
                            Id=Convert.ToInt64(r.GetValue(0)), Code=r.GetString(1), Name=r.GetString(2),
                            NoteCount=Convert.ToInt32(r.GetValue(3)), TaskCount=Convert.ToInt32(r.GetValue(4)),
                            ResourceCount=Convert.ToInt32(r.GetValue(5))
                        });
                    }
                }
            }
            return list;
        }

        public static async Task<List<NoteModel>> GetNotesAsync(Account account, long? subjectId = null)
        {
            string sql = @"SELECT n.NoteID,n.SubjectID,s.Code,s.Name,n.Title,n.Content,n.CreatedAt
                           FROM SIKAI.Notes n JOIN SIKAI.Subjects s ON s.SubjectID=n.SubjectID
                           WHERE n.OwnerRole=:ownerRole AND n.OwnerID=:ownerId AND n.SubjectID IN " + MySubjectIds;
            if (subjectId.HasValue) sql += " AND n.SubjectID=:subjectId";
            sql += " ORDER BY n.CreatedAt DESC";
            List<NoteModel> list = new List<NoteModel>();
            using (OracleConnection conn=await OpenAsync())
            using (OracleCommand cmd=new OracleCommand(sql,conn))
            {
                cmd.BindByName=true; AddOwner(cmd,account);
                if(subjectId.HasValue) cmd.Parameters.Add("subjectId",subjectId.Value);
                using(OracleDataReader r=await cmd.ExecuteReaderAsync())
                while(await r.ReadAsync())
                    list.Add(new NoteModel { Id=Convert.ToInt64(r.GetValue(0)), SubjectId=Convert.ToInt64(r.GetValue(1)),
                        SubjectCode=r.GetString(2),SubjectName=r.GetString(3),Title=r.GetString(4),
                        Content=r.IsDBNull(5)?"":r.GetValue(5).ToString(),CreatedAt=r.GetDateTime(6)});
            }
            return list;
        }

        public static async Task<List<TaskModel>> GetTasksAsync(Account account, long? subjectId = null)
        {
            string sql=@"SELECT t.TaskID,t.SubjectID,s.Code,s.Name,t.Title,t.Description,t.Status,t.Priority,t.TaskType,t.DueDate,t.OwnerRole,t.OwnerID
                         FROM SIKAI.Tasks t JOIN SIKAI.Subjects s ON s.SubjectID=t.SubjectID
                         WHERE ((t.OwnerRole=:ownerRole AND t.OwnerID=:ownerId) OR (:ownerRole='STUDENT' AND t.OwnerRole='TEACHER')) AND t.SubjectID IN " + MySubjectIds;
            if(subjectId.HasValue) sql+=" AND t.SubjectID=:subjectId";
            sql+=" ORDER BY t.DueDate";
            List<TaskModel> list=new List<TaskModel>();
            using(OracleConnection conn=await OpenAsync())
            using(OracleCommand cmd=new OracleCommand(sql,conn))
            {
                cmd.BindByName=true; AddOwner(cmd,account);
                if(subjectId.HasValue) cmd.Parameters.Add("subjectId",subjectId.Value);
                using(OracleDataReader r=await cmd.ExecuteReaderAsync())
                while(await r.ReadAsync())
                    list.Add(new TaskModel { Id=Convert.ToInt64(r.GetValue(0)),SubjectId=Convert.ToInt64(r.GetValue(1)),
                        SubjectCode=r.GetString(2),SubjectName=r.GetString(3),Title=r.GetString(4),
                        Description=r.IsDBNull(5)?"":r.GetString(5),Status=r.GetString(6),Priority=r.GetString(7),
                        TaskType=r.GetString(8),DueDate=r.GetDateTime(9),OwnerRole=r.GetString(10),OwnerId=Convert.ToInt64(r.GetValue(11))});
            }
            return list;
        }

        public static async Task UpdateTaskStatusAsync(Account account,long taskId,string status)
        {
            const string sql="UPDATE SIKAI.Tasks SET Status=:status WHERE TaskID=:taskId AND OwnerRole=:ownerRole AND OwnerID=:ownerId";
            using(OracleConnection conn=await OpenAsync())
            using(OracleCommand cmd=new OracleCommand(sql,conn))
            {
                cmd.BindByName=true; cmd.Parameters.Add("status",status);cmd.Parameters.Add("taskId",taskId);AddOwner(cmd,account);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task<List<ResourceModel>> GetResourcesAsync(Account account,long? subjectId=null)
        {
            string sql=@"SELECT r.ResourceID,r.SubjectID,s.Code,s.Name,r.Title,r.Description,r.ResourceType,r.Url,r.UploadedAt
                         FROM SIKAI.Resources r JOIN SIKAI.Subjects s ON s.SubjectID=r.SubjectID
                         WHERE ((r.OwnerRole=:ownerRole AND r.OwnerID=:ownerId) OR (:ownerRole='STUDENT' AND r.OwnerRole='TEACHER')) AND r.SubjectID IN " + MySubjectIds;
            if(subjectId.HasValue) sql+=" AND r.SubjectID=:subjectId";
            sql+=" ORDER BY r.UploadedAt DESC";
            List<ResourceModel> list=new List<ResourceModel>();
            using(OracleConnection conn=await OpenAsync())
            using(OracleCommand cmd=new OracleCommand(sql,conn))
            {
                cmd.BindByName=true;AddOwner(cmd,account);
                if(subjectId.HasValue)cmd.Parameters.Add("subjectId",subjectId.Value);
                using(OracleDataReader r=await cmd.ExecuteReaderAsync())
                while(await r.ReadAsync())
                    list.Add(new ResourceModel {Id=Convert.ToInt64(r.GetValue(0)),SubjectId=Convert.ToInt64(r.GetValue(1)),
                        SubjectCode=r.GetString(2),SubjectName=r.GetString(3),Title=r.GetString(4),
                        Description=r.IsDBNull(5)?"":r.GetString(5),ResourceType=r.GetString(6),
                        Url=r.IsDBNull(7)?"":r.GetString(7),UploadedAt=r.GetDateTime(8)});
            }
            return list;
        }

        public static async Task<List<StudyPlanModel>> GetStudyPlansAsync(Account account, DateTime? date=null)
        {
            string sql=@"SELECT p.PlanID,p.SubjectID,s.Code,s.Name,p.Description,p.PlanDate,p.StartTime,p.EndTime
                         FROM SIKAI.StudyPlans p JOIN SIKAI.Subjects s ON s.SubjectID=p.SubjectID
                         WHERE p.OwnerRole=:ownerRole AND p.OwnerID=:ownerId AND p.SubjectID IN " + MySubjectIds;
            if(date.HasValue)sql+=" AND TRUNC(p.PlanDate)=TRUNC(:planDate)";
            sql+=" ORDER BY p.PlanDate,p.StartTime";
            List<StudyPlanModel> list=new List<StudyPlanModel>();
            using(OracleConnection conn=await OpenAsync())
            using(OracleCommand cmd=new OracleCommand(sql,conn))
            {
                cmd.BindByName=true;AddOwner(cmd,account);
                if(date.HasValue)cmd.Parameters.Add("planDate",date.Value);
                using(OracleDataReader r=await cmd.ExecuteReaderAsync())
                while(await r.ReadAsync())
                    list.Add(new StudyPlanModel {Id=Convert.ToInt64(r.GetValue(0)),SubjectId=Convert.ToInt64(r.GetValue(1)),
                        SubjectCode=r.GetString(2),SubjectName=r.GetString(3),Description=r.GetString(4),
                        PlanDate=r.GetDateTime(5),StartTime=r.GetString(6),EndTime=r.GetString(7)});
            }
            return list;
        }

        public static async Task AddStudyPlanAsync(Account account,long subjectId,string description,DateTime date,string startTime,string endTime)
        {
            const string sql=@"INSERT INTO SIKAI.StudyPlans(PlanID,SubjectID,OwnerRole,OwnerID,Description,PlanDate,StartTime,EndTime,StudyHours)
                               VALUES(SIKAI.studyplan_seq.NEXTVAL,:subjectId,:ownerRole,:ownerId,:description,:planDate,:startTime,:endTime,:studyHours)";
            using(OracleConnection conn=await OpenAsync())
            using(OracleCommand cmd=new OracleCommand(sql,conn))
            {
                cmd.BindByName=true;cmd.Parameters.Add("subjectId",subjectId);AddOwner(cmd,account);
                cmd.Parameters.Add("description",description);cmd.Parameters.Add("planDate",date.Date);
                cmd.Parameters.Add("startTime",startTime);cmd.Parameters.Add("endTime",endTime);
                TimeSpan st, et; decimal hours = 0;
                if (TimeSpan.TryParse(startTime, out st) && TimeSpan.TryParse(endTime, out et) && et > st)
                    hours = (decimal)(et - st).TotalHours;
                cmd.Parameters.Add("studyHours", hours);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task<DashboardData> GetDashboardAsync(Account account)
        {
            await EnsureClassSubjectsAsync();
            DashboardData d = new DashboardData
            {
                RecentNotes = new List<NoteModel>(),
                TodayTasks = new List<TaskModel>(),
                TodayPlans = new List<StudyPlanModel>()
            };

            
            
            
            d.NoteCount = await GetCountAsync(account,
                "Dashboard notes",
                "SELECT COUNT(*) FROM SIKAI.Notes WHERE OwnerRole=:ownerRole AND OwnerID=:ownerId AND SubjectID IN " + MySubjectIds);

            d.TaskCount = await GetCountAsync(account,
                "Dashboard tasks",
                "SELECT COUNT(*) FROM SIKAI.Tasks WHERE OwnerRole=:ownerRole AND OwnerID=:ownerId AND SubjectID IN " + MySubjectIds);

            d.CompletedTaskCount = await GetCountAsync(account,
                "Dashboard completed tasks",
                "SELECT COUNT(*) FROM SIKAI.Tasks WHERE OwnerRole=:ownerRole AND OwnerID=:ownerId AND UPPER(Status)='COMPLETED' AND SubjectID IN " + MySubjectIds);

            d.SubjectCount = await GetCountAsync(account,
                "Dashboard subjects",
                "SELECT COUNT(*) FROM SIKAI.Subjects s WHERE s.SubjectID IN " + MySubjectIds);

            d.StudyHours = await GetDecimalAsync(account,
                "Dashboard study hours",
                "SELECT NVL(SUM(StudyHours),0) FROM SIKAI.StudyPlans WHERE OwnerRole=:ownerRole AND OwnerID=:ownerId AND SubjectID IN " + MySubjectIds);

            List<NoteModel> notes = await GetNotesAsync(account);
            for (int i = 0; i < notes.Count && i < 5; i++)
                d.RecentNotes.Add(notes[i]);

            List<TaskModel> tasks = await GetTasksAsync(account);
            tasks.RemoveAll(x => x.DueDate.Date != DateTime.Today);
            if (tasks.Count > 5) tasks = tasks.GetRange(0, 5);
            d.TodayTasks = tasks;

            d.TodayPlans = await GetStudyPlansAsync(account, DateTime.Today);
            return d;
        }

        private static async Task<int> GetCountAsync(Account account, string operation, string sql)
        {
            try
            {
                using (OracleConnection conn = await OpenAsync())
                using (OracleCommand cmd = new OracleCommand(sql, conn))
                {
                    cmd.BindByName = true;
                    AddOwner(cmd, account);
                    object value = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(value);
                }
            }
            catch (OracleException ex)
            {
                throw new InvalidOperationException(
                    operation + " failed. ORA-" + ex.Number.ToString("00000") + ": " + ex.Message, ex);
            }
        }

        private static async Task<decimal> GetDecimalAsync(Account account, string operation, string sql)
        {
            try
            {
                using (OracleConnection conn = await OpenAsync())
                using (OracleCommand cmd = new OracleCommand(sql, conn))
                {
                    cmd.BindByName = true;
                    AddOwner(cmd, account);
                    object value = await cmd.ExecuteScalarAsync();
                    return Convert.ToDecimal(value);
                }
            }
            catch (OracleException ex)
            {
                throw new InvalidOperationException(
                    operation + " failed. ORA-" + ex.Number.ToString("00000") + ": " + ex.Message, ex);
            }
        }

        public static async Task<List<NoteModel>> GetSubjectRecentNotesAsync(Account account,long subjectId){return await GetNotesAsync(account,subjectId);}
        public static async Task<List<TaskModel>> GetSubjectTasksAsync(Account account,long subjectId){return await GetTasksAsync(account,subjectId);}
        public static async Task<List<ResourceModel>> GetSubjectResourcesAsync(Account account,long subjectId){return await GetResourcesAsync(account,subjectId);}

        public static async Task<int[]> GetActivityCountsAsync(Account account)
        {
            string sql=@"SELECT
                (SELECT COUNT(*) FROM SIKAI.Notes WHERE OwnerRole=:ownerRole AND OwnerID=:ownerId AND SubjectID IN "+MySubjectIds+@"),
                (SELECT COUNT(*) FROM SIKAI.Tasks WHERE OwnerRole=:ownerRole AND OwnerID=:ownerId AND UPPER(Status)='COMPLETED' AND SubjectID IN "+MySubjectIds+@"),
                (SELECT COUNT(*) FROM SIKAI.Subjects s WHERE s.SubjectID IN "+MySubjectIds+@"),
                (SELECT NVL(SUM(StudyHours),0) FROM SIKAI.StudyPlans WHERE OwnerRole=:ownerRole AND OwnerID=:ownerId AND SubjectID IN "+MySubjectIds+@")
                FROM dual";
            using(OracleConnection conn=await OpenAsync())
            using(OracleCommand cmd=new OracleCommand(sql,conn))
            {
                cmd.BindByName=true;AddOwner(cmd,account);
                using(OracleDataReader r=await cmd.ExecuteReaderAsync())
                {await r.ReadAsync();return new[]{Convert.ToInt32(r.GetValue(0)),Convert.ToInt32(r.GetValue(1)),Convert.ToInt32(r.GetValue(2)),Convert.ToInt32(Convert.ToDecimal(r.GetValue(3)))};}
            }
        }
        
        
        

        public static string Describe(Exception ex, string fallback)
        {
            OracleException oracle = ex as OracleException;
            if (oracle != null)
                return "Database error ORA-" + oracle.Number.ToString("00000") + ": " + oracle.Message;
            return string.IsNullOrWhiteSpace(ex == null ? null : ex.Message) ? fallback : ex.Message;
        }

        public static async Task CreateClassAsync(Account teacher, string className, string classCode)
        {
            RequireTeacher(teacher);
            if (string.IsNullOrWhiteSpace(className) || string.IsNullOrWhiteSpace(classCode))
                throw new ArgumentException("Enter a class name and class code.");

            const string sql = @"INSERT INTO SIKAI.SikaiClasses (ClassID, ClassCode, ClassName, TeacherID)
                                 VALUES (SIKAI.class_seq.NEXTVAL, :classCode, :className, :teacherId)";
            using (OracleConnection conn = await OpenAsync())
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.BindByName = true;
                cmd.Parameters.Add("classCode", classCode.Trim().ToUpperInvariant());
                cmd.Parameters.Add("className", className.Trim());
                cmd.Parameters.Add("teacherId", teacher.Id);
                await cmd.ExecuteNonQueryAsync();
            }
            await EnsureClassSubjectsAsync();
        }

        
        
        

        public static async Task AddTeacherResourceAsync(Account teacher, long subjectId, string title,
            string description, string resourceType, string url)
        {
            RequireTeacher(teacher);
            const string sql = @"INSERT INTO SIKAI.Resources
                (ResourceID, SubjectID, OwnerRole, OwnerID, Title, Description, ResourceType, Url, UploadedAt)
                SELECT SIKAI.resources_seq.NEXTVAL, :subjectId, 'TEACHER', :teacherId, :title, :description, :resourceType, :url, SYSDATE
                FROM dual
                WHERE EXISTS (SELECT 1 FROM SIKAI.SikaiClasses c WHERE c.SubjectID=:subjectId AND c.TeacherID=:teacherId)";
            using (OracleConnection conn = await OpenAsync())
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.BindByName = true;
                cmd.Parameters.Add("subjectId", subjectId);
                cmd.Parameters.Add("teacherId", teacher.Id);
                cmd.Parameters.Add("title", title.Trim());
                cmd.Parameters.Add("description", description ?? "");
                cmd.Parameters.Add("resourceType", resourceType.Trim());
                cmd.Parameters.Add("url", (url ?? "").Trim());
                int rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0) throw new InvalidOperationException("Select a class that belongs to your teacher account.");
                await NotifyClassStudentsAsync(conn, teacher.Id, subjectId, "New learning resource: " + title.Trim(), description ?? "");
            }
        }

        public static async Task AddTeacherAssignmentAsync(Account teacher, long subjectId, string title,
            string description, DateTime dueDate, string priority)
        {
            RequireTeacher(teacher);
            const string sql = @"INSERT INTO SIKAI.Tasks
                (TaskID, SubjectID, OwnerRole, OwnerID, Title, Description, Status, Priority, TaskType, DueDate)
                SELECT SIKAI.tasks_seq.NEXTVAL, :subjectId, 'TEACHER', :teacherId, :title, :description, 'Assigned', :priority, 'Assignment', :dueDate
                FROM dual
                WHERE EXISTS (SELECT 1 FROM SIKAI.SikaiClasses c WHERE c.SubjectID=:subjectId AND c.TeacherID=:teacherId)";
            using (OracleConnection conn = await OpenAsync())
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.BindByName = true;
                cmd.Parameters.Add("subjectId", subjectId);
                cmd.Parameters.Add("teacherId", teacher.Id);
                cmd.Parameters.Add("title", title.Trim());
                cmd.Parameters.Add("description", description ?? "");
                cmd.Parameters.Add("priority", string.IsNullOrWhiteSpace(priority) ? "Medium" : priority);
                cmd.Parameters.Add("dueDate", dueDate.Date);
                int rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0) throw new InvalidOperationException("Select a class that belongs to your teacher account.");
                await NotifyClassStudentsAsync(conn, teacher.Id, subjectId, "New assignment: " + title.Trim(),
                    "Due " + dueDate.ToString("dd MMM yyyy") + ". " + (description ?? ""));
            }
        }

        private static void RequireTeacher(Account account)
        {
            if (account == null || account.Role != Role.Teacher)
                throw new InvalidOperationException("Sign in with a teacher account to perform this action.");
        }

        private static async Task NotifyClassStudentsAsync(OracleConnection conn, long teacherId,
            long subjectId, string title, string body)
        {
            const string sql = @"INSERT INTO SIKAI.Notifications
                (NotificationID, RecipientRole, RecipientID, Title, Body, CreatedAt, IsRead)
                SELECT SIKAI.notification_seq.NEXTVAL, 'STUDENT', e.StudentID, :title, :body, SYSDATE, 'N'
                FROM SIKAI.ClassEnrollments e
                JOIN SIKAI.SikaiClasses c ON c.ClassID=e.ClassID
                WHERE c.TeacherID=:teacherId AND c.SubjectID=:subjectId";
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.BindByName = true;
                cmd.Parameters.Add("title", title);
                cmd.Parameters.Add("body", body);
                cmd.Parameters.Add("teacherId", teacherId);
                cmd.Parameters.Add("subjectId", subjectId);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        
        
        

        public static async Task<List<MessageContact>> GetMessageContactsAsync(Account account)
        {
            if (account == null) throw new ArgumentNullException("account");
            string sql;
            if (account.Role == Role.Teacher)
            {
                sql = @"SELECT DISTINCT s.StudentID, s.FullName, s.Email
                        FROM SIKAI.Student s
                        JOIN SIKAI.ClassEnrollments e ON e.StudentID=s.StudentID
                        JOIN SIKAI.SikaiClasses c ON c.ClassID=e.ClassID
                        WHERE c.TeacherID=:userId
                        ORDER BY s.FullName";
            }
            else
            {
                sql = @"SELECT DISTINCT t.TeacherID, t.FullName, t.Email
                        FROM SIKAI.Teacher t
                        JOIN SIKAI.SikaiClasses c ON c.TeacherID=t.TeacherID
                        JOIN SIKAI.ClassEnrollments e ON e.ClassID=c.ClassID
                        WHERE e.StudentID=:userId
                        ORDER BY t.FullName";
            }
            List<MessageContact> list = new List<MessageContact>();
            using (OracleConnection conn = await OpenAsync())
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.BindByName = true;
                cmd.Parameters.Add("userId", account.Id);
                using (OracleDataReader r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                        list.Add(new MessageContact
                        {
                            Id = Convert.ToInt64(r.GetValue(0)),
                            Role = account.Role == Role.Teacher ? Role.Student : Role.Teacher,
                            FullName = r.GetString(1), Email = r.GetString(2)
                        });
                }
            }
            return list;
        }

        public static async Task<List<MessageItem>> GetMessagesAsync(Account account, MessageContact contact)
        {
            if (account == null || contact == null) return new List<MessageItem>();
            const string sql = @"SELECT m.MessageID, m.SenderRole, m.SenderID,
                    CASE WHEN m.SenderRole='STUDENT' THEN s.FullName ELSE t.FullName END AS SenderName,
                    m.MessageBody, m.SentAt
                FROM SIKAI.Messages m
                LEFT JOIN SIKAI.Student s ON m.SenderRole='STUDENT' AND s.StudentID=m.SenderID
                LEFT JOIN SIKAI.Teacher t ON m.SenderRole='TEACHER' AND t.TeacherID=m.SenderID
                WHERE (m.SenderRole=:myRole AND m.SenderID=:myId AND m.ReceiverRole=:otherRole AND m.ReceiverID=:otherId)
                   OR (m.SenderRole=:otherRole AND m.SenderID=:otherId AND m.ReceiverRole=:myRole AND m.ReceiverID=:myId)
                ORDER BY m.SentAt, m.MessageID";
            List<MessageItem> list = new List<MessageItem>();
            using (OracleConnection conn = await OpenAsync())
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.BindByName = true;
                cmd.Parameters.Add("myRole", account.Role.ToString().ToUpperInvariant());
                cmd.Parameters.Add("myId", account.Id);
                cmd.Parameters.Add("otherRole", contact.Role.ToString().ToUpperInvariant());
                cmd.Parameters.Add("otherId", contact.Id);
                using (OracleDataReader r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                    {
                        Role senderRole = string.Equals(r.GetString(1), "TEACHER", StringComparison.OrdinalIgnoreCase) ? Role.Teacher : Role.Student;
                        list.Add(new MessageItem
                        {
                            Id = Convert.ToInt64(r.GetValue(0)), SenderRole = senderRole,
                            SenderId = Convert.ToInt64(r.GetValue(2)), SenderName = r.IsDBNull(3) ? "User" : r.GetString(3),
                            Body = r.IsDBNull(4) ? "" : r.GetValue(4).ToString(), SentAt = r.GetDateTime(5),
                            IsMine = senderRole == account.Role && Convert.ToInt64(r.GetValue(2)) == account.Id
                        });
                    }
                }
            }
            return list;
        }

        public static async Task SendMessageAsync(Account sender, MessageContact receiver, string body)
        {
            if (sender == null || receiver == null) throw new InvalidOperationException("Select a conversation first.");
            if (string.IsNullOrWhiteSpace(body)) throw new InvalidOperationException("Enter a message before sending.");
            if (body.Trim().Length > 4000) throw new InvalidOperationException("Messages must be 4,000 characters or fewer.");

            const string relationSql = @"SELECT COUNT(*) FROM SIKAI.SikaiClasses c
                WHERE ((:senderRole='TEACHER' AND c.TeacherID=:senderId AND EXISTS
                    (SELECT 1 FROM SIKAI.ClassEnrollments e WHERE e.ClassID=c.ClassID AND e.StudentID=:otherId))
                   OR (:senderRole='STUDENT' AND c.TeacherID=:otherId AND EXISTS
                    (SELECT 1 FROM SIKAI.ClassEnrollments e WHERE e.ClassID=c.ClassID AND e.StudentID=:senderId)))";
            using (OracleConnection conn = await OpenAsync())
            {
                using (OracleCommand relation = new OracleCommand(relationSql, conn))
                {
                    relation.BindByName = true;
                    relation.Parameters.Add("senderRole", sender.Role.ToString().ToUpperInvariant());
                    relation.Parameters.Add("senderId", sender.Id);
                    relation.Parameters.Add("otherId", receiver.Id);
                    int allowed = Convert.ToInt32(await relation.ExecuteScalarAsync());
                    if (allowed == 0) throw new InvalidOperationException("Messages are only available between teachers and students who share a class.");
                }

                const string insertSql = @"INSERT INTO SIKAI.Messages
                    (MessageID, SenderRole, SenderID, ReceiverRole, ReceiverID, MessageBody, SentAt, IsRead)
                    VALUES (SIKAI.message_seq.NEXTVAL, :senderRole, :senderId, :receiverRole, :receiverId, :body, SYSDATE, 'N')";
                using (OracleCommand cmd = new OracleCommand(insertSql, conn))
                {
                    cmd.BindByName = true;
                    cmd.Parameters.Add("senderRole", sender.Role.ToString().ToUpperInvariant());
                    cmd.Parameters.Add("senderId", sender.Id);
                    cmd.Parameters.Add("receiverRole", receiver.Role.ToString().ToUpperInvariant());
                    cmd.Parameters.Add("receiverId", receiver.Id);
                    cmd.Parameters.Add("body", body.Trim());
                    await cmd.ExecuteNonQueryAsync();
                }

                const string notificationSql = @"INSERT INTO SIKAI.Notifications
                    (NotificationID, RecipientRole, RecipientID, Title, Body, CreatedAt, IsRead)
                    VALUES (SIKAI.notification_seq.NEXTVAL, :role, :id, :title, :body, SYSDATE, 'N')";
                using (OracleCommand cmd = new OracleCommand(notificationSql, conn))
                {
                    cmd.BindByName = true;
                    cmd.Parameters.Add("role", receiver.Role.ToString().ToUpperInvariant());
                    cmd.Parameters.Add("id", receiver.Id);
                    cmd.Parameters.Add("title", "New message from " + sender.FullName);
                    cmd.Parameters.Add("body", body.Trim().Length > 180 ? body.Trim().Substring(0, 180) : body.Trim());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task<List<NotificationItem>> GetNotificationsAsync(Account account)
        {
            const string sql = @"SELECT NotificationID, Title, Body, CreatedAt, IsRead
                FROM (SELECT NotificationID, Title, Body, CreatedAt, IsRead
                      FROM SIKAI.Notifications WHERE RecipientRole=:role AND RecipientID=:id
                      ORDER BY CreatedAt DESC, NotificationID DESC)
                WHERE ROWNUM <= 30";
            List<NotificationItem> items = new List<NotificationItem>();
            using (OracleConnection conn = await OpenAsync())
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.BindByName = true;
                cmd.Parameters.Add("role", account.Role.ToString().ToUpperInvariant());
                cmd.Parameters.Add("id", account.Id);
                using (OracleDataReader r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                        items.Add(new NotificationItem
                        {
                            Id = Convert.ToInt64(r.GetValue(0)), Title = r.GetString(1),
                            Body = r.IsDBNull(2) ? "" : r.GetValue(2).ToString(), CreatedAt = r.GetDateTime(3),
                            IsRead = !r.IsDBNull(4) && string.Equals(r.GetString(4), "Y", StringComparison.OrdinalIgnoreCase)
                        });
                }
            }
            return items;
        }

        public static async Task MarkNotificationsReadAsync(Account account)
        {
            const string sql = @"UPDATE SIKAI.Notifications SET IsRead='Y'
                WHERE RecipientRole=:role AND RecipientID=:id AND IsRead='N'";
            using (OracleConnection conn = await OpenAsync())
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.BindByName = true;
                cmd.Parameters.Add("role", account.Role.ToString().ToUpperInvariant());
                cmd.Parameters.Add("id", account.Id);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        
        
        

        public static async Task SubmitAssignmentAsync(Account student, long taskId, string submissionText)
        {
            if (student == null || student.Role != Role.Student)
                throw new InvalidOperationException("Sign in as a student to submit an assignment.");
            if (string.IsNullOrWhiteSpace(submissionText))
                throw new InvalidOperationException("Enter your answer or a link to your work.");

            const string permissionSql = @"SELECT COUNT(*) FROM SIKAI.Tasks t
                WHERE t.TaskID=:taskId AND t.OwnerRole='TEACHER' AND t.SubjectID IN
                    (SELECT c.SubjectID FROM SIKAI.SikaiClasses c JOIN SIKAI.ClassEnrollments e ON e.ClassID=c.ClassID WHERE e.StudentID=:studentId)
                AND t.DueDate >= TRUNC(SYSDATE)";
            using (OracleConnection conn = await OpenAsync())
            {
                using (OracleCommand check = new OracleCommand(permissionSql, conn))
                {
                    check.BindByName = true;
                    check.Parameters.Add("taskId", taskId);
                    check.Parameters.Add("studentId", student.Id);
                    int allowed = Convert.ToInt32(await check.ExecuteScalarAsync());
                    if (allowed == 0) throw new InvalidOperationException("This assignment is not available to you or its due date has passed.");
                }

                const string updateSql = @"UPDATE SIKAI.AssignmentSubmissions
                    SET SubmissionText=:body, SubmittedAt=SYSDATE, Status='Pending', Feedback=NULL
                    WHERE TaskID=:taskId AND StudentID=:studentId";
                int changed;
                using (OracleCommand update = new OracleCommand(updateSql, conn))
                {
                    update.BindByName = true;
                    update.Parameters.Add("body", submissionText.Trim());
                    update.Parameters.Add("taskId", taskId);
                    update.Parameters.Add("studentId", student.Id);
                    changed = await update.ExecuteNonQueryAsync();
                }
                if (changed == 0)
                {
                    const string insertSql = @"INSERT INTO SIKAI.AssignmentSubmissions
                        (SubmissionID, TaskID, StudentID, SubmissionText, SubmittedAt, Status, Feedback)
                        VALUES (SIKAI.submission_seq.NEXTVAL, :taskId, :studentId, :body, SYSDATE, 'Pending', NULL)";
                    using (OracleCommand insert = new OracleCommand(insertSql, conn))
                    {
                        insert.BindByName = true;
                        insert.Parameters.Add("taskId", taskId);
                        insert.Parameters.Add("studentId", student.Id);
                        insert.Parameters.Add("body", submissionText.Trim());
                        await insert.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        public static async Task<List<SubmissionItem>> GetPendingSubmissionsAsync(Account teacher)
        {
            RequireTeacher(teacher);
            const string sql = @"SELECT sub.SubmissionID, sub.TaskID, sub.StudentID, st.FullName, st.Email,
                    t.Title, s.Code, sub.SubmissionText, sub.SubmittedAt, sub.Status, sub.Feedback
                FROM SIKAI.AssignmentSubmissions sub
                JOIN SIKAI.Tasks t ON t.TaskID=sub.TaskID
                JOIN SIKAI.Subjects s ON s.SubjectID=t.SubjectID
                JOIN SIKAI.Student st ON st.StudentID=sub.StudentID
                WHERE t.OwnerRole='TEACHER' AND t.OwnerID=:teacherId
                ORDER BY CASE WHEN sub.Status='Pending' THEN 0 ELSE 1 END, sub.SubmittedAt DESC";
            List<SubmissionItem> list = new List<SubmissionItem>();
            using (OracleConnection conn = await OpenAsync())
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.BindByName = true;
                cmd.Parameters.Add("teacherId", teacher.Id);
                using (OracleDataReader r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                        list.Add(new SubmissionItem
                        {
                            Id = Convert.ToInt64(r.GetValue(0)), TaskId = Convert.ToInt64(r.GetValue(1)),
                            StudentId = Convert.ToInt64(r.GetValue(2)), StudentName = r.GetString(3), StudentEmail = r.GetString(4),
                            TaskTitle = r.GetString(5), SubjectCode = r.GetString(6),
                            SubmissionText = r.IsDBNull(7) ? "" : r.GetValue(7).ToString(),
                            SubmittedAt = r.GetDateTime(8), Status = r.GetString(9), Feedback = r.IsDBNull(10) ? "" : r.GetValue(10).ToString()
                        });
                }
            }
            return list;
        }

        public static async Task ReviewSubmissionAsync(Account teacher, long submissionId, string status, string feedback)
        {
            RequireTeacher(teacher);
            if (status != "Reviewed" && status != "Needs Changes")
                throw new InvalidOperationException("Choose a valid review status.");
            const string sql = @"UPDATE SIKAI.AssignmentSubmissions sub
                SET Status=:status, Feedback=:feedback
                WHERE sub.SubmissionID=:submissionId AND sub.TaskID IN
                    (SELECT t.TaskID FROM SIKAI.Tasks t WHERE t.OwnerRole='TEACHER' AND t.OwnerID=:teacherId)";
            using (OracleConnection conn = await OpenAsync())
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.BindByName = true;
                cmd.Parameters.Add("status", status);
                cmd.Parameters.Add("feedback", feedback ?? "");
                cmd.Parameters.Add("submissionId", submissionId);
                cmd.Parameters.Add("teacherId", teacher.Id);
                int rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0) throw new InvalidOperationException("Submission not found under your assignments.");
            }
        }


		
		
		

		
		
		
		

		public static async Task<bool> EnrollStudentByCodeAsync(Account student, string classCode)
		{
			const string checkSql = @"
                SELECT COUNT(*)
                FROM SIKAI.SikaiClasses
                WHERE UPPER(ClassCode) = UPPER(:classCode)";

			const string insertSql = @"
                INSERT INTO SIKAI.ClassEnrollments
                    (EnrollmentID, ClassID, StudentID, EnrolledAt)
                SELECT
                    SIKAI.enrollment_seq.NEXTVAL,
                    ClassID,
                    :studentId,
                    SYSDATE
                FROM SIKAI.SikaiClasses
                WHERE UPPER(ClassCode) = UPPER(:classCode)
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM SIKAI.ClassEnrollments ce
                      WHERE ce.ClassID = SikaiClasses.ClassID
                        AND ce.StudentID = :studentId
                  )";

			using (OracleConnection conn = await OpenAsync())
			{
				using (OracleCommand checkCmd = new OracleCommand(checkSql, conn))
				{
					checkCmd.BindByName = true;
					checkCmd.Parameters.Add("classCode", classCode.Trim());

					int count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

					if (count == 0)
						return false;
				}

				using (OracleCommand insertCmd = new OracleCommand(insertSql, conn))
				{
					insertCmd.BindByName = true;
					insertCmd.Parameters.Add("studentId", student.Id);
					insertCmd.Parameters.Add("classCode", classCode.Trim());

					int rows = await insertCmd.ExecuteNonQueryAsync();

					return rows > 0;
				}
			}
		}
		
		
		

		public static async Task<List<ClassModel>> GetStudentClassesAsync(Account student)
		{
			const string sql = @"
                SELECT c.ClassID,
                       c.ClassCode,
                       c.ClassName,
                       c.TeacherID,
                       t.FullName,
                       e.EnrolledAt
                FROM SIKAI.SikaiClasses c
                INNER JOIN SIKAI.ClassEnrollments e
                    ON c.ClassID = e.ClassID
                LEFT JOIN SIKAI.Teacher t
                    ON t.TeacherID = c.TeacherID
                WHERE e.StudentID = :studentId
                ORDER BY c.ClassName";

			List<ClassModel> list = new List<ClassModel>();

			using (OracleConnection conn = await OpenAsync())
			using (OracleCommand cmd = new OracleCommand(sql, conn))
			{
				cmd.BindByName = true;
				cmd.Parameters.Add("studentId", student.Id);

				using (OracleDataReader r = await cmd.ExecuteReaderAsync())
				{
					while (await r.ReadAsync())
					{
						list.Add(new ClassModel
						{
							Id = Convert.ToInt64(r.GetValue(0)),
							Code = r.GetString(1),
							Name = r.GetString(2),
							TeacherId = Convert.ToInt64(r.GetValue(3)),
							TeacherName = r.IsDBNull(4) ? "Unknown teacher" : r.GetString(4),
							EnrolledAt = r.IsDBNull(5) ? DateTime.MinValue : r.GetDateTime(5)
						});
					}
				}
			}

			return list;
		}
		
		
		
		private const string BackfillSql =
"BEGIN\n" +
			"  FOR c IN (SELECT ClassID, ClassCode, ClassName FROM SIKAI.SikaiClasses WHERE SubjectID IS NULL) LOOP\n" +
			"    DECLARE v_id NUMBER;\n" +
			"    BEGIN\n" +
			"      BEGIN\n" +
			"        SELECT s.SubjectID INTO v_id FROM SIKAI.Subjects s\n" +
			"         WHERE s.Code = c.ClassCode\n" +
			"           AND NOT EXISTS (SELECT 1 FROM SIKAI.SikaiClasses x WHERE x.SubjectID = s.SubjectID);\n" +
			"      EXCEPTION WHEN NO_DATA_FOUND THEN\n" +
			"        v_id := SIKAI.subject_seq.NEXTVAL;\n" +
			"        INSERT INTO SIKAI.Subjects (SubjectID, Code, Name)\n" +
			"        VALUES (v_id, SUBSTR(c.ClassCode, 1, 30), SUBSTR(c.ClassName, 1, 150));\n" +
			"      END;\n" +
			"      UPDATE SIKAI.SikaiClasses SET SubjectID = v_id WHERE ClassID = c.ClassID;\n" +
			"    END;\n" +
			"  END LOOP;\n" +
			"  COMMIT;\n" +
			"END;";

		public static async Task EnsureClassSubjectsAsync()
		{
			using (OracleConnection conn = await OpenAsync())
				await ExecAsync(conn, BackfillSql);
		}

		private static async Task ExecAsync(OracleConnection conn, string sql)
		{
			using (OracleCommand cmd = new OracleCommand(sql, conn))
				await cmd.ExecuteNonQueryAsync();
		}

		private static async Task TryExecAsync(OracleConnection conn, string sql, params int[] ignoreOraCodes)
		{
			try { await ExecAsync(conn, sql); }
			catch (OracleException ex) { if (Array.IndexOf(ignoreOraCodes, ex.Number) < 0) throw; }
		}

		public static async Task SetupEnrollmentDatabaseAsync()
		{
			using (OracleConnection conn = await OpenAsync())
			{
				
				try
				{
					using (OracleCommand cmd = new OracleCommand(
						"CREATE SEQUENCE SIKAI.enrollment_seq START WITH 1 INCREMENT BY 1",
						conn))
					{
						await cmd.ExecuteNonQueryAsync();
					}
				}
				catch (OracleException ex)
				{
					
					if (ex.Number != 955)
						throw;
				}

                await TryExecAsync(conn, "CREATE SEQUENCE SIKAI.class_seq START WITH 1 INCREMENT BY 1", 955, 1031);

				
				try
				{
					const string sql = @"
                CREATE TABLE SIKAI.SikaiClasses
                (
                    ClassID NUMBER PRIMARY KEY,
                    ClassCode VARCHAR2(50) NOT NULL UNIQUE,
                    ClassName VARCHAR2(200) NOT NULL,
                    TeacherID NUMBER NOT NULL
                )";

					using (OracleCommand cmd = new OracleCommand(sql, conn))
					{
						await cmd.ExecuteNonQueryAsync();
					}
				}
				catch (OracleException ex)
				{
					
					if (ex.Number != 955)
						throw;
				}

				
				
				await TryExecAsync(conn, "ALTER TABLE SIKAI.SikaiClasses ADD (SubjectID NUMBER)", 1430);
				await TryExecAsync(conn, "CREATE SEQUENCE SIKAI.subject_seq START WITH 100 INCREMENT BY 1", 955, 1031);

				
				await TryExecAsync(conn,
					"CREATE OR REPLACE TRIGGER SIKAI.trg_class_subject\n" +
					"BEFORE INSERT ON SIKAI.SikaiClasses\n" +
					"FOR EACH ROW\n" +
					"DECLARE\n" +
					"  v_id NUMBER;\n" +
					"BEGIN\n" +
					"  IF :NEW.SubjectID IS NULL THEN\n" +
					"    BEGIN\n" +
					"      SELECT s.SubjectID INTO v_id FROM SIKAI.Subjects s\n" +
					"       WHERE s.Code = :NEW.ClassCode\n" +
					"         AND NOT EXISTS (SELECT 1 FROM SIKAI.SikaiClasses c WHERE c.SubjectID = s.SubjectID);\n" +
					"    EXCEPTION WHEN NO_DATA_FOUND THEN\n" +
					"      v_id := SIKAI.subject_seq.NEXTVAL;\n" +
					"      INSERT INTO SIKAI.Subjects (SubjectID, Code, Name)\n" +
					"      VALUES (v_id, SUBSTR(:NEW.ClassCode, 1, 30), SUBSTR(:NEW.ClassName, 1, 150));\n" +
					"    END;\n" +
					"    :NEW.SubjectID := v_id;\n" +
					"  END IF;\n" +
					"END;", 1031);

				
				await TryExecAsync(conn,
					"CREATE OR REPLACE TRIGGER SIKAI.trg_class_subject_rename\n" +
					"AFTER UPDATE OF ClassName ON SIKAI.SikaiClasses\n" +
					"FOR EACH ROW\n" +
					"BEGIN\n" +
					"  IF :NEW.SubjectID IS NOT NULL THEN\n" +
					"    UPDATE SIKAI.Subjects SET Name = SUBSTR(:NEW.ClassName, 1, 150)\n" +
					"     WHERE SubjectID = :NEW.SubjectID;\n" +
					"  END IF;\n" +
					"END;", 1031);

				
				await ExecAsync(conn, BackfillSql);

				
				try
				{
					const string sql = @"
                CREATE TABLE SIKAI.ClassEnrollments
                (
                    EnrollmentID NUMBER PRIMARY KEY,
                    ClassID NUMBER NOT NULL,
                    StudentID NUMBER NOT NULL,
                    EnrolledAt DATE DEFAULT SYSDATE,

                    CONSTRAINT fk_enroll_class
                        FOREIGN KEY (ClassID)
                        REFERENCES SIKAI.SikaiClasses(ClassID),

                    CONSTRAINT fk_enroll_student
                        FOREIGN KEY (StudentID)
                        REFERENCES SIKAI.Student(StudentID),

                    CONSTRAINT uq_class_student
                        UNIQUE (ClassID, StudentID)
                )";

					using (OracleCommand cmd = new OracleCommand(sql, conn))
					{
						await cmd.ExecuteNonQueryAsync();
					}
				}
				catch (OracleException ex)
				{
					
					if (ex.Number != 955)
						throw;
				}

                await EnsureCollaborationTablesAsync(conn);
			}
		}

        private static async Task EnsureCollaborationTablesAsync(OracleConnection conn)
        {
            await TryExecAsync(conn, "CREATE SEQUENCE SIKAI.message_seq START WITH 1 INCREMENT BY 1", 955, 1031);
            await TryExecAsync(conn, "CREATE SEQUENCE SIKAI.notification_seq START WITH 1 INCREMENT BY 1", 955, 1031);
            await TryExecAsync(conn, "CREATE SEQUENCE SIKAI.submission_seq START WITH 1 INCREMENT BY 1", 955, 1031);

            await TryExecAsync(conn, @"CREATE TABLE SIKAI.Messages (
                MessageID NUMBER PRIMARY KEY, SenderRole VARCHAR2(10) NOT NULL, SenderID NUMBER NOT NULL,
                ReceiverRole VARCHAR2(10) NOT NULL, ReceiverID NUMBER NOT NULL, MessageBody VARCHAR2(4000) NOT NULL,
                SentAt DATE DEFAULT SYSDATE NOT NULL, IsRead CHAR(1) DEFAULT 'N' NOT NULL)", 955);

            await TryExecAsync(conn, @"CREATE TABLE SIKAI.Notifications (
                NotificationID NUMBER PRIMARY KEY, RecipientRole VARCHAR2(10) NOT NULL, RecipientID NUMBER NOT NULL,
                Title VARCHAR2(250) NOT NULL, Body VARCHAR2(1000), CreatedAt DATE DEFAULT SYSDATE NOT NULL,
                IsRead CHAR(1) DEFAULT 'N' NOT NULL)", 955);

            await TryExecAsync(conn, @"CREATE TABLE SIKAI.AssignmentSubmissions (
                SubmissionID NUMBER PRIMARY KEY, TaskID NUMBER NOT NULL REFERENCES SIKAI.Tasks(TaskID),
                StudentID NUMBER NOT NULL REFERENCES SIKAI.Student(StudentID), SubmissionText VARCHAR2(4000) NOT NULL,
                SubmittedAt DATE DEFAULT SYSDATE NOT NULL, Status VARCHAR2(30) DEFAULT 'Pending' NOT NULL, Feedback VARCHAR2(2000),
                CONSTRAINT uq_task_student_submission UNIQUE(TaskID, StudentID))", 955);
        }
	}
}
