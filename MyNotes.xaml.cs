using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sikai_Project
{
    public partial class MyNotes : Window
    {
        private List<NoteModel> _notes = new List<NoteModel>();
        private string _subjectFilter = "";

        public MyNotes()
        {
            InitializeComponent();
            Navigation.Wire(this);
            Loaded += async delegate { await LoadDataAsync(); };
        }

        private async Task LoadDataAsync()
        {
            if (Session.CurrentUser == null) return;
            try
            {
                _notes = await Db.GetNotesAsync(Session.CurrentUser);
                BuildFilters();
                Render();
            }
            catch (Exception ex) { MessageBox.Show("Could not load notes.\n" + ex.Message, "SIKAI", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void BuildFilters()
        {
            SubjectFilterPanel.Children.Clear();
            AddFilter("All", "");
            foreach (SubjectModel s in _notes.Select(n => new SubjectModel { Code=n.SubjectCode, Name=n.SubjectName }).GroupBy(x=>x.Code).Select(g=>g.First()).OrderBy(x=>x.Code))
                AddFilter(s.Code, s.Code);
        }

        private void AddFilter(string text, string value)
        {
            Button b = new Button { Content=text, Padding=new Thickness(15,8,15,8), Margin=new Thickness(0,0,8,0), Background=new SolidColorBrush(Color.FromRgb(244,241,251)), BorderThickness=new Thickness(0), Tag=value };
            b.Click += delegate { _subjectFilter = (string)b.Tag; Render(); };
            SubjectFilterPanel.Children.Add(b);
        }

        private void Render()
        {
            string q = SearchNotesTextBox.Text == null ? "" : SearchNotesTextBox.Text.Trim().ToLowerInvariant();
            IEnumerable<NoteModel> data = _notes.Where(n => (_subjectFilter=="" || n.SubjectCode==_subjectFilter) &&
                (q=="" || n.Title.ToLowerInvariant().Contains(q) || n.Content.ToLowerInvariant().Contains(q) || n.SubjectName.ToLowerInvariant().Contains(q)));
            if (SortComboBox.SelectedIndex == 1) data=data.OrderBy(n=>n.CreatedAt);
            else if (SortComboBox.SelectedIndex == 2) data=data.OrderBy(n=>n.Title);
            else data=data.OrderByDescending(n=>n.CreatedAt);

            NotesList.Items.Clear();
            foreach (NoteModel n in data)
            {
                StackPanel p=new StackPanel();
                p.Children.Add(new TextBlock{Text=n.Title,FontSize=17,FontWeight=FontWeights.SemiBold});
                p.Children.Add(new TextBlock{Text=n.SubjectCode+" • "+n.CreatedAt.ToString("dd MMM yyyy"),Foreground=Brushes.SlateGray,Margin=new Thickness(0,4,0,6)});
                p.Children.Add(new TextBlock{Text=n.Content,TextWrapping=TextWrapping.Wrap,Foreground=Brushes.Gray});
                NotesList.Items.Add(new Border{Background=Brushes.White,CornerRadius=new CornerRadius(16),Padding=new Thickness(18),Margin=new Thickness(0,0,0,12),Child=p});
            }
            if (NotesList.Items.Count==0) NotesList.Items.Add(new TextBlock{Text="No notes match your search.",Margin=new Thickness(5),Foreground=Brushes.Gray});
        }

        private void SearchNotesTextBox_TextChanged(object sender, TextChangedEventArgs e) { if (NotesList != null) Render(); }
        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (NotesList != null) Render(); }
    }
}
