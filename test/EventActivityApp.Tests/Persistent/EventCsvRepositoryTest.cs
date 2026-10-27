using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Xunit.Abstractions;

using EventActivityApp.Persistent;
using EventActivityApp.Domain;
using System.Linq;
using Microsoft.VisualBasic;
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
        Assert.True(File.Exists(_repo._filePath));
    }

    [Fact]
    private void EntertainmentMockFile_ShouldExist()
    {
        string path = Path.Combine(_repo.DataFolder, "EntertainmentActivities.CSV");
        Assert.True(File.Exists(_repo._filePath));

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
        _repo.CurrentType = EventType.fitness;
        var list = _repo.ReadCsv();
        Assert.Equal("FitnessActivities.CSV", Path.GetFileName(_repo._filePath));
        Assert.NotEmpty(list);
        Assert.IsType<List<EventDetail>>(list);

    }

    [Fact]
    private void CanReadEntertainmentEvents_ShouldReturnList()
    {
        _repo.CurrentType = EventType.entertainment;
        var list = _repo.ReadCsv();
        Assert.Equal("EntertainmentActivities.CSV", Path.GetFileName(_repo._filePath));
        Assert.NotEmpty(list);
        Assert.IsType<List<EventDetail>>(list);
    }

    [Fact]
    private void CanSaveFitnessEvent_ShouldWriteToFile()
    {
        _repo.CurrentType = EventType.fitness;
        string original = File.ReadAllText(_repo._filePath);
        var event1 = new EventActivityApp.Domain.FitnessEvent("28/10/2026", "Swimming", 3, "Liverpool Center");
        string[] lastLine;

        try
        {

            _repo.WriteCsv(event1);
            lastLine = File.ReadAllLines(_repo._filePath);
            Assert.Equal(
                "28/10/2026,Swimming,3,Liverpool Center", lastLine[^1]);

        }

        finally
        {
            File.WriteAllText(_repo._filePath, original);
        }
    }

    [Fact]
    public void CanSaveEntertainmentEvent_ShouldWriteToFile()
    {
        _repo.CurrentType = EventType.entertainment;
        string original = File.ReadAllText(_repo._filePath);
        var event1 = new EventActivityApp.Domain.EntertainmentEvent("28/10/2026", "Chess", 3, 1);
        string[] lastLine;

        try
        {
            _repo.WriteCsv(event1);
            lastLine = File.ReadAllLines(_repo._filePath);
            Assert.Equal(
                "28/10/2026,Chess,3,1", lastLine[^1]);

        }

        finally
        {
            File.WriteAllText(_repo._filePath, original);
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


