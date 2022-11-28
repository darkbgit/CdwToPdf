using CdwToPdf.Core;
using CdwToPdf.Core.Converter;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace CdwToPdf
{
    internal delegate void UpdateProgressBarDelegate(DependencyProperty dp, object value);
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BackgroundWorker _worker;

        public ObservableCollection<DrawingFileInfo> Drawings { get; private set; } = new();

        private const string KOMPAS_API = "KOMPAS.Application.7";
        //private const string KOMPAS_PATH_PDF_CONVERTER = @"C:\Program Files\ASCON\KOMPAS-3D v20\Bin\Pdf2d.dll";
        private const string KOMPAS_PATH_PDF_CONVERTER = @"C:\Program Files\ASCON\KOMPAS-3D v21\Bin\Pdf2d.dll";

        public MainWindow()
        {
            InitializeComponent();
            _worker = new BackgroundWorker();
            _worker.DoWork += Worker_DoWork;
            DataContext = this;
        }

        private void Worker_DoWork(object? sender, DoWorkEventArgs e)
        {
            ConvertFiles();
        }

        private void ConvertFiles()
        {
            UpdateProgressBarDelegate updProgress = pbConvert.SetValue;

            double value = 0;

            var converter = DrawingConverter.GetConverter(KOMPAS_PATH_PDF_CONVERTER, KOMPAS_API);

            if (converter == null)
            {
                MessageBox.Show("Couldn't create Kompas converter");
                return;
            }

            _ = Dispatcher.Invoke(updProgress, RangeBase.ValueProperty, ++value);
            if (!Drawings.Any()) return;

            using var targetDoc = new PdfDocument();

            foreach (var file in Drawings)
            {
                var lastIndex = file.Path.LastIndexOf('\\');

                if (lastIndex == -1)
                {
                    MessageBox.Show($"Wrong path for file {file.Path}");
                    continue;
                }
                var pdfFilePath = file.Path[..(lastIndex + 1)] + file + ".pdf";

                if (converter.Convert(file.Path, pdfFilePath, 0, false) == 0)
                {
                    MessageBox.Show($"Couldn't convert to pdf file {file.Path}");
                }

                using var pdfDoc = PdfReader.Open(pdfFilePath, PdfDocumentOpenMode.Import);
                try
                {
                    for (var i = 0; i < pdfDoc.PageCount; i++)
                    {
                        targetDoc.AddPage(pdfDoc.Pages[i]);
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show($"Couldn't add to pdf file {pdfFilePath}");
                }


                pdfDoc.Close();

                File.Delete(pdfFilePath);

                _ = Dispatcher.Invoke(updProgress, RangeBase.ValueProperty, ++value);
            }

            var first = Drawings.First();

            var newFilepath = first.Path
                [..(Drawings.First().Path.LastIndexOf('\\') + 1)] + first.Designation + " - " + first.Name;
            targetDoc.Save(newFilepath + ".pdf");

            MessageBox.Show("Completed");
        }




        private void BtnConvert_Click(object sender, RoutedEventArgs e)
        {
            btnConvert.IsEnabled = false;

            pbConvert.IsEnabled = true;
            pbConvert.Maximum = Drawings.Count + 1;
            pbConvert.Value = 0;

            _worker.RunWorkerAsync();
        }

        private void BtnChooseDir_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Выберите папку",
                UseDescriptionForTitle = true
                //ShowNewFolderButton = true,
            };

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            Drawings.Clear();

            var path = dialog.SelectedPath;

            var fileExtensions = cbWith3D.IsChecked.HasValue && cbWith3D.IsChecked.Value
                ? new[] { "cdw", "spw", "m3d", "a3d" } : new[] { "cdw", "spw" };

            List<string> files = (cbSubdirs.IsChecked.GetValueOrDefault()
                ? System.IO.Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                : System.IO.Directory.GetFiles(path))
                .Where(f => fileExtensions.Contains(f[^3..]))
                .ToList();

            var errorList = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    Drawings.Add(new DrawingFileInfo(file));
                }
                catch (Exception exception)
                {
                    errorList.Add(file + " " + exception.Message);
                }
            }
            
            Drawings.Sort((a, b) => a.Designation.CompareTo(b.Designation));

            if (errorList.Any())
            {
                MessageBox.Show(string.Join(Environment.NewLine, errorList));
            }

            //Drawings = new ObservableCollection<DrawingFileInfo>(Drawings.OrderBy(f => f.Designation));

            //lvFiles.ItemsSource = Drawings;

            btnConvert.IsEnabled = true;
            btnRename.IsEnabled = true;
        }

        private void BtnChooseFile_Click(object sender, RoutedEventArgs e)
        {

            using var dialog = new OpenFileDialog()
            {
                //Description = "Выберите папку",
                //UseDescriptionForTitle = true,
                //ShowNewFolderButton = true,
            };

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            Drawings.Clear();

            var path = dialog.FileName;

            try
            {
                Drawings.Add(new DrawingFileInfo(path));
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

            //lvFiles.ItemsSource = Drawings;

            btnConvert.IsEnabled = true;
            btnRename.IsEnabled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //lbFiles.ItemsSource = _filesToConvert;
        }

        private void MoveSelectedItemListBox(System.Windows.Controls.ListView lv, int idx, bool moveUp)
        {
            if (Drawings.Count <= 1) return;

            int offset = 0;

            if (idx >= 0)
            {
                offset = moveUp ? -1 : 1;
            }

            if (offset != 0)
            {
                int selectItem = idx + offset;

                //(lb.Items[selectItem], lb.Items[idx]) = (lb.Items[idx], lb.Items[selectItem]);

                (Drawings[selectItem], Drawings[idx]) = (Drawings[idx], Drawings[selectItem]);

                lv.Focus();
                lv.SelectedIndex = selectItem;
            }
        }

        private void BtnUp_Click(object sender, EventArgs e) =>
            MoveSelectedItemListBox(lvFiles, lvFiles.SelectedIndex, true);


        private void BtnDown_Click(object sender, EventArgs e) =>
            MoveSelectedItemListBox(lvFiles, lvFiles.SelectedIndex, false);


        private void BtnRemove_Click(object sender, EventArgs e)
        {
            int idx = lvFiles.SelectedIndex;

            //lbFiles.Items.RemoveAt(idx);
            Drawings.RemoveAt(idx);

            if (!Drawings.Any())
            {
                btnRename.IsEnabled = false;
                btnConvert.IsEnabled = false;
            }
            else
            {
                lvFiles.Focus();
                lvFiles.SelectedIndex = idx == Drawings.Count ? --idx : idx;
            }
        }

        private void LbFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ListView lv = (System.Windows.Controls.ListView)sender;
            if (e.AddedItems.Count > 0)
            {
                btnRemove.IsEnabled = true;

                btnDown.IsEnabled = lv.SelectedIndex < Drawings.Count - 1;

                btnUp.IsEnabled = lv.SelectedIndex > 0;
            }
            else
            {
                btnRemove.IsEnabled = false;
                btnUp.IsEnabled = false;
                btnDown.IsEnabled = false;
            }
        }

        private void BtnRename_Click(object sender, RoutedEventArgs e)
        {
            if (!Drawings.Any()) return;

            foreach (var file in Drawings.Where(f => !f.IsGoodName))
            {
                var lastIndex = file.Path.LastIndexOf('\\');

                if (lastIndex == -1)
                {
                    MessageBox.Show($"Wrong path for file {file.Path}");
                    continue;
                }

                file.RenameInZipFile();

                File.Move(file.Path, file.Path[..(lastIndex + 1)] + file.ToString() + ".cdw");
            }
        }
    }
}
