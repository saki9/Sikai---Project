using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sikai_Project
{
	public partial class EnrollClassWindow : Window
	{
		public EnrollClassWindow()
		{
			InitializeComponent();
			Loaded += async delegate { await LoadClassesAsync(); };
		}

		private void BackButton_Click(object sender, RoutedEventArgs e)
		{
			if (Session.CurrentUser == null)
			{
				new LoginWindow().Show();
				Close();
				return;
			}

			new DashboardWindow().Show();
			Close();
		}

		private async Task LoadClassesAsync()
		{
			MyClassesList.Items.Clear();

			if (Session.CurrentUser == null || Session.CurrentUser.Role != Role.Student)
			{
				MyClassesList.Items.Add(EmptyText("Log in as a student to see your classes."));
				return;
			}

			try
			{
				List<ClassModel> classes = await Db.GetStudentClassesAsync(Session.CurrentUser);

				foreach (ClassModel c in classes)
				{
					StackPanel p = new StackPanel();
					p.Children.Add(new TextBlock
					{
						Text = c.Name + "  (" + c.Code + ")",
						FontSize = 15,
						FontWeight = FontWeights.SemiBold,
						Foreground = new SolidColorBrush(Color.FromRgb(0x3F, 0x3A, 0x56))
					});
					p.Children.Add(new TextBlock
					{
						Text = "Teacher: " + c.TeacherName +
							(c.EnrolledAt == DateTime.MinValue ? "" : "   •   Enrolled " + c.EnrolledAt.ToString("dd MMM yyyy")),
						Margin = new Thickness(0, 4, 0, 0),
						Foreground = new SolidColorBrush(Color.FromRgb(0x70, 0x69, 0x7D))
					});

					MyClassesList.Items.Add(new Border
					{
						Background = Brushes.White,
						CornerRadius = new CornerRadius(12),
						Padding = new Thickness(14),
						Margin = new Thickness(0, 0, 0, 8),
						Child = p
					});
				}

				if (classes.Count == 0)
					MyClassesList.Items.Add(EmptyText("You are not enrolled in any classes yet."));
			}
			catch (Exception ex)
			{
				MyClassesList.Items.Add(EmptyText("Could not load your classes: " + ex.Message));
			}
		}

		private TextBlock EmptyText(string text)
		{
			return new TextBlock
			{
				Text = text,
				Foreground = new SolidColorBrush(Color.FromRgb(0x85, 0x80, 0x9B)),
				TextWrapping = TextWrapping.Wrap,
				Margin = new Thickness(2)
			};
		}

		private async void EnrollButton_Click(object sender, RoutedEventArgs e)
		{
			string classCode = ClassCodeTextBox.Text.Trim();

			if (string.IsNullOrWhiteSpace(classCode))
			{
				MessageBox.Show(
					"Please enter a class code.",
					"SIKAI",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);

				return;
			}

			if (Session.CurrentUser == null ||
				Session.CurrentUser.Role != Role.Student)
			{
				MessageBox.Show(
					"Please log in as a student first.",
					"SIKAI",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);

				return;
			}

			try
			{
				bool enrolled = await Db.EnrollStudentByCodeAsync(
					Session.CurrentUser,
					classCode);

				if (enrolled)
				{
					MessageBox.Show(
						"You have successfully enrolled in the class!",
						"SIKAI",
						MessageBoxButton.OK,
						MessageBoxImage.Information);

					ClassCodeTextBox.Clear();
					await LoadClassesAsync();
				}
				else
				{
					MessageBox.Show(
						"Invalid class code or you are already enrolled in this class.",
						"SIKAI",
						MessageBoxButton.OK,
						MessageBoxImage.Warning);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					"Enrollment failed:\n\n" + ex.Message,
					"SIKAI",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
			}
		}
	}
}