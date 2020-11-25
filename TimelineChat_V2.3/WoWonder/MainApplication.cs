using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Ads;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Lifecycle;
using DT.Xamarin.Agora;
using Firebase;
using Java.Lang;
using Newtonsoft.Json;
using Plugin.CurrentActivity;
using WoWonder.Activities;
using WoWonder.Activities.Communities.Groups;
using WoWonder.Activities.Communities.Pages;
using WoWonder.Activities.Live.Rtc;
using WoWonder.Activities.Live.Stats;
using WoWonder.Activities.Live.Utils;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.NativePost.Services;
using WoWonder.Activities.SettingsPreferences;
using WoWonder.Activities.UserProfile;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.OneSignal;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using Xamarin.Android.Net;
using Exception = System.Exception;
using Console = System.Console;
using Constants = WoWonder.Activities.Live.Page.Constants;

namespace WoWonder
{
    //You can specify additional application information in this attribute
    [Application(UsesCleartextTraffic = true)]
    public class MainApplication : Application, Application.IActivityLifecycleCallbacks
    {
        private static MainApplication Instance;
        public Activity Activity; 
        public MainApplication(IntPtr handle, JniHandleOwnership transer) : base(handle, transer)
        {
           
        }

        public override void OnCreate()
        {
            try
            {
                base.OnCreate();
                //A great place to initialize Xamarin.Insights and Dependency Services!
                RegisterActivityLifecycleCallbacks(this);

                Instance = this;

                Client a = new Client(AppSettings.TripleDesAppServiceProvider);
                Console.WriteLine(a);

                var sqLiteDatabase = new SqLiteDatabase();
                sqLiteDatabase.CheckTablesStatus();
                sqLiteDatabase.Get_data_Login_Credentials();
                 
                new Handler(Looper.MainLooper).PostDelayed(new Runnable(FirstRunExcite), 100);

                StartService(new Intent(this, typeof(ScheduledApiService))); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                Methods.DialogPopup.InvokeAndShowDialog(Activity, "ReportMode", exception.Message, "Close");
            }
        }

        private void FirstRunExcite()
        {
            try
            {
                if (AppSettings.ShowAdMobBanner || AppSettings.ShowAdMobInterstitial || AppSettings.ShowAdMobRewardVideo || AppSettings.ShowAdMobNative || AppSettings.ShowAdMobNativePost)
                    MobileAds.Initialize(this, GetString(Resource.String.admob_app_id));

                if (AppSettings.ShowFbBannerAds || AppSettings.ShowFbInterstitialAds || AppSettings.ShowFbRewardVideoAds)
                    InitializeFacebook.Initialize(this);

                InitRtcEngine();
                InitConfig();

                //Bypass Web Errors 
                //======================================
                if (AppSettings.TurnSecurityProtocolType3072On)
                {
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                    var client = new HttpClient(new AndroidClientHandler());
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls13;
                    Console.WriteLine(client);
                }
                 
                if (AppSettings.TurnTrustFailureOnWebException)
                {
                    //If you are Getting this error >>> System.Net.WebException: Error: TrustFailure /// then Set it to true
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    var b = new AesCryptoServiceProvider();
                    Console.WriteLine(b);
                }

                //OneSignal Notification  
                //======================================
                OneSignalNotification.RegisterNotificationDevice();
                 
                //Init Settings
                MainSettings.Init();

                ClassMapper.SetMappers();

                //App restarted after crash
                AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironmentOnUnhandledExceptionRaiser;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
                TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

                AppCompatDelegate.CompatVectorFromResourcesEnabled = true;
                FirebaseApp.InitializeApp(this);

                Methods.AppLifecycleObserver appLifecycleObserver = new Methods.AppLifecycleObserver();
                ProcessLifecycleOwner.Get().Lifecycle.AddObserver(appLifecycleObserver);

                var sqLiteDatabase = new SqLiteDatabase();
                if (AppSettings.LastChatSystem == SystemApiGetLastChat.New)
                    ListUtils.UserList = sqLiteDatabase.Get_LastUsersChat_List();
                else
                    ListUtils.UserChatList = sqLiteDatabase.GetLastUsersChatList();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                Toast.MakeText(this, exception.Message, ToastLength.Short)?.Show();
            }
        }
         
        private void AndroidEnvironmentOnUnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            try
            {
                Intent intent = new Intent(Activity, typeof(SplashScreenActivity));
                intent.AddCategory(Intent.CategoryHome);
                intent.PutExtra("crash", true);
                intent.SetAction(Intent.ActionMain);
                intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);

                PendingIntent pendingIntent = PendingIntent.GetActivity(GetInstance().BaseContext, 0, intent, PendingIntentFlags.OneShot);
                AlarmManager mgr = (AlarmManager)GetInstance()?.BaseContext?.GetSystemService(AlarmService);
                mgr?.Set(AlarmType.Rtc, JavaSystem.CurrentTimeMillis() + 100, pendingIntent);

                Activity.Finish();
                JavaSystem.Exit(2);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                //var message = e.Exception.Message;
                var stackTrace = e.Exception.StackTrace;

