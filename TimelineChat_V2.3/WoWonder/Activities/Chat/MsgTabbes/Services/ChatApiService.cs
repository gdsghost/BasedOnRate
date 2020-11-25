using System;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Java.Lang;
using Newtonsoft.Json;
using WoWonder.Activities.Chat.Call;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderClient.Requests;
using Exception = Java.Lang.Exception;

namespace WoWonder.Activities.Chat.MsgTabbes.Services
{
    [Service]
    public class ChatApiService : JobIntentService
    {
        private static Handler MainHandler;
        private ResultReceiver ResultSender;
        private PostUpdaterHelper PostUpdater;
         
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        protected override void OnHandleWork(Intent p0)
        {
             
        }

        public override void OnCreate()
        {
            try
            {
                base.OnCreate();
                PostUpdater = new PostUpdaterHelper(new Handler(Looper.MainLooper), ResultSender);

                MainHandler ??=new Handler(Looper.MainLooper);
                MainHandler.PostDelayed(PostUpdater, AppSettings.RefreshChatActivitiesSeconds);
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
                var rec = intent.GetParcelableExtra("receiverTag");
                ResultSender = (ResultReceiver)rec;
                if (PostUpdater != null)
                    PostUpdater.ResultSender = ResultSender;
                else
                    MainHandler.PostDelayed(new PostUpdaterHelper(new Handler(Looper.MainLooper), ResultSender), AppSettings.RefreshChatActivitiesSeconds);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }

            //MainHandler.PostDelayed(new PostUpdaterHelper(Application.Context,new Handler(Looper.MainLooper), ResultSender), AppSettings.RefreshChatActivitiesSeconds);
             
            return StartCommandResult.Sticky;
        }
    }

    public class PostUpdaterHelper : Java.Lang.Object, IRunnable
    {
        private static Handler MainHandler;
        public ResultReceiver ResultSender;
        private int SecondPassed;

        public PostUpdaterHelper(Handler mainHandler, ResultReceiver resultSender)
        {
            MainHandler = mainHandler;
            ResultSender = resultSender; 
        }

