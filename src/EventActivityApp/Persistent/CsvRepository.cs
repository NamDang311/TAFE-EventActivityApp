using CsvHelper;
using CsvHelper.Configuration;
using EventActivityApp.Domain;
using System.Globalization;
namespace EventActivityApp.Persistent;

/// <summary>
/// Provides read and write access to fitness and entertainment events stored in CSV files.
/// Reads, appends and searches events by event type.
/// </summary>
public class EventCsvRepository : DataRepository
{
    private const string CsvDateFormat = "d/MM/yyyy H:mm";

    // public EventType CurrentType { get; set; }

    /// <summary>
    /// Gets the CSV file name for the given event type.
    /// </summary>
    /// <param name="type">The event type.</param>
    /// <returns>The CSV file name.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="type"/> is <see cref="EventType.none"/>.</exception>
    private string GetFileName(EventType type) => type switch
    {
        EventType.fitness => "FitnessActivities.CSV",
        EventType.entertainment => "EntertainmentActivities.CSV",
        _ => throw new InvalidOperationException("Set correct event type")
    };

    /// <summary>
    /// Gets or sets the folder that contains the CSV files.
    /// </summary>
    public string DataFolder { get; set; } = GetPathToProjectFolder("Data");

    /// <summary>
    /// The full path of the CSV file used by the most recent read or write.
    /// </summary>
    public string _filePath = "";

    /// <summary>
    /// Reads event details from CSV files based on the specified event type.
    /// </summary>
    /// <param name="type">The event type to filter by. Supported values are "fitness", "entertainment", or null for all types.</param>
    /// <returns>A list of event details read from the corresponding CSV files.</returns>
    public List<EventDetail> ReadCsv(EventType type)
    {
        _filePath = Path.Combine(DataFolder, GetFileName(type));
        var results = new List<EventDetail>();
        if (type is EventType.fitness)
        {
            results.AddRange(ReadFile<FitnessActivity>("FitnessActivities.CSV"));
        }
        else if (type is EventType.entertainment)
        {
            results.AddRange(ReadFile<EntertainmentActivity>("EntertainmentActivities.CSV"));
        }
        else
        {
            throw new ArgumentOutOfRangeException("Invalid event type");
        }
        return results;
    }

    /// <summary>
    /// Reads every fitness and entertainment event from their CSV files.
    /// </summary>
    /// <returns>All events, fitness first, then entertainment.</returns>
    /// <exception cref="FileNotFoundException">Thrown when either CSV file is missing.</exception>
    public List<EventDetail> ReadAllCsv()
    {
        var results = new List<EventDetail>();
        results.AddRange(ReadCsv(EventType.fitness));
        results.AddRange(ReadCsv(EventType.entertainment));
        return results;
    }

    private List<T> ReadFile<T>(string fileName) where T : EventDetail
    {
        ;
        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException($"File not found at {_filePath}");
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            PrepareHeaderForMatch = args => args.Header.ToLower(),

        };
        using (var reader = new StreamReader(_filePath))
        using (var csv = new CsvReader(reader, config))
        {
            // Records are built through constructors, so [Format] on the property is ignored
            csv.Context.TypeConverterOptionsCache.GetOptions<DateTime>().Formats = [CsvDateFormat];

            return csv.GetRecords<T>().ToList();
        }
    }

    /// <summary>
    /// Appends one event to the CSV file for the given event type, without writing a header row.
    /// </summary>
    /// <typeparam name="T">The event type being written.</typeparam>
    /// <param name="record">The event to save.</param>
    /// <param name="type">The event type, which decides the target file.</param>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="type"/> is <see cref="EventType.none"/>.</exception>
    public void WriteCsv<T>(T record, EventType type) where T : EventDetail
    {
        _filePath = Path.Combine(DataFolder, GetFileName(type));
        using (var stream = File.Open(_filePath, FileMode.Append))
        using (var writer = new StreamWriter(stream))
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // Exclude header.
                HasHeaderRecord = false,
            };
            using (var csv = new CsvWriter(writer, config))
            {
                csv.Context.TypeConverterOptionsCache.GetOptions<DateTime>().Formats = [CsvDateFormat];
                csv.Context.RegisterClassMap<FitnessActivityMap>();
                csv.Context.RegisterClassMap<EntertainmentActivityMap>();
                csv.WriteRecord(record);
                csv.NextRecord();
            }
        }
        ;

    }


    /// <summary>
    /// Finds events whose start date is before, on or after the search date.
    /// </summary>
    /// <param name="searchDate">The date to compare against.</param>
    /// <param name="filter">How to compare each event's date with <paramref name="searchDate"/>.</param>
    /// <param name="data">The events to search. When <see langword="null"/>, all events are read from the CSV files.</param>
    /// <returns>The matching events, newest first.</returns>
    /// <exception cref="FileNotFoundException">Thrown when <paramref name="data"/> is null and a CSV file is missing.</exception>
    public List<EventDetail> SearchEvent(DateTime searchDate, SearchFilter filter, List<EventDetail>? data = null)
    {
        var results = new List<EventDetail>();
        if (data == null) { data = ReadAllCsv(); }
        foreach (var item in data)
        {
            DateTime eventDate = item.DateStartTime.Date;
            bool isMatched = filter switch
            {
                SearchFilter.before => eventDate < searchDate,
                SearchFilter.on => eventDate == searchDate,
                SearchFilter.after => eventDate > searchDate,
                _ => false
            };
            if (isMatched)
            {
                results.Add(item);
            }
        }



        return results.OrderByDescending(e => e.DateStartTime).ToList();
    }


    private static string GetPathToProjectFolder(string folderName)
    {
        // Get the base directory for the project.  This defaults to where the compiled code is
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        // Build a path to a folder in the project
        string projectDir = Path.Combine(baseDir, "..", "..", "..", "..", "EventActivityApp", folderName);

        // Turn the relative path into an absolute path
        string finalDir = Path.GetFullPath(projectDir);

        return finalDir;
    }
}
