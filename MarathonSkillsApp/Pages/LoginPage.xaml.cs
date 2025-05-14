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

namespace MarathonSkillsApp.Pages
{
    /// <summary>
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        private MarathonCountdown countdown;
        private DateTime marathonDate = new DateTime(2025, 10, 20);

        public LoginPage()
        {
            InitializeComponent();
            countdown = new MarathonCountdown(UpdateCountdownText, marathonDate);
            EmailTextBox.Text = "dnix171@seeley.net";
            PasswordBoxControl.Password = "iy$4DNN@";

        }
        private void UpdateCountdownText(string text)
        {
            CountdownTextBlock.Text = text;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            countdown.Stop();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBoxControl.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, введите Email и пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new mrthnskillsEntities())
            {
                var user = context.User.FirstOrDefault(u => u.Email == email && u.Password == password);

                if (user != null)
                {
                    CurrentUser.Email = user.Email;
                    MessageBox.Show($"Добро пожаловать, {user.FirstName} {user.LastName}!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    switch (user.RoleId.Trim())
                    {
                        case "R":
                            NavigationService.Navigate(new RunnerMenuPage());
                            break;
                        case "C":
                            NavigationService.Navigate(new CoordinatorMenuPage());
                            break;
                        case "A":
                            NavigationService.Navigate(new AdministratorMenuPage());
                            break;
                        default:
                            MessageBox.Show("Такой роли нет.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                    }
                }
                else
                {
                    MessageBox.Show("Неверный Email или пароль. Повторите попытку.", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MenuPage());
        }

  

        private void Back_btn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }


    }
}
