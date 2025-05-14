using MarathonSkillsApp.Classes;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace MarathonSkillsApp.Pages
{
    /// <summary>
    /// Логика взаимодействия для RunnerInfoManPage.xaml
    /// </summary>
    public partial class RunnerInfoManPage : Page
    {
        private MarathonCountdown countdown;
        private DateTime marathonDate = new DateTime(2025, 10, 20);
        private int runnerId;

        public RunnerInfoManPage(int runnerId)
        {
            InitializeComponent();
            this.runnerId = runnerId;
            countdown = new MarathonCountdown(UpdateCountdownText, marathonDate);
            LoadRunnerData();
        }

        private void UpdateCountdownText(string text)
        {
            CountdownTextBlock.Text = text;
            CountdownText.Text = text;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            countdown.Stop();
        }

        private void LoadRunnerData()
        {
            var runner = ConnectionClass.connect.Runner.FirstOrDefault(r => r.RunnerId == runnerId);
            if (runner == null)
            {
                MessageBox.Show("Бегун не найден!");
                NavigationService.GoBack();
                return;
            }

            var user = runner.User;
            var registration = runner.Registration.FirstOrDefault();
            var charity = registration?.Charity;
            var country = runner.Country;

            // Основная информация
            RunnerEmail.Text = $"Email: {user.Email}";
            RunnerName.Text = $"Имя: {user.FirstName}\nФамилия: {user.LastName}";
            RunnerGender.Text = $"Пол: {(runner.Gender == "M" ? "мужской" : "женский")}";
            RunnerBirthDate.Text = $"Дата рождения: {runner.DateOfBirth?.ToString("dd MMMM yyyy")}";
            RunnerCountry.Text = $"Страна: {country?.CountryName}";

            if (charity != null)
            {
                RunnerCharity.Text = $"Благотворит: {charity.CharityName}";
                RunnerDonation.Text = $"Пожертвовано: ${registration?.Sponsorship.Sum(s => s.Amount) ?? 0}";
            }
            else
            {
                RunnerCharity.Text = "Благотворит: не указано";
                RunnerDonation.Text = "Пожертвовано: $0";
            }

            RunnerPackage.Text = $"Выбранный пакет: {registration?.RaceKitOption?.RaceKitOption1}";
            RunnerDistance.Text = $"Дистанция: {string.Join(", ", registration?.RegistrationEvent.Select(re => re.Event.EventType.EventTypeName))}";

            // Статус регистрации
            if (registration != null)
            {
                PaymentStatus.Text = registration.RegistrationStatusId >= 2 ? "Подтверждена оплата" : "Оплата не подтверждена";
                KitStatus.Text = registration.RegistrationStatusId >= 3 ? "Выдан пакет" : "Пакет не выдан";
                StartStatus.Text = registration.RegistrationStatusId >= 4 ? "Вышел на старт" : "Не вышел на старт";
            }
            else
            {
                PaymentStatus.Text = "Нет данных о регистрации";
                KitStatus.Text = string.Empty;
                StartStatus.Text = string.Empty;
            }
            //LoadRunnerPhoto(runner.Photo);
        }

        private void LoadRunnerPhoto(byte[] photoData)
        {
            try
            {
                if (photoData != null && photoData.Length > 0)
                {
                    using (var stream = new MemoryStream(photoData))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                        RunnerPhoto.Source = bitmap;
                    }
                }
                else
                {
                    // Установка фото-заглушки, если фото нет в БД
                    RunnerPhoto.Source = new BitmapImage(
                        new Uri("/Resourses/no_photo.jpg", UriKind.Relative));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фотографии: {ex.Message}");
                RunnerPhoto.Source = new BitmapImage(
                    new Uri("/Resourses/no_photo.jpg", UriKind.Relative));
            }
        }

        private void PrintBadgeBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Печать бейджа для бегуна", "Печать", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Back_btn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
