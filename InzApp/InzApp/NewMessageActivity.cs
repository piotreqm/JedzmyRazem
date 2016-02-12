using System;
using System.Text;
using Android.App;
using Android.OS;
using Android.Widget;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Android.Content;
using Android.Net;
using Android.Preferences;
using Uri = System.Uri;

namespace InzApp
{
    [Activity(Label = "@string/NewMessageLabel", MainLauncher = false)]
    public class NewMessageActivity : Activity, TimePickerDialog.IOnTimeSetListener
    {
        private TextView mTimeTextView;
        private EditText mStartPlaceEditText;
        private EditText mEndPlaceEditText;
        private Button mAddButton;
        
        private DateTime mTime;
        private string mStartPlace;
        private string mEndPlace;
        private Information newMessage;

        HttpClient client;
        private ISharedPreferences preferences;
        private AuthenticationToken token;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.NewMessage);

            preferences = PreferenceManager.GetDefaultSharedPreferences(ApplicationContext);
            token = JsonConvert.DeserializeObject<AuthenticationToken>(preferences.GetString("token", null));

            mTimeTextView = FindViewById<TextView>(Resource.Id.timeTextView);
            mStartPlaceEditText = FindViewById<EditText>(Resource.Id.startPlaceEditText);
            mEndPlaceEditText = FindViewById<EditText>(Resource.Id.endPlaceEditText);
            mAddButton = FindViewById<Button>(Resource.Id.addButton);

            mTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
            mTimeTextView.Text = $"Czas: {mTime.ToString("HH:mm")}";

            mTimeTextView.Click += (object sender, EventArgs e) =>
            {
                var timePickerFragment = new TimePickerDialogFragment(this, mTime, this);
                timePickerFragment.Show(FragmentManager, null);
            };

            mAddButton.Click += async (object sender, EventArgs e) =>
            {   //sprawdzenie poprawno�ci wprowadzonych przez u�ytkownika danych
                if (!mStartPlaceEditText.Text.Equals("") && !mEndPlaceEditText.Text.Equals(""))
                {
                    mStartPlace = mStartPlaceEditText.Text;
                    mEndPlace = mEndPlaceEditText.Text;
                    var traveler = JsonConvert.DeserializeObject<Traveler>(preferences.GetString("traveler", null));
                    if (traveler == null || token == null)
                    {
                        Toast.MakeText(this, GetString(Resource.String.LoginTokenOrTravelerFailure), ToastLength.Long).Show();
                        Finish();
                        StartActivity(typeof(LoginActivity));
                    }
                    else
                    {
                        //utworzenie nowego obiektu - wiadomo�ci
                        newMessage = new Information
                        {
                            TravelerId = traveler.Id,
                            StartPlace = mStartPlace,
                            EndPlace = mEndPlace,
                            Time = mTime
                        };
                        //sprawdzenie, czy �rodowisko jest po��czone z sieci� Internet
                        var connectivityManager = (ConnectivityManager) GetSystemService(ConnectivityService);
                        var activeNetworkInfo = connectivityManager.ActiveNetworkInfo;
                        if (activeNetworkInfo == null || !activeNetworkInfo.IsConnected)
                        {
                            Toast.MakeText(this, GetString(Resource.String.NoConnectionInfo), ToastLength.Long).Show();
                            return;
                        }
                        //wywo�anie okna dialogowego progresu
                        var loadingMessage = ProgressDialog.Show(this, GetString(Resource.String.SendingMessageTitle),
                            GetString(Resource.String.SendingMessageContent), true, false);

                        //wywo�anie metody obs�uguj�cej ��danie http i oczekiwanie na wiadomo��
                        var apiMessage = await SaveMyMessage(newMessage);
                        loadingMessage.Dismiss();
                        if (apiMessage.IsSuccessStatusCode)
                        {
                            //ustawienie pozytywnego rezultatu operacji i zako�czenie aktywno�ci
                            SetResult(Result.Ok);
                        }
                        Finish();
                    }
                }
                else Toast.MakeText(this, GetString(Resource.String.NewMessageNoDataInfo), ToastLength.Long).Show();
            };
        }
        //asynchroniczna metoda typu TASK (odr�bny w�tek) wysy�aj�ca ��danie POST dodania wiadomo�ci
        private async Task<HttpResponseMessage> SaveMyMessage(Information message)
        {   //utworzenie adresu aplikacji web api
            var url = GetString(Resource.String.ApiLink) + "/api/information/postinformation";
            client = new HttpClient();
            //dodanie tokena autoryzacji zalogowanego klienta
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(token.TokenType, token.AccessToken);
            //przetworzenie obiektu na dane komunikacyjne JSON
            var json = JsonConvert.SerializeObject(message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            //wys�anie ��dania i oczekiwanie na odpowied� web api
            var response = await client.PostAsync(new Uri(url), content);
            return response;
        }

        public void OnTimeSet(TimePicker view, int hourOfDay, int minute)
        {
            mTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hourOfDay, minute, 0);
            mTimeTextView.Text = $"Czas: {mTime.ToString("HH:mm")}";
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            SetResult(Result.Canceled);
        }
    }
}