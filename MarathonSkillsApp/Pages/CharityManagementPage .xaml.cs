using MarathonSkillsApp.Classes;
using MarathonSkillsApp.DB_model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Threading;

namespace MarathonSkillsApp.Pages
{
    /// <summary>
    /// Логика взаимодействия для CharityManagementPage.xaml
    /// </summary>
    public partial class CharityManagementPage : Page
    {
        private readonly DateTime _raceStartTime = new DateTime(2025, 11, 24, 6, 0, 0);
        private DispatcherTimer _timer;
        public ObservableCollection<Charity> Charities { get; set; }

        public CharityManagementPage()
        {
            InitializeComponent();
            DataContext = this;
            Charities = new ObservableCollection<Charity>();

            LoadCharitiesFromDatabase();
            InitializeCountdownTimer();
        }

        public void LoadCharitiesFromDatabase()
        {
            try
            {
                Charities.Clear();

                var dbCharities = ConnectionClass.connect.Charity.ToList();

                foreach (var dbCharity in dbCharities)
                {
                    // Создаём объект Charity и заполняем его данными из БД
                    var charityViewModel = new Charity
                    {
                        Id = dbCharity.CharityId,
                        Name = dbCharity.CharityName,
                        Description = dbCharity.CharityDescription,

                        // Предполагается, что CharityLogo — это byte[]
                        LogoBytes = dbCharity.CharityLogo ?? GetDefaultLogoBytes()
                    };

                    Charities.Add(charityViewModel);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных из базы данных: {ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void InitializeCountdownTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
            UpdateCountdown();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateCountdown();
        }

        private void UpdateCountdown()
        {
            TimeSpan timeLeft = _raceStartTime - DateTime.Now;

            if (timeLeft.TotalMilliseconds > 0)
            {
                CountdownText.Text = $"{timeLeft.Days} дней {timeLeft.Hours} часов и {timeLeft.Minutes} минут до старта марафона!";
            }
            else
            {
                CountdownText.Text = "Марафон уже начался!";
                _timer.Stop();
            }
        }

        private void AddCharity_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddCharityPage());
        }

        private void EditCharity_Click(object sender, RoutedEventArgs e)
        {
            var charity = (sender as Button).DataContext as Charity;

            if (charity != null)
            {
                NavigationService.Navigate(new EditCharityPage(charity));
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdministratorMenuPage());
        }
        private byte[] GetDefaultLogoBytes()
        {
            // Путь к дефолтному изображению в проекте
            string defaultImagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "male-icon.png");

            if (File.Exists(defaultImagePath))
            {
                return File.ReadAllBytes(defaultImagePath);
            }

            // Альтернативный способ — загрузка из ресурса WPF
            using (var stream = Application.GetResourceStream(
                       new Uri("pack://application:,,,/Images/male-icon.png"))
                   ?.Stream)
            {
                if (stream != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        return memoryStream.ToArray();
                    }
                }
            }

            return null;
        }
    }
    
    public class Charity : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        private byte[] logoBytes;
        public byte[] LogoBytes
        {
            get => logoBytes;
            set
            {
                logoBytes = value;
                OnPropertyChanged(nameof(LogoImage)); 
            }
        }

        public BitmapImage LogoImage
        {
            get
            {
                try
                {
                    if (logoBytes == null || logoBytes.Length == 0)
                        return DefaultImage;

                    using (var memoryStream = new MemoryStream(logoBytes))
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.StreamSource = memoryStream;
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.EndInit();
                        image.Freeze(); 
                        return image;
                    }
                }
                catch
                {
                    return DefaultImage;
                }
            }
        }

        private static BitmapImage defaultImage;
        private static BitmapImage DefaultImage
        {
            get
            {
                if (defaultImage == null)
                {
                    defaultImage = new BitmapImage(
                        new Uri("pack://application:,,,/Images/male-icon.png", UriKind.Absolute));
                }
                return defaultImage;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
