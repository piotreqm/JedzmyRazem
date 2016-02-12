using System;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Widget;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;
using Android.Net;
using Uri = System.Uri;

namespace InzApp
{
    [Activity(Label = "@string/RegisterLabel", MainLauncher = false)]
    public class RegisterActivity : Activity, DatePickerDialog.IOnDateSetListener
    {
        private EditText mRegEmail;
        private EditText mRegPw;
        private EditText mRegPwConfirm;
        private EditText mRegName;
        private EditText mRegSurname;
        private TextView mRegDoB;
        private Button mRegButton;

        private string mEmail;
        private string mPw;
        private string mPwConfirm;
        private string mName;
        private string mSurname;
        private DateTime mDateOfBirth;

        private HttpClient client;
        private UserRegisterBinding newUser;
        private Traveler newTraveler;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Register);

            mRegEmail = FindViewById<EditText>(Resource.Id.regEMailEditText);
            mRegPw = FindViewById<EditText>(Resource.Id.regPwEditText);
            mRegPwConfirm = FindViewById<EditText>(Resource.Id.regPwConfirmEditText);
            mRegName = FindViewById<EditText>(Resource.Id.regNameEditText);
            mRegSurname = FindViewById<EditText>(Resource.Id.regSurnameEditText);
            mRegDoB = FindViewById<TextView>(Resource.Id.regDoBTextView);
            mRegButton = FindViewById<Button>(Resource.Id.regButton);

            mDateOfBirth = new DateTime(1970, 1, 1);
            mRegDoB.Click += (object sender, EventArgs e) =>
            {
                var datePickerFragment = new DatePickerDialogFragment(this, mDateOfBirth.AddMonths(-1), this);
                datePickerFragment.Cancelable = false;
                datePickerFragment.Show(FragmentManager, null);
            };

            mRegButton.Click += async (object sender, EventArgs e) =>
            {
                if (AreFieldsFilled())
                    if (Regex.Match(mRegEmail.Text, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$").Success)
                        if (IsPasswordMatchingPolicy())
                            if (mRegPw.Text.Equals(mRegPwConfirm.Text))
                            {
                                mEmail = mRegEmail.Text;
                                mPw = mRegPw.Text;
                                mPwConfirm = mRegPwConfirm.Text;
                                mName = mRegName.Text;
                                mSurname = mRegSurname.Text;
                                //ProgressDialog
                                var loadingMessage = ProgressDialog.Show(this,
                                    GetString(Resource.String.SendingUserRegisterTitle
                                        ), GetString(Resource.String.SendingUserRegisterContent), true, false);
                                //nowy u¿ytkownik
                                newUser = new UserRegisterBinding
                                {
                                    Email = mEmail,
                                    Password = mPw,
                                    ConfirmPassword = mPwConfirm
                                };

                                var connectivityManager = (ConnectivityManager) GetSystemService(ConnectivityService);
                                var activeNetworkInfo = connectivityManager.ActiveNetworkInfo;
                                if (activeNetworkInfo == null || !activeNetworkInfo.IsConnected)
                                {
                                    loadingMessage.Dismiss();
                                    Toast.MakeText(this, GetString(Resource.String.NoConnectionInfo), ToastLength.Long)
                                        .Show();
                                    return;
                                }

                                var responseMessage = await RegisterUser(newUser);
                                if (responseMessage.IsSuccessStatusCode)
                                {
                                    var responseMessageContent = await responseMessage.Content.ReadAsStringAsync();
                                    var userId = JsonConvert.DeserializeObject<string>(responseMessageContent);

                                    //nowy podró¿uj¹cy
                                    newTraveler = new Traveler
                                    {
                                        Name = mName,
                                        Surname = mSurname,
                                        DateOfBirth = mDateOfBirth,
                                        UserId = userId
                                    };

                                    responseMessage = await AddTraveler(newTraveler);

                                    if (!responseMessage.IsSuccessStatusCode)
                                        return;
                                    loadingMessage.Dismiss();
                                    SetResult(Result.Ok);
                                    Finish();
                                }
                                else
                                {
                                    loadingMessage.Dismiss();
                                    Toast.MakeText(this, GetString(Resource.String.RegisterUserModelNotOkInfo),
                                        ToastLength.Long).Show();
                                }
                            }
                            else
                                Toast.MakeText(this, GetString(Resource.String.RegisterUserPwsNotEqualInfo),
                                    ToastLength.Long).Show();
                        else
                        {
                            for (var i = 0; i < 2; i++) 
                            Toast.MakeText(this, GetString(Resource.String.RegisterUserPwNotMatchingPolicyInfo),
                                ToastLength.Long).Show();
                        }
                    else
                        Toast.MakeText(this, GetString(Resource.String.RegisterUserEmailNotCorrectInfo),
                            ToastLength.Long).Show();
                else Toast.MakeText(this, GetString(Resource.String.RegisterUserNoDataInfo), ToastLength.Long).Show();
            };
        }

        private async Task<HttpResponseMessage> RegisterUser(UserRegisterBinding newUser)
        {
            var url = GetString(Resource.String.ApiLink) + "/api/Account/Register";
            client = new HttpClient();

            var json = JsonConvert.SerializeObject(newUser);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(new Uri(url), content);
            return response;
        }

        private async Task<HttpResponseMessage> AddTraveler(Traveler newTraveler)
        {
            var url = GetString(Resource.String.ApiLink) + "/api/traveler/posttraveler";
            client = new HttpClient();

            var json = JsonConvert.SerializeObject(newTraveler);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(new Uri(url), content);
            return response;
        }

        private bool AreFieldsFilled()
        {
            return !mRegEmail.Text.Equals("") && !mRegName.Text.Equals("")
                   && !mRegName.Text.Equals("") && !mRegSurname.Text.Equals("")
                   && !mRegPwConfirm.Text.Equals("") && !mRegDoB.Text.Equals("Data urodzenia: ");
        }

        private bool IsPasswordMatchingPolicy()
        {
            var condition = mRegPw.Text.Length > 5 && mRegPw.Text.Any(c => !char.IsLetterOrDigit(c)) 
                && mRegPw.Text.Any(char.IsDigit) && mRegPw.Text.Any(char.IsLower) 
                && mRegPw.Text.Any(char.IsUpper);
            return condition;
        }

        public void OnDateSet(DatePicker view, int year, int monthOfYear, int dayOfMonth)
        {
            mDateOfBirth = new DateTime(year, monthOfYear + 1, dayOfMonth);
            mRegDoB.Text = $"Data urodzenia: {mDateOfBirth.ToString("d MMMM yyyy")}";
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            SetResult(Result.Canceled);
        }
    }
}