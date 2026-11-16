using EventActivityApp.Domain;
using EventActivityApp.Data;
namespace EventActivityApp.Persistent;


/// <summary>
/// Provides data access operations for events using stored procedures
/// against the underlying database.
/// </summary>
public class EventDbRepository : DataRepository
{
    /// <summary>
    /// Database helper used to open connections and execute commands.
    /// </summary>
    public Db Db = new Db();

    /// <summary>
    /// Reads all activities of the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The event type to filter by.</param>
    /// <returns>A list of activities matching the requested type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the event type is not supported.</exception>
    public List<EventDetail> ReadActivitiesbyType(EventType type)
    {
        var results = new List<EventDetail>();
        using var connection = Db.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = type switch
        {
            EventType.fitness => "GetAllFitnessActivities",
            EventType.entertainment => "GetAllEntertainmentActivities",
            _ => throw new InvalidOperationException("Incorrect event type")
        };
        command.CommandType = System.Data.CommandType.StoredProcedure;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            DateTime dateStartTime = reader.GetDateTime("DateStartTime");
            string title = reader.GetString("Title");
            decimal cost = reader.GetDecimal("Cost");

            if (type == EventType.fitness)
            { results.Add(new FitnessActivity(dateStartTime, title, cost, reader.GetString("Location"))); }
            else
            {
                results.Add(new EntertainmentActivity(dateStartTime, title, cost, reader.GetInt16("MinParticipants")));
            }
        }

