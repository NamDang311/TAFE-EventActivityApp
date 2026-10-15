using System;
using CsvHelper;
using CsvHelper.Configuration;

namespace EventActivityApp.Persistent;

public class EventCsvRepository
{
    public readonly string _filePath = Path.Combine("Data", "EntertainmentActivities.CSV");
    public EventCsvRepository(string filePath)
    {
        _filePath = filePath;
    }
    public void SaveEvent(EventActivityApp.Domain.EventDetail eventDetail)
    {
        var csvLine = $"{eventDetail.Date},{eventDetail.Title},{eventDetail.Cost},{eventDetail.Location}";
        
    })
}
