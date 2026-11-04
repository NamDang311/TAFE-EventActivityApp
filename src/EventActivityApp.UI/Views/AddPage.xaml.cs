using EventActivityApp.Persistent;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;



namespace EventActivityApp.UI.Views
{
    /// <summary>
    /// Interaction logic for AddPage.xaml
    /// </summary>
    public partial class AddPage : Page
    {

        /// <summary>
        /// Provides access to the event CSV repository.
        /// </summary>
        public EventCsvRepository _repo = new EventCsvRepository();

        /// <summary>
        /// Initializes a new instance of the AddPage class.
        /// </summary>
        public AddPage()
        {
            InitializeComponent();
        }

        private void OnSaveFitnessClicked(object sender, RoutedEventArgs e)
        {
            // Run a validation on the form inputs
            var values = ValidateTextInput("Fitness");
            if (values is null)
            {
                return;
            }

            //Create a new FitnessActivity object
            try
            {
                var eventDetail = new EventActivityApp.Domain.FitnessActivity($"{values[0]} {values[1]}", (string)values[2], (decimal)values[3], (string)values[4]);
                // Write to csv
                _repo.WriteCsv(eventDetail, EventType.fitness);
                MessageBox.Show($"Event saved:\n" +
                    $"{eventDetail}", "Event Saved", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnSaveEntertainmentClicked(object sender, RoutedEventArgs e)
        {
            // Run a validation on the form inputs
            var values = ValidateTextInput("Entertainment");
            if (values is null)
            {
                return;
            }
            // Create a new EventDetail object
            try
            {
                var eventDetail = new EventActivityApp.Domain.EntertainmentActivity($"{values[0]} {values[1]}", (string)values[2], (decimal)values[3], (int)values[4]);
                // Write to csv
                _repo.WriteCsv(eventDetail, EventType.entertainment);
                MessageBox.Show($"Event saved:\n" +
                    $"{eventDetail}", "Event Saved", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<object>? ValidateTextInput(string type)
        {
            // Validate date picker if empty
            if (DateTextBox.SelectedDate == null)
            {
                MessageBox.Show("Please select a date.", "Date required", MessageBoxButton.OK, MessageBoxImage.Error);
                DateTextBox.Focus();
                return null;
            }
            DateTime dateValue = DateTextBox.SelectedDate.Value;

            // Confirm before saving an event dated in the past
            if (dateValue.Date < DateTime.Today)
            {
                var confirm = MessageBox.Show(
                    $"The selected date {dateValue:d/MM/yyyy} is in the past. Do you want to continue?",
                    "Are you sure?", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (confirm != MessageBoxResult.OK)
                {
                    DateTextBox.Focus();
                    return null;
                }
            }
            string date = dateValue.ToString("d/MM/yyyy");

            // Validate time picker if empty or not in correct format
            if (!TimeSpan.TryParseExact(TimeTextBox.Text.Trim(), @"h\:mm", CultureInfo.InvariantCulture, out TimeSpan parsed))
            {
                MessageBox.Show("Please enter a time in h:mm format (e.g. 7:30).", "Time Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TimeTextBox.Focus();
                return null;
            }

            string time = parsed.ToString(@"h\:mm");

            // Validate title if empty
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Please enter a title.", "Title Required", MessageBoxButton.OK, MessageBoxImage.Error);
                TitleTextBox.Focus();
                return null;
            }

            string title = TitleTextBox.Text;

            // Validate cost if empty or incorrect format
            decimal cost;
            if (!decimal.TryParse(CostTextBox.Text, out cost))
            {
                MessageBox.Show("Please enter a valid number.", "Cost Error", MessageBoxButton.OK, MessageBoxImage.Error);
                CostTextBox.Focus();
                return null;
            }


            if (type == "Fitness")
            {
                // Validate location if empty
                if (string.IsNullOrWhiteSpace(FitnessLocationTextBox.Text))
                {
                    MessageBox.Show("Please enter a location.", "Location Required", MessageBoxButton.OK, MessageBoxImage.Error);
                    FitnessLocationTextBox.Focus();
                    return null;
                }

                string location = FitnessLocationTextBox.Text;

                return [date, time, title, cost, location];
            }
            else
            {
                // Validate location if empty
                int minParticipant;
                if (!int.TryParse(MinParticipantTextBox.Text, out minParticipant) || minParticipant < 2)
                {
                    MessageBox.Show("Please enter a valid number. Min participants is 2.", "Min Participant Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    MinParticipantTextBox.Focus();
                    return null;
                }

                return [date, time, title, cost, minParticipant];
            }
        }

    }
}
