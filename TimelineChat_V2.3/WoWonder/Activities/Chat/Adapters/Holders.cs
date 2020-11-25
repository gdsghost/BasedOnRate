using System;
using System.Linq;
using System.Timers;
using Android.Animation;
using Android.Media;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using AT.Markushi.UI;
using Com.Airbnb.Lottie;
using Com.Luseen.Autolinklibrary;
using Java.IO;
using Refractored.Controls;
using WoWonder.Activities.Chat.ChatWindow.Adapters;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.MusicBar;
using WoWonderClient.Classes.Message;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.Chat.Adapters
{
    public class Holders
    {
        public enum TypeClick
        {
            Text, Image, Sound, Contact, Video, Sticker, File, Product, Map
        }

        public static MessageModelType GetTypeModel(MessageData item)
        {
            try
            {
                MessageModelType modelType;

                if (item.FromId == UserDetails.UserId) // right
                {
                    item.Position = "right";
                }
                else if (item.ToId == UserDetails.UserId) // left
                {
                    item.Position = "left";
                }

                string imageUrl = "";
                if (!string.IsNullOrEmpty(item.Stickers))
                {
                    item.Stickers = item.Stickers.Replace(".mp4", ".gif");
                    imageUrl = item.Stickers;
                }

                if (!string.IsNullOrEmpty(item.Media))
                    imageUrl = item.Media;

                if (!string.IsNullOrEmpty(item.Text))
                    modelType = item.TypeTwo == "contact" ? item.Position == "left" ? MessageModelType.LeftContact : MessageModelType.RightContact : item.Position == "left" ? MessageModelType.LeftText : MessageModelType.RightText;
                else if (item.Product?.ProductClass != null && !string.IsNullOrEmpty(item.ProductId) && item.ProductId != "0")
                    modelType = item.Position == "left" ? MessageModelType.LeftProduct : MessageModelType.RightProduct;
                else if (!string.IsNullOrEmpty(item.Lat) && !string.IsNullOrEmpty(item.Lng) && item.Lat != "0" && item.Lng != "0")
                    modelType = item.Position == "left" ? MessageModelType.LeftMap : MessageModelType.RightMap;
                else if (!string.IsNullOrEmpty(imageUrl))
                {
                    var type = Methods.AttachmentFiles.Check_FileExtension(imageUrl);
                    switch (type)
                    {
                        case "Audio":
                            modelType = item.Position == "left" ? MessageModelType.LeftAudio : MessageModelType.RightAudio;
                            break;
                        case "Video":
                            modelType = item.Position == "left" ? MessageModelType.LeftVideo : MessageModelType.RightVideo;
                            break;
                        case "Image" when !string.IsNullOrEmpty(item.Media) && !item.Media.Contains(".gif"):
                            modelType = item.Media.Contains("sticker") ? item.Position == "left" ? MessageModelType.LeftSticker : MessageModelType.RightSticker : item.Position == "left" ? MessageModelType.LeftImage : MessageModelType.RightImage;
                            break;
                        case "Image" when !string.IsNullOrEmpty(item.Stickers) && item.Stickers.Contains(".gif"):
                        case "Image" when !string.IsNullOrEmpty(item.Media) && item.Media.Contains(".gif"):
                            modelType = item.Position == "left" ? MessageModelType.LeftGif : MessageModelType.RightGif;
                            break;
                        case "File":
                            modelType = item.Position == "left" ? MessageModelType.LeftFile : MessageModelType.RightFile;
                            break;
                        default:
                            modelType = MessageModelType.None;
                            break;
                    }
                }
                else
                {
                    modelType = MessageModelType.None;
                }

                return modelType;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return MessageModelType.None;
            }
        }

        public class MesClickEventArgs : EventArgs
        {
            public View View { get; set; }
            public int Position { get; set; }
            public TypeClick Type { get; set; }
        }

        public class TextViewHolder : RecyclerView.ViewHolder
        {
            #region Variables Basic

            public LinearLayout LytParent { get; private set; }
            public LinearLayout BubbleLayout { get; private set; }
            public TextView Time { get; private set; }
            public View MainView { get; private set; }
            public EventHandler ClickHandler { get; set; }
            public AutoLinkTextView AutoLinkTextView { get; private set; }
            public TextView Seen { get; private set; }
            public TextView UserName { get; private set; }
            public LottieAnimationView StarIcon { get; private set; }
            public ImageView StarImage { get; private set; }
            #endregion

            public TextViewHolder(View itemView, Action<MesClickEventArgs> clickListener, Action<MesClickEventArgs> longClickListener, bool showName) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    LytParent = itemView.FindViewById<LinearLayout>(Resource.Id.main);
                    BubbleLayout = itemView.FindViewById<LinearLayout>(Resource.Id.bubble_layout);
                    AutoLinkTextView = itemView.FindViewById<AutoLinkTextView>(Resource.Id.active);
                    Time = itemView.FindViewById<TextView>(Resource.Id.time);
                    Seen = itemView.FindViewById<TextView>(Resource.Id.seen);
                    StarImage = itemView.FindViewById<ImageView>(Resource.Id.fav);
                    StarImage.Visibility = ViewStates.Gone;
                    StarIcon = itemView.FindViewById<LottieAnimationView>(Resource.Id.starIcon);
                    StarIcon.Progress = 0;
                    StarIcon.CancelAnimation();
                    StarIcon.Visibility = ViewStates.Gone;

                    UserName = itemView.FindViewById<TextView>(Resource.Id.name);
                    if (UserName != null) UserName.Visibility = showName ? ViewStates.Visible : ViewStates.Gone;

                    itemView.Click += (sender, args) => clickListener(new MesClickEventArgs { View = itemView, Position = AdapterPosition, Type = TypeClick.Text });
                    AutoLinkTextView.LongClick += (sender, args) => longClickListener(new MesClickEventArgs { View = itemView, Position = AdapterPosition, Type = TypeClick.Text });
                    itemView.LongClick += (sender, args) => longClickListener(new MesClickEventArgs { View = itemView, Position = AdapterPosition, Type = TypeClick.Text });
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public class ImageViewHolder : RecyclerView.ViewHolder
        {
            #region Variables Basic

            public LinearLayout LytParent { get; private set; }
            public RelativeLayout BubbleLayout { get; private set; }
            public View MainView { get; private set; }
            public EventHandler ClickHandler { get; set; }
            public ImageView ImageView { get; private set; }
            public ProgressBar LoadingProgressview { get; private set; }
            public TextView Time { get; private set; }
            public TextView Seen { get; private set; }
            public TextView UserName { get; private set; }
            public ImageView StarImage { get; private set; }
            public LottieAnimationView StarIcon { get; private set; }

            #endregion

            public ImageViewHolder(View itemView, Action<MesClickEventArgs> clickListener, Action<MesClickEventArgs> longClickListener, bool showName, TypeClick typeClick) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    LytParent = itemView.FindViewById<LinearLayout>(Resource.Id.main);
                    BubbleLayout = itemView.FindViewById<RelativeLayout>(Resource.Id.bubble_layout);
                    ImageView = itemView.FindViewById<ImageView>(Resource.Id.imgDisplay);
                    LoadingProgressview = itemView.FindViewById<ProgressBar>(Resource.Id.loadingProgressview);
                    Time = itemView.FindViewById<TextView>(Resource.Id.time);
                    Seen = itemView.FindViewById<TextView>(Resource.Id.seen);
                    StarImage = itemView.FindViewById<ImageView>(Resource.Id.fav);
                    StarImage.Visibility = ViewStates.Gone;
                    StarIcon = itemView.FindViewById<LottieAnimationView>(Resource.Id.starIcon);
                    StarIcon.Progress = 0;
                    StarIcon.CancelAnimation();
                    StarIcon.Visibility = ViewStates.Gone;

                    UserName = itemView.FindViewById<TextView>(Resource.Id.name);
                    if (UserName != null) UserName.Visibility = showName ? ViewStates.Visible : ViewStates.Gone;

                    ImageView.Click += (sender, args) => clickListener(new MesClickEventArgs { View = itemView, Position = AdapterPosition, Type = typeClick });
                    itemView.LongClick += (sender, args) => longClickListener(new MesClickEventArgs { View = itemView, Position = AdapterPosition, Type = typeClick });
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public class SoundViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            #region Variables Basic

            private readonly MessageAdapter MessageAdapter;

            public LinearLayout LytParent { get; private set; }
            public LinearLayout BubbleLayout { get; private set; }
            public View MainView { get; private set; }
            public TextView DurationTextView { get; private set; }
            public TextView MsgTimeTextView { get; private set; }
            public CircleButton PlayButton { get; private set; }
            public ProgressBar LoadingProgressview { get; private set; }
            public TextView UserName { get; private set; }
            public TextView Seen { get; private set; }
            public ImageView StarImage { get; private set; }
            public LottieAnimationView StarIcon { get; private set; }

            #endregion

            public SoundViewHolder(View itemView, Action<MesClickEventArgs> clickListener, Action<MesClickEventArgs> longClickListener, MessageAdapter messageAdapter, bool showName) : base(itemView)
            {
                try
                {
                    MessageAdapter = messageAdapter;

                    MainView = itemView;
                    LytParent = itemView.FindViewById<LinearLayout>(Resource.Id.main);
                    BubbleLayout = itemView.FindViewById<LinearLayout>(Resource.Id.bubble_layout);
                    DurationTextView = itemView.FindViewById<TextView>(Resource.Id.Duration);
                    PlayButton = itemView.FindViewById<CircleButton>(Resource.Id.playButton);
                    MsgTimeTextView = itemView.FindViewById<TextView>(Resource.Id.time);
                    LoadingProgressview = itemView.FindViewById<ProgressBar>(Resource.Id.loadingProgressview);
                    Seen = itemView.FindViewById<TextView>(Resource.Id.seen);
                    StarImage = itemView.FindViewById<ImageView>(Resource.Id.fav);
                    StarImage.Visibility = ViewStates.Gone;
                    StarIcon = itemView.FindViewById<LottieAnimationView>(Resource.Id.starIcon);
                    StarIcon.Progress = 0;
                    StarIcon.CancelAnimation();
                    StarIcon.Visibility = ViewStates.Gone;

                    UserName = itemView.FindViewById<TextView>(Resource.Id.name);
                    if (UserName != null) UserName.Visibility = showName ? ViewStates.Visible : ViewStates.Gone;

                    PlayButton.SetOnClickListener(this);

                    itemView.Click += (sender, args) => clickListener(new MesClickEventArgs { View = itemView, Position = AdapterPosition, Type = TypeClick.Sound });
                    itemView.LongClick += (sender, args) => longClickListener(new MesClickEventArgs { View = itemView, Position = AdapterPosition, Type = TypeClick.Sound });
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = MessageAdapter.DifferList[AdapterPosition]?.MesData;
                        if (v.Id == PlayButton.Id && item != null)
                        {
                            PlaySound(AdapterPosition, item);
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            private void PlaySound(int position, MessageDataExtra message)
            {
                try
                {
                    if (MessageAdapter.PositionSound != position)
                    {
                        var list = MessageAdapter.DifferList.Where(a => a.TypeView == MessageModelType.LeftAudio || a.TypeView == MessageModelType.RightAudio && a.MesData.MediaPlayer != null).ToList();
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

                            MessageAdapter.NotifyItemChanged(MessageAdapter.PositionSound, "WithoutBlobAudio");
                        }
                    }

                    var fileName = message.Media.Split('/').Last();

                    var mediaFile = WoWonderTools.GetFile(MessageAdapter.Id, Methods.Path.FolderDcimSound, fileName, message.Media);
                    if (string.IsNullOrEmpty(message.MediaDuration) || message.MediaDuration == "00:00")
                    {
                        var duration = WoWonderTools.GetDuration(mediaFile);
                        DurationTextView.Text = Methods.AudioRecorderAndPlayer.GetTimeString(duration);
                    }
                    else
                        DurationTextView.Text = message.MediaDuration;

                    if (mediaFile.Contains("http"))
                        mediaFile = WoWonderTools.GetFile(MessageAdapter.Id, Methods.Path.FolderDcimSound, fileName, message.Media);

                    if (message.MediaPlayer == null)
                    {
                        DurationTextView.Text = "00:00";
                        MessageAdapter.PositionSound = position;
                        message.MediaPlayer = new MediaPlayer();
                        message.MediaPlayer.SetAudioAttributes(new AudioAttributes.Builder().SetUsage(AudioUsageKind.Media).SetContentType(AudioContentType.Music).Build());

                        message.MediaPlayer.Completion += (sender, e) =>
                        {
                            try
                            {
                                PlayButton.Tag = "Play";
                                PlayButton.SetImageResource(message.ModelType == MessageModelType.LeftAudio ? Resource.Drawable.ic_play_dark_arrow : Resource.Drawable.ic_play_arrow);

                                message.MediaIsPlaying = false;

                                message.MediaPlayer.Stop();
                                message.MediaPlayer.Reset();
                                message.MediaPlayer = null;

                                message.MediaTimer.Enabled = false;
                                message.MediaTimer.Stop();
                                message.MediaTimer = null;
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        };

                        message.MediaPlayer.Prepared += (s, ee) =>
                        {
                            try
                            {
                                message.MediaIsPlaying = true;
                                PlayButton.Tag = "Pause";
                                if (message.ModelType == MessageModelType.LeftAudio)
                                    PlayButton.SetImageResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.ic_media_pause_light : Resource.Drawable.ic_media_pause_dark);
                                else
                                    PlayButton.SetImageResource(Resource.Drawable.ic_media_pause_light);

                                message.MediaTimer ??= new Timer { Interval = 1000 };

                                message.MediaPlayer.Start();

                                //var durationOfSound = message.MediaPlayer.Duration;

                                message.MediaTimer.Elapsed += (sender, eventArgs) =>
                                {
                                    MessageAdapter.MainActivity.RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            if (message.MediaTimer != null && message.MediaTimer.Enabled)
                                            {
                                                if (message.MediaPlayer.CurrentPosition <= message.MediaPlayer.Duration)
                                                {
                                                    DurationTextView.Text = Methods.AudioRecorderAndPlayer.GetTimeString(message.MediaPlayer.CurrentPosition.ToString());
                                                }
                                                else
                                                {
                                                    DurationTextView.Text = Methods.AudioRecorderAndPlayer.GetTimeString(message.MediaPlayer.Duration.ToString());

                                                    PlayButton.Tag = "Play";
                                                    PlayButton.SetImageResource(message.ModelType == MessageModelType.LeftAudio ? Resource.Drawable.ic_play_dark_arrow : Resource.Drawable.ic_play_arrow);
                                                }
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                            PlayButton.Tag = "Play";
                                        }
                                    });
                                };
                                message.MediaTimer.Start();
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        };

                        if (mediaFile.Contains("http"))
                        {
                            message.MediaPlayer.SetDataSource(MessageAdapter.MainActivity, Uri.Parse(mediaFile));
                            message.MediaPlayer.PrepareAsync();
                        }
                        else
                        {
                            File file2 = new File(mediaFile);
                            var photoUri = FileProvider.GetUriForFile(MessageAdapter.MainActivity, MessageAdapter.MainActivity.PackageName + ".fileprovider", file2);

                            message.MediaPlayer.SetDataSource(MessageAdapter.MainActivity, photoUri);
                            message.MediaPlayer.PrepareAsync();
                        }

                        //message.SoundViewHolder = soundViewHolder;
                    }
                    else
                    {
                        if (PlayButton.Tag.ToString() == "Play")
                        {
                            PlayButton.Tag = "Pause";
                            if (message.ModelType == MessageModelType.LeftAudio)
                                PlayButton.SetImageResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.ic_media_pause_light : Resource.Drawable.ic_media_pause_dark);
                            else
                                PlayButton.SetImageResource(Resource.Drawable.ic_media_pause_light);

                            message.MediaIsPlaying = true;
                            message.MediaPlayer?.Start();

                            if (message.MediaTimer != null)
                            {
                                message.MediaTimer.Enabled = true;
                                message.MediaTimer.Start();
                            }
                        }
                        else if (PlayButton.Tag.ToString() == "Pause")
                        {
                            PlayButton.Tag = "Play";
                            PlayButton.SetImageResource(message.ModelType == MessageModelType.LeftAudio ? Resource.Drawable.ic_play_dark_arrow : Resource.Drawable.ic_play_arrow);

                            message.MediaIsPlaying = false;
                            message.MediaPlayer?.Pause();

                            if (message.MediaTimer != null)
                            {
                                message.MediaTimer.Enabled = false;
                                message.MediaTimer.Stop();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public class MusicBarViewHolder : RecyclerView.ViewHolder, View.IOnClickListener, MusicBar.IOnMusicBarAnimationChangeListener, MusicBar.IOnMusicBarProgressChangeListener, ValueAnimator.IAnimatorUpdateListener
        {
            #region Variables Basic
            private readonly MessageAdapter MessageAdapter;
            public LinearLayout LytParent { get; private set; }
            public LinearLayout BubbleLayout { get; private set; }
            public View MainView { get; private set; }
            public TextView DurationTextView { get; private set; }
            public TextView MsgTimeTextView { get; private set; }
            public CircleButton PlayButton { get; private set; }
            public ProgressBar LoadingProgressview { get; private set; }
            public FixedMusicBar FixedMusicBar { get; private set; }
            public TextView UserName { get; private set; }
            public TextView Seen { get; private set; }
            public ImageView StarImage { get; private set; }
            public LottieAnimationView StarIcon { get; private set; }

            #endregion

            public MusicBarViewHolder(View itemView, Action<MesClickEventArgs> clickListener, Action<MesClickEventArgs> longClickListener, MessageAdapter messageAdapter, bool showName) : base(itemView)
            {
                try
                {
                    MessageAdapter = messageAdapter;

                    MainView = itemView;
                    LytParent = itemView.FindViewById<LinearLayout>(Resource.Id.main);
                    BubbleLayout = itemView.FindViewById<LinearLayout>(Resource.Id.bubble_layout);
                    DurationTextView = itemView.FindViewById<TextView>(Resource.Id.Duration);
                    PlayButton = itemView.FindViewById<CircleButton>(Resource.Id.playButton);
                    MsgTimeTextView = itemView.FindViewById<TextView>(Resource.Id.time);
                    LoadingProgressview = itemView.FindViewById<ProgressBar>(Resource.Id.loadingProgressview);
                    FixedMusicBar = itemView.FindViewById<FixedMusicBar>(Resource.Id.miniMusicBar);
                    Seen = itemView.FindViewById<TextView>(Resource.Id.seen);
                    StarImage = itemView.FindViewById<ImageView>(Resource.Id.fav);
                    StarImage.Visibility = ViewStates.Gone;
                    StarIcon = itemView.FindViewById<LottieAnimationView>(Resource.Id.starIcon);
                    StarIcon.Progress = 0;
                    StarIcon.CancelAnimation();
                    StarIcon.Visibility = ViewStates.Gone;

                    UserName = itemView.FindViewById<TextView>(Resource.Id.name);
                    if (UserName != null) UserName.Visibility = showName ? ViewStates.Visible : ViewStates.Gone;

                    itemView.Click += (sender, args) => clickListener(new MesClickEventArgs { View = itemView, Position = AdapterPosition, Type = TypeClick.Sound });
                    itemView.LongClick += (sender, args) => longClickListener(new MesClickEventArgs { View = itemView, Position = AdapterPosition, Type = TypeClick.Sound });
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        MessageAdapter.MusicBarMessageData = MessageAdapter.DifferList[AdapterPosition]?.MesData;
                        if (v.Id == PlayButton.Id && MessageAdapter.MusicBarMessageData != null)
                        {
                            PlaySound(AdapterPosition, MessageAdapter.MusicBarMessageData);
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            private void PlaySound(int position, MessageDataExtra message)
            {
                try
                {
                    if (MessageAdapter.PositionSound != position)
                    {
                        var list = MessageAdapter.DifferList.Where(a => a.TypeView == MessageModelType.LeftAudio || a.TypeView == MessageModelType.RightAudio && a.MesData.MediaPlayer != null).ToList();
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
                        }
                    }

                    var fileName = message.Media.Split('/').Last();

                    var mediaFile = WoWonderTools.GetFile(MessageAdapter.Id, Methods.Path.FolderDcimSound, fileName, message.Media);
                    if (string.IsNullOrEmpty(message.MediaDuration) || message.MediaDuration == "00:00")
                    {
                        var duration = WoWonderTools.GetDuration(mediaFile);
                        DurationTextView.Text = message.MediaDuration = Methods.AudioRecorderAndPlayer.GetTimeString(duration);
                    }
                    else
                        DurationTextView.Text = message.MediaDuration;

                    if (message.MediaPlayer == null)
                    {
                        MessageAdapter.PositionSound = position;
                        message.MediaPlayer = new MediaPlayer();
                        message.MediaPlayer.SetAudioAttributes(new AudioAttributes.Builder().SetUsage(AudioUsageKind.Media).SetContentType(AudioContentType.Music).Build());

                        message.MediaPlayer.Completion += (sender, e) =>
                        {
                            try
                            {
                                PlayButton.Tag = "Play";
                                PlayButton.SetImageResource(message.ModelType == MessageModelType.LeftAudio ? Resource.Drawable.ic_play_dark_arrow : Resource.Drawable.ic_play_arrow);

                                message.MediaIsPlaying = false;

                                message.MediaPlayer.Stop();
                                message.MediaPlayer.Reset();
                                message.MediaPlayer = null;

                                message.MediaTimer.Enabled = false;
                                message.MediaTimer.Stop();
                                message.MediaTimer = null;

                                FixedMusicBar.StopAutoProgress();
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        };

                        message.MediaPlayer.Prepared += (s, ee) =>
                        {
                            try
                            {
                                message.MediaIsPlaying = true;
                                PlayButton.Tag = "Pause";
                                PlayButton.SetImageResource(message.ModelType == MessageModelType.LeftAudio ? Resource.Drawable.ic_media_pause_dark : Resource.Drawable.ic_media_pause_light);

                                if (message.MediaTimer == null)
                                    message.MediaTimer = new Timer { Interval = 1000 };

                                message.MediaPlayer.Start();

                                var mediaPlayerDuration = message.MediaPlayer.Duration;

                                //change bar width
                                FixedMusicBar.SetBarWidth(2);

                                //change Space Between Bars
                                FixedMusicBar.SetSpaceBetweenBar(2);

                                if (mediaFile.Contains("http"))
                                    mediaFile = WoWonderTools.GetFile(MessageAdapter.Id, Methods.Path.FolderDcimSound, fileName, message.Media);

                                FixedMusicBar.LoadFrom(mediaFile, mediaPlayerDuration);

                                FixedMusicBar.SetAnimationChangeListener(this);
                                FixedMusicBar.SetProgressChangeListener(this);
                                InitValueAnimator(1.0f, 0, mediaPlayerDuration);
                                FixedMusicBar.Show();

                                message.MediaTimer.Elapsed += (sender, eventArgs) =>
                                {
                                    MessageAdapter.MainActivity.RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            if (message.MediaTimer.Enabled)
                                            {
                                                if (message.MediaPlayer.CurrentPosition <= message.MediaPlayer.Duration)
                                                {
                                                    DurationTextView.Text = Methods.AudioRecorderAndPlayer.GetTimeString(message.MediaPlayer.CurrentPosition.ToString());
                                                }
                                                else
                                                {
                                                    DurationTextView.Text = Methods.AudioRecorderAndPlayer.GetTimeString(message.MediaPlayer.Duration.ToString());

                                                    PlayButton.Tag = "Play";
                                                    PlayButton.SetImageResource(message.ModelType == MessageModelType.LeftAudio ? Resource.Drawable.ic_play_dark_arrow : Resource.Drawable.ic_play_arrow);
                                                }
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                            PlayButton.Tag = "Play";
                                        }
                                    });
                                };
                                message.MediaTimer.Start();
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        };

                        if (mediaFile.Contains("http"))
                        {
                            message.MediaPlayer.SetDataSource(MessageAdapter.MainActivity, Uri.Parse(mediaFile));
                            message.MediaPlayer.PrepareAsync();
                        }
                        else
                        {
                            File file2 = new File(mediaFile);
                            var photoUri = FileProvider.GetUriForFile(MessageAdapter.MainActivity, MessageAdapter.MainActivity.PackageName + ".fileprovider", file2);

                            message.MediaPlayer.SetDataSource(MessageAdapter.MainActivity, photoUri);
                            message.MediaPlayer.PrepareAsync();
                        }

                        MessageAdapter.MusicBarMessageData = message;
                        //MusicBarMessageData.MusicBarViewHolder = musicBarViewHolder;
                        //message.MusicBarViewHolder = musicBarViewHolder;
                    }
                    else
                    {
                        if (PlayButton.Tag.ToString() == "Play")
                        {
                            PlayButton.Tag = "Pause";
                            PlayButton.SetImageResource(message.ModelType == MessageModelType.LeftAudio ? Resource.Drawable.ic_media_pause_dark : Resource.Drawable.ic_media_pause_light);

                            message.MediaIsPlaying = true;
                            message.MediaPlayer?.Start();

                            FixedMusicBar.Show();

                            InitValueAnimator(1.0f, FixedMusicBar.GetPosition(), message.MediaPlayer.Duration);

                            if (message.MediaTimer != null)
                            {
                                message.MediaTimer.Enabled = true;
                                message.MediaTimer.Start();
                            }
                        }
                        else if (PlayButton.Tag.ToString() == "Pause")
                        {
                            PlayButton.Tag = "Play";
                            PlayButton.SetImageResource(message.ModelType == MessageModelType.LeftAudio ? Resource.Drawable.ic_play_dark_arrow : Resource.Drawable.ic_play_arrow);

                            message.MediaIsPlaying = false;
                            message.MediaPlayer?.Pause();

                            //stop auto progress animation
                            FixedMusicBar.StopAutoProgress();

                            if (message.MediaTimer != null)
                            {
                                message.MediaTimer.Enabled = false;
                                message.MediaTimer.Stop();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            #region MusicBar

            private void InitValueAnimator(float playbackSpeed, int progress, int max)
            {
                int timeToEnd = (int)((max - progress) / playbackSpeed);
                if (timeToEnd > 0)
                {
                    if (ValueAnimator.OfInt(progress, max).SetDuration(timeToEnd) is ValueAnimator value)
                    {
                        FixedMusicBar?.SetProgressAnimator(value);

                        MessageAdapter.MValueAnimator = value;
                        MessageAdapter.MValueAnimator.SetInterpolator(new LinearInterpolator());
                        MessageAdapter.MValueAnimator.AddUpdateListener(this);
                        MessageAdapter.MValueAnimator.Start();
                    }
                }
            }

            //====================== IOnMusicBarAnimationChangeListener ======================
            public void OnHideAnimationEnd()
            {

            }

            public void OnHideAnimationStart()
            {

            }

            public void OnShowAnimationEnd()
            {

            }

            public void OnShowAnimationStart()
            {

            }

            //====================== IOnMusicBarProgressChangeListener ======================

            public void OnProgressChanged(MusicBar musicBarView, int progress, bool fromUser)
            {
                if (fromUser)
                    MessageAdapter.MSeekBarIsTracking = true;
            }

            public void OnStartTrackingTouch(MusicBar musicBarView)
            {
                MessageAdapter.MSeekBarIsTracking = true;
            }

            public void OnStopTrackingTouch(MusicBar musicBarView)
            {
                try
                {
                    MessageAdapter.MSeekBarIsTracking = false;
                    //MusicBarMessageData?.MusicBarViewHolder?.FixedMusicBar?.InitProgressAnimator(1.0f, musicBarView.GetPosition(), MusicBarMessageData.MediaPlayer.Duration);
                    InitValueAnimator(1.0f, musicBarView.GetPosition(), MessageAdapter.MusicBarMessageData.MediaPlayer.Duration);
                    MessageAdapter.MusicBarMessageData?.MediaPlayer?.SeekTo(musicBarView.GetPosition());
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void OnAnimationUpdate(ValueAnimator animation)
            {
                try
                {
                    if (MessageAdapter.MSeekBarIsTracking)
                    {
                        animation.Cancel();
                    }
                    else
                    {
                        FixedMusicBar.SetProgress((int)animation.AnimatedValue);
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }


            #endregion

        }

        public class ContactViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            #region Variables Basic

            private readonly MessageAdapter MessageAdapter;

            public LinearLayout LytParent { get; private set; }
            public LinearLayout BubbleLayout { get; private set; }
            public LinearLayout ContactLayout { get; private set; }
            public View MainView { get; private set; }
            public TextView UserContactNameTextView { get; private set; }
            public TextView UserNumberTextView { get; private set; }
            public TextView MsgTimeTextView { get; private set; }
            public CircleImageView ProfileImage { get; private set; }
            public TextView Seen { get; private set; }
            public TextView UserName { get; private set; }
            public ImageView StarImage { get; private set; }
            public LottieAnimationView StarIcon { get; private set; }

            #endregion

            public ContactViewHolder(View itemView, Action<MesClickEventArgs> clickListener, Action<MesClickEventArgs> longClickListener, MessageAdapter messageAdapter, bool showName) : base(itemView)
            {
                try
                {
                    MessageAdapter = messageAdapter;

                    MainView = itemView;
                    LytParent = itemView.FindViewById<LinearLayout>(Resource.Id.main);
                    BubbleLayout = itemView.FindViewById<LinearLayout>(Resource.Id.bubble_layout);
                    ContactLayout = itemView.FindViewById<LinearLayout>(Resource.Id.bubble_layout);
                    UserContactNameTextView = itemView.FindViewById<TextView>(Resource.Id.contactName);
                    UserNumberTextView = itemView.FindViewById<TextView>(Resource.Id.numberText);
                    MsgTimeTextView = itemView.FindViewById<TextView>(Resource.Id.time);
                    ProfileImage = itemView.FindViewById<CircleImageView>(Resource.Id.profile_image);
                    Seen = itemView.FindViewById<TextView>(Resource.Id.seen);
                    StarImage = itemView.FindViewById<ImageView>(Resource.Id.fav);
                    StarImage.Visibility = ViewStates.Gone;
                    StarIcon = itemView.FindViewById<LottieAnimationView>(Resource.Id.starIcon);
                    StarIcon.Progress = 0;
                    StarIcon.CancelAnimation();
                    StarIcon.Visibility = ViewStates.Gone;

                    UserName = itemView.FindViewById<TextView>(Resource.Id.name);
                    if (UserName != null) UserName.Visibility = showName ? ViewStates.Visible : ViewStates.Gone;

                    ContactLayout.SetOnClickListener(this);

                    itemView.Click += (sender, args) => clickListener(new MesClickEventArgs { View = itemView, Position = AdapterPosition, Type = TypeClick.Contact });
                    itemView.LongClick += (sender, args) => longClickListener(new MesClickEventArgs { View = itemView, Position = AdapterPosition, Type = TypeClick.Contact });
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = MessageAdapter.DifferList[AdapterPosition]?.MesData;
                        if (v.Id == ContactLayout.Id && item != null)
                        {
                            Methods.App.SaveContacts(MessageAdapter.MainActivity, item.ContactNumber, item.ContactName, "2");
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public class VideoViewHolder : RecyclerView.ViewHolder
        {
            #region Variables Basic

            public LinearLayout LytParent { get; private set; }
            public RelativeLayout BubbleLayout { get; private set; }
            public View MainView { get; private set; }
            public EventHandler ClickHandler { get; set; }
            public ImageView ImageView { get; private set; }
            public ProgressBar LoadingProgressview { get; private set; }
            public TextView MsgTimeTextView { get; private set; }
            public TextView IconView { get; private set; }
            public TextView FilenameTextView { get; private set; }
            public CircleButton PlayButton { get; private set; }
            public TextView UserName { get; private set; }
            public TextView Seen { get; private set; }
            public ImageView StarImage { get; private set; }
            public LottieAnimationView StarIcon { get; private set; }

            #endregion

            public VideoViewHolder(View itemView, Action<MesClickEventArgs> clickListener, Action<MesClickEventArgs> longClickListener, bool showName) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    LytParent = itemView.FindViewById<LinearLayout>(Resource.Id.main);
                    BubbleLayout = itemView.FindViewById<RelativeLayout>(Resource.Id.bubble_layout); //
                    ImageView = itemView.FindViewById<ImageView>(Resource.Id.imgDisplay);
                    LoadingProgressview = itemView.FindViewById<ProgressBar>(Resource.Id.loadingProgressview);
                    MsgTimeTextView = itemView.FindViewById<TextView>(Resource.Id.time);
                    IconView = itemView.FindViewById<TextView>(Resource.Id.icon);
                    FilenameTextView = itemView.FindViewById<TextView>(Resource.Id.fileName);
                    PlayButton = itemView.FindViewById<CircleButton>(Resource.Id.playButton);
                    Seen = itemView.FindViewById<TextView>(Resource.Id.seen);
                    StarImage = itemView.FindViewById<ImageView>(Resource.Id.fav);
                    StarImage.Visibility = ViewStates.Gone;
                    StarIcon = itemView.FindViewById<LottieAnimationView>(Resource.Id.starIcon);
                    StarIcon.Progress = 0;
                    StarIcon.CancelAnimation();
                    StarIcon.Visibility = ViewStates.Gone;

                    UserName = itemView.FindViewById<TextView>(Resource.Id.name);
                    if (UserName != null) UserName.Visibility = showName ? ViewStates.Visible : ViewStates.Gone;

                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconView, IonIconsFonts.Camera);

                    PlayButton.Click += (sender, args) => clickListener(new MesClickEventArgs { View = itemView, Position = AdapterPosition, Type = TypeClick.Video });
                    //itemView.Click += (sender, args) => clickListener(new MesClickEventArgs { View = itemView, Position = AdapterPosition, Type = TypeClick.Video });
                    itemView.LongClick += (sender, args) => longClickListener(new MesClickEventArgs { View = itemView, Position = AdapterPosition, Type = TypeClick.Video });
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public class StickerViewHolder : RecyclerView.ViewHolder
        {
            #region Variables Basic

            public LinearLayout LytParent { get; private set; }
            public View MainView { get; private set; }
            public EventHandler ClickHandler { get; set; }
            public ImageView ImageView { get; private set; }
            public ProgressBar LoadingProgressview { get; private set; }
            public TextView Time { get; private set; }
            public TextView UserName { get; private set; }
            public TextView Seen { get; private set; }
            public ImageView StarImage { get; private set; }
            public LottieAnimationView StarIcon { get; private set; }

            #endregion

            public StickerViewHolder(View itemView, Action<MesClickEventArgs> clickListener, Action<MesClickEventArgs> longClickListener, bool showName) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    LytParent = itemView.FindViewById<LinearLayout>(Resource.Id.main);
                    ImageView = itemView.FindViewById<ImageView>(Resource.Id.imgDisplay);
                    LoadingProgressview = itemView.FindViewById<ProgressBar>(Resource.Id.loadingProgressview);
                    Time = itemView.FindViewById<TextView>(Resource.Id.time);
                    Seen = itemView.FindViewById<TextView>(Resource.Id.seen);
                    StarImage = itemView.FindViewById<ImageView>(Resource.Id.fav);
                    StarImage.Visibility = ViewStates.Gone;
                    StarIcon = itemView.FindViewById<LottieAnimationView>(Resource.Id.starIcon);
                    StarIcon.Progress = 0;
                    StarIcon.CancelAnimation();
                    StarIcon.Visibility = ViewStates.Gone;

                    UserName = itemView.FindViewById<TextView>(Resource.Id.name);
                    if (UserName != null) UserName.Visibility = showName ? ViewStates.Visible : ViewStates.Gone;

                    itemView.Click += (sender, args) => clickListener(new MesClickEventArgs { View = itemView, Position = AdapterPosition, Type = TypeClick.Sticker });
                    itemView.LongClick += (sender, args) => longClickListener(new MesClickEventArgs { View = itemView, Position = AdapterPosition, Type = TypeClick.Sticker });
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public class NotSupportedViewHolder : RecyclerView.ViewHolder
        {
            #region Variables Basic

            public LinearLayout LytParent { get; private set; }
            public View MainView { get; private set; }
            public EventHandler ClickHandler { get; set; }
            public AutoLinkTextView AutoLinkNotsupportedView { get; private set; }

            #endregion

            public NotSupportedViewHolder(View itemView) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    LytParent = itemView.FindViewById<LinearLayout>(Resource.Id.main);
                    AutoLinkNotsupportedView = itemView.FindViewById<AutoLinkTextView>(Resource.Id.active);
                    var time = itemView.FindViewById<TextView>(Resource.Id.time);

                    time.Visibility = ViewStates.Gone;

                    var userName = itemView.FindViewById<TextView>(Resource.Id.name);
                    if (userName != null) userName.Visibility = ViewStates.Gone;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public class FileViewHolder : RecyclerView.ViewHolder
        {
            #region Variables Basic

            public LinearLayout LytParent { get; private set; }
            public LinearLayout BubbleLayout { get; private set; }
            public View MainView { get; private set; }
            public TextView FileNameTextView { get; private set; }
            public TextView SizeFileTextView { get; private set; }
            public TextView MsgTimeTextView { get; private set; }
            public TextView IconTypefile { get; private set; }
            public TextView UserName { get; private set; }
            public TextView Seen { get; private set; }
            public ImageView StarImage { get; private set; }
            public LottieAnimationView StarIcon { get; private set; }

            #endregion

            public FileViewHolder(View itemView, Action<MesClickEventArgs> clickListener, Action<MesClickEventArgs> longClickListener, bool showName) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    LytParent = itemView.FindViewById<LinearLayout>(Resource.Id.main);
                    BubbleLayout = itemView.FindViewById<LinearLayout>(Resource.Id.bubble_layout);
                    FileNameTextView = itemView.FindViewById<TextView>(Resource.Id.fileName);
                    SizeFileTextView = itemView.FindViewById<TextView>(Resource.Id.sizefileText);
                    MsgTimeTextView = itemView.FindViewById<TextView>(Resource.Id.time);
                    IconTypefile = itemView.FindViewById<TextView>(Resource.Id.Icontypefile);
                    Seen = itemView.FindViewById<TextView>(Resource.Id.seen);
                    StarImage = itemView.FindViewById<ImageView>(Resource.Id.fav);
                    StarImage.Visibility = ViewStates.Gone;
                    StarIcon = itemView.FindViewById<LottieAnimationView>(Resource.Id.starIcon);
                    StarIcon.Progress = 0;
                    StarIcon.CancelAnimation();
                    StarIcon.Visibility = ViewStates.Gone;

                    UserName = itemView.FindViewById<TextView>(Resource.Id.name);
                    if (UserName != null) UserName.Visibility = showName ? ViewStates.Visible : ViewStates.Gone;

                    itemView.Click += (sender, args) => clickListener(new MesClickEventArgs { View = itemView, Position = AdapterPosition, Type = TypeClick.File });
                    itemView.LongClick += (sender, args) => longClickListener(new MesClickEventArgs { View = itemView, Position = AdapterPosition, Type = TypeClick.File });
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public class ProductViewHolder : RecyclerView.ViewHolder
        {
            #region Variables Basic

            public LinearLayout LytParent { get; private set; }
            public LinearLayout BubbleLayout { get; private set; }
            public View MainView { get; private set; }
            public EventHandler ClickHandler { get; set; }
            public ImageView ImageView { get; private set; }
            public TextView Time { get; private set; }
            public TextView Seen { get; private set; }
            public TextView Title { get; private set; }
            public TextView Cat { get; private set; }
            public TextView Price { get; private set; }
            public TextView UserName { get; private set; }
            public ImageView StarImage { get; private set; }
            public LottieAnimationView StarIcon { get; private set; }

            #endregion

            public ProductViewHolder(View itemView, Action<MesClickEventArgs> clickListener, Action<MesClickEventArgs> longClickListener, bool showName) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    LytParent = itemView.FindViewById<LinearLayout>(Resource.Id.main);
                    BubbleLayout = itemView.FindViewById<LinearLayout>(Resource.Id.bubble_layout);
                    ImageView = itemView.FindViewById<ImageView>(Resource.Id.imgDisplay);
                    Time = itemView.FindViewById<TextView>(Resource.Id.time);
                    Seen = itemView.FindViewById<TextView>(Resource.Id.seen);
                    StarImage = itemView.FindViewById<ImageView>(Resource.Id.fav);
                    StarImage.Visibility = ViewStates.Gone;
                    StarIcon = itemView.FindViewById<LottieAnimationView>(Resource.Id.starIcon);
                    StarIcon.Progress = 0;
                    StarIcon.CancelAnimation();
                    StarIcon.Visibility = ViewStates.Gone;

                    Title = itemView.FindViewById<TextView>(Resource.Id.title);
                    Cat = itemView.FindViewById<TextView>(Resource.Id.cat);
                    Price = itemView.FindViewById<TextView>(Resource.Id.price);

                    UserName = itemView.FindViewById<TextView>(Resource.Id.name);
                    if (UserName != null) UserName.Visibility = showName ? ViewStates.Visible : ViewStates.Gone;

                    itemView.Click += (sender, args) => clickListener(new MesClickEventArgs { View = itemView, Position = AdapterPosition, Type = TypeClick.Product });
                    itemView.LongClick += (sender, args) => longClickListener(new MesClickEventArgs { View = itemView, Position = AdapterPosition, Type = TypeClick.Product });
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }


        //################################## LastMessages ###############################
        public class LastMessagesViewHolder : RecyclerView.ViewHolder, SwipeItemTouchHelper.ITouchViewHolder
        {
            #region Variables Basic

            public View MainView { get; private set; }

            public LinearLayout MainSwipe { get; private set; }
            public RelativeLayout RelativeLayoutMain { get; private set; }
            public ImageView IconCheckCountMessages { get; private set; }

            public ImageView LastMessagesIcon { get; private set; }
            public TextView TxtUsername { get; private set; }
            public TextView TxtLastMessages { get; private set; }
            public TextView TxtTimestamp { get; private set; }
            public ImageView ImageAvatar { get; private set; } //ImageView
            public CircleImageView ImageLastseen { get; private set; }
            public CircleButton MoreButton { get; private set; }
            public CircleButton CallButton { get; private set; }
            public CircleButton DeleteButton { get; private set; }

            #endregion

            public LastMessagesViewHolder(View itemView, Action<LastMessagesClickEventArgs> clickListener, Action<LastMessagesClickEventArgs> longClickListener
                        , Action<LastMessagesClickEventArgs> moreClickListener, Action<LastMessagesClickEventArgs> callClickListener, Action<LastMessagesClickEventArgs> deleteClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;

                    //Get values
                    MainSwipe = (LinearLayout)MainView.FindViewById(Resource.Id.mainSwipe);
                    MoreButton = (CircleButton)MainView.FindViewById(Resource.Id.moreButton);
                    CallButton = (CircleButton)MainView.FindViewById(Resource.Id.callButton);
                    DeleteButton = (CircleButton)MainView.FindViewById(Resource.Id.deleteButton);

                    RelativeLayoutMain = (RelativeLayout)MainView.FindViewById(Resource.Id.main);
                    ImageAvatar = (ImageView)MainView.FindViewById(Resource.Id.ImageAvatar);
                    ImageLastseen = (CircleImageView)MainView.FindViewById(Resource.Id.ImageLastseen);
                    TxtUsername = (TextView)MainView.FindViewById(Resource.Id.Txt_Username);
                    LastMessagesIcon = (ImageView)MainView.FindViewById(Resource.Id.IconLastMessages);
                    TxtLastMessages = (TextView)MainView.FindViewById(Resource.Id.Txt_LastMessages);
                    TxtTimestamp = (TextView)MainView.FindViewById(Resource.Id.Txt_timestamp);
                    IconCheckCountMessages = (ImageView)MainView.FindViewById(Resource.Id.IconCheckRead);

                    if (!AppSettings.EnableAudioVideoCall)
                        CallButton.Visibility = ViewStates.Gone;

                    //Create an Event
                    MoreButton.Click += (sender, e) => moreClickListener(new LastMessagesClickEventArgs { View = itemView, Position = AdapterPosition });
                    CallButton.Click += (sender, e) => callClickListener(new LastMessagesClickEventArgs { View = itemView, Position = AdapterPosition });
                    DeleteButton.Click += (sender, e) => deleteClickListener(new LastMessagesClickEventArgs { View = itemView, Position = AdapterPosition });

                    itemView.Click += (sender, e) => clickListener(new LastMessagesClickEventArgs { View = itemView, Position = AdapterPosition });
                    itemView.LongClick += (sender, e) => longClickListener(new LastMessagesClickEventArgs { View = itemView, Position = AdapterPosition });
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void OnItemSelected()
            {
                try
                {
                    //MainSwipe.SetBackgroundColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#333333") : Color.ParseColor("#efefef")); 
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void OnItemClear()
            {
                try
                {
                    //MainSwipe.SetBackgroundColor(Color.Transparent);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public class LastMessagesClickEventArgs : EventArgs
        {
            public View View { get; set; }
            public int Position { get; set; }
        }
    }
}