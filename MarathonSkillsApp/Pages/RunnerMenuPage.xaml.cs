using MarathonSkillsApp.Classes;
using MarathonSkillsApp.DB_model;
using MarathonSkillsApp.Window;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MarathonSkillsApp.Pages
{
    public partial class RunnerMenuPage : Page
    {
        private readonly MarathonCountdown _countdown;
        private readonly DateTime _marathonDate = new DateTime(2025, 10, 20);
        private readonly mrthnskillsEntities _db = new mrthnskillsEntities();

        public RunnerMenuPage()
        {
            InitializeComponent();
            _countdown = new MarathonCountdown(UpdateCountdownText, _marathonDate);
        }

        private void UpdateCountdownText(string text)
        {
            Dispatcher.Invoke(() => CountdownTextBlock.Text = text);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _countdown.Stop();
            _db.Dispose();
        }

        private void Contacts_btn_Click(object sender, RoutedEventArgs e)
        {
            new ContactsWindow().ShowDialog();
        }

        private void LogOut_btn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MenuPage());
        }

        private void RegistForEvent_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegisterForMarathonPage());
        }

        private void EditProfile_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditRunnerProfilePage(CurrentUser.Email));
        }

        private void MyResult_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MyResultsPage(CurrentUser.Email));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Получаем RegistrationId для текущего пользователя
            var registration = _db.Registration
                .FirstOrDefault(r => r.Runner.Email == CurrentUser.Email &&
                                   r.RegistrationStatusId == 1); // 1 = Активная регистрация

            if (registration != null)
            {
                NavigationService.Navigate(new MySponsorshipPage(registration.RegistrationId));
            }
            else
            {
                MessageBox.Show("У вас нет активной регистрации на марафон", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Back_btn_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}