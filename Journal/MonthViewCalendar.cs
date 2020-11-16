using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace CalendarJournal
{
    /// <summary>
    /// Custom calendar control that supports appointments.
    /// </summary>
    public class MonthViewCalendar : Calendar, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public static readonly DependencyProperty AppointmentsProperty =
            DependencyProperty.Register
            (
                "Appointments",
                typeof(ObservableCollection<Appointment>),
                typeof(Calendar)
            );



        public static readonly DependencyProperty EntriesMadeProperty =
            DependencyProperty.Register
            (
                "EntriesMade",
                typeof(ObservableCollection<Entry>),
                typeof(Calendar)
            );


        /// <summary>
        /// The list of appointments. This is a dependency property.
        /// </summary>
        public ObservableCollection<Appointment> Appointments
        {
            get { return (ObservableCollection<Appointment>)GetValue(AppointmentsProperty); }
            set { SetValue(AppointmentsProperty, value); }
        }


        public ObservableCollection<Entry> EntriesMade
        {
            get { return (ObservableCollection<Entry>)GetValue(EntriesMadeProperty); }
            set { SetValue(EntriesMadeProperty, value); }
        }


        static MonthViewCalendar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MonthViewCalendar), new FrameworkPropertyMetadata(typeof(MonthViewCalendar)));
        }

        public MonthViewCalendar()
            : base()
        {
            //SetValue(AppointmentsProperty, new ObservableCollection<Appointment>());

        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            //FrameworkElement element = e.OriginalSource as FrameworkElement;

            //if (element.DataContext is DateTime)
            //{
            //    AppointmentWindow appointmentWindow = new AppointmentWindow
            //    (
            //        (Appointment appointment) =>
            //        {
            //            Appointments.Add(appointment);
            //            if (PropertyChanged != null)
            //            {
            //                PropertyChanged(this, new PropertyChangedEventArgs("Appointments"));
            //            }
            //        }
            //    );
            //    appointmentWindow.Show();
            //}
        }
    }


    public class Entry
    {
        public DateTime DateTime { get; set; }

        public string Text { get; set; }
    }
}
