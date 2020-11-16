using System;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace CalendarJournal
{
    /// <summary>
    /// Gets the appointments for the specified date.
    /// </summary>
    [ValueConversion(typeof(ObservableCollection<Appointment>), typeof(ObservableCollection<Appointment>))]
    public class AppointmentsConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime date = (DateTime)values[1];

            ObservableCollection<Appointment> appointments = new ObservableCollection<Appointment>();

            var test = (ObservableCollection<Appointment>) values[0];

            if (test == null)
            {
                return appointments;
            }

            foreach (Appointment appointment in (ObservableCollection<Appointment>)values[0])
            {
                if (appointment.Date.Date == date)
                {
                    appointments.Add(appointment);
                }
            }

            return appointments;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
