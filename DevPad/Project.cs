using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using FastColoredTextBoxNS;
using System.Xml.Serialization;

namespace DevPad
{
    //Klasse um Elemente mit deren zuständen zu speichern
    public class ElementState
    {
        //Allgemeine Attribute
        public UIElement Element { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        //Bild Attribute
        public WriteableBitmap Bitmap { get; set; }
        public object Tag { get; set; }

        //TextEditoren Attirbute
        public string Text { get; set; }
        public Brush Background { get; set; }
        public Brush Foreground { get; set; }
        public string Typ { get; set; }
        public double Size { get; set; }

        //Parameterloser Konstruktor
        public ElementState() { }

        //Bilder
        public ElementState(UIElement element, double left, double top, int width, int height, WriteableBitmap bitmap, object tag)
        {
            Element = element;
            Left = left;
            Top = top;
            Width = width;
            Height = height;
            Bitmap = bitmap;
            Tag = tag;
        }

        //TextBoxen
        public ElementState(UIElement element, double left, double top, int width, int height, string text, Brush background, Brush foreground, string typ, double size)
        {
            Element = element;
            Left = left;
            Top = top;
            Width = width;
            Height = height;
            Text = text;
            Background = background;
            Foreground = foreground;
            Typ = typ;
            Size = size;
        }
    }
    public class Project
    {
        //LayerObject
        Layer layer;

        public UIElement selectedElement;
        public WriteableBitmap selectedBitmap;

        public List<ElementState> allElements = new List<ElementState>();

        //Undo / Redo Liste
        public List<List<ElementState>> UndoList = new List<List<ElementState>>();
        public List<List<ElementState>> RedoList = new List<List<ElementState>>();

        //Name des Projects
        public string Name = "Untitled";
        public string SavePath;

        //Feld
        public Canvas Field = new Canvas();

        //Instanzloser Konstruktor zum Laden eines Projektes
        public Project() 
        {
            Field.AllowDrop = true;
            Field.ClipToBounds = true;
            Field.Background = Brushes.Transparent;
            Field.Width = 1920;
            Field.Height = 1080;
        }

        //Konstruktor
        public Project(int width, int height, SolidColorBrush color)
        {

                //Resizen des Canvas und dessen Border
                Field.Width = width;
                Field.Height = height;

                Field.AllowDrop = true;
                Field.ClipToBounds = true;
                Field.Background = Brushes.Transparent;

            NewLayer(width,height,color);
            
        }

        public void NewLayer(int width, int height, SolidColorBrush color) {
            //Erster Layer erstellen
            layer = new Layer(width, height, 96, color);

            // Erstelle ein Image-Element, um das Bitmap im Canvas anzuzeigen
            Image image = new Image();
            image.Source = layer.LayerBitmap;
            image.Width = width;
            image.Height = height;
            image.Tag = layer.LayerBitmap;
            Canvas.SetLeft(image, 0);
            Canvas.SetTop(image, 0);
            Field.Children.Add(image);

            UpdateCurrentItems();
        }

        public void DeleteLayer()
        {
            Field.Children.Remove(selectedElement);
            selectedElement = null;
            UpdateCurrentItems();
        }

        //Undo / Redo Funktionen

        //Aktualisieren der allElements Liste
        public void UpdateCurrentItems()
        {
            //Lösche alle Elemente aus der Liste
            allElements.Clear();

            //Für jedes Element im Canvas wird das jeweilige Element in die Liste gespeichert
            foreach (UIElement element in Field.Children)
            {
                //Positionskoordinaten des Elements Speichern
                double left = Canvas.GetLeft(element);
                double top = Canvas.GetTop(element);

                // Prüfen, ob das Element eine Image-Quelle hat (WriteableBitmap) und Tag
                if (element is Image image && image.Tag != null)
                {
                    //Falls es ein Image Layer ist
                    allElements.Add(new ElementState(element, left, top, 
                        (int)image.Width, (int)image.Height, 
                        image.Tag as WriteableBitmap, image.Tag));
                }
                else if (element is TextEditor textEditor)
                {
                    //falls es ein TextEditor ist
                    allElements.Add(new ElementState(element, left, top, 
                        (int)textEditor.Width, (int)textEditor.Height, textEditor.Text, 
                        textEditor.Background, textEditor.Foreground, textEditor.Name, textEditor.FontSize));
                }
                else
                {
                    //alle anderen Elemente
                    allElements.Add(new ElementState(element, left, top, 0, 0, null, null));
                }

            }

            //Speichere in die Undoliste
            SaveInUndo();
        }

        
        //Speichere änderung in Undo Liste
        public void SaveInUndo()
        {
            // Kopie der aktuellen Elemente erstellen und in die Undo-Liste einfügen
            UndoList.Add(allElements.Select(elementState =>
            {
                if (elementState.Element is Image)
                {
                    // Erstelle eine Kopie des aktuellen Bildes (falls vorhanden)
                    WriteableBitmap bitmapCopy = elementState.Bitmap != null ? new WriteableBitmap(elementState.Bitmap) : null;

                    // Aktuellen Zustand als Tag speichern
                    object tagCopy = bitmapCopy;

                    //Werte setzen und als neues ElementState Objekt speichern
                    return new ElementState(
                        elementState.Element,
                        elementState.Left,
                        elementState.Top,
                        elementState.Width,
                        elementState.Height,
                        bitmapCopy,
                        tagCopy
                    );
                }
                else
                {
                    //Speicherung der TextEditors
                    return new ElementState(
                        elementState.Element,
                        elementState.Left,
                        elementState.Top,
                        elementState.Width,
                        elementState.Height,
                        elementState.Text,
                        elementState.Background,
                        elementState.Foreground,
                        elementState.Typ, 
                        elementState.Size
                    );
                }
            }).ToList());
        }

