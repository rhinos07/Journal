using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading;
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
                typeof(ObservableCollection<Entry>),
                typeof(MainWindow)
            );

        public ObservableCollection<Entry> EntriesMade
        {
            get { return (ObservableCollection<Entry>)GetValue(EntriesMadeProperty); }
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
            EditCommand = new CustomCommand(EditExecute);
            UndoCommand = new CustomCommand(UndoExecute);
            DeleteCommand = new CustomCommand(DeleteExecute);
            OpenHyperlinkCommand = new CustomCommand(OpenHyperlinkExecute);

            DataContext = this;

            RootPath = (string)Properties.Settings.Default["RootPath"];

            if (!CheckRootPath(RootPath))
            {
                RootPath = null;
            }

            if (string.IsNullOrEmpty(RootPath))
            {
                SelectRootPath();
            }


            if (string.IsNullOrEmpty(RootPath))
            {
                MessageBox.Show("No path to entires");
            }
            else
            {
                RegisterNewPath(RootPath);
            }




            InitializeComponent();
            Calendar.SelectedDate = DateTime.Now;
        }

        private bool CheckRootPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            if (!Directory.Exists(path))    
                return false;

            return true;    
        }

        private void SelectRootPath()
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
        }

        private void RegisterNewPath(string rootPath)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private DateTime lastRefresh = DateTime.MinValue;

        private void OnPathEntriesChanged(object sender, FileSystemEventArgs e)
        {
            if (lastRefresh > DateTime.Now.Subtract(TimeSpan.FromSeconds(10)))
            { 
                return;
            }

            lastRefresh = DateTime.Now;

            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,new Action(UpdateEntriesMade));
        }

        private void UpdateEntriesMade()
        {
            var list = new ObservableCollection<Entry>();

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
                            var entry = new Entry {DateTime = date};

                            entry.Text = ParseContents(file);

                            list.Add(entry);
                        }
                    }
                }
            }

            SetValue(EntriesMadeProperty, list);
        }

        private string ParseContents(string file)
        {
            var text = "";
            var contents = File.ReadAllText(file).ToLower();

            if (contents.Contains("kniebeuge"))
                text = text + "SQ ";

            if (contents.Contains("squat"))
                text = text + "SQ ";

            if (contents.Contains("kreuzheben"))
                text = text + "DL ";

            if (contents.Contains("deadlift"))
                text = text + "DL ";

            if (contents.Contains("bankdrücken"))
                text = text + "BP ";

            if (contents.Contains("bench press"))
                text = text + "BP ";
            else if (contents.Contains("press "))
                text = text + "OHP ";

            if (contents.Contains("overhead press"))
                text = text + "OHP ";

            if (contents.Contains("power clean"))
                text = text + "PC ";

            if (contents.Contains("pistol"))
                text = text + "Pi ";
            
            if (contents.Contains("schwimmen"))
                text = text + "Schw ";

            if (contents.Contains("liegestütz"))
                text = text + "Li ";

            if (contents.Contains("bauch"))
                text = text + "B ";

            if (contents.Contains("beinheben"))
                text = text + "B ";

            if (contents.Contains("rücken") && !contents.Contains("drücken"))
                text = text + "R ";

            if (contents.Contains("klimmzug"))
                text = text + "Kz ";

            if (contents.Contains("dip"))
                text = text + "Di ";
            
            if (contents.Contains("handstand"))
                text = text + "Hs ";

            if (contents.Contains("joggen"))
                text = text + "Jo ";

            if (contents.Contains("radfahren"))
                text = text + "Ff ";

            if (contents.Contains("mtb"))
                text = text + "Ff ";

            if (contents.Contains("yoga"))
                text = text + "Yo ";

            if (contents.Contains("trifecta"))
                text = text + "Tri ";

            if (contents.Contains("krank"))
                text = text + "Krank ";

            return text;
        }

        private void Button_Configuration_Click(object sender, RoutedEventArgs e)
        {
            SelectRootPath();

            if (!string.IsNullOrEmpty(RootPath))
            {
                RegisterNewPath(RootPath);
            }
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

        public ICommand EditCommand
        {
            get;
            internal set;
        }

        public void EditExecute()
        {
            DateEntryTextBox.Focus();
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
