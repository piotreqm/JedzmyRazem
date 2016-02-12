using System;
using Android.App;
using Android.Content;
using Android.OS;

namespace InzApp
{
    public class DatePickerDialogFragment : DialogFragment
    {
        private readonly Context mContext;
        private DateTime mDob;
        private readonly DatePickerDialog.IOnDateSetListener mListener;

        public DatePickerDialogFragment(Context context, DateTime Dob, DatePickerDialog.IOnDateSetListener listener)
        {
            mContext = context;
            mDob = Dob;
            mListener = listener;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            return new DatePickerDialog(mContext, mListener, mDob.Year, mDob.Month, mDob.Day);
        }
    }
}