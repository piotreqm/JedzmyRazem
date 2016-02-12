using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Widget;
using Newtonsoft.Json;

namespace InzApp
{
    [Activity(Label = "@string/OtherTravelersLabel", MainLauncher = false)]
    public class OtherTravelersActivity : Activity
    {
        private TextView mTitleTextView;
        private ListView mOtherTravelersMessagesListView;
        
        private List<Traveler> mItems;
        private string mStartPlace;
        private string mEndPlace;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.OtherTravelers);

            mTitleTextView = FindViewById<TextView>(Resource.Id.OtherTravelersMessagesTitleTextView);
            mOtherTravelersMessagesListView = FindViewById<ListView>(Resource.Id.OtherTravelersMessagesListView);
            mItems = new List<Traveler>();
            
            var loadingMessage = ProgressDialog.Show(this,
                GetString(Resource.String.OtherTravelersListLoadingTitle),
                GetString(Resource.String.OtherTravelersListLoadingContent), true, false);
            var getContentTravelersList = Intent.GetStringExtra("ResponseContent");

            mStartPlace = Intent.GetStringExtra("StartPlace");
            mEndPlace = Intent.GetStringExtra("EndPlace");
            mTitleTextView.Text = string.Format(GetString(Resource.String.OtherTravelersListTitle)
                + "\n{0} - {1}", mStartPlace, mEndPlace);

            mItems.Clear();
            var travelers = JsonConvert.DeserializeObject<List<Traveler>>(getContentTravelersList);
            foreach (var traveler in travelers)
            {
                mItems.Add(new Traveler
                {
                    Id = traveler.Id,
                    Name = traveler.Name,
                    Surname = traveler.Surname,
                    DateOfBirth = traveler.DateOfBirth
                });
            }

            var adapter = new OtherTravelersListViewAdapter(this, mItems);
            mOtherTravelersMessagesListView.Adapter = adapter;
            loadingMessage.Dismiss();
        }
    }
}