using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Java.Lang;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient;
using Exception = Java.Lang.Exception;

namespace WoWonder.Activities.NativePost.Services
{
    [Service]
    public class ScheduledApiService : Service
    {
        private static Handler MainHandler = new Handler(Looper.MainLooper);
        private PostUpdaterHelper PostUpdater;
         
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnCreate()
        {
            try
            {
                base.OnCreate();
                PostUpdater = new PostUpdaterHelper(new Handler(Looper.MainLooper));

                MainHandler ??= new Handler(Looper.MainLooper);
                MainHandler?.PostDelayed(PostUpdater, AppSettings.RefreshPostSeconds);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            base.OnStartCommand(intent, flags, startId);
            try
            {
                MainHandler?.PostDelayed(new PostUpdaterHelper(new Handler(Looper.MainLooper)), AppSettings.RefreshPostSeconds);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }

            //MainHandler.PostDelayed(new PostUpdaterHelper(Application.Context, new Handler(Looper.MainLooper), ResultSender), AppSettings.RefreshChatActivitiesSeconds);
             
            return StartCommandResult.Sticky;
        }
    }

    public class PostUpdaterHelper : Java.Lang.Object, IRunnable
    {
        private static Handler MainHandler;

        public PostUpdaterHelper(Handler mainHandler)
        {
            MainHandler = mainHandler;
        }

        public void Run()
        {
            //Toast.MakeText(Application.Context, "AppState " + Methods.AppLifecycleObserver.AppState, ToastLength.Short).Show();

            if (string.IsNullOrEmpty(Methods.AppLifecycleObserver.AppState))
                Methods.AppLifecycleObserver.AppState = "Background";

            if (Methods.AppLifecycleObserver.AppState == "Background")
            {
                try
                {
                    if (string.IsNullOrEmpty(Client.WebsiteUrl))
                    {
                        Client a = new Client(AppSettings.TripleDesAppServiceProvider);
                        Console.WriteLine(a);
                    }
                        
                    SqLiteDatabase dbDatabase = new SqLiteDatabase();

                    if (string.IsNullOrEmpty(Current.AccessToken))
                    {
                        var login = dbDatabase.Get_data_Login_Credentials();
                        Console.WriteLine(login);

                        if (string.IsNullOrEmpty(Current.AccessToken))
                            return;
                    }

                    if (Methods.CheckConnectivity())
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiPostAsync.FetchFirstNewsFeedApiPosts });
                     
                    //Toast.MakeText(Application.Context, "ResultSender wael", ToastLength.Short).Show();
                    MainHandler?.PostDelayed(new PostUpdaterHelper(new Handler(Looper.MainLooper)), AppSettings.RefreshPostSeconds);
                }
                catch (Exception e)
                {
                    //Toast.MakeText(Application.Context, "ResultSender failed", ToastLength.Short).Show();
                    MainHandler?.PostDelayed(new PostUpdaterHelper(new Handler(Looper.MainLooper)), AppSettings.RefreshPostSeconds);
                    Methods.DisplayReportResultTrack(e);
                }
            }

        }
    }
     
}