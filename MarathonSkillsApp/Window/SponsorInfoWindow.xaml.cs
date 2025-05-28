using MarathonSkillsApp.DB_model;
using MarathonSkillsApp.Pages;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MarathonSkillsApp.Window
{
    /// <summary>
    /// Логика взаимодействия для SponsorInfoWindow.xaml
    /// </summary>
    public partial class SponsorInfoWindow : System.Windows.Window
    {
        private DB_model.Charity _charity;

        public SponsorInfoWindow(DB_model.Charity charity)
        {
            InitializeComponent();
            _charity = charity;

            // Заполняем данные
            CharityNameTextBlock.Text = _charity.CharityName;
            CharityDescriptionTextBlock.Text = _charity.CharityDescription;

            // Отображаем логотип
            DisplayCharityLogo(_charity.CharityLogo);
        }

        private void DisplayCharityLogo(byte[] logoBytes)
        {
            if (logoBytes != null && logoBytes.Length > 0)
            {
                try
                {
                    using (var memoryStream = new MemoryStream(logoBytes))
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = memoryStream;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze(); // Важно для потокобезопасности

                        CharityLogoEllipse.Fill = new ImageBrush(bitmapImage)
                        {
                            Stretch = Stretch.UniformToFill
                        };
                    }
                }
                catch
                {
                    // Ошибка при загрузке изображения — можно оставить пустым или показать дефолтное
                    CharityLogoEllipse.Fill = Brushes.LightGray;
                }
            }
            else
            {
                CharityLogoEllipse.Fill = Brushes.Gray; // Нет изображения
            }
        }
    }
}