using System;
using System.Windows;

namespace CalendarJournal
{
    /// <summary>
    /// Interaction logic for AppointmentWindow.xaml
    /// </summary>
    public partial class AppointmentWindow : Window
    {
        private Action<Appointment> saveCallback;

        public AppointmentWindow(Action<Appointment> saveCallback)
        {
            InitializeComponent();

            this.saveCallback = saveCallback;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            Appointment appointment = new Appointment();
            appointment.Subject = subjectTbx.Text;
            appointment.Date = datePicker.SelectedDate.Value;

            saveCallback(appointment);
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }        
    }
}
