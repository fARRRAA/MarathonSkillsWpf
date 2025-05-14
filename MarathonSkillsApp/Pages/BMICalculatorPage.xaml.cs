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
    /// Логика взаимодействия для BMICalculatorPage.xaml
    /// </summary>
    public partial class BMICalculatorPage : Page
    {
        private string selectedGender = "Male"; // По умолчанию
        private MarathonCountdown countdown;
        private DateTime marathonDate = new DateTime(2025, 10, 20);

        public BMICalculatorPage()
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

        private void Back_btn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void MaleBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            selectedGender = "Male";

            // Активный стиль
            MaleBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FDC100"));
            MaleBorder.BorderThickness = new Thickness(4);
            MaleBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8DC"));
            MaleText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));

            // Неактивный стиль для женского
            FemaleBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B6B6B6"));
            FemaleBorder.BorderThickness = new Thickness(2);
            FemaleBorder.Background = Brushes.White;
            FemaleText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B6B6B6"));
        }

        private void FemaleBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            selectedGender = "Female";

            // Активный стиль
            FemaleBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FDC100"));
            FemaleBorder.BorderThickness = new Thickness(4);
            FemaleBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8DC"));
            FemaleText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));

            // Неактивный стиль для мужского
            MaleBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B6B6B6"));
            MaleBorder.BorderThickness = new Thickness(2);
            MaleBorder.Background = Brushes.White;
            MaleText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B6B6B6"));
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(WeightTextBox.Text, out double weight) || !double.TryParse(HeightTextBox.Text, out double heightCm))
            {
                MessageBox.Show("Введите корректные значения роста и веса.");
                return;
            }

            double heightM = heightCm / 100.0;
            double bmi = weight / (heightM * heightM);
            BMIScoreText.Text = bmi.ToString("F1");

            UpdateBMIVisuals(bmi);
        }

        
           private void UpdateBMIVisuals(double bmi)
        {
            string category;
            string imagePath;

            if (bmi < 18.5)
            {
                category = "Недостаточный";
                imagePath = "/Images/bmi-underweight-icon.png";
            }
            else if (bmi < 25)
            {
                category = "Здоровый";
                imagePath = "/Images/bmi-healthy-icon.png";
            }
            else if (bmi < 30)
            {
                category = "Избыточный";
                imagePath = "/Images/bmi-overweight-icon.png";
            }
            else
            {
                category = "Ожирение";
                imagePath = "/Images/bmi-obese-icon.png";
            }

            BMICategoryText.Text = category;
            BMICategoryIcon.Source = new BitmapImage(new Uri(imagePath, UriKind.Relative));

            double arrowX = GetArrowX(bmi);
            Canvas.SetLeft(ArrowPolygon, arrowX-5); // Центрируем стрелку
        }

       
        /// <summary>
        /// Обновляет положение стрелки на шкале
        /// </summary>
        private double GetArrowX(double bmi)
        {
            if (bmi < 18.5)
            {
                // Пропорция внутри сегмента "Недостаточный"
                return (bmi / 18.5) * 70;
            }
            else if (bmi < 25)
            {
                // Пропорция внутри "Здоровый"
                return 70 + ((bmi - 18.5) / (25 - 18.5)) * 90;
            }
            else if (bmi < 30)
            {
                // Пропорция внутри "Избыточный"
                return 160 + ((bmi - 25) / (30 - 25)) * 90;
            }
            else
            {
                // Пропорция внутри "Ожирение"
                return 250 + ((Math.Min(bmi, 40) - 30) / 10) * 100;
            }
        }

        private void Cancel_btn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
