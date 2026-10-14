using System.Windows;

namespace Sikai_Project
{
	public static class TeacherNavigation
	{
		public static void OpenDashboard(Window current)
		{
			new TeacherDashboardWindow().Show();
			current.Close();
		}

		public static void OpenCourses(Window current)
		{
			new TeacherCoursesWindow().Show();
			current.Close();
		}

		public static void OpenStudents(Window current)
		{
			new TeacherStudentsWindow().Show();
			current.Close();
		}

		public static void OpenAssignments(Window current)
		{
			new TeacherAssignmentsWindow().Show();
			current.Close();
		}

		public static void OpenMaterials(Window current)
		{
			new TeacherMaterialsWindow().Show();
			current.Close();
		}

		public static void OpenMessages(Window current)
		{
			new TeacherMessagesWindow().Show();
			current.Close();
		}

		public static void OpenPendingSubmissions(Window current)
		{
			new TeacherPendingSubmissionsWindow().Show();
			current.Close();
		}

		public static void Logout(Window current)
		{
			Session.CurrentUser = null;
			new LoginWindow().Show();
			current.Close();
		}
	}
}