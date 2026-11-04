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
        /// <summary>
        /// Provides access to the event CSV repository.
        /// </summary>
        public EventCsvRepository _repo = new EventCsvRepository();

        /// <summary>
        /// Initializes a new instance of the SearchPage class.
        /// </summary>
        public SearchPage()
        {
            InitializeComponent();
        }

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
                var allEvents = _repo.ReadAllActivities();

                //if (DateFilterRadioBefore.IsChecked == true)
                //{
                //    results.AddRange(_repo.SearchEvent(searchDate, SearchFilter.before, allEvents));
                //}
                //else if (DateFilterRadioOn.IsChecked == true)
                //{
                //    results.AddRange(_repo.SearchEvent(searchDate, SearchFilter.on, allEvents));
                //}
                //else if (DateFilterRadioAfter.IsChecked == true
                //)
                //{
                //    results.AddRange(_repo.SearchEvent(searchDate, SearchFilter.after, allEvents));
                //}


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



        private void OnDisplayAllClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                List<EventDetail> results = _repo.ReadAllActivities();
                ActivityTable.ItemsSource = results;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
