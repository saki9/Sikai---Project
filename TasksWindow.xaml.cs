using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sikai_Project
{
    public partial class TasksWindow : Window
    {
        private List<TaskModel> _tasks = new List<TaskModel>();
        public TasksWindow()
        {
            InitializeComponent(); Navigation.Wire(this); Loaded += async delegate { await LoadDataAsync(); };
        }
        private async Task LoadDataAsync()
        {
            if (Session.CurrentUser == null) return;
            try { _tasks = await Db.GetTasksAsync(Session.CurrentUser); Render(); }
            catch (Exception ex) { MessageBox.Show("Could not load tasks.\n" + ex.Message, "SIKAI", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void Render()
        {
            if (TasksList == null) return;
            string q = (SearchTasksTextBox.Text ?? "").Trim().ToLowerInvariant();
            string filter = StatusFilterComboBox.SelectedIndex <= 0 ? "" : ((ComboBoxItem)StatusFilterComboBox.SelectedItem).Content.ToString();
            IEnumerable<TaskModel> data = _tasks.Where(t => (filter == "" || t.Status.Equals(filter, StringComparison.OrdinalIgnoreCase)) &&
                (q == "" || (t.Title + " " + t.Description + " " + t.SubjectName).ToLowerInvariant().Contains(q))).OrderBy(t => t.DueDate);
            TasksList.Items.Clear();
            foreach (TaskModel t in data)
            {
                StackPanel p = new StackPanel();
                p.Children.Add(new TextBlock { Text = t.Title, FontSize = 17, FontWeight = FontWeights.SemiBold });
                p.Children.Add(new TextBlock { Text = t.SubjectCode + " • " + t.Priority + " • Due " + t.DueDate.ToString("dd MMM yyyy") + (t.OwnerRole == "TEACHER" ? " • Teacher assignment" : ""), Foreground = Brushes.SlateGray, Margin = new Thickness(0, 4, 0, 5) });
                p.Children.Add(new TextBlock { Text = t.Description, TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Gray });
                StackPanel actions = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
                if (t.OwnerRole == "TEACHER")
                {
                    Button submit = new Button { Content = "Submit answer", Tag = t, Margin = new Thickness(0, 0, 8, 0), Padding = new Thickness(12, 7, 12, 7), Background = new SolidColorBrush(Color.FromRgb(117, 103, 217)), Foreground = Brushes.White, BorderThickness = new Thickness(0) };
                    submit.Click += Submit_Click; actions.Children.Add(submit);
                    actions.Children.Add(new TextBlock { Text = "Submission status is shown in teacher review.", VerticalAlignment = VerticalAlignment.Center, Foreground = Brushes.SlateGray });
                }
                else
                {
                    foreach (string status in new[] { "To Do", "In Progress", "Completed" })
                    {
                        Button b = new Button { Content = status, Tag = t, Margin = new Thickness(0, 0, 7, 0), Padding = new Thickness(10, 6, 10, 6), BorderThickness = new Thickness(0), Background = new SolidColorBrush(Color.FromRgb(243, 240, 248)) };
                        b.Click += Status_Click; actions.Children.Add(b);
                    }
                }
                p.Children.Add(actions);
                TasksList.Items.Add(new Border { Background = Brushes.White, CornerRadius = new CornerRadius(16), Padding = new Thickness(18), Margin = new Thickness(0, 0, 0, 12), Child = p });
            }
            if (TasksList.Items.Count == 0) TasksList.Items.Add(new TextBlock { Text = "No tasks or assignments match your filter.", Foreground = Brushes.Gray });
        }
        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            TaskModel task = (TaskModel)((Button)sender).Tag;
            if (task.DueDate.Date < DateTime.Today) { MessageBox.Show("The due date for this assignment has passed.", "SIKAI", MessageBoxButton.OK, MessageBoxImage.Information); return; }
            TextSubmissionDialog dialog = new TextSubmissionDialog(task.Title);
            dialog.Owner = this;
            if (dialog.ShowDialog() != true) return;
            try { await Db.SubmitAssignmentAsync(Session.CurrentUser, task.Id, dialog.SubmissionText); MessageBox.Show("Your assignment was submitted successfully.", "SIKAI", MessageBoxButton.OK, MessageBoxImage.Information); }
            catch (Exception ex) { MessageBox.Show("Could not submit assignment.\n" + ex.Message, "SIKAI", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private async void Status_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender; TaskModel t = (TaskModel)b.Tag; string status = b.Content.ToString();
            try { await Db.UpdateTaskStatusAsync(Session.CurrentUser, t.Id, status); t.Status = status; Render(); }
            catch (Exception ex) { MessageBox.Show("Could not update task.\n" + ex.Message, "SIKAI", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void SearchTasksTextBox_TextChanged(object sender, TextChangedEventArgs e) { if (TasksList != null) Render(); }
        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (TasksList != null) Render(); }
    }

    internal sealed class TextSubmissionDialog : Window
    {
        private readonly TextBox _answer;
        public string SubmissionText { get { return _answer.Text.Trim(); } }
        public TextSubmissionDialog(string title)
        {
            Title = "Submit Assignment"; Width = 520; Height = 380; WindowStartupLocation = WindowStartupLocation.CenterOwner; ResizeMode = ResizeMode.NoResize; Background = Brushes.White;
            StackPanel panel = new StackPanel { Margin = new Thickness(22) };
            panel.Children.Add(new TextBlock { Text = title, FontSize = 20, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 12) });
            panel.Children.Add(new TextBlock { Text = "Enter your answer or paste a link to your work:" });
            _answer = new TextBox { AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, MinHeight = 170, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Padding = new Thickness(8), Margin = new Thickness(0, 8, 0, 14) };
            panel.Children.Add(_answer);
            StackPanel buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            Button cancel = new Button { Content = "Cancel", Padding = new Thickness(14, 7, 14, 7), Margin = new Thickness(0, 0, 8, 0) }; cancel.Click += delegate { DialogResult = false; };
            Button save = new Button { Content = "Submit", Padding = new Thickness(14, 7, 14, 7), Background = new SolidColorBrush(Color.FromRgb(117, 103, 217)), Foreground = Brushes.White, BorderThickness = new Thickness(0) };
            save.Click += delegate { if (string.IsNullOrWhiteSpace(_answer.Text)) { MessageBox.Show("Enter your assignment answer or a link.", "SIKAI", MessageBoxButton.OK, MessageBoxImage.Information); return; } DialogResult = true; };
            buttons.Children.Add(cancel); buttons.Children.Add(save); panel.Children.Add(buttons); Content = panel;
        }
    }
}
