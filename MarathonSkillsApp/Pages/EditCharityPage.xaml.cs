using MarathonSkillsApp.Classes;
using MarathonSkillsApp.DB_model;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MarathonSkillsApp.Pages
{
    public partial class EditCharityPage : Page
    {
        private DB_model.Charity _currentCharity;
        private MarathonCountdown countdown;
        private DateTime marathonDate = new DateTime(2025, 10, 20);

        public EditCharityPage(Charity charity)
        {
            InitializeComponent();
            DataContext = this;

            // Преобразуем в DB_model.Charity
            _currentCharity = ConvertToDbModel(charity);

            countdown = new MarathonCountdown(UpdateCountdownText, marathonDate);
            SetCharityData(_currentCharity);
        }
        private void UpdateCountdownText(string text)
        {
            CountdownText.Text = text;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            countdown.Stop();
        }
        private DB_model.Charity ConvertToDbModel(Charity viewModel)
        {
            return new DB_model.Charity
            {
                CharityId = viewModel.Id,
                CharityName = viewModel.Name,
                CharityDescription = viewModel.Description,
                CharityLogo = viewModel.LogoBytes // byte[]
            };
        }
        public void SetCharityData(DB_model.Charity charity)
        {
            if (charity == null || charity.CharityId == 0)
            {
                MessageBox.Show("Ошибка: организация не найдена.");
                return;
            }

            NameTextBox.Text = charity.CharityName;
            DescriptionTextBox.Text = charity.CharityDescription;

            UpdateLogoImage();
        }

        private void UpdateLogoImage()
        {
            if (_currentCharity?.CharityLogo != null && _currentCharity.CharityLogo.Length > 0)
            {
                try
                {
                    using (var memoryStream = new MemoryStream(_currentCharity.CharityLogo))
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = memoryStream;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();

                        LogoImage.Fill = new ImageBrush(bitmapImage)
                        {
                            Stretch = Stretch.UniformToFill
                        };
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при отображении изображения: {ex.Message}");
                    LogoImage.Fill = Brushes.LightGray;
                }
            }
            else
            {
                LogoImage.Fill = Brushes.Gray; // Нет фото
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    // Читаем файл как byte[]
                    byte[] logoBytes = File.ReadAllBytes(filePath);

                    // Сохраняем в объект
                    _currentCharity.CharityLogo = logoBytes;

                    // Обновляем отображение
                    UpdateLogoImage();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при чтении файла: {ex.Message}");
                }
            }
        }

        private void SaveCharityData(DB_model.Charity charity)
        {
            using (var dbContext = new mrthnskillsEntities())
            {
                var charityInDb = dbContext.Charity.Find(charity.CharityId);
                if (charityInDb == null) return;

                charityInDb.CharityName = charity.CharityName;
                charityInDb.CharityDescription = charity.CharityDescription;

                // Сохраняем напрямую byte[] в поле CharityLogo (тип varbinary(max))
                charityInDb.CharityLogo = charity.CharityLogo;

                dbContext.SaveChanges();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _currentCharity.CharityName = NameTextBox.Text;
            _currentCharity.CharityDescription = DescriptionTextBox.Text;

            SaveCharityData(_currentCharity);

            NavigationService.Navigate(new CharityManagementPage());
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CharityManagementPage());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CharityManagementPage());
        }
    }
}