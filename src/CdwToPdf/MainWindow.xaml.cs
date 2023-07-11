﻿using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CdwHelper.Core.Interfaces;
using CdwHelper.Core.Models;
using CdwHelper.WPF.Models;
using CdwHelper.WPF.ViewModels;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace CdwHelper.WPF;

public partial class MainWindow : Window
{
    private readonly BackgroundWorker _convertWorker = new()
    {
        WorkerReportsProgress = true
    };
    private readonly BackgroundWorker _openWorker = new()
    {
        WorkerReportsProgress = true
    };

    private readonly IPdfConverter _pdfConverter;
    private readonly IFileAnalyzer _fileAnalyzer;

    public MainWindow(IPdfConverter pdfConverter, IFileAnalyzer fileAnalyzer)
    {
        _pdfConverter = pdfConverter;
        _fileAnalyzer = fileAnalyzer;

        InitializeComponent();

        _convertWorker.DoWork += ConvertWorkerDoWork;
        _convertWorker.RunWorkerCompleted += ConvertWorker_RunWorkerCompleted;
        _convertWorker.ProgressChanged += Worker_ProgressChanged;

        _openWorker.DoWork += OpenWorkerDoWork;
        _openWorker.RunWorkerCompleted += OpenWorker_RunWorkerCompleted;
        _openWorker.ProgressChanged += Worker_ProgressChanged;

        DataContext = new DrawingsViewModel();
    }

    private void Worker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
    {
        pbConvert.Value = e.ProgressPercentage;
    }

    private void OpenWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        ((DrawingsViewModel)DataContext).Drawings.Sort((a, b) => a.Marking.CompareTo(b.Marking));
        ((DrawingsViewModel)DataContext).CheckMarkings();

        MessageBox.Show("Done");

