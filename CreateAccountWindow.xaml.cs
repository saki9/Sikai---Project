using System;
using System.Windows;

namespace Sikai_Project
{
    public partial class CreateAccountWindow : Window
    {
        public CreateAccountWindow()
        {
            InitializeComponent();
            NameBox.Focus();
        }

        private Role SelectedRole
        {
            get { return TeacherRadio.IsChecked == true ? Role.Teacher : Role.Student; }
        }

        private void GeneratePassword_Click(object sender, RoutedEventArgs e)
        {
            PasswordBox1.Text = PasswordHasher.RandomText(10, PasswordHasher.PasswordAlphabet);
        }

        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            FormError.Visibility = Visibility.Collapsed;
            FormSuccess.Visibility = Visibility.Collapsed;

            string name = NameBox.Text.Trim();
            string email = EmailBox.Text.Trim();
            string password = PasswordBox1.Text;
            string question = QuestionBox.Text.Trim();
            string answer = AnswerBox.Text.Trim();

            if (name.Length == 0 || email.Length == 0 || password.Length == 0 ||
                question.Length == 0 || answer.Length == 0)
            {
                ShowError("Fill in every field before creating the account.");
                return;
            }

            if (password.Length < 8)
            {
                ShowError("Use a password of at least 8 characters.");
                return;
            }

            CreateButton.IsEnabled = false;
            try
            {
                await Db.CreateUserAsync(
                    SelectedRole,
                    name,
                    email,
                    PasswordHasher.CreateEntry(password),
                    question,
                    PasswordHasher.CreateEntry(PasswordHasher.NormalizeAnswer(answer)));

                FormSuccess.Text = SelectedRole + " account created for " + email +
                                    ".\nPassword: " + password + " (write this down, it won't be shown again)";
                FormSuccess.Visibility = Visibility.Visible;

                NameBox.Clear();
                EmailBox.Clear();
                PasswordBox1.Clear();
                AnswerBox.Clear();
                NameBox.Focus();
            }
            catch (Exception ex)
            {
                ShowError(Db.Describe(ex, "That email is already registered for this role."));
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
