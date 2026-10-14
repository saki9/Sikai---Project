using System;
using System.Windows;

namespace Sikai_Project
{
    public partial class CreateClassWindow : Window
    {
        public CreateClassWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                if (Session.CurrentUser == null || Session.CurrentUser.Role != Role.Teacher)
                {
                    ShowError("Please sign in with a teacher account to create a class.");
                    CreateButton.IsEnabled = false;
                }
            };
        }

        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            FormError.Visibility = Visibility.Collapsed;
            FormSuccess.Visibility = Visibility.Collapsed;

            string className = ClassNameBox.Text.Trim();
            string classCode = ClassCodeBox.Text.Trim();

            if (className.Length == 0 || classCode.Length == 0)
            {
                ShowError("Enter a class name and a class code.");
                return;
            }

            if (Session.CurrentUser == null || Session.CurrentUser.Role != Role.Teacher)
            {
                ShowError("Please sign in with a teacher account to create a class.");
                return;
            }

            CreateButton.IsEnabled = false;
            try
            {
                await Db.CreateClassAsync(Session.CurrentUser, className, classCode);

                FormSuccess.Text = "Class \"" + className + "\" created with code " + classCode + ".";
                FormSuccess.Visibility = Visibility.Visible;

                ClassNameBox.Clear();
                ClassCodeBox.Clear();
                ClassNameBox.Focus();
            }
            catch (Exception ex)
            {
                ShowError(Db.Describe(ex, "That class code is already in use."));
            }
            finally
            {
                CreateButton.IsEnabled = true;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ShowError(string message)
        {
            FormError.Text = message;
            FormError.Visibility = Visibility.Visible;
        }
    }
}
