using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace CalendarJournal
{

    [ValueConversion(typeof(ObservableCollection<DateTime>), typeof(ObservableCollection<DateTime>))]
    public class EntriesMadeConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var date = (DateTime)values[1];

            if ((ObservableCollection<DateTime>) values[0] == null)
            {
                return new ObservableCollection<DateTime>();
            }

            if (((ObservableCollection<DateTime>)values[0]).Any(entryExistsAt => entryExistsAt.Date == date.Date))
            {
                return new ObservableCollection<DateTime>{date.Date};
            }

            return new ObservableCollection<DateTime>();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
