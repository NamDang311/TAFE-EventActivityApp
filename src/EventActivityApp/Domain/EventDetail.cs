using System;
using System.Data.Common;
using System.Diagnostics;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
namespace EventActivityApp.Domain;

public abstract class EventDetail
{
    public int ActivityID { get; set; }
    public DateTime DateStartTime { get; set; }
    public string Title
    {
        get; set
        {
            if (value.Length < 3)
            {
                throw new ArgumentException("Title must have at least 3 characters.");
            }
            field = value;
        }
    }
    public decimal Cost
    {
        get; set
        {
            if (value < 0) { throw new ArgumentException("Cost cannot be negative."); }
            field = value;
        }
    }

    protected EventDetail(DateTime dateStartTime, string title, decimal cost)
    {
        DateStartTime = dateStartTime;
        Title = title;
        Cost = cost;
    }

}

public class FitnessActivity : EventDetail
{
    public string Location { get; set; }
    public FitnessActivity(DateTime dateStartTime, string title, decimal cost, string location)
         : base(dateStartTime, title, cost)
    {
        Location = location;
    }
    public override string ToString()
    {
        return $"Fitness event Date: {DateStartTime}, Title: {Title}, Cost: {Cost:C}, Location: {Location}";
    }
}

public class EntertainmentActivity : EventDetail
{
    public int MinParticipants
    {
        get; set
        {
            if (value < 2) { throw new ArgumentException("Min participant is 2."); }
            field = value;
        }
    }
    public EntertainmentActivity(DateTime dateStartTime, string title, decimal cost, int minParticipants)
         : base(dateStartTime, title, cost)
    {
        MinParticipants = minParticipants;
    }


    public override string ToString()
    {
        return $"Entertainment event Date: {DateStartTime}, Title: {Title}, Cost: {Cost:C}, Min Participant: {MinParticipants}";
    }
}



// Map the property to fix the order when writting csv. 
// CSVHelper: For a derived class, reflection usually lists the subclass's own properties before the inherited ones. 

public sealed class FitnessActivityMap : ClassMap<FitnessActivity>
{
    public FitnessActivityMap()
    {
        Map(m => m.DateStartTime).Index(0).Name("DateStartTime").TypeConverterOption.Format("d/MM/yyyy H:mm");
        Map(m => m.Title).Index(1).Name("Title");
        Map(m => m.Cost).Index(2).Name("Cost");
        Map(m => m.Location).Index(3).Name("Location");
    }
}

public sealed class EntertainmentActivityMap : ClassMap<EntertainmentActivity>
{
    public EntertainmentActivityMap()
    {
        Map(m => m.DateStartTime).Index(0).Name("DateStartTime").TypeConverterOption.Format("d/MM/yyyy H:mm");
        Map(m => m.Title).Index(1).Name("Title");
        Map(m => m.Cost).Index(2).Name("Cost");
        Map(m => m.MinParticipants).Index(3).Name("MinParticipants");
    }
}