using MarathonSkillsApp.Classes;
using MarathonSkillsApp.DB_model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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
using System.IO;

namespace MarathonSkillsApp.Pages
{
    /// <summary>
    /// Логика взаимодействия для EditRunnerProfilePage.xaml
    /// </summary>
    public partial class EditRunnerProfilePage : Page
    {
        private MarathonCountdown countdown;
        private DateTime marathonDate = new DateTime(2025, 10, 20);
        private string currentUserEmail;

        public EditRunnerProfilePage(string email)
        {
            InitializeComponent();
            countdown = new MarathonCountdown(UpdateCountdownText, marathonDate);
            currentUserEmail = email;
            this.Loaded += EditProfilePage_Loaded;

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

        private void BrowsePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

            if (dlg.ShowDialog() == true)
            {
                PhotoPathTextBox.Text = dlg.FileName;
                RunnerImage.Source = new BitmapImage(new Uri(dlg.FileName));
            }
        }


        private void EditProfilePage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadGenders();
            LoadCountries();
            LoadRunnerData(); // из БД по Email
        }

        private void LoadGenders()
        {
            using (var context = new mrthnskillsEntities())
            {
                var genders = context.Gender.ToList();
                GenderComboBox.ItemsSource = genders;
                GenderComboBox.DisplayMemberPath = "Gender1"; // т.к. поле одно
                GenderComboBox.SelectedValuePath = "Gender1";
            }
        }

        private void LoadCountries()
        {
            using (var context = new mrthnskillsEntities())
            {
                var countries = context.Country.ToList();
                CountryComboBox.ItemsSource = countries;
                CountryComboBox.DisplayMemberPath = "CountryName";
                CountryComboBox.SelectedValuePath = "CountryCode";
            }
        }

        private void LoadRunnerData()
        {
            using (var context = new mrthnskillsEntities())
            {
                var runner = context.Runner.Include(r => r.User)
                .FirstOrDefault(r => r.Email == currentUserEmail);

                if (runner != null)
                {
                    EmailTextBlock.Text = runner.Email;
                    FirstNameTextBox.Text = runner.User.FirstName;
                    LastNameTextBox.Text = runner.User.LastName;
                    GenderComboBox.SelectedValue = runner.Gender;
                    CountryComboBox.SelectedValue = runner.CountryCode;
                    BirthDatePicker.SelectedDate = runner.DateOfBirth;

                    if (runner.Photo!=null)
                    {
                        if(runner.Photo.Length > 0)
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

                                RunnerImage.Source = bitmapImage;

                                PhotoPathTextBox.Text = "profilePhoto.png";
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка загрузки фото: " + ex.Message);
                        }
                        }

                    }


                }

            }
        }
        private bool IsValidPassword(string password)
        {
            return password.Length >= 6 &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsDigit) &&
                   password.Any(ch => "!@#$%^".Contains(ch));
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                GenderComboBox.SelectedItem == null ||
                CountryComboBox.SelectedItem == null ||
                BirthDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка возраста
            var birthDate = BirthDatePicker.SelectedDate.Value;
            var age = DateTime.Now.Year - birthDate.Year;
            if (birthDate > DateTime.Now.AddYears(-age)) age--;

            if (age < 10)
            {
                MessageBox.Show("Вам должно быть не менее 10 лет.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка пароля (если указан)
            string password = PassTextBox.Password.Trim();
            string confirmPassword = RePassTextBox.Password.Trim();

            if (!string.IsNullOrWhiteSpace(password))
            {
                if (password != confirmPassword)
                {
                    MessageBox.Show("Пароль и подтверждение пароля не совпадают.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!IsValidPassword(password))
                {
                    MessageBox.Show("Пароль должен содержать минимум 6 символов, 1 заглавную букву, 1 цифру и один из символов: ! @ # $ % ^", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            // Работа с БД
            using (var context = new mrthnskillsEntities())
            {
                var runner = context.Runner.Include(r => r.User)
                                           .FirstOrDefault(r => r.Email == currentUserEmail);

                if (runner != null)
                {
                    // Обновление данных пользователя
                    runner.User.FirstName = FirstNameTextBox.Text.Trim();
                    runner.User.LastName = LastNameTextBox.Text.Trim();
                    if (!string.IsNullOrWhiteSpace(password))
                        runner.User.Password = password;

                    // Обновление данных бегуна
                    runner.Gender = GenderComboBox.SelectedValue.ToString();
                    runner.CountryCode = CountryComboBox.SelectedValue.ToString();
                    runner.DateOfBirth = birthDate;

                    string selectedPhotoPath = PhotoPathTextBox.Text;

                    if (!string.IsNullOrEmpty(selectedPhotoPath) && File.Exists(selectedPhotoPath))
                    {
                        try
                        {
                            byte[] photoBytes = File.ReadAllBytes(selectedPhotoPath);

                            runner.Photo = photoBytes;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка при чтении или сохранении фото: " + ex.Message,
                                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else
                    {
                        // Если фото не выбрано — можно оставить null или предыдущее значение
                        runner.Photo = null;
                    }
                    try
                    {
                        context.SaveChanges();
                        MessageBox.Show("Профиль успешно обновлён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

        }

        private void Cancel_btn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void LogOut_btn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MenuPage());

        }
        private void Back_btn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
