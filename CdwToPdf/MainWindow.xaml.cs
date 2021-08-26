using System;
using System.Collections.Generic;
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
using KompasAPI7;
using Pdf2d_LIBRARY;

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

        private void BtnConvert_Click(object sender, RoutedEventArgs e)
        {
            const string KOMPAS_PROG_ID = "KOMPAS.Application.7";
            const string KOMPAS_PATH_PDF_CONVERTER = @"C:\Program Files\ASCON\KOMPAS-3D v20\Bin\Pdf2d.dll";

            Type t7 = Type.GetTypeFromProgID(KOMPAS_PROG_ID) ?? throw new InvalidOperationException();
            IApplication kmpsApp =
                Activator.CreateInstance(t7) as IApplication ?? throw new InvalidOperationException();



            IConverter iConverter = kmpsApp.Converter[KOMPAS_PATH_PDF_CONVERTER];

            IPdf2dParam param = (IPdf2dParam)iConverter.ConverterParameters(0);
            param.OnlyThinLine = true;

            var result = iConverter.Convert(@"C:\Try\1.cdw", @"C:\Try\1.pdf", 0, false);

            System.Windows.MessageBox.Show(result.ToString());


        }

        private void BtnChoose_Click(object sender, RoutedEventArgs e)
        {
            string path;
            using var dialog = new FolderBrowserDialog
            {
                Description = "Выберите папку",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path = dialog.SelectedPath;
                System.Windows.MessageBox.Show(path);
            }
        }
    }
}
