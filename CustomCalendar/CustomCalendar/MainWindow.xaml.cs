using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Ookii.Dialogs.Wpf;

namespace CalendarJournal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window , INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public static DependencyProperty EntriesMadeProperty =
                DependencyProperty.Register
            (
                "EntriesMade",
                typeof(ObservableCollection<DateTime>),
                typeof(MainWindow)
            );

        public ObservableCollection<DateTime> EntriesMade
        {
            get { return (ObservableCollection<DateTime>)GetValue(EntriesMadeProperty); }
            set { SetValue(EntriesMadeProperty, value); }
        }


        public static DependencyProperty RootPathProperty =
        DependencyProperty.Register
            (
                "RootPath",
                typeof(string),
                typeof(MainWindow)
            );

        public string RootPath
        {
            get { return (string)GetValue(RootPathProperty); }
            set { SetValue(RootPathProperty, value); }
        }

        private readonly FileSystemWatcher _watcher = new FileSystemWatcher();

        public MainWindow()
        {
            SaveCommand = new CustomCommand(SaveExecute);
            UndoCommand = new CustomCommand(UndoExecute);
            DeleteCommand = new CustomCommand(DeleteExecute);
            OpenHyperlinkCommand = new CustomCommand(OpenHyperlinkExecute);

            DataContext = this;

            if (string.IsNullOrEmpty((string)Properties.Settings.Default["RootPath"]))
            {
                SelectRootDir();
            }
            else
            {
                RootPath = (string) Properties.Settings.Default["RootPath"];
                RegisterNewPath();
            }

            InitializeComponent();
            Calendar.SelectedDate = DateTime.Now;
        }

        private void SelectRootDir()
        {
            var dialog = new VistaFolderBrowserDialog
            {
                Description = "Please select a folder.",
                UseDescriptionForTitle = true
            };
            if (!VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
                MessageBox.Show("Because you are not using Windows Vista or later, the regular folder browser dialog will be used. Please use Windows Vista to see the new dialog.", "Sample folder browser dialog");
            var showDialog = dialog.ShowDialog();
            if (showDialog != null && (bool)showDialog)
            {
                RootPath = dialog.SelectedPath;
                //MessageBox.Show("The selected folder was: " + dialog.SelectedPath, "Sample folder browser dialog");
            }

            RegisterNewPath();
        }

        private void RegisterNewPath()
        {
            Properties.Settings.Default["RootPath"] = RootPath;
            Properties.Settings.Default.Save(); 

            _watcher.Path = RootPath;
            _watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            _watcher.Changed += OnPathEntriesChanged;
            _watcher.Created += OnPathEntriesChanged;
            _watcher.Deleted += OnPathEntriesChanged;

            _watcher.EnableRaisingEvents = true;

            UpdateEntriesMade();
        }

        private void OnPathEntriesChanged(object sender, FileSystemEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,new Action(UpdateEntriesMade));
        }

        private void UpdateEntriesMade()
        {
            var list = new ObservableCollection<DateTime>();

            if (!string.IsNullOrEmpty(RootPath))
            {
                foreach (var file in Directory.GetFiles(RootPath))
                {
                    var filename = Path.GetFileName(file);
                    if (filename != null)
                    {
                        var filenameparts = filename.Split('.');
                        DateTime date;
                        if (DateTime.TryParseExact(filenameparts[0], "yyyy-MM-dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out date))
                        {
                            list.Add(date);
                        }
                    }
                }
            }

            SetValue(EntriesMadeProperty, list);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SelectRootDir();
        }

        public ICommand SaveCommand
        {
            get;
            internal set;
        }


        public void SaveExecute()
        {
            DateEntryTextBox.SaveEntry();
        }

        public ICommand UndoCommand
        {
            get;
            internal set;
        }


        public void UndoExecute()
        {
            this.DateEntryTextBox.UndoEntry();
        }

        public ICommand DeleteCommand
        {
            get;
            internal set;
        }


        public void DeleteExecute()
        {
            DateEntryTextBox.DeleteEntry();
        }

        public ICommand OpenHyperlinkCommand
        {
            get;
            internal set;
        }


        public void OpenHyperlinkExecute()
        {
            System.Diagnostics.Process.Start("http://www.joesgoals.com/");
        }
    }

    public class CustomCommand : ICommand
    {
        private readonly Action _saveExecute;

        public CustomCommand(Action saveExecute)
        {
            _saveExecute = saveExecute;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _saveExecute();
        }
    }
}
