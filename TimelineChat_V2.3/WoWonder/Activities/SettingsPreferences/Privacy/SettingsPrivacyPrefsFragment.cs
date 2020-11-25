using Android.App;
using Android.Content;
using Android.OS;

using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using AndroidX.Preference;
using Java.Lang;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient.Requests;
using Exception = System.Exception;


namespace WoWonder.Activities.SettingsPreferences.Privacy
{
    public class SettingsPrivacyPrefsFragment : PreferenceFragmentCompat, ISharedPreferencesOnSharedPreferenceChangeListener, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback 
    {
        #region  Variables Basic

        private Preference PrivacyCanFollowPref, PrivacyCanMessagePref, PrivacyCanSeeMyFriendsPref, PrivacyCanPostOnMyTimelinePref, PrivacyCanSeeMyBirthdayPref;

        private SwitchPreferenceCompat PrivacyConfirmRequestFollowsPref, PrivacyShowMyActivitiesPref, PrivacyShareMyLocationPref, PrivacyOnlineUserPref;
        private string SCanFollowPref = "0", SCanMessagePref = "0",SCanSeeMyFriendsPref = "0", SCanPostOnMyTimelinePref = "0", SCanSeeMyBirthdayPref = "0", SConfirmRequestFollowsPref = "0", SShowMyActivitiesPref = "0", SOnlineUsersPref = "0", SShareMyLocationPref = "0" , TypeDialog;
         
        private readonly Activity ActivityContext;

        #endregion

        #region General

