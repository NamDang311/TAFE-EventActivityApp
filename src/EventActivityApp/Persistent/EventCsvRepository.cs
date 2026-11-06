using CsvHelper;
using CsvHelper.Configuration;
using EventActivityApp.Domain;
using System.Globalization;
namespace EventActivityApp.Persistent;

using EventActivityApp.Data;
using Microsoft.VisualBasic;
using MySqlConnector;

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
    public Db Db = new Db();

    // private string GetFileName(EventType type) => type switch
    // {
    //     EventType.fitness => "FitnessActivities.CSV",
    //     EventType.entertainment => "EntertainmentActivities.CSV",
    //     _ => throw new InvalidOperationException("Set correct event type")
    // };
    // public string DataFolder { get; set; } = GetPathToProjectFolder("Data");
    // public string _filePath = "";


    // public List<EventDetail> ReadCsv(EventType type)
    // {
    //     _filePath = Path.Combine(DataFolder, GetFileName(type));
    //     var results = new List<EventDetail>();
    //     if (type is EventType.fitness)
    //     {
    //         results.AddRange(ReadFile<FitnessActivity>("FitnessActivities.CSV"));
    //     }
    //     else if (type is EventType.entertainment)
    //     {
    //         results.AddRange(ReadFile<EntertainmentActivity>("EntertainmentActivities.CSV"));
    //     }
    //     else
    //     {
    //         throw new ArgumentOutOfRangeException("Invalid event type");
    //     }
    //     return results;
    // }

    public List<EventDetail> ReadActivitiesbyType(EventType type)
    {
        var results = new List<EventDetail>();
        using var connection = Db.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = type switch
        {
            EventType.fitness => "GetAllFitnessActivities",
            EventType.entertainment => "GetAllEntertainmentActivities",
            _ => throw new InvalidOperationException("Incorrect event type")
        };
        command.CommandType = System.Data.CommandType.StoredProcedure;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            DateTime dateStartTime = reader.GetDateTime("DateStartTime");
            string title = reader.GetString("Title");
            decimal cost = reader.GetDecimal("Cost");

            if (type == EventType.fitness)
            { results.Add(new FitnessActivity(dateStartTime, title, cost, reader.GetString("Location"))); }
            else
            {
                results.Add(new EntertainmentActivity(dateStartTime, title, cost, reader.GetInt16("MinParticipants")));
            }
        }

        return results;

    }

    public List<EventDetail> ReadAllActivities()
    {

        /** old methods*/
        // var results = new List<EventDetail>();
        // results.AddRange(ReadCsv(EventType.fitness));
        // results.AddRange(ReadCsv(EventType.entertainment));
        // return results;
        /** ----------------- */

        var results = new List<EventDetail>();
        using var connection = Db.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "GetAllActivities";
        command.CommandType = System.Data.CommandType.StoredProcedure;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            DateTime dateStartTime = reader.GetDateTime("DateStartTime");
            string title = reader.GetString("Title");
            decimal cost = reader.GetDecimal("Cost");

            // Check activityType to map new object.
            EventDetail activity = reader.GetString("ActivityType") switch
            {
                "Fitness" => new FitnessActivity(dateStartTime, title, cost, reader.GetString("Location")),
                "Entertainment" => new EntertainmentActivity(dateStartTime, title, cost, reader.GetInt16("MinParticipants")),
                _ => throw new InvalidOperationException($"Unknown activity type")
            };
            results.Add(activity);
        }

        return results;
    }


    // private List<T> ReadFile<T>(string fileName) where T : EventDetail
    // {
    //     if (!File.Exists(_filePath))
    //     {
    //         throw new FileNotFoundException($"File not found at {_filePath}");
    //     }

    //     var config = new CsvConfiguration(CultureInfo.InvariantCulture)
    //     {
    //         PrepareHeaderForMatch = args => args.Header.ToLower(),
    //     };
    //     using (var reader = new StreamReader(_filePath))
    //     using (var csv = new CsvReader(reader, config))
    //     {
    //         // Records are built through constructors, so [Format] on the property is ignored
    //         csv.Context.TypeConverterOptionsCache.GetOptions<DateTime>().Formats = [CsvDateFormat];

    //         return csv.GetRecords<T>().ToList();
    //     }
    // }

    // public void WriteCsv<T>(T record, EventType type) where T : EventDetail
    // {
    // _filePath = Path.Combine(DataFolder, GetFileName(type));
    // using (var stream = File.Open(_filePath, FileMode.Append))
    // using (var writer = new StreamWriter(stream))
    // {
    //     var config = new CsvConfiguration(CultureInfo.InvariantCulture)
    //     {
    //         // Exclude header.
    //         HasHeaderRecord = false,
    //     };
    //     using (var csv = new CsvWriter(writer, config))
    //     {
    //         csv.Context.TypeConverterOptionsCache.GetOptions<DateTime>().Formats = [CsvDateFormat];
    //         csv.Context.RegisterClassMap<FitnessActivityMap>();
    //         csv.Context.RegisterClassMap<EntertainmentActivityMap>();
    //         csv.WriteRecord(record);
    //         csv.NextRecord();
    //     }
    // }
    // ;

    // }
    // public void WriteEvent<T>(T record, EventType type) where T : EventDetail
    public void WriteEvent(EventDetail record)
    {

        using var connection = Db.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandType = System.Data.CommandType.StoredProcedure;
        command.Parameters.AddWithValue("p_DateStartTime", record.DateStartTime);
        command.Parameters.AddWithValue("p_Title", record.Title);
        command.Parameters.AddWithValue("p_Cost", record.Cost);
        switch (record)
        {
            case FitnessActivity fitness:
                command.CommandText = "AddFitnessActivity";
                command.Parameters.AddWithValue("p_Location", fitness.Location);
                break;
            case EntertainmentActivity entertainment:
                command.CommandText = "AddEntertainmentActivity";
                command.Parameters.AddWithValue("p_MinParticipants", entertainment.MinParticipants);
                break;
            default:
                throw new InvalidOperationException("Invalid event type.");
        }

        command.ExecuteNonQuery();
    }

    public bool CheckEventExist(DateTime date)
    {
        using var connection = Db.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "CheckActivityExistsByDate";
        command.CommandType = System.Data.CommandType.StoredProcedure;
        command.Parameters.AddWithValue("p_DateToCheck", date.Date);

        using var reader = command.ExecuteReader();
        reader.Read();                                   
        int exists = reader.GetInt32("ActivityExists"); 
        return exists == 1;
    }

    public void UpdateEvent(EventDetail record)
    {
        var refKey = SearchEvent(record.DateStartTime, SearchFilter.on);
        if (refKey.Count == 1)
        {
            using var connection = Db.OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "UpdateActivityWithTypeCheck";
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.AddWithValue("p_ActivityID", refKey[^1].ActivityID);
            command.Parameters.AddWithValue("p_NewDateStartTime", record.DateStartTime);
            command.Parameters.AddWithValue("p_NewTitle", record.Title);
            command.Parameters.AddWithValue("p_NewCost", record.Cost);
            switch (record)
            {
                case FitnessActivity fitness:
                    command.Parameters.AddWithValue("p_Location", fitness.Location);
                    command.Parameters.AddWithValue("p_NewType", "Fitness");
                    command.Parameters.AddWithValue("p_MinParticipants", DBNull.Value);
                    break;
                case EntertainmentActivity entertainment:
                    command.Parameters.AddWithValue("p_Location", DBNull.Value);
                    command.Parameters.AddWithValue("p_NewType", "Entertainment");
                    command.Parameters.AddWithValue("p_MinParticipants", entertainment.MinParticipants);

                    break;
                default:
                    throw new InvalidOperationException("Invalid event type.");
            }
            command.ExecuteNonQuery();
        }
    }

    // public List<EventDetail> SearchEvent(DateTime searchDate, SearchFilter filter, List<EventDetail> data)
    // {
    //     var results = new List<EventDetail>();
    //     foreach (var item in data)
    //     {
    //         DateTime eventDate = DateTime.ParseExact(item.DateStartTime, "d/MM/yyyy H:mm", CultureInfo.InvariantCulture).Date;
    //         bool isMatched = filter switch
    //         {
    //             SearchFilter.before => eventDate < searchDate,
    //             SearchFilter.on => eventDate == searchDate,
    //             SearchFilter.after => eventDate > searchDate,
    //             _ => false
    //         };
    //         if (isMatched)
    //         {
    //             results.Add(item);
    //         }
    //     }
    //     return results.OrderByDescending(e => DateTime.ParseExact(e.DateStartTime, "d/MM/yyyy H:mm", CultureInfo.InvariantCulture)).ToList();
    // }

    public List<EventDetail> SearchEvent(DateTime searchDate, SearchFilter filter)
    {
        var results = new List<EventDetail>();
        using var connection = Db.OpenConnection();
        string query = "SearchActivitiesByDate";
        using var command = new MySqlCommand(query, connection);
        command.CommandType = System.Data.CommandType.StoredProcedure;
        command.Parameters.AddWithValue("p_SearchDate", searchDate.Date);
        command.Parameters.AddWithValue("p_Operator", filter.ToString());

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            DateTime dateStartTime = reader.GetDateTime("DateStartTime");
            string title = reader.GetString("Title");
            decimal cost = reader.GetDecimal("Cost");
            Int32 id = reader.GetInt32("ActivityID");

            /** 
            Since DB procedure doesn't return activity type, this is a way around. 
            check if whether the return reader location col is not null
            if YES => Create FitnessActivity
            else => Create EntertainmentActivity
            ----
            Optional: Retrive ActivityID as part of query to map later.
            */
            EventDetail activity = !reader.IsDBNull(reader.GetOrdinal("Location"))
            ? new FitnessActivity(dateStartTime, title, cost, reader.GetString("Location"))
            : new EntertainmentActivity(dateStartTime, title, cost, reader.GetInt16("MinParticipants"));
            activity.ActivityID = id;
            results.Add(activity);

        }

        // Re-sort the result list.
        return results.OrderByDescending(e => e.DateStartTime).ToList();

    }

    /// <summary>
    /// Ultils to get path to csv files
    /// </summary>
    /// <param name="folderName"></param>
    /// <returns></returns>
    // private static string GetPathToProjectFolder(string folderName)
    // {
    //     // Get the base directory for the project.  This defaults to where the compiled code is
    //     string baseDir = AppDomain.CurrentDomain.BaseDirectory;
    // // Build a path to a folder in the project
    // string projectDir = Path.Combine(baseDir, "..", "..", "..", "..", "EventActivityApp", folderName);

    // // Turn the relative path into an absolute path
    // string finalDir = Path.GetFullPath(projectDir);

    //     return finalDir;
    // }
}


