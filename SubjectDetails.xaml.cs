using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sikai_Project
{
    public partial class SubjectDetails : Window
    {
        private readonly long _subjectId;
        public SubjectDetails(long subjectId)
        {
            InitializeComponent();_subjectId=subjectId;Navigation.Wire(this);Loaded+=async delegate{await LoadDataAsync();};
        }
        private async Task LoadDataAsync()
        {
            if(Session.CurrentUser==null)return;
            try{
                List<SubjectModel> subjects=await Db.GetSubjectsAsync(Session.CurrentUser);
                SubjectModel s=subjects.Find(x=>x.Id==_subjectId);
                if(s==null){MessageBox.Show("Subject not found.");Navigation.BackToSubjects(this);return;}
                SubjectCodeText.Text=s.Code;SubjectNameText.Text=s.Name;
                List<NoteModel> notes=await Db.GetSubjectRecentNotesAsync(Session.CurrentUser,_subjectId);
                List<TaskModel> tasks=await Db.GetSubjectTasksAsync(Session.CurrentUser,_subjectId);
                List<ResourceModel> resources=await Db.GetSubjectResourcesAsync(Session.CurrentUser,_subjectId);
                NotesCountText.Text=notes.Count.ToString();TasksCountText.Text=tasks.Count.ToString();ResourcesCountText.Text=resources.Count.ToString();
                foreach(NoteModel n in notes.GetRange(0,Math.Min(5,notes.Count)))SubjectNotesList.Items.Add(Card(n.Title,n.Content,n.CreatedAt.ToString("dd MMM yyyy")));
                foreach(TaskModel t in tasks.GetRange(0,Math.Min(5,tasks.Count)))SubjectTasksList.Items.Add(Card(t.Title,t.Status+" • "+t.Priority,"Due "+t.DueDate.ToString("dd MMM yyyy")));
                foreach(ResourceModel r in resources.GetRange(0,Math.Min(5,resources.Count)))SubjectResourcesList.Items.Add(Card(r.Title,r.Description,r.ResourceType));
                if(SubjectNotesList.Items.Count==0)SubjectNotesList.Items.Add(Empty("No notes."));
                if(SubjectTasksList.Items.Count==0)SubjectTasksList.Items.Add(Empty("No tasks."));
                if(SubjectResourcesList.Items.Count==0)SubjectResourcesList.Items.Add(Empty("No resources."));
            }catch(Exception ex){MessageBox.Show("Could not load subject data.\n"+ex.Message,"SIKAI",MessageBoxButton.OK,MessageBoxImage.Error);}
        }
        private Border Card(string title,string detail,string meta){StackPanel p=new StackPanel();p.Children.Add(new TextBlock{Text=title,FontSize=15,FontWeight=FontWeights.SemiBold});p.Children.Add(new TextBlock{Text=detail,Foreground=Brushes.Gray,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,4,0,0)});p.Children.Add(new TextBlock{Text=meta,Foreground=Brushes.SlateGray,Margin=new Thickness(0,4,0,0)});return new Border{Background=Brushes.White,CornerRadius=new CornerRadius(14),Padding=new Thickness(14),Margin=new Thickness(0,0,0,8),Child=p};}
        private TextBlock Empty(string t){return new TextBlock{Text=t,Foreground=Brushes.Gray,Margin=new Thickness(5)};}
        private void Back_Click(object sender,RoutedEventArgs e){Navigation.BackToSubjects(this);}
    }
}
