using System.Windows;

namespace Sikai_Project
{
    public partial class MainWindow : Window
    {
        private readonly AdminAccount _admin;

        public MainWindow(AdminAccount admin)
        {
            InitializeComponent();
            _admin = admin;
            WelcomeText.Text = "SIGNED IN: " + admin.FullName.ToUpperInvariant();
        }

        private void CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            new CreateAccountWindow { Owner = this }.ShowDialog();
        }

        private void CreateClass_Click(object sender, RoutedEventArgs e)
        {
            new CreateClassWindow { Owner = this }.ShowDialog();
        }

        private void RemoveAccount_Click(object sender, RoutedEventArgs e)
        {
            new RemoveAccountWindow { Owner = this }.ShowDialog();
        }

        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            Session.CurrentAdmin = null;
            Session.CurrentUser = null;
            new LoginWindow().Show();
            Close();
        }
    }
}
