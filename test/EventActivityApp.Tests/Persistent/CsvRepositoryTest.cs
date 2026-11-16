using System;
using System.Collections.Generic;
using System.IO;
using EventActivityApp.Domain;
namespace EventActivityApp.Persistent;


public class EventCsvRepositoryTest
{
    private readonly string baseDir = AppDomain.CurrentDomain.BaseDirectory;
    private readonly EventCsvRepository _repo;

    public EventCsvRepositoryTest()
    {
        _repo = new EventCsvRepository
        {
            DataFolder = Path.Combine(
                baseDir, "..", "..", "..", "Data")
        };
    }

    [Fact]
    public void FitnessMockFile_ShouldExist()
    {
        string path = Path.Combine(_repo.DataFolder, "FitnessActivities.CSV");
        Assert.True(File.Exists(path));
    }

    [Fact]
    public void EntertainmentMockFile_ShouldExist()
    {
        string path = Path.Combine(_repo.DataFolder, "EntertainmentActivities.CSV");
        Assert.True(File.Exists(path));

    }

    [Fact]
    public void FilesMissing_ThrowsFileNotFound()
    {
        string path = Path.Combine(_repo.DataFolder, "FitnessActivities.CSV");
        string fitnessContent = File.ReadAllText(path);
        try
        {
            File.Delete(path);
            Assert.Throws<FileNotFoundException>(() => _repo.ReadCsv(EventType.fitness));
        }
        finally
        {
            File.WriteAllText(path, fitnessContent);
        }
    }



    [Fact]
    public void CanReadFitnessEvents_ShouldReturnList()
    {
        var list = _repo.ReadCsv(EventType.fitness);
        Assert.Equal("FitnessActivities.CSV", Path.GetFileName(_repo._filePath));
        Assert.NotEmpty(list);
        Assert.IsType<List<EventDetail>>(list);

    }

    [Fact]
    public void CanReadEntertainmentEvents_ShouldReturnList()
    {

        var list = _repo.ReadCsv(EventType.entertainment);
        Assert.Equal("EntertainmentActivities.CSV", Path.GetFileName(_repo._filePath));
        Assert.NotEmpty(list);
        Assert.IsType<List<EventDetail>>(list);
    }

    [Fact]
    public void CanSaveFitnessEvent_ShouldWriteToFile()
    {
        string path = Path.Combine(_repo.DataFolder, "FitnessActivities.CSV");
        string original = File.ReadAllText(path);
        var event1 = new EventActivityApp.Domain.FitnessActivity(new DateTime(2026, 10, 28, 1, 0, 0), "Swimming", 3, "Liverpool Center");
        string[] lastLine;

        try
        {

            _repo.WriteCsv(event1, EventType.fitness);
            lastLine = File.ReadAllLines(path);
            Assert.Equal(
                "28/10/2026 1:00,Swimming,3,Liverpool Center", lastLine[^1]);

        }

        finally
        {
            File.WriteAllText(path, original);
        }
    }

    [Fact]
    public void CanSaveEntertainmentEvent_ShouldWriteToFile()
    {
        string path = Path.Combine(_repo.DataFolder, "EntertainmentActivities.CSV");
        string original = File.ReadAllText(path);
        var event1 = new EventActivityApp.Domain.EntertainmentActivity(new DateTime(2026, 10, 28, 1, 0, 0), "Chess", 3, 2);
        string[] lastLine;

        try
        {
            _repo.WriteCsv(event1, EventType.entertainment);
            lastLine = File.ReadAllLines(path);
            Assert.Equal(
                "28/10/2026 1:00,Chess,3,2", lastLine[^1]);

        }

        finally
        {
            File.WriteAllText(path, original);
        }
    }

    [Theory]
    [InlineData(SearchFilter.before, "Running")]
    [InlineData(SearchFilter.on, "Yoga")]
    [InlineData(SearchFilter.after, "Swimming")]
    public void SearchEvent_ReturnsEventsMatchingFilter(SearchFilter filter, string expectedTitle)
    {
        var data = new List<EventDetail>
    {
        new FitnessActivity(new DateTime(2026, 10, 29, 9, 0, 0), "Running", 0, "Sydney Park"),
        new FitnessActivity(new DateTime(2026, 10, 30, 6, 23, 0), "Yoga", 12, "Mt Pritchard"),
        new FitnessActivity(new DateTime(2026, 10, 31, 8, 0, 0), "Swimming", 10, "Bondi Beach"),
    };

        var results = _repo.SearchEvent(new DateTime(2026, 10, 30), filter, data);

        Assert.Equal(expectedTitle, Assert.Single(results).Title);
    }
}


