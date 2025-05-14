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
    /// Логика взаимодействия для ProverkaRunnersPage.xaml
    /// </summary>
    public partial class ProverkaRunnersPage : Page
    {
        private MarathonCountdown countdown;
        private DateTime marathonDate = new DateTime(2025, 10, 20);

        public ProverkaRunnersPage()
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

        private void Btn_back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void Participated_earlier_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new LoginPage());
        }

        private void New_runner_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegistrationPage());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
