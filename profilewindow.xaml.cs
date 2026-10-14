using System;
using System.Threading.Tasks;
using System.Windows;

namespace Sikai_Project
{
    public partial class ProfileWindow : Window
    {
        public ProfileWindow(){InitializeComponent();Navigation.Wire(this);Loaded+=async delegate{await LoadDataAsync();};}
        private async Task LoadDataAsync()
        {
            if(Session.CurrentUser==null)return;
            try{
                Account a=await Db.FindAccountAsync(Session.CurrentUser.Role,Session.CurrentUser.Email);
                if(a!=null)Session.CurrentUser=a;
                ProfileNameText.Text=Session.CurrentUser.FullName;EmailText.Text=Session.CurrentUser.Email;RoleText.Text=Session.CurrentUser.Role.ToString();
                int[] c=await Db.GetActivityCountsAsync(Session.CurrentUser);
                TotalNotesText.Text=c[0].ToString();CompletedTasksText.Text=c[1].ToString();TotalSubjectsText.Text=c[2].ToString();StudyHoursText.Text=c[3].ToString();
            }catch(Exception ex){MessageBox.Show("Could not load profile.\n"+ex.Message,"SIKAI",MessageBoxButton.OK,MessageBoxImage.Error);}
        }
    }
}
