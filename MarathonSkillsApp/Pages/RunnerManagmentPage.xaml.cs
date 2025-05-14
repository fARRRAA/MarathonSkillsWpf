using MarathonSkillsApp.Classes;
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
using Microsoft.Win32;
using System.IO;

namespace MarathonSkillsApp.Pages
{
    /// <summary>
    /// Логика взаимодействия для RunnerManagmentPage.xaml
    /// </summary>
    public partial class RunnerManagmentPage : Page
    {
        private MarathonCountdown countdown;
        private DateTime marathonDate = new DateTime(2025, 10, 20);

        public RunnerManagmentPage()
        {
            InitializeComponent();
            countdown = new MarathonCountdown(UpdateCountdownText, marathonDate);

            LoadComboBoxes();
            LoadRunners();
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
        private void LoadComboBoxes()
        {
            // Заполняем статус регистрации
            StatusComboBox.ItemsSource = ConnectionClass.connect.RegistrationStatus
                .Select(s => new { s.RegistrationStatusId, s.RegistrationStatus1 })
                .ToList();
            StatusComboBox.DisplayMemberPath = "RegistrationStatus1";
            StatusComboBox.SelectedValuePath = "RegistrationStatusId";

            // Заполняем типы дистанций
            DistanceComboBox.ItemsSource = ConnectionClass.connect.EventType
                .Select(e => new { e.EventTypeId, e.EventTypeName })
                .ToList();
            DistanceComboBox.DisplayMemberPath = "EventTypeName";
            DistanceComboBox.SelectedValuePath = "EventTypeId";

            // Заполняем параметры сортировки
            SortComboBox.Items.Add("Имя");
            SortComboBox.Items.Add("Фамилия");
            SortComboBox.Items.Add("Email");
        }
        private void LoadRunners()
        {
            var marathon = ConnectionClass.connect.Marathon
                .FirstOrDefault(m => m.YearHeld == 2015);

            if (marathon == null)
            {
                MessageBox.Show("Марафон 2015 года не найден.");
                return;
            }

            var query = ConnectionClass.connect.RegistrationEvent
                .Where(re => re.Event.MarathonId == marathon.MarathonId);

            // Фильтрация по статусу
            if (StatusComboBox.SelectedValue != null)
            {
                byte selectedStatus = (byte)StatusComboBox.SelectedValue;
                query = query.Where(re => re.Registration.RegistrationStatusId == selectedStatus);
            }

            // Фильтрация по дистанции
            if (DistanceComboBox.SelectedValue != null)
            {
                string selectedDistance = DistanceComboBox.SelectedValue.ToString();
                query = query.Where(re => re.Event.EventTypeId == selectedDistance);
            }

            var runners = query
                .Select(re => new
                {
                     re.Registration.Runner.RunnerId, // Добавляем RunnerId
                    re.Registration.Runner.User.FirstName,
                    re.Registration.Runner.User.LastName,
                        re.Registration.Runner.User.Email,
                        Status = re.Registration.RegistrationStatus.RegistrationStatus1
                })
                .Distinct()
                .ToList();

            // Сортировка
            if (SortComboBox.SelectedItem != null)
            {
                string selectedSort = SortComboBox.SelectedItem.ToString();
                if (selectedSort == "Имя")
                    runners = runners.OrderBy(r => r.FirstName).ToList();
                else if (selectedSort == "Фамилия")
                    runners = runners.OrderBy(r => r.LastName).ToList();
                else if (selectedSort == "Email")
                    runners = runners.OrderBy(r => r.Email).ToList();
            }
            else
            {
                runners = runners.OrderBy(r => r.LastName).ToList();
            }

            // Устанавливаем список бегунов в ItemsControl
            RunnerListPanel.ItemsSource = runners;

            TotalRunnersTextBlock.Text = $"Всего бегунов: {runners.Count}";
        }

        private void ViewRunner_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int runnerId)
            {
                NavigationService.Navigate(new RunnerInfoManPage(runnerId));
            }
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            LoadRunners();
        }

