using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace CalendarJournal
{

    [ValueConversion(typeof(ObservableCollection<Entry>), typeof(ObservableCollection<Entry>))]
    public class EntriesMadeConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var date = (DateTime)values[1];

            if ((ObservableCollection<Entry>)values[0] == null)
            {
                return new ObservableCollection<Entry>();
            }

            var entry = ((ObservableCollection<Entry>) values[0]).FirstOrDefault(entryExistsAt =>
                entryExistsAt.DateTime.Date == date.Date);

            if (entry != null)
            {
                return new ObservableCollection<Entry> { entry };
            }

            return new ObservableCollection<Entry>();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
