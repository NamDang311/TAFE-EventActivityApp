using System;
using System.Collections.Generic;
using System.Data;
using EventActivityApp.Data;
using EventActivityApp.Domain;
using MySqlConnector;
using Xunit.Abstractions;

namespace EventActivityApp.Persistent;

// TODO: Db as a subclass
public class EventCsvRepositoryTest
{
    // private readonly string baseDir = AppDomain.CurrentDomain.BaseDirectory;
    private readonly EventCsvRepository _repo = new EventCsvRepository();
    private readonly Db db = new Db();
    private readonly ITestOutputHelper _output;

    public EventCsvRepositoryTest(ITestOutputHelper output)
    {
        _output = output;
    }
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

    [Fact]
    public void CanConnectToDatabase()
    {
        using var connection = db.OpenConnection();
        Assert.Equal(ConnectionState.Open, connection.State);
        Assert.Equal("ActivitiesDb", connection.Database);
    }

    // [Fact]
    // private void CanReadFitnessEvents_ShouldReturnList()
    // {
    //     var list = _repo.ReadCsv(EventType.fitness);
    //     Assert.Equal("FitnessActivities.CSV", Path.GetFileName(_repo._filePath));
    //     Assert.NotEmpty(list);
    //     Assert.IsType<List<EventDetail>>(list);

    // }

    [Fact]
    private void CanReadFitnessEvents_ShouldReturnList()
    {
        var results = _repo.ReadActivitiesbyType(EventType.fitness);
        Assert.NotEmpty(results);
        Assert.IsType<List<EventDetail>>(results);
    }

    // [Fact]
    // private void CanReadEntertainmentEvents_ShouldReturnList()
    // {

    //     var list = _repo.ReadCsv(EventType.entertainment);
    //     Assert.Equal("EntertainmentActivities.CSV", Path.GetFileName(_repo._filePath));
    //     Assert.NotEmpty(list);
    //     Assert.IsType<List<EventDetail>>(list);
    // }

    [Fact]
    private void CanReadEntertainmentEvents_ShouldReturnList()
    {
        var results = _repo.ReadActivitiesbyType(EventType.fitness);
        Assert.NotEmpty(results);
        Assert.IsType<List<EventDetail>>(results);
    }

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


    [Fact]
    private void CanSaveFitnessEvent_ShouldWriteDb()
    {
        var eventDate = new DateTime(2027, 12, 29, 7, 00, 00);

        var event1 = new EventActivityApp.Domain.FitnessActivity(eventDate, "Test Fitness Event - Should be deleted", 3, "Liverpool Center");
        try
        {
            _repo.WriteEvent(event1);
            var results = _repo.ReadActivitiesbyType(EventType.fitness);
            Assert.Contains(results, item => item.Title == event1.Title && item.DateStartTime == event1.DateStartTime);
        }
        catch (MySqlException ex)
        {
            _output.WriteLine(ex.Message);
        }
        finally
        {
            _repo.DeleteActivityById(_repo.SearchEvent(eventDate, SearchFilter.on)[^1].ActivityID);
        }

    }


    [Fact]
    private void UpdateFitnessEvent_ShouldUpdateDb()
    {
        var eventDate = new DateTime(2027, 12, 30, 7, 00, 00);
        var event1 = new EventActivityApp.Domain.FitnessActivity(eventDate, "Test Fitness Event - Overwritten", 3, "Liverpool Center");
        try
        {
            _repo.WriteEvent(event1);
        }
        catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
        {
            _repo.UpdateEvent(event1);
            var result = _repo.SearchEvent(eventDate, SearchFilter.on);
            Assert.Contains(result, item => item.Title == event1.Title);
        }
        finally
        {
            _repo.DeleteActivityById(_repo.SearchEvent(eventDate, SearchFilter.on)[^1].ActivityID);
        }

    }

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

    [Fact]
    private void CanSaveEntertainmentEvent_ShouldWriteDb()
    {
        var eventDate = new DateTime(2026, 12, 31, 7, 00, 00);
        var event1 = new EventActivityApp.Domain.EntertainmentActivity(eventDate, "Test Entertainment Event - Should be deleted", 12, 4);
        try
        {
            _repo.WriteEvent(event1);
            var results = _repo.ReadActivitiesbyType(EventType.entertainment);
            Assert.Contains(results, item => item.Title == event1.Title && item.DateStartTime == event1.DateStartTime);
        }

        catch (MySqlException ex)
        {
            _output.WriteLine(ex.Message);
        }
        finally
        {
            _repo.DeleteActivityById(_repo.SearchEvent(eventDate, SearchFilter.on)[^1].ActivityID);
        }
    }

    // [Theory]
    // [InlineData(SearchFilter.before, "Running")]
    // [InlineData(SearchFilter.on, "Yoga")]
    // [InlineData(SearchFilter.after, "Swimming")]
    // public void SearchEvent_ReturnsEventsMatchingFilter(SearchFilter filter, string expectedTitle)
    // {
    //     var data = new List<EventDetail>
    // {
    //     new FitnessActivity(new DateTime(2026, 10, 29, 9, 0, 0), "Running", 0, "Sydney Park"),
    //     new FitnessActivity(new DateTime(2026, 10, 30, 6, 23, 0), "Yoga", 12, "Mt Pritchard"),
    //     new FitnessActivity(new DateTime(2026, 10, 31, 8, 0, 0), "Swimming", 10, "Bondi Beach"),
    // };

    //     var results = _repo.SearchEvent(new DateTime(2026, 10, 30), filter, data);

    //     Assert.Equal(expectedTitle, Assert.Single(results).Title);
    // }

    [Theory]
    [InlineData("2025-03-01", SearchFilter.on, new string[] { "Walking" })]
    [InlineData("2025-03-08", SearchFilter.before, new string[] { "Walking", "Running" })]
    [InlineData("2026-12-30", SearchFilter.after, new string[] { "Test Entertainment Event" })]
    public void SearchEvent_ReturnsEventsMatchingFilter(string dateTime, SearchFilter filter, string[] expected)
    {
        var searchDate = DateTime.Parse(dateTime);
        var results = _repo.SearchEvent(searchDate, filter);

        Assert.All(expected, exTitle => Assert.Contains(results, item => item.Title == exTitle));

    }

    // Positive test
    [Fact]
    public void ReadEntertainmentEvents_ShouldReturnResults()
    {
        var results = _repo.ReadActivitiesbyType(EventType.entertainment);
        Assert.NotEmpty(results);
        Assert.All(results, item => Assert.IsType<EntertainmentActivity>(item));
    }

    // Negative test
    [Fact]
    public void SearchEvent_NoEventsOnDate_ShouldReturnEmpty()
    {
        var results = _repo.SearchEvent(new DateTime(1900, 1, 1), SearchFilter.on);
        Assert.Empty(results);
    }

    // Boundary test
    [Fact]
    public void CostBelowZero_ShouldThrowsArgumentException()
    {
        var date = new DateTime(2027, 1, 1, 7, 0, 0);
        Assert.Throws<ArgumentException>(() => new FitnessActivity(date, "Boundary Test", -0.01m, "Park"));
    }

}

