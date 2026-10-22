using System;
using System.Collections.Generic;
using System.Text;


using EventActivityApp.Domain;
namespace EventActivityApp.Domain;

internal class EventDetailTest
{
    public void Cost_Negative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            new EventDetail("2015-23-12", "Hiking", -1m, "Sydney Park");
        });
}
}