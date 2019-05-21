using GuestListManager.Data;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace GuestListManager.Converters
{
    public class GuestCheckedInCounterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value != null)
            {
                ObservableCollection<Guest> guests = (ObservableCollection<Guest>)value;
                return guests.Where(guest => guest.IsCheckedIn).Count(); ;
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
