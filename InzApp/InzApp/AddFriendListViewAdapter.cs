using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace InzApp
{
    class AddFriendListViewAdapter : BaseAdapter<Traveler>
    {
        public List<Traveler> mItems;
        private Context mContext;

        public AddFriendListViewAdapter(Context context, List<Traveler> items)
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
                row = LayoutInflater.From(mContext).Inflate(Resource.Layout.AddFriendListViewRow, null);
            }

            TextView txtName = row.FindViewById<TextView>(Resource.Id.nameTextView);
            txtName.Text = mItems[position].Name;

            TextView txtSurname = row.FindViewById<TextView>(Resource.Id.surnameTextView);
            txtSurname.Text = mItems[position].Surname;

            TextView txtDoB = row.FindViewById<TextView>(Resource.Id.dateOfBirthTextView);
            txtDoB.Text = mItems[position].DateOfBirth.ToString("d MMMM yyyy");

            return row;
        }

    }
}