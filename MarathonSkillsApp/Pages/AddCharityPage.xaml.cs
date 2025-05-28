using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MarathonSkillsApp.DB_model;

namespace MarathonSkillsApp.Pages
{
    public partial class AddCharityPage : Page
    {
        private DB_model.Charity _currentCharity;
        private bool _isEditMode;
        private byte[] _logoBytes; // Хранит фото как байты

        public AddCharityPage() : this(new DB_model.Charity())
        {
            InitializeComponent();
            _isEditMode = false;
        }

        public AddCharityPage(DB_model.Charity charity)
        {
            InitializeComponent();
            _currentCharity = charity;
            _isEditMode = (_currentCharity.CharityId != 0);
        }

        public void SetCharityData(DB_model.Charity charity)
        {
            _currentCharity = charity;
            _isEditMode = true;

            NameTextBox.Text = _currentCharity.CharityName;
            DescriptionTextBox.Text = _currentCharity.CharityDescription;

            if (_currentCharity.CharityLogo != null && _currentCharity.CharityLogo.Length > 0)
            {
                _logoBytes = _currentCharity.CharityLogo;
                ShowImage(_logoBytes);
            }
            else
            {
                LogoImage.Fill = Brushes.Gray;
            }
        }

        private void ShowImage(byte[] imageBytes)
        {
            try
            {
                using (var ms = new MemoryStream(imageBytes))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    LogoImage.Fill = new ImageBrush(bitmap);
                }
            }
            catch
            {
                LogoImage.Fill = Brushes.White;
            }
        }

        private void SaveCharityData()
        {
            using (var dbContext = new mrthnskillsEntities())
            {
                _currentCharity.CharityName = NameTextBox.Text.Trim();
                _currentCharity.CharityDescription = DescriptionTextBox.Text.Trim();
                _currentCharity.CharityLogo = _logoBytes; // Сохраняем байты в БД

                if (_currentCharity.CharityId == 0)
                {
                    dbContext.Charity.Add(_currentCharity);
                }
                else
                {
                    var existing = dbContext.Charity.Find(_currentCharity.CharityId);
                    if (existing != null)
                    {
                        existing.CharityName = _currentCharity.CharityName;
                        existing.CharityDescription = _currentCharity.CharityDescription;
                        existing.CharityLogo = _currentCharity.CharityLogo;
                    }
                }

                dbContext.SaveChanges();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
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
            NavigationService.GoBack();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string filePath = openFileDialog.FileName;
                    _logoBytes = File.ReadAllBytes(filePath); // Читаем как байты
                    ShowImage(_logoBytes);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удалось загрузить изображение: " + ex.Message);
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}