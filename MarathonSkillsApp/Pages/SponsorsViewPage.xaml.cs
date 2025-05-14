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
    /// Логика взаимодействия для SponsorsViewPage.xaml
    /// </summary>
    public partial class SponsorsViewPage : Page
    {
        private List<CharityGroup> charityGroups;
        private MarathonCountdown countdown;
        private DateTime marathonDate = new DateTime(2025, 10, 20);

        public SponsorsViewPage()
        {
            InitializeComponent();
            countdown = new MarathonCountdown(UpdateCountdownText, marathonDate);
            LoadSponsorsFromDB();
            SortComboBox.Items.Add("По сумме");
            SortComboBox.Items.Add("По названию");
            SortComboBox.SelectedIndex = 0;
            RefreshSponsorList();
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
        private void LoadSponsorsFromDB()
        {
            charityGroups = ConnectionClass.connect.Charity
                .Select(charity => new CharityGroup
                {
                    CharityId = charity.CharityId,
                    CharityName = charity.CharityName,
                    CharityLogo = charity.CharityLogo,
                    TotalAmount = ConnectionClass.connect.Registration
                        .Where(r => r.CharityId == charity.CharityId)
                        .Join(ConnectionClass.connect.Sponsorship,
                            reg => reg.RegistrationId,
                            s => s.RegistrationId,
                            (reg, s) => s.Amount)
                        .DefaultIfEmpty(0)
                        .Sum()
                })
                .Where(g => g.TotalAmount > 0)
                .ToList();
        }

        private void RefreshSponsorList()
        {
            SponsorsListPanel.Children.Clear();

            IEnumerable<CharityGroup> sortedList = charityGroups;

            // Сортировка
            if (SortComboBox.SelectedItem?.ToString() == "По сумме")
                sortedList = charityGroups.OrderByDescending(g => g.TotalAmount);
            else if (SortComboBox.SelectedItem?.ToString() == "По названию")
                sortedList = charityGroups.OrderBy(g => g.CharityName);

            // Подсчёт общей информации
            CharityCountTextBlock.Text = $"Благотворительные организации: {sortedList.Count()}";
            TotalDonationTextBlock.Text = $"Всего спонсорских взносов: ${sortedList.Sum(g => g.TotalAmount):N0}";

            foreach (var group in sortedList)
            {
                SponsorsListPanel.Children.Add(CreateSponsorElement(group));
            }
        }

        private Grid CreateSponsorElement(CharityGroup group)
        {
            Grid grid = new Grid
            {
                Height = 60,
                Background = (SponsorsListPanel.Children.Count % 2 == 0)
                    ? new SolidColorBrush(Color.FromRgb(234, 234, 234))
                    : Brushes.White,
                Margin = new Thickness(0, 0, 0, 1)
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });

            // Логотип
            Image logo = new Image
            {
                Width = 40,
                Height = 40,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            try
            {
                logo.Source = new BitmapImage(new Uri($"/Images/{group.CharityLogo}", UriKind.Relative));
            }
            catch
            {
                logo.Source = new BitmapImage(new Uri("/Images/default.png", UriKind.Relative));
            }

            Grid.SetColumn(logo, 0);
            grid.Children.Add(logo);

            // Название
            TextBlock name = new TextBlock
            {
                Text = group.CharityName,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };
            Grid.SetColumn(name, 1);
            grid.Children.Add(name);

            // Сумма
            TextBlock sum = new TextBlock
            {
                Text = "$" + group.TotalAmount.ToString("N0"),
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetColumn(sum, 2);
            grid.Children.Add(sum);

            return grid;
        }

        public class CharityGroup
        {
            public int CharityId { get; set; }
            public string CharityName { get; set; }
            public string CharityLogo { get; set; }
            public decimal TotalAmount { get; set; }
        }

        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshSponsorList();
        }

        private void Back_btn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }

}
    
