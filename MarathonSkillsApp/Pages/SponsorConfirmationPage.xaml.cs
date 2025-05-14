using MarathonSkillsApp.Classes;
using MarathonSkillsApp.DB_model;
using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для SponsorConfirmationPage.xaml
    /// </summary>
    public partial class SponsorConfirmationPage : Page
    {
        private MarathonCountdown countdown;
        private DateTime marathonDate = new DateTime(2025, 10, 20);
        private int _registrationId;
        private decimal _donationAmount;

        public SponsorConfirmationPage(int registrationId, decimal donationAmount)
        {
            InitializeComponent();
            countdown = new MarathonCountdown(UpdateCountdownText, marathonDate);
            _registrationId = registrationId;
            _donationAmount = donationAmount;
            LoadThankYouInfo();

        }
        private void UpdateCountdownText(string text)
        {
            // Обновляем текст в TextBlock
            CountdownTextBlock.Text = text;
        }

        // При закрытии окна или перехода со страницы остановите таймер
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            countdown.Stop();
        }

        private void LoadThankYouInfo()
        {
            try
            {
                using (var context = new mrthnskillsEntities())
                {
                    var registration = context.Registration
                        .Include("Runner")
                        .Include("Runner.User")
                        .Include("Runner.Country")
                        .Include("Charity")
                        .FirstOrDefault(r => r.RegistrationId == _registrationId);

                    if (registration == null)
                        return;

                    var runner = registration.Runner;
                    var user = runner.User;
                    var charity = registration.Charity;

                    // Получаем страну по коду
                    var country = context.Country.FirstOrDefault(c => c.CountryCode == runner.CountryCode);
                    string countryName = country?.CountryName ?? "Неизвестная страна";

                    // 🏃 Текст бегуна: Имя Фамилия (RU) из Россия
                    RunnerInfoTextBlock.Text = $"{user.FirstName} {user.LastName} ({runner.RunnerId}) из {countryName}";

                    // 💖 Название благотворительности
                    CharityNameTextBlock.Text = charity?.CharityName ?? "Без названия";

                    // 💵 Сумма пожертвования
                    AmountTextBlock.Text = $"${_donationAmount}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке информации: " + ex.Message);
            }
        }

        private void Back_btn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void Btn_back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
