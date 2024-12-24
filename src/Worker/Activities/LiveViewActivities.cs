using System;
using Temporalio.Activities;
using Worker.Models;

namespace Worker.Activities;

public class LiveViewActivities
{
    public LiveViewActivities()
    {
        
    }

    [Activity]
    public bool StartLiveViewActivity(EventModel liveViewModel)
    {
        Console.WriteLine($"Live View Started {liveViewModel}");
        return true;
    }
}
