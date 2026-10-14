using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sikai_Project
{
    public partial class TeacherMaterialsWindow : Window
    {
        private List<ResourceModel> _resources = new List<ResourceModel>();
        public TeacherMaterialsWindow()
        {
            InitializeComponent();
            Loaded += async delegate { await LoadDataAsync(); };
        }

        private async Task LoadDataAsync()
        {
            if (Session.CurrentUser == null || Session.CurrentUser.Role != Role.Teacher) { StatusText.Text = "Please sign in with a teacher account."; return; }
            try
            {
                SubjectComboBox.ItemsSource = await TeacherDb.GetTeacherSubjectsAsync(Session.CurrentUser);
                _resources = await Db.GetResourcesAsync(Session.CurrentUser);
                RenderMaterials();
            }
            catch (Exception ex) { StatusText.Text = "Could not load materials: " + ex.Message; }
        }

        private void RenderMaterials()
        {
            if (MaterialsPanel == null) return;
            string q = (SearchBox.Text ?? "").Trim();
            MaterialsPanel.Children.Clear();
            foreach (ResourceModel r in _resources.Where(x => q.Length == 0 || (x.Title + " " + x.SubjectName + " " + x.Description).IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0).OrderByDescending(x => x.UploadedAt))
            {
                StackPanel body = new StackPanel();
                body.Children.Add(new TextBlock { Text = r.Title, FontSize = 18, FontWeight = FontWeights.SemiBold, Foreground = new SolidColorBrush(Color.FromRgb(41, 38, 58)) });
                body.Children.Add(new TextBlock { Text = r.SubjectCode + " • " + r.ResourceType + " • " + r.UploadedAt.ToString("dd MMM yyyy"), Margin = new Thickness(0, 5, 0, 6), Foreground = Brushes.SlateGray });
                body.Children.Add(new TextBlock { Text = r.Description, TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Gray });
                if (!string.IsNullOrWhiteSpace(r.Url)) { Button open = new Button { Content = "Open resource", Tag = r.Url, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 10, 0, 0), Padding = new Thickness(12, 6, 12, 6) }; open.Click += OpenResource_Click; body.Children.Add(open); }
                MaterialsPanel.Children.Add(new Border { Background = Brushes.White, CornerRadius = new CornerRadius(12), Padding = new Thickness(18), Margin = new Thickness(0, 0, 0, 12), Child = body });
            }
            if (MaterialsPanel.Children.Count == 0) MaterialsPanel.Children.Add(new TextBlock { Text = "No published resources found.", Foreground = Brushes.Gray, Margin = new Thickness(0, 8, 0, 12) });
        }

        private async void AddMaterial_Click(object sender, RoutedEventArgs e)
        {
            SubjectModel subject = SubjectComboBox.SelectedItem as SubjectModel;
            string title = TitleBox.Text.Trim();
            if (subject == null || title.Length == 0) { StatusText.Text = "Choose a class/subject and enter a title."; return; }
            ComboBoxItem typeItem = ResourceTypeComboBox.SelectedItem as ComboBoxItem;
            string type = typeItem == null ? "Link" : typeItem.Content.ToString();
            try
            {
                await Db.AddTeacherResourceAsync(Session.CurrentUser, subject.Id, title, DescriptionBox.Text.Trim(), type, UrlBox.Text.Trim());
                TitleBox.Clear(); DescriptionBox.Clear(); UrlBox.Clear(); StatusText.Foreground = Brushes.ForestGreen; StatusText.Text = "Resource published. Students in the class have been notified.";
                _resources = await Db.GetResourcesAsync(Session.CurrentUser); RenderMaterials();
            }
            catch (Exception ex) { StatusText.Foreground = Brushes.Firebrick; StatusText.Text = ex.Message; }
        }
        private void OpenResource_Click(object sender, RoutedEventArgs e) { string url = ((Button)sender).Tag as string; try { Process.Start(url); } catch (Exception ex) { MessageBox.Show("Could not open resource: " + ex.Message, "SIKAI", MessageBoxButton.OK, MessageBoxImage.Error); } }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) { RenderMaterials(); }
        private void Dashboard_Click(object s, RoutedEventArgs e) { new TeacherDashboardWindow().Show(); Close(); }
        private void Courses_Click(object s, RoutedEventArgs e) { new TeacherCoursesWindow().Show(); Close(); }
        private void Students_Click(object s, RoutedEventArgs e) { TeacherNavigation.OpenStudents(this); }
        private void Assignments_Click(object s, RoutedEventArgs e) { new TeacherAssignmentsWindow().Show(); Close(); }
        private void Pending_Click(object s, RoutedEventArgs e) { new TeacherPendingSubmissionsWindow().Show(); Close(); }
        private void Messages_Click(object s, RoutedEventArgs e) { new TeacherMessagesWindow().Show(); Close(); }
        private void Logout_Click(object s, RoutedEventArgs e) { TeacherNavigation.Logout(this); }
    }
}
