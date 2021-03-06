﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using AFollestad.MaterialDialogs;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.Content;
using AndroidX.Interpolator.View.Animation;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using AT.Markushi.UI;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Bumptech.Glide.Util;
using Developer.SEmojis.Actions;
using Developer.SEmojis.Helper;
using Java.Lang;
using Newtonsoft.Json;
using Refractored.Controls;
using TheArtOfDev.Edmodo.Cropper;
using WoWonder.Activities.AddPost;
using WoWonder.Activities.Chat.Adapters;
using WoWonder.Activities.Chat.ChatWindow;
using WoWonder.Activities.Chat.ChatWindow.Adapters;
using WoWonder.Activities.Chat.PageChat.Fragment;
using WoWonder.Activities.Chat.Viewer;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.PageChat;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using SupportFragment = AndroidX.Fragment.App.Fragment;
using File = Java.IO.File;
using Uri = Android.Net.Uri;
using MessageData = WoWonder.Helpers.Model.MessageDataExtra;
using String = System.String;

namespace WoWonder.Activities.Chat.PageChat
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ResizeableActivity = true, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
    public class PageChatWindowActivity : AppCompatActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback, View.IOnLayoutChangeListener
    { 
        #region Variables Basic

        private ImageView ChatEmojImage;
        private LinearLayout RootView;
        private EmojiconEditText EmojIconEditTextView;
        private CircleImageView PageChatProfileImage;
        public CircleButton ChatSendButton;
        public ImageView ChatColorButton, ChatStickerButton, ChatMediaButton;
        public RecyclerView MRecycler;
        private RecyclerView RecyclerHiSuggestion;
        private FrameLayout ButtonFragmentHolder;
        public FrameLayout TopFragmentHolder;
        private LinearLayoutManager LayoutManager;
        private SupportFragment MainFragmentOpened;
        private Methods.AudioRecorderAndPlayer RecorderService;
        private FastOutSlowInInterpolator Interpolation;
        private readonly string MainChatColor = AppSettings.MainColor;
        private Toolbar ToolBar;
        private Timer Timer;
        private bool IsRecording;
        private LinearLayout FirstLiner, FirstBoxOnButton;
        private RelativeLayout SayHiLayout;
        private RecyclerView SayHiSuggestionsRecycler;
        private EmptySuggetionRecylerAdapter SuggestionAdapter;
        //Action Bar Buttons 
        private ImageView BackButton, AudioCallButton, VideoCallButton, MoreButton;
        private TextView ActionBarTitle, ActionBarSubTitle;
        //Say Hi 
        private TextView SayHiToTextView;
        private AdapterModelsClassMessage SelectedItemPositions;
        private PageChatRecordSoundFragment ChatRecourdSoundBoxFragment;
        public PageChatStickersTabFragment ChatStickersTabBoxFragment;
        public MessageAdapter MAdapter;

        private string GifFile = "", PermissionsType = "", TaskWork, TypeChat, ShowEmpty;
        public PageClass DataProfilePage;
        public ChatObject PageData;
        public string PageId = "" , UserId = "";
        private static PageChatWindowActivity Instance;

        private AdView MAdView;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                Window.SetSoftInputMode(SoftInput.AdjustResize);

                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                Methods.App.FullScreenApp(this);

                Window.SetStatusBarColor(Color.ParseColor(MainChatColor));

                // Set our view from the "ChatWindowLayout" layout resource
                SetContentView(Resource.Layout.ChatWindowLayout);

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                PageId = Intent?.GetStringExtra("PageId") ?? "";
                TypeChat = Intent?.GetStringExtra("TypeChat") ?? "";
                ShowEmpty = Intent?.GetStringExtra("ShowEmpty") ?? "";

                var dataNotifier = Intent?.GetStringExtra("Notifier") ?? "Data not available";
                if (dataNotifier != "Data not available" && !string.IsNullOrEmpty(dataNotifier))
                {
                    if (dataNotifier == "Notifier")
                    {
                        string dataApp = Intent?.GetStringExtra("App");
                        if (dataApp == "Timeline")
                        {
                            UserId = Intent?.GetStringExtra("UserID");

                            string name = Intent?.GetStringExtra("PageName");
                            string avatar = Intent?.GetStringExtra("Avatar");

                            ActionBarTitle.Text = name; // page name
                            SayHiToTextView.Text = name;

                            Glide.With(this).Load(avatar).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Into(PageChatProfileImage);
                        } 
                    }
                }

                LoadData_Item();

                AdsGoogle.Ad_Interstitial(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                AddOrRemoveEvent(true);

                if (Timer != null)
                {
                    Timer.Enabled = true;
                    Timer.Start();
                }

                MAdView?.Resume();
                base.OnResume();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                MAdView?.Pause();

                AddOrRemoveEvent(false);

                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                }
                base.OnPause();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnStart()
        {
            try
            {
                ResetMediaPlayer();
                base.OnStart();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                    Timer.Dispose();
                    Timer = null;
                }

                ResetMediaPlayer();
                MAdView?.Destroy();

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            try
            {
                base.OnConfigurationChanged(newConfig);

                var currentNightMode = newConfig.UiMode & UiMode.NightMask;
                switch (currentNightMode)
                {
                    case UiMode.NightNo:
                        // Night mode is not active, we're using the light theme
                        AppSettings.SetTabDarkTheme = false;
                        break;
                    case UiMode.NightYes:
                        // Night mode is active, we're using dark theme
                        AppSettings.SetTabDarkTheme = true;
                        break;
                }

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                //Audio FrameWork initialize 
                RecorderService = new Methods.AudioRecorderAndPlayer(PageId);

                Interpolation = new FastOutSlowInInterpolator();

                ChatRecourdSoundBoxFragment = new PageChatRecordSoundFragment();
                ChatStickersTabBoxFragment = new PageChatStickersTabFragment();

                //Say Hi 
                SayHiLayout = FindViewById<RelativeLayout>(Resource.Id.SayHiLayout);
                SayHiSuggestionsRecycler = FindViewById<RecyclerView>(Resource.Id.recylerHiSuggetions);
                SayHiToTextView = FindViewById<TextView>(Resource.Id.toUserText);
                //User Info 
                ActionBarTitle = FindViewById<TextView>(Resource.Id.Txt_Username);
                ActionBarSubTitle = FindViewById<TextView>(Resource.Id.Txt_last_time);
                //ActionBarButtons
                BackButton = FindViewById<ImageView>(Resource.Id.BackButton);
                AudioCallButton = FindViewById<ImageView>(Resource.Id.IconCall);
                VideoCallButton = FindViewById<ImageView>(Resource.Id.IconvideoCall);
                MoreButton = FindViewById<ImageView>(Resource.Id.IconMore);
                PageChatProfileImage = FindViewById<CircleImageView>(Resource.Id.userProfileImage);
                RootView = FindViewById<LinearLayout>(Resource.Id.rootChatWindowView);
                ChatEmojImage = FindViewById<ImageView>(Resource.Id.emojiicon);
                EmojIconEditTextView = FindViewById<EmojiconEditText>(Resource.Id.EmojiconEditText5);
                ChatSendButton = FindViewById<CircleButton>(Resource.Id.sendButton);
                MRecycler = FindViewById<RecyclerView>(Resource.Id.recyler);
                ChatColorButton = FindViewById<ImageView>(Resource.Id.colorButton);
                ChatStickerButton = FindViewById<ImageView>(Resource.Id.stickerButton);
                ChatMediaButton = FindViewById<ImageView>(Resource.Id.mediaButton);
                ButtonFragmentHolder = FindViewById<FrameLayout>(Resource.Id.ButtomFragmentHolder);
                TopFragmentHolder = FindViewById<FrameLayout>(Resource.Id.TopFragmentHolder);
                FirstLiner = FindViewById<LinearLayout>(Resource.Id.firstLiner);
                FirstBoxOnButton = FindViewById<LinearLayout>(Resource.Id.firstBoxonButtom);

                ChatColorButton.Visibility = ViewStates.Gone;
                AudioCallButton.Visibility = ViewStates.Gone;
                VideoCallButton.Visibility = ViewStates.Gone;
                MoreButton.Visibility = ViewStates.Gone;

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, null);

                //SupportFragmentManager.BeginTransaction().Add(ButtonFragmentHolder.Id, ChatColorBoxFragment, "ChatColorBoxFragment");
                SupportFragmentManager.BeginTransaction().Add(TopFragmentHolder.Id, ChatRecourdSoundBoxFragment, "Chat_Recourd_Sound_Fragment");

                if (ShowEmpty == "no")
                {
                    SayHiLayout.Visibility = ViewStates.Gone;
                    SayHiSuggestionsRecycler.Visibility = ViewStates.Gone;
                }

                if (AppSettings.SetTabDarkTheme)
                {
                    TopFragmentHolder.SetBackgroundColor(Color.ParseColor("#282828"));
                    FirstLiner.SetBackgroundColor(Color.ParseColor("#282828"));
                    FirstBoxOnButton.SetBackgroundColor(Color.ParseColor("#282828")); 
                }
                else
                {
                    TopFragmentHolder.SetBackgroundColor(Color.White);
                    FirstLiner.SetBackgroundColor(Color.White);
                    FirstBoxOnButton.SetBackgroundColor(Color.White);
                }

                if (AppSettings.ShowButtonRecordSound)
                {
                    ChatSendButton.LongClickable = true;
                    ChatSendButton.Tag = "Free";
                    ChatSendButton.SetImageResource(Resource.Drawable.microphone);
                }
                else
                {
                    ChatSendButton.Tag = "Text";
                    ChatSendButton.SetImageResource(Resource.Drawable.SendLetter);
                }

                ChatSendButton.SetColor(Color.ParseColor(MainChatColor));

                if (AppSettings.ShowButtonStickers)
                {
                    ChatStickerButton.Visibility = ViewStates.Visible;
                    ChatStickerButton.Tag = "Closed";
                }
                else
                {
                    ChatStickerButton.Visibility = ViewStates.Gone;
                }

                //ChatContactButton.Visibility = AppSettings.ShowButtonContact ? ViewStates.Visible : ViewStates.Gone;

                if (AppSettings.ShowButtonColor)
                {
                    ChatColorButton.Visibility = ViewStates.Visible;
                    ChatColorButton.Tag = "Closed";
                }
                else
                {
                    ChatColorButton.Visibility = ViewStates.Gone;
                }

                Methods.SetColorEditText(EmojIconEditTextView, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                ChatEmojImage.SetColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.ParseColor("#444444"));

                var emojisIcon = new EmojIconActions(this, RootView, EmojIconEditTextView, ChatEmojImage);
                emojisIcon.ShowEmojIcon();
                emojisIcon.SetIconsIds(Resource.Drawable.ic_action_keyboard, Resource.Drawable.ic_action_sentiment_satisfied_alt);
                EmojIconEditTextView.AddTextChangedListener(new MyTextWatcher(this));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                ToolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new MessageAdapter(this, PageId , true)
                {
                    DifferList = new ObservableCollection<AdapterModelsClassMessage>()
                };
                 
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<AdapterModelsClassMessage>(this, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                XamarinRecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new XamarinRecyclerViewOnScrollListener(LayoutManager, null);
                xamarinRecyclerViewOnScrollListener.LoadMoreEvent += OnScrollLoadMorefromTop_Event;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);

                RecyclerHiSuggestion = FindViewById<RecyclerView>(Resource.Id.recylerHiSuggetions);
                SuggestionAdapter = new EmptySuggetionRecylerAdapter(this);
                RecyclerHiSuggestion.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false));
                RecyclerHiSuggestion.SetAdapter(SuggestionAdapter);

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    ChatSendButton.Touch += ChatSendButtonOnTouch;
                    ChatMediaButton.Click += ChatMediaButtonOnClick;
                    ChatStickerButton.Click += ChatStickerButtonOnClick;
                    ChatSendButton.LongClick += ChatSendButtonOnLongClick;
                    //EmojIconEditTextView.TextChanged += EmojIconEditTextViewOnTextChanged;
                    BackButton.Click += BackButton_Click; 
                    PageChatProfileImage.Click += UserChatProfileOnClick;
                    ActionBarTitle.Click += UserChatProfileOnClick;
                    ActionBarSubTitle.Click += UserChatProfileOnClick;
                    SuggestionAdapter.OnItemClick += SuggestionAdapterOnOnItemClick;
                    MAdapter.ItemClick += MAdapterOnItemClick;
                    MAdapter.ItemLongClick += MAdapterOnItemLongClick;
                }
                else
                {
                    ChatSendButton.Touch -= ChatSendButtonOnTouch;
                    ChatMediaButton.Click -= ChatMediaButtonOnClick;
                    ChatStickerButton.Click -= ChatStickerButtonOnClick;
                    ChatSendButton.LongClick -= ChatSendButtonOnLongClick;
                    //EmojIconEditTextView.TextChanged -= EmojIconEditTextViewOnTextChanged;
                    BackButton.Click -= BackButton_Click; 
                    PageChatProfileImage.Click -= UserChatProfileOnClick;
                    ActionBarTitle.Click -= UserChatProfileOnClick;
                    ActionBarSubTitle.Click -= UserChatProfileOnClick;
                    SuggestionAdapter.OnItemClick -= SuggestionAdapterOnOnItemClick;
                    MAdapter.ItemClick -= MAdapterOnItemClick;
                    MAdapter.ItemLongClick -= MAdapterOnItemLongClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        } 
        public static PageChatWindowActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        #endregion

        #region Events
  
        private void MAdapterOnItemClick(object sender, Holders.MesClickEventArgs e)
        {
            try
            {
                if (e.Position <= -1) return;
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    switch (e.Type)
                    {
                        case Holders.TypeClick.Text:
                        case Holders.TypeClick.Contact:
                            item.MesData.ShowTimeText = !item.MesData.ShowTimeText;
                            MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(item));
                            break;
                        case Holders.TypeClick.File:
                        {
                            var fileName = item.MesData.Media.Split('/').Last();
                            string imageFile = Methods.MultiMedia.CheckFileIfExits(item.MesData.Media);
                            if (imageFile != "File Dont Exists")
                            {
                                try
                                {
                                    var extension = fileName.Split('.').Last();
                                    string mimeType = MimeTypeMap.GetMimeType(extension);

                                    Intent openFile = new Intent();
                                    openFile.SetFlags(ActivityFlags.NewTask);
                                    openFile.SetFlags(ActivityFlags.GrantReadUriPermission);
                                    openFile.SetAction(Intent.ActionView);
                                    openFile.SetDataAndType(Uri.Parse(imageFile), mimeType);
                                    StartActivity(openFile);
                                }
                                catch (Exception exception)
                                {
                                    Methods.DisplayReportResultTrack(exception);
                                }
                            }
                            else
                            {
                                var extension = fileName.Split('.').Last();
                                string mimeType = MimeTypeMap.GetMimeType(extension);

                                Intent i = new Intent(Intent.ActionView);
                                i.SetData(Uri.Parse(item.MesData.Media));
                                i.SetType(mimeType);
                                StartActivity(i);
                                // Toast.MakeText(MainActivity, MainActivity.GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                            }

                            break;
                        }
                        case Holders.TypeClick.Video:
                        {
                            var fileName = item.MesData.Media.Split('/').Last();
                            var mediaFile = WoWonderTools.GetFile(PageId, Methods.Path.FolderDcimVideo, fileName, item.MesData.Media);

                            string imageFile = Methods.MultiMedia.CheckFileIfExits(mediaFile);
                            if (imageFile != "File Dont Exists")
                            {
                                File file2 = new File(mediaFile);
                                var mediaUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);

                                if (AppSettings.OpenVideoFromApp)
                                {
                                    Intent intent = new Intent(this, typeof(VideoFullScreenActivity));
                                    intent.PutExtra("videoUrl", mediaUri.ToString());
                                    StartActivity(intent);
                                }
                                else
                                {
                                    Intent intent = new Intent();
                                    intent.SetAction(Intent.ActionView);
                                    intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                                    intent.SetDataAndType(mediaUri, "video/*");
                                    StartActivity(intent);
                                }
                            }
                            else
                            {
                                if (AppSettings.OpenVideoFromApp)
                                {
                                    Intent intent = new Intent(this, typeof(VideoFullScreenActivity));
                                    intent.PutExtra("videoUrl", item.MesData.Media);
                                    StartActivity(intent);
                                }
                                else
                                {
                                    Intent intent = new Intent(Intent.ActionView, Uri.Parse(item.MesData.Media));
                                    StartActivity(intent);
                                }
                            }

                            break;
                        }
                        case Holders.TypeClick.Image:
                        {
                            if (AppSettings.OpenImageFromApp)
                            {
                                Intent intent = new Intent(this, typeof(ImageViewerActivity));
                                intent.PutExtra("Id", PageId);
                                intent.PutExtra("SelectedItem", JsonConvert.SerializeObject(item.MesData));
                                StartActivity(intent); 
                            }
                            else
                            {
                                var fileName = item.MesData.Media.Split('/').Last();
                                var mediaFile = WoWonderTools.GetFile(PageId, Methods.Path.FolderDcimImage, fileName, item.MesData.Media);

                                string imageFile = Methods.MultiMedia.CheckFileIfExits(mediaFile);
                                if (imageFile != "File Dont Exists")
                                {
                                    File file2 = new File(mediaFile);
                                    var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);

                                    Intent intent = new Intent();
                                    intent.SetAction(Intent.ActionView);
                                    intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                                    intent.SetDataAndType(photoUri, "image/*");
                                    StartActivity(intent);
                                }
                                else
                                {
                                    Intent intent = new Intent(Intent.ActionView, Uri.Parse(mediaFile));
                                    StartActivity(intent);
                                } 
                            }
                             
                            break;
                        }
                        case Holders.TypeClick.Map:
                        {
                            // Create a Uri from an intent string. Use the result to create an Intent. 
                            var uri = Uri.Parse("geo:" + item.MesData.Lat + "," + item.MesData.Lng);
                            var intent = new Intent(Intent.ActionView, uri);
                            intent.SetPackage("com.google.android.apps.maps");
                            intent.AddFlags(ActivityFlags.NewTask);
                            StartActivity(intent);
                            break;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SuggestionAdapterOnOnItemClick(object sender, AdapterClickEvents e)
        {
            try
            {
                var position = e.Position;
                var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string time2 = unixTimestamp.ToString(CultureInfo.InvariantCulture);

                MessageData m1 = new MessageData
                {
                    Id = time2,
                    FromId = UserDetails.UserId,
                    PageId = PageId,
                    Text = SuggestionAdapter.GetItem(position).RealMessage,
                    Seen = "0",
                    Time = time2,
                    Position = "right",
                    ModelType = MessageModelType.RightText,
                    TimeText = DateTime.Now.ToShortTimeString(),
                    SendFile = true,
                };

                MAdapter.DifferList.Add(new AdapterModelsClassMessage()
                {
                    TypeView = MessageModelType.RightText,
                    Id = Long.ParseLong(m1.Id),
                    MesData = m1
                });

                var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                MAdapter.NotifyItemInserted(indexMes);

                //Scroll Down >> 
                MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                if (Methods.CheckConnectivity())
                {
                    Task.Factory.StartNew(() =>
                    {
                        PageMessageController.SendMessageTask(this, PageId, UserId, time2, SuggestionAdapter.GetItem(position).RealMessage).ConfigureAwait(false);
                    });
                }
                else
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short)?.Show();
                }

                SayHiLayout.Visibility = ViewStates.Gone;
                SayHiSuggestionsRecycler.Visibility = ViewStates.Gone;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void UserChatProfileOnClick(object sender, EventArgs e)
        {
            // open profile page
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            try
            {
                Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void EmojIconEditTextViewOnTextChanged(/*object sender, TextChangedEventArgs e*/)
        {
            try
            {
                if (AppSettings.ShowButtonRecordSound)
                {
                    if (!ButtonFragmentHolder.TranslationY.Equals(1200))
                        ButtonFragmentHolder.TranslationY = 1200;

                    if (IsRecording && EmojIconEditTextView.Text == GetString(Resource.String.Lbl_Recording))
                    {
                        ChatSendButton.Tag = "Text";
                        ChatSendButton.SetImageResource(Resource.Drawable.SendLetter);

                        EditTextOpen();
                    }
                    else if (!string.IsNullOrEmpty(EmojIconEditTextView.Text))
                    {
                        ChatSendButton.Tag = "Text";
                        ChatSendButton.SetImageResource(Resource.Drawable.SendLetter);

                        EditTextOpen(); 
                    }
                    else if (IsRecording)
                    {
                        ChatSendButton.Tag = "Text";
                        ChatSendButton.SetImageResource(Resource.Drawable.SendLetter);


                        EditTextOpen();
                    }
                    else
                    {
                        ChatSendButton.Tag = "Free";
                        ChatSendButton.SetImageResource(Resource.Drawable.microphone);

                        EditTextClose();
                    }
                }
                else
                {
                    ChatSendButton.Tag = "Text";
                    ChatSendButton.SetImageResource(Resource.Drawable.SendLetter);

                    EditTextOpen();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void EditTextClose()
        {

            ChatMediaButton.SetImageResource(Resource.Drawable.attach);
            ChatMediaButton.SetColorFilter(Color.ParseColor("#444444"));
            ChatMediaButton.Tag = "attachment";
            ViewGroup.LayoutParams layoutParams = ChatMediaButton.LayoutParameters;
            layoutParams.Width = 52;
            layoutParams.Height = 52;
            ChatMediaButton.LayoutParameters = layoutParams;
            ChatStickerButton.Visibility = ViewStates.Visible;
            ChatColorButton.Visibility = ViewStates.Gone;
        }

        private void EditTextOpen()
        {
            ChatMediaButton.SetImageResource(Resource.Drawable.ic_next);
            ChatMediaButton.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
            ChatMediaButton.Tag = "arrow";
            ChatStickerButton.Visibility = ViewStates.Gone;
            ChatColorButton.Visibility = ViewStates.Gone;
            ViewGroup.LayoutParams layoutParams = ChatMediaButton.LayoutParameters;
            layoutParams.Width = 42;
            layoutParams.Height = 42;
            ChatMediaButton.LayoutParameters = layoutParams;
        }
         
        //Show Load More Event when scroll to the top Of Recycle
        private async void OnScrollLoadMorefromTop_Event(object sender, EventArgs e)
        {
            try
            {
                //Start Loader Get from Database or API Request >>
                //SwipeRefreshLayout.Refreshing = true;
                //SwipeRefreshLayout.Enabled = true;


                //Code get first Message id where LoadMore >>
                var firstMessageid = MAdapter.DifferList.FirstOrDefault()?.MesData?.Id;
                if (firstMessageid != "")
                    await LoadMoreMessages_API();

                //SwipeRefreshLayout.Refreshing = false;
                //SwipeRefreshLayout.Enabled = false;

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Run Timer
        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                RunOnUiThread(MessageUpdater);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Copy text
        private void MAdapterOnItemLongClick(object sender, Holders.MesClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    SelectedItemPositions = MAdapter.GetItem(e.Position);
                    if (SelectedItemPositions != null)
                    {
                        var arrayAdapter = new List<string>();
                        var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                        if (e.Type == Holders.TypeClick.Text)
                            arrayAdapter.Add(GetText(Resource.String.Lbl_Copy));

                        if (SelectedItemPositions.MesData.Position == "right")
                        {
                            arrayAdapter.Add(GetText(Resource.String.Lbl_MessageInfo));
                            arrayAdapter.Add(GetText(Resource.String.Lbl_DeleteMessage));
                        }

                        arrayAdapter.Add(GetText(Resource.String.Lbl_Forward));

                        dialogList.Items(arrayAdapter);
                        dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                        dialogList.AlwaysCallSingleChoiceCallback();
                        dialogList.ItemsCallback(this).Build().Show();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Intent Contact (result is 506 , Permissions is 101 )
        private void ChatContactButtonOnClick()
        {
            try
            {
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    //request code of result is 506
                    new IntentController(this).OpenIntentGetContactNumberPhone();
                }
                else
                {
                    //Check to see if any permission in our group is available, if one, then all are
                    if (CheckSelfPermission(Manifest.Permission.ReadContacts) == Permission.Granted)
                    {
                        //request code of result is 506
                        new IntentController(this).OpenIntentGetContactNumberPhone();
                    }
                    else
                    {
                        //101 >> ReadContacts && ReadPhoneNumbers
                        new PermissionsController(this).RequestPermission(101);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Intent Location (result is 506 , Permissions is 101 )
        private void ChatLocationButtonOnClick()
        {
            try
            {
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    //Open intent Location when the request code of result is 502
                    new IntentController(this).OpenIntentLocation();
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Granted && CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) == Permission.Granted)
                    {
                        //Open intent Location when the request code of result is 502
                        new IntentController(this).OpenIntentLocation();
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(105);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Send Sticker
        private void ChatStickerButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (ChatStickerButton.Tag.ToString() == "Closed")
                {
                    ResetButtonTags();
                    ChatStickerButton.Tag = "Opened";
                    ChatStickerButton.Drawable.SetTint(Color.ParseColor(AppSettings.MainColor));
                    ReplaceButtomFragment(ChatStickersTabBoxFragment);
                }
                else
                {
                    ResetButtonTags();
                    ChatStickerButton.Drawable.SetTint(Color.ParseColor("#888888"));
                    TopFragmentHolder.Animate().SetInterpolator(Interpolation).TranslationY(1200).SetDuration(300);
                    SupportFragmentManager.BeginTransaction().Remove(ChatStickersTabBoxFragment).Commit();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //record voices ( Permissions is 102 )
        private async void ChatSendButtonOnLongClick(object sender, View.LongClickEventArgs e)
        {
            try
            {
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    if (ChatSendButton.Tag.ToString() == "Free")
                    {
                        //Set Record Style
                        IsRecording = true;

                        if (UserDetails.SoundControl)
                            Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("RecourdVoiceButton.mp3");

                        if (EmojIconEditTextView.Text != GetString(Resource.String.Lbl_Recording))
                        {
                            EmojIconEditTextView.Text = GetString(Resource.String.Lbl_Recording);
                            EmojIconEditTextView.SetTextColor(Color.ParseColor("#FA3C4C"));
                        }

                        ChatSendButton.SetColor(Color.ParseColor("#FA3C4C"));
                        ChatSendButton.SetImageResource(Resource.Drawable.ic_stop_white_24dp);

                        ResetMediaPlayer();

                        RecorderService = new Methods.AudioRecorderAndPlayer(PageId);
                        //Start Audio record
                        await Task.Delay(600);
                        RecorderService.StartRecording();
                    }
                }
                else
                {
                    //Check to see if any permission in our group is available, if one, then all are
                    if (CheckSelfPermission(Manifest.Permission.RecordAudio) == Permission.Granted)
                    {
                        if (ChatSendButton.Tag.ToString() == "Free")
                        {
                            //Set Record Style
                            IsRecording = true;

                            if (UserDetails.SoundControl)
                                Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("RecourdVoiceButton.mp3");

                            if (EmojIconEditTextView.Text != GetString(Resource.String.Lbl_Recording))
                            {
                                EmojIconEditTextView.Text = GetString(Resource.String.Lbl_Recording);
                                EmojIconEditTextView.SetTextColor(Color.ParseColor("#FA3C4C"));
                            }

                            ChatSendButton.SetColor(Color.ParseColor("#FA3C4C"));
                            ChatSendButton.SetImageResource(Resource.Drawable.ic_stop_white_24dp);

                            ResetMediaPlayer();

                            RecorderService = new Methods.AudioRecorderAndPlayer(PageId);
                            //Start Audio record
                            await Task.Delay(600);
                            RecorderService.StartRecording();
                        }
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(102);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // Event sent media (image , Camera , video , file , music )
        private void ChatMediaButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                    if (AppSettings.ShowButtonImage)
                        arrayAdapter.Add(GetText(Resource.String.Lbl_ImageGallery));
                    if (AppSettings.ShowButtonCamera)
                        arrayAdapter.Add(GetText(Resource.String.Lbl_TakeImageFromCamera));
                    if (AppSettings.ShowButtonVideo && WoWonderTools.CheckAllowedFileSharingInServer("Video"))
                        arrayAdapter.Add(GetText(Resource.String.Lbl_VideoGallery));
                    if (AppSettings.ShowButtonVideo && WoWonderTools.CheckAllowedFileSharingInServer("Video"))
                        arrayAdapter.Add(GetText(Resource.String.Lbl_RecordVideoFromCamera));
                    if (AppSettings.ShowButtonAttachFile && WoWonderTools.CheckAllowedFileSharingInServer("File"))
                        arrayAdapter.Add(GetText(Resource.String.Lbl_File));
                    if (AppSettings.ShowButtonMusic && WoWonderTools.CheckAllowedFileSharingInServer("Audio"))
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Music));
                    if (AppSettings.ShowButtonGif)
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Gif));
                    if (AppSettings.ShowButtonContact)
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Contact));
                    if (AppSettings.ShowButtonLocation)
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Location));

                    dialogList.Title(GetString(Resource.String.Lbl_Select_what_you_want));
                    dialogList.Items(arrayAdapter);
                    dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ChatSendButtonOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                var handled = false;

                if (e.Event.Action == MotionEventActions.Down)
                {
                    OnClick_OfSendButton();
                }

                if (e.Event.Action == MotionEventActions.Up)
                {
                    try
                    {
                        if (IsRecording)
                        {
                            RecorderService.StopRecording();
                            var filePath = RecorderService.GetRecorded_Sound_Path();

                            ChatSendButton.SetColor(Color.ParseColor(MainChatColor));
                            ChatSendButton.SetImageResource(Resource.Drawable.SendLetter);

                            if (EmojIconEditTextView.Text == GetString(Resource.String.Lbl_Recording))
                            {
                                if (!string.IsNullOrEmpty(filePath))
                                {
                                    Bundle bundle = new Bundle();
                                    bundle.PutString("FilePath", filePath);
                                    ChatRecourdSoundBoxFragment.Arguments = bundle;
                                    ReplaceTopFragment(ChatRecourdSoundBoxFragment);
                                }

                                EmojIconEditTextView.Text = "";
                                if (AppSettings.SetTabDarkTheme)
                                {
                                    EmojIconEditTextView.SetTextColor(Color.White);
                                    EmojIconEditTextView.SetHintTextColor(Color.White);
                                }
                                else
                                {
                                    EmojIconEditTextView.SetTextColor(Color.ParseColor("#444444"));
                                    EmojIconEditTextView.SetHintTextColor(Color.ParseColor("#444444"));
                                }
                            }

                            IsRecording = false;
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }

                    ChatSendButton.Pressed = false;
                    handled = true;
                }

                e.Handled = handled;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Send Message type => "right_audio" Or "right_text"
        private void OnClick_OfSendButton()
        {
            try
            {
                IsRecording = false;
                 
                var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var time2 = unixTimestamp.ToString();

                if (ChatSendButton.Tag.ToString() == "Audio")
                {
                    var interPolator = new FastOutSlowInInterpolator();
                    TopFragmentHolder.Animate().SetInterpolator(interPolator).TranslationY(1200).SetDuration(300);
                    SupportFragmentManager.BeginTransaction().Remove(ChatRecourdSoundBoxFragment).Commit();

                    string filePath = RecorderService.GetRecorded_Sound_Path();
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        MessageData m1 = new MessageData
                        {
                            Id = time2,
                            FromId = UserDetails.UserId,
                            PageId = PageId,
                            Media = filePath,
                            Seen = "0",
                            Time = time2,
                            Position = "right",
                            TimeText = GetText(Resource.String.Lbl_Uploading),
                            //MediaDuration = Methods.AudioRecorderAndPlayer.GetTimeString(Methods.AudioRecorderAndPlayer.Get_MediaFileDuration(filePath)),
                            ModelType = MessageModelType.RightAudio, 
                            SendFile = true,
                        };

                        MAdapter.DifferList.Add(new AdapterModelsClassMessage()
                        {
                            TypeView = MessageModelType.RightAudio,
                            Id = Long.ParseLong(m1.Id),
                            MesData = m1
                        });

                        var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                        MAdapter.NotifyItemInserted(indexMes);

                        //Scroll Down >> 
                        MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                        //Here on This function will send Selected audio file to the user 
                        if (Methods.CheckConnectivity())
                        {
                            Task.Factory.StartNew(() =>
                            {
                                PageMessageController.SendMessageTask(this, PageId, UserId, time2, EmojIconEditTextView.Text, "", filePath).ConfigureAwait(false);
                            });
                        }
                        else
                        {
                            Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short)?.Show();
                        }
                    }

                    ChatSendButton.Tag = "Free";
                    ChatSendButton.SetImageResource(Resource.Drawable.microphone);

                }
                else if (ChatSendButton.Tag.ToString() == "Text")
                {
                    if (String.IsNullOrEmpty(EmojIconEditTextView.Text))
                    {

                    }
                    else
                    {
                        //Here on This function will send Text Messages to the user 

                        //remove \n in a string
                        string replacement = Regex.Replace(EmojIconEditTextView.Text, @"\t|\n|\r", "");

                        MessageData m1 = new MessageData
                        {
                            Id = time2,
                            FromId = UserDetails.UserId,
                            PageId = PageId,
                            Text = replacement,
                            Seen = "0",
                            Time = time2,
                            Position = "right",
                            ModelType = MessageModelType.RightText,
                            TimeText = DateTime.Now.ToShortTimeString(),
                            SendFile = true,
                        };

                        MAdapter.DifferList.Add(new AdapterModelsClassMessage()
                        {
                            TypeView = MessageModelType.RightText,
                            Id = Long.ParseLong(m1.Id),
                            MesData = m1
                        });

                        var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                        MAdapter.NotifyItemInserted(indexMes);

                        //Scroll Down >> 
                        MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                        if (Methods.CheckConnectivity())
                        {
                            Task.Factory.StartNew(() =>
                            {
                                PageMessageController.SendMessageTask(this, PageId, UserId, time2, EmojIconEditTextView.Text).ConfigureAwait(false);
                            });
                        }
                        else
                        {
                            Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short)?.Show();
                        }

                        EmojIconEditTextView.Text = "";
                    }

                    if (AppSettings.ShowButtonRecordSound)
                    {
                        ChatSendButton.SetImageResource(Resource.Drawable.microphone);
                        ChatSendButton.Tag = "Free";
                    }
                    else
                    {
                        ChatSendButton.Tag = "Text";
                        ChatSendButton.SetImageResource(Resource.Drawable.SendLetter);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                string timeNow = DateTime.Now.ToString("hh:mm");
                var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var time2 = unixTimestamp.ToString();
                 
                if (requestCode == 506 && resultCode == Result.Ok) // right_contact
                {
                    var contact = Methods.PhoneContactManager.Get_ContactInfoBy_Id(data.Data.LastPathSegment);
                    if (contact != null)
                    {
                        var name = contact.UserDisplayName;
                        var phone = contact.PhoneNumber;

                        MessageData m1 = new MessageData
                        {
                            Id = time2,
                            FromId = UserDetails.UserId,
                            PageId = PageId,
                            ContactName = name,
                            ContactNumber = phone,
                            Seen = "0",
                            Time = time2,
                            TimeText = timeNow,
                            Position = "right",
                            ModelType = MessageModelType.RightContact,
                            SendFile = true,
                        };
                        MAdapter.DifferList.Add(new AdapterModelsClassMessage()
                        {
                            TypeView = MessageModelType.RightContact,
                            Id = Long.ParseLong(m1.Id),
                            MesData = m1
                        });

                        var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                        MAdapter.NotifyItemInserted(indexMes);

                        //Scroll Down >> 
                        MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                        var dictionary = new Dictionary<string, string>();

                        if (!dictionary.ContainsKey(name))
                        {
                            dictionary.Add(name, phone);
                        }

                        string dataContact = JsonConvert.SerializeObject(dictionary.ToArray().FirstOrDefault(a => a.Key == name));

                        if (Methods.CheckConnectivity())
                        {
                            //Send contact function
                            Task.Factory.StartNew(() =>
                            {
                                PageMessageController.SendMessageTask(this, PageId, UserId, time2, dataContact, "1").ConfigureAwait(false);
                            });
                        }
                        else
                        {
                            Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short)?.Show();
                        }
                    }
                }
                else if (requestCode == 500 && resultCode == Result.Ok) // right_image 
                {
                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var check = WoWonderTools.CheckMimeTypesWithServer(filepath);
                        if (!check)
                        {
                            //this file not supported on the server , please select another file 
                            Toast.MakeText(this, GetString(Resource.String.Lbl_ErrorFileNotSupported),ToastLength.Short)?.Show();
                            return;
                        }

                        var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                        if (type == "Image")
                        {
                            MessageData m1 = new MessageData
                            {
                                Id = time2,
                                FromId = UserDetails.UserId,
                                PageId = PageId,
                                Media = filepath,
                                Seen = "0",
                                Time = time2,
                                Position = "right",
                                ModelType = MessageModelType.RightImage,
                                TimeText = timeNow,
                                SendFile = true,
                            };
                            MAdapter.DifferList.Add(new AdapterModelsClassMessage()
                            {
                                TypeView = MessageModelType.RightImage,
                                Id = Long.ParseLong(m1.Id),
                                MesData = m1
                            });

                            var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                            MAdapter.NotifyItemInserted(indexMes);

                            //Scroll Down >> 
                            MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                            //Send image function
                            if (Methods.CheckConnectivity())
                            {
                                Task.Factory.StartNew(() =>
                                {
                                    PageMessageController.SendMessageTask(this, PageId, UserId, time2, EmojIconEditTextView.Text, "", filepath).ConfigureAwait(false);
                                });
                            }
                            else
                            {
                                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, GetString(Resource.String.Lbl_Please_check_your_details), ToastLength.Long)?.Show();
                        }
                    }
                }
                else if (requestCode == CropImage.CropImageActivityRequestCode) // right_image 
                {
                    var result = CropImage.GetActivityResult(data);
                    if (resultCode == Result.Ok)
                    {
                        if (result.IsSuccessful)
                        {
                            var resultUri = result.Uri;

                            if (!string.IsNullOrEmpty(resultUri.Path))
                            {
                                MessageData m1 = new MessageData
                                {
                                    Id = time2,
                                    FromId = UserDetails.UserId,
                                    PageId = PageId,
                                    Media = resultUri.Path,
                                    Seen = "0",
                                    Time = time2,
                                    Position = "right",
                                    ModelType = MessageModelType.RightImage,
                                    TimeText = timeNow,
                                    SendFile = true,
                                };
                                MAdapter.DifferList.Add(new AdapterModelsClassMessage()
                                {
                                    TypeView = MessageModelType.RightImage,
                                    Id = Long.ParseLong(m1.Id),
                                    MesData = m1
                                });

                                var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                                MAdapter.NotifyItemInserted(indexMes);

                                //Scroll Down >> 
                                MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                                //Send image function
                                if (Methods.CheckConnectivity())
                                {
                                    Task.Factory.StartNew(() =>
                                    {
                                        PageMessageController.SendMessageTask(this, PageId, UserId, time2, EmojIconEditTextView.Text, "", resultUri.Path).ConfigureAwait(false);
                                    });
                                }
                                else
                                {
                                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                                }
                            }
                            else
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                            }
                        }
                    }
                }
                else if (requestCode == 503 && resultCode == Result.Ok) // Add right_image using camera   
                {
                    if (string.IsNullOrEmpty(IntentController.CurrentPhotoPath))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Failed_to_load),ToastLength.Short)?.Show();
                    }
                    else
                    {
                        //var thumbnail = MediaStore.Images.Media.GetBitmap(ContentResolver, IntentController.ImageCameraUri); 
                        //Bitmap bitmap = BitmapFactory.DecodeFile(IntentController.currentPhotoPath);

                        if (Methods.MultiMedia.CheckFileIfExits(IntentController.CurrentPhotoPath) != "File Dont Exists")
                        {
                            MessageData m1 = new MessageData
                            {
                                Id = time2,
                                FromId = UserDetails.UserId,
                                PageId = PageId,
                                Media = IntentController.CurrentPhotoPath,
                                Seen = "0",
                                Time = time2,
                                Position = "right",
                                ModelType = MessageModelType.RightImage,
                                TimeText = timeNow,
                                SendFile = true,
                            };
                            MAdapter.DifferList.Add(new AdapterModelsClassMessage()
                            {
                                TypeView = MessageModelType.RightImage,
                                Id = Long.ParseLong(m1.Id),
                                MesData = m1
                            });

                            var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                            MAdapter.NotifyItemInserted(indexMes);

                            //Scroll Down >> 
                            MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                            //Send image function
                            if (Methods.CheckConnectivity())
                            {
                                Task.Factory.StartNew(() =>
                                {
                                    PageMessageController.SendMessageTask(this, PageId, UserId, time2, EmojIconEditTextView.Text, "", IntentController.CurrentPhotoPath).ConfigureAwait(false);
                                });
                            }
                            else
                            {
                                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                            }
                        }
                        else
                        {
                            //Toast.MakeText(this, GetText(Resource.String.Lbl_Failed_to_load),ToastLength.Short)?.Show();
                        }
                    }
                }
                else if (requestCode == 501 && resultCode == Result.Ok) // right_video 
                {
                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var check = WoWonderTools.CheckMimeTypesWithServer(filepath);
                        if (!check)
                        {
                            //this file not supported on the server , please select another file 
                            Toast.MakeText(this, GetString(Resource.String.Lbl_ErrorFileNotSupported),ToastLength.Short)?.Show();
                            return;
                        }

                        var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                        if (type == "Video")
                        {
                            var fileName = filepath.Split('/').Last();
                            var fileNameWithoutExtension = fileName.Split('.').First();
                            var pathWithoutFilename = Methods.Path.FolderDcimVideo;
                            //var fullPathFile = new File(Methods.Path.FolderDcimVideo, fileNameWithoutExtension + ".png");

                            var videoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(pathWithoutFilename, fileNameWithoutExtension + ".png");
                            if (videoPlaceHolderImage == "File Dont Exists")
                            {
                                var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(this, data.Data.ToString());
                                Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, pathWithoutFilename);
                            }
                            //wael
                            //var newCopyedFilepath = Methods.MultiMedia.CopyMediaFileTo(filepath, Methods.Path.FolderDcimVideo + "/" + PageId, false, true);
                            //if (newCopyedFilepath != "Path File Dont exits")
                            //{
                                MessageData m1 = new MessageData
                                {
                                    Id = time2,
                                    FromId = UserDetails.UserId,
                                    PageId = PageId,
                                    Media = filepath,
                                    Seen = "0",
                                    Time = time2,
                                    Position = "right",
                                    ModelType = MessageModelType.RightVideo,
                                    TimeText = timeNow,
                                    SendFile = true,
                                };
                                MAdapter.DifferList.Add(new AdapterModelsClassMessage()
                                {
                                    TypeView = MessageModelType.RightVideo,
                                    Id = Long.ParseLong(m1.Id),
                                    MesData = m1
                                });

                                var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                                MAdapter.NotifyItemInserted(indexMes);

                                //Scroll Down >> 
                                MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                                //Send Video function
                                if (Methods.CheckConnectivity())
                                {
                                    Task.Factory.StartNew(() =>
                                    {
                                        PageMessageController.SendMessageTask(this, PageId, UserId, time2, EmojIconEditTextView.Text, "", filepath).ConfigureAwait(false);
                                    });
                                }
                                else
                                {
                                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                                }
                            //}
                        }
                    }  
                }
                else if (requestCode == 513 && resultCode == Result.Ok) // right_video camera 
                {
                    if (Methods.MultiMedia.CheckFileIfExits(IntentController.CurrentVideoPath) != "File Dont Exists" && Build.VERSION.SdkInt <= BuildVersionCodes.OMr1)
                    {
                        var fileName = IntentController.CurrentVideoPath.Split('/').Last();
                        var fileNameWithoutExtension = fileName.Split('.').First();
                        var path = Methods.Path.FolderDcimVideo + "/" + fileNameWithoutExtension + ".png";

                        MessageData m1 = new MessageData
                        {
                            Id = time2,
                            FromId = UserDetails.UserId,
                            PageId = PageId,
                            Media = IntentController.CurrentVideoPath,
                            Seen = "0",
                            Time = time2,
                            Position = "right",
                            ModelType = MessageModelType.RightVideo,
                            TimeText = timeNow,
                            SendFile = true,
                        };
                        MAdapter.DifferList.Add(new AdapterModelsClassMessage()
                        {
                            TypeView = MessageModelType.RightVideo,
                            Id = Long.ParseLong(m1.Id),
                            MesData = m1
                        });

                        var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                        MAdapter.NotifyItemInserted(indexMes);

                        //Scroll Down >> 
                        MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                        //Send Video function
                        if (Methods.CheckConnectivity())
                        {
                            Task.Factory.StartNew(() =>
                            {
                                PageMessageController.SendMessageTask(this, PageId, UserId, time2, EmojIconEditTextView.Text, "", IntentController.CurrentVideoPath).ConfigureAwait(false);
                            });
                        }
                        else
                        {
                            Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                        }
                    }
                    else
                    {
                        var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                        if (filepath != null)
                        {
                            var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                            if (type == "Video")
                            {
                                MessageData m1 = new MessageData
                                {
                                    Id = time2,
                                    FromId = UserDetails.UserId,
                                    PageId = PageId,
                                    Media = filepath,
                                    Seen = "0",
                                    Time = time2,
                                    Position = "right",
                                    ModelType = MessageModelType.RightVideo,
                                    TimeText = timeNow,
                                    SendFile = true,
                                };
                                MAdapter.DifferList.Add(new AdapterModelsClassMessage()
                                {
                                    TypeView = MessageModelType.RightVideo,
                                    Id = Long.ParseLong(m1.Id),
                                    MesData = m1
                                });

                                var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                                MAdapter.NotifyItemInserted(indexMes);

                                //Scroll Down >> 
                                MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                                //Send Video function
                                if (Methods.CheckConnectivity())
                                {
                                    Task.Factory.StartNew(() =>
                                    {
                                        PageMessageController.SendMessageTask(this, PageId, UserId, time2, EmojIconEditTextView.Text, "", filepath).ConfigureAwait(false);
                                    });
                                }
                                else
                                {
                                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                                }
                            }
                        }
                    }
                }
                else if (requestCode == 504 && resultCode == Result.Ok) // right_file
                {
                    string filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var check = WoWonderTools.CheckMimeTypesWithServer(filepath);
                        if (!check)
                        {
                            //this file not supported on the server , please select another file 
                            Toast.MakeText(this, GetString(Resource.String.Lbl_ErrorFileNotSupported),ToastLength.Short)?.Show();
                            return;
                        }

                        string totalSize = Methods.FunString.Format_byte_size(filepath);
                        MessageData m1 = new MessageData
                        {
                            Id = time2,
                            FromId = UserDetails.UserId,
                            PageId = PageId,
                            Media = filepath,
                            FileSize = totalSize,
                            TimeText = timeNow,
                            Seen = "0",
                            Time = time2,
                            Position = "right",
                            ModelType = MessageModelType.RightFile,
                            SendFile = true,
                        };
                        MAdapter.DifferList.Add(new AdapterModelsClassMessage()
                        {
                            TypeView = MessageModelType.RightFile,
                            Id = Long.ParseLong(m1.Id),
                            MesData = m1
                        });

                        var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                        MAdapter.NotifyItemInserted(indexMes);

                        //Scroll Down >> 
                        MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                        //Send Video function
                        if (Methods.CheckConnectivity())
                        {
                            Task.Factory.StartNew(() =>
                            {
                                PageMessageController.SendMessageTask(this, PageId, UserId, time2, EmojIconEditTextView.Text, "", filepath).ConfigureAwait(false);
                            });
                        }
                        else
                        {
                            Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetString(Resource.String.Lbl_Please_check_your_details), ToastLength.Long)?.Show();
                    }
                }
                else if (requestCode == 505 && resultCode == Result.Ok) // right_audio
                {
                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var check = WoWonderTools.CheckMimeTypesWithServer(filepath);
                        if (!check)
                        {
                            //this file not supported on the server , please select another file 
                            Toast.MakeText(this, GetString(Resource.String.Lbl_ErrorFileNotSupported),ToastLength.Short)?.Show();
                            return;
                        }

                        var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                        if (type == "Audio")
                        {
                            //wael
                            //var newCopyedFilepath = Methods.MultiMedia.CopyMediaFileTo(filepath, Methods.Path.FolderDcimSound + "/" + PageId, false, true);
                            //if (newCopyedFilepath != "Path File Dont exits")
                            //{
                                string totalSize = Methods.FunString.Format_byte_size(filepath);
                                MessageData m1 = new MessageData
                                {
                                    Id = time2,
                                    FromId = UserDetails.UserId,
                                    PageId = PageId,
                                    Media = filepath,
                                    FileSize = totalSize,
                                    Seen = "0",
                                    Time = time2,
                                    Position = "right",
                                    TimeText = GetText(Resource.String.Lbl_Uploading),
                                    MediaDuration = Methods.AudioRecorderAndPlayer.GetTimeString(Methods.AudioRecorderAndPlayer.Get_MediaFileDuration(filepath)),
                                    ModelType = MessageModelType.RightAudio,
                                    SendFile = true,
                                };

                                MAdapter.DifferList.Add(new AdapterModelsClassMessage()
                                {
                                    TypeView = MessageModelType.RightAudio,
                                    Id = Long.ParseLong(m1.Id),
                                    MesData = m1
                                });

                                var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                                MAdapter.NotifyItemInserted(indexMes);

                                //Scroll Down >> 
                                MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                                //Send Video function
                                if (Methods.CheckConnectivity())
                                {
                                    Task.Factory.StartNew(() =>
                                    {
                                        PageMessageController.SendMessageTask(this, PageId, UserId, time2, "", "", filepath).ConfigureAwait(false);
                                    });
                                }
                                else
                                {
                                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                                }
                            //}
                        } 
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Failed_to_load),ToastLength.Short)?.Show();
                    }
                }
                else if (requestCode == 300 && resultCode == Result.Ok) // right_gif
                {
                    // G_fixed_height_small_url, // UrlGif - view  >>  mediaFileName
                    // G_fixed_height_small_mp4, //MediaGif - sent >>  media

                    var gifLink = data.GetStringExtra("MediaGif") ?? "Data not available";
                    if (gifLink != "Data not available" && !string.IsNullOrEmpty(gifLink))
                    {
                        var gifUrl = data.GetStringExtra("UrlGif") ?? "Data not available";
                        GifFile = gifLink;

                        MessageData m1 = new MessageData
                        {
                            Id = time2,
                            FromId = UserDetails.UserId,
                            PageId = PageId,
                            Media = GifFile,
                            MediaFileName = gifUrl,
                            Seen = "0",
                            Time = time2,
                            Position = "right",
                            ModelType = MessageModelType.RightGif,
                            TimeText = timeNow,
                            Stickers = gifUrl,
                            SendFile = true,
                        };

                        MAdapter.DifferList.Add(new AdapterModelsClassMessage()
                        {
                            TypeView = MessageModelType.RightGif,
                            Id = Long.ParseLong(m1.Id),
                            MesData = m1
                        });

                        var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                        MAdapter.NotifyItemInserted(indexMes);

                        //Scroll Down >> 
                        MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                        //Send image function
                        if (Methods.CheckConnectivity())
                        {
                            Task.Factory.StartNew(() =>
                            {
                                PageMessageController.SendMessageTask(this, PageId, UserId, time2, EmojIconEditTextView.Text, "", "", "", "", gifUrl).ConfigureAwait(false);
                            });
                        }
                        else
                        {
                            Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetString(Resource.String.Lbl_Please_check_your_details) + " ", ToastLength.Long)?.Show();
                    }
                }
                else if (requestCode == 502 && resultCode == Result.Ok) // Location
                {
                    //var placeAddress = data.GetStringExtra("Address") ?? "";
                    var placeLatLng = data.GetStringExtra("latLng") ?? "";
                    if (!string.IsNullOrEmpty(placeLatLng))
                    {
                        string[] latLng = placeLatLng.Split(',');
                        if (latLng?.Length > 0)
                        {
                            string lat = latLng[0];
                            string lng = latLng[1];
                             
                            MessageData m1 = new MessageData
                            {
                                Id = time2,
                                FromId = UserDetails.UserId,
                                PageId = PageId,
                                Lat = lat,
                                Lng = lng,
                                Seen = "0",
                                Time = time2,
                                Position = "right",
                                ModelType = MessageModelType.RightMap,
                                TimeText = timeNow,
                                SendFile = true,
                            };

                            MAdapter.DifferList.Add(new AdapterModelsClassMessage()
                            {
                                TypeView = MessageModelType.RightGif,
                                Id = Long.ParseLong(m1.Id),
                                MesData = m1
                            });

                            var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                            MAdapter.NotifyItemInserted(indexMes);

                            //Scroll Down >> 
                            MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                            //Send image function
                            if (Methods.CheckConnectivity())
                            {
                                Task.Factory.StartNew(() =>
                                {
                                    PageMessageController.SendMessageTask(this, PageId, UserId, time2, EmojIconEditTextView.Text, "", "", "", "", "", lat,lng).ConfigureAwait(false);
                                });
                            }
                            else
                            {
                                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Permissions
        public override async void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 123)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder(PageId);
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                    }
                }
                else if (requestCode == 108)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        switch (PermissionsType)
                        {
                            //requestCode >> 500 => Image Gallery
                            case "Image" when AppSettings.ImageCropping:
                                OpenDialogGallery("Image");
                                break;
                            case "Image": //requestCode >> 500 => Image Gallery
                                new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures), false);
                                break;
                            case "Video":
                                //requestCode >> 501 => video Gallery
                                new IntentController(this).OpenIntentVideoGallery();
                                break;
                            case "Video_camera":
                                //requestCode >> 501 => video Gallery
                                new IntentController(this).OpenIntentVideoCamera();
                                break;
                            case "Camera":
                                //requestCode >> 503 => Camera
                                new IntentController(this).OpenIntentCamera();
                                break;
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                    }
                }
                else if (requestCode == 100)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        switch (PermissionsType)
                        {
                            case "File":
                                //requestCode >> 504 => File
                                new IntentController(this).OpenIntentFile(GetText(Resource.String.Lbl_SelectFile));
                                break;
                            case "Music":
                                //requestCode >> 505 => Music
                                new IntentController(this).OpenIntentAudio();
                                break;
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                    }
                }
                else if (requestCode == 101)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        //request code of result is 506
                        new IntentController(this).OpenIntentGetContactNumberPhone();
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                    }
                }
                else if (requestCode == 105)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        //Open intent Location when the request code of result is 502
                        new IntentController(this).OpenIntentLocation();
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                    }
                }
                else if (requestCode == 102)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        if (ChatSendButton.Tag.ToString() == "Free")
                        {
                            //Set Record Style
                            IsRecording = true;

                            if (UserDetails.SoundControl)
                                Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("RecourdVoiceButton.mp3");

                            if (EmojIconEditTextView.Text != GetString(Resource.String.Lbl_Recording))
                            {
                                EmojIconEditTextView.Text = GetString(Resource.String.Lbl_Recording);
                                EmojIconEditTextView.SetTextColor(Color.ParseColor("#FA3C4C"));
                            }

                            ChatSendButton.SetColor(Color.ParseColor("#FA3C4C"));
                            ChatSendButton.SetImageResource(Resource.Drawable.ic_stop_white_24dp);

                            ResetMediaPlayer();

                            RecorderService = new Methods.AudioRecorderAndPlayer(PageId);
                            //Start Audio record
                            await Task.Delay(600);
                            RecorderService.StartRecording();
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                if (itemString.ToString() == GetText(Resource.String.Lbl_ImageGallery)) // image 
                {
                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        if (AppSettings.ImageCropping)
                            OpenDialogGallery("Image"); //requestCode >> 500 => Image Gallery
                        else
                            new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures), false); //requestCode >> 500 => Image Gallery
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted
                                                                                                  && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                        {
                            if (AppSettings.ImageCropping)
                                OpenDialogGallery("Image"); //requestCode >> 500 => Image Gallery
                            else
                                new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures), false); //requestCode >> 500 => Image Gallery
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(108);
                        }
                    }
                }
                else if (itemString.ToString() == GetText(Resource.String.Lbl_TakeImageFromCamera)) // Camera 
                {
                    PermissionsType = "Camera";

                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 503 => Camera
                        new IntentController(this).OpenIntentCamera();
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted
                                                                                                  && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                        {
                            //requestCode >> 503 => Camera
                            new IntentController(this).OpenIntentCamera();
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(108);
                        }
                    }
                }
                else if (itemString.ToString() == GetText(Resource.String.Lbl_VideoGallery)) // video  
                {
                    PermissionsType = "Video";

                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 501 => video Gallery
                        new IntentController(this).OpenIntentVideoGallery();
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted
                                                                                                  && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                        {
                            //requestCode >> 501 => video Gallery
                            new IntentController(this).OpenIntentVideoGallery();
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(108);
                        }
                    }
                }
                else if (itemString.ToString() == GetText(Resource.String.Lbl_RecordVideoFromCamera)) // video camera
                {
                    PermissionsType = "Video_camera";

                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 513 => video camera
                        new IntentController(this).OpenIntentVideoCamera();
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted
                                                                                                  && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                        {
                            //requestCode >> 513 => video camera
                            new IntentController(this).OpenIntentVideoCamera();
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(108);
                        }
                    }
                }
                else if (itemString.ToString() == GetText(Resource.String.Lbl_File)) // File  
                {
                    PermissionsType = "File";

                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 504 => File
                        new IntentController(this).OpenIntentFile(GetText(Resource.String.Lbl_SelectFile));
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                            CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                        {
                            //requestCode >> 504 => File
                            new IntentController(this).OpenIntentFile(GetText(Resource.String.Lbl_SelectFile));
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(100);
                        }
                    }
                }
                else if (itemString.ToString() == GetText(Resource.String.Lbl_Music)) // Music  
                {
                    PermissionsType = "Music";

                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                        new IntentController(this).OpenIntentAudio(); //505
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                            new IntentController(this).OpenIntentAudio(); //505
                        else
                            new PermissionsController(this).RequestPermission(100);
                    }
                }
                else if (itemString.ToString() == GetText(Resource.String.Lbl_Gif)) // Gif  
                {
                    StartActivityForResult(new Intent(this, typeof(GifActivity)), 300);
                }
                else if (itemString.ToString() == GetText(Resource.String.Lbl_Contact)) // Contact  
                {
                    ChatContactButtonOnClick();
                }
                else if (itemString.ToString() == GetText(Resource.String.Lbl_Location)) // Location  
                {
                    ChatLocationButtonOnClick();
                }
                else if (itemString.ToString() == GetText(Resource.String.Lbl_Copy))
                {
                    CopyItems();
                }
                else if (itemString.ToString() == GetText(Resource.String.Lbl_Forward))
                {
                    ForwardItems();
                }
                else if (itemString.ToString() == GetText(Resource.String.Lbl_DeleteMessage))
                {
                    DeleteMessageItems();
                }
                else if (itemString.ToString() == GetText(Resource.String.Lbl_MessageInfo))
                {
                    var intent = new Intent(this, typeof(MessageInfoActivity));
                    intent.PutExtra("UserId", PageId);
                    intent.PutExtra("SelectedItem", JsonConvert.SerializeObject(SelectedItemPositions.MesData));
                    StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        //Copy Messages
        private void CopyItems()
        {
            try
            {
                string allText = "";
                if (SelectedItemPositions != null && !string.IsNullOrEmpty(SelectedItemPositions.MesData.Text))
                {
                    allText = SelectedItemPositions.MesData.Text;
                }
                Methods.CopyToClipboard(this, allText);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Forward Messages
        private void ForwardItems()
        {
            try
            {
                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                }

                if (SelectedItemPositions != null)
                {
                    var intent = new Intent(this, typeof(ForwardMessagesActivity));
                    intent.PutExtra("SelectedItem", JsonConvert.SerializeObject(SelectedItemPositions.MesData));
                    StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Delete Message
        private void DeleteMessageItems()
        {
            try
            {
                if (SelectedItemPositions != null)
                {
                    if (Methods.CheckConnectivity())
                    {
                        if (Timer != null)
                        {
                            Timer.Enabled = false;
                            Timer.Stop();
                        }

                        var index = MAdapter.DifferList.IndexOf(SelectedItemPositions);
                        if (index != -1)
                        {
                            MAdapter.DifferList.Remove(SelectedItemPositions);

                            MAdapter.NotifyItemRemoved(index);
                            MAdapter.NotifyItemRangeChanged(index, MAdapter.DifferList.Count);
                        }

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.DeleteMessage(SelectedItemPositions.Id.ToString()) });

                        if (Timer != null)
                        {
                            Timer.Enabled = true;
                            Timer.Start();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region loadData

        private void LoadData_Item()
        {
            try
            {
                if (TypeChat == "PageProfile")
                {
                    string obj = Intent?.GetStringExtra("PageObject") ?? "";
                    if (!string.IsNullOrEmpty(obj))
                        DataProfilePage = JsonConvert.DeserializeObject<PageClass>(obj);

                    if (DataProfilePage != null)
                    {
                        if (DataProfilePage.LastMessage != null)
                            UserId = DataProfilePage.IsPageOnwer != null && DataProfilePage.IsPageOnwer.Value ? DataProfilePage.LastMessage.FromId == UserDetails.UserId ? DataProfilePage.LastMessage.ToId : DataProfilePage.LastMessage.FromId : UserDetails.UserId ?? UserDetails.UserId;
                        else
                            UserId = DataProfilePage.IsPageOnwer != null && DataProfilePage.IsPageOnwer.Value ? DataProfilePage.UserId : UserDetails.UserId;

                        Glide.With(this).Load(DataProfilePage.Avatar).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Into(PageChatProfileImage);
                        ActionBarTitle.Text = Methods.FunString.DecodeString(DataProfilePage.PageName);
                        SayHiToTextView.Text = Methods.FunString.DecodeString(DataProfilePage.PageName);

                        ActionBarSubTitle.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(DataProfilePage.LikesCount)) + GetText(Resource.String.Lbl_PeopleLikeThis);
                    }
                }
                else
                {
                    string obj = Intent?.GetStringExtra("PageObject") ?? "";
                    if (!string.IsNullOrEmpty(obj))
                        PageData = JsonConvert.DeserializeObject<ChatObject>(obj);

                    if (PageData != null)
                    {
                        var userAdminPage = PageData.UserId;
                        if (userAdminPage == PageData.LastMessage.LastMessageClass.ToData.UserId)
                        {
                            UserId = PageData.LastMessage.LastMessageClass.UserData.UserId;
                            var name = PageData.LastMessage.LastMessageClass.UserData.Name + " (" + PageData.PageName + ")";

                            ActionBarTitle.Text = Methods.FunString.DecodeString(name);
                            SayHiToTextView.Text = Methods.FunString.DecodeString(name);
                        }
                        else
                        {
                            UserId = PageData.LastMessage.LastMessageClass.ToData.UserId;
                            var name = PageData.LastMessage.LastMessageClass.ToData.Name + " (" + PageData.PageName + ")";

                            ActionBarTitle.Text = Methods.FunString.DecodeString(name);
                            SayHiToTextView.Text = Methods.FunString.DecodeString(name);
                        }

                        //ActionBarSubTitle.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(PageData.LikesCount)) + GetText(Resource.String.Lbl_PeopleLikeThis));
                        Glide.With(this).Load(PageData.Avatar).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Into(PageChatProfileImage);
                    }
                }

                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt >= 23)
                {
                    if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                        CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted &&
                        CheckSelfPermission(Manifest.Permission.AccessMediaLocation) == Permission.Granted &&
                        CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted
                        && CheckSelfPermission(Manifest.Permission.RecordAudio) == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder(PageId);
                    }
                    else
                    {
                        RequestPermissions(new[]
                        {
                            Manifest.Permission.Camera,
                            Manifest.Permission.AccessMediaLocation,
                            Manifest.Permission.ReadExternalStorage,
                            Manifest.Permission.WriteExternalStorage,
                            Manifest.Permission.RecordAudio,
                            Manifest.Permission.ModifyAudioSettings,
                        }, 123);
                    }
                }
                else
                {
                    Methods.Path.Chack_MyFolder(PageId);
                }

                GetMessages();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void GetMessages()
        {
            try
            {
                MAdapter.DifferList.Clear();

                //SwipeRefreshLayout.Refreshing = true;
                //SwipeRefreshLayout.Enabled = true;

               await GetMessages_Api();

               if (MAdapter.DifferList.Count > 0 && SayHiLayout.Visibility != ViewStates.Gone)
               {
                   SayHiLayout.Visibility = ViewStates.Gone;
                   SayHiSuggestionsRecycler.Visibility = ViewStates.Gone;
               }
               else if (MAdapter.DifferList.Count == 0)
               {
                   SayHiLayout.Visibility = ViewStates.Visible;
                   SayHiSuggestionsRecycler.Visibility = ViewStates.Visible;
               }

                TaskWork = "Working";

                //Run timer
                Timer = new Timer {Interval = AppSettings.MessageRequestSpeed, Enabled = true};
                Timer.Elapsed += TimerOnElapsed;
                Timer.Start();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task GetMessages_Api()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    (int apiStatus, var respond) = await RequestsAsync.PageChat.FetchPageChatMessages(PageId, UserId);
                    if (apiStatus == 200)
                    {
                        if (respond is PageMessagesObject result)
                        {
                            int countList = MAdapter.DifferList.Count;
                            var respondList = result.Data.Count;
                            if (respondList > 0)
                            {
                                result.Data.Reverse();

                                foreach (var item in from item in result.Data let check = MAdapter.DifferList.FirstOrDefault(a => a.MesData.Id == item.Id) where check == null select item)
                                {
                                    var type = Holders.GetTypeModel(item);
                                    if (type == MessageModelType.None)
                                        continue;

                                    MAdapter.DifferList.Add(new AdapterModelsClassMessage()
                                    {
                                        TypeView = type,
                                        Id = Long.ParseLong(item.Id),
                                        MesData = await WoWonderTools.MessageFilter(PageId, item, type)
                                    });
                                }

                                RunOnUiThread(() =>
                                {
                                    if (countList > 0)
                                        MAdapter.NotifyItemRangeInserted(countList, MAdapter.DifferList.Count - countList);
                                    else
                                        MAdapter.NotifyDataSetChanged();

                                    //var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                                    //Scroll Down >> 
                                    MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);
                                });
                            }
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);

                    //SwipeRefreshLayout.Refreshing = false;
                    //SwipeRefreshLayout.Enabled = false;

                }
                else Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short)?.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void MessageUpdater()
        {
            try
            {
                if (TaskWork == "Working")
                {
                    TaskWork = "Stop";

                    if (Methods.CheckConnectivity())
                    {
                        var lastMessageId = MAdapter.DifferList.LastOrDefault()?.MesData?.Id;
                        (int apiStatus, var respond) = await RequestsAsync.PageChat.FetchPageChatMessages(PageId, UserId, "0", "0");
                        if (apiStatus == 200)
                        {
                            int countList = MAdapter.DifferList.Count;
                            if (respond is PageMessagesObject result)
                            {
                                var respondList = result.Data.Count;
                                if (respondList > 0)
                                {
                                    result.Data.Reverse();

                                    foreach (var item in result.Data)
                                    {
                                        var type = Holders.GetTypeModel(item);
                                        if (type == MessageModelType.None)
                                            continue;
                                         
                                        var check = MAdapter.DifferList.FirstOrDefault(a => a.MesData.Id == item.Id);
                                        if (check == null)
                                        {
                                            MAdapter.DifferList.Add(new AdapterModelsClassMessage()
                                            {
                                                TypeView = type,
                                                Id = Long.ParseLong(item.Id),
                                                MesData = await WoWonderTools.MessageFilter(PageId, item, type)
                                            });

                                            //if (countList > 0)
                                            //    MAdapter.NotifyItemRangeInserted(countList, MAdapter.DifferList.Count - countList);
                                            //else
                                                MAdapter.NotifyDataSetChanged();

                                            //var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                                            //Scroll Down >> 
                                            MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);
                                              
                                            if (UserDetails.SoundControl)
                                                Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Popup_GetMesseges.mp3");
                                        }
                                        else if (check.MesData.Seen == "0" && check.MesData.Seen != item.Seen)
                                        {
                                            check.Id = Convert.ToInt32(item.Id);
                                            check.MesData = await WoWonderTools.MessageFilter(PageId, item, type, true);
                                            check.TypeView = type;

                                            if (check.MesData.Position == "right")
                                                MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(check));
                                        }
                                    }
                                     
                                    if (MAdapter.DifferList.Count > 0 && SayHiLayout.Visibility != ViewStates.Gone)
                                    {
                                        SayHiLayout.Visibility = ViewStates.Gone;
                                        SayHiSuggestionsRecycler.Visibility = ViewStates.Gone;
                                    }
                                    else if (MAdapter.DifferList.Count == 0)
                                    {
                                        SayHiLayout.Visibility = ViewStates.Visible;
                                        SayHiSuggestionsRecycler.Visibility = ViewStates.Visible;
                                    }

                                }
                            }
                        }
                        else Methods.DisplayReportResult(this, respond);
                    }
                    else Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short)?.Show();

                    TaskWork = "Working";
                }
            }
            catch (Exception e)
            {
                TaskWork = "Working";
                Methods.DisplayReportResultTrack(e);
            }
        }

        private bool RunLoadMore;
        private async Task LoadMoreMessages_API()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (RunLoadMore)
                        return;

                    RunLoadMore = true;

                    var data = MAdapter.DifferList.FirstOrDefault();
                    var firstMessageId = data?.MesData?.Id ?? "0";
                    var index = MAdapter.DifferList.IndexOf(data);
                    Console.WriteLine(index);

                    (int apiStatus, var respond) = await RequestsAsync.PageChat.FetchPageChatMessages(PageId, UserId, firstMessageId, "0", "15");
                    if (apiStatus == 200)
                    {
                        if (respond is PageMessagesObject result)
                        {
                            var respondList = result.Data.Count;
                            if (respondList > 0)
                            {
                                foreach (var item in from item in result.Data let check = MAdapter.DifferList.FirstOrDefault(a => a.MesData.Id == item.Id) where check == null select item)
                                {
                                    var type = Holders.GetTypeModel(item);
                                    if (type == MessageModelType.None)
                                        continue;

                                    var check = MAdapter.DifferList.FirstOrDefault(a => a.MesData.Id == item.Id);
                                    if (check != null) continue;
                                    var mes = new AdapterModelsClassMessage()
                                    {
                                        TypeView = type,
                                        Id = Long.ParseLong(item.Id),
                                        MesData = await WoWonderTools.MessageFilter(PageId, item, type)
                                    };

                                    MAdapter.DifferList.Insert(0, mes);

                                    RunOnUiThread(() =>
                                    {
                                        MAdapter?.NotifyItemInserted(MAdapter.DifferList.IndexOf(mes));

                                        var indexMes = MAdapter.DifferList.IndexOf(data);
                                        if (indexMes > -1)
                                        {
                                            //Scroll Down >> 
                                            //MRecycler.SmoothScrollToPosition(indexMes);
                                        }
                                    });
                                }
                            }
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);

                    //SwipeRefreshLayout.Refreshing = false;
                    //SwipeRefreshLayout.Enabled = false;


                    RunLoadMore = false;
                }
                else Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short)?.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                RunLoadMore = false;
            }
        }

        #endregion

        private void ResetMediaPlayer()
        {
            try
            {
                var list = MAdapter.DifferList.Where(a => a.TypeView == MessageModelType.LeftAudio || a.TypeView == MessageModelType.RightAudio && a.MesData.MediaPlayer != null).ToList();
                if (list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        item.MesData.MediaIsPlaying = false;

                        if (item.MesData.MediaPlayer != null)
                        {
                            item.MesData.MediaPlayer.Stop();
                            item.MesData.MediaPlayer.Reset();
                        }
                        item.MesData.MediaPlayer = null;
                        item.MesData.MediaTimer = null;

                        item.MesData.MediaPlayer?.Release();
                        item.MesData.MediaPlayer = null;
                    }
                    MAdapter.NotifyDataSetChanged();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ResetButtonTags()
        {
            try
            {
                ChatStickerButton.Tag = "Closed";
                ChatColorButton.Tag = "Closed";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnBackPressed()
        {
            if (SupportFragmentManager.BackStackEntryCount > 0)
            {
                SupportFragmentManager.PopBackStack();
                ResetButtonTags();
                ChatColorButton.Drawable.SetTint(Color.ParseColor("#888888"));
                ChatStickerButton.Drawable.SetTint(Color.ParseColor("#888888"));

                if (SupportFragmentManager.Fragments.Count > 0)
                {
                    var fragmentManager = SupportFragmentManager.BeginTransaction();
                    foreach (var vrg in SupportFragmentManager.Fragments)
                    {
                        Console.WriteLine(vrg);
                        //if (SupportFragmentManager.Fragments.Contains(ChatColorBoxFragment))
                        //{
                        //    fragmentManager.Remove(ChatColorBoxFragment);
                        //}
                        //else
                        if (SupportFragmentManager.Fragments.Contains(ChatStickersTabBoxFragment))
                        {
                            fragmentManager.Remove(ChatStickersTabBoxFragment);
                        }
                    }

                    fragmentManager.Commit();
                }
            }
            else
            {
                base.OnBackPressed();
            }
        }

        #region Fragment

        private void ReplaceTopFragment(SupportFragment fragmentView)
        {
            try
            {
                if (fragmentView.IsVisible)
                    return;

                var trans = SupportFragmentManager.BeginTransaction();
                trans.Replace(TopFragmentHolder.Id, fragmentView);

                if (SupportFragmentManager.BackStackEntryCount == 0)
                {
                    trans.AddToBackStack(null);
                }

                trans.Commit();

                TopFragmentHolder.TranslationY = 1200;
                TopFragmentHolder.Animate().SetInterpolator(new FastOutSlowInInterpolator()).TranslationYBy(-1200).SetDuration(500);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ReplaceButtomFragment(SupportFragment fragmentView)
        {
            try
            {
                if (fragmentView != MainFragmentOpened)
                {
                    //if (MainFragmentOpened == ChatColorBoxFragment)
                    //{
                    //    ChatColorButton.Drawable.SetTint(Color.ParseColor("#888888"));
                    //}
                    //else
                    if (MainFragmentOpened == ChatStickersTabBoxFragment)
                    {
                        ChatStickerButton.Drawable.SetTint(Color.ParseColor("#888888"));
                    }
                }

                if (fragmentView.IsVisible)
                    return;

                var trans = SupportFragmentManager.BeginTransaction();
                trans.Replace(ButtonFragmentHolder.Id, fragmentView);

                if (SupportFragmentManager.BackStackEntryCount == 0)
                {
                    trans.AddToBackStack(null);
                }

                trans.Commit();

                ButtonFragmentHolder.TranslationY = 1200;
                ButtonFragmentHolder.Animate().SetInterpolator(new FastOutSlowInInterpolator()).TranslationYBy(-1200)
                    .SetDuration(500);
                MainFragmentOpened = fragmentView;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public async void Update_One_Messeges(MessageData message)
        {
            try
            {
                var type = Holders.GetTypeModel(message);
                if (type == MessageModelType.None)
                    return;

                var checker = MAdapter.DifferList.FirstOrDefault(a => a.MesData.Id == message.Id);
                if (checker != null)
                {
                    checker.Id = Convert.ToInt32(message.Id);
                    checker.MesData = await WoWonderTools.MessageFilter(PageId, message, type);
                    checker.TypeView = type;
                     
                    RunOnUiThread(() =>
                    {
                        try
                        {
                            switch (checker.TypeView)
                            {
                                case MessageModelType.RightGif:
                                    MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker), "WithoutBlobGIF");
                                    break;
                                case MessageModelType.RightText:
                                    MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker));
                                    break;
                                case MessageModelType.RightSticker:

                                    break;
                                case MessageModelType.RightContact:

                                    break;
                                case MessageModelType.RightFile:
                                    MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker), "WithoutBlobFile");
                                    break;
                                case MessageModelType.RightVideo:
                                    MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker), "WithoutBlobVideo");
                                    break;
                                case MessageModelType.RightImage:
                                    //MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker), "WithoutBlobImage");
                                    break;
                                case MessageModelType.RightAudio:
                                    MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker), "WithoutBlobAudio");
                                    break;
                                case MessageModelType.RightMap:

                                    break;
                            }

                            //Scroll Down >> 
                            MRecycler.ScrollToPosition(MAdapter.DifferList.Count - 1);

                            if (MAdapter.DifferList.Count > 0 && SayHiLayout.Visibility != ViewStates.Gone)
                            {
                                SayHiLayout.Visibility = ViewStates.Gone;
                                SayHiSuggestionsRecycler.Visibility = ViewStates.Gone;
                            }
                            else if (MAdapter.DifferList.Count == 0)
                            {
                                SayHiLayout.Visibility = ViewStates.Visible;
                                SayHiSuggestionsRecycler.Visibility = ViewStates.Visible;
                            }

                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnLayoutChange(View v, int left, int top, int right, int bottom, int oldLeft, int oldTop, int oldRight, int oldBottom)
        {
            var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
           new Handler(Looper.MainLooper).PostDelayed(() =>
            {
                MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);
                MRecycler.SmoothScrollToPosition(indexMes);
            }, 200);
        }

        private void OpenDialogGallery(string typeImage)
        {
            try
            {
                PermissionsType = typeImage;
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Methods.Path.Chack_MyFolder(PageId);

                    //Open Image 
                    var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage + "/" + PageId, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                    CropImage.Activity()
                        .SetInitialCropWindowPaddingRatio(0)
                        .SetAutoZoomEnabled(true)
                        .SetMaxZoom(4)
                        .SetGuidelines(CropImageView.Guidelines.On)
                        .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                        .SetOutputUri(myUri).Start(this);
                }
                else
                {
                    if (!CropImage.IsExplicitCameraPermissionRequired(this) && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                        CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder(PageId);

                        //Open Image 
                        var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage + "/" + PageId, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                        CropImage.Activity()
                            .SetInitialCropWindowPaddingRatio(0)
                            .SetAutoZoomEnabled(true)
                            .SetMaxZoom(4)
                            .SetGuidelines(CropImageView.Guidelines.On)
                            .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                            .SetOutputUri(myUri).Start(this);
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(108);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private class XamarinRecyclerViewOnScrollListener : RecyclerView.OnScrollListener
        {
            public delegate void LoadMoreEventHandler(object sender, EventArgs e);

            public event LoadMoreEventHandler LoadMoreEvent;

            private readonly LinearLayoutManager LayoutManager;
            //private readonly SwipeRefreshLayout SwipeRefreshLayout;

            public XamarinRecyclerViewOnScrollListener(LinearLayoutManager layoutManager, SwipeRefreshLayout swipeRefreshLayout)
            {
                LayoutManager = layoutManager;
                //SwipeRefreshLayout = //SwipeRefreshLayout;
            }

            public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
            {
                try
                {
                    base.OnScrolled(recyclerView, dx, dy);

                    var visibleItemCount = recyclerView.ChildCount;
                    var totalItemCount = recyclerView.GetAdapter().ItemCount;

                    var pastVisiblesItems = LayoutManager.FindFirstVisibleItemPosition();
                    if (pastVisiblesItems == 0 && (visibleItemCount != totalItemCount))
                    {
                        //Load More  from API Request
                        LoadMoreEvent?.Invoke(this, null);
                        //Start Load More messages From Database
                    }
                    else
                    {
                        //if (//SwipeRefreshLayout.Refreshing)
                        //{
                        //    //SwipeRefreshLayout.Refreshing = false;
                        //    //SwipeRefreshLayout.Enabled = false;

                        //}
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        private class MyTextWatcher : Java.Lang.Object, ITextWatcher
        {
            private readonly PageChatWindowActivity ChatWindow;
            public MyTextWatcher(PageChatWindowActivity chatWindow)
            {
                ChatWindow = chatWindow;
            }
            public void AfterTextChanged(IEditable s)
            {
                ChatWindow.EmojIconEditTextViewOnTextChanged();
            }

            public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
            {

            }

            public void OnTextChanged(ICharSequence s, int start, int before, int count)
            {

            }
        }

    }
}