using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF_Thumbnails
{
    /// <summary>
    /// Interaction logic for EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow : UserControl
    {
        private Export _parent;
        private BitmapSource _source;
        private BoxArtFrame _frame;
        public EditorWindow()
        {
            InitializeComponent();
        }

        public void SetImage(BitmapSource source, BoxArtFrame frame, Export parent)
        {
            CropControl.SetImage(source, frame.Width, frame.Height);
            CropControl.Callback = RenderFrames;
            _parent = parent;
            _source = source;
            _frame = frame;
            RenderFrames();
        }

        public void RenderFrames()
        {
            if (_parent.font_size.Value == null || _parent.title_shift_h.Value == null || _parent.title_shift_v.Value == null) return;
            var fmt = DrawTextFormat.Make((CropControl.TglTextOverrideTF.IsChecked == true ? CropControl.Typeface : _parent.Typeface),
                (double)(CropControl.TglTextOverride.IsChecked == true ? CropControl.ov_font_size.Value ?? _parent.font_size.Value : _parent.font_size.Value),
                ((SolidColorBrush)_parent.fore_title_color_box.Fill).Color,
                _parent.chkb_bkg_title.IsChecked,
                _parent.chkb_otln.IsChecked,
                _parent.chkb_drpshdw.IsChecked,
                _parent.chkb_centering.IsChecked,
                true,
                new Point((double)_parent.title_shift_h.Value, (double)_parent.title_shift_v.Value),
                backgroundColor: ((SolidColorBrush)_parent.bkg_title_color_box.Fill).Color);
            BitmapSource titleFrame;
            BitmapSource frame;
            //attempt resize without stretching
            switch (CropControl.ImageCrop.SelectedIndex)
            {
                case 1:
                    frame = _source.ResizedImage(_frame.Width, _frame.Height);
                    break;
                case 2:
                    frame = _source.ScaledCrop(_frame.Width, _frame.Height, CropControl.ImageScale, CropControl.CropX.Value,
                        CropControl.CropY.Value);
                    break;
                default:
                    frame = _source.ResizeImageAndRatio(_frame.Width, _frame.Height, (int)CropControl.WidthAdjustment,
                        (int)CropControl.HeightAdjustment, CropControl.ImageScale);
                    break;
            }


            if (frame == null)
            {
                System.Windows.MessageBox.Show(
                    "Image adjustments could not be made as the values given would cause errors in positioning.");
                return;
            }

            //force stretch to conform sizing
            if (frame.PixelWidth != _frame.Width || frame.PixelHeight != _frame.Height) frame = frame.ResizedImage(_frame.Width, _frame.Height);

            if (_parent.text_title.LineCount > 1)
            {
                string[] lines = new string[_parent.text_title.LineCount];
                for (int i = 0; i < _parent.text_title.LineCount; i++)
                {
                    lines[i] = _parent.text_title.GetLineText(i);
                }

                titleFrame = frame.DrawText(lines, fmt);
            }
            else
                titleFrame = frame.DrawText(_parent.text_title.Text, fmt);

            if (_parent.logo_file.Source != null)
            {
                double scale = ((double)(_parent.logo_scale.Value ?? 1) / 100);
                if (Math.Abs(scale) < 0.001) scale = 1;
                double sWidth = _parent.logo_file.Source.Width * scale;

                double sHeight = _parent.logo_file.Source.Height * scale;

                BitmapSource scaledLogo = ((BitmapSource)_parent.logo_file.Source).ResizedImage((int)sWidth, (int)sHeight);
                if (scaledLogo.PixelWidth > _frame.Width || scaledLogo.PixelHeight > _frame.Height)
                {
                    scaledLogo = scaledLogo.ScaleImage(_frame.Width, _frame.Height);
                }

                double pointX = (_frame.Width - scaledLogo.PixelWidth) / 2 + (_parent.logo_shift_w.Value ?? 0);
                double pointY = (_frame.Height - scaledLogo.PixelHeight) / 2 + (_parent.logo_shift_h.Value ?? 0);
                frame = frame.DrawImage(scaledLogo, new Point(pointX, pointY));
                titleFrame = titleFrame.DrawImage(scaledLogo, new Point(pointX, pointY));
            }

            if (_parent.banner_file.Source != null)
            {

                BitmapSource scaledBanner = ((BitmapSource)_parent.banner_file.Source).ScaleImage(_frame.Width, _frame.Height);

                titleFrame = titleFrame.DrawImage(scaledBanner, new Point(0, titleFrame.Height - scaledBanner.Height));
            }

            KeyImage.Source = frame;
            TitleImage.Source = titleFrame;
        }
    }
}
