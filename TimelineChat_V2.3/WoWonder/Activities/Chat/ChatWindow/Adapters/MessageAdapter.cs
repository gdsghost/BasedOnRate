using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Request;
using Bumptech.Glide.Request.Target;
using Bumptech.Glide.Request.Transition;
using Com.Airbnb.Lottie;
using Com.Luseen.Autolinklibrary;
using Java.IO;
using Java.Util;
using WoWonder.Activities.Chat.Adapters;
using WoWonder.Activities.Search;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Message;
using Console = System.Console;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;
using Uri = Android.Net.Uri;
using Priority = Bumptech.Glide.Priority;

namespace WoWonder.Activities.Chat.ChatWindow.Adapters
{
    public class MessageAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<Holders.MesClickEventArgs> ItemClick;
        public event EventHandler<Holders.MesClickEventArgs> ItemLongClick;

        public ObservableCollection<AdapterModelsClassMessage> DifferList = new ObservableCollection<AdapterModelsClassMessage>();

        public readonly Activity MainActivity;
        private readonly RequestOptions Options;
        private readonly RequestBuilder FullGlideRequestBuilder;
        public readonly string Id; // to_id 
        private readonly bool ShowName;

        public int PositionSound;
        public bool MSeekBarIsTracking;
        public ValueAnimator MValueAnimator;
        public MessageDataExtra MusicBarMessageData;
         
