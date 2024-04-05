using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Microsoft.Win32;

namespace DevPad
{
    internal class DrawTool
    {
        //LinienDicke und farbe
        public int lineThickness = 2;
        public SolidColorBrush SelectedColor = Brushes.Black;
        public SolidColorBrush EraserColor = Brushes.Transparent;

        public bool Draw = false;
        public bool Erase = false;
        public bool Fill = false;
        public bool Pick = false;

        //Zeichenpunkt
        Point StartPoint;
        Point Fillpoint;

        //Zeichnen-------------------------

        //Zeichnen beginnen
        public void StartDrawLine(MouseButtonEventArgs e, UIElement element, WriteableBitmap bitmap)
        {
            StartPoint = e.GetPosition(element);
            MoveLine(e, element, bitmap);

            
        }

        //Linie Ziehen
        public void MoveLine(MouseEventArgs e, UIElement element, WriteableBitmap bitmap)
        {
            if (Draw)
            {
                Point currentPosition = e.GetPosition(element);
                Color color = SelectedColor.Color;
                DrawLine(StartPoint, currentPosition, color, bitmap);
                StartPoint = currentPosition;
            }

            if (Erase) 
            {
                Point currentPosition = e.GetPosition(element);
                Color color = EraserColor.Color;
                DrawLine(StartPoint, currentPosition, color, bitmap);
                StartPoint = currentPosition;
            }

            if (Fill)
            {
                Fillpoint = e.GetPosition(element);
                Color targetColor = GetPixelColor((int)StartPoint.X, (int)StartPoint.Y, bitmap);
                FillColor(Fillpoint, targetColor, SelectedColor.Color, bitmap);
            }
        }

        //Funktion um Pixelfarbe zu bekommen
        public Color GetPixelColor(int x, int y, WriteableBitmap bitmap)
        {
            if (bitmap != null)
            {
                // Überprüfen, ob die Koordinaten im gültigen Bereich des Bitmaps liegen
                if (x >= 0 && x < bitmap.PixelWidth && y >= 0 && y < bitmap.PixelHeight)
                {
                    // Erstellen Sie ein Byte-Array, das den gesamten Inhalt des WriteableBitmaps enthält
                    byte[] pixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];
                    bitmap.CopyPixels(pixels, bitmap.PixelWidth * 4, 0);

                    // Berechnen Sie den Index des aktuellen Pixels im Pixel-Array
                    int index = (y * bitmap.PixelWidth + x) * 4; // Multipliziert mit 4 für ARGB-Werte

                    // Extrahieren Sie die Farbwerte aus dem Pixel-Array
                    byte blue = pixels[index];
                    byte green = pixels[index + 1];
                    byte red = pixels[index + 2];
                    byte alpha = pixels[index + 3];

                    // Rückgabe der Farbe des Pixels
                    return Color.FromArgb(alpha, red, green, blue);
                }
                
            }
            return Colors.Transparent;
        }


