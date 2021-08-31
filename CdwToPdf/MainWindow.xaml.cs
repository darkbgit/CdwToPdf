using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using CdwToPdf.Analyzer;
using CdwToPdf.Enums;
using KompasAPI7;
using Pdf2d_LIBRARY;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using FileInfo = CdwToPdf.Core.FileInfo;
using ListBox = System.Windows.Controls.ListBox;


namespace CdwToPdf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
       

        private List<FileInfo> _filesToConvert = new();
        

        private void BtnConvert_Click(object sender, RoutedEventArgs e)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            const string KOMPAS_PROG_ID = "KOMPAS.Application.7";
            const string KOMPAS_PATH_PDF_CONVERTER = @"C:\Program Files\ASCON\KOMPAS-3D v20\Bin\Pdf2d.dll";

            Type t7 = Type.GetTypeFromProgID(KOMPAS_PROG_ID) ?? throw new InvalidOperationException();
            IApplication kmpsApp =
                Activator.CreateInstance(t7) as IApplication ?? throw new InvalidOperationException();



            IConverter iConverter = kmpsApp.Converter[KOMPAS_PATH_PDF_CONVERTER];

            IPdf2dParam param = (IPdf2dParam)iConverter.ConverterParameters(0);
            //param.OnlyThinLine = true;

            if (!_filesToConvert.Any()) return;

            var path = _filesToConvert.FirstOrDefault().Path
            [..^_filesToConvert.FirstOrDefault().Path.Split('\\').LastOrDefault().Length];

            using var targetDoc = new PdfDocument();

            foreach (var file in _filesToConvert)
            {
                var pdfFile = file.Path[..^file.Path.Split('\\').LastOrDefault().Length] + file + ".pdf";
                var result = iConverter
                    .Convert(file.Path, pdfFile, 0, false);

                System.Windows.MessageBox.Show(result.ToString());

                using var pdfDoc = PdfReader.Open(pdfFile, PdfDocumentOpenMode.Import);
                for (var i = 0; i < pdfDoc.PageCount; i++)
                    targetDoc.AddPage(pdfDoc.Pages[i]);
                pdfDoc.Close();
            }

            targetDoc.Save(path + "combine.pdf");
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
            System.Windows.MessageBox.Show(path);

            string[] files = cbSubdirs.IsChecked.GetValueOrDefault()
                ? System.IO.Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                : System.IO.Directory.GetFiles(path);

            _filesToConvert = files
                .Where(f => f.EndsWith("cdw") || f.EndsWith("spw"))
                .Select(f => new FileInfo
                {
                    Path = f
                })
                .ToList();

            foreach (var file in _filesToConvert)
            {
   
                FileStream fs = new FileStream(file.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
                CdwAnalyzer cdwAnalyzer = new(fs, file.Path);

                if (!cdwAnalyzer.IsCompleted) continue;

                file.Designation = cdwAnalyzer.FileInfo.Designation;
                file.Name = cdwAnalyzer.FileInfo.Name;
                file.DrawingType = cdwAnalyzer.FileInfo.DrawingType;
            }


            lbFiles.Items.Clear();

            //lbFiles.UpdateLayout();

            _filesToConvert = _filesToConvert.OrderBy(f => f.Designation).ToList();

            foreach (var item in _filesToConvert)
            {
                lbFiles.Items.Add(item.ToString());
            }
                
            System.Windows.MessageBox.Show(string.Join(Environment.NewLine, _filesToConvert));
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

            FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            CdwAnalyzer cdwAnalyzer = new CdwAnalyzer(file, path);
           


            _filesToConvert.Add(new FileInfo
            {
                Path = path,
                Name = cdwAnalyzer.FileInfo.Name,
                Designation = cdwAnalyzer.FileInfo.Designation,
                DrawingType = cdwAnalyzer.FileInfo.DrawingType
            });


            lbFiles.Items.Clear();

            lbFiles.Items.Add(_filesToConvert.FirstOrDefault());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //lbFiles.ItemsSource = _filesToConvert;
        }

        private static void MoveSelectedItemListBox(System.Windows.Controls.ListBox lb, int idx, bool moveUp)
        {
            if (lb.Items.Count > 1)
            {
                int offset = 0;

                if (idx >= 0)
                {
                    offset = moveUp ? -1 : 1;
                }

                if (offset != 0)
                {
                    int selectItem = idx + offset;

                    (lb.Items[selectItem], lb.Items[idx]) = (lb.Items[idx], lb.Items[selectItem]);
                    
                    lb.Focus();
                    lb.SelectedIndex = selectItem;
                }
            }
        }

        private void BtnUp_Click(object sender, EventArgs e) => 
            MoveSelectedItemListBox(lbFiles, lbFiles.SelectedIndex, true);
        

        private void BtnDown_Click(object sender, EventArgs e) =>
            MoveSelectedItemListBox(lbFiles, lbFiles.SelectedIndex, false);
        

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            int idx = lbFiles.SelectedIndex;

            lbFiles.Items.RemoveAt(idx);

            lbFiles.Focus();
            lbFiles.SelectedIndex = idx;
        }

        private void lbFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lb = sender as ListBox;
            if (e.AddedItems.Count > 0)
            {
                btnRemove.IsEnabled = true;

                btnDown.IsEnabled = lb.SelectedIndex < lbFiles.Items.Count - 1;

                btnUp.IsEnabled = lb.SelectedIndex > 0;
            }
            else
            {
                btnRemove.IsEnabled = false;
                btnUp.IsEnabled = false;
                btnDown.IsEnabled = false;
            }
        }
    }
}