        public MessageAdapter(Activity activity, string userid , bool showName)
        {
            try
            {
                //HasStableIds = true;
                MainActivity = activity;
                Id = userid;
                ShowName = showName;
                DifferList = new ObservableCollection<AdapterModelsClassMessage>();

                Options = new RequestOptions().Apply(RequestOptions.CircleCropTransform()
                    .CenterCrop()
                    .SetPriority(Priority.High).Override(400)
                    .SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All)
                    .Error(Resource.Drawable.ImagePlacholder)
                    .Placeholder(Resource.Drawable.ImagePlacholder));

                FullGlideRequestBuilder = Glide.With(MainActivity).AsDrawable().Apply(new RequestOptions().Apply(RequestOptions.CircleCropTransform().CenterCrop().Transform(new MultiTransformation(new CenterCrop(), new RoundedCorners(25))).SetPriority(Priority.High).Override(450).SetDiskCacheStrategy(DiskCacheStrategy.Automatic).SkipMemoryCache(true).SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All).Placeholder(new ColorDrawable(Color.ParseColor("#888888")))));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                switch (viewType)
                {
                    case (int)MessageModelType.RightProduct:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_Products, parent, false);
                            Holders.ProductViewHolder viewHolder = new Holders.ProductViewHolder(row, OnClick , OnLongClick, ShowName);
                            return viewHolder;
                        }
                    case (int)MessageModelType.LeftProduct:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_Products, parent, false);
                            Holders.ProductViewHolder viewHolder = new Holders.ProductViewHolder(row, OnClick, OnLongClick, ShowName);
                            return viewHolder;
                        } 
                    case (int)MessageModelType.RightText:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_view, parent, false);
                            Holders.TextViewHolder textViewHolder = new Holders.TextViewHolder(row, OnClick, OnLongClick, ShowName);
                            return textViewHolder;
                        }
                    case (int)MessageModelType.LeftText:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_view, parent, false);
                            Holders.TextViewHolder textViewHolder = new Holders.TextViewHolder(row, OnClick, OnLongClick, ShowName);
                            return textViewHolder;
                        }
                    case (int)MessageModelType.RightImage:
                    case (int)MessageModelType.RightGif:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_image, parent, false);
                            Holders.ImageViewHolder imageViewHolder = new Holders.ImageViewHolder(row, OnClick, OnLongClick, ShowName, Holders.TypeClick.Image);
                            return imageViewHolder;
                        }
                    case (int)MessageModelType.LeftImage:
                    case (int)MessageModelType.LeftGif:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_image, parent, false);
                            Holders.ImageViewHolder imageViewHolder = new Holders.ImageViewHolder(row, OnClick, OnLongClick, ShowName, Holders.TypeClick.Image);
                            return imageViewHolder;
                        }
                    case (int)MessageModelType.RightMap:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_image, parent, false);
                            Holders.ImageViewHolder imageViewHolder = new Holders.ImageViewHolder(row, OnClick, OnLongClick, ShowName, Holders.TypeClick.Map);
                            return imageViewHolder;
                        }
                    case (int)MessageModelType.LeftMap:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_image, parent, false);
                            Holders.ImageViewHolder imageViewHolder = new Holders.ImageViewHolder(row, OnClick, OnLongClick, ShowName, Holders.TypeClick.Map);
                            return imageViewHolder;
                        }
                    case (int)MessageModelType.RightAudio:
                        {
                            if (AppSettings.ShowMusicBar)
                            {
                                View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_AudioBar, parent, false);
                                Holders.MusicBarViewHolder soundViewHolder = new Holders.MusicBarViewHolder(row, OnClick, OnLongClick, this, ShowName);
                                return soundViewHolder;
                            }
                            else
                            {
                                View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_Audio, parent, false);
                                Holders.SoundViewHolder soundViewHolder = new Holders.SoundViewHolder(row, OnClick, OnLongClick, this, ShowName);
                                return soundViewHolder;
                            }
                        }
                    case (int)MessageModelType.LeftAudio:
                        {
                            if (AppSettings.ShowMusicBar)
                            {
                                View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_AudioBar, parent, false);
                                Holders.MusicBarViewHolder soundViewHolder = new Holders.MusicBarViewHolder(row, OnClick, OnLongClick, this, ShowName);
                                return soundViewHolder;
                            }
                            else
                            {
                                View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_Audio, parent, false);
                                Holders.SoundViewHolder soundViewHolder = new Holders.SoundViewHolder(row, OnClick, OnLongClick, this, ShowName);
                                return soundViewHolder;
                            }
                        }
                    case (int)MessageModelType.RightContact:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_Contact, parent, false);
                            Holders.ContactViewHolder contactViewHolder = new Holders.ContactViewHolder(row, OnClick, OnLongClick, this, ShowName);
                            return contactViewHolder;
                        }
                    case (int)MessageModelType.LeftContact:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_Contact, parent, false);
                            Holders.ContactViewHolder contactViewHolder = new Holders.ContactViewHolder(row, OnClick, OnLongClick, this, ShowName);
                            return contactViewHolder;
                        }
                    case (int)MessageModelType.RightVideo:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_Video, parent, false);
                            Holders.VideoViewHolder videoViewHolder = new Holders.VideoViewHolder(row, OnClick, OnLongClick, ShowName);
                            return videoViewHolder;
                        }
                    case (int)MessageModelType.LeftVideo:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_Video, parent, false);
                            Holders.VideoViewHolder videoViewHolder = new Holders.VideoViewHolder(row, OnClick, OnLongClick, ShowName);
                            return videoViewHolder;
                        }
                    case (int)MessageModelType.RightSticker:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_sticker, parent, false);
                            Holders.StickerViewHolder stickerViewHolder = new Holders.StickerViewHolder(row, OnClick, OnLongClick, ShowName);
                            return stickerViewHolder;
                        }
                    case (int)MessageModelType.LeftSticker:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_sticker, parent, false);
                            Holders.StickerViewHolder stickerViewHolder = new Holders.StickerViewHolder(row, OnClick, OnLongClick, ShowName);
                            return stickerViewHolder;
                        }
                    case (int)MessageModelType.RightFile:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Right_MS_file, parent, false);
                            Holders.FileViewHolder viewHolder = new Holders.FileViewHolder(row, OnClick, OnLongClick, ShowName);
                            return viewHolder;
                        }
                    case (int)MessageModelType.LeftFile:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_file, parent, false);
                            Holders.FileViewHolder viewHolder = new Holders.FileViewHolder(row, OnClick, OnLongClick, ShowName);
                            return viewHolder;
                        }
                    default:
                        {
                            View row = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Left_MS_view, parent, false);
                            Holders.NotSupportedViewHolder viewHolder = new Holders.NotSupportedViewHolder(row);
                            return viewHolder;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder vh, int position)
        {
            try
            {
                var item = DifferList[position];

                var itemViewType = vh.ItemViewType;
                switch (itemViewType)
                {
                    case (int)MessageModelType.RightProduct:
                    case (int)MessageModelType.LeftProduct:
                        {
                            Holders.ProductViewHolder holder = vh as Holders.ProductViewHolder;
                            LoadProductOfChatItem(holder, position, item.MesData);
                            break;
                        }
                    case (int)MessageModelType.RightGif:
                    case (int)MessageModelType.LeftGif:
                        {
                            Holders.ImageViewHolder holder = vh as Holders.ImageViewHolder;
                            LoadGifOfChatItem(holder, position, item.MesData);
                            break;
                        }
                    case (int)MessageModelType.RightText:
                    case (int)MessageModelType.LeftText:
                        {
                            Holders.TextViewHolder holder = vh as Holders.TextViewHolder;
                            LoadTextOfChatItem(holder, position, item.MesData);
                            break;
                        }
                    case (int)MessageModelType.RightImage:
                    case (int)MessageModelType.LeftImage:
                        {
                            Holders.ImageViewHolder holder = vh as Holders.ImageViewHolder;
                            LoadImageOfChatItem(holder, position, item.MesData);
                            break;
                        }
                    case (int)MessageModelType.RightMap:
                    case (int)MessageModelType.LeftMap:
                    {
                        Holders.ImageViewHolder holder = vh as Holders.ImageViewHolder;
                        LoadMapOfChatItem(holder, position, item.MesData);
                        break;
                    } 
                    case (int)MessageModelType.RightAudio:
                    case (int)MessageModelType.LeftAudio:
                        {
                            if (AppSettings.ShowMusicBar)
                            {
                                Holders.MusicBarViewHolder holder = vh as Holders.MusicBarViewHolder;
                                LoadAudioBarOfChatItem(holder, position, item.MesData);
                                break;
                            }
                            else
                            {
                                Holders.SoundViewHolder holder = vh as Holders.SoundViewHolder;
                                LoadAudioOfChatItem(holder, position, item.MesData);
                                break;
                            }
                        }
                    case (int)MessageModelType.RightContact:
                    case (int)MessageModelType.LeftContact:
                        {
                            Holders.ContactViewHolder holder = vh as Holders.ContactViewHolder;
                            LoadContactOfChatItem(holder, position, item.MesData);
                            break;
                        }
                    case (int)MessageModelType.RightVideo:
                    case (int)MessageModelType.LeftVideo:
                        {
                            Holders.VideoViewHolder holder = vh as Holders.VideoViewHolder;
                            LoadVideoOfChatItem(holder, position, item.MesData);
                            break;
                        }
                    case (int)MessageModelType.RightSticker:
                    case (int)MessageModelType.LeftSticker:
                        {
                            Holders.StickerViewHolder holder = vh as Holders.StickerViewHolder;
                            LoadStickerOfChatItem(holder, position, item.MesData);
                            break;
                        }
                    case (int)MessageModelType.RightFile:
                    case (int)MessageModelType.LeftFile:
                        {
                            Holders.FileViewHolder holder = vh as Holders.FileViewHolder;
                            LoadFileOfChatItem(holder, position, item.MesData);
                            break;
                        }
                    default:
                        {
                            if (!string.IsNullOrEmpty(item.MesData.Text) || !string.IsNullOrWhiteSpace(item.MesData.Text))
                            {
                                if (vh is Holders.TextViewHolder holderText)
                                {
                                    LoadTextOfChatItem(holderText, position, item.MesData); 
                                }
                            }
                            else
                            {
                                if (vh is Holders.NotSupportedViewHolder holder)
                                    holder.AutoLinkNotsupportedView.Text = MainActivity.GetText(Resource.String.Lbl_TextChatNotSupported);
                            }
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position, IList<Object> payloads)
        {
            try
            {
                if (payloads.Count > 0)
                {
                    var item = DifferList[position]; 
                    switch (payloads[0].ToString())
                    {
                        case "WithoutBlobImage":
                        case "WithoutBlobGIF":
                            {
                                if (viewHolder is Holders.ImageViewHolder holder)
                                    LoadImageOfChatItem(holder, position, item.MesData);
                                //NotifyItemChanged(position);
                                break;
                            }
                        case "WithoutBlobVideo":
                            {
                                if (viewHolder is Holders.VideoViewHolder holder)
                                    LoadVideoOfChatItem(holder, position, item.MesData);
                                //NotifyItemChanged(position);
                                break;
                            }
                        case "WithoutBlobAudio":
                            {
                                if (AppSettings.ShowMusicBar)
                                {
                                    if (viewHolder is Holders.MusicBarViewHolder holder)
                                    {
                                        if (item.MesData.Position == "right")
                                        {
                                            SetSeenMessage(holder.Seen, item.MesData.Seen);
                                            holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(item.MesData.ChatColor));
                                        }

                                        SetStartedMessage(holder.StarIcon, holder.StarImage, item.MesData.IsStarted);

                                        if (item.MesData.SendFile)
                                        {
                                            holder.LoadingProgressview.Indeterminate = true;
                                            holder.LoadingProgressview.Visibility = ViewStates.Visible;
                                            holder.PlayButton.Visibility = ViewStates.Gone;
                                        }
                                        else
                                        {
                                            holder.LoadingProgressview.Indeterminate = false;
                                            holder.LoadingProgressview.Visibility = ViewStates.Gone;
                                            holder.PlayButton.Visibility = ViewStates.Visible;
                                        } 
                                    }
                                    //NotifyItemChanged(position);
                                    break;
                                }
                                else
                                {
                                    if (viewHolder is Holders.SoundViewHolder holder)
                                        LoadAudioOfChatItem(holder, position, item.MesData);
                                    //NotifyItemChanged(position);
                                    break;
                                }
                            }
                        case "WithoutBlobFile":
                            {
                                if (viewHolder is Holders.FileViewHolder holder)
                                    LoadFileOfChatItem(holder, position, item.MesData);
                                //NotifyItemChanged(position);
                                break;
                            }
                        default:
                            base.OnBindViewHolder(viewHolder, position, payloads);
                            break;
                    }
                }
                else
                {
                    base.OnBindViewHolder(viewHolder, position, payloads);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                base.OnBindViewHolder(viewHolder, position, payloads);
            }
        }

        #region Function Load Message

        private void SetStartedMessage(LottieAnimationView favAnimationView, ImageView starImage, bool star)
        {
            try
            {
                if (favAnimationView != null)
                {
                    if (star)
                    {
                        favAnimationView.PlayAnimation();
                        favAnimationView.Visibility = ViewStates.Visible;
                        starImage.SetImageResource(Resource.Drawable.icon_star_filled_post_vector);
                        starImage.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        favAnimationView.Progress = 0;
                        favAnimationView.CancelAnimation();
                        favAnimationView.Visibility = ViewStates.Gone;
                        //starImage.SetImageResource(Resource.Drawable.icon_fav_post_vector);
                        starImage.Visibility = ViewStates.Gone;
                    } 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        
        private void SetSeenMessage(TextView view , string seen)
        {
            try
            {
                if (view != null)
                {
                    if (seen == "0")
                    {
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, view, FontAwesomeIcon.Check);
                        view.SetTextColor(Color.ParseColor("#efefef"));
                    } 
                    else  
                    {
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, view, FontAwesomeIcon.CheckDouble);
                        view.SetTextColor(Color.Blue);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadTextOfChatItem(Holders.TextViewHolder holder, int position, MessageData message)
        {
            try
            {
                Console.WriteLine(position);
                holder.Time.Text = message.TimeText;

                if (holder.UserName != null && ShowName)
                {
                    holder.UserName.Text = WoWonderTools.GetNameFinal(message.UserData);
                    holder.UserName.Visibility = ViewStates.Visible;
                }

                if (message.Position == "right")
                {
                    SetSeenMessage(holder.Seen, message.Seen);

                    if (!ShowName)
                        holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor));
                }

                if (!ShowName)
                    SetStartedMessage(holder.StarIcon, holder.StarImage, message.IsStarted);
                 
                holder.Time.Visibility = message.ShowTimeText ? ViewStates.Visible : ViewStates.Gone;

                if (message.ModelType == MessageModelType.LeftText)
                {
                    holder.AutoLinkTextView.AddAutoLinkMode(AutoLinkMode.ModePhone, AutoLinkMode.ModeEmail, AutoLinkMode.ModeHashtag, AutoLinkMode.ModeUrl, AutoLinkMode.ModeMention);
                    holder.AutoLinkTextView.SetPhoneModeColor(ContextCompat.GetColor(MainActivity, Resource.Color.left_ModePhone_color));
                    holder.AutoLinkTextView.SetEmailModeColor(ContextCompat.GetColor(MainActivity, Resource.Color.left_ModeEmail_color));
                    holder.AutoLinkTextView.SetHashtagModeColor(ContextCompat.GetColor(MainActivity, Resource.Color.left_ModeHashtag_color));
                    holder.AutoLinkTextView.SetUrlModeColor(ContextCompat.GetColor(MainActivity, Resource.Color.left_ModeUrl_color));
                    holder.AutoLinkTextView.SetMentionModeColor(ContextCompat.GetColor(MainActivity, Resource.Color.left_ModeMention_color));
                }
                else
                {
                    holder.AutoLinkTextView.AddAutoLinkMode(AutoLinkMode.ModePhone, AutoLinkMode.ModeEmail, AutoLinkMode.ModeHashtag, AutoLinkMode.ModeUrl, AutoLinkMode.ModeMention);
                    holder.AutoLinkTextView.SetPhoneModeColor(ContextCompat.GetColor(MainActivity, Resource.Color.right_ModePhone_color));
                    holder.AutoLinkTextView.SetEmailModeColor(ContextCompat.GetColor(MainActivity, Resource.Color.right_ModeEmail_color));
                    holder.AutoLinkTextView.SetHashtagModeColor(ContextCompat.GetColor(MainActivity, Resource.Color.right_ModeHashtag_color));
                    holder.AutoLinkTextView.SetUrlModeColor(ContextCompat.GetColor(MainActivity, Resource.Color.right_ModeUrl_color));
                    holder.AutoLinkTextView.SetMentionModeColor(ContextCompat.GetColor(MainActivity, Resource.Color.right_ModeMention_color));
                }
                holder.AutoLinkTextView.AutoLinkOnClick += AutoLinkTextViewOnAutoLinkOnClick;

                holder.AutoLinkTextView.SetAutoLinkText(message.Text); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AutoLinkTextViewOnAutoLinkOnClick(object sender1, AutoLinkOnClickEventArgs autoLinkOnClickEventArgs)
        {
            try
            {
                var typetext = Methods.FunString.Check_Regex(autoLinkOnClickEventArgs.P1);
                if (typetext == "Email")
                {
                    Methods.App.SendEmail(MainActivity, autoLinkOnClickEventArgs.P1);
                }
                else if (typetext == "Website")
                {
                    var url = autoLinkOnClickEventArgs.P1;
                    if (!autoLinkOnClickEventArgs.P1.Contains("http"))
                    {
                        url = "http://" + autoLinkOnClickEventArgs.P1;
                    }

                    Methods.App.OpenWebsiteUrl(MainActivity, url);
                }
                else if (typetext == "Hashtag")
                {

                }
                else if (typetext == "Mention")
                {
                    var intent = new Intent(MainActivity, typeof(SearchTabbedActivity));
                    intent.PutExtra("Key", autoLinkOnClickEventArgs.P1.Replace("@", ""));
                    MainActivity.StartActivity(intent);
                }
                else if (typetext == "Number")
                {
                    Methods.App.SaveContacts(MainActivity, autoLinkOnClickEventArgs.P1, "", "2");
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void LoadMapOfChatItem(Holders.ImageViewHolder holder, int position, MessageData message)
        {
            try
            {
                if (holder.UserName != null && ShowName)
                {
                    holder.UserName.Text = WoWonderTools.GetNameFinal(message.UserData);
                    holder.UserName.Visibility = ViewStates.Visible;
                }

                if (message.Position == "right")
                {
                    SetSeenMessage(holder.Seen, message.Seen);

                    if (!ShowName)
                        holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor));
                }

                if (!ShowName)
                    SetStartedMessage(holder.StarIcon, holder.StarImage, message.IsStarted);

                holder.Time.Text = message.TimeText;
                 
                LatLng latLng = new LatLng(Convert.ToDouble(message.Lat) , Convert.ToDouble(message.Lng));

                var addresses = await WoWonderTools.ReverseGeocodeCurrentLocation(latLng);
                if (addresses != null)
                { 
                    var deviceAddress = addresses.GetAddressLine(0);
                      
                    string imageUrlMap = "https://maps.googleapis.com/maps/api/staticmap?";
                    //imageUrlMap += "center=" + item.CurrentLatitude + "," + item.CurrentLongitude;
                    imageUrlMap += "center=" + deviceAddress;
                    imageUrlMap += "&zoom=13";
                    imageUrlMap += "&scale=2";
                    imageUrlMap += "&size=150x150";
                    imageUrlMap += "&maptype=roadmap";
                    imageUrlMap += "&key=" + MainActivity.GetText(Resource.String.google_maps_key);
                    imageUrlMap += "&format=png";
                    imageUrlMap += "&visual_refresh=true";
                    imageUrlMap += "&markers=size:small|color:0xff0000|label:1|" + deviceAddress;
                     
                    FullGlideRequestBuilder.Load(imageUrlMap).Into(holder.ImageView);
                }
                 
                holder.LoadingProgressview.Indeterminate = false;
                holder.LoadingProgressview.Visibility = ViewStates.Gone; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
          
        private void LoadImageOfChatItem(Holders.ImageViewHolder holder, int position, MessageData message)
        {
            try
            {
                if (holder.UserName != null && ShowName)
                {
                    holder.UserName.Text = WoWonderTools.GetNameFinal(message.UserData);
                    holder.UserName.Visibility = ViewStates.Visible;
                }

                if (message.Position == "right")
                {
                    SetSeenMessage(holder.Seen, message.Seen);

                    if (!ShowName)
                        holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor));
                }

                if (!ShowName)
                    SetStartedMessage(holder.StarIcon, holder.StarImage, message.IsStarted);

                Console.WriteLine(position);
                var fileName = message.Media.Split('/').Last();
                message.Media = WoWonderTools.GetFile(Id, Methods.Path.FolderDcimImage, fileName, message.Media);

                holder.Time.Text = message.TimeText;

                if (message.Media.Contains("http"))
                {
                    GlideImageLoader.LoadImage(MainActivity, message.Media, holder.ImageView, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                }
                else
                {
                    var file = Uri.FromFile(new File(message.Media));
                    // GlideImageLoader.LoadImage(MainActivity, file.ToString(), holder.ImageView, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                    Glide.With(MainActivity).Load(file.Path).Apply(GlideImageLoader.GetRequestOptions(ImageStyle.RoundedCrop, ImagePlaceholders.Drawable)).Into(holder.ImageView);
                }

                holder.LoadingProgressview.Indeterminate = false;
                holder.LoadingProgressview.Visibility = ViewStates.Gone; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadProductOfChatItem(Holders.ProductViewHolder holder, int position, MessageDataExtra message)
        {
            try
            {
                if (holder.UserName != null && ShowName)
                {
                    holder.UserName.Text = WoWonderTools.GetNameFinal(message.UserData);
                    holder.UserName.Visibility = ViewStates.Visible;
                }

                if (message.Position == "right")
                {
                    SetSeenMessage(holder.Seen, message.Seen);

                    if (!ShowName)
                        holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor));
                }

                if (!ShowName)
                    SetStartedMessage(holder.StarIcon, holder.StarImage, message.IsStarted);

                Console.WriteLine(position);
                string imageUrl = !string.IsNullOrEmpty(message.Media) ? message.Media : message.Product?.ProductClass?.Images[0]?.Image;
                holder.Time.Text = message.TimeText;

                holder.Title.Text = message.Product?.ProductClass?.Name;
                holder.Cat.Text = CategoriesController.ListCategoriesProducts.FirstOrDefault(a => a.CategoriesId == message.Product?.ProductClass?.Category)?.CategoriesName;

                var (currency, currencyIcon) = WoWonderTools.GetCurrency(message.Product?.ProductClass?.Currency);
                holder.Price.Text = currencyIcon + " " + message.Product?.ProductClass?.Price;
                Console.WriteLine(currency);

                if (imageUrl != null && (imageUrl.Contains("http://") || imageUrl.Contains("https://")))
                {
                    GlideImageLoader.LoadImage(MainActivity, imageUrl, holder.ImageView, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                }
                else
                {
                    var file = Uri.FromFile(new File(imageUrl));
                    Glide.With(MainActivity).Load(file.Path).Apply(GlideImageLoader.GetRequestOptions(ImageStyle.RoundedCrop, ImagePlaceholders.Drawable)).Into(holder.ImageView);
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadAudioBarOfChatItem(Holders.MusicBarViewHolder musicBarViewHolder, int position, MessageDataExtra message)
        {
            try
            {
                if (musicBarViewHolder.UserName != null && ShowName)
                {
                    musicBarViewHolder.UserName.Text = WoWonderTools.GetNameFinal(message.UserData);
                    musicBarViewHolder.UserName.Visibility = ViewStates.Visible;
                }

                if (message.Position == "right")
                {
                    SetSeenMessage(musicBarViewHolder.Seen, message.Seen);

                    if (!ShowName)
                        musicBarViewHolder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor));
                }

                if (!ShowName)
                    SetStartedMessage(musicBarViewHolder.StarIcon, musicBarViewHolder.StarImage, message.IsStarted);

                if (message.SendFile)
                {
                    musicBarViewHolder.LoadingProgressview.Indeterminate = true;
                    musicBarViewHolder.LoadingProgressview.IndeterminateDrawable.SetColorFilter(new PorterDuffColorFilter(Color.White, PorterDuff.Mode.Multiply));
                    musicBarViewHolder.LoadingProgressview.Visibility = ViewStates.Visible;
                    musicBarViewHolder.PlayButton.Visibility = ViewStates.Gone;
                }
                else
                {
                    musicBarViewHolder.LoadingProgressview.Indeterminate = false;
                    musicBarViewHolder.LoadingProgressview.Visibility = ViewStates.Gone;
                    musicBarViewHolder.PlayButton.Visibility = ViewStates.Visible;
                }

                musicBarViewHolder.MsgTimeTextView.Text = message.TimeText;

                var fileName = message.Media.Split('/').Last();

                var mediaFile = WoWonderTools.GetFile(Id, Methods.Path.FolderDcimSound, fileName, message.Media);

                var duration = WoWonderTools.GetDuration(mediaFile);
                if (string.IsNullOrEmpty(message.MediaDuration) || message.MediaDuration == "00:00")
                {
                    musicBarViewHolder.DurationTextView.Text = Methods.AudioRecorderAndPlayer.GetTimeString(duration);
                }
                else
                    musicBarViewHolder.DurationTextView.Text = message.MediaDuration;

                if (mediaFile.Contains("http"))
                    mediaFile = WoWonderTools.GetFile(Id, Methods.Path.FolderDcimSound, fileName, message.Media);

                musicBarViewHolder.FixedMusicBar.LoadFrom(mediaFile, Convert.ToInt32(duration));
                musicBarViewHolder.FixedMusicBar.Show();

                if (message.MediaIsPlaying)
                {
                    if (message.ModelType == MessageModelType.LeftAudio)
                        musicBarViewHolder.PlayButton.SetImageResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.ic_media_pause_light : Resource.Drawable.ic_media_pause_dark);
                    else
                        musicBarViewHolder.PlayButton.SetImageResource(Resource.Drawable.ic_media_pause_light);
                }
                else
                {
                    musicBarViewHolder.PlayButton.SetImageResource(message.ModelType == MessageModelType.LeftAudio ? Resource.Drawable.ic_play_dark_arrow : Resource.Drawable.ic_play_arrow);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadAudioOfChatItem(Holders.SoundViewHolder soundViewHolder, int position, MessageDataExtra message)
        {
            try
            {
                if (soundViewHolder.UserName != null && ShowName)
                {
                    soundViewHolder.UserName.Text = WoWonderTools.GetNameFinal(message.UserData);
                    soundViewHolder.UserName.Visibility = ViewStates.Visible;
                }

                if (message.Position == "right")
                {
                    SetSeenMessage(soundViewHolder.Seen, message.Seen);

                    if (!ShowName)
                        soundViewHolder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor));
                }

                if (!ShowName)
                    SetStartedMessage(soundViewHolder.StarIcon, soundViewHolder.StarImage, message.IsStarted);

                if (message.SendFile)
                {
                    soundViewHolder.LoadingProgressview.Indeterminate = true;
                    soundViewHolder.LoadingProgressview.IndeterminateDrawable.SetColorFilter(new PorterDuffColorFilter(Color.White, PorterDuff.Mode.Multiply));
                    soundViewHolder.LoadingProgressview.Visibility = ViewStates.Visible;
                    soundViewHolder.PlayButton.Visibility = ViewStates.Gone;
                }
                else
                {
                    soundViewHolder.LoadingProgressview.Indeterminate = false;
                    soundViewHolder.LoadingProgressview.Visibility = ViewStates.Gone;
                    soundViewHolder.PlayButton.Visibility = ViewStates.Visible;
                }

                soundViewHolder.MsgTimeTextView.Text = message.TimeText;

                var fileName = message.Media.Split('/').Last();

                var mediaFile = WoWonderTools.GetFile(Id, Methods.Path.FolderDcimSound, fileName, message.Media);
                if (string.IsNullOrEmpty(message.MediaDuration) || message.MediaDuration == "00:00")
                {
                    var duration = WoWonderTools.GetDuration(mediaFile);
                    soundViewHolder.DurationTextView.Text = Methods.AudioRecorderAndPlayer.GetTimeString(duration);
                }
                else
                    soundViewHolder.DurationTextView.Text = message.MediaDuration;
                 
                if (message.MediaIsPlaying)
                {
                    if (message.ModelType == MessageModelType.LeftAudio)
                        soundViewHolder.PlayButton.SetImageResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.ic_media_pause_light : Resource.Drawable.ic_media_pause_dark);
                    else
                        soundViewHolder.PlayButton.SetImageResource(Resource.Drawable.ic_media_pause_light);
                }
                else
                {
                    soundViewHolder.PlayButton.SetImageResource(message.ModelType == MessageModelType.LeftAudio ? Resource.Drawable.ic_play_dark_arrow : Resource.Drawable.ic_play_arrow);
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
          
        private void LoadContactOfChatItem(Holders.ContactViewHolder holder, int position, MessageDataExtra message)
        {
            try
            {
                if (holder.UserName != null && ShowName)
                {
                    holder.UserName.Text = WoWonderTools.GetNameFinal(message.UserData);
                    holder.UserName.Visibility = ViewStates.Visible;
                }

                if (message.Position == "right")
                {
                    SetSeenMessage(holder.Seen, message.Seen);

                    if (!ShowName)
                        holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor));
                }

                if (!ShowName)
                    SetStartedMessage(holder.StarIcon, holder.StarImage, message.IsStarted);

                Console.WriteLine(position);
                holder.MsgTimeTextView.Text = message.TimeText;
                holder.MsgTimeTextView.Visibility = message.ShowTimeText ? ViewStates.Visible : ViewStates.Gone;

                if (!string.IsNullOrEmpty(message.ContactName))
                {
                    holder.UserContactNameTextView.Text = message.ContactName;
                    holder.UserNumberTextView.Text = message.ContactNumber;
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadVideoOfChatItem(Holders.VideoViewHolder holder, int position, MessageDataExtra message)
        {
            try
            {
                if (holder.UserName != null && ShowName)
                {
                    holder.UserName.Text = WoWonderTools.GetNameFinal(message.UserData);
                    holder.UserName.Visibility = ViewStates.Visible;
                }

                if (message.Position == "right")
                {
                    SetSeenMessage(holder.Seen, message.Seen);

                    if (!ShowName)
                        holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor));
                }

                if (!ShowName)
                    SetStartedMessage(holder.StarIcon, holder.StarImage, message.IsStarted);

                var fileName = message.Media.Split('/').Last();
                var fileNameWithoutExtension = fileName.Split('.').First();
                var mediaFile = WoWonderTools.GetFile(Id, Methods.Path.FolderDcimVideo, fileName, message.Media);

                holder.MsgTimeTextView.Text = message.TimeText;
                holder.FilenameTextView.Text = Methods.FunString.SubStringCutOf(fileNameWithoutExtension, 12) + ".mp4";

                var videoImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDcimVideo + Id, fileNameWithoutExtension + ".png");
                if (videoImage == "File Dont Exists")
                {
                    File file2 = new File(mediaFile);  
                    try
                    {
                        Uri photoUri = message.Media.Contains("http") ? Uri.Parse(message.Media) : FileProvider.GetUriForFile(MainActivity, MainActivity.PackageName + ".fileprovider", file2);
                        Glide.With(MainActivity)
                            .AsBitmap()
                            .Apply(GlideImageLoader.GetRequestOptions(ImageStyle.RoundedCrop, ImagePlaceholders.Drawable))
                            .Load(photoUri) // or URI/path
                            .Into(new MySimpleTarget(this, holder, position));  //image view to set thumbnail to 
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                        Glide.With(MainActivity)
                            .AsBitmap()
                            .Apply(GlideImageLoader.GetRequestOptions(ImageStyle.RoundedCrop, ImagePlaceholders.Drawable))
                            .Load(file2) // or URI/path
                            .Into(new MySimpleTarget(this, holder, position));  //image view to set thumbnail to 
                    } 
                }
                else
                {
                    File file = new File(videoImage);
                    try
                    {
                        Uri photoUri = FileProvider.GetUriForFile(MainActivity, MainActivity.PackageName + ".fileprovider", file);
                        Glide.With(MainActivity).Load(photoUri).Apply(GlideImageLoader.GetRequestOptions(ImageStyle.RoundedCrop, ImagePlaceholders.Drawable)).Into(holder.ImageView);
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                        Glide.With(MainActivity).Load(file).Apply(GlideImageLoader.GetRequestOptions(ImageStyle.RoundedCrop, ImagePlaceholders.Drawable)).Into(holder.ImageView);
                    }  
                }

                if (message.SendFile)
                {
                    holder.LoadingProgressview.Indeterminate = true;
                    holder.LoadingProgressview.Visibility = ViewStates.Visible;
                    holder.PlayButton.Visibility = ViewStates.Gone;
                }
                else
                {
                    holder.LoadingProgressview.Indeterminate = false;
                    holder.LoadingProgressview.Visibility = ViewStates.Gone;
                    holder.PlayButton.Visibility = ViewStates.Visible;
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class MySimpleTarget : CustomTarget
        {
            private readonly MessageAdapter MAdapter;
            private readonly Holders.VideoViewHolder ViewHolder;
            private readonly int Position;
            public MySimpleTarget(MessageAdapter adapter, Holders.VideoViewHolder viewHolder, int position)
            {
                try
                {
                    MAdapter = adapter;
                    ViewHolder = viewHolder;
                    Position = position;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnResourceReady(Object resource, ITransition transition)
            {
                try
                {
                    if (MAdapter.DifferList?.Count > 0)
                    {
                        var item = MAdapter.DifferList[Position].MesData;
                        if (item != null)
                        {
                            var fileName = item.Media.Split('/').Last();
                            var fileNameWithoutExtension = fileName.Split('.').First();

                            var pathImage = Methods.Path.FolderDcimVideo + MAdapter.Id + "/" + fileNameWithoutExtension + ".png";

                            var videoImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDcimVideo + MAdapter.Id, fileNameWithoutExtension + ".png");
                            if (videoImage == "File Dont Exists")
                            {
                                if (resource is Bitmap bitmap)
                                {
                                    Methods.MultiMedia.Export_Bitmap_As_Image(bitmap, fileNameWithoutExtension, Methods.Path.FolderDcimVideo + MAdapter.Id + "/");

                                    File file2 = new File(pathImage);
                                    var photoUri = FileProvider.GetUriForFile(MAdapter.MainActivity, MAdapter.MainActivity.PackageName + ".fileprovider", file2);

                                    Glide.With(MAdapter.MainActivity).Load(photoUri).Apply(GlideImageLoader.GetRequestOptions(ImageStyle.RoundedCrop, ImagePlaceholders.Drawable)).Into(ViewHolder.ImageView);

                                    item.ImageVideo = photoUri.ToString();
                                }
                            }
                            else
                            {
                                File file2 = new File(pathImage);
                                var photoUri = FileProvider.GetUriForFile(MAdapter.MainActivity, MAdapter.MainActivity.PackageName + ".fileprovider", file2);

                                Glide.With(MAdapter.MainActivity).Load(photoUri).Apply(GlideImageLoader.GetRequestOptions(ImageStyle.RoundedCrop, ImagePlaceholders.Drawable)).Into(ViewHolder.ImageView);

                                item.ImageVideo = photoUri.ToString();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnLoadCleared(Drawable p0) { }
        }

        private void LoadStickerOfChatItem(Holders.StickerViewHolder holder, int position, MessageDataExtra message)
        {
            try
            { 
                if (holder.UserName != null && ShowName)
                {
                    holder.UserName.Text = WoWonderTools.GetNameFinal(message.UserData);
                    holder.UserName.Visibility = ViewStates.Visible;
                }

                if (message.Position == "right")
                {
                    SetSeenMessage(holder.Seen, message.Seen); 
                }

                if (!ShowName)
                    SetStartedMessage(holder.StarIcon, holder.StarImage, message.IsStarted);

                Console.WriteLine(position);
                var fileName = message.Media.Split('/').Last();
                message.Media = WoWonderTools.GetFile(Id, Methods.Path.FolderDiskSticker, fileName, message.Media);

                holder.Time.Text = message.TimeText;
                if (message.Media.Contains("http"))
                {
                    Glide.With(MainActivity).Load(message.Media).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder)).Into(holder.ImageView);
                }
                else
                {
                    var file = Uri.FromFile(new File(message.Media));
                    Glide.With(MainActivity).Load(file.Path).Apply(Options).Into(holder.ImageView);
                    //Glide.With(MainActivity).Load(file.Path).Apply(GlideImageLoader.GetRequestOptions(ImageStyle.RoundedCrop, ImagePlaceholders.Drawable)).Into(holder.ImageView);
                }

                holder.LoadingProgressview.Indeterminate = false;
                holder.LoadingProgressview.Visibility = ViewStates.Gone; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadFileOfChatItem(Holders.FileViewHolder holder, int position, MessageDataExtra message)
        {
            try
            {
                if (holder.UserName != null && ShowName)
                {
                    holder.UserName.Text = WoWonderTools.GetNameFinal(message.UserData);
                    holder.UserName.Visibility = ViewStates.Visible;
                }

                if (message.Position == "right")
                {
                    SetSeenMessage(holder.Seen, message.Seen);

                    if (!ShowName)
                        holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(message.ChatColor));
                }

                if (!ShowName)
                    SetStartedMessage(holder.StarIcon, holder.StarImage, message.IsStarted);

                Console.WriteLine(position);
                var fileName = message.Media.Split('/').Last();
                var fileNameWithoutExtension = fileName.Split('.').First();
                var fileNameExtension = fileName.Split('.').Last();

                message.Media = WoWonderTools.GetFile(Id, Methods.Path.FolderDcimFile, fileName, message.Media);

                holder.MsgTimeTextView.Text = message.TimeText;
                holder.FileNameTextView.Text = Methods.FunString.SubStringCutOf(fileNameWithoutExtension, 10) + fileNameExtension;
                holder.SizeFileTextView.Text = message.FileSize;

                if (fileNameExtension.Contains("rar") || fileNameExtension.Contains("RAR") || fileNameExtension.Contains("zip") || fileNameExtension.Contains("ZIP"))
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, holder.IconTypefile, "\uf1c6"); //ZipBox
                }
                else if (fileNameExtension.Contains("txt") || fileNameExtension.Contains("TXT"))
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, holder.IconTypefile, "\uf15c"); //NoteText
                }
                else if (fileNameExtension.Contains("docx") || fileNameExtension.Contains("DOCX") || fileNameExtension.Contains("doc") || fileNameExtension.Contains("DOC"))
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, holder.IconTypefile, "\uf1c2"); //FileWord
                }
                else if (fileNameExtension.Contains("pdf") || fileNameExtension.Contains("PDF"))
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, holder.IconTypefile, "\uf1c1"); //FilePdf
                }
                else if (fileNameExtension.Contains("apk") || fileNameExtension.Contains("APK"))
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, holder.IconTypefile, "\uf17b"); //Fileandroid
                }
                else
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, holder.IconTypefile, "\uf15b"); //file
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadGifOfChatItem(Holders.ImageViewHolder holder, int position, MessageDataExtra item)
        {
            try
            {
                if (holder.UserName != null && ShowName)
                {
                    holder.UserName.Text = WoWonderTools.GetNameFinal(item.UserData);
                    holder.UserName.Visibility = ViewStates.Visible;
                }

                if (item.Position == "right")
                {
                    SetSeenMessage(holder.Seen, item.Seen);

                    if (!ShowName)
                        holder.BubbleLayout.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(item.ChatColor));
                }

                if (!ShowName)
                    SetStartedMessage(holder.StarIcon, holder.StarImage, item.IsStarted);

                Console.WriteLine(position);
                // G_fixed_height_small_url, // UrlGif - view  >>  mediaFileName
                // G_fixed_height_small_mp4, //MediaGif - sent >>  media

                string imageUrl = "";
                if (!string.IsNullOrEmpty(item.Stickers))
                    imageUrl = item.Stickers;
                else if (!string.IsNullOrEmpty(item.Media))
                    imageUrl = item.Media;
                else if (!string.IsNullOrEmpty(item.MediaFileName))
                    imageUrl = item.MediaFileName;

                string[] fileName = imageUrl.Split(new[] { "/", "200.gif?cid=", "&rid=200" }, StringSplitOptions.RemoveEmptyEntries);
                var lastFileName = fileName.Last();
                var name = fileName[3] + lastFileName;

                item.Media = WoWonderTools.GetFile(Id, Methods.Path.FolderDiskGif, name, imageUrl);

                if (item.Media != null && item.Media.Contains("http"))
                {
                    GlideImageLoader.LoadImage(MainActivity, imageUrl, holder.ImageView, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                }
                else
                {
                    var file = Uri.FromFile(new File(item.Media));
                    Glide.With(MainActivity).Load(file.Path).Apply(GlideImageLoader.GetRequestOptions(ImageStyle.RoundedCrop, ImagePlaceholders.Drawable)).Into(holder.ImageView);
                    //Glide.With(MainActivity).Load(file.Path).Apply(Options).Into(holder.ImageGifView);
                }

                holder.LoadingProgressview.Indeterminate = false;
                holder.LoadingProgressview.Visibility = ViewStates.Gone; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        public override int ItemCount => DifferList?.Count ?? 0;

        public AdapterModelsClassMessage GetItem(int position)
        {
            var item = DifferList[position];

            return item;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                var item = DifferList[position];
                if (item == null)
                    return (int)MessageModelType.None;

                switch (item.TypeView)
                {
                    case MessageModelType.RightProduct:
                        return (int)MessageModelType.RightProduct;
                    case MessageModelType.LeftProduct:
                        return (int)MessageModelType.LeftProduct;
                    case MessageModelType.RightGif:
                        return (int)MessageModelType.RightGif;
                    case MessageModelType.LeftGif:
                        return (int)MessageModelType.LeftGif;
                    case MessageModelType.RightText:
                        return (int)MessageModelType.RightText;
                    case MessageModelType.LeftText:
                        return (int)MessageModelType.LeftText;
                    case MessageModelType.RightImage:
                        return (int)MessageModelType.RightImage;
                    case MessageModelType.LeftImage:
                        return (int)MessageModelType.LeftImage;
                    case MessageModelType.RightAudio:
                        return (int)MessageModelType.RightAudio;
                    case MessageModelType.LeftAudio:
                        return (int)MessageModelType.LeftAudio;
                    case MessageModelType.RightContact:
                        return (int)MessageModelType.RightContact;
                    case MessageModelType.LeftContact:
                        return (int)MessageModelType.LeftContact;
                    case MessageModelType.RightVideo:
                        return (int)MessageModelType.RightVideo;
                    case MessageModelType.LeftVideo:
                        return (int)MessageModelType.LeftVideo;
                    case MessageModelType.RightSticker:
                        return (int)MessageModelType.RightSticker;
                    case MessageModelType.LeftSticker:
                        return (int)MessageModelType.LeftSticker;
                    case MessageModelType.RightFile:
                        return (int)MessageModelType.RightFile;
                    case MessageModelType.LeftFile:
                        return (int)MessageModelType.LeftFile;
                    case MessageModelType.RightMap:
                        return (int)MessageModelType.RightMap;
                    case MessageModelType.LeftMap:
                        return (int)MessageModelType.LeftMap;
                    default:
                        return (int)MessageModelType.None;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return (int)MessageModelType.None;
            }
        }

        void OnClick(Holders.MesClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(Holders.MesClickEventArgs args) => ItemLongClick?.Invoke(this, args);
         
        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = DifferList[p0];
                if (item == null)
                    return Collections.SingletonList(p0);
                 
                string imageUrl = "";

                switch (item.TypeView)
                {
                    case MessageModelType.LeftProduct:
                    case MessageModelType.RightProduct:
                        imageUrl = !string.IsNullOrEmpty(item.MesData.Media) ? item.MesData.Media : item.MesData.Product?.ProductClass?.Images[0]?.Image;
                        break; 
                    case MessageModelType.RightGif: 
                    case MessageModelType.LeftGif: 
                        if (!string.IsNullOrEmpty(item.MesData.Stickers))
                            imageUrl = item.MesData.Stickers;
                        else if (!string.IsNullOrEmpty(item.MesData.Media))
                            imageUrl = item.MesData.Media;
                        else if (!string.IsNullOrEmpty(item.MesData.MediaFileName))
                            imageUrl = item.MesData.MediaFileName;

                        string[] fileName = imageUrl.Split(new[] { "/", "200.gif?cid=", "&rid=200" }, StringSplitOptions.RemoveEmptyEntries);
                        var lastFileName = fileName.Last();
                        var name = fileName[3] + lastFileName;

                        imageUrl = WoWonderTools.GetFile(Id, Methods.Path.FolderDiskGif, name, imageUrl); 
                        break;
                    case MessageModelType.RightImage: 
                    case MessageModelType.LeftImage: 
                        imageUrl = WoWonderTools.GetFile(Id, Methods.Path.FolderDcimImage, item.MesData.Media.Split('/').Last(), item.MesData.Media);
                        break;
                    case MessageModelType.RightVideo: 
                    case MessageModelType.LeftVideo: 
                        imageUrl = WoWonderTools.GetFile(Id, Methods.Path.FolderDcimVideo, item.MesData.Media.Split('/').Last(), imageUrl);
                        break;
                    case MessageModelType.RightSticker: 
                    case MessageModelType.LeftSticker: 
                        imageUrl = WoWonderTools.GetFile(Id, Methods.Path.FolderDiskSticker, item.MesData.Media.Split('/').Last(), item.MesData.Media);
                        break;
                    case MessageModelType.RightMap: 
                    case MessageModelType.LeftMap:
                        if (item.MesData.MessageMap.Contains("https://maps.googleapis.com/maps/api/staticmap?"))
                            imageUrl = item.MesData.MessageMap; 
                        break; 
                }
                 
                if (imageUrl != "")
                    d.Add(imageUrl);
                 
                return d;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(MainActivity, p0.ToString(), ImageStyle.CenterCrop);
        }

    }
}