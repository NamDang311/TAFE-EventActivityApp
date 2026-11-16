using System;
using System.Collections.Generic;
using System.Data;
using EventActivityApp.Data;
using EventActivityApp.Domain;
using MySqlConnector;
using Xunit.Abstractions;

namespace EventActivityApp.Persistent;


public class DbRepositoryTest
{

    private readonly EventDbRepository _repo = new EventDbRepository();
    private readonly Db db = new Db();
    private readonly ITestOutputHelper _output;

    public DbRepositoryTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void CanConnectToDatabase()
    {
        using var connection = db.OpenConnection();
        Assert.Equal(ConnectionState.Open, connection.State);
        Assert.Equal("ActivitiesDb", connection.Database);
    }



    [Fact]
    private void CanReadFitnessEvents_ShouldReturnList()
    {
        var results = _repo.ReadActivitiesbyType(EventType.fitness);
        Assert.NotEmpty(results);
        Assert.IsType<List<EventDetail>>(results);
    }


    [Fact]
    private void CanReadEntertainmentEvents_ShouldReturnList()
    {
        var results = _repo.ReadActivitiesbyType(EventType.fitness);
        Assert.NotEmpty(results);
        Assert.IsType<List<EventDetail>>(results);
    }




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


    [Theory]
    [InlineData("2025-03-01", SearchFilter.on, new string[] { "Walking" })]
    [InlineData("2025-03-08", SearchFilter.before, new string[] { "Walking", "Running" })]
    [InlineData("2026-11-09", SearchFilter.after, new string[] { "Running" })]
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

