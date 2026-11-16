namespace EventActivityApp.Domain;

/// <summary>
/// An entertainment event that requires a minimum number of participants.
/// </summary>
public class EntertainmentActivity : EventDetail
{
    /// <summary>
    /// Gets or sets the minimum number of participants needed for the event to go ahead.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the value is less than 2.</exception>
    public int MinParticipants
    {
        get; set
        {
            if (value < 2) { throw new ArgumentException("Min participant is 2."); }
            field = value;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntertainmentActivity"/> class.
    /// </summary>
    /// <param name="dateStartTime">The date and time the event starts.</param>
    /// <param name="title">The title of the event.</param>
    /// <param name="cost">The cost of the event. Must not be negative.</param>
    /// <param name="minParticipants">The minimum number of participants. Must be at least 2.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="cost"/> is negative or <paramref name="minParticipants"/> is less than 2.</exception>
    public EntertainmentActivity(DateTime dateStartTime, string title, decimal cost, int minParticipants) : base(dateStartTime, title, cost)
    {
        MinParticipants = minParticipants;
    }

    /// <summary>
    /// Returns a readable summary of the entertainment event.
    /// </summary>
    /// <returns>A string containing the date, title, cost and minimum participants.</returns>
    public override string ToString()
    {
        return $"Entertainment event Date: {DateStartTime}, Title: {Title}, Cost: {Cost:C}, Min Participant: {MinParticipants}";
    }
}
