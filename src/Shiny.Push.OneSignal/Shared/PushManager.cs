#if !NETSTANDARD
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Com.OneSignal.Abstractions;
using OS = global::Com.OneSignal.OneSignal;


namespace Shiny.Push.OneSignal
{
    public class PushManager : IPushManager,
                               IPushPropertySupport,
                               IShinyStartupTask
    {
        readonly OneSignalPushConfig config;
        readonly PushContainer container;


        public PushManager(OneSignalPushConfig config, PushContainer container)
        {
            this.config = config;
            this.container = container;
        }


        public void Start()
        {
            OS.Current.SetLogLevel(this.config.LogLevel, this.config.VisualLogLevel);
            OS.Current
                .StartInit(this.config.AppId)
                .HandleNotificationReceived(async notification =>
                {
                    var dict = notification?
                        .payload
                        .additionalData?
                        .ToDictionary(
                            y => y.Key,
                            y => y.Value.ToString()
                        ) ?? new Dictionary<string, string>(0);

                    var data = (IReadOnlyDictionary<string, string>)dict;
                    await this.container
                        .OnReceived(data)
                        .ConfigureAwait(false);
                })
                .Settings(new Dictionary<string, bool>
                {
                    { IOSSettings.kOSSettingsKeyAutoPrompt, config.iOSAutoPrompt },
                    { IOSSettings.kOSSettingsKeyInAppLaunchURL, config.iOSInAppLaunchURL }
                })
                .InFocusDisplaying(this.config.InFocusDisplay)
                .EndInit();
        }


        public DateTime? CurrentRegistrationTokenDate => this.container.CurrentRegistrationTokenDate;
        public string? CurrentRegistrationToken => this.container.CurrentRegistrationToken;


        public async Task<PushAccessState> RequestAccess(CancellationToken cancelToken = default)
        {
            OS.Current.SetSubscription(true);
            OS.Current.RegisterForPushNotifications();
            var ids = await OS.Current.IdsAvailableAsync();
            this.container.SetCurrentToken(ids.PushToken, false);

            return new PushAccessState(AccessState.Available, ids.PushToken);
        }


        public Task UnRegister()
        {
            this.container.ClearRegistration();
            this.ClearProperties();
            OS.Current.SetSubscription(false);
            return Task.CompletedTask;
        }


        public IObservable<IReadOnlyDictionary<string, string>> WhenReceived() => this.container.WhenReceived();
        public IReadOnlyDictionary<string, string> CurrentProperties => this.Properties;


        public void ClearProperties()
        {
            OS.Current.RemoveExternalUserId();
            OS.Current.SetLocationShared(false);
            OS.Current.DeleteTags(this.CurrentProperties.Keys.ToList());
            this.Properties.Clear();
            this.WriteProps();
        }


        public void RemoveProperty(string property)
        {
            switch (property.ToLower())
            {
                case "userid":
                    OS.Current.RemoveExternalUserId();
                    break;

                case "location":
                    OS.Current.SetLocationShared(false);
                    break;

                default:
                    OS.Current.DeleteTag(property);
                    break;
            }
            this.Properties.Remove(property);
            this.WriteProps();
        }


        public void SetProperty(string property, string value)
        {
            switch (property.ToLower())
            {
                case "userid":
                    OS.Current.SetExternalUserId(value);
                    break;

                case "location":
                    OS.Current.SetLocationShared(value.Equals("true", StringComparison.CurrentCultureIgnoreCase));
                    break;

                default:
                    OS.Current.SendTag(property, value);
                    break;
            }
            this.Properties[property] = value;
            this.WriteProps();
        }


        Dictionary<string, string>? props;
        Dictionary<string, string> Properties
        {
            get
            {
                this.props ??= this.container
                    .Store
                    .Get<Dictionary<string, string>>(nameof(this.Properties))
                    ?? new Dictionary<string, string>(0);
                return props;
            }
        }


        void WriteProps() => this.container.Store.Set(
            nameof(this.Properties),
            this.Properties
        );
    }
}
#endif