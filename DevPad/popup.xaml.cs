using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DevPad
{
    /// <summary>
    /// Interaktionslogik für popup.xaml
    /// </summary>
    public partial class Popup : Window
    {
        public int CanvasWidth { get; private set; }
        public int CanvasHeight { get; private set; }

        public SolidColorBrush color = Brushes.White;

        public Popup(Button b1, Button b2, Button b3, Button b4, Button b5,Button b6, Button b7, Button b8, Button b9, Button b10)
        {
            InitializeComponent();

            //Setze Hintergrundfarben
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

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(WidthTextBox.Text, out int width) && int.TryParse(HeightTextBox.Text, out int height))
            {
                CanvasWidth = width;
                CanvasHeight = height;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Bitte gültige Werte eingeben.");
            }
        }

        //Farbauswahl
        private void BrushButton_Click(object sender, RoutedEventArgs e)
        {
            // Den ausgewählten Button ermitteln
            Button button = sender as Button;

            if (button != null)
            {
                // Die Hintergrundfarbe des Buttons als SolidColorBrush abrufen
                color = button.Background as SolidColorBrush;


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
    }
}
