using System;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS; 
using AndroidX.AppCompat.App;
using AndroidX.Preference;
using WoWonder.Activities.AddPost.Service;
using WoWonder.Activities.Chat.Floating;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils; 

namespace WoWonder.Activities.SettingsPreferences
{
    public static class MainSettings
    {
        public static ISharedPreferences SharedData, LastPosition, InAppReview;
        public static readonly string LightMode = "light";
        public static readonly string DarkMode = "dark";
        public static readonly string DefaultMode = "default";

        public static readonly string PrefKeyLastPositionX = "last_position_x";
        public static readonly string PrefKeyLastPositionY = "last_position_y";
        public static readonly string PrefKeyInAppReview = "In_App_Review";
        public static readonly string PrefKeyUpdateApp = "In_App_Update";

        public static void Init()
        {
            try
            {
                SharedData = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
                LastPosition = Application.Context.GetSharedPreferences("last_position", FileCreationMode.Private);
                InAppReview = Application.Context.GetSharedPreferences("In_App_Review", FileCreationMode.Private);
                
                string getValue = SharedData.GetString("Night_Mode_key", string.Empty);
                ApplyTheme(getValue);

                PostService.ActionPost = Application.Context.PackageName + ".action.ACTION_POST";
                PostService.ActionStory = Application.Context.PackageName + ".action.ACTION_STORY";

                var cdv = InitFloating.CanDrawOverlays(Application.Context);
                if (cdv)
                    UserDetails.SChatHead = SharedData.GetBoolean("chatheads_key", cdv);

                UserDetails.SoundControl = SharedData.GetBoolean("checkBox_PlaySound_key", true); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static void ApplyTheme(string themePref)
        {
            try
            {
                if (themePref == LightMode)
                {
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
                    AppSettings.SetTabDarkTheme = false;
                }
                else if (themePref == DarkMode)
                {
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightYes;
                    AppSettings.SetTabDarkTheme = true;
                }
                else if (themePref == DefaultMode)
                {
                    AppCompatDelegate.DefaultNightMode = (int)Build.VERSION.SdkInt >= 29 ? AppCompatDelegate.ModeNightFollowSystem : AppCompatDelegate.ModeNightAutoBattery;

                    var currentNightMode = Application.Context.Resources?.Configuration?.UiMode & UiMode.NightMask;
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
                }
                else
                {
                    if (AppSettings.SetTabDarkTheme) return;

                    var currentNightMode = Application.Context.Resources?.Configuration?.UiMode & UiMode.NightMask;
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
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}