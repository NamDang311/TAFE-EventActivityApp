using System.Windows;
using System.Windows.Controls;

namespace myapp.Views
{
    /// <summary>
    /// Interaction logic for AddPage.xaml
    /// </summary>
    public partial class AddPage : Page
    {
        public AddPage()
        {
            InitializeComponent();
        }

        public void OnSaveFitnessClicked(object sender, RoutedEventArgs e)
        {
            // Handle the save fitness logic here
           string date = DateTextBox.Text;
            string title = TitleTextBox.Text;
            decimal cost = decimal.Parse(CostTextBox.Text);
            string location = LocationTextBox.Text;
            // Create a new EventDetail object
            try
            {
                var eventDetail = new EventActivityApp.Domain.EventDetail(date, title, cost, location);
                // You can now save the eventDetail to your data store or perform other actions
                MessageBox.Show($"Event saved: {eventDetail}", "Event Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            }
    }
}
