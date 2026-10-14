using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sikai_Project
{
    public partial class StudyPlanner : Window
    {
        private List<SubjectModel> _subjects=new List<SubjectModel>();
        private DateTime _selectedDate;
        public StudyPlanner()
        {
            InitializeComponent();Navigation.Wire(this);
            _selectedDate=DateTime.Today;PlannerCalendar.SelectedDate=_selectedDate;PlanDatePicker.SelectedDate=_selectedDate;
            Loaded+=async delegate{await LoadDataAsync();};
        }
        private async Task LoadDataAsync()
        {
            if(Session.CurrentUser==null)return;
            try{
                _subjects=await Db.GetSubjectsAsync(Session.CurrentUser);
                SubjectComboBox.Items.Clear();
                foreach(SubjectModel s in _subjects)SubjectComboBox.Items.Add(s);
                SubjectComboBox.DisplayMemberPath="Name";
                if(SubjectComboBox.Items.Count>0)SubjectComboBox.SelectedIndex=0;
                await LoadPlansAsync(_selectedDate);
            }catch(Exception ex){MessageBox.Show("Could not load study planner data.\n"+ex.Message,"SIKAI",MessageBoxButton.OK,MessageBoxImage.Error);}
        }
        private async Task LoadPlansAsync(DateTime date)
        {
            List<StudyPlanModel> plans=await Db.GetStudyPlansAsync(Session.CurrentUser,date);
            PlansList.Items.Clear();
            foreach(StudyPlanModel p in plans)
            {
                StackPanel s=new StackPanel();
                s.Children.Add(new TextBlock{Text=p.SubjectCode+" • "+p.Description,FontSize=16,FontWeight=FontWeights.SemiBold});
                s.Children.Add(new TextBlock{Text=p.StartTime+" - "+p.EndTime,Foreground=Brushes.SlateGray,Margin=new Thickness(0,4,0,0)});
                PlansList.Items.Add(new Border{Background=Brushes.White,CornerRadius=new CornerRadius(14),Padding=new Thickness(15),Margin=new Thickness(0,0,0,9),Child=s});
            }
            if(PlansList.Items.Count==0)PlansList.Items.Add(new TextBlock{Text="No study plan for this date.",Foreground=Brushes.Gray});
        }
        private async void AddPlanButton_Click(object sender,RoutedEventArgs e)
        {
            if(Session.CurrentUser==null||SubjectComboBox.SelectedItem==null){MessageBox.Show("Select a subject.");return;}
            DateTime date=PlanDatePicker.SelectedDate??DateTime.Today;
            string desc=(PlanDescriptionTextBox.Text??"").Trim(), start=(StartTimeTextBox.Text??"").Trim(), end=(EndTimeTextBox.Text??"").Trim();
            TimeSpan st,et;
            if(desc==""){MessageBox.Show("Enter a study description.");return;}
            if(!TimeSpan.TryParse(start,out st)||!TimeSpan.TryParse(end,out et)||et<=st){MessageBox.Show("Enter valid times in HH:mm format. End time must be after start time.");return;}
            try{
                SubjectModel s=(SubjectModel)SubjectComboBox.SelectedItem;
                await Db.AddStudyPlanAsync(Session.CurrentUser,s.Id,desc,date,start,end);
                PlanDescriptionTextBox.Clear();StartTimeTextBox.Clear();EndTimeTextBox.Clear();
                _selectedDate=date;PlannerCalendar.SelectedDate=date;await LoadPlansAsync(date);
                MessageBox.Show("Study plan saved to the database.","SIKAI",MessageBoxButton.OK,MessageBoxImage.Information);
            }catch(Exception ex){MessageBox.Show("Could not save study plan.\n"+ex.Message,"SIKAI",MessageBoxButton.OK,MessageBoxImage.Error);}
        }
        private async void PlannerCalendar_SelectedDatesChanged(object sender,SelectionChangedEventArgs e)
        {
            if(PlannerCalendar.SelectedDate.HasValue&&Session.CurrentUser!=null){_selectedDate=PlannerCalendar.SelectedDate.Value;PlanDatePicker.SelectedDate=_selectedDate;try{await LoadPlansAsync(_selectedDate);}catch{}}
        }
    }
}
