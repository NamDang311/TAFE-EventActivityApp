namespace EventActivityApp.Domain;

/// <summary>
/// A fitness event that takes place at a specific location.
/// </summary>
public class FitnessActivity : EventDetail
{
    /// <summary>
    /// Gets or sets where the fitness event takes place.
    /// </summary>
    public string Location { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FitnessActivity"/> class.
    /// </summary>
    /// <param name="dateStartTime">The date and time the event starts.</param>
    /// <param name="title">The title of the event.</param>
    /// <param name="cost">The cost of the event. Must not be negative.</param>
    /// <param name="location">The location of the event.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="cost"/> is negative.</exception>
    public FitnessActivity(DateTime dateStartTime, string title, decimal cost, string location) : base(dateStartTime, title, cost)
    {
        Location = location;
    }

    /// <summary>
    /// Returns a readable summary of the fitness event.
    /// </summary>
    /// <returns>A string containing the date, title, cost and location.</returns>
    public override string ToString()
    {
        return $"Fitness event Date: {DateStartTime}, Title: {Title}, Cost: {Cost:C}, Location: {Location}";
    }
}
