using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sikai_Project
{
    public partial class TeacherAssignmentsWindow : Window
    {
        private List<TaskModel> _assignments = new List<TaskModel>();
        public TeacherAssignmentsWindow()
        {
            InitializeComponent();
            DueDatePicker.SelectedDate = DateTime.Today.AddDays(7);
            Loaded += async delegate { await LoadDataAsync(); };
        }
        private async Task LoadDataAsync()
        {
            if (Session.CurrentUser == null || Session.CurrentUser.Role != Role.Teacher) { StatusText.Text = "Please sign in with a teacher account."; return; }
            try { SubjectComboBox.ItemsSource = await TeacherDb.GetTeacherSubjectsAsync(Session.CurrentUser); _assignments = await TeacherDb.GetTeacherAssignmentsAsync(Session.CurrentUser); RenderAssignments(); }
            catch (Exception ex) { StatusText.Text = "Could not load assignments: " + ex.Message; }
        }
        private void RenderAssignments()
        {
            if (AssignmentsPanel == null) return;
            string q = (SearchBox.Text ?? "").Trim(); AssignmentsPanel.Children.Clear();
            foreach (TaskModel t in _assignments.Where(x => q.Length == 0 || (x.Title + " " + x.SubjectName + " " + x.Description).IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0).OrderBy(x => x.DueDate))
            {
                StackPanel content = new StackPanel();
                content.Children.Add(new TextBlock { Text = t.Title, FontSize = 18, FontWeight = FontWeights.SemiBold, Foreground = new SolidColorBrush(Color.FromRgb(41, 38, 58)) });
                content.Children.Add(new TextBlock { Text = t.SubjectCode + " • Due " + t.DueDate.ToString("dd MMM yyyy") + " • " + t.Priority, Margin = new Thickness(0, 5, 0, 6), Foreground = Brushes.SlateGray });
                content.Children.Add(new TextBlock { Text = t.Description, TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Gray });
                AssignmentsPanel.Children.Add(new Border { Background = Brushes.White, CornerRadius = new CornerRadius(12), Padding = new Thickness(18), Margin = new Thickness(0, 0, 0, 12), Child = content });
            }
            if (AssignmentsPanel.Children.Count == 0) AssignmentsPanel.Children.Add(new TextBlock { Text = "No assignments published yet.", Foreground = Brushes.Gray });
        }
        private async void CreateAssignment_Click(object sender, RoutedEventArgs e)
        {
            SubjectModel subject = SubjectComboBox.SelectedItem as SubjectModel;
            ComboBoxItem p = PriorityComboBox.SelectedItem as ComboBoxItem;
            if (subject == null || string.IsNullOrWhiteSpace(TitleBox.Text) || !DueDatePicker.SelectedDate.HasValue) { StatusText.Text = "Choose a subject, title, and due date."; return; }
            try
            {
                await Db.AddTeacherAssignmentAsync(Session.CurrentUser, subject.Id, TitleBox.Text.Trim(), DescriptionBox.Text.Trim(), DueDatePicker.SelectedDate.Value, p == null ? "Medium" : p.Content.ToString());
                TitleBox.Clear(); DescriptionBox.Clear(); DueDatePicker.SelectedDate = DateTime.Today.AddDays(7); StatusText.Foreground = Brushes.ForestGreen; StatusText.Text = "Assignment published. Students in the class have been notified.";
                _assignments = await TeacherDb.GetTeacherAssignmentsAsync(Session.CurrentUser); RenderAssignments();
            }
            catch (Exception ex) { StatusText.Foreground = Brushes.Firebrick; StatusText.Text = ex.Message; }
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) { RenderAssignments(); }
        private void Dashboard_Click(object s, RoutedEventArgs e) { new TeacherDashboardWindow().Show(); Close(); }
        private void Courses_Click(object s, RoutedEventArgs e) { new TeacherCoursesWindow().Show(); Close(); }
        private void Materials_Click(object s, RoutedEventArgs e) { new TeacherMaterialsWindow().Show(); Close(); }
        private void Students_Click(object s, RoutedEventArgs e) { TeacherNavigation.OpenStudents(this); }
        private void Pending_Click(object s, RoutedEventArgs e) { new TeacherPendingSubmissionsWindow().Show(); Close(); }
        private void Messages_Click(object s, RoutedEventArgs e) { new TeacherMessagesWindow().Show(); Close(); }
        private void Logout_Click(object s, RoutedEventArgs e) { TeacherNavigation.Logout(this); }
    }
}
