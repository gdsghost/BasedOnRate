using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Widget;
using Iceteck.SiliCompressorrLib;
using Java.Net;
using WoWonder.Activities.Chat.Adapters;
using WoWonder.Activities.Chat.MsgTabbes;
using WoWonder.Activities.Chat.PageChat;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.PageChat;
using WoWonderClient.Requests;
using File = Java.IO.File;
using MessageData = WoWonderClient.Classes.Message.MessageData;

namespace WoWonder.Helpers.Controller
{
    public static class PageMessageController
    {
        //############# DONT'T MODIFY HERE ############# 
        private static ChatObject PageData;
        private static PageClass DataProfilePage;
        private static PageChatWindowActivity MainWindowActivity;

        private static MsgTabbedMainActivity GlobalContext;

        //========================= Functions ========================= 
        public static async Task SendMessageTask(PageChatWindowActivity windowActivity, string pageId, string id, string messageId, string text = "", string contact = "", string pathFile = "", string imageUrl = "", string stickerId = "", string gifUrl = "", string lat = "", string lng = "")
        {
            try
            {
                MainWindowActivity = windowActivity;
                if (windowActivity.PageData != null)
                    PageData = windowActivity.PageData;
                else if (windowActivity.DataProfilePage != null)
                    DataProfilePage = windowActivity.DataProfilePage;

                GlobalContext = MsgTabbedMainActivity.GetInstance();

                if (Methods.AttachmentFiles.Check_FileExtension(pathFile) == "Video")
                {
                    File destinationPath = new File(Methods.Path.FolderDcimVideo + "/Compressor");

                    if (!Directory.Exists(destinationPath.Path))
                        Directory.CreateDirectory(destinationPath.Path);

                    await Task.Factory.StartNew(() => new VideoCompressAsyncTask(windowActivity, pageId, id, messageId, text, pathFile).Execute("false", pathFile, destinationPath.Path));
                }
                else
                {
                    StartApiService(pageId, id, messageId, text, contact, pathFile, imageUrl, stickerId, gifUrl, lat, lng);
                }
                 
                await Task.Delay(0);
            }
            catch (Exception ex)
            {
               Methods.DisplayReportResultTrack(ex);
            }
        }

        private static void StartApiService(string pageId ,string id, string messageId, string text = "", string contact = "", string pathFile = "", string imageUrl = "", string stickerId = "", string gifUrl = "", string lat = "", string lng = "")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(MainWindowActivity, MainWindowActivity.GetString(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => SendMessage(pageId ,id, messageId, text, contact, pathFile, imageUrl, stickerId, gifUrl, lat, lng) });
        }

        private static async Task SendMessage(string pageId ,string id, string messageId, string text = "", string contact = "", string pathFile = "", string imageUrl = "", string stickerId = "", string gifUrl = "", string lat = "", string lng = "")
        {
            var (apiStatus, respond) = await RequestsAsync.PageChat.Send_MessageToPageChat(pageId ,id, messageId, text, contact, pathFile, imageUrl, stickerId, gifUrl, lat, lng);
            if (apiStatus == 200)
            {
                if (respond is PageSendMessageObject result)
                {
                    UpdateLastIdMessage(result.Data);
                }
            }
            else Methods.DisplayReportResult(MainWindowActivity, respond);
        }

