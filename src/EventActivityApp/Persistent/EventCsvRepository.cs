using CsvHelper;
using CsvHelper.Configuration;
using EventActivityApp.Domain;
using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;

namespace EventActivityApp.Persistent;

public class EventCsvRepository
{
    public string _fileName = "FitnessActivities.CSV";
    public string _filePath => Path.Combine(GetPathToProjectFolder("Data"), _fileName);

    public void SetEvent(string type)
    {
        if (type == "fitness")
        {
            _fileName = "FitnessActivities.CSV";
        }
        else if (type == "entertainment")
        {
            _fileName = "EntertainmentActivities.CSV";
        }
        else
        {
            throw new ArgumentException("Invalid event type");
        }
    }

    public List<EventDetail> ReadCsv()
    {
        var results = new List<EventDetail>();
        results.AddRange(ReadFile<FitnessEvent>("FitnessActivities.CSV"));
        results.AddRange(ReadFile<EntertainmentEvent>("EntertainmentActivities.CSV"));
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
        string path = Path.Combine(GetPathToProjectFolder("Data"), fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"File not found at {path}");
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            PrepareHeaderForMatch = args => args.Header.ToLower(),
        };
        using (var reader = new StreamReader(path))
        using (var csv = new CsvReader(reader, config))
        {

            return csv.GetRecords<T>().ToList();
        }
    }

    public void WriteCsv<T>(T record) where T : EventDetail
    {

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


