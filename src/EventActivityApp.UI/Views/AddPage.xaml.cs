using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using EventActivityApp.Domain;
using EventActivityApp.Persistent;

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
        public EventDbRepository _repo = new EventDbRepository();

        private DateTime? _confirmedPastDate;

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
            if (!ValidateTextInput(EventType.fitness))
                return;

            var values = GetInputsData(EventType.fitness);

            EventDetail newEventDetail = new EventActivityApp.Domain.FitnessActivity((DateTime)values[0], (string)values[1], (decimal)values[2], (string)values[3]);
            try
            {
                // Check if there is an event on the selected date
                if (_repo.CheckEventExist((DateTime)values[0]))
                {
                    ShowMsgBoxForReplacing(newEventDetail);
                }
                else
                {
                    // Confirm Write to db
                    _repo.WriteEvent(newEventDetail!);
                    MessageBox.Show($"Event saved:\n" +
                        $"{newEventDetail}", "Event Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }

            catch
            {
                MessageBox.Show("Something went wrong", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnSaveEntertainmentClicked(object sender, RoutedEventArgs e)
        {
            // Run a validation on the form inputs
            if (!ValidateTextInput(EventType.entertainment))
                return;

            var values = GetInputsData(EventType.entertainment);
            // Create a new EventDetail object
            EventDetail newEventDetail = new EntertainmentActivity((DateTime)values[0], (string)values[1], (decimal)values[2], (int)values[3]);
            try
            {
                // Check if there is an event on the selected date
                if (_repo.CheckEventExist((DateTime)values[0]))
                {

                    ShowMsgBoxForReplacing(newEventDetail);
                }
                else
                {
                    // Confirm write to db
                    MessageBox.Show($"Event saved:\n" +
                        $"{newEventDetail}", "Event Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateTextInput(EventType type)
        {

            if (DateTextBox.SelectedDate == null)
            { return ShowMsgBox("Please select a date.", "Date required", DateTextBox); }


            var date = DateTextBox.SelectedDate.Value;

            if (date < DateTime.Today && _confirmedPastDate != date)
            {
                var confirm = MessageBox.Show(
                    $"The selected date {date:d/MM/yyyy} is in the past. Do you want to continue?",
                    "Are you sure?", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (confirm != MessageBoxResult.OK)
                {
                    DateTextBox.Focus();
                    return false;

                }
                _confirmedPastDate = date;
            }

            if (!TimeSpan.TryParseExact(TimeTextBox.Text.Trim(), @"h\:mm", CultureInfo.InvariantCulture, out _))
                return ShowMsgBox("Please enter a time in h:mm format (e.g. 7:30).", "Time Error", TimeTextBox);

            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
                return ShowMsgBox("Please enter a title.", "Title Required", TitleTextBox);

            if (!decimal.TryParse(CostTextBox.Text, out _))
                return ShowMsgBox("Please enter a valid number.", "Cost Error", CostTextBox);

            if (type == EventType.fitness)
            {
                if (string.IsNullOrWhiteSpace(FitnessLocationTextBox.Text))
                    return ShowMsgBox("Please enter a location.", "Location Required", FitnessLocationTextBox);
            }
            else if (type == EventType.entertainment)
            {
                if (!int.TryParse(MinParticipantTextBox.Text, out int min) || min < 2)
                    return ShowMsgBox("Please enter a valid number. Min participants is 2.",
                                "Min Participant Error", MinParticipantTextBox);
            }

            return true;
        }

        private bool ShowMsgBox(string message, string caption, Control field)
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
            field.Focus();
            return false;
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

        private List<object> GetInputsData(EventType type)
        {
            DateTime dateValue = DateTextBox.SelectedDate!.Value;
            DateTime date = dateValue;

            var time = TimeSpan.ParseExact(TimeTextBox.Text.Trim(), @"h\:mm", CultureInfo.InvariantCulture);
            string title = TitleTextBox.Text;
            decimal cost = decimal.Parse(CostTextBox.Text);
            var data = new List<object> { dateValue + time, title, cost };
            if (type == EventType.fitness)
                data.Add(FitnessLocationTextBox.Text.Trim());
            else if (type == EventType.entertainment)
                data.Add(int.Parse(MinParticipantTextBox.Text));

            return data;
        }
    }
}
