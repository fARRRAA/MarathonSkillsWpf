using MarathonSkillsApp.Classes;
using MarathonSkillsApp.DB_model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для RegistrationPage.xaml
    /// </summary>
    public partial class RegistrationPage : Page
    {
        private MarathonCountdown countdown;
        private DateTime marathonDate = new DateTime(2025, 10, 20);

        private string selectedPhotoPath;
        public RegistrationPage()
        {
            InitializeComponent();
            countdown = new MarathonCountdown(UpdateCountdownText, marathonDate);

        }
        private void UpdateCountdownText(string text)
        {
            CountdownTextBlock.Text = text;
        }

        public bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        public bool IsValidPassword(string password)
        {
            return password.Length >= 6 &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsDigit) &&
                   Regex.IsMatch(password, @"[!@#\$%\^]");
        }

        // При закрытии окна или перехода со страницы остановите таймер
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            countdown.Stop();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (var context = new mrthnskillsEntities())
            {
                var genders = context.Gender
                    .Select(g => new ComboBoxItemWrapper<string> { Display = g.Gender1, Value = g.Gender1 })
                    .ToList();
                GenderComboBox.ItemsSource = genders;

                var countries = context.Country
                    .Select(c => new ComboBoxItemWrapper<string> { Display = c.CountryName, Value = c.CountryCode })
                    .ToList();
                CountryComboBox.ItemsSource = countries;
            }
        }

        private void BrowsePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Диалог выбора файла
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png";

            if (openFileDialog.ShowDialog() == true)
            {
                selectedPhotoPath = openFileDialog.FileName;
                string fileName = System.IO.Path.GetFileName(selectedPhotoPath);

                // 2. Папка назначения внутри проекта
                string folderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "runnerPhotos");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                // 3. Полный путь назначения
                string destinationPath = System.IO.Path.Combine(folderPath, fileName);

                try
                {
                    // 4. Копирование файла (если ещё не скопирован или заменить)
                    File.Copy(selectedPhotoPath, destinationPath, true);

                    // 5. Отображение фото в Image
                    RunnerPhoto.Source = new BitmapImage(new Uri(destinationPath));

                    // 6. Отображение имени файла в TextBox
                    PhotoPathTextBox.Text = fileName;

                    // 7. Сохраняй `fileName` в БД, а не полный путь
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке изображения: " + ex.Message);
                }
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;
            string firstName = FirstNameTextBox.Text.Trim();
            string lastName = LastNameTextBox.Text.Trim();
            var gender = GenderComboBox.SelectedItem as ComboBoxItemWrapper<string>;
            var country = CountryComboBox.SelectedItem as ComboBoxItemWrapper<string>;
            DateTime? birthDate = DateOfBirthPicker.SelectedDate;

            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            {
                MessageBox.Show("Введите корректный Email.");
                return;
            }
            if (!IsValidPassword(password))
            {
                MessageBox.Show("Пароль должен содержать не менее 6 символов, 1 заглавную букву, 1 цифру и 1 спецсимвол (!@#$%^).");
                return;
            }
            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают.");
                return;
            }
            if (birthDate == null || (DateTime.Now.Year - birthDate.Value.Year) < 10)
            {
                MessageBox.Show("Вам должно быть не менее 10 лет.");
                return;
            }
            if (gender == null || country == null || string.IsNullOrEmpty(selectedPhotoPath))
            {
                MessageBox.Show("Пожалуйста, заполните все поля и выберите фотографию.");
                return;
            }

            using (var context = new mrthnskillsEntities())
            {
                var userExists = context.User.Any(u => u.Email == email);
                if (userExists)
                {
                    MessageBox.Show("Пользователь с таким Email уже существует.");
                    return;
                }

                // Копируем фото в папку проекта
                string photoFileName = System.IO.Path.GetFileName(selectedPhotoPath);

                var newUser = new User
                {
                    Email = email,
                    Password = password, // Можно захэшировать
                    FirstName = firstName,
                    LastName = lastName,
                    RoleId = "R" // Роль бегуна
                };

                var newRunner = new Runner
                {
                    Email = email,
                    Gender = gender.Value,
                    DateOfBirth = birthDate.Value,
                    CountryCode = country.Value,
                    Photo = photoFileName
                };

                context.User.Add(newUser);
                context.Runner.Add(newRunner);
                context.SaveChanges();

                MessageBox.Show("Регистрация успешна!");
                NavigationService.Navigate(new LoginPage());
            }
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
