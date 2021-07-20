using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QuantBook.Ch01
{
    /// <summary>
    /// Interaction logic for CodeOnlyView.xaml
    /// </summary>
    public partial class CodeOnlyView : UserControl
    {
        private TextBlock txBlock;
        private TextBox txBox;

        public CodeOnlyView()
        {
            InitializeComponent();
            SetInitialization();
        }

        private void SetInitialization()
        {
            // Configure the UserControl:
            this.Height = 300;
            this.Width = 300;

            // Create Grid and StackPanel and add them to UserControl:
            Grid grid = new Grid();
            StackPanel stackPanel = new StackPanel();
            grid.Children.Add(stackPanel);
            this.Content = grid;

            // Add a text block to stackPanel:
            txBlock = new TextBlock();
            txBlock.Margin = new Thickness(5);
            txBlock.Height = 30;
            txBlock.TextAlignment = TextAlignment.Center;
            txBlock.Text = "Hello WPF!";
            stackPanel.Children.Add(txBlock);

            // Add a text box to stackPanel:
            txBox = new TextBox();
            txBox.Margin = new Thickness(5);
            txBox.Width = 200;
            txBox.TextAlignment = TextAlignment.Center;
            txBox.TextChanged += OnTextChanged;
            stackPanel.Children.Add(txBox);

            // Add button to stackPanel used to change text color:
            Button btnColor = new Button();
            btnColor.Margin = new Thickness(5);
            btnColor.Width = 200;
            btnColor.Content = "Change Text Color";
            btnColor.Click += btnChangeColor_Click;
            stackPanel.Children.Add(btnColor);

            // Add button to stackPanel used to change text font size:
            Button btnSize = new Button();
            btnSize.Margin = new Thickness(5);
            btnSize.Width = 200;
            btnSize.Content = "Change Text Size";
            btnSize.Click += btnChangeSize_Click;
            stackPanel.Children.Add(btnSize);
        }

        private void OnTextChanged(object sender,
            TextChangedEventArgs e)
        {
            txBlock.Text = txBox.Text;
        }

        private void btnChangeColor_Click(object sender,
            RoutedEventArgs e)
        {
            if (txBlock.Foreground == Brushes.Black)
                txBlock.Foreground = Brushes.Red;
            else
                txBlock.Foreground = Brushes.Black;
        }

        private void btnChangeSize_Click(object sender,
            RoutedEventArgs e)
        {
            if (txBlock.FontSize == 11)
                txBlock.FontSize = 24;
            else
                txBlock.FontSize = 11;
        }
    }
}