        private static async void UpdateLastIdMessage(List<MessageData> chatMessages)
        {
            try
            {
                foreach (var messageInfo in chatMessages)
                {
                    var typeModel = Holders.GetTypeModel(messageInfo);
                    if (typeModel == MessageModelType.None)
                        continue;

                    var message = await WoWonderTools.MessageFilter(messageInfo.PageId, messageInfo, typeModel);
                    
                    message.ModelType = typeModel;
                    message.SendFile = false;

                    var checker = MainWindowActivity?.MAdapter.DifferList?.FirstOrDefault(a => a.MesData.Id == message.MessageHashId);
                    if (checker != null)
                    { 
                        //checker.TypeView = typeModel;
                        checker.MesData = message;
                        checker.Id = Java.Lang.Long.ParseLong(message.Id);
                        checker.TypeView = typeModel;

                        checker.MesData.Id = message.Id;
                        checker.MesData.FromId = message.FromId;
                        checker.MesData.GroupId = message.GroupId;
                        checker.MesData.ToId = message.ToId;
                        checker.MesData.Text = message.Text;
                        checker.MesData.Media = message.Media;
                        checker.MesData.MediaFileName = message.MediaFileName;
                        checker.MesData.MediaFileNames = message.MediaFileNames;
                        checker.MesData.Time = message.Time;
                        checker.MesData.Seen = message.Seen;
                        checker.MesData.DeletedOne = message.DeletedOne;
                        checker.MesData.DeletedTwo = message.DeletedTwo;
                        checker.MesData.SentPush = message.SentPush;
                        checker.MesData.NotificationId = message.NotificationId;
                        checker.MesData.TypeTwo = message.TypeTwo;
                        checker.MesData.Stickers = message.Stickers;
                        checker.MesData.TimeText = message.TimeText;
                        checker.MesData.Position = message.Position;
                        checker.MesData.ModelType = message.ModelType;
                        checker.MesData.FileSize = message.FileSize;
                        checker.MesData.MediaDuration = message.MediaDuration;
                        checker.MesData.MediaIsPlaying = message.MediaIsPlaying;
                        checker.MesData.ContactNumber = message.ContactNumber;
                        checker.MesData.ContactName = message.ContactName;
                        checker.MesData.ProductId = message.ProductId;
                        checker.MesData.MessageUser = message.MessageUser;
                        checker.MesData.Product = message.Product;
                        checker.MesData.MessageHashId = message.MessageHashId;
                        checker.MesData.Lat = message.Lat;
                        checker.MesData.Lng = message.Lng;
                        checker.MesData.SendFile = false;

                        if (AppSettings.LastChatSystem == SystemApiGetLastChat.New)
                        {
                            var updaterUser = GlobalContext?.LastChatTab?.MAdapter?.ChatList.FirstOrDefault(a => a.UserId == message.ToId);
                            if (updaterUser != null)
                            {
                                var index = GlobalContext.LastChatTab.MAdapter.ChatList.IndexOf(GlobalContext.LastChatTab.MAdapter.ChatList.FirstOrDefault(x => x.PageId == message.PageId));
                                if (index > -1)
                                {
                                    if (typeModel == MessageModelType.RightGif)
                                        updaterUser.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendGifFile);
                                    else if (typeModel == MessageModelType.RightText)
                                        updaterUser.LastMessage.LastMessageClass.Text = !string.IsNullOrEmpty(message.Text) ? Methods.FunString.DecodeString(message.Text) : GlobalContext?.GetText(Resource.String.Lbl_SendMessage);
                                    else if (typeModel == MessageModelType.RightSticker)
                                        updaterUser.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendStickerFile);
                                    else if (typeModel == MessageModelType.RightContact)
                                        updaterUser.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendContactnumber);
                                    else if (typeModel == MessageModelType.RightFile)
                                        updaterUser.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendFile);
                                    else if (typeModel == MessageModelType.RightVideo)
                                        updaterUser.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendVideoFile);
                                    else if (typeModel == MessageModelType.RightImage)
                                        updaterUser.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendImageFile);
                                    else if (typeModel == MessageModelType.RightAudio)
                                        updaterUser.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendAudioFile);
                                    else if (typeModel == MessageModelType.RightMap)
                                        updaterUser.LastMessage.LastMessageClass.Text = GlobalContext?.GetText(Resource.String.Lbl_SendLocationFile);

