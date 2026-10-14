using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Sikai_Project
{
    public partial class RemoveAccountWindow : Window
    {
        private List<UserRow> _allUsers = new List<UserRow>();

        public RemoveAccountWindow()
        {
            InitializeComponent();
            Loaded += async (s, e) => await LoadUsersAsync();
        }

        private Role SelectedRole
        {
            get { return TeacherRadio.IsChecked == true ? Role.Teacher : Role.Student; }
        }

        private UserRow SelectedUser
        {
            get { return UsersGrid.SelectedItem as UserRow; }
        }

        private async System.Threading.Tasks.Task LoadUsersAsync()
        {
            FormError.Visibility = Visibility.Collapsed;
            DeleteButton.IsEnabled = false;
            try
            {
                _allUsers = await Db.ListUsersAsync(SelectedRole);
                ApplyFilter();
            }
            catch (Exception ex)
            {
                ShowError(Db.Describe(ex, ""));
            }
        }

        private void ApplyFilter()
        {
            string term = SearchBox.Text.Trim().ToLowerInvariant();

            UsersGrid.ItemsSource = term.Length == 0
                ? _allUsers
                : _allUsers.Where(u =>
                    u.FullName.ToLowerInvariant().Contains(term) ||
                    u.Email.ToLowerInvariant().Contains(term)).ToList();
        }

        private async void Role_Changed(object sender, RoutedEventArgs e)
        {
            SearchBox.Clear();
            await LoadUsersAsync();
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadUsersAsync();
        }

        private void UsersGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            DeleteButton.IsEnabled = SelectedUser != null;
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            FormError.Visibility = Visibility.Collapsed;

            UserRow user = SelectedUser;
            if (user == null)
                return;

            var confirm = MessageBox.Show(
                "Remove " + SelectedRole.ToString().ToLower() + " \"" + user.FullName + "\" (" + user.Email + ")?\n" +
                "This cannot be undone.",
                "SIKAI Admin",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            DeleteButton.IsEnabled = false;
            try
            {
                await Db.DeleteUserAsync(SelectedRole, user.Id);
                await LoadUsersAsync();
            }
            catch (Exception ex)
            {
                ShowError(Db.Describe(ex, ""));
                DeleteButton.IsEnabled = true;
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
