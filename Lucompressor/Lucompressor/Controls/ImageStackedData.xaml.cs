using Microsoft.UI.Xaml.Controls;
using Lucompressor_RTComp;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Lucompressor.Controls
{
    public sealed partial class ImageStackedData : UserControl, INotifyPropertyChanged
    {
        private ImageProps m_imgsettings = new ImageProps();

        public ImageProps ImgSettings
        {
            get => m_imgsettings;
            set
            {
                m_imgsettings = value;
                OnPropertyChanged(nameof(ImgSettings));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public ImageStackedData()
        {
            this.InitializeComponent();
        }

        private void ValidateNumber_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (args.NewText.Length != 0)
                args.Cancel = !int.TryParse(args.NewText, out _);
        }

        private void TBX_Height_TextChanged(object sender, TextChangedEventArgs e)
        {
            ImgSettings.ImgHeight = TBX_Height.Text.Length == 0 ? 0 : int.Parse(TBX_Height.Text);
        }

        private void TBX_Width_TextChanged(object sender, TextChangedEventArgs e)
        {            
            ImgSettings.ImgWidth = TBX_Width.Text.Length == 0 ? 0 : int.Parse(TBX_Width.Text);
        }

        private void SLD_Quality_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            ImgSettings.ImgQualityValue = e.NewValue;
        }
    }
}
