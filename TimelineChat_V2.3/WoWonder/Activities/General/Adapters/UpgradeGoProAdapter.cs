using Android.App;
using Android.Content.Res;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.ObjectModel;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Utils;
using Console = System.Console;

namespace WoWonder.Activities.General.Adapters
{
    public class UpgradeGoProClass
    {
        public int Id { get; set; }
        public string HexColor { get; set; }
        public string PlanText { get; set; }

        public string PlanTime { get; set; }

        public string PlanPrice { get; set; }

        public int ImageResource { get; set; }
    }

    public class UpgradeGoProAdapter : RecyclerView.Adapter
    {
        #region Variables Basic

        public event EventHandler<UpgradeGoProAdapterClickEventArgs> UpgradeButtonItemClick;
        public event EventHandler<UpgradeGoProAdapterClickEventArgs> ItemClick;
        public event EventHandler<UpgradeGoProAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        private WoTextDecorator WoTextDecorator { get; set; } 
        public ObservableCollection<UpgradeGoProClass> PlansList = new ObservableCollection<UpgradeGoProClass>();

        #endregion

        public UpgradeGoProAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                WoTextDecorator = new WoTextDecorator();
                 
                //PlansList.Add(new UpgradeGoProClass { Id = 1, HexColor = "#4c7737", PlanText = Resource.String.go_pro_plan_1,PlanTime="",PlanPrice="" ImageResource = Resource.Drawable.gopro_medal });
                 
                for (int i = 0; i < AppSettings.LimitGoProPlansCountsTo + 1; i++)
                {
                    string name = "go_pro_plan_" + i;
                    int? resourceId = ActivityContext.Resources?.GetIdentifier(name, "array", ActivityContext.ApplicationInfo.PackageName);

                    if (resourceId == 0)
                        continue;

                    string[] plan = ActivityContext.Resources?.GetStringArray(resourceId.Value); 
                    resourceId = ActivityContext.Resources?.GetIdentifier("ic_plan_" + i, "drawable", ActivityContext.ApplicationInfo.PackageName);
                    if (resourceId == 0)
                        continue;

                    var price = plan[1];
                    switch (i)
                    {
                        case 1:
                            price = ListUtils.SettingsSiteList?.ProPackages?.Star?.Price ?? AppSettings.WeeklyPrice;
                            break;
                        case 2:
                            price = ListUtils.SettingsSiteList?.ProPackages?.Hot?.Price ?? AppSettings.MonthlyPrice;
                            break;
                        case 3:
                            price = ListUtils.SettingsSiteList?.ProPackages?.Ultima?.Price ?? AppSettings.YearlyPrice;
                            break;
                        case 4:
                            price = ListUtils.SettingsSiteList?.ProPackages?.Vip?.Price ?? AppSettings.LifetimePrice;
                            break;
                    }

                    PlansList.Add(new UpgradeGoProClass { Id = i, HexColor = plan[3], PlanText = plan[0], PlanPrice = price, PlanTime = plan[2], ImageResource = resourceId.Value });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => PlansList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_GoPro_Pricess_View
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_GoPro_Pricess_View, parent, false);
                UpgradePlansViewHolder vh = new UpgradePlansViewHolder(itemView, UpgradeButtonClick,Click, LongClick);
                return vh;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is UpgradePlansViewHolder holder)
                {
                    UpgradeGoProClass item = PlansList[position];
                    if (item != null)
                    {
                        if (AppSettings.SetTabDarkTheme)
                        {
                            holder.MainLayout.SetBackgroundResource(Resource.Drawable.ShadowLinerLayoutDark);
                            holder.RelativeLayout.SetBackgroundResource(Resource.Drawable.price_gopro_item_style_dark);
                        }

                        holder.PlanImg.SetImageResource(item.ImageResource);
                        holder.PlanImg.SetColorFilter(Color.ParseColor(item.HexColor));

                        var (currency, currencyIcon) = WoWonderTools.GetCurrency(ListUtils.SettingsSiteList?.Currency);
                        Console.WriteLine(currency);
                        if (ListUtils.SettingsSiteList != null)
                            holder.PriceText.Text = currencyIcon + item.PlanPrice;
                        else
                            holder.PriceText.Text = item.PlanPrice;
                         
                        holder.PlanText.Text = item.PlanText;
                        holder.PerText.Text = item.PlanTime;

                        holder.PlanText.SetTextColor(Color.ParseColor(item.HexColor));
                        holder.PriceText.SetTextColor(Color.ParseColor(item.HexColor));
                        holder.UpgradeButton.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(item.HexColor));

                        Typeface font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "ionicons.ttf");

