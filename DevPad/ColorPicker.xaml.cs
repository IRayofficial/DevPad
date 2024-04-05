using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace DevPad
{
    /// <summary>
    /// Interaktionslogik für ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : Window
    {
        private int Width = 255;
        private int Height = 255;

        private bool SearchColor = false;

        WriteableBitmap writableBitmap;

        MainWindow M;

        //Konstruktor mit Buttonwerten
        public ColorPicker(Button b1, Button b2, Button b3, Button b4, Button b5, Button b6, Button b7, Button b8, Button b9, Button b10, MainWindow main )
        {
            InitializeComponent();

            M = main;

            //Standartwerte
            A.Value = 255;
            R.Value = 255;
            G.Value = 255;
            B.Value = 255;
            
            DrawColorPalette();

            //Button Farben vom Mainwindow übergeben

            B1.Background = b1.Background; 
            B2.Background = b2.Background;
            B3.Background = b3.Background;
            B4.Background = b4.Background;
            B5.Background = b5.Background;
            B6.Background = b6.Background;
            B7.Background = b7.Background;
            B8.Background = b8.Background;
            B9.Background = b9.Background;
            B10.Background = b10.Background;
        }

        private void DrawColorPalette()
        {
            // Erstelle ein neues WritableBitmap
            writableBitmap = new WriteableBitmap((int)Width, (int)Height, 96, 96, PixelFormats.Bgra32, null);

            // Schleife durch alle möglichen X-Werte (Breite des Bildes)
            for (int x = 0; x < Width; x++)
            {
                byte[] pixels = new byte[4 * Height]; // Byte-Array für die Pixelinformationen einer Spalte

                // Schleife durch alle Y-Werte (Höhe des Bildes)
                for (int y = 0; y < Height; y++)
                {
                    int index = y * 4;

                    // Setze die Farbwerte basierend auf den Schiebereglerwerten und der Position
                    pixels[index] = (byte)(B.Value * x / Width); // Blau
                    pixels[index + 1] = (byte)(G.Value * x / Width);// Grün
                    pixels[index + 2] = (byte)(R.Value * x / Width); // Rot
                    pixels[index + 3] = (byte)A.Value; // Alpha-Wert
                }
                // Schreibe die Farbspalte in die Bitmap
                writableBitmap.WritePixels(new Int32Rect(x, 0, 1, Height), pixels, 4, 0);
            }

            // Zeige die Farbpalette im Image-Control an
            ColorImage.Source = writableBitmap;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RBox.Text = R.Value.ToString();
            GBox.Text = G.Value.ToString();
            BBox.Text = B.Value.ToString();
            ABox.Text = A.Value.ToString();
            DrawColorPalette();
        }

        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            btn.Background = SelectedColor.Background;

            //übergabe ans MainWindow
            M.BT1.Background = B1.Background;
            M.BT2.Background = B2.Background;
            M.BT3.Background = B3.Background;
            M.BT4.Background = B4.Background;
            M.BT5.Background = B5.Background;
            M.BT6.Background = B6.Background;
            M.BT7.Background = B7.Background;
            M.BT8.Background = B8.Background;
            M.BT9.Background = B9.Background;
            M.BT10.Background = B10.Background;
        }

        //Farbsuche
        private void ColorCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SearchColor = true;
            ColorCanvas_MouseMove(sender,e);
        }

        private void ColorCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (SearchColor)
            {
                Point startPoint = Mouse.GetPosition(ColorImage);

                // Sicherstellen, dass die Mausposition innerhalb der Grenzen des Bildes liegt
                if (startPoint.X < 0 || startPoint.X >= ColorImage.Width ||
                    startPoint.Y < 0 || startPoint.Y >= ColorImage.Height)
                {
                    SelectedColor.Background = Brushes.Transparent;
                }
                else
                {
                    writableBitmap.Lock();

                    IntPtr buffer = writableBitmap.BackBuffer;
                    int pixelSize = (writableBitmap.Format.BitsPerPixel + 7) / 8;
                    int offset = (int)startPoint.Y * writableBitmap.BackBufferStride + (int)startPoint.X * pixelSize;
                    byte[] pixelValues = new byte[pixelSize];
                    Marshal.Copy(buffer + offset, pixelValues, 0, pixelSize);

                    // Rückgabe der Farbe des Pixels
                    SelectedColor.Background = new SolidColorBrush(Color.FromArgb(pixelValues[3], pixelValues[2], pixelValues[1], pixelValues[0]));
                    // Entsperren des Bildes nach der Verarbeitung
                    writableBitmap.Unlock();
                }
            }
        }

        private void ColorCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SearchColor = false;
        }

        private void RBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                int val = int.Parse(e.Text);

                if (val < 0)
                {
                    val = 0;
                }
                if (val > 255)
                {
                    val = 255;
                }
                R.Value = val;
            }
            catch 
            {
            
            }
        }

        private void GBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void BBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void ABox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }
    }
}
