using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using System.Net.Http;
using System.Net.Http.Headers;
using Android.Net;
using Android.Preferences;
using Newtonsoft.Json;
using Uri = System.Uri;

namespace InzApp
{
    [Activity(Label = "@string/FindFriendToAddLabel", MainLauncher = false)]
    public class FindFriendToAddActivity : Activity
    {
        private EditText mFriendNameEditText;
        private EditText mFriendSurnameEditText;
        private Button mFindFriendButton;
            
        private string mFriendName;
        private string mFriendSurname;
        private HttpClient client;

        private ISharedPreferences preferences;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.FindFriendToAdd);

            preferences = PreferenceManager.GetDefaultSharedPreferences(ApplicationContext);

            mFriendNameEditText = FindViewById<EditText>(Resource.Id.friendNameEditText);
            mFriendSurnameEditText = FindViewById<EditText>(Resource.Id.friendSurnameEditText);
            mFindFriendButton = FindViewById<Button>(Resource.Id.findFriendButton);

            mFindFriendButton.Click += async (object sender, EventArgs e) =>
            {
                if (!mFriendNameEditText.Text.Equals("") && !mFriendSurnameEditText.Text.Equals(""))
                {
                    mFriendName = mFriendNameEditText.Text;
                    mFriendSurname = mFriendSurnameEditText.Text;
                    var token = JsonConvert.DeserializeObject<AuthenticationToken>(preferences.GetString("token", null));
                    if (token == null)
                    {
                        Toast.MakeText(this, GetString(Resource.String.LoginTokenOrTravelerFailure), ToastLength.Long).Show();
                        Finish();
                        StartActivity(typeof(LoginActivity));
                    }
                    var connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
                    var activityNetworkInfo = connectivityManager.ActiveNetworkInfo;
                    if (activityNetworkInfo == null || !activityNetworkInfo.IsConnected)
                    {
                        Toast.MakeText(this, GetString(Resource.String.NoConnectionInfo),ToastLength.Long).Show();
                        return;
                    }
                    var url = string.Format(GetString(Resource.String.ApiLink)
                        + "/api/traveler/gettravelerbyname?travelername={0}&travelersurname={1}",
                        mFriendName, mFriendSurname);
                    client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue(token.TokenType, token.AccessToken);

                    var loadingMessage = ProgressDialog.Show(this,
                        GetString(Resource.String.SearchingForFriendTitle),
                        GetString(Resource.String.SearchingForFriendContent), true, false);
                    var response = await client.GetAsync(new Uri(url));
                    loadingMessage.Dismiss();

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        var addFriendActivityIntent = new Intent(this, typeof (AddFriendActivity));
                        addFriendActivityIntent.PutExtra("Content", content);
                        StartActivityForResult(addFriendActivityIntent, 3);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        Toast.MakeText(this, GetString(Resource.String.FindFriendToAddNotFoundInfo),
                            ToastLength.Long).Show();
                    else
                        Toast.MakeText(this, GetString(Resource.String.FindFriendToAddNotOkInfo), ToastLength.Long)
                            .Show();
                }
                else Toast.MakeText(this, GetString(Resource.String.FindFriendToAddNoDataInfo), ToastLength.Long).Show();
            };
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode != 3) return;
            switch (resultCode)
            {
                case Result.Ok:
                    FinishActivity(3);
                    SetResult(resultCode);
                    Finish();
                    break;
                case Result.Canceled:
                    FinishActivity(3);
                    break;
                case (Result)409:
                    FinishActivity(3);
                    SetResult(resultCode);
                    Finish();
                    break;
                default:
                    Toast.MakeText(this, GetString(Resource.String.FriendAddNotOkInfo), ToastLength.Long).Show();
                    break;
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            SetResult(Result.Canceled);
        }
    }
}