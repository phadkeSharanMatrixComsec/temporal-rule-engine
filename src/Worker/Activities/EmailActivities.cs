using System;
using Temporalio.Activities;
using Worker.Models;

namespace Worker.Activities;

public class EmailActivities
{
    public EmailActivities()
    {

    }

    // [Activity]
    // public bool SendEmailActivity(EventModel emailEvent)
    // {
    //     Console.WriteLine("Email Sent " + emailEvent.EventData);
    //     return true;
    // }
    
    [Activity]
    public bool SendEmailActivity(EventModel emailEvent)
    {
        Console.WriteLine("Attempting to send email: " + emailEvent.EventData);
        throw new Exception("Failed to send email.");
    }

}
