using System;

namespace EventActivityApp.Domain;

public class EventDetail
{
    public string Date { get; set
        {
          
            if (DateTime.Parse(value) < DateTime.Now)
            {
                throw new ArgumentException("Date cannot be in the past.");
            }
            field = value;
        } }
    public  string Title { get; set; }
    public decimal Cost { get; set { 
        if (value < 0) { throw new ArgumentException("Cost cannot be negative."); }
        field=value;
        } }
    public  string Location { get; set; }

    public EventDetail(string date, string title, decimal cost, string location)
    {
        Date = date;
        Title = title;
        Cost = cost;
        Location = location;
    }

    public override string ToString()
    {
        return $"Date: {Date}, Title: {Title}, Cost: {Cost:C}, Location: {Location}";
    }
}
