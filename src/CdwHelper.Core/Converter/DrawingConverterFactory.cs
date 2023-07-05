using KompasAPI7;
using Pdf2d_LIBRARY;
using System.Text;

namespace CdwHelper.Core.Converter;

public class DrawingConverterFactory
{
    public static IConverter? GetConverter(string pathPdfConverter, string apiApplication)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var t7 = Type.GetTypeFromProgID(apiApplication);
        if (t7 == null) return null;

        if (Activator.CreateInstance(t7) is not IApplication kompasApp) return null;

        IConverter iConverter = kompasApp.Converter[pathPdfConverter];

        if (iConverter != null)
        {
            IPdf2dParam param = (IPdf2dParam)iConverter.ConverterParameters(0);
            //param.OnlyThinLine = true;
        }

        return iConverter;
    }
}
