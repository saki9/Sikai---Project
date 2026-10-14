using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sikai_Project
{
	public partial class TeacherStudentsWindow : Window
	{
		
		private bool _refillingClasses;

		public TeacherStudentsWindow()
		{
			InitializeComponent();

			Loaded += async delegate
			{
				
				if (Session.CurrentUser == null ||
					Session.CurrentUser.Role != Role.Teacher)
				{
					TeacherNavigation.Logout(this);
					return;
				}

				await LoadClassesAsync(null);
			};
		}

		private TeacherClassItem SelectedClass
		{
			get { return ClassCombo.SelectedItem as TeacherClassItem; }
		}

		private ClassStudentItem SelectedStudent
		{
			get { return StudentsGrid.SelectedItem as ClassStudentItem; }
		}

		
		
		

		private async Task LoadClassesAsync(long? keepClassId)
		{
			try
			{
				var classes = await TeacherDb.GetTeacherClassesAsync(Session.CurrentUser);

				_refillingClasses = true;
				ClassCombo.ItemsSource = classes;

				TeacherClassItem pick = null;
				if (keepClassId.HasValue)
					pick = classes.Find(c => c.ClassId == keepClassId.Value);
				if (pick == null && classes.Count > 0)
					pick = classes[0];

				ClassCombo.SelectedItem = pick;
				_refillingClasses = false;

				if (classes.Count == 0)
				{
					StudentsGrid.ItemsSource = null;
					ShowEmpty("You don't have any classes yet.\nCreate one from My Courses to get started.");
					return;
				}

				await LoadStudentsAsync();
			}
			catch (Exception ex)
			{
				_refillingClasses = false;
				ShowStatus("Could not load your classes: " + ex.Message, true);
			}
		}

		private async Task LoadStudentsAsync()
		{
			TeacherClassItem selected = SelectedClass;
			RemoveButton.IsEnabled = false;

			if (selected == null)
			{
				StudentsGrid.ItemsSource = null;
				return;
			}

			try
			{
				var students = await TeacherDb.GetClassStudentsAsync(
					Session.CurrentUser, selected.ClassId);

				StudentsGrid.ItemsSource = students;

				if (students.Count == 0)
					ShowEmpty("No students are enrolled in this class yet.\nStudents join using the class code: " + selected.ClassCode);
				else
					EmptyText.Visibility = Visibility.Collapsed;
			}
			catch (Exception ex)
			{
				ShowStatus("Could not load students: " + ex.Message, true);
			}
		}

		
		
		

		private async void ClassCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (_refillingClasses)
				return;

			StatusText.Text = "";
			await LoadStudentsAsync();
		}

		private void StudentsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			RemoveButton.IsEnabled = SelectedStudent != null;
		}

		private async void Remove_Click(object sender, RoutedEventArgs e)
		{
			TeacherClassItem cls = SelectedClass;
			ClassStudentItem student = SelectedStudent;
			if (cls == null || student == null)
				return;

			MessageBoxResult answer = MessageBox.Show(
				"Remove " + student.FullName + " from \"" + cls.ClassName + "\"?\n\n" +
				"They can rejoin later with the class code.",
				"SIKAI",
				MessageBoxButton.YesNo,
				MessageBoxImage.Warning);

			if (answer != MessageBoxResult.Yes)
				return;

			RemoveButton.IsEnabled = false;
			try
			{
				bool removed = await TeacherDb.RemoveStudentFromClassAsync(
					Session.CurrentUser, cls.ClassId, student.StudentId);

				if (removed)
					ShowStatus(student.FullName + " was removed from " + cls.ClassName + ".", false);
				else
					ShowStatus("That student was already removed.", true);

				
				await LoadClassesAsync(cls.ClassId);
			}
			catch (Exception ex)
			{
				ShowStatus("Could not remove the student: " + ex.Message, true);
				RemoveButton.IsEnabled = SelectedStudent != null;
			}
		}

		
		
		

		private void Dashboard_Click(object sender, RoutedEventArgs e)
		{
			TeacherNavigation.OpenDashboard(this);
		}

		private void Courses_Click(object sender, RoutedEventArgs e)
		{
			TeacherNavigation.OpenCourses(this);
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

		
		
		

		private void ShowEmpty(string text)
		{
			EmptyText.Text = text;
			EmptyText.Visibility = Visibility.Visible;
		}

		private void ShowStatus(string text, bool isError)
		{
			StatusText.Text = text;
			StatusText.Foreground = isError
				? new SolidColorBrush(Color.FromRgb(0xD9, 0x6C, 0x6C))
				: new SolidColorBrush(Color.FromRgb(0x4E, 0x8F, 0x6B));
		}
	}
}
