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
    /// Логика взаимодействия для MarathonInfoPage.xaml
    /// </summary>
    public partial class MarathonInfoPage : Page
    {
        private MarathonCountdown countdown;
        private DateTime marathonDate = new DateTime(2025, 10, 20);

        public MarathonInfoPage()
        {
            InitializeComponent();
            countdown = new MarathonCountdown(UpdateCountdownText, marathonDate);
            LoadInfoText();
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

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new InteractiveMapPage());
        }

        private void LoadInfoText()
        {
            try
            {
                string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resourses", "marathon-skills-2016-marathon-info.txt");
                var encoding = System.Text.Encoding.GetEncoding("windows-1251");
                string text = System.IO.File.ReadAllText(filePath, encoding);
                InfoTextBlock.Text = text;
            }
            catch (Exception ex)
            {
                InfoTextBlock.Text = "Не удалось загрузить информацию: " + ex.Message;
            }
        }

        private void Back_btn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
