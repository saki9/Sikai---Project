using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sikai_Project
{
    public partial class TeacherMessagesWindow : Window
    {
        private List<MessageContact> _contacts = new List<MessageContact>();
        public TeacherMessagesWindow() { InitializeComponent(); Loaded += async delegate { await LoadContactsAsync(); }; }
        private async Task LoadContactsAsync()
        {
            if (Session.CurrentUser == null || Session.CurrentUser.Role != Role.Teacher) { StatusText.Text = "Please sign in with a teacher account."; return; }
            try { _contacts = await Db.GetMessageContactsAsync(Session.CurrentUser); ContactsList.ItemsSource = _contacts; if (_contacts.Count > 0) ContactsList.SelectedIndex = 0; else { MessagesPanel.Children.Clear(); StatusText.Text = "No enrolled students yet."; } }
            catch (Exception ex) { StatusText.Text = ex.Message; }
        }
        private async void ContactsList_SelectionChanged(object sender, SelectionChangedEventArgs e) { await LoadConversationAsync(); }
        private async Task LoadConversationAsync()
        {
            MessageContact c = ContactsList.SelectedItem as MessageContact; if (c == null || Session.CurrentUser == null) return;
            ConversationTitle.Text = c.FullName + "  ·  " + c.Email; MessagesPanel.Children.Clear();
            try
            {
                List<MessageItem> messages = await Db.GetMessagesAsync(Session.CurrentUser, c);
                foreach (MessageItem m in messages) AddMessage(m);
                if (messages.Count == 0) MessagesPanel.Children.Add(new TextBlock { Text = "No messages yet. Send a message to begin the conversation.", Foreground = Brushes.Gray, Margin = new Thickness(8, 12, 8, 12), TextWrapping = TextWrapping.Wrap });
                MessagesScrollViewer.ScrollToEnd();
            }
            catch (Exception ex) { StatusText.Text = ex.Message; }
        }
        private void AddMessage(MessageItem m)
        {
            StackPanel bubble = new StackPanel(); bubble.Children.Add(new TextBlock { Text = (m.IsMine ? "You" : m.SenderName) + "  ·  " + m.SentAt.ToString("dd MMM, h:mm tt"), FontSize = 11, Foreground = Brushes.Gray });
            bubble.Children.Add(new TextBlock { Text = m.Body, TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DarkSlateGray, Margin = new Thickness(0, 4, 0, 0) });
            MessagesPanel.Children.Add(new Border { Background = m.IsMine ? new SolidColorBrush(Color.FromRgb(239, 235, 255)) : new SolidColorBrush(Color.FromRgb(247, 246, 250)), CornerRadius = new CornerRadius(10), Padding = new Thickness(12), Margin = new Thickness(6, 4, 6, 4), HorizontalAlignment = m.IsMine ? HorizontalAlignment.Right : HorizontalAlignment.Left, MaxWidth = 600, Child = bubble });
        }
        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            MessageContact c = ContactsList.SelectedItem as MessageContact; string body = MessageBox.Text.Trim();
            if (c == null || body.Length == 0) { StatusText.Text = "Choose a student and enter a message."; return; }
            try { await Db.SendMessageAsync(Session.CurrentUser, c, body); MessageBox.Clear(); StatusText.Foreground = Brushes.ForestGreen; StatusText.Text = "Message sent."; await LoadConversationAsync(); }
            catch (Exception ex) { StatusText.Foreground = Brushes.Firebrick; StatusText.Text = ex.Message; }
        }
        private void Dashboard_Click(object s, RoutedEventArgs e) { new TeacherDashboardWindow().Show(); Close(); }
        private void Courses_Click(object s, RoutedEventArgs e) { new TeacherCoursesWindow().Show(); Close(); }
        private void Students_Click(object s, RoutedEventArgs e) { TeacherNavigation.OpenStudents(this); }
        private void Materials_Click(object s, RoutedEventArgs e) { new TeacherMaterialsWindow().Show(); Close(); }
        private void Assignments_Click(object s, RoutedEventArgs e) { new TeacherAssignmentsWindow().Show(); Close(); }
        private void Pending_Click(object s, RoutedEventArgs e) { new TeacherPendingSubmissionsWindow().Show(); Close(); }
        private void Logout_Click(object s, RoutedEventArgs e) { TeacherNavigation.Logout(this); }
    }
}
