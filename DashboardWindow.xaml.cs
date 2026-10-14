using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Sikai_Project
{
    public partial class DashboardWindow : Window
    {
        public DashboardWindow()
        {
            InitializeComponent();
            Navigation.Wire(this);
            Loaded += async delegate { await LoadDataAsync(); };
        }

        private async Task LoadDataAsync()
        {
            if (Session.CurrentUser == null) return;
            int hour = DateTime.Now.Hour;
            GreetingText.Text = hour < 12 ? "Good morning," : (hour < 17 ? "Good afternoon," : "Good evening,");
            UserNameText.Text = Session.CurrentUser.FullName;
            try
            {
                DashboardData d = await Db.GetDashboardAsync(Session.CurrentUser);
                NotesCountText.Text = d.NoteCount.ToString();
                TasksCountText.Text = d.TaskCount.ToString();
                CompletedText.Text = d.CompletedTaskCount + " completed";
                SubjectsCountText.Text = d.SubjectCount.ToString();
                StudyHoursText.Text = d.StudyHours.ToString("0.##");

                TodayTasksList.Items.Clear();
                foreach (TaskModel t in d.TodayTasks)
                    TodayTasksList.Items.Add(MakeCard(t.Title, t.SubjectCode + " • " + t.Status + " • " + t.Priority, t.DueDate.ToString("dd MMM yyyy")));

                RecentNotesList.Items.Clear();
                foreach (NoteModel n in d.RecentNotes)
                    RecentNotesList.Items.Add(MakeCard(n.Title, n.SubjectCode, n.CreatedAt.ToString("dd MMM yyyy")));

                TodayPlansList.Items.Clear();
                foreach (StudyPlanModel p in d.TodayPlans)
                    TodayPlansList.Items.Add(MakeCard(p.SubjectCode + " • " + p.Description, p.StartTime + " - " + p.EndTime, p.PlanDate.ToString("dd MMM yyyy")));
                if (d.TodayTasks.Count == 0) TodayTasksList.Items.Add(MakeEmpty("No tasks due today."));
                if (d.RecentNotes.Count == 0) RecentNotesList.Items.Add(MakeEmpty("No notes found."));
                if (d.TodayPlans.Count == 0) TodayPlansList.Items.Add(MakeEmpty("No study plans for today."));
            }
            catch (Exception ex) { MessageBox.Show("Could not load dashboard data.\n\n" + ex.Message, "SIKAI", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private Border MakeCard(string title, string detail, string date)
        {
            StackPanel p = new StackPanel();
            p.Children.Add(new TextBlock { Text=title, FontSize=15, FontWeight=FontWeights.SemiBold, Foreground=(System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#292443") });
            p.Children.Add(new TextBlock { Text=detail, Margin=new Thickness(0,5,0,0), Foreground=(System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#70697D") });
            p.Children.Add(new TextBlock { Text=date, Margin=new Thickness(0,4,0,0), FontSize=11, Foreground=(System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#A08FAF") });
            return new Border { Background=System.Windows.Media.Brushes.White, CornerRadius=new CornerRadius(14), Padding=new Thickness(14), Margin=new Thickness(0,0,0,8), Child=p };
        }
        private TextBlock MakeEmpty(string text) { return new TextBlock { Text=text, Foreground=(System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#85809B"), Margin=new Thickness(5) }; }
    }
}
