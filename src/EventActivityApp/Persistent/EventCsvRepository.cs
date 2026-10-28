using CsvHelper;
using CsvHelper.Configuration;
using EventActivityApp.Domain;
using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
namespace EventActivityApp.Persistent;


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
    // public EventType CurrentType { get; set; }
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
            results.AddRange(ReadFile<FitnessEvent>("FitnessActivities.CSV"));
        }
        else if (type is EventType.entertainment)
        {
            results.AddRange(ReadFile<EntertainmentEvent>("EntertainmentActivities.CSV"));
        }
        else
        {
            throw new ArgumentOutOfRangeException("Invalid event type");
        }
        return results;
    }

    public List<EventDetail> ReadAllCsv()
    {
        var results = new List<EventDetail>();
        results.AddRange(ReadCsv(EventType.fitness));
        results.AddRange(ReadCsv(EventType.entertainment));
        return results;
    }

    /// <summary>
    /// Reads all records from a CSV file in the Data folder and maps each row to an event of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The event type to create for each row (for example, <see cref="FitnessEvent"/> or <see cref="EntertainmentEvent"/>).</typeparam>
    /// <param name="fileName">The name of the CSV file to read, such as "FitnessActivities.CSV".</param>
    /// <returns>A list of the events read from the file.</returns>
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
                csv.Context.RegisterClassMap<FitnessEventMap>();
                csv.Context.RegisterClassMap<EntertainmentEventMap>();
                csv.WriteRecord(record);
                csv.NextRecord();
            }
        }
        ;

    }


    public List<EventDetail> SearchEvent(DateTime searchDate, SearchFilter filter, List<EventDetail> data)
    {
        var results = new List<EventDetail>();
        foreach (var item in data)
        {
            DateTime eventDate = DateTime.ParseExact(item.DateStartTime, "d/MM/yyyy H:mm", CultureInfo.InvariantCulture).Date;
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



        return results.OrderByDescending(e => DateTime.ParseExact(e.DateStartTime, "d/MM/yyyy H:mm", CultureInfo.InvariantCulture)).ToList();
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


