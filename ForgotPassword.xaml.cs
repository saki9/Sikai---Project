using System;
using System.Windows;
using Oracle.ManagedDataAccess.Client;

namespace Sikai_Project
{
    public partial class ForgotPassword : Window
    {
        private const int MaxAnswerAttempts = 3;
        private const int MinPasswordLength = 8;

        private Account _account;
        private int _wrongAnswers;

        public ForgotPassword()
        {
            InitializeComponent();
            EmailBox.Focus();
        }

        private Role SelectedRole
        {
            get { return TeacherRadio.IsChecked == true ? Role.Teacher : Role.Student; }
        }

        
        private async void Continue_Click(object sender, RoutedEventArgs e)
        {
            FormError.Visibility = Visibility.Collapsed;

            string email = EmailBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                EmailError.Visibility = Visibility.Visible;
                return;
            }
            EmailError.Visibility = Visibility.Collapsed;

            SetBusy(ContinueButton, true, "CONTINUE");
            try
            {
                Account account = await Db.FindAccountAsync(SelectedRole, email);

                if (account == null)
                {
                    ShowFormError("No " + SelectedRole.ToString().ToLower() + " account uses that email.");
                    return;
                }

                if (string.IsNullOrEmpty(account.SecurityQuestion) ||
                    string.IsNullOrEmpty(account.SecurityAnswerEntry))
                {
                    ShowFormError("This account has no security question yet. contact your school administrator to reset your password.");
                    return;
                }

                _account = account;
                _wrongAnswers = 0;

                QuestionText.Text = account.SecurityQuestion;
                IntroText.Text = "Answer your security question, then choose a new password.";
                Step1Panel.Visibility = Visibility.Collapsed;
                Step2Panel.Visibility = Visibility.Visible;
                ContinueButton.IsDefault = false;
                ResetButton.IsDefault = true;
                AnswerBox.Focus();
            }
            catch (OracleException ex)
            {
                ShowFormError("Can't reach the database (ORA-" + ex.Number.ToString("00000") + "). Try again in a moment.");
            }
            catch (Exception ex)
            {
                ShowFormError("Something went wrong: " + ex.Message);
            }
            finally
            {
                SetBusy(ContinueButton, false, "CONTINUE");
            }
        }

        
        private async void Reset_Click(object sender, RoutedEventArgs e)
        {
            FormError.Visibility = Visibility.Collapsed;

            string answer = AnswerBox.Text;
            string newPassword = NewPasswordBox.Password;
            string confirm = ConfirmPasswordBox.Password;

            bool answerMissing = string.IsNullOrWhiteSpace(answer);
            bool passwordTooShort = newPassword.Length < MinPasswordLength;
            bool mismatch = newPassword != confirm;

            AnswerError.Visibility = answerMissing ? Visibility.Visible : Visibility.Collapsed;
            NewPasswordError.Visibility = passwordTooShort ? Visibility.Visible : Visibility.Collapsed;
            ConfirmError.Visibility = (!passwordTooShort && mismatch) ? Visibility.Visible : Visibility.Collapsed;

            if (answerMissing || passwordTooShort || mismatch)
                return;

            if (!PasswordHasher.Verify(PasswordHasher.NormalizeAnswer(answer), _account.SecurityAnswerEntry))
            {
                _wrongAnswers++;
                int left = MaxAnswerAttempts - _wrongAnswers;

                if (left <= 0)
                {
                    MessageBox.Show("Too many wrong answers. Please try again later or contact your school administrator.",
                        "SIKAI", MessageBoxButton.OK, MessageBoxImage.Warning);
                    GoToLogin();
                    return;
                }

                ShowFormError("That answer doesn't match. " + left + (left == 1 ? " attempt" : " attempts") + " left.");
                return;
            }

            SetBusy(ResetButton, true, "RESET PASSWORD");
            try
            {
                await Db.UpdatePasswordAsync(_account.Role, _account.Id, PasswordHasher.CreateEntry(newPassword));

                MessageBox.Show("Your password has been reset. You can log in with it now.",
                    "SIKAI", MessageBoxButton.OK, MessageBoxImage.Information);
                GoToLogin();
            }
            catch (OracleException ex)
            {
                ShowFormError("Couldn't save the new password (ORA-" + ex.Number.ToString("00000") + "). Try again.");
            }
            catch (Exception ex)
            {
                ShowFormError("Something went wrong: " + ex.Message);
            }
            finally
            {
                SetBusy(ResetButton, false, "RESET PASSWORD");
            }
        }

        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            GoToLogin();
        }

        private void GoToLogin()
        {
            new LoginWindow().Show();
            Close();
        }

        private void ShowFormError(string message)
        {
            FormError.Text = message;
            FormError.Visibility = Visibility.Visible;
        }

        private static void SetBusy(System.Windows.Controls.Button button, bool busy, string idleText)
        {
            button.IsEnabled = !busy;
            button.Content = busy ? "PLEASE WAIT..." : idleText;
            button.Opacity = busy ? 0.6 : 1.0;
        }
    }
}
