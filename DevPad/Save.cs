using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Xml.Serialization;
using ICSharpCode.AvalonEdit;
using System.Windows.Media;
using FastColoredTextBoxNS;
using ICSharpCode.AvalonEdit.Highlighting;



namespace DevPad
{
    [Serializable]
    public class SerializableElementState
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] BitmapBytes { get; set; }

        //TextboxSpezifisch
        public string Text { get; set; }
        public string Background { get; set; } // Store as string
        public string Foreground { get; set; } // Store as string
        public string Typ {  get; set; } //Check ob es ein Codeblock ist
        public double Size { get; set; }

    }

    public class SaveLoad
    {
        Project p;
        public SaveLoad(Project project) 
        { 
            p = project;
        }


        // Speichern normal
        public void SaveCanvasToFile()
        {
            if (!File.Exists(p.SavePath)) 
            {
                // Dateidialog öffnen, um Speicherpfad auszuwählen
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "XML Dateien (*.xml)|*.xml";
                saveFileDialog.FileName = p.Name;
                if (saveFileDialog.ShowDialog() == true)
                {
                    //Namen ändern zu gespeicherten Namen
                    p.Name = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
                    p.SavePath = saveFileDialog.FileName;
                    Save(saveFileDialog.FileName);
                }
            }
            else { Save(p.SavePath); }
        }

        //Speichern unter
        public void SaveAs()
        {
            // Dateidialog öffnen, um Speicherpfad auszuwählen
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML Dateien (*.xml)|*.xml";
            saveFileDialog.FileName = p.Name;
            if (saveFileDialog.ShowDialog() == true)
            {
                //Namen ändern zu gespeicherten Namen
                p.Name = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
                p.SavePath = saveFileDialog.FileName;
                Save(saveFileDialog.FileName);
            }
        }

        //Speichern
        public void Save(string fileName)
        {
            // Erstelle eine Liste von serialisierbaren Elementen
            List<SerializableElementState> serializableElements = new List<SerializableElementState>();

            //Für jedes Element in elementState
            foreach (ElementState elementState in p.allElements)
            {
                // Überprüfe, ob das Element ein Image ist und eine Bitmap hat
                if (elementState.Element is Image image && elementState.Bitmap is WriteableBitmap bitmap)
                {
                    // Füge serialisierbares Element hinzu (Bilder)
                    serializableElements.Add(new SerializableElementState
                    {
                        Left = elementState.Left,
                        Top = elementState.Top,
                        Width = elementState.Width,
                        Height = elementState.Height,
                        BitmapBytes = ImageToBytes(bitmap),
                    });
                }
                else
                {
                    // Füge serialisierbares Element hinzu (Textboxen)
                    serializableElements.Add(new SerializableElementState
                    {
                        Left = elementState.Left,
                        Top = elementState.Top,
                        Width = elementState.Width,
                        Height = elementState.Height,
                        Text = elementState.Text,
                        Background = BrushToString(elementState.Background),
                        Foreground = BrushToString(elementState.Foreground),
                        Typ = elementState.Typ,
                        Size = elementState.Size,
                    });
                }
            }

            // Serialisiere die Liste und speichere sie in eine XML-Datei
            XmlSerializer serializer = new XmlSerializer(typeof(List<SerializableElementState>));
            using (FileStream stream = new FileStream(fileName, FileMode.Create))
            {
                serializer.Serialize(stream, serializableElements);
            }
        }

        // Laden des Canvas aus einer Datei
        public void LoadCanvasFromFile()
        {
            // Dateidialog öffnen, um Ladepfad auszuwählen
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Dateien (*.xml)|*.xml";
            if (openFileDialog.ShowDialog() == true)
            {
                p.Name = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                p.SavePath = openFileDialog.FileName;
                // Deserialisiere die Liste aus der XML-Datei
                XmlSerializer serializer = new XmlSerializer(typeof(List<SerializableElementState>));
                using (FileStream stream = new FileStream(openFileDialog.FileName, FileMode.Open))
                {
                    List<SerializableElementState> serializableElements = (List<SerializableElementState>)serializer.Deserialize(stream);

                    // Lösche alle vorhandenen Elemente aus dem Canvas
                    p.Field.Children.Clear();
                    p.allElements.Clear();

                    // Erstelle und füge die Elemente basierend auf den deserialisierten Daten hinzu
                    foreach (SerializableElementState serializableElement in serializableElements)
                    {
                        UIElement element;
                        if (serializableElement.BitmapBytes != null)
                        {
                            //Falls es ein Bitmap hat ist es ein Bild
                            WriteableBitmap bitmap = BytesToImage(serializableElement.BitmapBytes);
                            Image image = new Image
                            {
                                Source = bitmap,
                                Width = serializableElement.Width,
                                Height = serializableElement.Height,
                                Tag = bitmap 
                            };

                            //Setze die Image parameter in Element
                            element = image;
                        }
                        else
                        {
                            //Andernfalls ist es ein TextEditor
                            TextEditor editor = new TextEditor
                            {
                                Width = serializableElement.Width,
                                Height = serializableElement.Height,
                                Background = StringToBrush(serializableElement.Background),
                                Foreground = StringToBrush(serializableElement.Foreground),
                                Text = serializableElement.Text,
                                Name = serializableElement.Typ,
                                FontSize = serializableElement.Size,
                            };
                            //Check ob es sich um ein Codefeld handelt
                            if (editor.Name == "Code")
                            {
                                // Lade das C# Syntaxdefinitionsschema
                                IHighlightingDefinition customHighlighting = HighlightingManager.Instance.GetDefinition("C#");
                                editor.SyntaxHighlighting = customHighlighting;
                            }
                            else
                            {
                                editor.Background = Brushes.Transparent;
                                editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                                editor.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                            }
                            element = editor;
                        }

                        // Setze die Position des Elements im Canvas
                        Canvas.SetLeft(element, serializableElement.Left);
                        Canvas.SetTop(element, serializableElement.Top);

                        // Füge das Element dem Canvas hinzu
                        p.Field.Children.Add(element);

                        //Falls es ein Image ist
                        if (element is Image) 
                        {
                            // Füge das Element der Liste hinzu
                            p.allElements.Add(new ElementState
                            {
                                Element = element,
                                Left = serializableElement.Left,
                                Top = serializableElement.Top,
                                Width = serializableElement.Width,
                                Height = serializableElement.Height,
                                Bitmap = element is Image imageElement ? (WriteableBitmap)imageElement.Source : null,
                                Tag = element is Image tag ? (WriteableBitmap)tag.Source : null,
                            });
                        }

                        //Falls es ein Texteditor ist
                        if (element is TextEditor)
                        {
                            // Füge das Element der Liste hinzu
                            p.allElements.Add(new ElementState
                            {
                                Element = element,
                                Left = serializableElement.Left,
                                Top = serializableElement.Top,
                                Width = serializableElement.Width,
                                Height = serializableElement.Height,
                                Text = serializableElement.Text,
                                Background = StringToBrush(serializableElement.Background),
                                Foreground = StringToBrush(serializableElement.Foreground),
                                Typ = serializableElement.Typ,
                                Size = serializableElement.Size,
                            });
                        }

                    }
                    p.SaveInUndo();
                }
            }
        }

        // Hilfsmethode, um ein Image in ein Byte-Array zu konvertieren
        private byte[] ImageToBytes(WriteableBitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(stream);
                return stream.ToArray();
            }
        }

        // Hilfsmethode, um ein Byte-Array in ein Image zu konvertieren
        private WriteableBitmap BytesToImage(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                BitmapDecoder decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                return new WriteableBitmap(decoder.Frames[0]);
            }
        }

        // Serialize Brush to string
        private string BrushToString(Brush brush)
        {
            if (brush is SolidColorBrush solidColorBrush)
            {
                return solidColorBrush.Color.ToString();
            }
            return null;
        }

        // Deserialize string to Brush
        private Brush StringToBrush(string color)
        {
            if (color != null)
            {
                return (Brush)new BrushConverter().ConvertFromString(color);
            }
            return null;
        }

    }
}


   
