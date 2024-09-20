using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Lagrange.Core.Message.Entity;
using PropertyChanged;

namespace Meow.UI.Views.MessageChainViews;

[AddINotifyPropertyChangedInterface]
public partial class ImageEntityView : UserControl
{
    public ImageEntityView(ImageEntity imageEntity)
    {
        InitializeComponent();
        LoadImage(imageEntity);
    }

    private void LoadImage(ImageEntity imageEntity)
    {
        var bitmap = new BitmapImage();

        bitmap.BeginInit();
        bitmap.UriSource = new Uri(imageEntity.ImageUrl, UriKind.Absolute);
        bitmap.EndInit();

        NetworkImage.Source = bitmap;
    }

    private void ImageShowIcon_OnClick(object sender, RoutedEventArgs e)
    {
        Popup.IsOpen = !Popup.IsOpen;
    }
}