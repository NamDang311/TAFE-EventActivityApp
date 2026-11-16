using CsvHelper.Configuration;
namespace EventActivityApp.Domain;
/* 
Map the property to fix the order when writting csv. 
CSVHelper: For a derived class, reflection usually lists the subclass's own properties before the inherited ones. 
*/

internal sealed class FitnessActivityMap : ClassMap<FitnessActivity>
{
    /// <summary>
    /// Initializes the map: DateStartTime, Title, Cost, Location.
    /// </summary>
    public FitnessActivityMap()
    {
        Map(m => m.DateStartTime).Index(0).Name("DateStartTime").TypeConverterOption.Format("d/MM/yyyy H:mm");
        Map(m => m.Title).Index(1).Name("Title");
        Map(m => m.Cost).Index(2).Name("Cost");
        Map(m => m.Location).Index(3).Name("Location");
    }
}

internal sealed class EntertainmentActivityMap : ClassMap<EntertainmentActivity>
{
    /// <summary>
    /// Initializes the map: DateStartTime, Title, Cost, MinParticipants.
    /// </summary>
    public EntertainmentActivityMap()
    {
        Map(m => m.DateStartTime).Index(0).Name("DateStartTime").TypeConverterOption.Format("d/MM/yyyy H:mm");
        Map(m => m.Title).Index(1).Name("Title");
        Map(m => m.Cost).Index(2).Name("Cost");
        Map(m => m.MinParticipants).Index(3).Name("MinParticipants");
    }
}