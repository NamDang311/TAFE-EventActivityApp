namespace EventActivityApp.Persistent;

using EventActivityApp.Domain;


/// <summary>
/// The kind of event, which also decides the CSV file it is stored in.
/// </summary>
public enum EventType
{
    /// <summary>No type selected.</summary>
    none,
    /// <summary>A fitness event.</summary>
    fitness,
    /// <summary>An entertainment event.</summary>
    entertainment,
}

/// <summary>
/// How to compare event dates against a search date.
/// </summary>
public enum SearchFilter
{
    /// <summary>Events dated before the search date.</summary>
    before,
    /// <summary>Events dated on the search date.</summary>
    on,
    /// <summary>Events dated after the search date.</summary>
    after
}

/// <summary>
/// Base class for data repositories
/// </summary>
public abstract class DataRepository
{
 
}