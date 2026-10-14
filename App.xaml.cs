using System;
using System.Windows;

namespace Sikai_Project
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            
            try
            {
                await Db.SetupEnrollmentDatabaseAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database initialization could not finish. Check Oracle access and schema setup before continuing.\n\n" + ex.Message,
                    "SIKAI", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            new LoginWindow().Show();
        }
    }
}
