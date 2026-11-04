using CsvHelper;
using CsvHelper.Configuration;
using EventActivityApp.Domain;
using System.Globalization;
namespace EventActivityApp.Persistent;

using EventActivityApp.Data;

public enum EventType
{
    none,
    fitness,
    entertainment,
}

public enum SearchFilter
{
    before, on, after
}


public class EventCsvRepository
{
    private const string CsvDateFormat = "d/MM/yyyy H:mm";

    public Db Db = new Db();
    
    private string GetFileName(EventType type) => type switch
    {
        EventType.fitness => "FitnessActivities.CSV",
        EventType.entertainment => "EntertainmentActivities.CSV",
        _ => throw new InvalidOperationException("Set correct event type")
    };

    public string DataFolder { get; set; } = GetPathToProjectFolder("Data");

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

    public List<EventDetail> ReadAllCsv()
    {
        using var connection = Db.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "GetAllActivities";
        command.CommandType = System.Data.CommandType.StoredProcedure;
        var results = new List<EventDetail>();

        /** Back up for old methods*/
        // results.AddRange(ReadCsv(EventType.fitness));
        // results.AddRange(ReadCsv(EventType.entertainment));
        /** ----------------- */

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            string dateStartTime = reader.GetDateTime("DateStartTime").ToString("d/MM/yyyy H:mm", CultureInfo.InvariantCulture);
            string title = reader.GetString("Title");
            decimal cost = reader.GetDecimal("Cost");

            EventDetail activity = reader.GetString("ActivityType") switch
            {
                "Fitness" => new FitnessEvent(dateStartTime, title, cost, reader.GetString("Location")),
                "Entertainment" => new EntertainmentEvent(dateStartTime, title, cost, reader.GetInt32("MinParticipants")),
                "Unknown" => throw new InvalidOperationException($"Unknown activity type")
            };
            results.Add(activity);
        }

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


    public List<EventDetail> SearchEvent(DateTime searchDate, SearchFilter filter, List<EventDetail> data)
    {
        // var results = new List<EventDetail>();
        // foreach (var item in data)
        // {
        //     DateTime eventDate = DateTime.ParseExact(item.DateStartTime, "d/MM/yyyy H:mm", CultureInfo.InvariantCulture).Date;
        //     bool isMatched = filter switch
        //     {
        //         SearchFilter.before => eventDate < searchDate,
        //         SearchFilter.on => eventDate == searchDate,
        //         SearchFilter.after => eventDate > searchDate,
        //         _ => false
        //     };
        //     if (isMatched)
        //     {
        //         results.Add(item);
        //     }
        // }

        // return results.OrderByDescending(e => DateTime.ParseExact(e.DateStartTime, "d/MM/yyyy H:mm", CultureInfo.InvariantCulture)).ToList();

        using var connection = Db.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SearchActivitesByDate(@d,@op)";
        command.Parameters.AddWithValue("p", searchDate);
        command.Parameters.AddWithValue("op", filter);
        command.CommandType = System.Data.CommandType.StoredProcedure;
        var results = new List<EventDetail>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
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

    /// <summary>
    /// Ultils to get path to csv files
    /// </summary>
    /// <param name="folderName"></param>
    /// <returns></returns>
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


