using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Android.Gms.Extensions;
using Android.Runtime;
using Firebase;
using Firebase.Messaging;
using Shiny.Push.Infrastructure;


namespace Shiny.Push
{
    public class NativeAdapter : INativeAdapter
    {
        readonly IAndroidContext context;
        readonly FirebaseConfig? config;


        public NativeAdapter(IAndroidContext context, FirebaseConfig? config = null)
        {
            this.context = context;
            this.config = config;
        }


        Func<IReadOnlyDictionary<string, string>, Task>? onReceived;
        public Func<IReadOnlyDictionary<string, string>, Task>? OnReceived
        {
            get => this.onReceived;
            set
            {
                this.onReceived = value;
                if (this.onReceived == null)
                {
                    ShinyFirebaseService.MessageReceived = null;
                }
                else
                {
                    ShinyFirebaseService.MessageReceived = async msg =>
                    {
                        var dict = msg.Data.ToDictionary(x => x.Key, x => x.Value);
                        var data = (IReadOnlyDictionary<string, string>)dict;
                        await this.onReceived.Invoke(data).ConfigureAwait(false);
                    };
                }
            }
        }


        Func<string, Task>? onToken;
        public Func<string, Task>? OnTokenRefreshed
        {
            get => this.onToken;
            set
            {
                this.onToken = value;
                if (this.onToken == null)
                {
                    ShinyFirebaseService.NewToken = null;
                }
                else
                {
                    ShinyFirebaseService.NewToken = async token => await this.onToken.Invoke(token).ConfigureAwait(false);
                }
            }
        }


        public async Task<PushAccessState> RequestAccess()
        {
            if (this.config == null)
            {
                FirebaseMessaging.Instance.AutoInitEnabled = true;
            }
            else
            {
                var options = new FirebaseOptions.Builder()
                    .SetApplicationId(this.config.AppId)
                    //.SetProjectId(this.config.ProjectId)
                    .SetApiKey(this.config.ApiKey)
                    .SetGcmSenderId(this.config.SenderId)
                    .Build();
                FirebaseApp.InitializeApp(this.context.AppContext, options);
            }

            var task = await FirebaseMessaging.Instance.GetToken();
            var token = task.JavaCast<Java.Lang.String>().ToString();
            return new PushAccessState(AccessState.Available, token);
        }


        public async Task UnRegister()
        {
            FirebaseMessaging.Instance.AutoInitEnabled = false;
            await Task.Run(() => FirebaseMessaging.Instance.DeleteToken()).ConfigureAwait(false);
        }
    }
}
