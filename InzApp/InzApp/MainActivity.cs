using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Widget;

namespace InzApp
{
    [Activity(Label = "@string/MainLabel", MainLauncher = false)]
    public class MainActivity : Activity
    {   //zmienne dla przycisków
        private ImageButton mMyMessagesButton;
        private ImageButton mFriendsMessagesButton;
        private ImageButton mMyFriendsButton;
        private ImageButton mOtherTravelersButton;
        // zmienne dla przechowywania danych w œrodowisku oraz ich edytora
        private ISharedPreferences preferences;
        private ISharedPreferencesEditor editor;
        // metoda wywo³ania na stworzenie widoku
        protected override void OnCreate(Bundle bundle)
        {   //przypisanie widoku AXML do klasy
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            // Shared preferences file for storing bearer token for whole app - initialization:
            preferences = PreferenceManager.GetDefaultSharedPreferences(ApplicationContext);
            editor = preferences.Edit();

            //przypisanie kontrolek widoku do zmiennych
            mMyMessagesButton = base.FindViewById<ImageButton>(Resource.Id.msgImageButton);
            mFriendsMessagesButton = base.FindViewById<ImageButton>(Resource.Id.friendsMsgImageButton);
            mMyFriendsButton = base.FindViewById<ImageButton>(Resource.Id.friendsImageButton);
            mOtherTravelersButton = base.FindViewById<ImageButton>(Resource.Id.travelersImageButton);
            //metody dla zdarzenia klikniêcia na przycisk
            mMyMessagesButton.Click += MMyMessagesButton_Click;
            mFriendsMessagesButton.Click += MFriendsMessagesButton_Click;
            mMyFriendsButton.Click += MMyFriendsButton_Click;
            mOtherTravelersButton.Click += MOtherTravelersButton_Click;
        }
        //metody wywo³uj¹ce odpowiednie dla przycisku aktywnoœci (klasy dla widoków)
        private void MMyMessagesButton_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MyMessagesActivity));
        }

        private void MFriendsMessagesButton_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(FriendsMessagesActivity));
        }

        private void MMyFriendsButton_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MyFriendsActivity));
        }

        private void MOtherTravelersButton_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(FindOtherTravelersActivity));
        }

        public override void OnBackPressed()
        {
            var confirmLogout = new AlertDialog.Builder(this);
            confirmLogout.SetTitle(GetString(Resource.String.LogoutConfirmationTitle));
            confirmLogout.SetMessage(GetString(Resource.String.LogoutConfirmationContent));
            confirmLogout.SetPositiveButton(GetString(Resource.String.LogoutConfirmationPositiveButtonText),
                (senderAlert, args) =>
                {
                    editor.Remove("token");
                    editor.Remove("traveler");
                    editor.Apply();
                    SetResult(Result.Ok);
                    Finish();
                });
            confirmLogout.SetNegativeButton(GetString(Resource.String.LogoutConfirmationNegativeButtonText),
                (senderAlert, args) =>
                {
                    Toast.MakeText(this,GetString(Resource.String.LogoutConfirmationLogoutCancelled),ToastLength.Long).Show();
                });

            var dialog = confirmLogout.Create();
            dialog.Show();
        }
    }
}