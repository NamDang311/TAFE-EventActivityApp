using System;
using System.Collections.Generic;
using System.IO;
using EventActivityApp.Data;
using EventActivityApp.Domain;
namespace EventActivityApp.Persistent;


public class EventCsvRepositoryTest
{
    // private readonly string baseDir = AppDomain.CurrentDomain.BaseDirectory;
    // private readonly EventCsvRepository _repo;
    private readonly Db db = new Db();

    // public EventCsvRepositoryTest()
    // {
    //     _repo = new EventCsvRepository
    //     {
    //         DataFolder = Path.Combine(
    //             baseDir, "..", "..", "..", "Data")
    //     };
    // }

    // [Fact]
    // private void FitnessMockFile_ShouldExist()
    // {
    //     string path = Path.Combine(_repo.DataFolder, "FitnessActivities.CSV");
    //     Assert.True(File.Exists(path));
    // }

    // [Fact]
    // private void EntertainmentMockFile_ShouldExist()
    // {
    //     string path = Path.Combine(_repo.DataFolder, "EntertainmentActivities.CSV");
    //     Assert.True(File.Exists(path));

    // }

    // [Fact]
    // public void FilesMissing_ThrowsFileNotFound()
    // {
    //     string path = Path.Combine(_repo.DataFolder, "FitnessActivities.CSV");
    //     string fitnessContent = File.ReadAllText(path);
    //     try
    //     {
    //         File.Delete(path);
    //         Assert.Throws<FileNotFoundException>(() => _repo.ReadCsv(EventType.fitness));
    //     }
    //     finally
    //     {
    //         File.WriteAllText(path, fitnessContent);
    //     }
    // }



    // [Fact]
    // private void CanReadFitnessEvents_ShouldReturnList()
    // {
    //     var list = _repo.ReadCsv(EventType.fitness);
    //     Assert.Equal("FitnessActivities.CSV", Path.GetFileName(_repo._filePath));
    //     Assert.NotEmpty(list);
    //     Assert.IsType<List<EventDetail>>(list);

    // }

    // [Fact]
    // private void CanReadEntertainmentEvents_ShouldReturnList()
    // {

    //     var list = _repo.ReadCsv(EventType.entertainment);
    //     Assert.Equal("EntertainmentActivities.CSV", Path.GetFileName(_repo._filePath));
    //     Assert.NotEmpty(list);
    //     Assert.IsType<List<EventDetail>>(list);
    // }

    // [Fact]
    // private void CanSaveFitnessEvent_ShouldWriteToFile()
    // {
    //     string path = Path.Combine(_repo.DataFolder, "FitnessActivities.CSV");
    //     string original = File.ReadAllText(path);
    //     var event1 = new EventActivityApp.Domain.FitnessEvent("28/10/2026", "Swimming", 3, "Liverpool Center");
    //     string[] lastLine;

    //     try
    //     {

    //         _repo.WriteCsv(event1, EventType.fitness);
    //         lastLine = File.ReadAllLines(path);
    //         Assert.Equal(
    //             "28/10/2026,Swimming,3,Liverpool Center", lastLine[^1]);

    //     }

    //     finally
    //     {
    //         File.WriteAllText(path, original);
    //     }
    // }

    // [Fact]
    // public void CanSaveEntertainmentEvent_ShouldWriteToFile()
    // {
    //     string path = Path.Combine(_repo.DataFolder, "EntertainmentActivities.CSV");
    //     string original = File.ReadAllText(path);
    //     var event1 = new EventActivityApp.Domain.EntertainmentEvent("28/10/2026", "Chess", 3, 1);
    //     string[] lastLine;

    //     try
    //     {
    //         _repo.WriteCsv(event1, EventType.entertainment);
    //         lastLine = File.ReadAllLines(path);
    //         Assert.Equal(
    //             "28/10/2026,Chess,3,1", lastLine[^1]);

    //     }

    //     finally
    //     {
    //         File.WriteAllText(path, original);
    //     }
    // }

    [Theory]
    [InlineData(SearchFilter.on)]
    public void SearchEvent_ReturnsEventsMatchingFilter(SearchFilter filter)
    {
        using var connection = db.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SearchActivitiesByDate";
        command.Parameters.AddWithValue("p_SearchDate", new DateTime(2025, 3, 9));
        command.Parameters.AddWithValue("p_Operator", filter.ToString());
        command.CommandType = System.Data.CommandType.StoredProcedure;
        var results = new List<EventDetail>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            DateTime dateStartTime = reader.GetDateTime("DateStartTime");
            string title = reader.GetString("Title");
            decimal cost = reader.GetDecimal("Cost");

            string start = dateStartTime.ToString("yyyy-MM-dd HH:mm:ss");

            // Entertainment rows have no Location
            EventDetail activity = reader.IsDBNull(reader.GetOrdinal("Location"))
                ? new EntertainmentEvent(start, title, cost, reader.GetInt32("MinParticipants"))
                : new FitnessEvent(start, title, cost, reader.GetString("Location"));
            results.Add(activity);
        }

        var data = new List<EventDetail>
    {
        new FitnessEvent("2025-03-09 08:00:00", "Yoga", 10, "Bondi Beach"),
    };

        Assert.Equal(data[0].Title, Assert.Single(results).Title);
    }
}


