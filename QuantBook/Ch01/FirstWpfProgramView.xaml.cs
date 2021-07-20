using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QuantBook.Ch01
{
    /// <summary>
    /// Interaction logic for FirstWpfProgramView.xaml
    /// </summary>
    public partial class FirstWpfProgramView : UserControl
    {
        public FirstWpfProgramView()
        {
            InitializeComponent();
        }

        private void txBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            txBlock.Text = txBox.Text;
        }

        private void btnChangeColor_Click(object sender, RoutedEventArgs e)
        {
            if (txBlock.Foreground == Brushes.Black)
                txBlock.Foreground = Brushes.Red;
            else
                txBlock.Foreground = Brushes.Black;
        }

        private void btnChangeSize_Click(object sender, RoutedEventArgs e)
        {
            if (txBlock.FontSize == 11)
                txBlock.FontSize = 24;
            else
                txBlock.FontSize = 11;
        }
    }
}