        //Undo Funktion
        public void Undo() 
        {
            if (UndoList.Count > 1)
            {
                // Aktuellen Zustand sichern
                RedoList.Add(new List<ElementState>(UndoList.Last()));

                // Letzten Eintrag entfernen
                UndoList.Remove(UndoList.Last());

                // Aktuellen Zustand wiederherstellen
                allElements.Clear();
                allElements.AddRange(UndoList.Last());
                UpdateProject();
            }
        }

        public void Redo()
        {
            if (RedoList.Count > 0)
            {
                // Aktuellen Zustand sichern
                UndoList.Add(new List<ElementState>(RedoList.Last()));

                // Letzten Eintrag entfernen
                RedoList.Remove(RedoList.Last());

                // Aktuellen Zustand wiederherstellen
                allElements.Clear();
                allElements.AddRange(UndoList.Last());
                UpdateProject();
            }
        }

        //Projekt nach Liste aktualisieren
        public void UpdateProject()
        {
            //Alle Children entfernen
            Field.Children.Clear();

            //Für jedes Element in der Elementliste
            foreach (ElementState elementState in allElements)
            {
                // Setze die Position des Elements
                Canvas.SetLeft(elementState.Element, elementState.Left);
                Canvas.SetTop(elementState.Element, elementState.Top);

                // Überprüfe, ob das Element ein Image ist und eine Bitmap hat
                if (elementState.Element is Image image && elementState.Tag is WriteableBitmap bitmap)
                {
                    // die werte des Elements
                    image.Source = bitmap;
                    image.Width = elementState.Width;
                    image.Height = elementState.Height;
                    image.Tag = elementState.Tag; // Aktuellen Status als Tag setzen
                }

                if (elementState.Element is TextEditor editor)
                {
                    //Werte des TextEditors
                    editor.Foreground = elementState.Foreground;
                    editor.Background = elementState.Background;
                    editor.Text = elementState.Text;
                    editor.Width = elementState.Width;
                    editor.Height = elementState.Height;
                }

                //Element in Canvas einsetzen
                Field.Children.Add(elementState.Element);
            }
        }

        //Einfügbare Elemente

        //Bild einfügen
        public void InsertImage_Click()
        {
            //Bild auswählen
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                Uri uri = new Uri(openFileDialog.FileName);
                BitmapImage bitmap = new BitmapImage(uri);

                layer = new Layer(bitmap.PixelWidth, bitmap.PixelHeight, 96);

                // Kopiere die Pixel des Bildes auf das WriteableBitmap
                bitmap.CopyPixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight),
                                       layer.LayerBitmap.BackBuffer,
                                       layer.LayerBitmap.BackBufferStride * layer.LayerBitmap.PixelHeight,
                                       layer.LayerBitmap.BackBufferStride);

                // Aktualisiere das WriteableBitmap, um die Änderungen anzuzeigen
                layer.LayerBitmap.Lock();
                layer.LayerBitmap.AddDirtyRect(new Int32Rect(0, 0, layer.LayerBitmap.PixelWidth, layer.LayerBitmap.PixelHeight));
                layer.LayerBitmap.Unlock();

                Image image = new Image
                {
                    Source = layer.LayerBitmap,
                    Width = layer.LayerBitmap.PixelWidth,
                    Height = layer.LayerBitmap.PixelHeight,
                    Tag = layer.LayerBitmap
                };

                Canvas.SetLeft(image, 0);
                Canvas.SetTop(image, 0);

                Field.Children.Add(image);

                UpdateCurrentItems();
            }
        }

        //Textbox einfügen
        public void InsertTextBox()
        {
            // Erstellen Sie eine neue TextBox
            TextEditor textBox = new TextEditor();
            textBox.Text = "Hier den Text eingeben";
            textBox.Width = 200;
            textBox.Height = 100;
            textBox.AllowDrop = true;
            textBox.Background = Brushes.Transparent;
            textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            textBox.Name = "Text";


            // Legen Sie die Position der TextBox im Canvas fest
            Canvas.SetLeft(textBox, 0);
            Canvas.SetTop(textBox, 0);

            // Fügen Sie die TextBox dem Canvas hinzu
            Field.Children.Add(textBox);

            UpdateCurrentItems();
        }

        //Codeeditor
        public void CreateCodeBox()
        {
            TextEditor codeBox = new TextEditor();
            codeBox.Text = "//Hier steht dein Code...";
            codeBox.FontFamily = new FontFamily("Consolas");
            codeBox.FontSize = 12;
            codeBox.Background = Brushes.DarkSlateGray;
            codeBox.Foreground = Brushes.White;
            codeBox.AllowDrop = true;
            codeBox.Width = 200;
            codeBox.Height = 100;
            codeBox.Name = "Code";

            // Lade das C# Syntaxdefinitionsschema
            IHighlightingDefinition customHighlighting = HighlightingManager.Instance.GetDefinition("C#");
            codeBox.SyntaxHighlighting = customHighlighting;

            // Füge das StackPanel zum Canvas hinzu
            Canvas.SetLeft(codeBox, 0 * (Field.Children.Count + 1)); // Platzierung basierend auf der Anzahl der bereits hinzugefügten Elemente
            Canvas.SetTop(codeBox, 0);
            Field.Children.Add(codeBox);

            UpdateCurrentItems();
        }

    }

}

