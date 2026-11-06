using System;
using System.Collections.Generic;
using System.IO;
using EventActivityApp.Data;
using EventActivityApp.Domain;
using Xunit.Abstractions;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using MySqlConnector;
namespace EventActivityApp.Persistent;


public class EventCsvRepositoryTest
{
    // private readonly string baseDir = AppDomain.CurrentDomain.BaseDirectory;
    private readonly EventCsvRepository _repo = new EventCsvRepository();
    private readonly Db db = new Db();
    private readonly ITestOutputHelper _output;

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
    public EventCsvRepositoryTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void CanConnectToDatabase()
    {
        using var connection = db.OpenConnection();
        Assert.Equal(System.Data.ConnectionState.Open, connection.State);
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
        var results = _repo.ReadActivities(EventType.fitness);
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
        var results = _repo.ReadActivities(EventType.fitness);
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

        var event1 = new EventActivityApp.Domain.FitnessActivity(new DateTime(2026, 11, 6, 7, 00, 00), "Swimming", 3, "Liverpool Center");
        try
        {
            _repo.WriteEvent(event1);
        }
        catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
        {
            _output.WriteLine($"Already exists: {ex.Message}");
        }
        var results = _repo.ReadActivities(EventType.fitness);
        Assert.Contains(results, item => item.Title == event1.Title && item.DateStartTime == event1.DateStartTime);
    }


    [Fact]
    private void UpdateFitnessEvent_ShouldWriteDb()
    {

        var event1 = new EventActivityApp.Domain.FitnessActivity(new DateTime(2026, 11, 7, 7, 00, 00), "Swimming", 3, "Liverpool Center");
        var results = _repo.SearchEvent(new DateTime(2026, 11, 7, 7, 00, 00), SearchFilter.on);
        try
        {
            _repo.WriteEvent(event1);
        }
        catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
        {
            _output.WriteLine($"Already exists: {ex.Message}");
            _repo.UpdateEvent(event1);
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

        var event1 = new EventActivityApp.Domain.EntertainmentActivity(new DateTime(2026, 11, 20, 7, 00, 00), "Test Event", 12, 4);
        try
        {
            _repo.WriteEvent(event1);
            var results = _repo.ReadActivities(EventType.entertainment);
            Assert.Contains(results, item => item.Title == event1.Title && item.DateStartTime == event1.DateStartTime);
        }
        catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
        {
            _output.WriteLine($"Already exists: {ex.Message}");
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
    [InlineData("2026-11-07", SearchFilter.on, new string[] { "Swimming" })]
    [InlineData("2025-03-08", SearchFilter.before, new string[] { "Walking", "Running" })]
    [InlineData("2026-11-06", SearchFilter.after, new string[] { "Karaoke" })]
    public void SearchEvent_ReturnsEventsMatchingFilter(string dateTime, SearchFilter filter, string[] expected)
    {
        var searchDate = DateTime.Parse(dateTime);
        var results = _repo.SearchEvent(searchDate, filter);

        Assert.All(expected, exTitle => Assert.Contains(results, item => item.Title == exTitle));

    }
}

