using System;
using System.Windows;
using Oracle.ManagedDataAccess.Client;

namespace Sikai_Project
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            EmailBox.Focus();
        }

        private Role SelectedRole
        {
            get { return TeacherRadio.IsChecked == true ? Role.Teacher : Role.Student; }
        }

        private string EnteredPassword
        {
            get { return ShowPasswordToggle1.IsChecked == true ? PasswordTextBox1.Text : PasswordBox1.Password; }
        }

        
        private void ShowPassword_Changed(object sender, RoutedEventArgs e)
        {
            if (ShowPasswordToggle1.IsChecked == true)
            {
                PasswordTextBox1.Text = PasswordBox1.Password;
                PasswordTextBox1.Visibility = Visibility.Visible;
                PasswordBox1.Visibility = Visibility.Collapsed;
                PasswordTextBox1.Focus();
                PasswordTextBox1.CaretIndex = PasswordTextBox1.Text.Length;
            }
            else
            {
                PasswordBox1.Password = PasswordTextBox1.Text;
                PasswordBox1.Visibility = Visibility.Visible;
                PasswordTextBox1.Visibility = Visibility.Collapsed;
                PasswordBox1.Focus();
            }
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            FormError.Visibility = Visibility.Collapsed;

            string email = EmailBox.Text.Trim();
            string password = EnteredPassword;

            bool emailMissing = string.IsNullOrWhiteSpace(email);
            bool passwordMissing = string.IsNullOrEmpty(password);

            EmailError.Visibility = emailMissing ? Visibility.Visible : Visibility.Collapsed;
            PasswordError.Visibility = passwordMissing ? Visibility.Visible : Visibility.Collapsed;

            if (emailMissing || passwordMissing)
                return;

            SetBusy(true);
            try
            {
                Account account = await Db.FindAccountAsync(SelectedRole, email);
                if (account == null || !PasswordHasher.Verify(password, account.PasswordEntry))
                {
                    ShowFormError("Email or password is incorrect.");
                    return;
                }

                Session.CurrentUser = account;
                OpenDashboard(account);
            }
            catch (OracleException ex)
            {
                string who = "";
                try
                {
                    using (var conn = await Db.OpenAsync())
                    using (var cmd = new OracleCommand(
                        "SELECT sys_context('USERENV','SESSION_USER') || ' @ ' || sys_context('USERENV','CON_NAME') FROM dual", conn))
                    {
                        who = " [connected as " + await cmd.ExecuteScalarAsync() + "]";
                    }
                }
                catch { }

                ShowFormError("Database error ORA-" + ex.Number.ToString("00000") + who);
            }
            catch (Exception ex)
            {
                ShowFormError("Something went wrong: " + ex.Message);
            }
            finally
            {
                SetBusy(false);
            }
        }

        
        private void OpenDashboard(Account account)
        {
            if (account.Role == Role.Teacher)
                new TeacherDashboardWindow().Show();
            else
                new DashboardWindow().Show();

            Close();
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            new ForgotPassword().Show();
            Close();
        }

        private void ShowFormError(string message)
        {
            FormError.Text = message;
            FormError.Visibility = Visibility.Visible;
        }

        private void SetBusy(bool busy)
        {
            LoginButton.IsEnabled = !busy;
            LoginButton.Content = busy ? "SIGNING IN..." : "LOGIN";
            LoginButton.Opacity = busy ? 0.6 : 1.0;
        }
    }
}
