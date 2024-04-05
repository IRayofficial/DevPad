using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace DevPad
{
    internal class Layer
    {
        public WriteableBitmap LayerBitmap { get; set; }

        public double DpiStandart = 96;

        public PixelFormat pFormat = PixelFormats.Bgra32;

        //Für Ebenen
        public Layer(int width ,int height, double dpi, SolidColorBrush brush)
        {
            //Farbe Umwandeln
            Color color = brush.Color;

            LayerBitmap = new WriteableBitmap(width, height, dpi, dpi, pFormat, null);

            //Pixelvariabel wird gesetzt
            byte[] pixels = new byte[width * height * 4];

            // Setze alle Pixel auf weiß
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = color.B; // Blau-Komponente
                pixels[i + 1] = color.G; // Grün-Komponente
                pixels[i + 2] = color.R; // Rot-Komponente
                pixels[i + 3] = color.A; // Alpha-Komponente (vollständig undurchsichtig)
            }

            LayerBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);
        }

        //Für Bilder
        public Layer(int width, int height, double dpi)
        {
            LayerBitmap = new WriteableBitmap(width, height, dpi, dpi, pFormat, null);
        }
    }
}
