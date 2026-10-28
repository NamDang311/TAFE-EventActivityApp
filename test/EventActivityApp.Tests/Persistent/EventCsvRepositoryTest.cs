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
    private void FitnessMockFile_ShouldExist()
    {
        string path = Path.Combine(_repo.DataFolder, "FitnessActivities.CSV");
        Assert.True(File.Exists(path));
    }

    [Fact]
    private void EntertainmentMockFile_ShouldExist()
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
    private void CanReadFitnessEvents_ShouldReturnList()
    {
        var list = _repo.ReadCsv(EventType.fitness);
        Assert.Equal("FitnessActivities.CSV", Path.GetFileName(_repo._filePath));
        Assert.NotEmpty(list);
        Assert.IsType<List<EventDetail>>(list);

    }

    [Fact]
    private void CanReadEntertainmentEvents_ShouldReturnList()
    {

        var list = _repo.ReadCsv(EventType.entertainment);
        Assert.Equal("EntertainmentActivities.CSV", Path.GetFileName(_repo._filePath));
        Assert.NotEmpty(list);
        Assert.IsType<List<EventDetail>>(list);
    }

    [Fact]
    private void CanSaveFitnessEvent_ShouldWriteToFile()
    {
        string path = Path.Combine(_repo.DataFolder, "FitnessActivities.CSV");
        string original = File.ReadAllText(path);
        var event1 = new EventActivityApp.Domain.FitnessEvent("28/10/2026", "Swimming", 3, "Liverpool Center");
        string[] lastLine;

        try
        {

            _repo.WriteCsv(event1, EventType.fitness);
            lastLine = File.ReadAllLines(path);
            Assert.Equal(
                "28/10/2026,Swimming,3,Liverpool Center", lastLine[^1]);

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
        var event1 = new EventActivityApp.Domain.EntertainmentEvent("28/10/2026", "Chess", 3, 1);
        string[] lastLine;

        try
        {
            _repo.WriteCsv(event1, EventType.entertainment);
            lastLine = File.ReadAllLines(path);
            Assert.Equal(
                "28/10/2026,Chess,3,1", lastLine[^1]);

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
        new FitnessEvent("29/10/2026 9:00", "Running", 0, "Sydney Park"),
        new FitnessEvent("30/10/2026 6:23", "Yoga", 12, "Mt Pritchard"),
        new FitnessEvent("31/10/2026 8:00", "Swimming", 10, "Bondi Beach"),
    };

        var results = _repo.SearchEvent(new DateTime(2026, 10, 30), filter, data);

        Assert.Equal(expectedTitle, Assert.Single(results).Title);
    }
}


