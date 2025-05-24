using MarathonSkillsApp.DB_model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Data.Entity;

namespace MarathonSkillsApp.Pages
{
    /// <summary>
    /// Логика взаимодействия для VolunteerManagementPage.xaml
    /// </summary>
    public partial class VolunteerManagementPage : Page, INotifyPropertyChanged
    {
        private readonly DateTime _raceStartTime = new DateTime(2025, 11, 24, 6, 0, 0);
        private DispatcherTimer _timer;
        private mrthnskillsEntities _context = new mrthnskillsEntities();
        private ObservableCollection<Volunteer> _volunteers = new ObservableCollection<Volunteer>();
        private string _sortProperty = "LastName";

        public ObservableCollection<Volunteer> Volunteers => _volunteers;

        public string VolunteerCountText => $"Всего волонтеров: {Volunteers.Count}";

        public VolunteerManagementPage()
        {
            InitializeComponent();
            _volunteers.CollectionChanged += Volunteers_CollectionChanged;
            this.DataContext = this;
            this.Loaded += Page_Loaded;
            this.Unloaded += Page_Unloaded;
        }

        private void Volunteers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(VolunteerCountText));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
            UpdateCountdown();
            LoadVolunteers();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }

            if (_context != null)
            {
                _context.Dispose();
                _context = null;
            }
        }

        private async void LoadVolunteers()
        {
            try
            {
                var list = await _context.Volunteer
                    .Include(v => v.Country)
                    .Include(v => v.Gender1)
                    .ToListAsync();

                _volunteers.Clear();
                foreach (var v in list)
                    _volunteers.Add(v);

                // Привязка источника данных к таблице
                VolunteersDataGrid.ItemsSource = Volunteers;

                // Сразу применим сортировку по умолчанию
                SortVolunteers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
            }
        }

        private void SortVolunteers()
        {
            if (Volunteers == null) return;

            var sorted = Volunteers.ToList();

            if (_sortProperty == "FirstName")
                sorted = sorted.OrderBy(v => v.FirstName).ToList();
            else if (_sortProperty == "Country")
                sorted = sorted.OrderBy(v => v.Country != null ? v.Country.CountryName : "").ToList();
            else if (_sortProperty == "Gender")
                sorted = sorted.OrderBy(v => v.Gender1 != null ? v.Gender1.Gender1 : "").ToList();
            else // По умолчанию сортировка по фамилии
                sorted = sorted.OrderBy(v => v.LastName).ToList();

            _volunteers.Clear();
            foreach (var v in sorted)
                _volunteers.Add(v);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateCountdown();
        }
        public void ReloadVolunteers()
        {
            try
            {
                using (var context = new mrthnskillsEntities())
                {
                    var volunteers = context.Volunteer
                        .Include(v => v.Country)
                        .Include(v => v.Gender1)
                        .ToList();

                    _volunteers.Clear();
                    foreach (var v in volunteers)
                        _volunteers.Add(v);

                    OnPropertyChanged("VolunteerCountText");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении данных: " + ex.Message);
            }
        }
        private void UpdateCountdown()
        {
            TimeSpan timeLeft = _raceStartTime - DateTime.Now;
            if (timeLeft.TotalSeconds > 0)
            {
                CountdownTextBlock.Text = string.Format("{0} дней {1} часов и {2} минут до старта марафона!",
                    timeLeft.Days, timeLeft.Hours, timeLeft.Minutes);
            }
            else
            {
                CountdownTextBlock.Text = "Марафон уже начался!";
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var list = await _context.Volunteer
                    .Include(v => v.Country)
                    .Include(v => v.Gender1)
                    .ToListAsync();

                _volunteers.Clear();
                foreach (var v in list)
                    _volunteers.Add(v);

                SortVolunteers(); // Применяем сортировку после загрузки

                MessageBox.Show("Данные обновлены!", "Обновление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadVolunteersButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new VolunteerImportPage());// загрузка волонтёров при повторном нажатии
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdministratorMenuPage());
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                _sortProperty = selectedItem.Tag?.ToString();
                SortVolunteers();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private void LogOut_btn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MenuPage());
        }
    }
}

