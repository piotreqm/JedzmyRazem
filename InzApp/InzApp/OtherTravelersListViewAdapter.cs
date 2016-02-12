using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using System;

namespace InzApp
{
    class OtherTravelersListViewAdapter : BaseAdapter<Traveler>
    {
        public List<Traveler> mItems;
        private Context mContext;

        public OtherTravelersListViewAdapter(Context context, List<Traveler> items)
        {
            mContext = context;
            mItems = items;
        }

        public override int Count
        {
            get
            {
                return mItems.Count;
            }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override Traveler this[int position]
        {
            get
            {
                return mItems[position];
            }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View row = convertView;

            if (row == null)
            {
                row = LayoutInflater.From(mContext).Inflate(Resource.Layout.OtherTravelersListViewRow, null);
            }

            TextView txtName = row.FindViewById<TextView>(Resource.Id.OtherTravelersMessagesNameTextView);
            txtName.Text = mItems[position].Name;

            TextView txtSurname = row.FindViewById<TextView>(Resource.Id.OtherTravelersMessagesSurnameTextView);
            txtSurname.Text = mItems[position].Surname;

            TextView txtDoB = row.FindViewById<TextView>(Resource.Id.OtherTravelersMessagesAgeTextView);
            int age = DateTime.Today.Year - mItems[position].DateOfBirth.Year;
            if (mItems[position].DateOfBirth > DateTime.Today.AddYears(-age)) age--;
            txtDoB.Text = age.ToString();

            return row;
        }

    }
}