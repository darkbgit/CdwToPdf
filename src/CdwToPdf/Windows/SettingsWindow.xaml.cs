using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using CdwHelper.Core.Models;
using Microsoft.Extensions.Options;

namespace CdwHelper.WPF.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly PdfOptions _pdfOptions;
        private readonly IOptionsMonitor<PdfOptions> _pdfOptionsMonitor;

        public SettingsWindow(IOptionsMonitor<PdfOptions> pdfOptionsMonitor)
        {
            _pdfOptionsMonitor = pdfOptionsMonitor;

            _pdfOptions = new PdfOptions
            {
                KompasAPI = _pdfOptionsMonitor.CurrentValue.KompasAPI,
                PathToPdf2d = _pdfOptionsMonitor.CurrentValue.PathToPdf2d
            };

            InitializeComponent();

            DataContext = _pdfOptions;
            SaveButton.IsEnabled = false;
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateOptions(_pdfOptions);
            SaveButton.IsEnabled = false;
            Hide();
        }

        private static void UpdateOptions(PdfOptions pdfOptions)
        {
            var json = JsonSerializer.Serialize(new { PdfOptions = pdfOptions }, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("options.json", json);
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            _pdfOptions.KompasAPI = _pdfOptionsMonitor.CurrentValue.KompasAPI;
            _pdfOptions.PathToPdf2d = _pdfOptionsMonitor.CurrentValue.PathToPdf2d;

            SaveButton.IsEnabled = false;
            Hide();
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SaveButton.IsEnabled = true;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (sender is not SettingsWindow) return;

            if (Owner is not MainWindow { SettingsWindow: not null } main) return;

            main.SettingsWindow = null;
        }
    }
}
