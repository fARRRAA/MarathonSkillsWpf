using MarathonSkillsApp.Classes;
using MarathonSkillsApp.DB_model;
using MarathonSkillsApp.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    public partial class SponsorRunnerPage : Page
    {
        private MarathonCountdown countdown;
        private DateTime marathonDate = new DateTime(2025, 10, 20);
        private DB_model.Charity selectedCharity;

        public SponsorRunnerPage()
        {
            InitializeComponent();
            LoadRunners();
            InitializeYearComboBox();
            InitializeMonthComboBox();
            countdown = new MarathonCountdown(UpdateCountdownText, marathonDate);
        }

        private void InitializeYearComboBox()
        {
            int currentYear = DateTime.Now.Year;
            for (int i = 0; i < 10; i++)
            {
                YearComboBox.Items.Add((currentYear + i).ToString());
            }
            YearComboBox.SelectedIndex = 0;
        }

        private void InitializeMonthComboBox()
        {
            int currentMonth = DateTime.Now.Month;
            MonthComboBox.SelectedIndex = currentMonth - 1;
        }

        private void UpdateCountdownText(string text)
        {
            CountdownTextBlock.Text = text;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            countdown.Stop();
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCharity != null)
            {
                SponsorInfoWindow infoWindow = new SponsorInfoWindow(selectedCharity);
                infoWindow.Show();
            }
            else
            {
                MessageBox.Show("Пожалуйста, сначала выберите бегуна", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void IncreaseAmount_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(AmountTextBox.Text, out decimal value))
            {
                value += 10;
                AmountTextBox.Text = value.ToString();
            }
        }

        private void DecreaseAmount_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(AmountTextBox.Text, out decimal value) && value >= 10)
            {
                value -= 10;
                AmountTextBox.Text = value.ToString();
            }
        }

        private void PayButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
            {
                MessageBox.Show("Пожалуйста, заполните все поля корректно.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var sponsorship = new Sponsorship
            {
                SponsorName = SponsorNameTextBox.Text,
                RegistrationId = (int)RunnerComboBox.SelectedValue,
                Amount = decimal.Parse(AmountTextBox.Text)
            };

            using (var context = new mrthnskillsEntities())
            {
                context.Sponsorship.Add(sponsorship);
                context.SaveChanges();
            }

            NavigationService.Navigate(new SponsorConfirmationPage(sponsorship.RegistrationId, sponsorship.Amount));
        }

        public bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(SponsorNameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите ваше имя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (RunnerComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите бегуна.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(CardHolderTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите имя владельца карты.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!Regex.IsMatch(CardNumberTextBox.Text, @"^\d{16}$"))
            {
                MessageBox.Show("Номер карты должен содержать 16 цифр.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (MonthComboBox.SelectedItem == null || YearComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите срок действия карты.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // Проверка срока действия карты
            int month = int.Parse((string)((ComboBoxItem)MonthComboBox.SelectedItem).Content);
            int year = int.Parse((string)YearComboBox.SelectedItem);
            DateTime expirationDate = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);

            if (expirationDate < DateTime.Now)
            {
                MessageBox.Show("Срок действия карты истёк.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!Regex.IsMatch(CvcTextBox.Text, @"^\d{3}$"))
            {
                MessageBox.Show("CVC код должен содержать 3 цифры.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!decimal.TryParse(AmountTextBox.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Пожалуйста, введите корректную сумму пожертвования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        public void LoadRunners()
        {
            using (var context = new mrthnskillsEntities())
            {
                var runners = (from r in context.Runner
                               join u in context.User on r.Email equals u.Email
                               join reg in context.Registration on r.RunnerId equals reg.RunnerId
                               where reg.RegistrationStatusId == 1 // активные регистрации
                               select new
                               {
                                   u.FirstName,
                                   u.LastName,
                                   r.CountryCode,
                                   reg.RegistrationId,
                                   reg.CharityId
                               }).ToList() // Материализуем данные перед форматированием
                              .Select(x => new RunnerItem
                              {
                                  Display = $"{x.FirstName} {x.LastName} - ({x.CountryCode})",
                                  RegistrationId = x.RegistrationId,
                                  CharityId = x.CharityId
                              });

                RunnerComboBox.ItemsSource = runners.ToList();
            }
        }

        public class RunnerItem
        {
            public string Display { get; set; }
            public int RegistrationId { get; set; }
            public int CharityId { get; set; }
        }

        private void AmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(AmountTextBox.Text, out decimal value))
            {
                AmountTextBlock.Text = $"${value}";
            }
            else
            {
                AmountTextBlock.Text = "$0";
            }
        }

        private void RunnerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedRunner = RunnerComboBox.SelectedItem as RunnerItem;

            if (selectedRunner != null)
            {
                using (var context = new mrthnskillsEntities())
                {
                    selectedCharity = context.Charity.FirstOrDefault(c => c.CharityId == selectedRunner.CharityId);

                    if (selectedCharity != null)
                    {
                        CharityNameTextBlock.Text = selectedCharity.CharityName;
                    }
                }
            }
        }

        private void Back_btn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void CardNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void CvcTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void AmountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры и запятую/точку (для десятичных чисел)
            if (!char.IsDigit(e.Text, 0) && e.Text != "," && e.Text != ".")
            {
                e.Handled = true;
            }
        }
    }
}