        public async void Run()
        {
            //Toast.MakeText(Application.Context, "AppState " + Methods.AppLifecycleObserver.AppState,ToastLength.Short)?.Show();

            if (string.IsNullOrEmpty(Methods.AppLifecycleObserver.AppState))
                Methods.AppLifecycleObserver.AppState = "Background";
             
            if (AppSettings.LastChatSystem == SystemApiGetLastChat.New)
            {
                try
                {
                    //Toast.MakeText(Application.Context, "Started",ToastLength.Short)?.Show(); 
                    if (Methods.AppLifecycleObserver.AppState == "Background")
                    {
                        try
                        {
                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                            var login = dbDatabase.Get_data_Login_Credentials();
                            Console.WriteLine(login);

                            if (string.IsNullOrEmpty(Current.AccessToken))
                                return;

                            (int apiStatus, var respond) = await RequestsAsync.Global.GetChatAsync("all");
                            
                            if (apiStatus != 200 || !(respond is LastChatObject result))
                            {
                                // Methods.DisplayReportResult(Activity, respond);
                            }
                            else
                            {
                                //Toast.MakeText(Application.Context, "ResultSender 1 \n" + data,ToastLength.Short)?.Show();

                                #region Call >> Video_Call_User

                                try
                                {
                                    if (AppSettings.EnableAudioVideoCall)
                                    {
                                        if (AppSettings.UseLibrary == SystemCall.Twilio)
                                        {
                                            bool twilioVideoCall = result.VideoCall ?? false;
                                            bool twilioAudioCall = result.AudioCall ?? false;

                                            if (AppSettings.EnableVideoCall)
                                            {
                                                #region Twilio Video call

                                                if (twilioVideoCall && SecondPassed <= 5 && !MsgTabbedMainActivity.RunCall)
                                                {
                                                    var callUser = result.VideoCallUser?.CallUserClass;
                                                    if (callUser != null)
                                                    {
                                                        MsgTabbedMainActivity.RunCall = true;

                                                        var userId = callUser.UserId;
                                                        var avatar = callUser.Avatar;
                                                        var name = callUser.Name;

                                                        var videosData = callUser.Data;
                                                        if (videosData != null)
                                                        {
                                                            var id = videosData.Id; //call_id
                                                            var accessToken = videosData.AccessToken;
                                                            var accessToken2 = videosData.AccessToken2;
                                                            var fromId = videosData.FromId;
                                                            var active = videosData.Active;
                                                            var time = videosData.Called;
                                                            var declined = videosData.Declined;
                                                            var roomName = videosData.RoomName;

                                                            Intent intent = new Intent(Application.Context, typeof(VideoAudioComingCallActivity));
                                                            intent.PutExtra("UserID", userId);
                                                            intent.PutExtra("avatar", avatar);
                                                            intent.PutExtra("name", name);
                                                            intent.PutExtra("access_token", accessToken);
                                                            intent.PutExtra("access_token_2", accessToken2);
                                                            intent.PutExtra("from_id", fromId);
                                                            intent.PutExtra("active", active);
                                                            intent.PutExtra("time", time);
                                                            intent.PutExtra("CallID", id);
                                                            intent.PutExtra("status", declined);
                                                            intent.PutExtra("room_name", roomName);
                                                            intent.PutExtra("declined", declined);
                                                            intent.PutExtra("type", "Twilio_video_call");

                                                            string avatarSplit = avatar.Split('/').Last();
                                                            var getImg = Methods.MultiMedia.GetMediaFrom_Disk(Methods.Path.FolderDiskImage, avatarSplit);
                                                            if (getImg == "File Dont Exists")
                                                                Methods.MultiMedia.DownloadMediaTo_DiskAsync(Methods.Path.FolderDiskImage, avatar);

                                                            if (SecondPassed <= 5)
                                                            {
                                                                MsgTabbedMainActivity.RunCall = false;
                                                                SecondPassed = 0;

                                                                if (!VideoAudioComingCallActivity.IsActive)
                                                                {
                                                                    intent.AddFlags(ActivityFlags.NewTask);
                                                                    Application.Context.StartActivity(intent);
                                                                }
                                                            }
                                                            else
                                                                SecondPassed++;
                                                        }
                                                    }
                                                }
                                                else if (twilioVideoCall == false && twilioAudioCall == false)
                                                {
                                                    if (SecondPassed > 5)
                                                    {
                                                        MsgTabbedMainActivity.RunCall = false;

                                                        SecondPassed = 0;

                                                        if (VideoAudioComingCallActivity.IsActive)
                                                            VideoAudioComingCallActivity.CallActivity?.FinishVideoAudio();
                                                    }
                                                }
                                                else
                                                {
                                                    MsgTabbedMainActivity.RunCall = false;

                                                    if (VideoAudioComingCallActivity.IsActive)
                                                        VideoAudioComingCallActivity.CallActivity?.FinishVideoAudio();
                                                }
                                                 
                                                #endregion
                                            }

                                            if (AppSettings.EnableAudioCall)
                                            {
                                                #region Twilio Audio call

                                                if (twilioAudioCall && !MsgTabbedMainActivity.RunCall)
                                                {
                                                    var callUser = result.AudioCallUser?.CallUserClass;
                                                    if (callUser != null)
                                                    {
                                                        MsgTabbedMainActivity.RunCall = true;

                                                        var userId = callUser.UserId;
                                                        var avatar = callUser.Avatar;
                                                        var name = callUser.Name;

                                                        var videosData = callUser.Data;
                                                        if (videosData != null)
                                                        {
                                                            var id = videosData.Id; //call_id
                                                            var accessToken = videosData.AccessToken;
                                                            var accessToken2 = videosData.AccessToken2;
                                                            var fromId = videosData.FromId;
                                                            var active = videosData.Active;
                                                            var time = videosData.Called;
                                                            var declined = videosData.Declined;
                                                            var roomName = videosData.RoomName;

                                                            Intent intent = new Intent(Application.Context, typeof(VideoAudioComingCallActivity));
                                                            intent.PutExtra("UserID", userId);
                                                            intent.PutExtra("avatar", avatar);
                                                            intent.PutExtra("name", name);
                                                            intent.PutExtra("access_token", accessToken);
                                                            intent.PutExtra("access_token_2", accessToken2);
                                                            intent.PutExtra("from_id", fromId);
                                                            intent.PutExtra("active", active);
                                                            intent.PutExtra("time", time);
                                                            intent.PutExtra("CallID", id);
                                                            intent.PutExtra("status", declined);
                                                            intent.PutExtra("room_name", roomName);
                                                            intent.PutExtra("declined", declined);
                                                            intent.PutExtra("type", "Twilio_audio_call");

                                                            string avatarSplit = avatar.Split('/').Last();
                                                            var getImg =
                                                                Methods.MultiMedia.GetMediaFrom_Disk(Methods.Path.FolderDiskImage,
                                                                    avatarSplit);
                                                            if (getImg == "File Dont Exists")
                                                                Methods.MultiMedia.DownloadMediaTo_DiskAsync(
                                                                    Methods.Path.FolderDiskImage, avatar);

                                                            if (SecondPassed <= 5)
                                                            {
                                                                MsgTabbedMainActivity.RunCall = false;
                                                                SecondPassed = 0;

                                                                if (!VideoAudioComingCallActivity.IsActive)
                                                                {
                                                                    intent.AddFlags(ActivityFlags.NewTask);
                                                                    Application.Context.StartActivity(intent);
                                                                }
                                                            }
                                                            else
                                                                SecondPassed++;
                                                        }
                                                    }
                                                }
                                                else if (twilioAudioCall == false && twilioVideoCall == false)
                                                {
                                                    if (SecondPassed <= 5)
                                                    {
                                                        MsgTabbedMainActivity.RunCall = false;

                                                        if (VideoAudioComingCallActivity.IsActive)
                                                            VideoAudioComingCallActivity.CallActivity?.FinishVideoAudio();
                                                    }
                                                }
                                                else
                                                {
                                                    MsgTabbedMainActivity.RunCall = false;

                                                    if (VideoAudioComingCallActivity.IsActive)
                                                        VideoAudioComingCallActivity.CallActivity?.FinishVideoAudio();
                                                }

                                                #endregion
                                            }
                                        }
                                        else if (AppSettings.UseLibrary == SystemCall.Agora)
                                        {
                                            #region Agora Audio/Video call

                                            var agoraCall = result.AgoraCall ?? false;
                                            if (agoraCall && SecondPassed <= 5 && !MsgTabbedMainActivity.RunCall)
                                            {
                                                var callUser = result.AgoraCallData?.CallUserClass;

                                                if (callUser != null)
                                                {
                                                    MsgTabbedMainActivity.RunCall = true;

                                                    var userId = callUser.UserId;
                                                    var avatar = callUser.Avatar;
                                                    var name = callUser.Name;

                                                    var videosData = callUser.Data;
                                                    if (videosData != null)
                                                    {
                                                        var id = videosData.Id; //call_id
                                                                                //var accessToken = videosData.AccessToken;
                                                                                //var accessToken2 = videosData.AccessToken2;
                                                        var fromId = videosData.FromId;
                                                        //var active = videosData.Active;
                                                        var time = videosData.Called;
                                                        //var declined = videosData.Declined;
                                                        var roomName = videosData.RoomName;
                                                        var type = videosData.Type;
                                                        var status = videosData.Status;

                                                        string avatarSplit = avatar.Split('/').Last();
                                                        var getImg = Methods.MultiMedia.GetMediaFrom_Disk(Methods.Path.FolderDiskImage, avatarSplit);
                                                        if (getImg == "File Dont Exists")
                                                            Methods.MultiMedia.DownloadMediaTo_DiskAsync(Methods.Path.FolderDiskImage, avatar);

                                                        if (type == "video")
                                                        {
                                                            if (AppSettings.EnableVideoCall)
                                                            {
                                                                Intent intent = new Intent(Application.Context, typeof(VideoAudioComingCallActivity));
                                                                intent.PutExtra("UserID", userId);
                                                                intent.PutExtra("avatar", avatar);
                                                                intent.PutExtra("name", name);
                                                                intent.PutExtra("from_id", fromId);
                                                                intent.PutExtra("status", status);
                                                                intent.PutExtra("time", time);
                                                                intent.PutExtra("CallID", id);
                                                                intent.PutExtra("room_name", roomName);
                                                                intent.PutExtra("type", "Agora_video_call_recieve");
                                                                intent.PutExtra("declined", "0");

                                                                if (!VideoAudioComingCallActivity.IsActive)
                                                                {
                                                                    intent.AddFlags(ActivityFlags.NewTask);
                                                                    Application.Context.StartActivity(intent);
                                                                }
                                                            }
                                                        }
                                                        else if (type == "audio")
                                                        {
                                                            if (AppSettings.EnableAudioCall)
                                                            {
                                                                Intent intent = new Intent(Application.Context, typeof(VideoAudioComingCallActivity));
                                                                intent.PutExtra("UserID", userId);
                                                                intent.PutExtra("avatar", avatar);
                                                                intent.PutExtra("name", name);
                                                                intent.PutExtra("from_id", fromId);
                                                                intent.PutExtra("status", status);
                                                                intent.PutExtra("time", time);
                                                                intent.PutExtra("CallID", id);
                                                                intent.PutExtra("room_name", roomName);
                                                                intent.PutExtra("type", "Agora_audio_call_recieve");
                                                                intent.PutExtra("declined", "0");

                                                                if (SecondPassed < 5)
                                                                    SecondPassed++;
                                                                else
                                                                {
                                                                    MsgTabbedMainActivity.RunCall = false;

                                                                    SecondPassed = 0;


                                                                    if (!VideoAudioComingCallActivity.IsActive)
                                                                    {
                                                                        intent.AddFlags(ActivityFlags.NewTask);
                                                                        Application.Context.StartActivity(intent);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else if (agoraCall == false)
                                            {
                                                if (SecondPassed <= 5)
                                                {
                                                    MsgTabbedMainActivity.RunCall = false;

                                                    SecondPassed = 0;

                                                    if (VideoAudioComingCallActivity.IsActive)
                                                        VideoAudioComingCallActivity.CallActivity?.FinishVideoAudio();

                                                }
                                            }
                                            else
                                            {
                                                MsgTabbedMainActivity.RunCall = false;

                                                if (VideoAudioComingCallActivity.IsActive)
                                                    VideoAudioComingCallActivity.CallActivity?.FinishVideoAudio();
                                            }
                                            #endregion
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }

                                #endregion

                                if (result.Data.Count > 0)
                                { 
                                    ListUtils.UserList = new ObservableCollection<ChatObject>(result.Data);
                                     
                                    //Insert All data users to database
                                    dbDatabase.Insert_Or_Update_LastUsersChat(Application.Context, ListUtils.UserList, UserDetails.SChatHead);
                                    
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                            // Toast.MakeText(Application.Context, "Exception  " + e,ToastLength.Short)?.Show();
                        }
                    }
                    else
                    {
                        (int apiStatus, var respond) = await RequestsAsync.Global.GetChatAsync("all");
                        if (apiStatus != 200 || !(respond is LastChatObject result))
                        {
                            if (respond is ErrorObject errorMessage)
                            {
                                string errorText = errorMessage.Error.ErrorText;

                                if (errorText.Contains("Invalid or expired access_token") || errorText.Contains("No session sent"))
                                    ApiRequest.Logout(MsgTabbedMainActivity.GetInstance().Activity);
                            }
                        }
                        else
                        {
                            var b = new Bundle();
                            b.PutString("Json", JsonConvert.SerializeObject(result));
                            ResultSender?.Send(0, b);

                            //Toast.MakeText(Application.Context, "ResultSender 2 \n" + data,ToastLength.Short)?.Show(); 
                        }
                    }

                    MainHandler.PostDelayed(new PostUpdaterHelper(new Handler(Looper.MainLooper), ResultSender), AppSettings.RefreshChatActivitiesSeconds);
                }
                catch (Exception e)
                {
                    //Toast.MakeText(Application.Context, "ResultSender failed",ToastLength.Short)?.Show();
                    MainHandler.PostDelayed(new PostUpdaterHelper(new Handler(Looper.MainLooper), ResultSender), AppSettings.RefreshChatActivitiesSeconds);
                    Methods.DisplayReportResultTrack(e); 
                }
            }
            else
            {
                try
                {
                    //Toast.MakeText(Application.Context, "Started \n" + Methods.AppLifecycleObserver.AppState + "\n" + ResultSender,ToastLength.Short)?.Show(); 
                    if (Methods.AppLifecycleObserver.AppState == "Background")
                    {
                        try
                        { 
                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                            var login = dbDatabase.Get_data_Login_Credentials();
                            Console.WriteLine(login);

                            if (string.IsNullOrEmpty(Current.AccessToken))
                                return;

                            (int apiStatus, var respond) = await RequestsAsync.Message.Get_users_list_Async(UserDetails.UserId, UserDetails.UserId, "35", "0", false);
                            
                            if (apiStatus != 200 || !(respond is GetUsersListObject result))
                            {
                                // Methods.DisplayReportResult(Activity, respond);
                            }
                            else
                            {
                               // Toast.MakeText(Application.Context, "ResultSender 1 \n",ToastLength.Short)?.Show();

                                #region Call >> Video_Call_User

                                try
                                {
                                    if (AppSettings.EnableAudioVideoCall)
                                    {
                                        if (AppSettings.UseLibrary == SystemCall.Twilio)
                                        {
                                            bool twilioVideoCall = result.VideoCall != null && result.VideoCall.Value;
                                            bool twilioAudioCall = result.AudioCall != null && result.AudioCall.Value;

                                            if (AppSettings.EnableVideoCall)
                                            {
                                                #region Twilio Video call

                                                if (twilioVideoCall && SecondPassed <= 5 && !MsgTabbedMainActivity.RunCall)
                                                {
                                                    //Toast.MakeText(Application.Context, "Twilio Video call",ToastLength.Short)?.Show();

                                                    var callUser = result.VideoCallUser?.CallUserClass;
                                                    if (callUser != null)
                                                    {
                                                       // Toast.MakeText(Application.Context, "Twilio Video Run Call",ToastLength.Short)?.Show();

                                                        MsgTabbedMainActivity.RunCall = true;

                                                        var userId = callUser.UserId;
                                                        var avatar = callUser.Avatar;
                                                        var name = callUser.Name;

                                                        var videosData = callUser.Data;
                                                        if (videosData != null)
                                                        {
                                                            var id = videosData.Id; //call_id
                                                            var accessToken = videosData.AccessToken;
                                                            var accessToken2 = videosData.AccessToken2;
                                                            var fromId = videosData.FromId;
                                                            var active = videosData.Active;
                                                            var time = videosData.Called;
                                                            var declined = videosData.Declined;
                                                            var roomName = videosData.RoomName;

                                                            Intent intent = new Intent(Application.Context, typeof(VideoAudioComingCallActivity));
                                                            intent.PutExtra("UserID", userId);
                                                            intent.PutExtra("avatar", avatar);
                                                            intent.PutExtra("name", name);
                                                            intent.PutExtra("access_token", accessToken);
                                                            intent.PutExtra("access_token_2", accessToken2);
                                                            intent.PutExtra("from_id", fromId);
                                                            intent.PutExtra("active", active);
                                                            intent.PutExtra("time", time);
                                                            intent.PutExtra("CallID", id);
                                                            intent.PutExtra("status", declined);
                                                            intent.PutExtra("room_name", roomName);
                                                            intent.PutExtra("declined", declined);
                                                            intent.PutExtra("type", "Twilio_video_call");

                                                            string avatarSplit = avatar.Split('/').Last();
                                                            var getImg = Methods.MultiMedia.GetMediaFrom_Disk(Methods.Path.FolderDiskImage, avatarSplit);
                                                            if (getImg == "File Dont Exists")
                                                                Methods.MultiMedia.DownloadMediaTo_DiskAsync(Methods.Path.FolderDiskImage, avatar);

                                                            if (SecondPassed <= 5)
                                                            {
                                                                MsgTabbedMainActivity.RunCall = false;
                                                                SecondPassed = 0;

                                                                if (!VideoAudioComingCallActivity.IsActive)
                                                                {
                                                                    intent.AddFlags(ActivityFlags.NewTask);
                                                                    Application.Context.StartActivity(intent);
                                                                }
                                                            }
                                                            else
                                                                SecondPassed++;
                                                        }
                                                    }
                                                }
                                                else if (twilioVideoCall == false && twilioAudioCall == false)
                                                {
                                                    if (SecondPassed > 5)
                                                    {
                                                        MsgTabbedMainActivity.RunCall = false;

                                                        SecondPassed = 0;

                                                        if (VideoAudioComingCallActivity.IsActive)
                                                            VideoAudioComingCallActivity.CallActivity?.FinishVideoAudio();
                                                    }
                                                }
                                                else
                                                {
                                                    MsgTabbedMainActivity.RunCall = false;
                                                 
                                                    if (VideoAudioComingCallActivity.IsActive)
                                                        VideoAudioComingCallActivity.CallActivity?.FinishVideoAudio();

                                                }

                                                #endregion
                                            }

                                            if (AppSettings.EnableAudioCall)
                                            {
                                                #region Twilio Audio call
                                               
                                                if (twilioAudioCall && !MsgTabbedMainActivity.RunCall)
                                                {
                                                    //Toast.MakeText(Application.Context, "Twilio Audio call",ToastLength.Short)?.Show();

                                                    var callUser = result.AudioCallUser?.CallUserClass;
                                                    if (callUser != null)
                                                    {
                                                        MsgTabbedMainActivity.RunCall = true;

                                                        var userId = callUser.UserId;
                                                        var avatar = callUser.Avatar;
                                                        var name = callUser.Name;

                                                        var videosData = callUser.Data;
                                                        if (videosData != null)
                                                        {
                                                            var id = videosData.Id; //call_id
                                                            var accessToken = videosData.AccessToken;
                                                            var accessToken2 = videosData.AccessToken2;
                                                            var fromId = videosData.FromId;
                                                            var active = videosData.Active;
                                                            var time = videosData.Called;
                                                            var declined = videosData.Declined;
                                                            var roomName = videosData.RoomName;

                                                            Intent intent = new Intent(Application.Context, typeof(VideoAudioComingCallActivity));
                                                            intent.PutExtra("UserID", userId);
                                                            intent.PutExtra("avatar", avatar);
                                                            intent.PutExtra("name", name);
                                                            intent.PutExtra("access_token", accessToken);
                                                            intent.PutExtra("access_token_2", accessToken2);
                                                            intent.PutExtra("from_id", fromId);
                                                            intent.PutExtra("active", active);
                                                            intent.PutExtra("time", time);
                                                            intent.PutExtra("CallID", id);
                                                            intent.PutExtra("status", declined);
                                                            intent.PutExtra("room_name", roomName);
                                                            intent.PutExtra("declined", declined);
                                                            intent.PutExtra("type", "Twilio_audio_call");

                                                            string avatarSplit = avatar.Split('/').Last();
                                                            var getImg =
                                                                Methods.MultiMedia.GetMediaFrom_Disk(Methods.Path.FolderDiskImage,
                                                                    avatarSplit);
                                                            if (getImg == "File Dont Exists")
                                                                Methods.MultiMedia.DownloadMediaTo_DiskAsync(
                                                                    Methods.Path.FolderDiskImage, avatar);

                                                            if (SecondPassed <= 5)
                                                            {
                                                                MsgTabbedMainActivity.RunCall = false;
                                                                SecondPassed = 0;

                                                                if (!VideoAudioComingCallActivity.IsActive)
                                                                {
                                                                    intent.AddFlags(ActivityFlags.NewTask);
                                                                    Application.Context.StartActivity(intent);
                                                                }
                                                            }
                                                            else
                                                                SecondPassed++;
                                                        }
                                                    }
                                                }
                                                else if (twilioAudioCall == false && twilioVideoCall == false)
                                                {
                                                    if (SecondPassed <= 5)
                                                    {
                                                        MsgTabbedMainActivity.RunCall = false;

                                                        if (VideoAudioComingCallActivity.IsActive)
                                                            VideoAudioComingCallActivity.CallActivity?.FinishVideoAudio();
                                                    }
                                                }
                                                else
                                                {
                                                    MsgTabbedMainActivity.RunCall = false;
                                                 
                                                    if (VideoAudioComingCallActivity.IsActive)
                                                        VideoAudioComingCallActivity.CallActivity?.FinishVideoAudio();
                                                }

                                                #endregion
                                            }
                                        }
                                        else if (AppSettings.UseLibrary == SystemCall.Agora)
                                        {
                                            #region Agora Audio/Video call

                                            var agoraCall = result.AgoraCall ?? false;
                                            if (agoraCall && SecondPassed <= 5 && !MsgTabbedMainActivity.RunCall)
                                            {
                                                var callUser = result.AgoraCallData?.CallUserClass;

                                                if (callUser != null)
                                                {
                                                    MsgTabbedMainActivity.RunCall = true;

                                                    var userId = callUser.UserId;
                                                    var avatar = callUser.Avatar;
                                                    var name = callUser.Name;

                                                    var videosData = callUser.Data;
                                                    if (videosData != null)
                                                    {
                                                        var id = videosData.Id; //call_id
                                                                                //var accessToken = videosData.AccessToken;
                                                                                //var accessToken2 = videosData.AccessToken2;
                                                        var fromId = videosData.FromId;
                                                        //var active = videosData.Active;
                                                        var time = videosData.Called;
                                                        //var declined = videosData.Declined;
                                                        var roomName = videosData.RoomName;
                                                        var type = videosData.Type;
                                                        var status = videosData.Status;

                                                        string avatarSplit = avatar.Split('/').Last();
                                                        var getImg = Methods.MultiMedia.GetMediaFrom_Disk(Methods.Path.FolderDiskImage, avatarSplit);
                                                        if (getImg == "File Dont Exists")
                                                            Methods.MultiMedia.DownloadMediaTo_DiskAsync(Methods.Path.FolderDiskImage, avatar);

                                                        if (type == "video")
                                                        {
                                                            if (AppSettings.EnableVideoCall)
                                                            {
                                                                Intent intent = new Intent(Application.Context, typeof(VideoAudioComingCallActivity));
                                                                intent.PutExtra("UserID", userId);
                                                                intent.PutExtra("avatar", avatar);
                                                                intent.PutExtra("name", name);
                                                                intent.PutExtra("from_id", fromId);
                                                                intent.PutExtra("status", status);
                                                                intent.PutExtra("time", time);
                                                                intent.PutExtra("CallID", id);
                                                                intent.PutExtra("room_name", roomName);
                                                                intent.PutExtra("type", "Agora_video_call_recieve");
                                                                intent.PutExtra("declined", "0");

                                                                if (!VideoAudioComingCallActivity.IsActive)
                                                                {
                                                                    intent.AddFlags(ActivityFlags.NewTask);
                                                                    Application.Context.StartActivity(intent);
                                                                }
                                                            }
                                                        }
                                                        else if (type == "audio")
                                                        {
                                                            if (AppSettings.EnableAudioCall)
                                                            {
                                                                Intent intent = new Intent(Application.Context, typeof(VideoAudioComingCallActivity));
                                                                intent.PutExtra("UserID", userId);
                                                                intent.PutExtra("avatar", avatar);
                                                                intent.PutExtra("name", name);
                                                                intent.PutExtra("from_id", fromId);
                                                                intent.PutExtra("status", status);
                                                                intent.PutExtra("time", time);
                                                                intent.PutExtra("CallID", id);
                                                                intent.PutExtra("room_name", roomName);
                                                                intent.PutExtra("type", "Agora_audio_call_recieve");
                                                                intent.PutExtra("declined", "0");

                                                                if (SecondPassed <= 5)
                                                                {
                                                                    MsgTabbedMainActivity.RunCall = false;

                                                                    SecondPassed = 0;


                                                                    if (!VideoAudioComingCallActivity.IsActive)
                                                                    {
                                                                        intent.AddFlags(ActivityFlags.NewTask);
                                                                        Application.Context.StartActivity(intent);
                                                                    }
                                                                }
                                                                else
                                                                    SecondPassed++;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else if (agoraCall == false)
                                            {
                                                if (SecondPassed <= 5)
                                                {
                                                    MsgTabbedMainActivity.RunCall = false;

                                                    SecondPassed = 0;

                                                    if (VideoAudioComingCallActivity.IsActive)
                                                        VideoAudioComingCallActivity.CallActivity?.FinishVideoAudio();

                                                }
                                            }
                                            else
                                            {
                                                MsgTabbedMainActivity.RunCall = false;

                                                if (VideoAudioComingCallActivity.IsActive)
                                                    VideoAudioComingCallActivity.CallActivity?.FinishVideoAudio();
                                            }

                                            #endregion
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                    //Toast.MakeText(Application.Context, "Exception  " + e,ToastLength.Short)?.Show();
                                }

                                #endregion

                                if (result.Users.Count > 0)
                                {
                                    ListUtils.UserChatList = new ObservableCollection<GetUsersListObject.User>(result.Users);

                                    //Insert All data users to database
                                    dbDatabase.Insert_Or_Update_LastUsersChat(Application.Context, ListUtils.UserChatList , UserDetails.SChatHead);
                                    

                                    //Toast.MakeText(Application.Context, "ResultSender 3 \n",ToastLength.Short)?.Show();
                                }
                            }

                            //Toast.MakeText(Application.Context, "ResultSender wael",ToastLength.Short)?.Show();

                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                            //Toast.MakeText(Application.Context, "Exception  " + e,ToastLength.Short)?.Show();
                        }
                    }
                    else
                    {
                        (int apiStatus, var respond) = await RequestsAsync.Message.Get_users_list_Async(UserDetails.UserId, UserDetails.UserId);
                        if (apiStatus != 200 || !(respond is GetUsersListObject result))
                        {
                            if (respond is ErrorObject errorMessage)
                            {
                                string errorText = errorMessage.Error.ErrorText;

                                if (errorText.Contains("Invalid or expired access_token") || errorText.Contains("No session sent"))
                                    ApiRequest.Logout(MsgTabbedMainActivity.GetInstance().Activity);
                            }
                        }
                        else
                        {
                            var b = new Bundle();
                            b.PutString("Json", JsonConvert.SerializeObject(result));
                            ResultSender?.Send(0, b);

                            //Toast.MakeText(Application.Context, "ResultSender 2 \n",ToastLength.Short)?.Show(); 
                        }
                    }

                    MainHandler.PostDelayed(new PostUpdaterHelper(new Handler(Looper.MainLooper), ResultSender), AppSettings.RefreshChatActivitiesSeconds);
                }
                catch (Exception e)
                {
                    //Toast.MakeText(Application.Context, "ResultSender failed",ToastLength.Short)?.Show();
                    MainHandler.PostDelayed(new PostUpdaterHelper(new Handler(Looper.MainLooper), ResultSender), AppSettings.RefreshChatActivitiesSeconds);
                    Methods.DisplayReportResultTrack(e); 
                }
            }
        } 
    }
}