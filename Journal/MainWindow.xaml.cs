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

            if (contents.Contains("bankdrücke"))
                text = text + "BP ";

            if (contents.Contains("bench press"))
                text = text + "BP ";
            else if (contents.Contains("press "))
                text = text + "OHP ";


            if (contents.Contains("overhead press"))
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

            if (contents.Contains("rücken"))
                text = text + "R ";

            if (contents.Contains("klimmzug"))
                text = text + "Kz ";

            if (contents.Contains("dip"))
                text = text + "Di ";
            
            if (contents.Contains("handstand"))
                text = text + "Hs ";

            if (contents.Contains("trifecta"))
                text = text + "Tri ";

            if (contents.Contains("oggen"))
                text = text + "Jo ";

            if (contents.Contains("ahrradfahren"))
                text = text + "Ff ";
            
            return text;
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
