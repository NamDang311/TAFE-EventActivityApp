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
        public EventCsvRepository _repo;
        public AddPage()
        {
            InitializeComponent();
            _repo = new EventCsvRepository();
        }

        /// <summary>
        /// Validates input, saves the fitness event, and displays the result.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void OnSaveFitnessClicked(object sender, RoutedEventArgs e)
        {
            // Run a validation on the form inputs
            var values = ValidateTextInput("Fitness");
            if (values is null)
            {
                return;
            }

            //Create a new FitnessEvent object
            try
            {
                var eventDetail = new EventActivityApp.Domain.FitnessEvent($"{values[0]} {values[1]}", (string)values[2], (decimal)values[3], (string)values[4]);
                // Write to csv
                _repo.CurrentType = EventType.fitness;
                _repo.WriteCsv(eventDetail);
                MessageBox.Show($"Event saved:\n" +
                    $"{eventDetail}", "Event Saved", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Validates input, saves the entertainment event, and displays the result.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
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
                var eventDetail = new EventActivityApp.Domain.EntertainmentEvent($"{values[0]} {values[1]}", (string)values[2], (decimal)values[3], (int)values[4]);
                // Write to csv
                _repo.CurrentType = EventType.entertainment;
                _repo.WriteCsv(eventDetail);
                MessageBox.Show($"Event saved:\n" +
                    $"{eventDetail}", "Event Saved", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Validates the text input in the form and builds the event field values.
        /// </summary>
        /// <param name="type">The type of event being validated ("Fitness" or "Entertainment").</param>
        /// <returns>The validated field values, or <see langword="null"/> when validation fails.</returns>
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
            string date = dateValue.ToString("dd/MM/yyyy");

            // Validate time picker if empty or not in correct format
            if (!TimeSpan.TryParseExact(TimeTextBox.Text.Trim(), @"hh\:mm", CultureInfo.InvariantCulture, out TimeSpan parsed))
            {
                MessageBox.Show("Please enter a time in HH:mm format (e.g. 14:30).", "Time Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TimeTextBox.Focus();
                return null;
            }

            string time = parsed.ToString(@"hh\:mm");

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
                if (!int.TryParse(MinParticipantTextBox.Text, out minParticipant))
                {
                    MessageBox.Show("Please enter a valid number.", "Min Participant Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    MinParticipantTextBox.Focus();
                    return null;
                }

                return [date, time, title, cost, minParticipant];
            }
        }

    }
}
