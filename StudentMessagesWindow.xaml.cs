using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sikai_Project
{
    public partial class StudentMessagesWindow : Window
    {
        private List<MessageContact> _contacts = new List<MessageContact>();
        public StudentMessagesWindow()
        {
            InitializeComponent();
            StudentNameText.Text = Session.CurrentUser == null ? "Student" : Session.CurrentUser.FullName;
            Loaded += async delegate { await LoadAsync(); };
        }
        private async Task LoadAsync()
        {
            if (Session.CurrentUser == null || Session.CurrentUser.Role != Role.Student) { StatusText.Text = "Please sign in with a student account."; return; }
            try
            {
                _contacts = await Db.GetMessageContactsAsync(Session.CurrentUser); ContactsList.ItemsSource = _contacts;
                List<NotificationItem> notifications = await Db.GetNotificationsAsync(Session.CurrentUser); RenderNotifications(notifications);
                await Db.MarkNotificationsReadAsync(Session.CurrentUser);
                if (_contacts.Count > 0) ContactsList.SelectedIndex = 0;
                else MessagesPanel.Children.Add(new TextBlock { Text = "No teachers are linked to your classes yet. Join a class to start messaging.", TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Gray, Margin = new Thickness(8, 12, 8, 12) });
            }
            catch (Exception ex) { StatusText.Text = "Could not load messages: " + ex.Message; }
        }
        private void RenderNotifications(List<NotificationItem> notifications)
        {
            NotificationsPanel.Children.Clear();
            foreach (NotificationItem n in notifications)
            {
                StackPanel content = new StackPanel(); content.Children.Add(new TextBlock { Text = n.Title, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
                content.Children.Add(new TextBlock { Text = n.Body, TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Gray, Margin = new Thickness(0, 4, 0, 4) });
                content.Children.Add(new TextBlock { Text = n.CreatedAt.ToString("dd MMM, h:mm tt") + (n.IsRead ? " · Read" : " · New"), FontSize = 10, Foreground = Brushes.SlateGray });
                NotificationsPanel.Children.Add(new Border { Background = n.IsRead ? new SolidColorBrush(Color.FromRgb(249, 248, 252)) : new SolidColorBrush(Color.FromRgb(239, 235, 255)), CornerRadius = new CornerRadius(9), Padding = new Thickness(10), Margin = new Thickness(0, 0, 0, 9), Child = content });
            }
            if (notifications.Count == 0) NotificationsPanel.Children.Add(new TextBlock { Text = "No notifications yet.", Foreground = Brushes.Gray, TextWrapping = TextWrapping.Wrap });
        }
        private async void ContactsList_SelectionChanged(object sender, SelectionChangedEventArgs e) { await LoadConversationAsync(); }
        private async Task LoadConversationAsync()
        {
            MessageContact c = ContactsList.SelectedItem as MessageContact; if (c == null) return;
            ConversationTitle.Text = c.FullName; MessagesPanel.Children.Clear();
            try
            {
                List<MessageItem> messages = await Db.GetMessagesAsync(Session.CurrentUser, c);
                foreach (MessageItem m in messages) AddMessage(m);
                if (messages.Count == 0) MessagesPanel.Children.Add(new TextBlock { Text = "No messages yet. Write the first message below.", Foreground = Brushes.Gray, Margin = new Thickness(8, 12, 8, 12), TextWrapping = TextWrapping.Wrap });
                MessagesScrollViewer.ScrollToEnd();
            }
            catch (Exception ex) { StatusText.Text = ex.Message; }
        }
        private void AddMessage(MessageItem m)
        {
            StackPanel body = new StackPanel(); body.Children.Add(new TextBlock { Text = (m.IsMine ? "You" : m.SenderName) + " · " + m.SentAt.ToString("dd MMM, h:mm tt"), FontSize = 11, Foreground = Brushes.Gray });
            body.Children.Add(new TextBlock { Text = m.Body, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 0), Foreground = Brushes.DarkSlateGray });
            MessagesPanel.Children.Add(new Border { Background = m.IsMine ? new SolidColorBrush(Color.FromRgb(239, 235, 255)) : new SolidColorBrush(Color.FromRgb(247, 246, 250)), CornerRadius = new CornerRadius(10), Padding = new Thickness(12), Margin = new Thickness(5, 4, 5, 4), HorizontalAlignment = m.IsMine ? HorizontalAlignment.Right : HorizontalAlignment.Left, MaxWidth = 560, Child = body });
        }
        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            MessageContact c = ContactsList.SelectedItem as MessageContact; string body = MessageBox.Text.Trim();
            if (c == null || body.Length == 0) { StatusText.Text = "Choose a teacher and enter a message."; return; }
            try { await Db.SendMessageAsync(Session.CurrentUser, c, body); MessageBox.Clear(); StatusText.Foreground = Brushes.ForestGreen; StatusText.Text = "Message sent."; await LoadConversationAsync(); }
            catch (Exception ex) { StatusText.Foreground = Brushes.Firebrick; StatusText.Text = ex.Message; }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e) { new DashboardWindow().Show(); Close(); }
    }
}
