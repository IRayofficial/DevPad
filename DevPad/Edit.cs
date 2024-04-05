using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using MahApps.Metro.Controls;
using System.Windows.Media.Animation;
namespace DevPad
{
    internal class Edit
    {
        Point StartPoint;
        MainWindow programm;

        public Edit(MainWindow programm)
        {
            this.programm = programm;
        }

        //Element auswählen
        public UIElement ClickOnObject(MouseButtonEventArgs e)
        {
            UIElement thisElement = e.Source as UIElement;

            if (thisElement is Canvas || thisElement is Grid grid && grid.Name == "work")
            {
                return null;
            }

            if (thisElement is Image)
            {
                //Canvasauswahl
                return thisElement as Image;

            }

            //Standart Objekt
            return thisElement;
        }

        public WriteableBitmap GetBitmap(UIElement element)
        {
            if (element is Image)
            {
                //Bitmap auslesen
                object layer = (element as Image).Tag;
                if (layer != null)
                {
                    return layer as WriteableBitmap;
                }
                return null;
            }
            return null;
        }

        public void SelectAndMove(UIElement element)
        {
            if (element != null)
            {
                //überprüfung ob es sich um ein StackpanelElement Handelt
                StartPoint = Mouse.GetPosition(programm.canvas);
            }
        }

        //Element bewegen
        public void MoveSelectedElement(MouseEventArgs e, UIElement element)
        {
            //objekt
            if (element != null)
            {
                // Aktuelle Mausposition relativ zur Canvas erhalten
                Point currentMousePosition = e.GetPosition(programm.canvas);

                // Differenz zwischen der aktuellen Mausposition und der vorherigen Mausposition
                double deltaX = currentMousePosition.X - StartPoint.X;
                double deltaY = currentMousePosition.Y - StartPoint.Y;

                // Das ausgewählte Element ist ein anderes Element
                // Aktuelle Position des ausgewählten Elements
                double currentLeft = Canvas.GetLeft(element);
                double currentTop = Canvas.GetTop(element);

                // Neue Position des Elements unter Berücksichtigung der Mausbewegung
                double newLeft = currentLeft + deltaX;
                double newTop = currentTop + deltaY;

                // Element auf die neue Position setzen
                Canvas.SetLeft(element, newLeft);
                Canvas.SetTop(element, newTop);
                // startPoint auf die aktuelle Mausposition setzen, um die Differenz zu aktualisieren
                StartPoint = currentMousePosition;
            }
        }
    }
}