        private void ExportCsvButton_Click(object sender, RoutedEventArgs e)
        {
            var marathon = ConnectionClass.connect.Marathon
                .FirstOrDefault(m => m.YearHeld == 2015);

            if (marathon == null)
            {
                MessageBox.Show("Марафон 2015 года не найден.");
                return;
            }

            var query = ConnectionClass.connect.RegistrationEvent
                .Where(re => re.Event.MarathonId == marathon.MarathonId);

            if (StatusComboBox.SelectedValue != null)
            {
                byte selectedStatus = (byte)StatusComboBox.SelectedValue;
                query = query.Where(re => re.Registration.RegistrationStatusId == selectedStatus);
            }

            if (DistanceComboBox.SelectedValue != null)
            {
                string selectedDistance = DistanceComboBox.SelectedValue.ToString();
                query = query.Where(re => re.Event.EventTypeId == selectedDistance);
            }

            // Сначала загружаем все нужные данные в память
            var runnersRaw = query
                .GroupBy(re => re.RegistrationId)
                .Select(g => new
                {
                    Registration = g.FirstOrDefault().Registration,
                    Events = g.Select(ev => ev.Event.EventType.EventTypeName).Distinct()
                })
                .ToList(); // <-- теперь мы в памяти

            // Затем формируем EventTypes строкой
            var runners = runnersRaw
                .Select(r => new
                {
                    Registration = r.Registration,
                    EventTypes = string.Join(", ", r.Events)
                })
                .ToList();

            // Теперь обрабатываем данные в памяти, где можно использовать ?. для безопасного доступа
            var runnerDetails = runners
                .Where(r => r.Registration != null)  // Фильтруем, если регистрация не null
                .Select(r => new
                {
                    Runner = r.Registration.Runner,
                    FirstName = r.Registration.Runner.User?.FirstName,
                    LastName = r.Registration.Runner.User?.LastName,
                    Email = r.Registration.Runner.User?.Email,
                    Gender = r.Registration.Runner.Gender,
                    Country = r.Registration.Runner.CountryCode,
                    DateOfBirth = r.Registration.Runner.DateOfBirth,
                    RegistrationStatus = r.Registration.RegistrationStatus?.RegistrationStatus1,
                    Kit = r.Registration.RaceKitOption?.RaceKitOption1,
                    EventTypes = r.EventTypes
                })
                .ToList();

            if (runnerDetails.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта.");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV файлы (*.csv)|*.csv",
                Title = "Сохранить CSV",
                FileName = "RunnersExport.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                {
                    writer.WriteLine("Имя,Фамилия,Email,Пол,Страна,Дата рождения,Возраст,Статус,Комплект,Типы марафона");

                    foreach (var runner in runnerDetails)
                    {
                        // Проверка, если дата рождения не null
                        if (runner.DateOfBirth.HasValue)
                        {
                            int age = DateTime.Today.Year - runner.DateOfBirth.Value.Year;
                            if (runner.DateOfBirth.Value > DateTime.Today.AddYears(-age)) age--;

                            writer.WriteLine($"\"{runner.FirstName}\",\"{runner.LastName}\",\"{runner.Email}\",\"{runner.Gender}\",\"{runner.Country}\",\"{runner.DateOfBirth.Value:yyyy-MM-dd}\",\"{age}\",\"{runner.RegistrationStatus}\",\"{runner.Kit}\",\"{runner.EventTypes}\"");
                        }
                    }
                }

                MessageBox.Show("Экспорт завершен успешно.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EmailListButton_Click(object sender, RoutedEventArgs e)
        {
            var marathon = ConnectionClass.connect.Marathon.FirstOrDefault(m => m.YearHeld == 2015);
            if (marathon == null)
            {
                MessageBox.Show("Марафон 2015 года не найден.");
                return;
            }

            var query = ConnectionClass.connect.RegistrationEvent
                .Where(re => re.Event.MarathonId == marathon.MarathonId);

            if (StatusComboBox.SelectedValue != null)
            {
                byte selectedStatus = (byte)StatusComboBox.SelectedValue;
                query = query.Where(re => re.Registration.RegistrationStatusId == selectedStatus);
            }

            if (DistanceComboBox.SelectedValue != null)
            {
                string selectedDistance = DistanceComboBox.SelectedValue.ToString();
                query = query.Where(re => re.Event.EventTypeId == selectedDistance);
            }

            var runners = query
                .Select(re => new
                {
                    re.Registration.Runner.User.FirstName,
                    re.Registration.Runner.User.LastName,
                    re.Registration.Runner.User.Email
                })
                .Distinct();

            // Сортировка
            if (SortComboBox.SelectedItem != null)
            {
                string selectedSort = SortComboBox.SelectedItem.ToString();
                if (selectedSort == "Имя")
                    runners = runners.OrderBy(r => r.FirstName);
                else if (selectedSort == "Фамилия")
                    runners = runners.OrderBy(r => r.LastName);
                else if (selectedSort == "Email")
                    runners = runners.OrderBy(r => r.Email);
            }
            else
            {
                runners = runners.OrderBy(r => r.LastName);
            }

            var runnersList = runners.ToList();

            if (runnersList.Count == 0)
            {
                MessageBox.Show("Нет бегунов, соответствующих фильтрам.");
                return;
            }

            // Форматируем список
            StringBuilder emailListBuilder = new StringBuilder();
            foreach (var runner in runnersList)
            {
                emailListBuilder.Append($"\"{runner.LastName} {runner.FirstName}\" <{runner.Email}>;\n");
            }

            string emailList = emailListBuilder.ToString();

            // Показываем во всплывающем окне с возможностью копирования
            ShowEmailPopup(emailList);
        }
        private void ShowEmailPopup(string emailList)
        {
            System.Windows.Window popup = new System.Windows.Window()
            {
                Title = "Список e-mail адресов",
                Width = 600,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = Brushes.White
            };

            TextBox textBox = new TextBox
            {
                Text = emailList,
                FontSize = 14,
                FontFamily = new FontFamily("Consolas"),
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                AcceptsReturn = true,
                AcceptsTab = true,
                IsReadOnly = true
            };

            popup.Content = textBox;
            popup.ShowDialog();
        }

        private void Back_btn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void LogOut_btn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MenuPage());
        }
    }

}

