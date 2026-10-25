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
    private readonly ITestOutputHelper _output;

    private readonly string baseDir = AppDomain.CurrentDomain.BaseDirectory;
    private readonly EventCsvRepository _repo;
    private readonly string _fitnessTestFilePath;
    private readonly string _entertainmentTestFilePath;

    public EventCsvRepositoryTest(ITestOutputHelper output)
    {
        _output = output;
        _repo = new EventCsvRepository
        {
            DataFolder = Path.Combine(
                baseDir, "..", "..", "..", "Data")
        };
        _fitnessTestFilePath = Path.Combine(_repo.DataFolder, "FitnessActivities.CSV");
        _entertainmentTestFilePath = Path.Combine(_repo.DataFolder, "EntertainmentActivities.CSV");
    }

    [Fact]
    public void FitnessProdFile_ShouldExist()
    {
        var filePath = Path.Combine(baseDir, "..", "..", "..", "..", "..", "src", "EventActivityApp", "Data", "FitnessActivities.CSV");
        Assert.True(File.Exists(filePath));

    }

    [Fact]
    public void EntertainmentProdFile_ShouldExist()
    {

        var filePath = Path.Combine(baseDir, "..", "..", "..", "..", "..", "src", "EventActivityApp", "Data", "EntertainmentActivities.CSV");
        Assert.True(File.Exists(filePath));

    }

    // NOTE: The belows will point to Mock Data folder.
    [Fact]
    public void FilesMissing_ThrowsFileNotFound()
    {
        string fitnessContent = File.ReadAllText(_fitnessTestFilePath);
        try
        {

            File.Delete(_fitnessTestFilePath);
            Assert.Throws<FileNotFoundException>(() => _repo.ReadCsv());
        }
        finally
        {
            File.WriteAllText(_fitnessTestFilePath, fitnessContent);
        }

    }

    [Fact]
    public void CanSaveFitnessEvent_ShouldWriteToFile()
    {

        string original = File.ReadAllText(_fitnessTestFilePath);
        var event1 = new EventActivityApp.Domain.FitnessEvent("28/10/2026", "Swimming", 3, "Liverpool Center");
        _repo.SetEvent("fitness");
        string[] lastLine;

        try
        {

            _repo.WriteCsv(event1);
            lastLine = File.ReadAllLines(_fitnessTestFilePath);
            Assert.Equal(
                "28/10/2026,Swimming,3,Liverpool Center", lastLine[^1]);

        }

        finally
        {
            File.WriteAllText(_fitnessTestFilePath, original);
        }
    }

    [Fact]
    public void CanSaveEntertainmentEvent_ShouldWriteToFile()
    {

        string original = File.ReadAllText(_entertainmentTestFilePath);
        var event1 = new EventActivityApp.Domain.EntertainmentEvent("28/10/2026", "Chess", 3, 1);
        _repo.SetEvent("entertainment");
        string[] lastLine;

        try
        {
            _repo.WriteCsv(event1);
            lastLine = File.ReadAllLines(_entertainmentTestFilePath);
            Assert.Equal(
                "28/10/2026,Chess,3,1", lastLine[^1]);

        }

        finally
        {
            File.WriteAllText(_entertainmentTestFilePath, original);
        }
    }
}


