using MarathonSkillsApp.Classes;
using MarathonSkillsApp.DB_model;
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
using System.Windows.Threading;

namespace MarathonSkillsApp.Pages
{
    /// <summary>
    /// Логика взаимодействия для InventoryManagementPage.xaml
    /// </summary>
    public partial class InventoryManagementPage : Page
    {
        private DispatcherTimer timer;
        private List<InventoryItem> inventoryItems;
        private MarathonCountdown countdown;


        public class InventoryItem
        {
            public string ItemName { get; set; }
            public string TypeA { get; set; }
            public string TypeB { get; set; }
            public string TypeC { get; set; }
            public int Required { get; set; }
            public int Stock { get; set; }

            public int ToOrder => Required > Stock ? Required - Stock : 0;
        }

        public InventoryManagementPage()
        {
            InitializeComponent();
            this.Loaded += InventoryManagementPage_Loaded;
            countdown = new MarathonCountdown(UpdateCountdownText);

        }

        private void InventoryManagementPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
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

        public void LoadData()
        {
            using (var db = new mrthnskillsEntities())
            {
                // Получаем число уникальных бегунов, зарегистрированных на марафон
                var totalRunners = db.Registration
                    .Where(r => r.RegistrationStatusId == 1) // Можно уточнить статус, если нужно
                    .Select(r => r.RunnerId)
                    .Distinct()
                    .Count();

                txtTotalRunners.Text = $"Всего зарегистрировано бегунов на марафон: {totalRunners}";

                // Группируем регистраций по RaceKitOptionId
                var kitCounts = db.Registration
                    .GroupBy(r => r.RaceKitOptionId)
                    .ToDictionary(g => g.Key.Trim(), g => g.Count());

                // Загружаем все комплекты и их содержимое
                var kitContents = db.KitContents.ToList();
                var kitTypes = db.KitTypes.ToList();
                var inventories = db.Inventory.ToList();

                var inventoryItems = new List<InventoryItem>();

                foreach (var inv in inventories)
                {
                    var item = new InventoryItem
                    {
                        ItemName = inv.ItemName,
                        Stock = inv.StockQuantity ?? 0,
                    };

                    // Кол-во для типа A
                    var aKits = kitTypes.Where(k => k.TypeName == "A").Select(k => k.KitTypeID).ToList();
                    item.TypeA = aKits.Sum(kitId =>
                    {
                        var qty = kitContents.FirstOrDefault(kc => kc.KitTypeID == kitId && kc.InventoryID == inv.InventoryID)?.Quantity ?? 0;
                        var raceKitOptionId = kitTypes.FirstOrDefault(k => k.KitTypeID == kitId)?.RaceKitOptionId.Trim();
                        int count = 0;
                        if (raceKitOptionId != null && kitCounts.TryGetValue(raceKitOptionId, out var foundCount))
                        {
                            count = foundCount;
                        }
                        return qty * count;
                    }).ToString();

                    // Кол-во для типа B
                    var bKits = kitTypes.Where(k => k.TypeName == "B").Select(k => k.KitTypeID).ToList();
                    item.TypeB = bKits.Sum(kitId =>
                    {
                        var qty = kitContents.FirstOrDefault(kc => kc.KitTypeID == kitId && kc.InventoryID == inv.InventoryID)?.Quantity ?? 0;
                        var raceKitOptionId = kitTypes.FirstOrDefault(k => k.KitTypeID == kitId)?.RaceKitOptionId.Trim();
                        int count = 0;
                        if (raceKitOptionId != null && kitCounts.TryGetValue(raceKitOptionId, out var foundCount))
                        {
                            count = foundCount;
                        }
                        return qty * count;
                    }).ToString();

                    // Кол-во для типа C
                    var cKits = kitTypes.Where(k => k.TypeName == "C").Select(k => k.KitTypeID).ToList();
                    item.TypeC = cKits.Sum(kitId =>
                    {
                        var qty = kitContents.FirstOrDefault(kc => kc.KitTypeID == kitId && kc.InventoryID == inv.InventoryID)?.Quantity ?? 0;
                        var raceKitOptionId = kitTypes.FirstOrDefault(k => k.KitTypeID == kitId)?.RaceKitOptionId.Trim();
                        int count = 0;
                        if (raceKitOptionId != null && kitCounts.TryGetValue(raceKitOptionId, out var foundCount))
                        {
                            count = foundCount;
                        }
                        return qty * count;
                    }).ToString();

                    // Подсчет общего количества (Required)
                    item.Required = int.Parse(item.TypeA) + int.Parse(item.TypeB) + int.Parse(item.TypeC);

                    inventoryItems.Add(item);
                }

                dgInventory.ItemsSource = inventoryItems;
            }
        }      

        private void StartTimer()
        {
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var marathonStart = new DateTime(2025, 11, 24, 6, 0, 0);
            var timeRemaining = marathonStart - DateTime.Now;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdministratorMenuPage());
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MenuPage());
        }

        private void btnReport_Click(object sender, RoutedEventArgs e)
        {
            if (dgInventory.ItemsSource is List<InventoryItem> inventory)
            {
                // Привязываем данные к скрытому ItemsControl
                reportItemsControl.ItemsSource = inventory;

                // Делаем панель видимой для печати
                printPanel.Visibility = Visibility.Visible;

                PrintDialog printDialog = new PrintDialog();

                if (printDialog.ShowDialog() == true)
                {
                    // Измеряем и отображаем перед печатью
                    printPanel.Measure(new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight));
                    printPanel.Arrange(new Rect(new Point(0, 0), printPanel.DesiredSize));
                    printDialog.PrintVisual(printPanel, "Отчет по инвентарю");
                }

                // Скрываем снова
                printPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Нет данных для отчета", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnReceipt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new InventoryReceiptPage());
        }
    }
}