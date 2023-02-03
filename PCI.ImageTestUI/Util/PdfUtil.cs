using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using System;
using System.Collections.Generic;
using iText.IO.Image;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iText.Layout.Element;

namespace PCI.ImageTestUI.Util
{
    public class PdfUtil
    {
        public Image[] ConvertImagesToPdfImage(System.Drawing.Image[] images)
        {
            List<Image> listImage = new List<Image>();
            foreach (var image in images)
            {
                ImageData data = ImageDataFactory.Create(image, null);
                listImage.Add(new Image(data));
            }
            return listImage.ToArray();
        }
        protected void MergeImageToPdf(string sourceFile, Image[] images)
        {
            Image image = images[0];
            PdfDocument pdfDoc = new PdfDocument(new PdfWriter(sourceFile));
            Document doc = new Document(pdfDoc, new PageSize(image.GetImageWidth(), image.GetImageHeight()));

            for (int i = 0; i < images.Length; i++)
            {
                image = images[i];
                pdfDoc.AddNewPage(new PageSize(image.GetImageWidth(), image.GetImageHeight()));
                image.SetFixedPosition(i + 1, 0, 0);
                doc.Add(image);
            }

            doc.Close();
        }
    }
}
