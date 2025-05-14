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
using System.Data.Entity;

namespace MarathonSkillsApp.Pages
{
    /// <summary>
    /// Логика взаимодействия для MyResultsPage.xaml
    /// </summary>
    public partial class MyResultsPage : Page
    {
        private MarathonCountdown countdown;
        private DateTime marathonDate = new DateTime(2025, 10, 20);
        private string currentUserEmail;
        private Runner currentRunner;

        public MyResultsPage(string email)
        {
            InitializeComponent();
            countdown = new MarathonCountdown(UpdateCountdownText, marathonDate);
            currentUserEmail = email;
            Loaded += MyResultsPage_Loaded;
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
        private void MyResultsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRunnerDataAndResults();
        }

        private void LoadRunnerDataAndResults()
        {
            using (var context = new mrthnskillsEntities())
            {
                currentRunner = context.Runner
                    .Include(r => r.User)
                    .FirstOrDefault(r => r.Email == currentUserEmail);

                if (currentRunner == null)
                {
                    MessageBox.Show("Не удалось загрузить данные пользователя.");
                    return;
                }

                // Вывод пола и возрастной категории
                string genderText = currentRunner.Gender == "Male" ? "мужской" : "женский";
                GenderTextBlock.Text = $"Пол: {genderText}";

                int age = CalculateAge(currentRunner.DateOfBirth ?? DateTime.Now);
                AgeCategoryTextBlock.Text = $"Возрастная категория: {GetAgeCategory(age)}";

                // Загрузка результатов
                LoadResults(context, currentRunner.RunnerId, currentRunner.Gender, age);
            }
        }


        private bool isFirstLoad = true; // Добавляем флаг первой загрузки

        private void LoadResults(mrthnskillsEntities context, int runnerId, string gender, int age, bool showMessage = false)
        {
            var category = GetAgeCategory(age);
            var results = (from re in context.RegistrationEvent
                           join reg in context.Registration on re.RegistrationId equals reg.RegistrationId
                           join ev in context.Event on re.EventId equals ev.EventId
                           join et in context.EventType on ev.EventTypeId equals et.EventTypeId
                           join m in context.Marathon on ev.MarathonId equals m.MarathonId
                           where reg.RunnerId == runnerId && re.RaceTime != null
                           select new
                           {
                               Marathon = m.MarathonName,
                               Distance = et.EventTypeName,
                               Time = re.RaceTime,
                               EventId = ev.EventId
                           }).ToList();

            if (results.Count == 0 && showMessage && !isFirstLoad)
            {
                MessageBox.Show("Вы пока не участвовали в соревнованиях.");
            }

            // Создание списка для привязки
            var resultsList = new List<ResultViewModel>();

            foreach (var result in results)
            {
                TimeSpan raceTime = TimeSpan.FromSeconds((double)result.Time);
                string timeFormatted = raceTime.ToString(@"hh\:mm\:ss");

                var allTimes = context.RegistrationEvent
                    .Where(r => r.EventId == result.EventId && r.RaceTime != null)
                    .OrderBy(r => r.RaceTime)
                    .Select(r => r.RaceTime)
                    .ToList();
                int overallPlace = allTimes.IndexOf(result.Time) + 1;

                var categoryTimes = (from re in context.RegistrationEvent
                                     join reg in context.Registration on re.RegistrationId equals reg.RegistrationId
                                     join run in context.Runner on reg.RunnerId equals run.RunnerId
                                     where re.EventId == result.EventId &&
                                           re.RaceTime != null &&
                                           run.Gender == gender
                                     select new
                                     {
                                         RaceTime = re.RaceTime,
                                         BirthDate = run.DateOfBirth
                                     })
                                    .ToList()
                                    .Where(x => GetAgeCategory(CalculateAge(x.BirthDate ?? DateTime.Now)) == category)
                                    .OrderBy(x => x.RaceTime)
                                    .Select(x => x.RaceTime)
                                    .ToList();
                int categoryPlace = categoryTimes.IndexOf(result.Time) + 1;

                // Добавление в список
                resultsList.Add(new ResultViewModel
                {
                    Marathon = result.Marathon,
                    Distance = result.Distance,
                    Time = timeFormatted,
                    OverallRank = overallPlace,
                    CategoryRank = categoryPlace
                });

                isFirstLoad = false;
            }

            // Привязка данных к ItemsControl
            ResultsPanel.ItemsSource = resultsList;
        }

        private TextBlock CreateCell(string text, int column)
        {
            var tb = new TextBlock
            {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 14,
                Foreground = Brushes.Black
            };
            Grid.SetColumn(tb, column);
            return tb;
        }

        private int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            int age = today.Year - birthDate.Year;
            if (birthDate > today.AddYears(-age)) age--;
            return age;
        }

        private string GetAgeCategory(int age)
        {
            if (age < 18) return "до 18";
            if (age <= 29) return "18-29";
            if (age <= 39) return "30-39";
            if (age <= 55) return "40-55";
            if (age <= 70) return "56-70";
            return "более 70";
        }
        public class ResultViewModel
        {
            public string Marathon { get; set; }
            public string Distance { get; set; }
            public string Time { get; set; }
            public int OverallRank { get; set; }
            public int CategoryRank { get; set; }
        }

        private void PrevResults_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PreviousResultsPage());
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
