using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.OS;


using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using WoWonder.Activities.Movies.Adapters;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Movies;
using WoWonderClient.Requests;

namespace WoWonder.Activities.Videos.Fragment
{
    public class VideosCommentFragment : AndroidX.Fragment.App.Fragment
    {
        #region  Variables Basic

        private VideoViewerActivity GlobalContext;
        public MoviesCommentAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private EditText TxtComment;
        private ImageView ImgSent;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = VideoViewerActivity.GetInstance();
        }
         
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.VideoCommentLayout, container, false);

                InitComponent(view);
                SetRecyclerViewAdapters();
                AddOrRemoveEvent(true);

                return view;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
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

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                MRecycler = view.FindViewById<RecyclerView>(Resource.Id.recyler);
                TxtComment = view.FindViewById<EditText>(Resource.Id.commenttext);
                ImgSent = view.FindViewById<ImageView>(Resource.Id.send);

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                 
                TxtComment.Text = "";
                Methods.SetColorEditText(TxtComment, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
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
                MAdapter = new MoviesCommentAdapter(Activity, MRecycler, "Light", GlobalContext.MoviesId, "Comment")
                {
                    CommentList = new ObservableCollection<CommentsMoviesObject>()
                };

                if (!Methods.CheckConnectivity())
                    Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                else
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MAdapter.FetchMoviesApiComments(GlobalContext.MoviesId, "0") });
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
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                    ImgSent.Click += ImgSentOnClick;
                }
                else
                {
                    SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                    ImgSent.Click -= ImgSentOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                MAdapter.CommentList.Clear();
                MAdapter.NotifyDataSetChanged();

                if (!Methods.CheckConnectivity())
                    Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                else
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MAdapter.FetchMoviesApiComments(GlobalContext.MoviesId, "0") });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
         
        #region Event

        //Api sent Comment
        private async void ImgSentOnClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TxtComment.Text))
                    return;

                if (Methods.CheckConnectivity())
                {
                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                    //Comment Code 

                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    var time = unixTimestamp.ToString();

                    CommentsMoviesObject comment = new CommentsMoviesObject
                    {
                        Id = unixTimestamp.ToString(),
                        MovieId = GlobalContext.MoviesId,
                        UserId = UserDetails.UserId,
                        Text = TxtComment.Text,
                        Likes = "0",
                        Posted = time,
                        UserData = dataUser,
                        IsOwner = true,
                        Dislikes = "0",
                        IsCommentLiked = false,
                        Replies = new List<CommentsMoviesObject>()
                    };

                    MAdapter.CommentList.Add(comment);

                    var index = MAdapter.CommentList.IndexOf(comment);
                    if (index > -1)
                    {
                        MAdapter.NotifyItemInserted(index);
                    }

                    MRecycler.Visibility = ViewStates.Visible;

                    var dd = MAdapter.CommentList.FirstOrDefault();
                    if (dd?.Text == MAdapter.EmptyState)
                    {
                        MAdapter.CommentList.Remove(dd);
                        MAdapter.NotifyItemRemoved(MAdapter.CommentList.IndexOf(dd));
                    }

                    var text = TxtComment.Text;

                    //Hide keyboard
                    TxtComment.Text = "";

                    (int apiStatus, var respond) = await RequestsAsync.Movies.CreateComments(GlobalContext.MoviesId, text);
                    if (apiStatus == 200)
                    {
                        if (respond is GetCommentsMoviesObject result)
                        {
                            var date = MAdapter.CommentList.FirstOrDefault(a => a.Id == comment.Id) ?? MAdapter.CommentList.FirstOrDefault(x => x.Id == result.Data[0]?.Id);
                            if (date != null)
                            {
                                date = result.Data[0];
                                date.Id = result.Data[0].Id;

                                index = MAdapter.CommentList.IndexOf(MAdapter.CommentList.FirstOrDefault(a => a.Id == unixTimestamp.ToString()));
                                if (index > -1)
                                {
                                    MAdapter.CommentList[index] = result.Data[0];

                                    //MAdapter.NotifyItemChanged(index);
                                    MRecycler.ScrollToPosition(index);
                                }
                            }
                        }
                    }
                    else Methods.DisplayReportResult(Activity, respond);

                    //Hide keyboard
                    TxtComment.Text = "";
                }
                else
                {
                    Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

       
        #endregion

    }
}