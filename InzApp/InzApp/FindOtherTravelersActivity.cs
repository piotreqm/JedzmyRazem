using System;
using System.Net;
using Android.App;
using Android.OS;
using Android.Widget;
using System.Net.Http;
using System.Net.Http.Headers;
using Android.Content;
using Android.Net;
using Android.Preferences;
using Newtonsoft.Json;
using Uri = System.Uri;

namespace InzApp
{
    [Activity(Label = "@string/FindOtherTravelers", MainLauncher = false)]
    public class FindOtherTravelersActivity : Activity, TimePickerDialog.IOnTimeSetListener
    {
        private TextView mTimeTextView;
        private EditText mStartPlaceEdit;
        private EditText mEndPlaceEdit;
        private Button mFindOtherTravelerButton;

        private DateTime mTravelTime;
        private string mStartPlace;
        private string mEndPlace;

        private HttpClient client;
        private ISharedPreferences preferences;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.FindOtherTravelers);

            preferences = PreferenceManager.GetDefaultSharedPreferences(ApplicationContext);

            mTimeTextView = FindViewById<TextView>(Resource.Id.otherTravelerTimeTextView);
            mFindOtherTravelerButton = FindViewById<Button>(Resource.Id.findOtherTravelerButton);
            mStartPlaceEdit = FindViewById<EditText>(Resource.Id.OtherTravelerStartPlaceEditText);
            mEndPlaceEdit = FindViewById<EditText>(Resource.Id.OtherTravelerEndPlaceEditText);

            mTravelTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
            mTimeTextView.Text = $"Czas: {mTravelTime.ToString("HH:mm")}";

            mTimeTextView.Click += (object sender, EventArgs e) =>
            {
                var timePickerFragment = new TimePickerDialogFragment(this, mTravelTime, this);
                timePickerFragment.Show(FragmentManager, null);
            };

            mFindOtherTravelerButton.Click += async (object sender, EventArgs e) =>
            {
                if (mStartPlaceEdit.Text.Equals("") || mEndPlaceEdit.Text.Equals(""))
                    Toast.MakeText(this, GetString(Resource.String.OtherTravelersNoDataInfo), ToastLength.Long).Show();
                else
                {
                    mStartPlace = mStartPlaceEdit.Text;
                    mEndPlace = mEndPlaceEdit.Text;
                    var token = JsonConvert.DeserializeObject<AuthenticationToken>(preferences.GetString("token", null));
                    if (token == null)
                    {
                        Toast.MakeText(this, GetString(Resource.String.LoginTokenOrTravelerFailure), ToastLength.Long)
                            .Show();
                        Finish();
                        StartActivity(typeof(LoginActivity));
                    }
                    var connectivityManager = (ConnectivityManager) GetSystemService(ConnectivityService);
                    var activeNetworkInfo = connectivityManager.ActiveNetworkInfo;
                    if (activeNetworkInfo == null || !activeNetworkInfo.IsConnected)
                    {
                        Toast.MakeText(this, GetString(Resource.String.NoConnectionInfo), ToastLength.Long).Show();
                        return;
                    }
                    var url = string.Format(GetString(Resource.String.ApiLink) 
                        + "/api/information/getothertravelersbyinformations?travelHour={0}&travelMinutes={1}&startPlace={2}&endPlace={3}",
                        mTravelTime.Hour, mTravelTime.Minute, mStartPlace, mEndPlace);
                    client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(token.TokenType, token.AccessToken);

                    var loadingMessage = ProgressDialog.Show(this,
                        GetString(Resource.String.OtherTravelersListLoadingTitle),
                        GetString(Resource.String.OtherTravelersListLoadingContent), true, false);
                    var response = await client.GetAsync(new Uri(url));
                    loadingMessage.Dismiss();

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var otherTravelersActivityIntent = new Intent(this, typeof (OtherTravelersActivity));
                        var putContent = new Bundle();
                        putContent.PutString("ResponseContent", content);
                        putContent.PutString("StartPlace", mStartPlace);
                        putContent.PutString("EndPlace", mEndPlace);
                        otherTravelersActivityIntent.PutExtras(putContent);
                        StartActivity(otherTravelersActivityIntent);
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                        Toast.MakeText(this, GetString(Resource.String.OtherTravelersNotFoundInfo), ToastLength.Long)
                            .Show();
                    else
                        Toast.MakeText(this, GetString(Resource.String.OtherTravelersNotOkInfo), ToastLength.Long)
                            .Show();
                }
            };
        }

        public void OnTimeSet(TimePicker view, int hourOfDay, int minute)
        {
            mTravelTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hourOfDay, minute, 0);
            mTimeTextView.Text = $"Czas: {mTravelTime.ToString("HH:mm")}";
        }
    }
}