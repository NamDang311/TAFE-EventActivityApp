using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;

namespace EventActivityApp.Persistent;

public class EventCsvRepository
{
    public readonly string _filePath = Path.Combine(GetPathToProjectFolder("Data"), "EntertainmentActivities.CSV");
   
    public void SaveEvent(EventActivityApp.Domain.EventDetail eventDetail)
    {
        WriteCsv(eventDetail);
    }
    public void WriteCsv(EventActivityApp.Domain.EventDetail record) {
       
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


