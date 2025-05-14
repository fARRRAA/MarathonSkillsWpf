using MarathonSkillsApp.Classes;
using MarathonSkillsApp.DB_model;
using MarathonSkillsApp.Window;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MarathonSkillsApp.Pages
{
    public partial class RegisterForMarathonPage : Page
    {
        private MarathonCountdown countdown;
        private DateTime marathonDate = new DateTime(2025, 10, 20);

        public RegisterForMarathonPage()
        {
            InitializeComponent();
            countdown = new MarathonCountdown(UpdateCountdownText, marathonDate);
            this.Loaded += (s, e) =>
            {
                LoadCharities();
                UpdateTotalCost();
            };
        }

        private void UpdateCountdownText(string text)
        {
            CountdownTextBlock.Text = text;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            countdown.Stop();
        }

        private void RegisterForMarathonPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCharities();
            UpdateTotalCost();
        }

        private void LoadCharities()
        {
            using (var context = new mrthnskillsEntities())
            {
                cbCharity.ItemsSource = context.Charity.ToList();
                cbCharity.DisplayMemberPath = "CharityName";
                cbCharity.SelectedIndex = 0;
            }
        }

        private void UpdateTotalCost()
        {
            if (cbFullMarathon == null || cbHalfMarathon == null || cbSmallRace == null ||
        rbOptionA == null || rbOptionB == null || rbOptionC == null)
                return;

            decimal total = 0;

            // Стоимость забега
            if (cbFullMarathon.IsChecked == true) total += 145;
            if (cbHalfMarathon.IsChecked == true) total += 75;
            if (cbSmallRace.IsChecked == true) total += 20;

            // Стоимость комплекта
            if (rbOptionB.IsChecked == true) total += 20;
            else if (rbOptionC.IsChecked == true) total += 45;

            // Пожертвование
            if (decimal.TryParse(tbDonationAmount.Text, out decimal donation))
                total += donation;

            txtRegistrationFee.Text = $"${total}";
        }

        private void EventType_Checked(object sender, RoutedEventArgs e)
        {
            if (cbFullMarathon == null || cbHalfMarathon == null || cbSmallRace == null)
                return;

            UpdateTotalCost();
        }

        private void RaceKit_Checked(object sender, RoutedEventArgs e)
        {
            if (rbOptionA == null || rbOptionB == null || rbOptionC == null)
                return;

            UpdateTotalCost();
        }

        private void DonationAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTotalCost();
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (cbCharity.SelectedItem is Charity selectedCharity)
            {
                SponsorInfoWindow infoWindow = new SponsorInfoWindow(selectedCharity);
                infoWindow.Show();
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAnyEventSelected())
            {
                MessageBox.Show("Пожалуйста, выберите хотя бы один вид марафона.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtRegistrationFee.Text.Replace("$", ""), out decimal totalCost) || totalCost <= 0)
            {
                MessageBox.Show("Некорректная сумма регистрационного взноса.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(tbDonationAmount.Text, out decimal donationAmount) || donationAmount < 0)
            {
                MessageBox.Show("Некорректное значение в поле взноса.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var context = new mrthnskillsEntities())
                {
                    var runner = context.Runner.FirstOrDefault(r => r.Email == CurrentUser.Email);
                    if (runner == null)
                    {
                        MessageBox.Show("Не удалось найти текущего пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Получаем ID выбранной благотворительной организации
                    var selectedCharity = cbCharity.SelectedItem as DB_model.Charity;
                    if (selectedCharity == null)
                    {
                        MessageBox.Show("Пожалуйста, выберите благотворительную организацию.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string raceKitOption = rbOptionA.IsChecked == true ? "A" :
                                         rbOptionB.IsChecked == true ? "B" : "C";

                    var registration = new Registration
                    {
                        RunnerId = runner.RunnerId,
                        RegistrationDateTime = DateTime.Now,
                        RaceKitOptionId = raceKitOption,
                        RegistrationStatusId = 1, // Pending
                        Cost = totalCost,
                        CharityId = selectedCharity.CharityId, // Используем ID напрямую
                        SponsorshipTarget = donationAmount
                    };

                    context.Registration.Add(registration);
                    context.SaveChanges();

                    // Добавляем выбранные события
                    if (cbFullMarathon.IsChecked == true)
                        AddRegistrationEvent(context, registration.RegistrationId, "FM");
                    if (cbHalfMarathon.IsChecked == true)
                        AddRegistrationEvent(context, registration.RegistrationId, "HM");
                    if (cbSmallRace.IsChecked == true)
                        AddRegistrationEvent(context, registration.RegistrationId, "FR");

                    MessageBox.Show("Регистрация успешно завершена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService.Navigate(new RegistationConfirmationPage());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsAnyEventSelected()
        {
            return cbFullMarathon.IsChecked == true ||
                   cbHalfMarathon.IsChecked == true ||
                   cbSmallRace.IsChecked == true;
        }

        private void AddRegistrationEvent(mrthnskillsEntities context, int registrationId, string eventTypeId)
        {
            var events = context.Event.Where(ev => ev.EventTypeId == eventTypeId).ToList();
            foreach (var ev in events)
            {
                context.RegistrationEvent.Add(new RegistrationEvent
                {
                    RegistrationId = registrationId,
                    EventId = ev.EventId
                });
            }
            context.SaveChanges();
        }

        private void LogOut_btn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MenuPage());
        }

        private void Back_btn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void Cancel_btn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}