        // Funktion um Linien zu zeichnen
        private void DrawLine(Point startPoint, Point endPoint, Color color, WriteableBitmap bitmap)
        {
            // Überprüfe, ob das ausgewählte Layer-Bitmap existiert
            if (bitmap != null)
            {
                // Bestimme die Anzahl der Bytes pro Pixel
                int bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;
                // Berechne die Anzahl der Bytes pro Zeile (Stride)
                int stride = bytesPerPixel * bitmap.PixelWidth;
                // Berechne die Größe des Pixel-Puffers
                int bufferSize = stride * bitmap.PixelHeight;
                // Erstelle einen Byte-Array-Puffer, um die Pixel des Bitmaps zu speichern
                byte[] pixelBuffer = new byte[bufferSize];
                // Kopiere die Pixel des Bitmaps in den Puffer
                bitmap.CopyPixels(pixelBuffer, stride, 0);

                // Extrahiere die Koordinaten der Start- und Endpunkte der Linie
                int x0 = (int)startPoint.X;
                int y0 = (int)startPoint.Y;
                int x1 = (int)endPoint.X;
                int y1 = (int)endPoint.Y;

                // Berechne die Differenz in x- und y-Richtung
                int dx = Math.Abs(x1 - x0);
                int dy = Math.Abs(y1 - y0);
                // Bestimme die Schrittgrößen in x- und y-Richtung
                int sx = x0 < x1 ? 1 : -1;
                int sy = y0 < y1 ? 1 : -1;
                int err = dx - dy;

                // Setze die Dicke der Linie
                int thickness = lineThickness;

                // Konvertiere die RGB-Farbe in ein Byte-Array
                byte[] colorBytes = { color.B, color.G, color.R, color.A };

                // Schleife durch alle Pixel der Linie und setze ihre Farben entsprechend
                while (true)
                {
                    // Überprüfe, ob der aktuelle Punkt innerhalb der Grenzen des Bitmaps liegt
                    if (x0 >= 0 && x0 < bitmap.PixelWidth && y0 >= 0 && y0 < bitmap.PixelHeight)
                    {
                        // Durchlaufe die Dicke der Linie und setze die Farben
                        for (int i = -thickness / 2; i < thickness / 2; i++)
                        {
                            for (int j = -thickness / 2; j < thickness / 2; j++)
                            {
                                // Berechne den Index des aktuellen Pixels
                                int index = ((y0 + i) * stride) + ((x0 + j) * bytesPerPixel);
                                // Überprüfe, ob der Index innerhalb der Grenzen des Puffers liegt
                                if (index >= 0 && index < pixelBuffer.Length)
                                {
                                    // Setze die Farbe des aktuellen Pixels
                                    for (int k = 0; k < colorBytes.Length; k++)
                                    {
                                        pixelBuffer[index + k] = colorBytes[k];
                                    }
                                }
                            }
                        }
                    }

                    // Überprüfe, ob der Endpunkt erreicht wurde
                    if (x0 == x1 && y0 == y1) break;
                    // Berechne den Fehlerterm für die Bresenham-Algorithmus-Schleife
                    int e2 = 2 * err;
                    // Aktualisiere die Fehlerterm-Werte basierend auf der Schrittgröße
                    if (e2 > -dy)
                    {
                        err -= dy;
                        x0 += sx;
                    }
                    if (e2 < dx)
                    {
                        err += dx;
                        y0 += sy;
                    }
                }

                // Aktualisiere das Bitmap mit den geänderten Pixeln
                bitmap.WritePixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), pixelBuffer, stride, 0);
            }
        }

        //Fill Tool----------------> Selber schreiben (vereinfachen) 
        private void FillColor(Point startPoint, Color targetColor, Color replacementColor, WriteableBitmap bitmap)
        {
            // Überprüfen, ob das ausgewählte Layer-Bitmap existiert
            if (bitmap != null)
            {
                // Überprüfen, ob die Füllfarbe und die Ziel(farbe) gleich sind
                if (targetColor == replacementColor)
                {
                    return;
                }

                // Bestimme die Anzahl der Bytes pro Pixel
                int bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;
                // Berechne die Anzahl der Bytes pro Zeile (Stride)
                int stride = bytesPerPixel * bitmap.PixelWidth;
                // Berechne die Größe des Pixel-Puffers
                int bufferSize = stride * bitmap.PixelHeight;
                // Erstelle einen Byte-Array-Puffer, um die Pixel des Bitmaps zu speichern
                byte[] pixelBuffer = new byte[bufferSize];
                // Kopiere die Pixel des Bitmaps in den Puffer
                bitmap.CopyPixels(pixelBuffer, stride, 0);

                // Konvertiere die Ziel- und Ersatzfarbe in Byte-Arrays
                byte[] targetColorBytes = { targetColor.B, targetColor.G, targetColor.R, targetColor.A };
                byte[] replacementColorBytes = { replacementColor.B, replacementColor.G, replacementColor.R, replacementColor.A };

                // Erstelle eine Queue für die zu überprüfenden Pixel
                Queue<Point> pixels = new Queue<Point>();
                // Füge den Startpunkt zur Warteschlange hinzu
                pixels.Enqueue(startPoint);

                // Schleife, um alle benachbarten Pixel zu überprüfen
                while (pixels.Count > 0)
                {
                    // Aktuellen Pixel abrufen
                    Point currentPoint = pixels.Dequeue();
                    int x = (int)currentPoint.X;
                    int y = (int)currentPoint.Y;

                    // Überprüfe, ob der aktuelle Punkt innerhalb der Grenzen des Bitmaps liegt
                    if (x >= 0 && x < bitmap.PixelWidth && y >= 0 && y < bitmap.PixelHeight)
                    {
                        // Berechne den Index des aktuellen Pixels im Puffer
                        int index = (y * stride) + (x * bytesPerPixel);

                        // Überprüfe, ob der Pixel die Ziel-Farbe hat
                        bool isTargetColor = true;
                        for (int i = 0; i < targetColorBytes.Length; i++)
                        {
                            if (pixelBuffer[index + i] != targetColorBytes[i])
                            {
                                isTargetColor = false;
                                break;
                            }
                        }

                        // Wenn der Pixel die Ziel-Farbe hat, ersetze sie durch die Ersatzfarbe
                        if (isTargetColor)
                        {
                            for (int i = 0; i < replacementColorBytes.Length; i++)
                            {
                                pixelBuffer[index + i] = replacementColorBytes[i];
                            }

                            // Füge die benachbarten Pixel zur Warteschlange hinzu, wenn sie noch nicht überprüft wurden
                            pixels.Enqueue(new Point(x + 1, y));
                            pixels.Enqueue(new Point(x - 1, y));
                            pixels.Enqueue(new Point(x, y + 1));
                            pixels.Enqueue(new Point(x, y - 1));
                        }
                    }
                }

                // Aktualisiere das Bitmap mit den geänderten Pixeln
                bitmap.WritePixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), pixelBuffer, stride, 0);
            }
        }
    }
}