        public SettingsPrivacyPrefsFragment(Activity context)
        {
            try
            {
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                // create ContextThemeWrapper from the original Activity Context with the custom theme
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(ActivityContext, Resource.Style.SettingsThemeDark) : new ContextThemeWrapper(ActivityContext, Resource.Style.SettingsTheme);

                // clone the inflater using the ContextThemeWrapper
                var localInflater = inflater.CloneInContext(contextThemeWrapper);

                var view = base.OnCreateView(localInflater, container, savedInstanceState);

                return view;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            try
            {
                // Load the preferences from an XML resource
                AddPreferencesFromResource(Resource.Xml.SettingsPrefs_Privacy);

                MainSettings.SharedData = PreferenceManager.SharedPreferences;
                InitComponent();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnResume()
        {
            try
            {
                base.OnResume();
                PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnPause()
        {
            try
            {
                base.OnPause();
                PreferenceScreen.SharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);
                AddOrRemoveEvent(false);
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
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                MainSettings.SharedData = PreferenceManager.SharedPreferences;
                PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);

                PrivacyCanFollowPref = FindPreference("whocanfollow_key");
                PrivacyCanMessagePref = FindPreference("whocanMessage_key");
                PrivacyCanSeeMyFriendsPref = FindPreference("whoCanSeeMyfriends_key");
                PrivacyCanPostOnMyTimelinePref = FindPreference("whoCanPostOnMyTimeline_key");
                PrivacyCanSeeMyBirthdayPref = FindPreference("whoCanSeeMyBirthday_key");
                PrivacyConfirmRequestFollowsPref = (SwitchPreferenceCompat)FindPreference("ConfirmRequestFollows_key");
                PrivacyShowMyActivitiesPref = (SwitchPreferenceCompat)FindPreference("ShowMyActivities_key");
                PrivacyOnlineUserPref = (SwitchPreferenceCompat)FindPreference("onlineUser_key");
                PrivacyShareMyLocationPref = (SwitchPreferenceCompat)FindPreference("ShareMyLocation_key");

                //Update Preferences data on Load 
                OnSharedPreferenceChanged(MainSettings.SharedData, "whocanfollow_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "whocanMessage_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "whoCanSeeMyfriends_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "whoCanPostOnMyTimeline_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "whoCanSeeMyBirthday_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "ConfirmRequestFollows_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "ShowMyActivities_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "onlineUser_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "ShareMyLocation_key");

                PrivacyConfirmRequestFollowsPref.IconSpaceReserved = false;
                PrivacyShowMyActivitiesPref.IconSpaceReserved = false;
                PrivacyShareMyLocationPref.IconSpaceReserved = false;
                PrivacyOnlineUserPref.IconSpaceReserved = false;
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
                    PrivacyConfirmRequestFollowsPref.PreferenceChange += PrivacyConfirmRequestFollowsPref_OnPreferenceChange;
                    PrivacyShowMyActivitiesPref.PreferenceChange += PrivacyShowMyActivitiesPref_OnPreferenceChange;
                    PrivacyOnlineUserPref.PreferenceChange += PrivacyOnlineUserPref_OnPreferenceChange;
                    PrivacyShareMyLocationPref.PreferenceChange += PrivacyShareMyLocationPref_OnPreferenceChange;
                }
                else
                {
                    PrivacyConfirmRequestFollowsPref.PreferenceChange -= PrivacyConfirmRequestFollowsPref_OnPreferenceChange;
                    PrivacyShowMyActivitiesPref.PreferenceChange -= PrivacyShowMyActivitiesPref_OnPreferenceChange;
                    PrivacyOnlineUserPref.PreferenceChange -= PrivacyOnlineUserPref_OnPreferenceChange;
                    PrivacyShareMyLocationPref.PreferenceChange -= PrivacyShareMyLocationPref_OnPreferenceChange;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Confirm request when someone follows you
        private void PrivacyConfirmRequestFollowsPref_OnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs eventArgs)
        {
            try
            {
                if (eventArgs.Handled)
                {
                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();

                    var etp = (SwitchPreferenceCompat)sender;
                    var value = eventArgs.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    if (etp.Checked)
                    {
                        SConfirmRequestFollowsPref = "1";
                        if (dataUser != null)
                        {
                            dataUser.ConfirmFollowers = "1";
                            var sqLiteDatabase = new SqLiteDatabase();
                            sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                            
                        }
                    }
                    else
                    {
                        SConfirmRequestFollowsPref = "0";
                        if (dataUser != null)
                        {
                            dataUser.ConfirmFollowers = "0";
                            var sqLiteDatabase = new SqLiteDatabase();
                            sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                            
                        }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataPrivacy = new Dictionary<string, string>
                        {
                            {"confirm_followers", SConfirmRequestFollowsPref}
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Update_User_Data(dataPrivacy) });
                    }
                    else
                    {
                        Toast.MakeText(ActivityContext,
                                ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)
                            .Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        //Show my activities
        private void PrivacyShowMyActivitiesPref_OnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs eventArgs)
        {
            try
            {
                if (eventArgs.Handled)
                {
                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                    var etp = (SwitchPreferenceCompat)sender;
                    var value = eventArgs.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    if (etp.Checked)
                    {
                        if (dataUser != null)
                        {
                            dataUser.ShowActivitiesPrivacy = "1";
                            var sqLiteDatabase = new SqLiteDatabase();
                            sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                            
                        }

                        SShowMyActivitiesPref = "1";
                    }
                    else
                    {
                        if (dataUser != null)
                        {
                            dataUser.ShowActivitiesPrivacy = "0";
                            var sqLiteDatabase = new SqLiteDatabase();
                            sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                            
                        }

                        SShowMyActivitiesPref = "0";
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataPrivacy = new Dictionary<string, string>
                        {
                            {"show_activities_privacy", SShowMyActivitiesPref}
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Update_User_Data(dataPrivacy) });
                    }
                    else
                    {
                        Toast.MakeText(ActivityContext,
                                ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection),
                                ToastLength.Long)
                            .Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Status >> OnlineUser
        private void PrivacyOnlineUserPref_OnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs eventArgs)
        {
            try
            {
                if (eventArgs.Handled)
                {
                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                    var etp = (SwitchPreferenceCompat)sender;
                    var value = eventArgs.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    if (etp.Checked) //Online >> value = 0
                    {
                        SOnlineUsersPref = "0";

                        if (dataUser != null)
                        {
                            dataUser.Status = "0";
                            var sqLiteDatabase = new SqLiteDatabase();
                            sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                            
                        }
                    }
                    else //Offline >> value = 1
                    {
                        SOnlineUsersPref = "1";

                        if (dataUser != null)
                        {
                            dataUser.Status = "1";
                            var sqLiteDatabase = new SqLiteDatabase();
                            sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                            
                        }
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataPrivacy = new Dictionary<string, string>
                        {
                            {"status", SOnlineUsersPref}
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Update_User_Data(dataPrivacy) });
                    }
                    else
                    {
                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    } 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Share my location with public
        private void PrivacyShareMyLocationPref_OnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs eventArgs)
        {
            try
            {
                if (eventArgs.Handled)
                {
                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                    var etp = (SwitchPreferenceCompat)sender;
                    var value = eventArgs.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    if (etp.Checked) //Yes >> value = 1
                    {
                        if (dataUser != null)
                        {
                            dataUser.ShareMyLocation = "1";
                            var sqLiteDatabase = new SqLiteDatabase();
                            sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                            
                        }

                        SShareMyLocationPref = "1";
                    }
                    else //No >> value = 0
                    {
                        if (dataUser != null)
                        {
                            dataUser.ShareMyLocation = "0";
                            var sqLiteDatabase = new SqLiteDatabase();
                            sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                            
                        }

                        SShareMyLocationPref = "0";
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataPrivacy = new Dictionary<string, string>
                        {
                            {"share_my_location", SShareMyLocationPref}
                        };

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Update_User_Data(dataPrivacy) });
                    }
                    else
                    {
                        Toast.MakeText(ActivityContext,
                                ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection),
                                ToastLength.Long)
                            .Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
          
        #endregion
         
        //On Change 
        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            try
            {
                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                if (key.Equals("whocanfollow_key"))
                {
                    // Set summary to be the user-description for the selected value
                    Preference etp = FindPreference("whocanfollow_key");

                    string getValue = MainSettings.SharedData.GetString("whocanfollow_key", dataUser?.FollowPrivacy ?? string.Empty);
                    if (getValue == "0")
                    {
                        etp.Summary = ActivityContext.GetText(Resource.String.Lbl_Everyone);
                        SCanFollowPref = "0";
                    }
                    else if (getValue == "1")
                    {
                        etp.Summary = ActivityContext.GetText(Resource.String.Lbl_People_i_Follow);
                        SCanFollowPref = "1";
                    }
                    else
                    {
                        etp.Summary = getValue;
                    }
                }
                else if (key.Equals("whocanMessage_key"))
                {
                    // Set summary to be the user-description for the selected value
                    Preference etp = FindPreference("whocanMessage_key");

                    string getValue = MainSettings.SharedData.GetString("whocanMessage_key", dataUser?.MessagePrivacy ?? string.Empty);
                    if (getValue == "0")
                    {
                        etp.Summary = ActivityContext.GetText(Resource.String.Lbl_Everyone);
                        SCanMessagePref = "0";
                    }
                    else if (getValue == "1")
                    {
                        etp.Summary = ActivityContext.GetText(Resource.String.Lbl_People_i_Follow);
                        SCanMessagePref = "1";
                    }
                    else if (getValue == "2")
                    {
                        etp.Summary = ActivityContext.GetText(Resource.String.Lbl_No_body);
                        SCanMessagePref = "2";
                    }
                    else
                    {
                        etp.Summary = getValue;
                    } 
                }
                else if (key.Equals("whoCanSeeMyfriends_key"))
                {
                    // Set summary to be the user-description for the selected value
                    Preference etp = FindPreference("whoCanSeeMyfriends_key");

                    string getValue = MainSettings.SharedData.GetString("whoCanSeeMyfriends_key", dataUser?.FriendPrivacy ?? string.Empty);
                    if (getValue == "0")
                    {
                        etp.Summary = ActivityContext.GetText(Resource.String.Lbl_Everyone);
                        SCanSeeMyFriendsPref = "0";
                    }
                    else if (getValue == "1")
                    {
                        etp.Summary = ActivityContext.GetText(Resource.String.Lbl_People_i_Follow);
                        SCanSeeMyFriendsPref = "1";
                    }
                    else if (getValue == "2")
                    {
                        etp.Summary = ActivityContext.GetText(Resource.String.Lbl_People_Follow_Me);
                        SCanSeeMyFriendsPref = "2";
                    }
                    else if (getValue == "3")
                    {
                        etp.Summary = ActivityContext.GetText(Resource.String.Lbl_No_body);
                        SCanSeeMyFriendsPref = "3";
                    }
                    else
                    {
                        etp.Summary = getValue;
                    }
                }
                else if (key.Equals("whoCanPostOnMyTimeline_key"))
                { 
                    // Set summary to be the user-description for the selected value
                    Preference etp = FindPreference("whoCanPostOnMyTimeline_key");

                    string getValue = MainSettings.SharedData.GetString("whoCanPostOnMyTimeline_key", dataUser?.MessagePrivacy ?? string.Empty);
                    if (getValue == "0")
                    {
                        etp.Summary = ActivityContext.GetText(Resource.String.Lbl_Everyone);
                        SCanPostOnMyTimelinePref = "0";
                    }
                    else if (getValue == "1")
                    {
                        etp.Summary = ActivityContext.GetText(Resource.String.Lbl_People_i_Follow);
                        SCanPostOnMyTimelinePref = "1";
                    }
                    else if (getValue == "2")
                    {
                        etp.Summary = ActivityContext.GetText(Resource.String.Lbl_No_body);
                        SCanPostOnMyTimelinePref = "2";
                    }
                    else
                    {
                        etp.Summary = getValue;
                    }
                }
                else if (key.Equals("whoCanSeeMyBirthday_key"))
                {
                    // Set summary to be the user-description for the selected value
                    Preference etp = FindPreference("whocanseemybirthday_key");

                    string getValue = MainSettings.SharedData.GetString("whocanseemybirthday_key", dataUser?.BirthPrivacy ?? string.Empty);
                    if (getValue == "0")
                    {
                        etp.Summary = ActivityContext.GetText(Resource.String.Lbl_Everyone);
                        SCanSeeMyBirthdayPref = "0";
                    }
                    else if (getValue == "1")
                    {
                        etp.Summary = ActivityContext.GetText(Resource.String.Lbl_People_i_Follow);
                        SCanSeeMyBirthdayPref = "1";
                    }
                    else if (getValue == "2")
                    {
                        etp.Summary = ActivityContext.GetText(Resource.String.Lbl_No_body);
                        SCanSeeMyBirthdayPref = "1";
                    }
                    else
                    {
                        etp.Summary = getValue;
                    } 
                }
                else if (key.Equals("ConfirmRequestFollows_key"))
                {
                    try
                    {
                        var getvalue = MainSettings.SharedData.GetBoolean("ConfirmRequestFollows_key", true);
                        PrivacyConfirmRequestFollowsPref.Checked = getvalue; 
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }
                else if (key.Equals("ShowMyActivities_key"))
                {
                    try
                    {
                        var getvalue = MainSettings.SharedData.GetBoolean("ShowMyActivities_key", true);
                        PrivacyShowMyActivitiesPref.Checked = getvalue; 
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }
                else if (key.Equals("onlineUser_key"))
                {
                    var getvalue = MainSettings.SharedData.GetBoolean("onlineUser_key", true);
                    PrivacyOnlineUserPref.Checked = getvalue; 
                }
                else if (key.Equals("ShareMyLocation_key"))
                {
                    try
                    {
                        var getvalue = MainSettings.SharedData.GetBoolean("ShareMyLocation_key", true);
                        PrivacyShareMyLocationPref.Checked = getvalue;
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        public override bool OnPreferenceTreeClick(Preference preference)
        {
            try
            {  
                if (preference.Key == "whocanfollow_key")
                {
                    TypeDialog = "WhoCanFollow";

                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                    dialogList.Title(Resource.String.Lbl_Who_can_follow_me);

                    arrayAdapter.Add(GetText(Resource.String.Lbl_Everyone)); //>> value = 0
                    arrayAdapter.Add(GetText(Resource.String.Lbl_People_i_Follow)); //>> value = 1

                    dialogList.Items(arrayAdapter);
                    dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
                else if (preference.Key == "whocanMessage_key")
                {
                    TypeDialog = "Message";

                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                    dialogList.Title(Resource.String.Lbl_Who_can_message_me);

                    arrayAdapter.Add(GetText(Resource.String.Lbl_Everyone)); //>> value = 0
                    arrayAdapter.Add(GetText(Resource.String.Lbl_People_i_Follow)); //>> value = 1
                    arrayAdapter.Add(GetText(Resource.String.Lbl_No_body)); //>> value = 2

                    dialogList.Items(arrayAdapter);
                    dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
                else if (preference.Key == "whoCanSeeMyfriends_key")
                {
                    TypeDialog = "CanSeeMyFriends";

                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                    dialogList.Title(Resource.String.Lbl_CanSeeMyfriends);

                    arrayAdapter.Add(GetText(Resource.String.Lbl_Everyone)); //>> value = 0
                    arrayAdapter.Add(GetText(Resource.String.Lbl_People_i_Follow)); //>> value = 1
                    arrayAdapter.Add(GetText(Resource.String.Lbl_People_Follow_Me)); //>> value = 2
                    arrayAdapter.Add(GetText(Resource.String.Lbl_No_body)); //>> value = 3

                    dialogList.Items(arrayAdapter);
                    dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
                else if (preference.Key == "whoCanPostOnMyTimeline_key")
                {
                    TypeDialog = "CanPostOnMyTimeline";

                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                    dialogList.Title(Resource.String.Lbl_CanPostOnMyTimeline);

                    arrayAdapter.Add(GetText(Resource.String.Lbl_Everyone)); //>> value = 0
                    arrayAdapter.Add(GetText(Resource.String.Lbl_People_i_Follow)); //>> value = 1
                    arrayAdapter.Add(GetText(Resource.String.Lbl_No_body)); //>> value = 2

                    dialogList.Items(arrayAdapter);
                    dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
                else if (preference.Key == "whoCanSeeMyBirthday_key")
                {
                    TypeDialog = "Birthday";

                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                    dialogList.Title(Resource.String.Lbl_Who_can_see_my_birthday);

                    arrayAdapter.Add(GetText(Resource.String.Lbl_Everyone)); //>> value = 0
                    arrayAdapter.Add(GetText(Resource.String.Lbl_People_i_Follow)); //>> value = 1
                    arrayAdapter.Add(GetText(Resource.String.Lbl_No_body)); //>> value = 2

                    dialogList.Items(arrayAdapter);
                    dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }


                return base.OnPreferenceTreeClick(preference);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return base.OnPreferenceTreeClick(preference);
            }
        }

        #region MaterialDialog

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

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                string text = itemString.ToString();
                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();

                 
                if (TypeDialog == "WhoCanFollow")
                {
                    if (text == GetString(Resource.String.Lbl_Everyone))
                    {
                        MainSettings.SharedData.Edit()?.PutString("WhoCanFollow", "0").Commit();
                        PrivacyCanFollowPref.Summary = text;
                        SCanFollowPref = "0";
                    }
                    else if (text == GetString(Resource.String.Lbl_People_i_Follow))
                    {
                        MainSettings.SharedData.Edit()?.PutString("WhoCanFollow", "1").Commit();
                        PrivacyCanFollowPref.Summary = text;
                        SCanFollowPref = "1";
                    }

                    if (dataUser != null)
                    {
                        dataUser.FollowPrivacy = SCanFollowPref;
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataPrivacy = new Dictionary<string, string>
                        {
                            {"follow_privacy", SCanFollowPref}
                        };
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Update_User_Data(dataPrivacy) });
                    }
                    else
                    {
                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    }
                } 
                else if (TypeDialog == "Message")
                {
                    if (text == GetString(Resource.String.Lbl_Everyone))
                    {
                        MainSettings.SharedData.Edit()?.PutString("whocanMessage_key", "0").Commit();
                        PrivacyCanMessagePref.Summary = text;
                        SCanMessagePref = "0";
                    }
                    else if (text == GetString(Resource.String.Lbl_People_i_Follow))
                    {
                        MainSettings.SharedData.Edit()?.PutString("whocanMessage_key", "1").Commit();
                        PrivacyCanMessagePref.Summary = text;
                        SCanMessagePref = "1";
                    }
                    else if (text == GetString(Resource.String.Lbl_No_body))
                    {
                        MainSettings.SharedData.Edit()?.PutString("whocanMessage_key", "2").Commit();
                        PrivacyCanMessagePref.Summary = text;
                        SCanMessagePref = "2";
                    }

                    if (dataUser != null)
                    {
                        dataUser.MessagePrivacy = SCanMessagePref;
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataPrivacy = new Dictionary<string, string>
                        {
                            {"message_privacy", SCanMessagePref}
                        };
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Update_User_Data(dataPrivacy) });
                    }
                    else
                    {
                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    }
                }
                else if (TypeDialog == "CanSeeMyFriends")
                {
                    if (text == GetString(Resource.String.Lbl_Everyone))
                    {
                        MainSettings.SharedData.Edit()?.PutString("whoCanSeeMyfriends_key", "0").Commit();
                        PrivacyCanSeeMyFriendsPref.Summary = text;
                        SCanSeeMyFriendsPref = "0";
                    }
                    else if (text == GetString(Resource.String.Lbl_People_i_Follow))
                    {
                        MainSettings.SharedData.Edit()?.PutString("whoCanSeeMyfriends_key", "1").Commit();
                        PrivacyCanSeeMyFriendsPref.Summary = text;
                        SCanSeeMyFriendsPref = "1";
                    }
                    else if (text == GetString(Resource.String.Lbl_People_Follow_Me))
                    {
                        MainSettings.SharedData.Edit()?.PutString("whoCanSeeMyfriends_key", "2").Commit();
                        PrivacyCanSeeMyFriendsPref.Summary = text;
                        SCanSeeMyFriendsPref = "2";
                    }
                    else if (text == GetString(Resource.String.Lbl_No_body))
                    {
                        MainSettings.SharedData.Edit()?.PutString("whoCanSeeMyfriends_key", "3").Commit();
                        PrivacyCanSeeMyFriendsPref.Summary = text;
                        SCanSeeMyFriendsPref = "3";
                    }

                    if (dataUser != null)
                    {
                        dataUser.FriendPrivacy = SCanSeeMyFriendsPref;
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataPrivacy = new Dictionary<string, string>
                        {
                            {"friend_privacy", SCanSeeMyFriendsPref}
                        };
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Update_User_Data(dataPrivacy) });
                    }
                    else
                    {
                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    }
                }
                else if (TypeDialog == "CanPostOnMyTimeline")
                {
                    if (text == GetString(Resource.String.Lbl_Everyone))
                    {
                        MainSettings.SharedData.Edit()?.PutString("whoCanPostOnMyTimeline_key", "0").Commit();
                        PrivacyCanPostOnMyTimelinePref.Summary = text;
                        SCanPostOnMyTimelinePref = "0";
                    }
                    else if (text == GetString(Resource.String.Lbl_People_i_Follow))
                    {
                        MainSettings.SharedData.Edit()?.PutString("whoCanPostOnMyTimeline_key", "1").Commit();
                        PrivacyCanPostOnMyTimelinePref.Summary = text;
                        SCanPostOnMyTimelinePref = "1";
                    }
                    else if (text == GetString(Resource.String.Lbl_No_body))
                    {
                        MainSettings.SharedData.Edit()?.PutString("whoCanPostOnMyTimeline_key", "2").Commit();
                        PrivacyCanPostOnMyTimelinePref.Summary = text;
                        SCanPostOnMyTimelinePref = "2";
                    }

                    if (dataUser != null)
                    {
                        dataUser.PostPrivacy = SCanPostOnMyTimelinePref;
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataPrivacy = new Dictionary<string, string>
                        {
                            {"post_privacy", SCanPostOnMyTimelinePref}
                        };
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Update_User_Data(dataPrivacy) });
                    }
                    else
                    {
                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    }
                }
                else if (TypeDialog == "Birthday")
                {
                    if (text == GetString(Resource.String.Lbl_Everyone))
                    {
                        MainSettings.SharedData.Edit()?.PutString("whoCanSeeMyBirthday_key", "0").Commit();
                        PrivacyCanSeeMyBirthdayPref.Summary = text;
                        SCanSeeMyBirthdayPref = "0";
                    }
                    else if (text == GetString(Resource.String.Lbl_People_i_Follow))
                    {
                        MainSettings.SharedData.Edit()?.PutString("whoCanSeeMyBirthday_key", "1").Commit();
                        PrivacyCanSeeMyBirthdayPref.Summary = text;
                        SCanSeeMyBirthdayPref = "1";
                    }
                    else if (text == GetString(Resource.String.Lbl_No_body))
                    {
                        MainSettings.SharedData.Edit()?.PutString("whoCanSeeMyBirthday_key", "2").Commit();
                        PrivacyCanSeeMyBirthdayPref.Summary = text;
                        SCanSeeMyBirthdayPref = "2";
                    }

                    if (dataUser != null)
                    {
                        dataUser.BirthPrivacy = SCanSeeMyBirthdayPref;
                    }

                    if (Methods.CheckConnectivity())
                    {
                        var dataPrivacy = new Dictionary<string, string>
                        {
                            {"birth_privacy", SCanSeeMyBirthdayPref}
                        };
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Update_User_Data(dataPrivacy) });
                    }
                    else
                    {
                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
           
        #endregion

    }
}