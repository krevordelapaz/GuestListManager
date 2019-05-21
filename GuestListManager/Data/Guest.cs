using Prism.Mvvm;
using System;

namespace GuestListManager.Data
{
    public class Guest : BindableBase
    {
        private string name;

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                SetProperty(ref name, value);
            }
        }

        private string designatedSeats;

        public string DesignatedSeats
        {
            get
            {
                return designatedSeats;
            }
            set
            {
                SetProperty(ref designatedSeats, value);
            }
        }

        private bool hasFan;

        public bool HasFan
        {
            get
            {
                return hasFan;
            }
            set
            {
                SetProperty(ref hasFan, value);
            }
        }

        private bool isCheckedIn;

        public bool IsCheckedIn
        {
            get
            {
                return isCheckedIn;
            }
            set
            {
                SetProperty(ref isCheckedIn, value);
            }
        }


        private string checkInTime;

        public string CheckInTime
        {
            get
            {
                return checkInTime;
            }
            set
            {
                SetProperty(ref checkInTime, value);
            }
        }

        private string id;

        public string Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

    }
}
