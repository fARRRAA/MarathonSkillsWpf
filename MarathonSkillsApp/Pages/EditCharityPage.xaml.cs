using MarathonSkillsApp.Classes;
using MarathonSkillsApp.DB_model;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
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
    /// Логика взаимодействия для EditCharityPage.xaml
    /// </summary>
    public partial class EditCharityPage : Page
    {
        private Charity _currentCharity;
        private MarathonCountdown countdown;
        private DateTime marathonDate = new DateTime(2025, 10, 20);

        public EditCharityPage(Charity charity)
        {
            InitializeComponent();
            countdown = new MarathonCountdown(UpdateCountdownText, marathonDate);
            SetCharityData(charity);

        }
        private void UpdateCountdownText(string text)
        {
            // Обновляем текст в TextBlock
            CountdownText.Text = text;
        }

        // При закрытии окна или перехода со страницы остановите таймер
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            countdown.Stop();
        }
        public void SetCharityData(Charity charity)
        {
            _currentCharity = charity;
            if (_currentCharity == null || _currentCharity.Id == 0)
            {
                MessageBox.Show("Ошибка: организация не найдена.");
                return;
            }

            // Заполняем поля на форме данными организации
            NameTextBox.Text = _currentCharity.Name;
            DescriptionTextBox.Text = _currentCharity.Description;
            LogoPathTextBox.Text = _currentCharity.LogoPath;

            if (!string.IsNullOrEmpty(_currentCharity.LogoPath))
            {
                try
                {
                    string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", _currentCharity.LogoPath);
                    if (File.Exists(imagePath))
                    {
                        LogoImage.Fill = new ImageBrush(new BitmapImage(new Uri(imagePath, UriKind.Absolute)));
                    }
                    else
                    {
                        LogoImage.Fill = Brushes.White;
                    }
                }
                catch
                {
                    LogoImage.Fill = Brushes.White;
                }
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
                // Устанавливаем путь к выбранному файлу
                string filePath = openFileDialog.FileName;

                // Копируем файл в папку с изображениями
                string fileName = System.IO.Path.GetFileName(filePath);
                string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", fileName);
                System.IO.File.Copy(filePath, imagePath, true);

                // Обновляем путь к файлу в текстовом поле
                LogoPathTextBox.Text = fileName;

                // Отображаем изображение логотипа
                LogoImage.Fill = new ImageBrush(new BitmapImage(new Uri(imagePath, UriKind.Absolute)));
            }
        }

        private void SaveCharityData(Charity charity)
        {
            using (var dbContext = new mrthnskillsEntities())
            {
                var charityInDb = dbContext.Charity.FirstOrDefault(c => c.CharityId == charity.Id);
                if (charityInDb != null)
                {
                    charityInDb.CharityName = charity.Name;
                    charityInDb.CharityDescription = charity.Description;

                    // Сохраняем только имя файла, а не полный путь
                    charityInDb.CharityLogo = System.IO.Path.GetFileName(charity.LogoPath);

                    dbContext.SaveChanges();
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CharityManagementPage());
        }



        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Обновляем объект с изменениями
            _currentCharity.Name = NameTextBox.Text;
            _currentCharity.Description = DescriptionTextBox.Text;
            _currentCharity.LogoPath = LogoPathTextBox.Text;

            // Сохраняем данные
            SaveCharityData(_currentCharity);

            // Переход к странице с организациями (она сама обновит данные в OnNavigatedTo)
            NavigationService.Navigate(new CharityManagementPage());
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CharityManagementPage());
        }
    }
}
