using EventActivityApp.Persistent;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using MySqlConnector;
using EventActivityApp.Domain;



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
            if (values is null) return;

            EventDetail? newEventDetail = null;
            try
            {
                // Check if there is an event on the selected date
                if (_repo.CheckEventExist((DateTime)values[0]))
                {
                    newEventDetail = new EventActivityApp.Domain.FitnessActivity((DateTime)values[0], (string)values[1], (decimal)values[2], (string)values[3]);
                    ShowMsgBoxForReplacing(newEventDetail);
                }
                else
                {
                    // Write to db
                    newEventDetail = new EventActivityApp.Domain.FitnessActivity((DateTime)values[0], (string)values[1], (decimal)values[2], (string)values[3]);
                    _repo.WriteEvent(newEventDetail!);
                    MessageBox.Show($"Event saved:\n" +
                        $"{newEventDetail}", "Event Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            // catch (ArgumentException ex)
            // {
            //     MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            // }


            // Catch everything else
            catch
            {
                MessageBox.Show("Something went wrong", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnSaveEntertainmentClicked(object sender, RoutedEventArgs e)
        {
            // Run a validation on the form inputs
            var values = ValidateTextInput("Entertainment");
            if (values is null) return;
            // Create a new EventDetail object
            EventDetail? newEventDetail = null;
            try
            {
                // Check if there is an event on the selected date
                if (_repo.CheckEventExist((DateTime)values[0]))
                {
                    newEventDetail = new EventActivityApp.Domain.EntertainmentActivity((DateTime)values[0], (string)values[1], (decimal)values[2], (int)values[3]);
                    ShowMsgBoxForReplacing(newEventDetail);
                }
                else
                {
                    // Write to db
                    newEventDetail = new EventActivityApp.Domain.EntertainmentActivity((DateTime)values[0], (string)values[1], (decimal)values[2], (int)values[3]);
                    _repo.WriteEvent(newEventDetail);
                    MessageBox.Show($"Event saved:\n" +
                        $"{newEventDetail}", "Event Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            // catch (ArgumentException ex)
            // {
            //     MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            // }



            // Catch everything else
            catch
            {
                MessageBox.Show("Something went wrong", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowMsgBoxForReplacing(EventDetail record)
        {
            MessageBoxResult mb = MessageBox.Show($"There is already an existing event on the selected date. Do you want to replace the event?",
                    "Unable to Save Event", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (mb == MessageBoxResult.Yes)
            {
                _repo.UpdateEvent(record);
                MessageBox.Show($"Event saved:\n" + $"{record}", "Event Saved", MessageBoxButton.OK, MessageBoxImage.Information);
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
            DateTime date = dateValue;

            // Validate time picker if empty or not in correct format
            if (!TimeSpan.TryParseExact(TimeTextBox.Text.Trim(), @"h\:mm", CultureInfo.InvariantCulture, out TimeSpan parsed))
            {
                MessageBox.Show("Please enter a time in h:mm format (e.g. 7:30).", "Time Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TimeTextBox.Focus();
                return null;
            }

            TimeSpan time = parsed;

            // Validate title if empty
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Please enter a title.", "Title Required", MessageBoxButton.OK, MessageBoxImage.Error);
                TitleTextBox.Focus();
                return null;
            }

            if (TitleTextBox.Text.Length < 3)
            {
                MessageBox.Show("Title needs to have at least 3 characters.", "Title Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TitleTextBox.Focus();
                return null;
            }

            string title = TitleTextBox.Text;

            // Validate cost if empty or incorrect format
            decimal cost;
            if (!decimal.TryParse(CostTextBox.Text, out cost) || cost < 0)
            {
                MessageBox.Show("Cost must be 0 or more.", "Cost Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                return [date + time, title, cost, location];
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

                return [date + time, title, cost, minParticipant];
            }
        }

    }
}