        return results;

    }

    /// <summary>
    /// Reads all activities of every type, mapping each row to a
    /// <see cref="FitnessActivity"/> or <see cref="EntertainmentActivity"/>.
    /// </summary>
    /// <returns>A list of all activities.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a row has an unknown activity type.</exception>
    public List<EventDetail> ReadAllActivities()
    {
        var results = new List<EventDetail>();
        using var connection = Db.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "GetAllActivities";
        command.CommandType = System.Data.CommandType.StoredProcedure;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            DateTime dateStartTime = reader.GetDateTime("DateStartTime");
            string title = reader.GetString("Title");
            decimal cost = reader.GetDecimal("Cost");

            // Check activityType to map new object.
            EventDetail activity;
            switch (reader.GetString("ActivityType"))
            {
                case "Fitness":
                    activity = new FitnessActivity(dateStartTime, title, cost, reader.GetString("Location"));
                    break;
                case "Entertainment":
                    activity = new EntertainmentActivity(dateStartTime, title, cost, reader.GetInt16("MinParticipants"));
                    break;
                default:
                    throw new InvalidOperationException("Unknown activity type");
            }
            results.Add(activity);
        }

        return results;
    }


    /// <summary>
    /// Inserts a new activity using the stored procedure that matches its type.
    /// </summary>
    /// <param name="record">The fitness or entertainment activity to save.</param>
    /// <exception cref="InvalidOperationException">Thrown when the activity type is not supported.</exception>
    public void WriteEvent(EventDetail record)
    {

        using var connection = Db.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandType = System.Data.CommandType.StoredProcedure;
        command.Parameters.AddWithValue("p_DateStartTime", record.DateStartTime);
        command.Parameters.AddWithValue("p_Title", record.Title);
        command.Parameters.AddWithValue("p_Cost", record.Cost);
        switch (record)
        {
            case FitnessActivity fitness:
                command.CommandText = "AddFitnessActivity";
                command.Parameters.AddWithValue("p_Location", fitness.Location);
                break;
            case EntertainmentActivity entertainment:
                command.CommandText = "AddEntertainmentActivity";
                command.Parameters.AddWithValue("p_MinParticipants", entertainment.MinParticipants);
                break;
            default:
                throw new InvalidOperationException("Invalid event type.");
        }

        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Checks whether an activity already exists on the given date.
    /// </summary>
    /// <param name="date">The date to check; the time portion is ignored.</param>
    /// <returns><c>true</c> if an activity exists on that date; otherwise <c>false</c>.</returns>
    public bool CheckEventExist(DateTime date)
    {
        using var connection = Db.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "CheckActivityExistsByDate";
        command.CommandType = System.Data.CommandType.StoredProcedure;
        command.Parameters.AddWithValue("p_DateToCheck", date.Date);

        using var reader = command.ExecuteReader();
        reader.Read();
        int exists = reader.GetInt32("ActivityExists");
        return exists == 1;
    }

    /// <summary>
    /// Updates the activity scheduled on the same date as <paramref name="record"/>.
    /// Nothing is changed unless exactly one activity exists on that date.
    /// </summary>
    /// <param name="record">The new values for the activity.</param>
    /// <exception cref="InvalidOperationException">Thrown when the activity type is not supported.</exception>
    public void UpdateEvent(EventDetail record)
    {
        // Check if there is an existing event on the date
        List<EventDetail> refKey = SearchEvent(record.DateStartTime, SearchFilter.on);
        if (refKey.Count == 1)
        {
            using var connection = Db.OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "UpdateActivityWithTypeCheck";
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.AddWithValue("p_ActivityID", refKey[^1].ActivityID);
            command.Parameters.AddWithValue("p_NewDateStartTime", record.DateStartTime);
            command.Parameters.AddWithValue("p_NewTitle", record.Title);
            command.Parameters.AddWithValue("p_NewCost", record.Cost);
            switch (record)
            {
                case FitnessActivity fitness:
                    command.Parameters.AddWithValue("p_Location", fitness.Location);
                    command.Parameters.AddWithValue("p_NewType", "Fitness");
                    command.Parameters.AddWithValue("p_MinParticipants", DBNull.Value);
                    break;
                case EntertainmentActivity entertainment:
                    command.Parameters.AddWithValue("p_Location", DBNull.Value);
                    command.Parameters.AddWithValue("p_NewType", "Entertainment");
                    command.Parameters.AddWithValue("p_MinParticipants", entertainment.MinParticipants);
                    break;
                default:
                    throw new InvalidOperationException("Invalid event type.");
            }
            command.ExecuteNonQuery();
        }
    }


    /// <summary>
    /// Searches activities relative to a date using the specified filter.
    /// </summary>
    /// <param name="searchDate">The reference date; the time portion is ignored.</param>
    /// <param name="filter">The comparison to apply (for example on, before or after).</param>
    /// <returns>Matching activities, ordered by start time descending.</returns>
    public List<EventDetail> SearchEvent(DateTime searchDate, SearchFilter filter)
    {
        var results = new List<EventDetail>();
        using var connection = Db.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SearchActivitiesByDate";
        command.CommandType = System.Data.CommandType.StoredProcedure;
        command.Parameters.AddWithValue("p_SearchDate", searchDate.Date);
        command.Parameters.AddWithValue("p_Operator", filter.ToString());

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            DateTime dateStartTime = reader.GetDateTime("DateStartTime");
            string title = reader.GetString("Title");
            decimal cost = reader.GetDecimal("Cost");
            Int32 id = reader.GetInt32("ActivityID");

            EventDetail activity = !reader.IsDBNull(reader.GetOrdinal("Location"))
            ? new FitnessActivity(dateStartTime, title, cost, reader.GetString("Location"))
            : new EntertainmentActivity(dateStartTime, title, cost, reader.GetInt16("MinParticipants"));
            activity.ActivityID = id;
            results.Add(activity);
        }

        // Re-sort the result list.
        return results.OrderByDescending(e => e.DateStartTime).ToList();

    }

    /// <summary>
    /// Deletes an activity and its type-specific rows by identifier.
    /// </summary>
    /// <param name="id">The activity identifier.</param>
    public void DeleteActivityById(int id)
    {
        using var connection = Db.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM FitnessActivities WHERE ActivityID = @id; DELETE FROM EntertainmentActivities WHERE ActivityID = @id;DELETE FROM Activities WHERE ActivityID = @id;";
        command.Parameters.AddWithValue("@id", id);
        command.ExecuteNonQuery();
    }

}


