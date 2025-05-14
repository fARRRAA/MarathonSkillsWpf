using MarathonSkillsApp.Window;
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
    /// Логика взаимодействия для InteractiveMapPage.xaml
    /// </summary>
    public partial class InteractiveMapPage : Page
    {
        private Dictionary<string, string[]> checkpointData = new Dictionary<string, string[]>
        {
            { "1", new string[] { "Avenida Rudge", "Yes", "Yes", "No", "No", "No" } },
            { "2", new string[] { "Theatro Municipal", "Yes", "Yes", "Yes", "Yes", "Yes" } },
            { "3", new string[] { "Parque do Ibirapuera", "Yes", "Yes", "Yes", "No", "No" } },
            { "4", new string[] { "Jardim Luzitania", "Yes", "Yes", "Yes", "No", "Yes" } },
            { "5", new string[] { "Iguatemi", "Yes", "Yes", "Yes", "Yes", "No" } },
            { "6", new string[] { "Rua Lisboa", "Yes", "Yes", "Yes", "No", "No" } },
            { "7", new string[] { "Cemitério da Consolação", "Yes", "Yes", "Yes", "Yes", "Yes" } },
            { "8", new string[] { "Cemitério da Consolação", "Yes", "Yes", "Yes", "Yes", "Yes" } }
        };
        public InteractiveMapPage()
        {
            InitializeComponent();
        }

        private void Checkpoint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Ellipse clickedCheckpoint = (Ellipse)sender;
            string checkpointNumber = clickedCheckpoint.Tag.ToString();

            // Получение данных для выбранного чекпоинта
            if (checkpointData.ContainsKey(checkpointNumber))
            {
                string[] data = checkpointData[checkpointNumber];
                CheckpointInfoWindow infoWindow = new CheckpointInfoWindow(checkpointNumber, data);
                infoWindow.Show();
            }
            else
            {
                MessageBox.Show("Информация о чекпоинте отсутствует");
            }
        }

        private void Back_btn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
