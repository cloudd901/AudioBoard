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

namespace AudioBoard
{
    /// <summary>
    /// Interaction logic for PopupURL.xaml
    /// </summary>
    public partial class PopupURL : Window
    {
        public string AudioTitle = "";
        public string AudioUrl = "";

        public PopupURL()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                if (b.Content.ToString() == "OK")
                {
                    AudioTitle = textBoxTitle.Text;
                    AudioUrl = textBoxURL.Text;
                    this.DialogResult = true;
                }
                else
                {
                    this.DialogResult = false;
                }

                this.Close();
            }
        }
    }
}
