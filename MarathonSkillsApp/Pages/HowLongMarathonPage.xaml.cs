using MarathonSkillsApp.Classes;
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
    /// Логика взаимодействия для HowLongMarathonPage.xaml
    /// </summary>
    public partial class HowLongMarathonPage : Page
    {
        private MarathonCountdown countdown;
        private DateTime marathonDate = new DateTime(2025, 10, 20);
        private List<HowLongItem> howLongItems = new List<HowLongItem>
        {
            new HowLongItem { Name = "Sloth", ImageFile = "sloth.jpg", Type = "speed", Value = 0.12 },
            new HowLongItem { Name = "F1 Car", ImageFile = "f1-car.jpg", Type = "speed", Value = 345 },
            new HowLongItem { Name = "Horse", ImageFile = "horse.png", Type = "speed", Value = 15 },
            new HowLongItem { Name = "Slug", ImageFile = "slug.jpg", Type = "speed", Value = 0.01 },
            new HowLongItem { Name = "Capybara", ImageFile = "capybara.jpg", Type = "speed", Value = 35 },
            new HowLongItem { Name = "Jaguar", ImageFile = "jaguar.jpg", Type = "speed", Value = 80 },
            new HowLongItem { Name = "Worm", ImageFile = "worm.jpg", Type = "speed", Value = 0.03 },
            new HowLongItem { Name = "Bus", ImageFile = "bus.jpg", Type = "length", Value = 10 },
            new HowLongItem { Name = "Pair of Havaianas", ImageFile = "pair-of-havaianas.jpg", Type = "length", Value = 0.245 },
            new HowLongItem { Name = "Airbus A380", ImageFile = "airbus-a380.jpg", Type = "length", Value = 73 },
            new HowLongItem { Name = "Football Field", ImageFile = "football-field.jpg", Type = "length", Value = 105 },
            new HowLongItem { Name = "Ronaldinho", ImageFile = "ronaldinho.jpg", Type = "length", Value = 1.81 },
        };

        private bool isSpeedSelected = true;

        public HowLongMarathonPage()
        {
            InitializeComponent();
            DisplaySpeedItems(); // по умолчанию
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

        private void DisplaySpeedItems()
        {
            isSpeedSelected = true;
            RightPanel.Children.Clear();

            var speedItems = howLongItems.Where(i => i.Type == "speed").ToList();
            foreach (var item in speedItems)
            {
                var btn = CreateItemButton(item.Name, item.ImageFile, () => ShowSpeedDetails(item));
                RightPanel.Children.Add(btn);
            }
        }

        private void DisplayDistanceItems()
        {
            isSpeedSelected = false;
            RightPanel.Children.Clear();

            var distanceItems = howLongItems.Where(i => i.Type == "length").ToList();
            foreach (var item in distanceItems)
            {
                var btn = CreateItemButton(item.Name, item.ImageFile, () => ShowDistanceDetails(item));
                RightPanel.Children.Add(btn);
            }
        }

        private Border CreateItemButton(string name, string imageFile, Action onClick)
        {
            var image = new Image
            {
                Source = LoadImage(imageFile),
                Width = 100,
                Height = 100,
                Margin = new Thickness(5)
            };
            image.MouseLeftButtonUp += (s, e) => onClick();

            // Используем StackPanel с горизонтальной ориентацией
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };

            // Добавляем изображение слева
            panel.Children.Add(image);

            // Добавляем текст справа от изображения
            panel.Children.Add(new TextBlock
            {
                Text = name,
                Foreground = System.Windows.Media.Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0) // Отступ слева для текста
            });

            return new Border
            {
                BorderBrush = System.Windows.Media.Brushes.Gray,
                BorderThickness = new Thickness(1),
                Child = panel,
                Width = 250,
                Height= 80,
            };
        }


        private void ShowSpeedDetails(HowLongItem item)
        {
            ItemNameTextBlock.Text = item.Name;
            ItemImage.Source = LoadImage(item.ImageFile);

            if (item.Value > 0)
            {
                double hours = 42.0 / item.Value;
                TimeSpan ts = TimeSpan.FromHours(hours);
                string timeText = ts.Hours > 0 ? $"{ts.Hours} ч {ts.Minutes} мин" : $"{ts.Minutes} мин";
                ItemDescriptionTextBlock.Text = $"Максимальная скорость {item.Name} — {item.Value} км/ч. Это займет {timeText}, чтобы завершить 42 км марафон.";
            }
            else
            {
                ItemDescriptionTextBlock.Text = $"Недопустимая скорость для {item.Name}.";
            }
        }

        private void ShowDistanceDetails(HowLongItem item)
        {
            ItemNameTextBlock.Text = item.Name;
            ItemImage.Source = LoadImage(item.ImageFile);

            if (item.Value > 0)
            {
                double count = 42000.0 / item.Value;
                int rounded = (int)Math.Ceiling(count);
                ItemDescriptionTextBlock.Text = $"Длина {item.Name} — {item.Value} м. Это займет примерно {rounded} из них, чтобы покрыть дистанцию в 42 км марафона.";
            }
            else
            {
                ItemDescriptionTextBlock.Text = $"Недопустимая длина для {item.Name}.";
            }
        }

        private BitmapImage LoadImage(string fileName)
        {
            try
            {
                string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resourses", fileName);
                return new BitmapImage(new Uri(imagePath, UriKind.Absolute));
            }
            catch
            {
                return null;
            }
        }

        private void SpeedButton_Click(object sender, RoutedEventArgs e)
        {
            DisplaySpeedItems();
        }

        private void DistanceButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayDistanceItems();
        }

        private void Back_btn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }

    public class HowLongItem
    {
        public string Name { get; set; }
        public string ImageFile { get; set; }
        public string Type { get; set; } // "speed" или "length"
        public double Value { get; set; } // скорость (км/ч) или длина (м)
    }
}

