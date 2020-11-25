using Android.App;
using Android.Content;
using Android.Views.InputMethods;

namespace WoWonder.Activities.Live.Page
{
    public static class KeyboardHelper
    {
        public static void HideSoftKeyboard(Activity activity)
        {
            var currentFocus = activity.CurrentFocus;
            if (currentFocus != null)
            {
                var inputMethodManager = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);
                inputMethodManager.HideSoftInputFromWindow(currentFocus.WindowToken, HideSoftInputFlags.None);
            }
        }
        public static bool IsKeyBoardVisible(Activity activity)
        {
            var inputMethodManager = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);
            return inputMethodManager.IsAcceptingText;
        }

        public static void ShowSoftKeyboard(Activity activity)
        {
            var inputMethodManager = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);
            inputMethodManager.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);
        }
    }
}