using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sikai_Project
{
    public partial class subject : Window
    {
        private List<SubjectModel> _subjects=new List<SubjectModel>();
        public subject(){InitializeComponent();Navigation.Wire(this);Loaded+=async delegate{await LoadDataAsync();};}
        private async Task LoadDataAsync(){if(Session.CurrentUser==null)return;try{_subjects=await Db.GetSubjectsAsync(Session.CurrentUser);Render();}catch(Exception ex){MessageBox.Show("Could not load subjects.\n"+ex.Message,"SIKAI",MessageBoxButton.OK,MessageBoxImage.Error);}}
        private void Render()
        {
            string q=(SearchSubjectsTextBox.Text??"").Trim().ToLowerInvariant();
            SubjectsList.Items.Clear();
            foreach(SubjectModel s in _subjects.Where(x=>q==""||x.Code.ToLowerInvariant().Contains(q)||x.Name.ToLowerInvariant().Contains(q)))
            {
                Grid g=new Grid();g.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(1,GridUnitType.Star)});g.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});
                StackPanel p=new StackPanel();
                p.Children.Add(new TextBlock{Text=s.Code+"  •  "+s.Name,FontSize=19,FontWeight=FontWeights.SemiBold});
                p.Children.Add(new TextBlock{Text=s.NoteCount+" notes    "+s.TaskCount+" tasks    "+s.ResourceCount+" resources",Foreground=Brushes.SlateGray,Margin=new Thickness(0,7,0,0)});
                g.Children.Add(p);
                Button b=new Button{Content="View Subject  ❯",Tag=s,Padding=new Thickness(14,9,14,9),Margin=new Thickness(20,0,0,0),Background=new SolidColorBrush(Color.FromRgb(243,240,248)),BorderThickness=new Thickness(0)};
                b.Click+=ViewSubject_Click;Grid.SetColumn(b,1);g.Children.Add(b);
                SubjectsList.Items.Add(new Border{Background=Brushes.White,CornerRadius=new CornerRadius(16),Padding=new Thickness(20),Margin=new Thickness(0,0,0,12),Child=g});
            }
            if(SubjectsList.Items.Count==0)SubjectsList.Items.Add(new TextBlock{Text="You are not enrolled in any classes yet. Use Enroll to join a class with its class code.",Foreground=Brushes.Gray});
        }
        private void ViewSubject_Click(object sender,RoutedEventArgs e){SubjectModel s=(SubjectModel)((Button)sender).Tag;Navigation.OpenSubject(this,s.Id);}
        private void SearchSubjectsTextBox_TextChanged(object sender,TextChangedEventArgs e){if(SubjectsList!=null)Render();}
    }
}
