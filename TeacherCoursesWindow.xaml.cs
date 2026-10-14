using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sikai_Project
{
	public partial class TeacherCoursesWindow : Window
	{
		public TeacherCoursesWindow()
		{
			InitializeComponent();
			Loaded += async delegate { await LoadCoursesAsync(); };
		}

		
		private async Task LoadCoursesAsync()
		{
			CoursesPanel.Children.Clear();

			if (Session.CurrentUser == null || Session.CurrentUser.Role != Role.Teacher)
			{
				CoursesPanel.Children.Add(Message("Log in as a teacher to see your courses."));
				return;
			}

			try
			{
				List<TeacherClassItem> classes =
					await TeacherDb.GetTeacherClassesAsync(Session.CurrentUser);

				foreach (TeacherClassItem c in classes)
					CoursesPanel.Children.Add(MakeCourseCard(c));

				if (classes.Count == 0)
					CoursesPanel.Children.Add(Message("You are not assigned to any classes yet."));
			}
			catch (Exception ex)
			{
				CoursesPanel.Children.Add(Message("Could not load your courses: " + ex.Message));
			}
		}

		private Border MakeCourseCard(TeacherClassItem c)
		{
			StackPanel p = new StackPanel();
			p.Children.Add(new TextBlock
			{
				Text = c.ClassCode,
				FontSize = 13,
				FontWeight = FontWeights.Bold,
				Foreground = new SolidColorBrush(Color.FromRgb(0x75, 0x67, 0xD9))
			});
			p.Children.Add(new TextBlock
			{
				Text = c.ClassName,
				FontSize = 21,
				FontWeight = FontWeights.Bold,
				Foreground = new SolidColorBrush(Color.FromRgb(0x29, 0x26, 0x3A)),
				TextWrapping = TextWrapping.Wrap,
				Margin = new Thickness(0, 8, 0, 18)
			});

			Grid g = new Grid();
			g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
			g.Children.Add(new TextBlock
			{
				Text = "👥  " + c.StudentCount + (c.StudentCount == 1 ? " Student" : " Students"),
				Foreground = new SolidColorBrush(Color.FromRgb(0x77, 0x73, 0x8A)),
				VerticalAlignment = VerticalAlignment.Center
			});
			Button open = new Button
			{
				Content = "Open Course",
				Width = 135,
				Style = (Style)FindResource("PrimaryButton")
			};
			open.Click += OpenCourse_Click;
			Grid.SetColumn(open, 1);
			g.Children.Add(open);
			p.Children.Add(g);

			return new Border
			{
				Style = (Style)FindResource("Card"),
				Width = 430,
				Child = p
			};
		}

		private TextBlock Message(string text)
		{
			return new TextBlock
			{
				Text = text,
				Foreground = new SolidColorBrush(Color.FromRgb(0x77, 0x73, 0x8A)),
				Margin = new Thickness(5)
			};
		}

		private async void CreateClass_Click(object sender, RoutedEventArgs e)
        {
            if (Session.CurrentUser == null || Session.CurrentUser.Role != Role.Teacher)
            {
                MessageBox.Show("Please sign in with a teacher account to create a class.",
                    "SIKAI", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CreateClassWindow dialog = new CreateClassWindow { Owner = this };
            dialog.ShowDialog();
            await LoadCoursesAsync();
        }

		private void Dashboard_Click(object sender, RoutedEventArgs e)
		{
			new TeacherDashboardWindow().Show();
			Close();
		}

		private void Materials_Click(object sender, RoutedEventArgs e)
		{
			new TeacherMaterialsWindow().Show();
			Close();
		}

		private void Assignments_Click(object sender, RoutedEventArgs e)
		{
			new TeacherAssignmentsWindow().Show();
			Close();
		}

		private void Pending_Click(object sender, RoutedEventArgs e)
		{
			new TeacherPendingSubmissionsWindow().Show();
			Close();
		}

		private void Messages_Click(object sender, RoutedEventArgs e)
		{
			new TeacherMessagesWindow().Show();
			Close();
		}

		private void Students_Click(object sender, RoutedEventArgs e)
		{
			TeacherNavigation.OpenStudents(this);
		}

		private void Logout_Click(object sender, RoutedEventArgs e)
		{
			TeacherNavigation.Logout(this);
		}

		private void OpenCourse_Click(object sender, RoutedEventArgs e)
		{
			
			TeacherNavigation.OpenStudents(this);
		}

		private void SearchBox_TextChanged(
			object sender,
			TextChangedEventArgs e)
		{
			string search =
				SearchBox.Text.Trim().ToLower();

			foreach (UIElement element in CoursesPanel.Children)
			{
				Border card = element as Border;

				if (card == null)
					continue;

				TextBlock[] texts =
					FindTextBlocks(card);

				bool found = false;

				foreach (TextBlock text in texts)
				{
					if (!string.IsNullOrEmpty(text.Text) &&
						text.Text.ToLower().Contains(search))
					{
						found = true;
						break;
					}
				}

				card.Visibility =
					string.IsNullOrEmpty(search) || found
						? Visibility.Visible
						: Visibility.Collapsed;
			}
		}

		private TextBlock[] FindTextBlocks(DependencyObject parent)
		{
			System.Collections.Generic.List<TextBlock> result =
				new System.Collections.Generic.List<TextBlock>();

			int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);

			for (int i = 0; i < count; i++)
			{
				DependencyObject child =
					System.Windows.Media.VisualTreeHelper.GetChild(parent, i);

				TextBlock text = child as TextBlock;

				if (text != null)
					result.Add(text);

				result.AddRange(FindTextBlocks(child));
			}

			return result.ToArray();
		}
	}
}