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
    public partial class ResourcesWindow : Window
    {
        private List<ResourceModel> _resources=new List<ResourceModel>();
        public ResourcesWindow(){InitializeComponent();Navigation.Wire(this);Loaded+=async delegate{await LoadDataAsync();};}
        private async Task LoadDataAsync(){if(Session.CurrentUser==null)return;try{_resources=await Db.GetResourcesAsync(Session.CurrentUser);Render();}catch(Exception ex){MessageBox.Show("Could not load resources.\n"+ex.Message,"SIKAI",MessageBoxButton.OK,MessageBoxImage.Error);}}
        private void Render()
        {
            string q=(SearchResourcesTextBox.Text??"").Trim().ToLowerInvariant();
            string f=ResourceFilterComboBox.SelectedIndex<=0?"":((ComboBoxItem)ResourceFilterComboBox.SelectedItem).Content.ToString();
            IEnumerable<ResourceModel> data=_resources.Where(r=>(f==""||r.ResourceType.Equals(f,StringComparison.OrdinalIgnoreCase))&&(q==""||r.Title.ToLowerInvariant().Contains(q)||r.Description.ToLowerInvariant().Contains(q)||r.SubjectName.ToLowerInvariant().Contains(q))).OrderByDescending(r=>r.UploadedAt);
            ResourcesList.Items.Clear();
            foreach(ResourceModel r in data)
            {
                StackPanel p=new StackPanel();
                p.Children.Add(new TextBlock{Text=r.Title,FontSize=17,FontWeight=FontWeights.SemiBold});
                p.Children.Add(new TextBlock{Text=r.SubjectCode+" • "+r.ResourceType+" • "+r.UploadedAt.ToString("dd MMM yyyy"),Foreground=Brushes.SlateGray,Margin=new Thickness(0,4,0,5)});
                p.Children.Add(new TextBlock{Text=r.Description,TextWrapping=TextWrapping.Wrap,Foreground=Brushes.Gray});
                Button open=new Button{Content="Open",Tag=r,Margin=new Thickness(0,10,0,0),Padding=new Thickness(12,6,12,6),HorizontalAlignment=HorizontalAlignment.Left,Background=new SolidColorBrush(Color.FromRgb(243,240,248)),BorderThickness=new Thickness(0)};
                open.Click+=Open_Click;p.Children.Add(open);
                ResourcesList.Items.Add(new Border{Background=Brushes.White,CornerRadius=new CornerRadius(16),Padding=new Thickness(18),Margin=new Thickness(0,0,0,12),Child=p});
            }
            if(ResourcesList.Items.Count==0)ResourcesList.Items.Add(new TextBlock{Text="No resources match your search.",Foreground=Brushes.Gray});
        }
        private void Open_Click(object sender,RoutedEventArgs e)
        {
            ResourceModel r=(ResourceModel)((Button)sender).Tag;
            if(string.IsNullOrWhiteSpace(r.Url)){MessageBox.Show("This resource has no URL stored in the database.","SIKAI");return;}
            try{Process.Start(r.Url);}catch(Exception ex){MessageBox.Show("Could not open resource.\n"+ex.Message,"SIKAI",MessageBoxButton.OK,MessageBoxImage.Error);}
        }
        private void SearchResourcesTextBox_TextChanged(object sender,TextChangedEventArgs e){if(ResourcesList!=null)Render();}
        private void ResourceFilterComboBox_SelectionChanged(object sender,SelectionChangedEventArgs e){if(ResourcesList!=null)Render();}
    }
}
