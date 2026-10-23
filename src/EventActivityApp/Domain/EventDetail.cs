using System;
using System.Runtime.CompilerServices;
using CsvHelper.Configuration;
namespace EventActivityApp.Domain;



public class EventDetail
{
    public string DateStartTime { get; set; }
    public string Title { get; set; }
    public decimal Cost
    {
        get; set
        {
            if (value < 0) { throw new ArgumentException("Cost cannot be negative."); }
            field = value;
        }
    }

    // Used by CsvHelper when reading records
    protected EventDetail()
    {
        DateStartTime = string.Empty;
        Title = string.Empty;
    }

    protected EventDetail(string dateStartTime, string title, decimal cost)
    {
        DateStartTime = dateStartTime;
        Title = title;
        Cost = cost;
    }

}

public class FitnessEvent : EventDetail
{
    public string Location { get; set; }
    public FitnessEvent(string dateStartTime, string title, decimal cost, string location)
         : base(dateStartTime, title, cost)
    {
        Location = location;
    }
    public override string ToString()
    {
        return $"Fitness event Date: {DateStartTime}, Title: {Title}, Cost: {Cost:C}, Location: {Location}";
    }
}

public class EntertainmentEvent : EventDetail
{
    public int MinParticipants { get; set; }
    public EntertainmentEvent(string dateStartTime, string title, decimal cost, int minParticipants)
         : base(dateStartTime, title, cost)
    {
        MinParticipants = minParticipants;
    }


    public override string ToString()
    {
        return $"Entertainment event Date: {DateStartTime}, Title: {Title}, Cost: {Cost:C}, Min Participant: {MinParticipants}";
    }
}



/* 
Map the property to fix the order when writting csv. 
CSVHelper: For a derived class, reflection usually lists the subclass's own properties before the inherited ones. 
*/
public sealed class FitnessEventMap : ClassMap<FitnessEvent>
{
    public FitnessEventMap()
    {
        Map(m => m.DateStartTime).Index(0).Name("DateStartTime");
        Map(m => m.Title).Index(1).Name("Title");
        Map(m => m.Cost).Index(2).Name("Cost");
        Map(m => m.Location).Index(3).Name("Location");
    }
}

public sealed class EntertainmentEventMap : ClassMap<EntertainmentEvent>
{
    public EntertainmentEventMap()
    {
        Map(m => m.DateStartTime).Index(0).Name("DateStartTime");
        Map(m => m.Title).Index(1).Name("Title");
        Map(m => m.Cost).Index(2).Name("Cost");
        Map(m => m.MinParticipants).Index(3).Name("MinParticipants");
    }
}