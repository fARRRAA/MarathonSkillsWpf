using MarathonSkillsApp.Classes;
using MarathonSkillsApp.DB_model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MarathonSkillsApp.Pages
{
    /// <summary>
    /// Логика взаимодействия для CharityManagementPage.xaml
    /// </summary>
    public partial class CharityManagementPage : Page
    {
        private readonly DateTime _raceStartTime = new DateTime(2025, 11, 24, 6, 0, 0);
        private DispatcherTimer _timer;
        public ObservableCollection<Charity> Charities { get; set; }

        public CharityManagementPage()
        {
            InitializeComponent();
            DataContext = this;
            Charities = new ObservableCollection<Charity>();

            LoadCharitiesFromDatabase();
            InitializeCountdownTimer();
        }

        public void LoadCharitiesFromDatabase()
        {
            try
            {
                Charities.Clear();

                var dbCharities = ConnectionClass.connect.Charity.ToList();

                foreach (var charity in dbCharities)
                {
                    Charities.Add(new Charity
                    {
                        Id = charity.CharityId,
                        Name = charity.CharityName,
                        Description = charity.CharityDescription,
                        LogoPath = $"pack://application:,,,/Images/{charity.CharityLogo}"
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных из базы данных: {ex.Message}",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void InitializeCountdownTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
            UpdateCountdown();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateCountdown();
        }

        private void UpdateCountdown()
        {
            TimeSpan timeLeft = _raceStartTime - DateTime.Now;

            if (timeLeft.TotalMilliseconds > 0)
            {
                CountdownText.Text = $"{timeLeft.Days} дней {timeLeft.Hours} часов и {timeLeft.Minutes} минут до старта марафона!";
            }
            else
            {
                CountdownText.Text = "Марафон уже начался!";
                _timer.Stop();
            }
        }

        private void AddCharity_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddCharityPage());
        }

        private void EditCharity_Click(object sender, RoutedEventArgs e)
        {
            var charity = (sender as Button).DataContext as Charity;

            if (charity != null)
            {
                // Переход на страницу редактирования с передачей объекта Charity
                NavigationService.Navigate(new EditCharityPage(charity));
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdministratorMenuPage());
        }
    }

    public class Charity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LogoPath { get; set; }

        public BitmapImage LogoImage
        {
            get
            {
                try
                {
                    return new BitmapImage(new Uri(LogoPath, UriKind.Absolute));
                }
                catch
                {
                    return new BitmapImage(new Uri("pack://application:,,,/Images/default_logo.png", UriKind.Absolute));
                }
            }
        }
    }
}
