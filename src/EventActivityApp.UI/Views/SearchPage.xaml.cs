using System.DirectoryServices;
using System.Windows;
using System.Windows.Controls;
using EventActivityApp.Domain;
using EventActivityApp.Persistent;
using System.Diagnostics;
using System.Globalization;

namespace myapp.Views
{
    /// <summary>
    /// Interaction logic for SearchPage.xaml
    /// </summary>
    public partial class SearchPage : Page
    {
        public EventCsvRepository _repo;
        public SearchPage()
        {
            InitializeComponent();
            _repo = new EventCsvRepository();
        }

        /// <summary>
        /// Loads event details from the CSV repository and displays them in the activity table.
        /// </summary>
        /// <remarks>Displays an error message when an <see cref="ArgumentException"/> occurs.</remarks>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The routed event data.</param>
        private void SearchActivitiesClicked(object sender, RoutedEventArgs e)
        {
            // Check if Search Date field is empty
            if (SearchDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please select a date.", "Date Required", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DateTime searchDate = SearchDatePicker.SelectedDate.Value;

            try
            {
                var results = new List<EventDetail>();
                foreach (var item in _repo.ReadCsv())
                {
                    DateTime eventDate = DateTime.ParseExact(item.DateStartTime, "d/MM/yyyy H:mm", CultureInfo.InvariantCulture).Date;
                    if (DateFilterRadioBefore.IsChecked == true && eventDate < searchDate)
                    {
                        results.Add(item);
                    }
                    else if (DateFilterRadioOn.IsChecked == true && eventDate == searchDate)
                    {
                        results.Add(item);
                    }
                    else if (DateFilterRadioAfter.IsChecked == true && eventDate > searchDate
                    )
                    {
                        results.Add(item);
                    }
                }

                if (results.Count == 0)
                {
                    MessageBox.Show("Please change your filter.", "No Event Found", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Debug.WriteLine(results);
                ActivityTable.ItemsSource = results;





            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool FindEvent(EventDetail evt)
        {
            if (evt.DateStartTime == "1/03/2025 14:00")
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Displays all event details read from the CSV file.
        /// </summary>
        /// <param name="sender">The source of the routed event.</param>
        /// <param name="e">The event data.</param>
        private void OnDisplayAllClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                List<EventDetail> results = _repo.ReadCsv();
                ActivityTable.ItemsSource = results;
                foreach (var item in results)
                {
                    Debug.WriteLine($"Event {item}");
                }
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
