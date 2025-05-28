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
using System.Windows.Shapes;

namespace MarathonSkillsApp.Window
{
    /// <summary>
    /// Логика взаимодействия для RunnerPopupWindow.xaml
    /// </summary>
    public partial class RunnerPopupWindow
    {
        public RunnerPopupWindow(int runnerId)
        {
            InitializeComponent();
            LoadRunnerInfo(runnerId);
        }

        private void LoadRunnerInfo(int runnerId)
        {
            var runner = ConnectionClass.connect.Runner.FirstOrDefault(r => r.RunnerId == runnerId);
            if (runner == null) return;

            var user = runner.User;

            RunnerNameText.Text = $"{user.FirstName} {user.LastName}";
            RunnerCountryText.Text = $"Страна: {runner.CountryCode}";

            // Находим дату ближайшего прошедшего марафона
            var lastMarathon = ConnectionClass.connect.Event
                .Where(e => e.Marathon.YearHeld <= DateTime.Now.Year)
                .OrderByDescending(e => e.StartDateTime)
                .FirstOrDefault();

            if (runner.DateOfBirth.HasValue && lastMarathon?.StartDateTime.HasValue == true)
            {
                var dob = runner.DateOfBirth.Value;
                var marathonDate = lastMarathon.StartDateTime.Value;

                int age = marathonDate.Year - dob.Year;
                if (dob > marathonDate.AddYears(-age)) age--;

                RunnerAgeText.Text = $"Возраст: {age} лет";
            }

            if (runner.Photo.Length > 0)
            {
                try
                {
                    using (var memoryStream = new MemoryStream(runner.Photo))
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = memoryStream;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();

                        RunnerPhoto.Source = bitmapImage;

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки фото: " + ex.Message);
                }
            }

            // Получаем результаты
            var results = ConnectionClass.connect.RegistrationEvent
    .Where(re => re.Registration.RunnerId == runnerId && re.RaceTime != null)
    .ToList() // Переносим в память, после этого можно использовать обычный C#
    .Select(re => new
    {
        RaceTime = re.RaceTime.Value,
        EventType = re.Event.EventType.EventTypeName,
        MarathonName = $"{re.Event.Marathon.MarathonName}",
        EventDate = re.Event.StartDateTime
    })
    .OrderBy(r => r.RaceTime)
    .ToList();

            // Формируем список для отображения
            int place = 1;
            var displayResults = results.Select(r => new
            {
                Place = place++,
                Time = $"{(int)TimeSpan.FromSeconds(r.RaceTime).TotalHours}h {TimeSpan.FromSeconds(r.RaceTime).Minutes}m {TimeSpan.FromSeconds(r.RaceTime).Seconds}s",
                r.EventType,
                r.MarathonName
            }).ToList();

            ResultsGrid.ItemsSource = displayResults;
        }
    }
}
