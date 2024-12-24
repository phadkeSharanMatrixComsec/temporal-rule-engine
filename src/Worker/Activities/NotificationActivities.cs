using System;
using Temporalio.Activities;
using Worker.Models;

namespace Worker.Activities;

public class NotificationActivities
{
    public NotificationActivities()
    {
        
    }
    
    [Activity]
    public bool SendNotificationActivity(EventModel notificationModel)
    {
        Console.WriteLine("Notification Sent " + notificationModel.EventData);
        return true;
    }
}
