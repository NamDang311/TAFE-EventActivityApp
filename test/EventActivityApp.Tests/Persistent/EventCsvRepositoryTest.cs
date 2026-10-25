using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Xunit.Abstractions;

using EventActivityApp.Persistent;
using EventActivityApp.Domain;
namespace EventActivityApp.Persistent;


public class EventCsvRepositoryTest
{
    private readonly ITestOutputHelper _output;

    private readonly string baseDir = AppDomain.CurrentDomain.BaseDirectory;
    private readonly EventCsvRepository _repo;
    private readonly string _fitnessFilePath;
    private readonly string _entertainmentFilePath;

    public EventCsvRepositoryTest(ITestOutputHelper output)
    {
        _output = output;
        _repo = new EventCsvRepository
        {
            DataFolder = Path.Combine(
                baseDir, "..", "..", "..", "Data")
        };
        _fitnessFilePath = Path.Combine(_repo.DataFolder, "FitnessActivities.CSV");
        _entertainmentFilePath = Path.Combine(_repo.DataFolder, "EntertainmentActivities.CSV");
    }

    [Fact]
    public void CheckFileExist()
    {

    }

    [Fact]
    public void CanSaveFitnessEvent_ShouldWriteToFile()
    {

        var event1 = new EventActivityApp.Domain.EntertainmentEvent("27/10/2026", "Swimming", 12, 1);
        var event2 = new EventActivityApp.Domain.EntertainmentEvent("28/10/2026", "Skating", 5, 10);
        _repo.SetEvent("entertainment");
        _repo.WriteCsv(event1);
        _repo.WriteCsv(event2);


    }

}
