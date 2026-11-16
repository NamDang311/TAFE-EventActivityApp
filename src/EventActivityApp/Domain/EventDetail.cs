namespace EventActivityApp.Domain;

/// <summary>
/// Base class for all events, holding the details every event type shares.
/// </summary>
public abstract class EventDetail
{
    /// <summary>
    /// Gets or sets the unique identifier of the activity associated with the event.
    /// </summary>
    public int ActivityID { get; set; }
    /// <summary>
    /// Gets or sets the date and time the event starts.
    /// </summary>
    public DateTime DateStartTime { get; set; }

    /// <summary>
    /// Gets or sets the title of the event.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the cost of the event.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the value is negative.</exception>
    public decimal Cost
    {
        get; set
        {
            if (value < 0) { throw new ArgumentException("Cost cannot be negative."); }
            field = value;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventDetail"/> class.
    /// </summary>
    /// <param name="dateStartTime">The date and time the event starts.</param>
    /// <param name="title">The title of the event.</param>
    /// <param name="cost">The cost of the event. Must not be negative.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="cost"/> is negative.</exception>
    protected EventDetail(DateTime dateStartTime, string title, decimal cost)
    {
        DateStartTime = dateStartTime;
        Title = title;
        Cost = cost;
    }
}
