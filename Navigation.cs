using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sikai_Project
{
    public static class Navigation
    {
        
        
        
        public static void Wire(Window window)
        {
            if (window == null) return;

            if (window.IsLoaded)
            {
                WireButtons(window);
                return;
            }

            RoutedEventHandler handler = null;
            handler = delegate
            {
                window.Loaded -= handler;
                WireButtons(window);
            };
            window.Loaded += handler;
        }

        private static void WireButtons(Window window)
        {
            foreach (Button button in FindVisualChildren<Button>(window))
            {
                string text = button.Content == null ? "" : button.Content.ToString().Trim();
                if (string.IsNullOrWhiteSpace(text)) continue;

                
                if (button.Tag != null && button.Tag.ToString() == "SIKAI_NAV_WIRED")
                    continue;

                if (text.IndexOf("Dashboard", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    button.Click += delegate { OpenDashboard(window); };
                }
                else if (text.IndexOf("My Notes", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    button.Click += delegate { Open<MyNotes>(window); };
                }
                else if (text.IndexOf("Study Planner", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    button.Click += delegate { Open<StudyPlanner>(window); };
                }
                else if (text.IndexOf("Tasks", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    button.Click += delegate { Open<TasksWindow>(window); };
                }
                else if (text.IndexOf("Subjects", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    button.Click += delegate { Open<subject>(window); };
                }
                else if (text.IndexOf("Resources", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    button.Click += delegate { Open<ResourcesWindow>(window); };
                }
                else if (text.IndexOf("Messages", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         text.IndexOf("Notifications", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    button.Click += delegate { Open<StudentMessagesWindow>(window); };
                }
				else if (text.IndexOf("Enroll", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					button.Click += delegate { Open<EnrollClassWindow>(window); };
				}
				else if (text.IndexOf("Profile", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    button.Click += delegate { Open<ProfileWindow>(window); };
                }
                else if (text.IndexOf("Settings", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    button.Click += delegate { Open<SettingsWindow>(window); };
                }
                else if (text.IndexOf("Logout", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         text.IndexOf("Log Out", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    button.Click += delegate { Logout(window); };
                }
                else
                {
                    continue;
                }

                
                button.Tag = "SIKAI_NAV_WIRED";
            }
        }

        private static void OpenDashboard(Window owner)
        {
            if (owner is DashboardWindow) return;
            Open<DashboardWindow>(owner);
        }

        public static void OpenSubject(Window owner, long subjectId)
        {
            if (Session.CurrentUser == null)
            {
                new LoginWindow().Show();
                owner.Close();
                return;
            }

            SubjectDetails window = new SubjectDetails(subjectId);
            window.Show();
            owner.Close();
        }

        public static void BackToSubjects(Window owner)
        {
            Open<subject>(owner);
        }

        public static void Logout(Window owner)
        {
            Session.CurrentUser = null;
            new LoginWindow().Show();
            owner.Close();
        }

        private static void Open<T>(Window owner) where T : Window, new()
        {
            if (Session.CurrentUser == null)
            {
                new LoginWindow().Show();
                owner.Close();
                return;
            }

            T nextWindow;
            try
            {
                nextWindow = new T();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not open " + typeof(T).Name + ".\n\n" + ex.Message,
                    "SIKAI",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            nextWindow.Show();
            owner.Close();
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject obj)
            where T : DependencyObject
        {
            if (obj == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                T typed = child as T;
                if (typed != null) yield return typed;

                foreach (T item in FindVisualChildren<T>(child))
                    yield return item;
            }
        }
    }
}
