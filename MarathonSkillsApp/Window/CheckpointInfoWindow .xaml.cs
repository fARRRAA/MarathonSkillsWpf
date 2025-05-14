using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MarathonSkillsApp.Window
{
    public partial class CheckpointInfoWindow
    {
        public CheckpointInfoWindow(string checkpointNumber, string[] data)
        {
            InitializeComponent();

            TitleTextBlock.Text = $"Checkpoint {checkpointNumber}";
            LandmarkTextBlock.Text = data[0];

            // Скрываем все сервисы по умолчанию
            ServicesTextBlock.Visibility = Visibility.Collapsed;
            DrinksGrid.Visibility = Visibility.Collapsed;
            EnergyBarsGrid.Visibility = Visibility.Collapsed;
            ToiletsGrid.Visibility = Visibility.Collapsed;
            InformationGrid.Visibility = Visibility.Collapsed;
            MedicalGrid.Visibility = Visibility.Collapsed;

            // Проверяем каждый сервис и показываем только доступные
            bool hasAnyServices = false;

            if (data[1] == "Yes")
            {
                DrinksGrid.Visibility = Visibility.Visible;
                DrinksTextBlock.Text = "Да";
                DrinksTextBlock.Foreground = Brushes.Green;
                hasAnyServices = true;
            }

            if (data[2] == "Yes")
            {
                EnergyBarsGrid.Visibility = Visibility.Visible;
                EnergyBarsTextBlock.Text = "Да";
                EnergyBarsTextBlock.Foreground = Brushes.Green;
                hasAnyServices = true;
            }

            if (data[3] == "Yes")
            {
                ToiletsGrid.Visibility = Visibility.Visible;
                ToiletsTextBlock.Text = "Да";
                ToiletsTextBlock.Foreground = Brushes.Green;
                hasAnyServices = true;
            }

            if (data[4] == "Yes")
            {
                InformationGrid.Visibility = Visibility.Visible;
                InformationTextBlock.Text = "Да";
                InformationTextBlock.Foreground = Brushes.Green;
                hasAnyServices = true;
            }

            if (data[5] == "Yes")
            {
                MedicalGrid.Visibility = Visibility.Visible;
                MedicalTextBlock.Text = "Да";
                MedicalTextBlock.Foreground = Brushes.Green;
                hasAnyServices = true;
            }

            // Показываем заголовок "Услуги" только если есть хотя бы одна услуга
            if (hasAnyServices)
            {
                ServicesTextBlock.Visibility = Visibility.Visible;
            }

            // Динамически обновляем высоту окна
            UpdateWindowHeight();
        }

        private void UpdateWindowHeight()
        {
            // Рассчитываем высоту окна на основе видимых элементов
            double height = 180; // Базовая высота (заголовок, ориентир, кнопка закрытия)

            if (ServicesTextBlock.Visibility == Visibility.Visible)
                height += 25;

            if (DrinksGrid.Visibility == Visibility.Visible)
                height += 30;

            if (EnergyBarsGrid.Visibility == Visibility.Visible)
                height += 30;

            if (ToiletsGrid.Visibility == Visibility.Visible)
                height += 30;

            if (InformationGrid.Visibility == Visibility.Visible)
                height += 30;

            if (MedicalGrid.Visibility == Visibility.Visible)
                height += 30;

            this.Height = height + 40; // Добавляем отступы
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}