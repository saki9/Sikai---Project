using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sikai_Project
{
    public partial class TeacherPendingSubmissionsWindow : Window
    {
        private List<SubmissionItem> _submissions = new List<SubmissionItem>();
        public TeacherPendingSubmissionsWindow() { InitializeComponent(); Loaded += async delegate { await LoadDataAsync(); }; }
        private async Task LoadDataAsync()
        {
            if (Session.CurrentUser == null || Session.CurrentUser.Role != Role.Teacher) { StatusText.Text = "Please sign in with a teacher account."; return; }
            try { _submissions = await Db.GetPendingSubmissionsAsync(Session.CurrentUser); RenderSubmissions(); }
            catch (Exception ex) { StatusText.Text = "Could not load submissions: " + ex.Message; }
        }
        private void RenderSubmissions()
        {
            string q = (SearchBox.Text ?? "").Trim(); SubmissionsPanel.Children.Clear();
            foreach (SubmissionItem item in _submissions.Where(x => q.Length == 0 || (x.StudentName + " " + x.StudentEmail + " " + x.TaskTitle + " " + x.SubjectCode + " " + x.Status).IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                StackPanel p = new StackPanel();
                p.Children.Add(new TextBlock { Text = item.TaskTitle, FontSize = 18, FontWeight = FontWeights.SemiBold });
                p.Children.Add(new TextBlock { Text = item.SubjectCode + " • " + item.StudentName + " • " + item.SubmittedAt.ToString("dd MMM yyyy, h:mm tt") + " • " + item.Status, Foreground = Brushes.SlateGray, Margin = new Thickness(0, 5, 0, 8) });
                p.Children.Add(new TextBlock { Text = item.StudentEmail, Foreground = Brushes.Gray, Margin = new Thickness(0, 0, 0, 7) });
                p.Children.Add(new TextBlock { Text = item.SubmissionText, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 10) });
                TextBox feedback = new TextBox { Text = item.Feedback ?? "", AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, MinHeight = 55, Padding = new Thickness(8), VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
                p.Children.Add(new TextBlock { Text = "Feedback (optional)", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 4) }); p.Children.Add(feedback);
                StackPanel buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
                Button reviewed = new Button { Content = "Mark reviewed", Tag = new ReviewAction { Item = item, Feedback = feedback, Status = "Reviewed" }, Style = (Style)FindResource("PrimaryButton"), Margin = new Thickness(0, 0, 8, 0) }; reviewed.Click += Review_Click;
                Button changes = new Button { Content = "Request changes", Tag = new ReviewAction { Item = item, Feedback = feedback, Status = "Needs Changes" }, Padding = new Thickness(12, 7, 12, 7) }; changes.Click += Review_Click;
                buttons.Children.Add(reviewed); buttons.Children.Add(changes); p.Children.Add(buttons);
                SubmissionsPanel.Children.Add(new Border { Background = Brushes.White, CornerRadius = new CornerRadius(12), Padding = new Thickness(18), Margin = new Thickness(0, 0, 0, 12), Child = p });
            }
            if (SubmissionsPanel.Children.Count == 0) SubmissionsPanel.Children.Add(new TextBlock { Text = "No submissions found yet.", Foreground = Brushes.Gray, Margin = new Thickness(0, 10, 0, 0) });
        }
        private async void Review_Click(object sender, RoutedEventArgs e)
        {
            ReviewAction action = (ReviewAction)((Button)sender).Tag;
            try { await Db.ReviewSubmissionAsync(Session.CurrentUser, action.Item.Id, action.Status, action.Feedback.Text.Trim()); StatusText.Foreground = Brushes.ForestGreen; StatusText.Text = "Submission updated."; await LoadDataAsync(); }
            catch (Exception ex) { StatusText.Foreground = Brushes.Firebrick; StatusText.Text = ex.Message; }
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) { if (SubmissionsPanel != null) RenderSubmissions(); }
        private void Dashboard_Click(object s, RoutedEventArgs e) { new TeacherDashboardWindow().Show(); Close(); }
        private void Courses_Click(object s, RoutedEventArgs e) { new TeacherCoursesWindow().Show(); Close(); }
        private void Materials_Click(object s, RoutedEventArgs e) { new TeacherMaterialsWindow().Show(); Close(); }
        private void Assignments_Click(object s, RoutedEventArgs e) { new TeacherAssignmentsWindow().Show(); Close(); }
        private void Students_Click(object s, RoutedEventArgs e) { TeacherNavigation.OpenStudents(this); }
        private void Messages_Click(object s, RoutedEventArgs e) { new TeacherMessagesWindow().Show(); Close(); }
        private void Logout_Click(object s, RoutedEventArgs e) { TeacherNavigation.Logout(this); }
        private sealed class ReviewAction { public SubmissionItem Item; public TextBox Feedback; public string Status; }
    }
}