                                    GlobalContext.Activity.RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            GlobalContext?.LastChatTab?.MAdapter?.ChatList.Move(index, 0);
                                            GlobalContext?.LastChatTab?.MAdapter?.NotifyItemMoved(index, 0);
                                            GlobalContext?.LastChatTab?.MAdapter?.NotifyItemChanged(index, "WithoutBlob");
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                        }
                                    });
                                }
                            }
                            else
                            {
                                GlobalContext?.Activity.RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        if (PageData != null)
                                        {
                                            GlobalContext?.LastChatTab.MAdapter.ChatList.Insert(0, PageData);
                                            GlobalContext?.LastChatTab.MAdapter.NotifyItemInserted(0);
                                            GlobalContext?.LastChatTab.MRecycler.ScrollToPosition(GlobalContext.LastChatTab.MAdapter.ChatList.IndexOf(PageData));
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });
                            }
                        }
                        else
                        {
                            var updaterUser = GlobalContext?.LastPageChatsTab?.MAdapter?.LastPageList.FirstOrDefault(a => a.UserId == message.ToId);
                            if (updaterUser != null)
                            {
                                var index = GlobalContext.LastPageChatsTab.MAdapter.LastPageList.IndexOf(GlobalContext.LastPageChatsTab.MAdapter.LastPageList.FirstOrDefault(x => x.PageId == message.PageId));
                                if (index > -1)
                                {
                                    if (typeModel == MessageModelType.RightGif)
                                        updaterUser.LastMessage.Text = GlobalContext?.GetText(Resource.String.Lbl_SendGifFile);
                                    else if (typeModel == MessageModelType.RightText)
                                        updaterUser.LastMessage.Text = !string.IsNullOrEmpty(message.Text) ? Methods.FunString.DecodeString(message.Text) : GlobalContext?.GetText(Resource.String.Lbl_SendMessage);
                                    else if (typeModel == MessageModelType.RightSticker)
                                        updaterUser.LastMessage.Text = GlobalContext?.GetText(Resource.String.Lbl_SendStickerFile);
                                    else if (typeModel == MessageModelType.RightContact)
                                        updaterUser.LastMessage.Text = GlobalContext?.GetText(Resource.String.Lbl_SendContactnumber);
                                    else if (typeModel == MessageModelType.RightFile)
                                        updaterUser.LastMessage.Text = GlobalContext?.GetText(Resource.String.Lbl_SendFile);
                                    else if (typeModel == MessageModelType.RightVideo)
                                        updaterUser.LastMessage.Text = GlobalContext?.GetText(Resource.String.Lbl_SendVideoFile);
                                    else if (typeModel == MessageModelType.RightImage)
                                        updaterUser.LastMessage.Text = GlobalContext?.GetText(Resource.String.Lbl_SendImageFile);
                                    else if (typeModel == MessageModelType.RightAudio)
                                        updaterUser.LastMessage.Text = GlobalContext?.GetText(Resource.String.Lbl_SendAudioFile);
                                    else if (typeModel == MessageModelType.RightMap)
                                        updaterUser.LastMessage.Text = GlobalContext?.GetText(Resource.String.Lbl_SendLocationFile);

                                    GlobalContext.Activity.RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            GlobalContext?.LastPageChatsTab?.MAdapter?.LastPageList.Move(index, 0);
                                            GlobalContext?.LastPageChatsTab?.MAdapter?.NotifyItemMoved(index, 0);
                                            GlobalContext?.LastPageChatsTab?.MAdapter?.NotifyItemChanged(index, "WithoutBlob");
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                        }
                                    });
                                }
                            }
                            else
                            {
                                GlobalContext?.Activity.RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        if (DataProfilePage != null)
                                        {
                                            GlobalContext?.LastPageChatsTab?.MAdapter.LastPageList.Insert(0, DataProfilePage);
                                            GlobalContext?.LastPageChatsTab?.MAdapter.NotifyItemInserted(0);
                                            GlobalContext?.LastPageChatsTab?.MRecycler.ScrollToPosition(GlobalContext.LastPageChatsTab.MAdapter.LastPageList.IndexOf(DataProfilePage));
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });
                            }
                        }

                        GlobalContext?.Activity.RunOnUiThread(() =>
                        {
                            try
                            {
                                //Update data RecyclerView Messages.
                                if (message.ModelType != MessageModelType.RightSticker || message.ModelType != MessageModelType.RightImage || message.ModelType != MessageModelType.RightMap || message.ModelType != MessageModelType.RightVideo)
                                    MainWindowActivity.Update_One_Messeges(checker.MesData);

                                if (UserDetails.SoundControl)
                                    Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Popup_SendMesseges.mp3");
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        }); 
                    }

                    PageData = null;
                    DataProfilePage = null;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        private class VideoCompressAsyncTask : AsyncTask<string, string, string>
        {
            private readonly Context MContext;
            private readonly string PageId;
            private readonly string Id;
            private readonly string MessageHashId;
            private readonly string Text;
            private string FilePath;
            public VideoCompressAsyncTask(Context context, string pageId, string id, string messageHashId, string text, string filePath)
            {
                try
                {
                    MContext = context;
                    PageId = pageId;
                    Id = id;
                    MessageHashId = messageHashId;
                    Text = text;
                    FilePath = filePath;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            protected override string RunInBackground(params string[] paths)
            {
                string filePath = null!;
                try
                {
                    //This bellow is just a temporary solution to test that method call works
                    var b = bool.Parse(paths[0]);
                    if (b)
                    {
                        filePath = SiliCompressor.With(MContext).CompressVideo(paths[1], paths[2]);
                    }
                    else
                    {
                        Android.Net.Uri videoContentUri = Android.Net.Uri.Parse(paths[1]);

                        // Example using the bitrate and video size parameters = >> filePath = SiliCompressor.with(mContext).compressVideo(videoContentUri, paths[2], 1280,720,1500000);*/
                        filePath = SiliCompressor.With(MContext).CompressVideo(videoContentUri?.ToString(), paths[2]);
                    }
                }
                catch (URISyntaxException e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
                return filePath;
            }

            protected override void OnPostExecute(string compressedFilePath)
            {
                try
                {
                    base.OnPostExecute(compressedFilePath);

                    File imageFile = new File(compressedFilePath);
                    //float length = imageFile.Length() / 1024f; // Size in KB
                    //string value;
                    //if (length >= 1024)
                    //    value = length / 1024f + " MB";
                    //else
                    //    value = length + " KB";

                    //Methods.DisplayReportResultTrack("Name: " + imageFile.Name + " Size: " + value);

                    //Methods.DisplayReportResultTrack("Silicompressor Path: " + compressedFilePath);

                    var attach = imageFile.Path;
                    if (attach != null)
                    {
                        FilePath = imageFile.Path;
                        StartApiService(PageId ,Id, MessageHashId, Text, "", FilePath);
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }
           
    }
}