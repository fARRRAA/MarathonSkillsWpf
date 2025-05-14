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
                    // Обёртка для организации
                    var dockPanel = new DockPanel { Margin = new Thickness(0, 10, 0, 10) };

                    // Эллипс с изображением (если есть логотип)
                    var ellipse = new Ellipse
                    {
                        Width = 50,
                        Height = 50,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1
                    };

                    if (!string.IsNullOrEmpty(charity.CharityLogo))
                    {
                        try
                        {
                            var logoPath = System.IO.Path.Combine("Images", charity.CharityLogo); // путь к логотипу
                            if (File.Exists(logoPath))
                            {
                                var imageBrush = new ImageBrush
                                {
                                    ImageSource = new BitmapImage(new Uri(logoPath, UriKind.RelativeOrAbsolute)),
                                    Stretch = Stretch.UniformToFill
                                };
                                ellipse.Fill = imageBrush;
                            }
                            else
                            {
                                ellipse.Fill = Brushes.Black;
                            }
                        }
                        catch
                        {
                            ellipse.Fill = Brushes.Black;
                        }
                    }
                    else
                    {
                        ellipse.Fill = Brushes.Black;
                    }

                    dockPanel.Children.Add(ellipse);

                    // Текстовое описание
                    var stack = new StackPanel { Margin = new Thickness(10, 0, 0, 0) };
                    DockPanel.SetDock(stack, Dock.Right);

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
                        Foreground = Brushes.Black
                    };

                    stack.Children.Add(nameText);
                    stack.Children.Add(descText);
                    dockPanel.Children.Add(stack);

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
