# SIKAI – DB-Backed Study Notes Management System

## Technology
- C# WPF desktop application
- .NET Framework 4.7.2
- Oracle Database with `Oracle.ManagedDataAccess`

## Implemented workflows mapped to the Level 0 and Level 1 DFDs

### Student
- Log in and view the student dashboard.
- Join a class with its class code and view enrolled-class subjects.
- View/download/open resources shared by teachers and view assignments for enrolled classes.
- Submit assignment answers or work links before the due date.
- Update the status of personal study tasks.
- Chat with teachers who share a class and view notification messages.
- Manage personal notes, study plans, profile, and settings.

### Teacher
- Log in to a separate teacher dashboard.
- View assigned classes and enrolled students.
- Publish class learning materials with a resource type, description, and URL.
- Create assignments with instructions, due date, and priority.
- Review submissions and leave feedback or request changes.
- Chat with students enrolled in the teacher's classes.
- Publishing an assignment or resource notifies students in the matching class.

### Class management
- Teachers create classes from **My Courses**. Each class is automatically assigned to the signed-in teacher.
- Students join existing classes using the class code supplied by their teacher.
- Creating a class also ensures that it has a matching subject for course resources and assignments.

Account creation and account removal are handled by the separate SIKAI administration project; this student/teacher app only provides student and teacher login.

## Oracle setup
1. Open the project folder on Windows in Visual Studio.
2. Connect to the Oracle PDB using the SIKAI schema owner.
3. Run `setup.sql` in SQL Developer using **Run Script (F5)**. It creates the required Student/Teacher account tables only if they do not already exist, then creates the application tables/sequences, security-question columns, and class/enrolment tables. It seeds student and teacher test accounts, sample records, and a small demo class network. The test-account and class/enrolment seeds are designed to preserve IDs and not duplicate records when the script is rerun.
4. The application creates/ensures the `Messages`, `Notifications`, and `AssignmentSubmissions` tables and their sequences on startup. The schema owner must have permission to create tables, sequences, and triggers.
5. Check `App.config` and change its connection string to match your local Oracle service. The included value is:

   `User Id=sikai;Password=12345;Data Source=localhost:1521/orclpdb;`

The application intentionally checks that it connects to the `SIKAI` schema on an `ORCLPDB` service. If your service uses a different name, update the connection validation in `Db.OpenAsync` to match it.

## Test accounts
These accounts are inserted by `setup.sql`:

| Role | Email | Password |
|---|---|---|
| Student | `student@sikai.test` | `Student123` |
| Teacher | `teacher@sikai.test` | `Teacher123` |

The student and teacher demo accounts use `blue` as their password-recovery answer. Change demo passwords and credentials before using real personal or academic data.

## Build and verification
Open `Sikai - Project.slnx` in Visual Studio on Windows, restore packages if asked, and build the project. It targets .NET Framework WPF 4.7.2, so a compatible Windows/.NET Framework developer environment is required. The updated archive was checked for XML validity, project-file references, XAML event handlers, and expected schema statements in this environment; a full WPF build and live Oracle integration test could not be run here because the Windows MSBuild/WPF toolchain and a connected Oracle server were unavailable.
