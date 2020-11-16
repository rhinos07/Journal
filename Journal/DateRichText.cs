using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Ookii.Dialogs.Wpf;

//using Ookii.Dialogs.Wpf;

namespace CalendarJournal
{
    public class DateRichTextBox :  RichTextBox
    {

        public static readonly DependencyProperty DateProperty = DependencyProperty.Register("Date", typeof(DateTime?), 
            typeof(DateRichTextBox), new FrameworkPropertyMetadata(DateTime.MinValue, OnCurrentTimePropertyChanged));

        public DateTime Date { get; set; }

        private static void OnCurrentTimePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as DateRichTextBox;

            if (control != null)
            {
                if (e.NewValue != null)
                {
                    control.Date = (DateTime) e.NewValue;
                    control.LoadDate((DateTime) e.NewValue);
                }
            }
        }

        public static readonly DependencyProperty RootPathProperty = DependencyProperty.Register("RootPath", typeof(string),
            typeof(DateRichTextBox), new FrameworkPropertyMetadata(string.Empty, OnRootPathChanged));

        private static void OnRootPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as DateRichTextBox;

            if (control != null)
            {
                control.RootPath = (string) e.NewValue;
                control.LoadDate(control.Date);
            }
        }

        public string RootPath
        {
            get; 
            set;
        }

        private string _originalText;
        private DateTime _originalDate = DateTime.MinValue;

        private void LoadDate(DateTime newValue)
        {
            if (string.IsNullOrEmpty(RootPath))
                return;

            if (Date == DateTime.MinValue)
                return;

            if (_originalText != null && _originalText != DocumentToString(Document))
            {
                //if (MessageBox.Show("abspeichern ?", "nicht gespeicherte Änderungen", MessageBoxButton.YesNo) ==
                //    MessageBoxResult.Yes)
                {
                    DoSaveEntry(_originalDate, DocumentToString(Document));
                }
            }


            LoadDocument(newValue);

            _originalText = DocumentToString(Document);
            _originalDate = newValue;
        }

        private void DoSaveEntry(DateTime originalDate, string text)
        {
            if (originalDate == DateTime.MinValue)
                return;

            if (string.IsNullOrEmpty(RootPath))
            {
                SelectRootDir();
            }


            if (!string.IsNullOrEmpty(RootPath))
            {
                File.WriteAllText(RootPath + "\\" + originalDate.ToString("yyyy-MM-dd") + ".txt", text);
            }
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
                MessageBox.Show("The selected folder was: " + dialog.SelectedPath, "Sample folder browser dialog");

            }
            
        }

        private void LoadDocument(DateTime newValue)
        {
            if (string.IsNullOrEmpty(RootPath))
            {
                SelectRootDir();
            }


            Document = GenerateDefaultConent(newValue);

            if (File.Exists(RootPath + "\\" + newValue.ToString("yyyy-MM-dd") + ".txt"))
            {
                using (var fs = new FileStream(RootPath + "\\" + newValue.ToString("yyyy-MM-dd") + ".txt", FileMode.OpenOrCreate))
                {

                    var range = new TextRange(Document.ContentStart, Document.ContentEnd);
                    range.Load(fs, DataFormats.Text);
                }
            }
        }

        private FlowDocument GenerateDefaultConent(DateTime newValue)
        {
            var myFlowDoc = new FlowDocument();
            myFlowDoc.Blocks.Add(new Paragraph(new Run(newValue.ToLongDateString())));



            myFlowDoc.Blocks.Add(new Paragraph(new Run(GetCaption(newValue.DayOfWeek))));
            myFlowDoc.Blocks.Add(new Paragraph(new Run("5 Werte")));
            myFlowDoc.Blocks.Add(new Paragraph(new Run(GetTraining(newValue.DayOfWeek))));

            return myFlowDoc;
        }

        private string GetCaption(DayOfWeek dayOfWeek)
        {
            string caption = "";
            switch (dayOfWeek)
            {
                    case DayOfWeek.Monday:
                    caption = "Danke sagen";
                    break;
                    case DayOfWeek.Tuesday:
                                        caption = "Bombige Zeiten";
                                        break;
                    case DayOfWeek.Wednesday:
                                        caption = "Phantastische Zukunft";
                                        break;
                    case DayOfWeek.Thursday:
                                        caption = "Lieber";                                        
                                        break;
                    case DayOfWeek.Friday:
                                        caption = "Rückschau halten";
                                        break;
                                        

            }


            return caption;
        }

        private string GetTraining(DayOfWeek dayOfWeek)
        {
            string training = "";
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    training = "";
                    break;
                case DayOfWeek.Tuesday:
                    training = "Brücken";
                    break;
                case DayOfWeek.Wednesday:
                    training = "Handstand";
                    break;
                case DayOfWeek.Thursday:
                    training = "Beinheben";
                    break;
                case DayOfWeek.Friday:
                    training = "Kniebeugen";
                    break;
                case DayOfWeek.Saturday:
                    training = "Liegestütze";
                    break;
                case DayOfWeek.Sunday:
                    training = "Klimmzüge";
                    break;


            }


            return training;
        }

        private string DocumentToString(FlowDocument doc)
        {
            return new TextRange(doc.ContentStart, doc.ContentEnd).Text;
        }

        public void DeleteEntry()
        {
            if (MessageBox.Show("Wirklich löschen?", "Tagebuch", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (_originalDate == DateTime.MinValue)
                    return;

                if (string.IsNullOrEmpty(RootPath))
                {
                    return;
                }

                if (!string.IsNullOrEmpty(RootPath))
                {
                    File.Delete(RootPath + "\\" + _originalDate.ToString("yyyy-MM-dd") + ".txt");
                }

                // alte Werte laden
                _originalText = null;
                LoadDate(_originalDate);
            }
        }

        public void UndoEntry()
        {
            var range = new TextRange(Document.ContentStart, Document.ContentEnd);
            range.Text = _originalText;
        }

        public void SaveEntry()
        {
            DoSaveEntry(_originalDate, DocumentToString(Document));

            // abgespeicherten Werte laden
            _originalText = null;
            LoadDate(_originalDate);
        }
    }
}
