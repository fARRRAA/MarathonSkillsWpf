using MarathonSkillsApp.DB_model;
using Microsoft.Win32;
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
using System.Windows.Threading;

namespace MarathonSkillsApp.Pages
{
    /// <summary>
    /// Логика взаимодействия для VolunteerImportPage.xaml
    /// </summary>
    public partial class VolunteerImportPage : Page
    {
        private string _selectedFilePath;
        private readonly DateTime _raceStartTime = new DateTime(2025, 10, 20);
        private DispatcherTimer _timer;
        public VolunteerImportPage()
        {
            InitializeComponent();
            this.Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
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

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "CSV файлы (*.csv)|*.csv",
                Title = "Выберите CSV файл"
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedFilePath = dialog.FileName;
                FilePathTextBox.Text = _selectedFilePath;
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedFilePath) || !File.Exists(_selectedFilePath))
            {
                MessageBox.Show("Пожалуйста, выберите корректный CSV файл.");
                return;
            }

            try
            {
                int added = 0;
                int updated = 0;

                var lines = File.ReadAllLines(_selectedFilePath, Encoding.UTF8);
                if (lines.Length < 2)
                {
                    MessageBox.Show("Файл не содержит данных.");
                    return;
                }

                using (var context = new mrthnskillsEntities())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            for (int i = 1; i < lines.Length; i++) // пропускаем заголовок
                            {
                                var parts = lines[i].Split(',');
                                if (parts.Length < 5) // проверяем на 5 элементов, так как есть 5 полей
                                {
                                    MessageBox.Show($"Некорректная строка в файле на строке {i + 1}. Пропускаем.");
                                    continue;
                                }

                                string idStr = parts[0].Trim();
                                string firstName = parts[1].Trim();
                                string lastName = parts[2].Trim();
                                string countryCode = parts[3].Trim();
                                string genderRaw = parts[4].Trim();
                                string gender = genderRaw.Length == 1 ? (genderRaw == "M" ? "Male" : "Female") : genderRaw;

                                if (!int.TryParse(idStr, out int volunteerId))
                                {
                                    MessageBox.Show($"Некорректный ID в строке {i + 1}. Пропускаем.");
                                    continue;
                                }

                                var existing = context.Volunteer.FirstOrDefault(v => v.VolunteerId == volunteerId);
                                if (existing != null)
                                {
                                    existing.FirstName = firstName;
                                    existing.LastName = lastName;
                                    existing.CountryCode = countryCode;
                                    existing.Gender = gender;
                                    updated++;
                                }
                                else
                                {
                                    context.Volunteer.Add(new Volunteer
                                    {
                                        VolunteerId = volunteerId,
                                        FirstName = firstName,
                                        LastName = lastName,
                                        CountryCode = countryCode,
                                        Gender = gender,
                                    });
                                    added++;
                                }
                            }

                            context.SaveChanges();
                            transaction.Commit();

                            MessageBox.Show($"Импорт завершён!\nДобавлено: {added}\nОбновлено: {updated}", "Успешно");

                            // После импорта всегда переходим на страницу волонтёров, чтобы был актуальный список
                            NavigationService.Navigate(new VolunteerManagementPage());
                        }
                        catch (Exception saveEx)
                        {
                            transaction.Rollback();
                            MessageBox.Show("Ошибка при сохранении в БД: " + saveEx.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обработке CSV: " + ex.Message);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new VolunteerManagementPage());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new VolunteerManagementPage());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MenuPage());
        }
    }
}

