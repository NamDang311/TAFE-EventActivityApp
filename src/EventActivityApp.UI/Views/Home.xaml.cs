using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EventActivityApp.UI.Views
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page
    {

        public Home()
        {
            InitializeComponent();
        }

        private void OnAddActivityClicked(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Uri("Views/AddPage.xaml", UriKind.Relative));
        }


        private void OnSearchActivitiesClicked(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Uri("Views/SearchPage.xaml", UriKind.Relative));
        }
    }
}
