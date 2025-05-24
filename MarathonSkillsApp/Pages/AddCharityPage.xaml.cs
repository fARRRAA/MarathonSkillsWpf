using System;
using System.Collections.Generic;
using System.Data.Entity;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MarathonSkillsApp.DB_model;

namespace MarathonSkillsApp.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddCharityPage.xaml
    /// </summary>
    public partial class AddCharityPage : Page
    {
        private DB_model.Charity _currentCharity;
        private bool _isEditMode;

        public AddCharityPage()
        {
            InitializeComponent();
            _currentCharity = new DB_model.Charity();
            _isEditMode = false;
        }

        // Метод для передачи существующей организации в режим редактирования
        public void SetCharityData(DB_model.Charity charity)
        {
            _currentCharity = charity;
            _isEditMode = true;

            // Заполняем поля данными
            NameTextBox.Text = _currentCharity.CharityName;
            DescriptionTextBox.Text = _currentCharity.CharityDescription;
            LogoPathTextBox.Text = _currentCharity.CharityLogo;

            // Проверяем, есть ли логотип
            if (!string.IsNullOrEmpty(_currentCharity.CharityLogo))
            {
                try
                {
                    // Формируем путь к изображению
                    string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", _currentCharity.CharityLogo);
                    if (File.Exists(imagePath)) // Проверяем, существует ли файл
                    {
                        LogoImage.Fill = new ImageBrush(new BitmapImage(new Uri(imagePath)));
                    }
                    else
                    {
                        LogoImage.Fill = Brushes.White; // Если файл не найден, показываем серый цвет
                    }
                }
                catch
                {
                    LogoImage.Fill = Brushes.White; // В случае ошибки показываем серый цвет
                }
            }
            else
            {
                LogoImage.Fill = Brushes.Gray; // Если логотип отсутствует, показываем серый цвет
            }
        }

        private void SaveCharityData()
        {
            using (var dbContext = new mrthnskillsEntities())
            {
                _currentCharity.CharityName = NameTextBox.Text.Trim();
                _currentCharity.CharityDescription = DescriptionTextBox.Text.Trim();

                // Обработка логотипа
                if (!string.IsNullOrEmpty(LogoPathTextBox.Text))
                {
                    string logoFileName = System.IO.Path.GetFileName(LogoPathTextBox.Text); // Имя файла логотипа
                    string imagesFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

                    // Убедимся, что папка Images существует
                    if (!Directory.Exists(imagesFolder))
                    {
                        Directory.CreateDirectory(imagesFolder);
                    }

                    string destPath = System.IO.Path.Combine(imagesFolder, logoFileName); // Сохраняем логотип с именем файла, без подпапки

                    // Копируем файл, если он ещё не скопирован или путь отличается
                    if (File.Exists(LogoPathTextBox.Text) && (!File.Exists(destPath) || (new FileInfo(LogoPathTextBox.Text)).FullName != (new FileInfo(destPath)).FullName))
                    {
                        File.Copy(LogoPathTextBox.Text, destPath, true);
                    }

                    // Сохраняем только имя файла в базе данных
                    _currentCharity.CharityLogo = logoFileName;
                }

                if (_currentCharity.CharityId == 0)
                {
                    dbContext.Charity.Add(_currentCharity); // Добавляем новую организацию
                }
                else
                {
                    var charityInDb = dbContext.Charity.Find(_currentCharity.CharityId);
                    if (charityInDb != null)
                    {
                        charityInDb.CharityName = _currentCharity.CharityName;
                        charityInDb.CharityDescription = _currentCharity.CharityDescription;
                        charityInDb.CharityLogo = _currentCharity.CharityLogo;
                    }
                }

                dbContext.SaveChanges(); // Сохраняем изменения в базе данных
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Введите название организации.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                SaveCharityData();
                MessageBox.Show("Данные успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new CharityManagementPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack(); // Возвращаемся назад
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                LogoPathTextBox.Text = openFileDialog.FileName;

                // Отображаем выбранную картинку
                LogoImage.Fill = new ImageBrush(new BitmapImage(new Uri(openFileDialog.FileName)));
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack(); // Кнопка для возврата назад
        }
    }
}
