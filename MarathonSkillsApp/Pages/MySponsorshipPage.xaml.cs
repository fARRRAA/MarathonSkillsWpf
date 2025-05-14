using MarathonSkillsApp.DB_model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MarathonSkillsApp.Pages
{
    public partial class MySponsorshipPage : Page
    {
        private readonly DateTime _raceStartTime = new DateTime(2025, 10, 20);
        private DispatcherTimer _timer;
        private readonly mrthnskillsEntities _dbContext;
        private readonly int _registrationId; // Изменяем с RunnerId на RegistrationId

        public MySponsorshipPage(int registrationId) // Изменяем параметр конструктора
        {
            InitializeComponent();
            _dbContext = new mrthnskillsEntities();
            _registrationId = registrationId;
            Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
            UpdateCountdown();
            LoadSponsorshipData();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateCountdown();
        }

        private void UpdateCountdown()
        {
            TimeSpan timeLeft = _raceStartTime - DateTime.Now;
            if (timeLeft.TotalSeconds > 0)
            {
                CountdownTextBlock.Text = string.Format("{0} дней {1} часов и {2} минут до старта марафона!",
                    timeLeft.Days, timeLeft.Hours, timeLeft.Minutes);
            }
            else
            {
                CountdownTextBlock.Text = "Марафон уже начался!";
            }
        }

        private void LoadSponsorshipData()
        {
            try
            {
                var registration = _dbContext.Registration
                    .FirstOrDefault(r => r.RegistrationId == _registrationId);

                if (registration == null)
                {
                    ShowNoRegistrationMessage();
                    return;
                }

                var charity = registration.Charity;
                if (charity != null)
                {
                    DisplayCharityInfo(charity);
                }

                var sponsorships = _dbContext.Sponsorship
                    .Where(s => s.RegistrationId == registration.RegistrationId)
                    .ToList();

                DisplaySponsorships(sponsorships);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowNoRegistrationMessage()
        {
            CharityPanel.Visibility = Visibility.Collapsed;
            SponsorsPanel.Visibility = Visibility.Collapsed;

            var messageText = new TextBlock
            {
                Text = "Вы не зарегистрированы на Marathon Skills 2025 или у вас нет спонсоров.",
                FontSize = 16,
                Foreground = Brushes.Black, 
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            MainStackPanel.Children.Add(messageText);
        }

        private void DisplayCharityInfo(DB_model.Charity charity)
        {
            CharityNameTextBlock.Text = charity.CharityName;
            CharityDescriptionTextBlock.Text = string.IsNullOrWhiteSpace(charity.CharityDescription)
                ? "Описание отсутствует"
                : charity.CharityDescription;

            string logoFileName = string.IsNullOrEmpty(charity.CharityLogo)
                ? "default_logo.png"
                : charity.CharityLogo;

            string imagePath = $"pack://application:,,,/Images/{logoFileName}";

            try
            {
                LogoImage.Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
            }
            catch
            {
                LogoImage.Source = new BitmapImage(
                    new Uri("pack://application:,,,/Images/default_logo.png", UriKind.Absolute));
            }
        }

        private void DisplaySponsorships(List<Sponsorship> sponsorships)
        {
            SponsorsItemsControl.ItemsSource = null;

            if (sponsorships.Count == 0)
            {
                var noSponsorsText = new TextBlock
                {
                    Text = "У вас пока нет спонсоров.",
                    FontSize = 14,
                    Foreground = Brushes.Black, 
                    Margin = new Thickness(0, 10, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                SponsorsListPanel.Children.Add(noSponsorsText);
                TotalAmountTextBlock.Text = "Всего $0";
                return;
            }

            var sponsorList = sponsorships.Select(s => new
            {
                SponsorName = s.SponsorName,
                Amount = s.Amount
            }).ToList();

            SponsorsItemsControl.ItemsSource = sponsorList;
            TotalAmountTextBlock.Text = $"Всего {sponsorships.Sum(s => s.Amount):C}";
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MenuPage());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RunnerMenuPage());
        }
    }
}