using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ICSharpCode.AvalonEdit;

namespace DevPad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Projectliste
        List<Project> projects = new List<Project>();

        //Project mit dem gearbeitet wird
        Project activProject;

        //Selectorrahmen
        Border selector = new Border();

        //Klassenerstellung
        DrawTool drawTool = new DrawTool();
        Edit edit;

        //Attribute---------------------------------------------------------------------------------------------------

        //Maus check
        private bool inWorkingArea = false;
        private bool Manipulate = false;

        //Modes
        private bool isDrawing = false;
        private bool isEditing = false;
        private bool isWriting = false;

        public MainWindow()
        {
            InitializeComponent();

            //Start in FullScreen
            this.WindowState = WindowState.Maximized;

            //Bearbeiten
            edit = new Edit(this);

            //Slider-Eventhandler für Linienstärke
            lineThicknessSlider.ValueChanged += LineThicknessSlider_ValueChanged;

            //ShortCut eventhandler
            PreviewKeyDown += Shortcuts;
        }

        //Tastaturshortcuts---------------------------------------------------------------------------------------------------------------------------------------
        private void Shortcuts(object sender, KeyEventArgs e)
        {
            if(activProject!= null && Keyboard.Modifiers == ModifierKeys.Control)
            {
                // Überprüfen, welcher Buchstabe gedrückt wurde
                switch (e.Key)
                {
                    case Key.S:
                        SaveXML(sender,e);
                        break;
                    case Key.Z:
                        UndoButton_Click(sender,e);
                        break;
                    case Key.U:
                        Redo_Click(sender,e);
                        break;
                    // Fügen Sie weitere Cases hinzu, um andere Buchstaben zu behandeln
                    default:
                        break;
                }
            }
        }

        //Obere Leiste--------------------------------------------------------------------------------------------------------------------------------------------

        //DateiButton
        private void Datei_Click(object sender, RoutedEventArgs e)
        {
            // Erstellen Sie das Kontextmenü
            ContextMenu contextMenu = new ContextMenu();

            //Neue Datei---
            MenuItem newFile = new MenuItem();
            newFile.Header = "Neu";
            newFile.Click += SetupProject;
            Image nfi = new Image();
            nfi.Source = new BitmapImage(new Uri("Assets/new.png", UriKind.Relative));
            nfi.Width = 14;
            nfi.Height = 14;
            newFile.Icon = nfi;
            contextMenu.Items.Add(newFile);

            //Laden---
            MenuItem load = new MenuItem();
            load.Header = "Laden";
            load.Click += LoadXML;
            Image ldi = new Image();
            ldi.Source = new BitmapImage(new Uri("Assets/load.png", UriKind.Relative));
            ldi.Width = 14;
            ldi.Height = 14;
            load.Icon = ldi;
            contextMenu.Items.Add(load);

            //Wird nur angezeigt wenn ein Projekt geladen ist
            if (activProject != null) 
            {
                //Speichern---
                MenuItem save = new MenuItem();
                save.Header = "Speichern";
                save.Click += SaveXML;
                Image saveImage = new Image();
                saveImage.Source = new BitmapImage(new Uri("Assets/save.png", UriKind.Relative));
                saveImage.Width = 14;
                saveImage.Height = 14;
                save.Icon = saveImage;
                contextMenu.Items.Add(save);

                //Speichern Als---
                MenuItem saveAs = new MenuItem();
                saveAs.Header = "Speichern Als";
                saveAs.Click += SaveUnder;
                Image saveAsImage = new Image();
                saveAsImage.Source = new BitmapImage(new Uri("Assets/save.png", UriKind.Relative));
                saveAsImage.Width = 14;
                saveAsImage.Height = 14;
                saveAs.Icon = saveAsImage;
                contextMenu.Items.Add(saveAs);

                //Exportieren---
                MenuItem export = new MenuItem();
                export.Header = "Als Bild Exportieren";
                export.Click += ExportImage;
                Image exportImg = new Image();
                exportImg.Source = new BitmapImage(new Uri("Assets/export.png", UriKind.Relative));
                exportImg.Width = 14;
                exportImg.Height = 14;
                export.Icon = exportImg;
                contextMenu.Items.Add(export);
            }
            // Zeige das Kontextmenu an
            contextMenu.IsOpen = true;
        }

        //TextButton
        private void Text_Click(object sender, RoutedEventArgs e)
        {
            if (activProject != null) 
            {
                // Erstellen Sie das Kontextmenü
                ContextMenu contextMenu = new ContextMenu();

                // Hinzufügen von Menüelementen zum Kontextmenü
                MenuItem text = new MenuItem();
                text.Header = "Textfeld einfügen";
                text.Click += InsertTextClick;
                Image t = new Image();
                t.Source = new BitmapImage(new Uri("Assets/text.png", UriKind.Relative));
                t.Width = 14;
                t.Height = 14;
                text.Icon = t;
                contextMenu.Items.Add(text);

                MenuItem code = new MenuItem();
                code.Header = "Codefeld einfügen";
                code.Click += InsertCodeClick;
                Image c = new Image();
                c.Source = new BitmapImage(new Uri("Assets/code.png", UriKind.Relative));
                c.Width = 14;
                c.Height = 14;
                code.Icon = c;
                contextMenu.Items.Add(code);

                // Anzeige des Kontextmenus
                contextMenu.IsOpen = true;
            }
            LoadLayer();
        }

        //BildButton
        private void Image_Click(object sender, RoutedEventArgs e)
        {
            if (activProject != null) 
            {
                // Erstellen Sie das Kontextmenü
                ContextMenu contextMenu = new ContextMenu();

                // Hinzufügen von Menüelementen zum Kontextmenü
                MenuItem insertPic = new MenuItem();
                insertPic.Header = "Bild einfügen";
                insertPic.Click += InsertImage;
                Image exportImg = new Image();
                exportImg.Source = new BitmapImage(new Uri("Assets/export.png", UriKind.Relative));
                exportImg.Width = 14;
                exportImg.Height = 14;
                insertPic.Icon = exportImg;
                contextMenu.Items.Add(insertPic);

                //Anzeige des Kontextmenus
                contextMenu.IsOpen = true;
            }
        }

        //Funktionen für die obere LeisteButtons------------------------------------------------------------------------------------------------------------------

        //Neue Datei
        public void SetupProject(object sender, RoutedEventArgs e)
        {
            Popup popup = new Popup(BT1, BT2, BT3, BT4, BT5, BT6, BT7, BT8, BT9, BT10);
            if (popup.ShowDialog() == true)
            {
                int width = popup.CanvasWidth;
                int height = popup.CanvasHeight;
                SolidColorBrush color = popup.color;
                //Neues Projectobjekt wird Instanziert
                Project project = new Project(width, height, color);

                //Wird der Liste hinzugefügt
                projects.Add(project);

                //Aktives Projekt aus das neu erstellte Projekt 
                activProject = project;

                //Canvas Children entfernen
                canvas.Children.Clear();

                //Canvas Resizen
                canvas.Width = project.Field.Width;
                canvas.Height = project.Field.Height;

                //Feld einfügen
                canvas.Children.Add(project.Field);
                UpdateProjectPanel();
                PlaceSelector();
            }
            LoadLayer();
        }

        //Speichern
        public void SaveXML(object sender, RoutedEventArgs e)
        {
            SaveLoad saveLoad = new SaveLoad(activProject);
            saveLoad.SaveCanvasToFile();
            UpdateProjectPanel();
        }

        //Speichern unter
        private void SaveUnder(object sender, RoutedEventArgs e)
        {
            SaveLoad saveLoad = new SaveLoad(activProject);
            saveLoad.SaveAs();
            UpdateProjectPanel();
        }

        //Laden
        public void LoadXML(object sender, RoutedEventArgs e)
        {
            Project p = new Project();
            SaveLoad saveLoad = new SaveLoad(p);
            saveLoad.LoadCanvasFromFile();

            //Wird der Liste hinzugefügt
            projects.Add(p);

            //Aktives Projekt aus das neu erstellte Projekt 
            activProject = p;

            UpdateProjectPanel();

            //Canvas Children entfernen
            canvas.Children.Clear();

            //Canvas Resizen
            canvas.Width = p.Field.Width;
            canvas.Height = p.Field.Height;

            //Feld einfügen
            canvas.Children.Add(p.Field);
            PlaceSelector();
        }

        //Exportieren
        public void ExportImage(object sender, RoutedEventArgs e)
        {
            Export.ExportCanvasAsImage(activProject.Field, activProject.Name);
        }

        //UndoButton
        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            if (activProject != null)
            {
                activProject.Undo();
            }
            LoadLayer();
            SelectorTrack();
        }

        //RedoButton
        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            if (activProject != null)
            {
                activProject.Redo();
            }
            LoadLayer();
            SelectorTrack();
        }

        //Text
        private void InsertTextClick(object sender, RoutedEventArgs e)
        {
            activProject.InsertTextBox();
            LoadLayer();
        }

        //Code
        private void InsertCodeClick(object sender, RoutedEventArgs e)
        {
            activProject.CreateCodeBox();
            LoadLayer();
        }

        //Bild
        private void InsertImage(object sender, RoutedEventArgs e)
        {
            activProject.InsertImage_Click();
            LoadLayer();
        }

        //Projektmanager(Leiste oben)-----------------------------------------------------------------------------------------------------------------------------

        //Tabgenerierung
        public void UpdateProjectPanel()
        {
            projectPanel.Children.Clear();
            foreach (Project p in projects)
            {
                //Stackpanel
                StackPanel stack = new StackPanel();
                stack.Orientation = Orientation.Horizontal;

                //Projekt auswählen
                Button button = new Button();
                button.Click += ChangeProject;
                button.Content = p.Name;
                button.Background = Brushes.Gray;
                button.Tag = p;
                button.Margin = new Thickness(3, 0, 0, 0);

                //Projekt entfernen
                Button remove = new Button();
                remove.Click += CloseProject;
                remove.Background = Brushes.Gray;
                remove.Tag = p;
                remove.Content = "X";

                stack.Children.Add(button);
                stack.Children.Add(remove);

                projectPanel.Children.Add(stack);
            }
            ChangeButtonPanel();
            LoadLayer();
        }

        //Projekt schliessen
        public void CloseProject(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            canvas.Children.Clear();
            canvas.Width = 0;
            canvas.Height = 0;
            activProject = null;
            projects.Remove(btn.Tag as Project);
            UpdateProjectPanel();
            LoadLayer();
        }

        //Projekt auf Knopfdruck ändern
        public void ChangeProject(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            canvas.Children.Clear();
            activProject = btn.Tag as Project;

            canvas.Width = activProject.Field.Width;
            canvas.Height = activProject.Field.Height;
            canvas.Children.Add(activProject.Field);

            ChangeButtonPanel();
            LoadLayer();
            PlaceSelector();
        }

        //Projektauswahl Buttonfarbe ändern
        public void ChangeButtonPanel()
        {
            //reset aller ButtonFarben
            foreach (var child in projectPanel.Children)
            {
                if (child is StackPanel sp)
                {
                    foreach (var child2 in sp.Children)
                    {
                        if (child2 is Button button)
                        {
                            if (button.Tag == activProject)
                            {
                                button.Background = Brushes.LightGray;
                            }
                            else
                            {
                                button.Background = Brushes.Gray;
                            }
                        }
                    }
                }
            }
        }

        //MenuleistenFunktionen-----------------------------------------------------------------------------------------------------------------------------------

        //Menu Links ein/ausfahren
        private void LMenu_Click(object sender, RoutedEventArgs e)
        {
            //Zwischen 200 un 0px togglen
            bool ToolsVisible = LBar.ActualWidth == 200;

            if (ToolsVisible)
            {
                LBar.Width = new GridLength(0);
            }
            else
            {
                LBar.Width = new GridLength(200);
            }
        }

        //Menu rechts ein/ausfahren
        private void RMenu_Click(object sender, RoutedEventArgs e)
        {
            //Zwischen 200 un 0px togglen
            bool ToolsVisible = RBar.ActualWidth == 200;

            if (ToolsVisible)
            {
                RBar.Width = new GridLength(0);
            }
            else
            {
                RBar.Width = new GridLength(200);
            }
        }

        //Menuleiste links----------------------------------------------------------------------------------------------------------------------------------------

        //ZeichnenMode
        private void draw_Click(object sender, RoutedEventArgs e)
        {
            //Setze Booleans
            isDrawing = true;
            isEditing = false;
            isWriting = false;

            //Visibility ändern
            DrawTools.Visibility = Visibility.Visible;
            EditTools.Visibility = Visibility.Hidden;
            WriteTools.Visibility = Visibility.Hidden;
            ChangeMode(sender, e);
        }

        //BearbeitenMode
        private void custom_Click(object sender, RoutedEventArgs e)
        {
            //Werte Setzen
            isEditing = true;
            isWriting = false;
            isDrawing = false;

            //Visibility ändern
            DrawTools.Visibility = Visibility.Hidden;
            EditTools.Visibility = Visibility.Visible;
            WriteTools.Visibility = Visibility.Hidden;
            ChangeMode(sender, e);
        }

        //SchreibenMode
        private void write_Click(object sender, RoutedEventArgs e)
        {
            //Werte Setzen
            isWriting = true;
            isEditing = false;
            isDrawing = false;

            //Visibility ändern
            DrawTools.Visibility = Visibility.Hidden;
            EditTools.Visibility = Visibility.Hidden;
            WriteTools.Visibility = Visibility.Visible;
            ChangeMode(sender, e);
        }

        //Selector für die Modis(Rand der Knöpfe verändern)
        private void ChangeMode(object Sender, RoutedEventArgs e)
        {
            Button button = Sender as Button;
            foreach (Button btn in modes.Children)
            {
                btn.BorderThickness = new Thickness(1);
            }
            button.BorderThickness = new Thickness(2);
        }

        //ZeichenModus--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        //Zeichnen
        private void Pen_Click(object sender, RoutedEventArgs e)
        {
            drawTool.Draw = true;
            drawTool.Erase = false;
            drawTool.Fill = false;
            drawTool.Pick = false;
            ChangeModeColor(sender);
        }

        //Radieren
        private void Eraser_Click(object sender, RoutedEventArgs e)
        {
            drawTool.Draw = false;
            drawTool.Erase = true;
            drawTool.Fill = false;
            drawTool.Pick = false;
            ChangeModeColor(sender);
        }

        //Füllen
        private void FloodFill_Click(object sender, RoutedEventArgs e)
        {
            drawTool.Draw = false;
            drawTool.Erase = false;
            drawTool.Fill = true;
            drawTool.Pick = false;
            ChangeModeColor(sender);
        }

        //Farbpipette
        private void PickColor_Click(object sender, RoutedEventArgs e)
        {
            drawTool.Draw = false;
            drawTool.Erase = false;
            drawTool.Fill = false;
            drawTool.Pick = true;
            ChangeModeColor(sender);
        }

        private void ChangeModeColor(object sender)
        {
            Button btn = sender as Button;

            foreach (Button button in DrawToolBox.Children)
            {
                button.BorderThickness = new Thickness(1);
            }
            btn.BorderThickness = new Thickness(3);
        }

        //Farbpalette ändern
        private void ChangeColor_Click(object sender, RoutedEventArgs e)
        {
            ColorPicker colorPicker = new ColorPicker(BT1, BT2, BT3, BT4, BT5, BT6, BT7, BT8, BT9, BT10, this);
            if (colorPicker.ShowDialog() == true)
            {

            }
        }

        //Liniendicke über slider
        private void LineThicknessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Setzen Sie die Dicke der gezeichneten Linien entsprechend des Sliderwerts
            drawTool.lineThickness = (int)e.NewValue;
        }

        //Farbauswahl(Um die Farbe zum Zeichnen zu ändern)
        private void BrushButton_Click(object sender, RoutedEventArgs e)
        {
            // Den ausgewählten Button ermitteln
            Button button = sender as Button;

            if (button != null)
            {
                // Die Hintergrundfarbe des Buttons als SolidColorBrush abrufen
                drawTool.SelectedColor = button.Background as SolidColorBrush;

                // Alle Buttons im Grid durchgehen und den Rahmen entfernen
                foreach (Button btn in colorGrid.Children)
                {
                    btn.BorderThickness = new Thickness(1);
                }

                // Den Rahmen für den ausgewählten Button hinzufügen
                button.BorderThickness = new Thickness(2);
                button.BorderBrush = Brushes.Black;
            }
        }

        //Menu rechts---------------------------------------------------------------------------------------------------------------------------------------------

        //Layer Hinzufügen +Button
        private void AddLayer_Click(object sender, RoutedEventArgs e)
        {
            if (activProject != null) 
            {
                Popup popup = new Popup(BT1, BT2, BT3, BT4, BT5, BT6, BT7, BT8, BT9, BT10);
                if (popup.ShowDialog() == true)
                {
                    int width = popup.CanvasWidth;
                    int height = popup.CanvasHeight;
                    SolidColorBrush color = popup.color;
                    //Neues Projectobjekt wird Instanziert
                    activProject.NewLayer(width, height, color);

                    UpdateProjectPanel();
                }
                LoadLayer();
            }
        }

        //Layer entfernen -Button
        private void RemoveLayer_Click(object sender, RoutedEventArgs e)
        {
            if (activProject != null && activProject.selectedElement != null)
            {
                activProject.DeleteLayer();
                LoadLayer();
                SelectorTrack();
            }
        }

        //Ebenenmanager Objekte laden
        public void LoadLayer()
        {
            if (activProject != null)
            {
                int i = 0;
                LayerManager.Items.Clear();
                foreach (ElementState objectlayer in activProject.allElements)
                {
                    //Rand
                    Border border = new Border();
                    border.BorderThickness = new Thickness(1);
                    border.BorderBrush = Brushes.Black;

                    //Grid erstellen
                    Grid panel = new Grid();
                    panel.Background = Brushes.Gray;

                    //Collumdefinition
                    ColumnDefinition column1 = new ColumnDefinition();
                    column1.Width = new GridLength(1,GridUnitType.Auto);
                    ColumnDefinition collum2 = new ColumnDefinition();
                    collum2.Width = new GridLength (1, GridUnitType.Auto);
                    ColumnDefinition column3 = new ColumnDefinition();
                    column3.Width = new GridLength(1, GridUnitType.Star);
                    panel.ColumnDefinitions.Add(column1);
                    panel.ColumnDefinitions.Add (collum2);
                    panel.ColumnDefinitions.Add(column3);

                    if (objectlayer.Element is Image icon)
                    {
                        //Vorschaubild
                        Image image = new Image();
                        image.Source = icon.Source;
                        image.Height = 30;
                        image.Width = 30;
                        Grid.SetColumn(image,0);
                        panel.Children.Add(image);
                    }
                    else
                    {
                        //Vorschau dass es ein Textblock ist
                        TextBlock textBlock = new TextBlock();
                        textBlock.Height = 30;
                        textBlock.Width = 30;
                        textBlock.Text = "T";
                        textBlock.TextAlignment = TextAlignment.Center;
                        Grid.SetColumn(textBlock,0);
                        panel.Children.Add(textBlock);
                    }

                    //Visibillitybutton
                    ToggleButton visible = new ToggleButton();
                    if (objectlayer.Element.Visibility == Visibility.Visible)
                    {
                        visible.IsChecked = true;
                    }
                    else
                    {
                        visible.IsChecked = false;
                    }
                    
                    visible.Click += Visible_Click;
                    visible.Tag = objectlayer.Element as UIElement;
                    visible.Background = Brushes.Transparent;
                    Image logo = new Image();
                    logo.Source = new BitmapImage(new Uri("Assets/v.png", UriKind.Relative));
                    //logo.Height = 20;
                    logo.Width = 20;
                    visible.Content = logo;
                    Grid.SetColumn(visible,1);
                    panel.Children.Add(visible);

                    //EbenenLayer
                    TextBlock label = new TextBlock();
                    label.Text = "Ebene " + i;
                    label.HorizontalAlignment = HorizontalAlignment.Stretch;
                    label.Width = 120;
                    label.Margin = new Thickness(5);
                    
                    Grid.SetColumn(label,2);
                    panel.Children.Add(label);

                    border.Child = panel;

                    ListBoxItem item = new ListBoxItem();
                    item.Tag = objectlayer.Element as UIElement;
                    item.Background = Brushes.LightGray;
                    item.HorizontalAlignment = HorizontalAlignment.Stretch;
                    item.Content = border;
                    item.PreviewMouseLeftButtonDown += Item_MouseDown;
                    item.IsEnabled = true;
                    item.BorderBrush = Brushes.Black;
                    item.BorderThickness = new Thickness(1);

                    //Das neue Item über dem alten Item einfügen
                    LayerManager.Items.Insert(0, item);
                    i++;
                }
            }
            else
            {
                LayerManager.Items.Clear(); 
            }
        }

        private void Visible_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton btn = sender as ToggleButton;

            UIElement element = btn.Tag as UIElement;

            if (btn.IsChecked == true)
            {
                element.Visibility = Visibility.Visible;
            }
            else
            {
                element.Visibility = Visibility.Hidden;
            }
        }

        //Falls ein Element(Layer) ausgewählt wurde
        private void Item_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item = sender as ListBoxItem;

            //Farbänderung
            foreach(ListBoxItem listBox in LayerManager.Items)
            {
                listBox.BorderBrush = Brushes.Black;
                listBox.BorderThickness = new Thickness(1);
            }
            //Ausgewähltes Element Rand ändern
            item.BorderBrush = Brushes.LightGreen;
            item.BorderThickness = new Thickness(3);

            UIElement thisElement = item.Tag as UIElement;
            if (thisElement is Image)
            {
                //Canvasauswahl
                activProject.selectedElement = thisElement as Image;
            }
            else
            {
                activProject.selectedElement = thisElement;
            }

            SelectorTrack();
        }

        //Maus Steuerung------------------------------------------------------------------------------------------------------------------------------------------

        //Maus runter
        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (activProject != null) 
            {
                if (inWorkingArea)
                {
                    //Setze den ManipulatorBool auf true
                    Manipulate = true;

                    //Wenn CRTL nicht gedrückt STANDARTFUNKTIONEN!!
                    if (isDrawing)
                    {
                        activProject.selectedBitmap = edit.GetBitmap(activProject.selectedElement);

                        if (drawTool.Fill || drawTool.Erase || drawTool.Draw)
                        {
                            drawTool.StartDrawLine(e, activProject.selectedElement, activProject.selectedBitmap);
                        }

                        if (drawTool.Pick)
                        {
                            foreach (Button btn in colorGrid.Children)
                            {
                                if (btn.BorderThickness == new Thickness(2))
                                {
                                    Point point = e.GetPosition(activProject.selectedElement);
                                    Color color = drawTool.GetPixelColor((int)point.X,(int)point.Y, activProject.selectedBitmap);
                                    drawTool.SelectedColor = new SolidColorBrush(color);
                                    btn.Background = new SolidColorBrush(color);
                                }
                            }
                        }
                        
                    }

                    //Bearbeitungsmodus
                    if (isEditing)
                    {
                        edit.SelectAndMove(activProject.selectedElement);
                    }
                }
            }
        }

        //Maustaste loslassen
        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (activProject != null)
            {
                //Speichert Linie als ganzes um die ganze Linie zu Undoen
                if (isDrawing || isEditing)
                {
                    activProject.UpdateCurrentItems();
                }
                //Reset der ausgewählten Layer
                Manipulate = false;
                activProject.selectedBitmap = null;
            }
        }

        //Maus Bewegen
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            //Prüft ob es im Arbeitsbereich ist
            inWorkingArea = IsMouseInsideMainGrid(e);

            //Textmodus Prüfen
            CheckTextMode(isWriting);

            if (inWorkingArea)
            {
                //Selector Bewegen
                SelectorTrack();

                //Falls linke Maustaste gedrückt wird
                if (activProject != null && Manipulate) 
                {
                    //falls Zeichnen ausgewählt wurde
                    if (isDrawing)
                    {
                        activProject.selectedBitmap = edit.GetBitmap(activProject.selectedElement);
                        drawTool.MoveLine(e, activProject.selectedElement, activProject.selectedBitmap);
                    }

                    //falls Bearbeiten gewählt wurde
                    if (isEditing)
                    {
                        edit.MoveSelectedElement(e, activProject.selectedElement);
                    }
                }
            }
        }

        //Mausrad
        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //Zoom
            if (Keyboard.Modifiers == ModifierKeys.Control)
            { 
                // Wenn das Mausrad nach vorne (nach oben) gedreht wird, zoomen Sie hinein.
                // Andernfalls, zoomen Sie heraus.
                double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;

                // Aktuelle Skalierung des Canvas
                double currentScale = canvas.LayoutTransform.Value.M11;

                // Neuen Zoom berechnen
                double newScale = currentScale * zoomFactor;

                // Mindest- und Höchstzoom begrenzen
                if (newScale < 0.1)
                    newScale = 0.1;
                if (newScale > 10)
                    newScale = 10;

                // Zoom-Transformation anwenden
                ScaleTransform scaleTransform = new ScaleTransform(newScale, newScale);
                canvas.LayoutTransform = scaleTransform;
            }
        }

        //Grundfunktionen------------------------------------------------------------------------------------------------------------------

        //Mauszeigerüberprüfung
        public static bool IsMouseInsideMainGrid(MouseEventArgs e)
        {
            Grid work = Application.Current.MainWindow.FindName("work") as Grid;

            if (work != null)
            {
                // Mausposition relativ zum Grid "work"
                Point mousePosition = e.GetPosition(work);

                // Überprüfen, ob der Mauszeiger innerhalb der Grenzen des Grids "work" liegt
                return mousePosition.X >= 0 && mousePosition.X <= work.ActualWidth &&
                       mousePosition.Y >= 0 && mousePosition.Y <= work.ActualHeight;
            }

            // Falls das Grid "work" nicht gefunden wurde, wird false zurückgegeben
            return false;
        }

        //Schreibemodus--------------------------------------
        public void CheckTextMode(bool isWriting)
        {
            if (activProject != null)
            {
                if (canvas == null)
                    return;

                foreach (var item in activProject.Field.Children)
                {
                    if (item is TextEditor)
                    {
                        TextEditor textBox = item as TextEditor;
                        textBox.IsReadOnly = !isWriting;
                    }

                }
            }
        }

        //Selector für die Ausgewählten Elemente------------------------------------------------------------------------------------------------------------------

        //Auswahlborder erstellen
        private void PlaceSelector()
        {
            selector.BorderBrush = Brushes.Black;
            selector.Width = 0;
            selector.Height = 0;
            selector.Name = "Selector";
            canvas.Children.Add(selector);
        }

        //Auswahlborder an das Ausgewählte Element anhängen
        private void SelectorTrack()
        {
            // Überprüfen, ob ein Projekt ausgewählt ist und ein Element im Projekt ausgewählt wurde
            if (activProject != null && activProject.selectedElement != null)
            {
                // Abrufen der aktuellen Größe des ausgewählten Elements
                Size size = activProject.selectedElement.RenderSize;

                // Festlegen der Position des Auswahlrahmens
                Canvas.SetTop(selector, Canvas.GetTop(activProject.selectedElement));
                Canvas.SetLeft(selector, Canvas.GetLeft(activProject.selectedElement));

                // Festlegen der Breite und Höhe des Auswahlrahmens
                selector.Width = size.Width;
                selector.Height = size.Height;
                selector.BorderThickness = new Thickness(2);
            }
            else
            {
                selector.Width = 0;
                selector.Height = 0;
                selector.BorderThickness = new Thickness(0);
            }
        }

    }
}
