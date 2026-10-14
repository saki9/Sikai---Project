using System;
using System.Threading.Tasks;
using System.Windows;
using Oracle.ManagedDataAccess.Client;

namespace Sikai_Project
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(){InitializeComponent();Navigation.Wire(this);Loaded+=async delegate{await LoadAccountAsync();};}
        private async Task LoadAccountAsync()
        {
            if(Session.CurrentUser==null)return;
            try{
                Account a=await Db.FindAccountAsync(Session.CurrentUser.Role,Session.CurrentUser.Email);
                if(a!=null)Session.CurrentUser=a;
                AccountNameText.Text=Session.CurrentUser.FullName;AccountEmailText.Text=Session.CurrentUser.Email;AccountRoleText.Text=Session.CurrentUser.Role.ToString();
            }catch(Exception ex){MessageBox.Show("Could not load account.\n"+ex.Message,"SIKAI",MessageBoxButton.OK,MessageBoxImage.Error);}
        }
        private async void UpdatePassword_Click(object sender,RoutedEventArgs e)
        {
            if(Session.CurrentUser==null)return;
            string current=CurrentPasswordBox.Password,next=NewPasswordBox.Password,confirm=ConfirmPasswordBox.Password;
            if(string.IsNullOrEmpty(current)||string.IsNullOrEmpty(next)||string.IsNullOrEmpty(confirm)){MessageBox.Show("Fill in all password fields.");return;}
            if(next.Length<8){MessageBox.Show("New password must contain at least 8 characters.");return;}
            if(next!=confirm){MessageBox.Show("New passwords do not match.");return;}
            if(!PasswordHasher.Verify(current,Session.CurrentUser.PasswordEntry)){MessageBox.Show("Current password is incorrect.");return;}
            UpdatePasswordButton.IsEnabled=false;
            try{string entry=PasswordHasher.CreateEntry(next);await Db.UpdatePasswordAsync(Session.CurrentUser.Role,Session.CurrentUser.Id,entry);Session.CurrentUser.PasswordEntry=entry;CurrentPasswordBox.Clear();NewPasswordBox.Clear();ConfirmPasswordBox.Clear();MessageBox.Show("Password updated successfully.");}
            catch(OracleException ex){MessageBox.Show("Database error ORA-"+ex.Number.ToString("00000")+".");}
            catch(Exception ex){MessageBox.Show("Could not update password.\n"+ex.Message);}
            finally{UpdatePasswordButton.IsEnabled=true;}
        }
    }
}
