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
using System.Windows.Shapes;

namespace MarathonSkillsApp.Window
{
    /// <summary>
    /// Логика взаимодействия для SponsorInfoWindow.xaml
    /// </summary>
    public partial class SponsorInfoWindow 
    {
        private Pages.Charity selectedCharity;

        public SponsorInfoWindow(Charity charity)
        {
            InitializeComponent();

            CharityNameTextBlock.Text = charity.CharityName;
            CharityDescriptionTextBlock.Text = charity.CharityDescription;

            if (!string.IsNullOrEmpty(charity.CharityLogo))
            {
                var logoPath = System.IO.Path.Combine(Environment.CurrentDirectory, "Images", charity.CharityLogo);
                CharityLogoEllipse.Fill = new ImageBrush(new BitmapImage(new Uri(logoPath)));
            }
        }

        public SponsorInfoWindow(Pages.Charity selectedCharity)
        {
            this.selectedCharity = selectedCharity;
        }
    }
}
