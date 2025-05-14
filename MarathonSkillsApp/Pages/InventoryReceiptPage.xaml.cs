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
    /// Логика взаимодействия для InventoryReceiptPage.xaml
    /// </summary>
    public partial class InventoryReceiptPage : Page
    {
        private MarathonCountdown countdown;

        private DispatcherTimer timer;
        private List<InventoryReceiptItem> inventoryItems;

        public class InventoryReceiptItem
        {
            public int InventoryId { get; set; }
            public string ItemName { get; set; }
            public int? Quantity { get; set; }
            public int CurrentStock { get; set; }
        }

        public InventoryReceiptPage()
        {
            InitializeComponent();
            LoadInventoryItems();
            countdown = new MarathonCountdown(UpdateCountdownText);

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

        private void LoadInventoryItems()
        {
            using (var db = new mrthnskillsEntities())
            {
                inventoryItems = db.Inventory
                    .Select(i => new InventoryReceiptItem
                    {
                        InventoryId = i.InventoryID,
                        ItemName = i.ItemName,
                        Quantity = null,
                        CurrentStock = (i.StockQuantity ?? 0)
                    })
                    .ToList();

                dgInventoryReceipt.ItemsSource = inventoryItems;
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
            NavigationService?.GoBack();
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MenuPage());
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new mrthnskillsEntities())
                {
                    var updatedItems = inventoryItems.Where(i => i.Quantity.HasValue).ToList();

                    if (!updatedItems.Any())
                    {
                        MessageBox.Show("Нет изменений для сохранения.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    foreach (var item in updatedItems)
                    {
                        var inventory = db.Inventory.Find(item.InventoryId);
                        if (inventory != null)
                        {
                            int newStock = (inventory.StockQuantity ?? 0) + item.Quantity.Value;
                            if (newStock < 0)
                            {
                                MessageBox.Show($"Невозможно списать {Math.Abs(item.Quantity.Value)} единиц товара '{item.ItemName}'. На складе только {inventory.StockQuantity} единиц.",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            inventory.StockQuantity = newStock;
                        }
                    }

                    db.SaveChanges();
                    MessageBox.Show("Изменения успешно сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadInventoryItems(); // Перезагружаем данные
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при сохранении данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
