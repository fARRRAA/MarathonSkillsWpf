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

namespace MarathonSkillsApp.Pages
{
    /// <summary>
    /// Логика взаимодействия для BMRCalculatorPage.xaml
    /// </summary>
    public partial class BMRCalculatorPage : Page
    {
        private string selectedGender = "Male"; // По умолчанию
        private MarathonCountdown countdown;
        private DateTime marathonDate = new DateTime(2025, 10, 20);


        public BMRCalculatorPage()
        {
            InitializeComponent();
            countdown = new MarathonCountdown(UpdateCountdownText, marathonDate);

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

        private void FemaleBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            selectedGender = "Female";

            FemaleBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#241d70"));
            FemaleBorder.BorderThickness = new Thickness(4);
            FemaleBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8DC"));
            FemaleText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));

            MaleBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B6B6B6"));
            MaleBorder.BorderThickness = new Thickness(2);
            MaleBorder.Background = Brushes.White;
            MaleText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B6B6B6"));
        }

        private void MaleBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            selectedGender = "Male";

            MaleBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#241d70"));
            MaleBorder.BorderThickness = new Thickness(4);
            MaleBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8DC"));
            MaleText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));

            FemaleBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B6B6B6"));
            FemaleBorder.BorderThickness = new Thickness(2);
            FemaleBorder.Background = Brushes.White;
            FemaleText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B6B6B6"));
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double height = double.Parse(HeightTextBox.Text);
                double weight = double.Parse(WeightTextBox.Text);
                int age = int.Parse(AgeTextBox.Text);

                double bmr = 0;

                if (selectedGender == "Male")
                {
                    bmr = 66 + (13.7 * weight) + (5 * height) - (6.8 * age);
                }
                else if (selectedGender == "Female")
                {
                    bmr = 655 + (9.6 * weight) + (1.8 * height) - (4.7 * age);
                }

                BmrTextBlock.Text = Math.Round(bmr).ToString("N0");

                CaloriesSedentaryTextBlock.Text = (bmr * 1.2).ToString("N0");
                CaloriesLightTextBlock.Text = (bmr * 1.375).ToString("N0");
                CaloriesModerateTextBlock.Text = (bmr * 1.55).ToString("N0");
                CaloriesHighTextBlock.Text = (bmr * 1.725).ToString("N0");
                CaloriesMaxTextBlock.Text = (bmr * 1.9).ToString("N0");
            }
            catch
            {
                MessageBox.Show("Пожалуйста, введите корректные числовые значения роста, веса и возраста.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            string message =
        "Сидячий образ жизни: минимум движения.\n" +
        "Маленькая активность: легкие упражнения 1–3 дня в неделю.\n" +
        "Средняя активность: умеренные упражнения 3–5 дней в неделю.\n" +
        "Сильная активность: интенсивные тренировки 6–7 дней в неделю.\n" +
        "Максимальная активность: физическая работа или 2 тренировки в день.";

            MessageBox.Show(message, "Уровни активности", MessageBoxButton.OK, MessageBoxImage.Information);
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
