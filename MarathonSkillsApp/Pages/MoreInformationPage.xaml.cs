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
    /// Логика взаимодействия для MoreInformationPage.xaml
    /// </summary>
    public partial class MoreInformationPage : Page
    {
        private MarathonCountdown countdown;
        private DateTime marathonDate = new DateTime(2025, 10, 20);

        public MoreInformationPage()
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

        private void ChariryList_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CharityListPage());
        }

        private void Back_btn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void HowLong_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new HowLongMarathonPage());
        }

        private void InfoPage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MarathonInfoPage());
        }

        private void BMI_Calc_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new BMICalculatorPage());
        }

        private void BMR_Calc_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new BMRCalculatorPage());
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PreviousResultsPage());

        }
    }
}
