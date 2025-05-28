using MarathonSkillsApp.Classes;
using MarathonSkillsApp.DB_model;
using System;
using System.Collections.Generic;
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

namespace MarathonSkillsApp.Pages
{
    /// <summary>
    /// Логика взаимодействия для CharityListPage.xaml
    /// </summary>
    public partial class CharityListPage : Page
    {
        private MarathonCountdown countdown;
        private DateTime marathonDate = new DateTime(2025, 10, 20);

        public CharityListPage()
        {
            InitializeComponent();
            LoadCharities();
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

        private void LoadCharities()
        {
            using (var context = new mrthnskillsEntities())
            {
                var charities = context.Charity.ToList();

                foreach (var charity in charities)
                {
                    // Создаем контейнер для каждой организации
                    var dockPanel = new DockPanel { Margin = new Thickness(30, 20, 30, 10) };

                    // Эллипс для изображения
                    var ellipse = new Ellipse
                    {
                        Width = 50,
                        Height = 50,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1
                    };

                    // Пытаемся загрузить изображение из byte[]
                    if (charity.CharityLogo != null && charity.CharityLogo.Length > 0)
                    {
                        try
                        {
                            using (var memoryStream = new MemoryStream(charity.CharityLogo))
                            {
                                var bitmapImage = new BitmapImage();
                                bitmapImage.BeginInit();
                                bitmapImage.StreamSource = memoryStream;
                                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                bitmapImage.EndInit();
                                bitmapImage.Freeze(); // Для потокобезопасности

                                ellipse.Fill = new ImageBrush(bitmapImage)
                                {
                                    Stretch = Stretch.UniformToFill
                                };
                            }
                        }
                        catch
                        {
                            ellipse.Fill = Brushes.LightGray; // Ошибка при загрузке
                        }
                    }
                    else
                    {
                        ellipse.Fill = Brushes.Gray; // Нет изображения
                    }

                    dockPanel.Children.Add(ellipse);

                    // Текстовое описание
                    var stackPanel = new StackPanel { Margin = new Thickness(30, 0, 30, 0) };
                    DockPanel.SetDock(stackPanel, Dock.Right);

                    var nameText = new TextBlock
                    {
                        Text = charity.CharityName,
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.Black
                    };

                    var descText = new TextBlock
                    {
                        Text = charity.CharityDescription,
                        FontSize = 14,
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = Brushes.Black,
                        Width = 600,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };

                    stackPanel.Children.Add(nameText);
                    stackPanel.Children.Add(descText);
                    dockPanel.Children.Add(stackPanel);

                    CharityListPanel.Children.Add(dockPanel);
                }
            }
        }

        private void Back_btn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
