Title: Scenarios
---

<?! PackageInfo "Shiny.Push.FirebaseMessaging" /?>

### Rich Notifications

```csharp
services.UseNotification<MyNotificationDelegate>();
services.UsePushWhatever<MyPushDelegate>();
services.AddSingleton<ILocalize, YourLocalizeComponent>();


// your push delegate
public class MyPushDelegate : IPushDelegate 
{
   readonly INotificationManager notifications;
   readonly ILocalize localize; 

   public MyPushDelegate(INotificationManager notifications, ILocalize localize) {
      this.notifications = notifications;
      this.localize = localize;
   }

   public async Task OnReceived(IReadOnlyDictionary<string, string> data) 
   {
      if (data.ContainKeys("Event")) 
     {
        await this.notifications.Send(this.localize["MyKey"]); // add things like channel here if necessary
     }
   }
}


// your notification delegate
public class MyNotificationDelegate : INotificationDelegate
{
   public async Task OnEntry(NotificationResponse response) 
   {
      // do what you want with the shiny notification here
   }
}
```