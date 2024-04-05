using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace DevPad
{
    //Fertig muss nicht mehr bearbeitet werden
    internal class Export
    {
        public static void ExportCanvasAsImage(Canvas canvas, String name)
        {
            // RenderTargetBitmap erstellen
            var renderBitmap = new RenderTargetBitmap(
                (int)canvas.Width,
                (int)canvas.Height,
                96d,
                96d,
                PixelFormats.Pbgra32);

            // Render auf Bitmap ausführen
            renderBitmap.Render(canvas);

            // BitmapEncoder erstellen
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            // Dialog zum Speichern der Datei öffnen
            var dialog = new SaveFileDialog();
            dialog.Filter = "PNG-Bild (*.png)|*.png";
            dialog.FileName = name;
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            if (dialog.ShowDialog() == true)
            {
                // Datei speichern
                using (var fileStream = new FileStream(dialog.FileName, FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
        }
    }
}
