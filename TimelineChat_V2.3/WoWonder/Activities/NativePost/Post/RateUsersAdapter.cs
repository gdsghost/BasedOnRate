using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;

using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Java.Util;
using Refractored.Controls;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using Exception = System.Exception;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.NativePost.Post
{
    public class RateUsersAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public enum TypeTextSecondary
        {
            About,
            LastSeen,
            None
        }
        public event EventHandler<RateUsersAdapterClickEventArgs> FollowButtonItemClick;
        public event EventHandler<RateUsersAdapterClickEventArgs> ItemClick;
        public event EventHandler<RateUsersAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<UserDataObject> UserList = new ObservableCollection<UserDataObject>();
        private readonly bool ShowButton;
        private readonly TypeTextSecondary Type;

        public RateUsersAdapter(Activity activity, bool showButton, TypeTextSecondary type)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = activity;
                ShowButton = showButton;
                Type = type;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => UserList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_HRateUser_view
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_HRateUser_view, parent, false);
                var vh = new RateUsersAdapterViewHolder(itemView, FollowButtonClick, Click, LongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is RateUsersAdapterViewHolder holder)
                {
                    var item = UserList[position];
                    if (item != null)
                    {
                        Initialize(holder, item);

                        //if (!ShowButton)
                        //    holder.Button.Visibility = ViewStates.Gone;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Initialize(RateUsersAdapterViewHolder holder, UserDataObject users)
        {
            try
            {
                GlideImageLoader.LoadImage(ActivityContext, users.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable, true);

                holder.Name.Text = Methods.FunString.SubStringCutOf(WoWonderTools.GetNameFinal(users), 20);
                holder.UserRatingBar.Rating = (float)Convert.ToDouble(users.Points);
                //if (users.Verified == "1")
                //    holder.Name.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.icon_checkmark_small_vector, 0);

                //if (Type == TypeTextSecondary.None)
                //{
                //    holder.About.Visibility = ViewStates.Gone;
                //}
                //else
                //{
                //    holder.About.Text = Type == TypeTextSecondary.About ? Methods.FunString.SubStringCutOf(WoWonderTools.GetAboutFinal(users), 25) : ActivityContext.GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(Convert.ToInt32(users.LastseenUnixTime), true);
                //}

                //Online Or offline
                //var online = WoWonderTools.GetStatusOnline(Convert.ToInt32(users.LastseenUnixTime), users.LastseenStatus);
                //holder.ImageLastSeen.SetImageResource(online ? Resource.Drawable.Green_Color : Resource.Drawable.Grey_Offline);

                //if (!ShowButton) return;

                //WoWonderTools.SetAddFriendCondition(users.IsFollowing, holder.Button);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnViewRecycled(Object holder)
        {
            try
            {
                if (ActivityContext?.IsDestroyed != false)
                    return;

                if (holder != null)
                {
                    if (holder is RateUsersAdapterViewHolder viewHolder)
                    {
                        Glide.With(ActivityContext).Clear(viewHolder.Image);
                    }
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public UserDataObject GetItem(int position)
        {
            return UserList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        private void FollowButtonClick(RateUsersAdapterClickEventArgs args) => FollowButtonItemClick?.Invoke(this, args);
        private void Click(RateUsersAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        private void LongClick(RateUsersAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = UserList[p0];
                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.Avatar != "")
                {
                    d.Add(item.Avatar);
                    return d;
                }

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
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CircleCrop);
        }

        #region Event Add Friend

        public void OnFollowButtonItemClick(object sender, RateUsersAdapterClickEventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
                else
                {
                    if (e.Position > -1)
                    {
                        UserDataObject item = GetItem(e.Position);
                        if (item != null)
                        {
                            WoWonderTools.SetAddFriend(ActivityContext, item, e.BtnAddUser);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

    }

    public class RateUsersAdapterViewHolder : RecyclerView.ViewHolder
    {
        public RateUsersAdapterViewHolder(View itemView, Action<RateUsersAdapterClickEventArgs> followButtonClickListener, Action<RateUsersAdapterClickEventArgs> clickListener, Action<RateUsersAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.card_pro_pic);
                Name = MainView.FindViewById<TextView>(Resource.Id.card_name);
                UserRatingBar = MainView.FindViewById<RatingBar>(Resource.Id.UserRatingBar);
                //About = MainView.FindViewById<TextView>(Resource.Id.card_dist);
                //Button = MainView.FindViewById<Button>(Resource.Id.cont);
                //ImageLastSeen = (CircleImageView)MainView.FindViewById(Resource.Id.ImageLastseen);

                //Event
                //Button.Click += (sender, e) => followButtonClickListener(new RateUsersAdapterClickEventArgs { View = itemView, Position = AdapterPosition, BtnAddUser = Button });
                itemView.Click += (sender, e) => clickListener(new RateUsersAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new RateUsersAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Variables Basic

        public View MainView { get; }

        public ImageView Image { get; private set; }
        public TextView Name { get; private set; }
        public RatingBar UserRatingBar { get; private set; }
        //public TextView About { get; private set; }
        //public Button Button { get; private set; }
        //public CircleImageView ImageLastSeen { get; private set; }

        #endregion
    }

    public class RateUsersAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public Button BtnAddUser { get; set; }
    }
}