                Methods.DisplayReportResult(Activity, stackTrace);
                Console.WriteLine(e.Exception);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                //var message = e;
                Methods.DisplayReportResult(Activity, e);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public static MainApplication GetInstance()
        {
            return Instance;
        }



        #region Agora

        private RtcEngine MRtcEngine;
        private EngineConfig MGlobalConfig = new EngineConfig();
        private AgoraEventHandler MHandler = new AgoraEventHandler();
        private StatsManager MStatsManager = new StatsManager();

        private void InitConfig()
        {
            try
            {
                ISharedPreferences pref = PrefManager.GetPreferences(ApplicationContext);
                MGlobalConfig.SetVideoDimenIndex(pref.GetInt(Constants.PrefResolutionIdx, Constants.DefaultProfileIdx));

                bool showStats = pref.GetBoolean(Constants.PrefEnableStats, false);
                MGlobalConfig.SetIfShowVideoStats(showStats);
                MStatsManager.EnableStats(showStats);

                MGlobalConfig.SetMirrorLocalIndex(pref.GetInt(Constants.PrefMirrorLocal, 0));
                MGlobalConfig.SetMirrorRemoteIndex(pref.GetInt(Constants.PrefMirrorRemote, 0));
                MGlobalConfig.SetMirrorEncodeIndex(pref.GetInt(Constants.PrefMirrorEncode, 0));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitRtcEngine()
        {
            try
            {
                MRtcEngine = DT.Xamarin.Agora.RtcEngine.Create(ApplicationContext, AppSettings.AppIdAgoraLive, MHandler);
                // Sets the channel profile of the Agora RtcEngine.
                // The Agora RtcEngine differentiates channel profiles and applies different optimization algorithms accordingly. For example, it prioritizes smoothness and low latency for a video call, and prioritizes video quality for a video broadcast.
                MRtcEngine.SetChannelProfile(DT.Xamarin.Agora.Constants.ChannelProfileLiveBroadcasting);
                MRtcEngine.EnableVideo();
                MRtcEngine.SetLogFile(FileUtil.InitializeLogFile(this));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public EngineConfig EngineConfig()
        {
            return MGlobalConfig;
        }

        public RtcEngine RtcEngine()
        {
            return MRtcEngine;
        }

        public StatsManager StatsManager()
        {
            return MStatsManager;
        }

        public void RegisterEventHandler(IEventHandler handler)
        {
            MHandler.AddHandler(handler);
        }

        public void RemoveEventHandler(IEventHandler handler)
        {
            MHandler.RemoveHandler(handler);
        }

        #endregion


        public override void OnTerminate() // on stop
        {
            try
            {
                base.OnTerminate();
                UnregisterActivityLifecycleCallbacks(this);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            try
            {
                Activity = activity;
                CrossCurrentActivity.Current.Activity = activity;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnActivityDestroyed(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityPaused(Activity activity)
        {
            Activity = activity;
        }

        public void OnActivityResumed(Activity activity)
        {
            try
            {
                Activity = activity;
                CrossCurrentActivity.Current.Activity = activity;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {
            Activity = activity;
        }

        public void OnActivityStarted(Activity activity)
        {
            try
            {
                Activity = activity;
                CrossCurrentActivity.Current.Activity = activity;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnActivityStopped(Activity activity)
        {
            Activity = activity;
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            { 
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void NavigateTo(Activity fromContext, Type toContext, dynamic passData)
        {
            try
            {
                var intent = new Intent(this, toContext);

                if (passData != null)
                {
                    if (toContext == typeof(GroupProfileActivity))
                    {
                        if (passData is GroupClass groupClass)
                        { 
                            intent.PutExtra("GroupObject", JsonConvert.SerializeObject(groupClass));
                            intent.PutExtra("GroupId", groupClass.GroupId);
                        }
                    }
                    else if (toContext == typeof(PageProfileActivity))
                    {
                        if (passData is PageClass pageClass)
                        {
                            intent.PutExtra("PageObject", JsonConvert.SerializeObject(pageClass));
                            intent.PutExtra("PageId", pageClass.PageId);
                        } 
                    }
                    else if (toContext == typeof(YoutubePlayerActivity))
                    {
                        if (passData is PostDataObject postData)
                        {
                            intent.PutExtra("PostObject", JsonConvert.SerializeObject(postData));
                            intent.PutExtra("PostId", postData.PostId);
                        }
                         
                        fromContext.StartActivity(intent);
                        ((AppCompatActivity)fromContext).OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_fade_out);
                        return;
                    }
                    else if (toContext == typeof(UserProfileActivity))
                    {
                        if (passData is UserDataObject userDataObject)
                        {
                            intent.PutExtra("UserObject", JsonConvert.SerializeObject(userDataObject));
                            intent.PutExtra("UserId", userDataObject.UserId);
                        }
                       
                        ((AppCompatActivity)fromContext).OverridePendingTransition(Resource.Animation.abc_popup_enter, Resource.Animation.popup_exit);
                        fromContext.StartActivity(intent);
                        return;
                    }

                    fromContext.StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}