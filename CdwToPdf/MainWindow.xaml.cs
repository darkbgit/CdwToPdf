using CdwToPdf.Core;
using KompasAPI7;
using Pdf2d_LIBRARY;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using CdwToPdf.Core.Analyzer;
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

        private ObservableCollection<DrawingFileInfo> _filesToConvert = new();

        public ObservableCollection<DrawingFileInfo> Drawings => _filesToConvert;

        private const string KOMPAS_API = "KOMPAS.Application.7";
        private const string KOMPAS_PATH_PDF_CONVERTER = @"C:\Program Files\ASCON\KOMPAS-3D v20\Bin\Pdf2d.dll";

        public MainWindow()
        {
            InitializeComponent();
            _worker = new BackgroundWorker();
            _worker.DoWork += Worker_DoWork;
            //DataContext = this;
        }

        private void Worker_DoWork(object? sender, DoWorkEventArgs e)
        {
            ConvertFiles();
        }

        private static IConverter? GetConverter(string pathPdfConverter, string apiApplication)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            

            Type? t7 = Type.GetTypeFromProgID(apiApplication);
            if (t7 == null) return null;

            if (Activator.CreateInstance(t7) is not IApplication kompasApp) return null;

            IConverter iConverter = kompasApp.Converter[pathPdfConverter];
            if (iConverter == null) return null;

            IPdf2dParam param = (IPdf2dParam)iConverter.ConverterParameters(0);
            //param.OnlyThinLine = true;

            return iConverter;
        }

        private void ConvertFiles()
        {
            UpdateProgressBarDelegate updProgress = pbConvert.SetValue;

            double value = 0;

            IConverter? converter = GetConverter(KOMPAS_PATH_PDF_CONVERTER, KOMPAS_API);

            if (converter == null)
            {
                MessageBox.Show("Couldn't create Kompas converter");
                return;
            }

            _ = Dispatcher.Invoke(updProgress, RangeBase.ValueProperty, ++value);
            if (!_filesToConvert.Any()) return;

            using var targetDoc = new PdfDocument();

            foreach (var file in _filesToConvert)
            {
                var lastIndex = file.Path.LastIndexOf('\\');

                if (lastIndex == -1)
                {
                    MessageBox.Show($"Wrong path for file {file.Path}");
                    continue;
                }
                var pdfFile = file.Path[..lastIndex] + file + ".pdf";

                if (converter.Convert(file.Path, pdfFile, 0, false) == 0)
                {
                    MessageBox.Show($"Couldn't convert to pdf file {file.Path}");
                }

                using var pdfDoc = PdfReader.Open(pdfFile, PdfDocumentOpenMode.Import);
                try
                {
                    for (var i = 0; i < pdfDoc.PageCount; i++)
                    {
                        targetDoc.AddPage(pdfDoc.Pages[i]);
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show($"Couldn't add to pdf file {pdfFile}");
                }
                

                pdfDoc.Close();

                File.Delete(pdfFile);

                _ = Dispatcher.Invoke(updProgress, RangeBase.ValueProperty, ++value);
            }
            

            var path = _filesToConvert.First().Path
                [..(_filesToConvert.First().Path.LastIndexOf('.') + 1)];
            targetDoc.Save(path + "pdf");

            MessageBox.Show("Completed");
        }




        private void BtnConvert_Click(object sender, RoutedEventArgs e)
        {
            pbConvert.IsEnabled = true;
            pbConvert.Maximum = _filesToConvert.Count + 1;
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

            _filesToConvert.Clear();

            var path = dialog.SelectedPath;

            List<string> files = (cbSubdirs.IsChecked.GetValueOrDefault()
                ? System.IO.Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                : System.IO.Directory.GetFiles(path))
                .Where(f => f.EndsWith("cdw") || f.EndsWith("spw"))
                .ToList();


            //foreach (var file in files)
            //{
            //    FileStream fs = new(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            //    _filesToConvert.Add(new FileAnalyzer(fs, file).Drawing);
            //}

            //_filesToConvert = _filesToConvert.OrderBy(f => f.Designation);

            _filesToConvert = new ObservableCollection<DrawingFileInfo>(files
                .Select(f => new DrawingFileInfo(f))
                .OrderBy(f => f.Designation));

            lvFiles.ItemsSource = Drawings;

            //foreach (var file in _filesToConvert)
            //{

            //    FileStream fs = new(file.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            //    CdwAnalyzer cdwAnalyzer = new(fs, file.Path);

            //    if (!cdwAnalyzer.IsCompleted) continue;

            //    file.Designation = cdwAnalyzer.DrawingFileInfo.Designation;
            //    file.Name = cdwAnalyzer.DrawingFileInfo.Name;
            //    file.DrawingType = cdwAnalyzer.DrawingFileInfo.DrawingType;
            //    file.IsAssemblyDrawing = cdwAnalyzer.DrawingFileInfo.IsAssemblyDrawing;
            //}


            //lbFiles.Items.Clear();

            ////lbFiles.UpdateLayout();

            //_filesToConvert = _filesToConvert.OrderBy(f => f.ToString()).ToList();

            //foreach (var item in _filesToConvert)
            //{
            //    lbFiles.Items.Add(item.ToString());
            //}

            //System.Windows.MessageBox.Show(string.Join(Environment.NewLine, _filesToConvert));
            btnConvert.IsEnabled = true;
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

            _filesToConvert.Clear();

            var path = dialog.FileName;

            FileStream file = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            //CdwAnalyzer cdwAnalyzer = new(file, path);

            FileAnalyzer drawing = new FileAnalyzer(file, path);

            MessageBox.Show($"{drawing.Drawing.Designation} - {drawing.Drawing.Name}\n{drawing.Drawing.Sheets}");


            //_filesToConvert.Add(new DrawingFileInfo
            //{
            //    Path = path,
            //    Name = cdwAnalyzer.DrawingFileInfo.Name,
            //    Designation = cdwAnalyzer.DrawingFileInfo.Designation,
            //    DrawingType = cdwAnalyzer.DrawingFileInfo.DrawingType,
            //    IsAssemblyDrawing = cdwAnalyzer.DrawingFileInfo.IsAssemblyDrawing
            //});


            //lbFiles.Items.Clear();

            //lbFiles.Items.Add(_filesToConvert.FirstOrDefault());

            //btnConvert.IsEnabled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //lbFiles.ItemsSource = _filesToConvert;
        }

        private void MoveSelectedItemListBox(System.Windows.Controls.ListView lv, int idx, bool moveUp)
        {
            if (_filesToConvert.Count <= 1) return;

            int offset = 0;

            if (idx >= 0)
            {
                offset = moveUp ? -1 : 1;
            }

            if (offset != 0)
            {
                int selectItem = idx + offset;

                //(lb.Items[selectItem], lb.Items[idx]) = (lb.Items[idx], lb.Items[selectItem]);

                (_filesToConvert[selectItem], _filesToConvert[idx]) = (_filesToConvert[idx], _filesToConvert[selectItem]);

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
            _filesToConvert.RemoveAt(idx);

            if (!_filesToConvert.Any())
            {
                btnRename.IsEnabled = false;
                btnConvert.IsEnabled = false;
            }
            else
            {
                lvFiles.Focus();
                lvFiles.SelectedIndex = idx == _filesToConvert.Count ? --idx : idx;
            }
        }

        private void LbFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ListView lv = (System.Windows.Controls.ListView)sender;
            if (e.AddedItems.Count > 0)
            {
                btnRemove.IsEnabled = true;

                btnDown.IsEnabled = lv.SelectedIndex < _filesToConvert.Count - 1;

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
            if (!_filesToConvert.Any()) return;


            foreach (var file in _filesToConvert.Where(f=>!f.IsGoodName))
            {
                var lastIndex = file.Path.LastIndexOf('\\');

                if (lastIndex == -1)
                {
                    MessageBox.Show($"Wrong path for file {file.Path}");
                    continue;
                }
                var pdfFile = file.Path[..lastIndex] + file + ".pdf";

                file.RenameInZipFile();

                File.Move(file.Path, file.Path[..lastIndex] + file.ToString() + ".cdw");


            }

        }
    }
}
