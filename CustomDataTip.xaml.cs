using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace CustomDataTip
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class CustomDataTipControl : UserControl
    {
        /// <summary>
        /// Foreground color
        /// </summary>
        public SolidColorBrush ForeBrush { get; set; }

        /// <summary>
        /// Background color
        /// </summary>
        public SolidColorBrush BackBrush { get; set; }

        public CustomDataTipControl()
        {
            InitializeComponent();
            var theme = ThemeUtil.GetCurrentTheme();

            BackBrush = Brushes.DarkGray;
            ForeBrush = Brushes.AntiqueWhite;

            var converter = new BrushConverter();

            switch (theme)
            {
                case VsTheme.Blue:
                    BackBrush = (SolidColorBrush) converter.ConvertFromString("#FF1B5277");
                    break;
                case VsTheme.Dark:
                    BackBrush = Brushes.DimGray;
                    break;
                case VsTheme.Light:
                    BackBrush = Brushes.LightGray;
                    ForeBrush = Brushes.Black;
                    break;
                case VsTheme.Unknown:
                    BackBrush = Brushes.DarkGray;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ResultTreeView.Background = BackBrush;
            ResultTreeView.Foreground = ForeBrush;
        }
    }
}