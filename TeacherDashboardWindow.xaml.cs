using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;

namespace Sikai_Project
{
	public partial class TeacherDashboardWindow : Window
	{
		public TeacherDashboardWindow()
		{
			InitializeComponent();

			Loaded += async delegate
			{
				await LoadDataAsync();
			};
		}

		private async Task LoadDataAsync()
		{
			if (Session.CurrentUser == null)
				return;

			NameText.Text = Session.CurrentUser.FullName;

			try
			{
				TeacherDashboardStats stats =
					await TeacherDb.GetDashboardStatsAsync(
						Session.CurrentUser);

				CoursesText.Text =
					stats.CourseCount.ToString();

				AssignmentsText.Text =
					stats.AssignmentCount.ToString();

				ReviewText.Text =
					stats.PendingSubmissionCount.ToString();

				StudentsText.Text =
					stats.StudentCount.ToString();

				
				DashboardCoursesList.Children.Clear();
				List<TeacherClassItem> classes =
					await TeacherDb.GetTeacherClassesAsync(Session.CurrentUser);

				foreach (TeacherClassItem c in classes)
				{
					DockPanel row = new DockPanel();
					row.Children.Add(new TextBlock
					{
						Text = "◉",
						FontSize = 18,
						Foreground = new SolidColorBrush(Color.FromRgb(0x8B, 0x70, 0xB8))
					});
					DockPanel.SetDock(row.Children[0], Dock.Left);
					row.Children.Add(new TextBlock
					{
						Text = c.ClassName + " (" + c.ClassCode + ")",
						Margin = new Thickness(12, 0, 0, 0),
						VerticalAlignment = VerticalAlignment.Center
					});
					row.Children.Add(new TextBlock
					{
						Text = c.StudentCount + (c.StudentCount == 1 ? " student" : " students"),
						Foreground = new SolidColorBrush(Color.FromRgb(0x85, 0x80, 0x9B)),
						HorizontalAlignment = HorizontalAlignment.Right
					});
					DashboardCoursesList.Children.Add(new Border
					{
						Background = new SolidColorBrush(Color.FromRgb(0xF4, 0xF0, 0xFA)),
						CornerRadius = new CornerRadius(10),
						Padding = new Thickness(12),
						Margin = new Thickness(0, 0, 0, 8),
						Child = row
					});
				}

				if (classes.Count == 0)
					DashboardCoursesList.Children.Add(new TextBlock
					{
						Text = "You are not assigned to any classes yet.",
						Foreground = new SolidColorBrush(Color.FromRgb(0x85, 0x80, 0x9B))
					});
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					"Could not load teacher dashboard.\n\n" +
					ex.Message,
					"SIKAI",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
			}
		}


		private void ManageStudents_Click(
			object sender,
			RoutedEventArgs e)
		{
			TeacherNavigation.OpenStudents(this);
		}

		private void Courses_Click(object sender, RoutedEventArgs e)
		{
			TeacherNavigation.OpenCourses(this);
		}

		private void Students_Click(object sender, RoutedEventArgs e)
		{
			TeacherNavigation.OpenStudents(this);
		}

		private void Materials_Click(object sender, RoutedEventArgs e)
		{
			TeacherNavigation.OpenMaterials(this);
		}

		private void Assignments_Click(object sender, RoutedEventArgs e)
		{
			TeacherNavigation.OpenAssignments(this);
		}

		private void Pending_Click(object sender, RoutedEventArgs e)
		{
			TeacherNavigation.OpenPendingSubmissions(this);
		}

		private void Messages_Click(object sender, RoutedEventArgs e)
		{
			TeacherNavigation.OpenMessages(this);
		}

		private void Logout_Click(object sender, RoutedEventArgs e)
		{
			TeacherNavigation.Logout(this);
		}


		private void AddMaterial_Click(
			object sender,
			RoutedEventArgs e)
		{
			new TeacherMaterialsWindow().Show();
			Close();
		}


		private void CreateAssignment_Click(
			object sender,
			RoutedEventArgs e)
		{
			new TeacherAssignmentsWindow().Show();
			Close();
		}


		private void MessageStudent_Click(
			object sender,
			RoutedEventArgs e)
		{
			new TeacherMessagesWindow().Show();
			Close();
		}
	}
}