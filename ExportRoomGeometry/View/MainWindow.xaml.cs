using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ExportRoomGeometry.View
{
    public partial class MainWindow : Window, IDisposable
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void Dispose()
        {
            
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Char.IsDigit(e.Text, 0) || (e.Text == "."))
            {
                e.Handled = false;
            }
            else e.Handled = true;
                  
        }

    }
}