                        string name = "go_pro_array_" + item.Id;
                        int? resourceId = ActivityContext.Resources?.GetIdentifier(name, "array", ActivityContext.ApplicationInfo.PackageName);
                        if (resourceId == 0)
                        {
                            return;
                        }

                        string[] planArray = ActivityContext.Resources?.GetStringArray(resourceId.Value);
                        if (planArray != null)
                        {
                            foreach (string options in planArray)
                            {
                                if (!string.IsNullOrEmpty(options))
                                {
                                    AppCompatTextView text = new AppCompatTextView(ActivityContext)
                                    {
                                        Text = options,
                                        TextSize = 13
                                    };
                                     
                                    text.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.ParseColor("#444444"));
                                    text.Gravity = GravityFlags.CenterHorizontal;
                                    text.SetTypeface(font, TypefaceStyle.Normal);
                                    WoTextDecorator.Content = options;
                                    WoTextDecorator.DecoratedContent = new Android.Text.SpannableString(options);
                                    WoTextDecorator.SetTextColor(IonIconsFonts.Checkmark, "#43a735");
                                    WoTextDecorator.SetTextColor(IonIconsFonts.Close, "#e13c4c");

                                    LinearLayout.LayoutParams paramsss = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);//height and width are inpixel
                                    paramsss.SetMargins(0, 30, 0, 5);

                                    text.LayoutParameters = paramsss;
                                    holder.OptionLinerLayout.AddView(text);
                                    WoTextDecorator.Build(text, WoTextDecorator.DecoratedContent);
                                }
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

        public UpgradeGoProClass GetItem(int position)
        {
            return PlansList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            }
        }

        private void UpgradeButtonClick(UpgradeGoProAdapterClickEventArgs args)
        {
            UpgradeButtonItemClick?.Invoke(this, args);
        }

        private void Click(UpgradeGoProAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(UpgradeGoProAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

    }

    public class UpgradePlansViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public LinearLayout MainLayout { get; private set; }
        public ImageView PlanImg { get; private set; }
        public TextView PriceText { get; private set; } 
        public TextView PerText { get; private set; }
        public Button UpgradeButton { get; private set; } 
        public TextView PlanText { get; private set; }
        public LinearLayout OptionLinerLayout { get; private set; } 
        public View MainView { get; private set; }
        public RelativeLayout RelativeLayout { get; private set; }

        #endregion

        public UpgradePlansViewHolder(View itemView ,Action<UpgradeGoProAdapterClickEventArgs> upgradeButtonClickListener, Action<UpgradeGoProAdapterClickEventArgs> clickListener, Action<UpgradeGoProAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                MainLayout = MainView.FindViewById<LinearLayout>(Resource.Id.mainLayout);
                PlanImg = MainView.FindViewById<ImageView>(Resource.Id.iv1);
                PriceText = MainView.FindViewById<TextView>(Resource.Id.priceTextView);
                PerText = MainView.FindViewById<TextView>(Resource.Id.PerText);
                PlanText = MainView.FindViewById<TextView>(Resource.Id.PlanText);
                OptionLinerLayout = MainView.FindViewById<LinearLayout>(Resource.Id.OptionLinerLayout);
                UpgradeButton = MainView.FindViewById<Button>(Resource.Id.UpgradeButton);
                RelativeLayout = MainView.FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);

                UpgradeButton.Click += (sender, e) => upgradeButtonClickListener(new UpgradeGoProAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.Click += (sender, e) => clickListener(new UpgradeGoProAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new UpgradeGoProAdapterClickEventArgs { View = itemView, Position = AdapterPosition });

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class UpgradeGoProAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}