        ChooseDirButton.IsEnabled = true;
        ChooseFileButton.IsEnabled = true;
        DrawingsToolBar.IsEnabled = true;
        ConvertButton.IsEnabled = true;
        pbConvert.Visibility = Visibility.Hidden;
    }

    private void ConvertWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        MessageBox.Show("Done");

        ChooseDirButton.IsEnabled = true;
        ChooseFileButton.IsEnabled = true;
        DrawingsToolBar.IsEnabled = true;
        FilesListView.IsEnabled = true;
        pbConvert.Visibility = Visibility.Hidden;
    }

    private void ConvertWorkerDoWork(object? sender, DoWorkEventArgs e)
    {
        if (sender is not BackgroundWorker worker) return;

        var drawings = new List<KompasDocument>();

        Application.Current.Dispatcher.Invoke(() =>
        {
            drawings = ((DrawingsViewModel)DataContext).Drawings.ToList();
        });

        if (!drawings.Any()) return;

        try
        {
            var errors = _pdfConverter.ConvertFiles(drawings, worker).ToList();

            if (errors.Any())
            {
                MessageBox.Show(string.Join(Environment.NewLine, errors));
            }
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
        }
    }

    private void OpenWorkerDoWork(object? sender, DoWorkEventArgs e)
    {
        if (sender is BackgroundWorker worker)
        {
            AnalyzeFiles(worker, e.Argument as IEnumerable<string> ?? Array.Empty<string>());
        }
    }

    //private void ConvertFiles(BackgroundWorker worker)
    //{
    //    if (!((DrawingsViewModel)DataContext).Drawings.Any()) return;

    //    int value = 0;

    //    var converter = DrawingConverterFactory.GetConverter(KompasPathPdfConverter, KompasApi);

    //    if (converter == null)
    //    {
    //        MessageBox.Show("Couldn't create Kompas converter.");
    //        return;
    //    }

    //    worker.ReportProgress(++value);

    //    using var targetDoc = new PdfDocument();

    //    var files = ((DrawingsViewModel)DataContext).Drawings
    //        .ToList();

    //    foreach (var file in files)
    //    {
    //        var lastIndex = file.FullFileName.LastIndexOf('\\');

    //        if (lastIndex == -1)
    //        {
    //            MessageBox.Show($"Wrong path for file {file.FullFileName}");
    //            continue;
    //        }
    //        var pdfFilePath = file.FullFileName[..(lastIndex + 1)] + file + ".pdf";

    //        if (converter.Convert(file.FullFileName, pdfFilePath, 0, false) == 0)
    //        {
    //            MessageBox.Show($"Couldn't convert to pdf file {file.FullFileName}");
    //        }

    //        using var pdfDoc = PdfReader.Open(pdfFilePath, PdfDocumentOpenMode.Import);
    //        try
    //        {
    //            for (var i = 0; i < pdfDoc.PageCount; i++)
    //            {
    //                targetDoc.AddPage(pdfDoc.Pages[i]);
    //            }
    //        }
    //        catch (Exception)
    //        {
    //            MessageBox.Show($"Couldn't add to pdf file {pdfFilePath}");
    //        }

    //        pdfDoc.Close();

    //        File.Delete(pdfFilePath);

    //        worker.ReportProgress(++value);
    //    }

    //    if (targetDoc.PageCount == 0)
    //    {
    //        worker.ReportProgress(++value);
    //        MessageBox.Show("Pdf document have not any page. File don't saved.");
    //        return;
    //    }

    //    var first = files.First();

    //    var newFilePath = first.FullFileName
    //        [..(first.FullFileName.LastIndexOf('\\') + 1)] + first.Marking + " - " + first.Name;
    //    targetDoc.Save(newFilePath + ".pdf");

    //    MessageBox.Show("Completed");
    //}

    private void AnalyzeFiles(BackgroundWorker worker, IEnumerable<string> filesPaths)
    {
        var value = 0;

        worker.ReportProgress(++value);

        var errorList = new List<string>();

        foreach (var file in filesPaths)
        {
            try
            {
                var document = _fileAnalyzer.Analyze(file);
                Application.Current.Dispatcher.Invoke(delegate { ((DrawingsViewModel)DataContext).Drawings.Add(document); });
            }
            catch (Exception exception)
            {
                errorList.Add(file + " " + exception.Message);
            }

            worker.ReportProgress(++value);
        }

        if (errorList.Any())
        {
            MessageBox.Show(string.Join(Environment.NewLine, errorList));
        }
    }

    private void BtnConvert_Click(object sender, RoutedEventArgs e)
    {
        ChooseDirButton.IsEnabled = false;
        ChooseFileButton.IsEnabled = false;
        DrawingsToolBar.IsEnabled = false;
        FilesListView.IsEnabled = false;

        pbConvert.IsEnabled = true;
        pbConvert.Visibility = Visibility.Visible;
        pbConvert.Maximum = ((DrawingsViewModel)DataContext).Drawings.Count;
        pbConvert.Value = 0;

        _convertWorker.RunWorkerAsync();
    }

    private void BtnChooseDir_Click(object sender, RoutedEventArgs e)
    {
        DrawingsToolBar.IsEnabled = false;
        ChooseDirButton.IsEnabled = false;
        ChooseFileButton.IsEnabled = false;

        using var dialog = new FolderBrowserDialog
        {
            Description = "Выберите папку",
            UseDescriptionForTitle = true
            //ShowNewFolderButton = true,
        };

        if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        {
            DrawingsToolBar.IsEnabled = true;
            ChooseDirButton.IsEnabled = true;
            ChooseFileButton.IsEnabled = true;
            return;
        }

        ((DrawingsViewModel)DataContext).Drawings.Clear();

        var path = dialog.SelectedPath;

        var fileExtensions = cbWith3D.IsChecked.HasValue && cbWith3D.IsChecked.Value
            ? new[] { "cdw", "spw", "m3d", "a3d" } : new[] { "cdw", "spw" };

        var files = (cbSubdirs.IsChecked.GetValueOrDefault()
                ? Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                : Directory.GetFiles(path))
            .Where(f => fileExtensions.Contains(f[^3..]))
            .ToList();

        pbConvert.IsEnabled = true;
        pbConvert.Visibility = Visibility.Visible;
        pbConvert.Maximum = files.Count + 1;
        pbConvert.Value = 0;

        _openWorker.RunWorkerAsync(files);
    }

    private void BtnChooseFile_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            //Description = "Выберите папку",
            //UseDescriptionForTitle = true,
            //ShowNewFolderButton = true,
        };

        if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

        ((DrawingsViewModel)DataContext).Drawings.Clear();

        var path = dialog.FileName;

        try
        {
            ((DrawingsViewModel)DataContext).Drawings.Add(_fileAnalyzer.Analyze(path));
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
        }

        ConvertButton.IsEnabled = true;
        RenameButton.IsEnabled = true;
    }

    private void MoveSelectedItemListBox(System.Windows.Controls.ListView lv, int idx, bool moveUp)
    {
        if (((DrawingsViewModel)DataContext).Drawings.Count <= 1) return;

        var offset = 0;

        if (idx >= 0)
        {
            offset = moveUp ? -1 : 1;
        }

        if (offset == 0) return;

        var selectItem = idx + offset;

        (((DrawingsViewModel)DataContext).Drawings[selectItem], ((DrawingsViewModel)DataContext).Drawings[idx]) = (((DrawingsViewModel)DataContext).Drawings[idx], ((DrawingsViewModel)DataContext).Drawings[selectItem]);

        lv.Focus();
        lv.SelectedIndex = selectItem;
    }

    private void BtnUp_Click(object sender, EventArgs e) =>
        MoveSelectedItemListBox(FilesListView, FilesListView.SelectedIndex, true);


    private void BtnDown_Click(object sender, EventArgs e) =>
        MoveSelectedItemListBox(FilesListView, FilesListView.SelectedIndex, false);


    private void BtnRemove_Click(object sender, EventArgs e)
    {
        int idx = FilesListView.SelectedIndex;

        ((DrawingsViewModel)DataContext).Drawings.RemoveAt(idx);

        if (!((DrawingsViewModel)DataContext).Drawings.Any())
        {
            RenameButton.IsEnabled = false;
            ConvertButton.IsEnabled = false;
        }
        else
        {
            FilesListView.Focus();
            FilesListView.SelectedIndex = idx == ((DrawingsViewModel)DataContext).Drawings.Count ? --idx : idx;
        }
    }

    private void LbFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        System.Windows.Controls.ListView lv = (System.Windows.Controls.ListView)sender;
        if (e.AddedItems.Count > 0)
        {
            RemoveButton.IsEnabled = true;

            DownButton.IsEnabled = lv.SelectedIndex < ((DrawingsViewModel)DataContext).Drawings.Count - 1;

            UpButton.IsEnabled = lv.SelectedIndex > 0;
        }
        else
        {
            RemoveButton.IsEnabled = false;
            UpButton.IsEnabled = false;
            DownButton.IsEnabled = false;
        }
    }

    private void BtnRename_Click(object sender, RoutedEventArgs e)
    {
        if (!((DrawingsViewModel)DataContext).Drawings.Any()) return;

        //foreach (var file in Drawings.Where(f => !f.IsGoodName))
        //{
        //    var lastIndex = file.Path.LastIndexOf('\\');

        //    if (lastIndex == -1)
        //    {
        //        MessageBox.Show($"Wrong path for file {file.Path}");
        //        continue;
        //    }

        //    file.RenameInZipFile();

        //    File.Move(file.Path, file.Path[..(lastIndex + 1)] + file.ToString() + ".cdw");
        //}
    }

    private void CalculateFormatsButton_OnClick(object sender, RoutedEventArgs e)
    {
        GetFormats(((DrawingsViewModel)DataContext).Drawings);
    }

    private static void GetFormats(IEnumerable<KompasDocument> documents)
    {
        var kompasDocuments = documents.ToList();

        if (!kompasDocuments.Any())
        {
            MessageBox.Show("No drawings.");
            return;
        }

        var result = kompasDocuments
            .SelectMany(d => d.Formats)
            .GroupBy(f => f.DrawingFormat)
            .OrderByDescending(f => f.Key)
            .Select(g => new { Format = g.Key, Count = g.Sum(f => f.Count * f.SheetsCount) })
            .ToList();

        MessageBox.Show(string.Join(Environment.NewLine, result.Select(i => $"{i.Format} - {i.Count}")));
    }
}
