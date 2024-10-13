using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;

namespace QSM.Windows;

internal class SymbolImageSourceConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		SymbolImage symbolImage = (SymbolImage)value;

		if (symbolImage.Symbol != null)
			return new SymbolIconSource() { Symbol = (Symbol)symbolImage.Symbol };
		if (symbolImage.ImagePath.EndsWith(".svg"))
			return new ImageIconSource() { ImageSource = new SvgImageSource(new Uri(symbolImage.ImagePath)) };
		if (!string.IsNullOrEmpty(symbolImage.ImagePath))
			return new BitmapIconSource() { UriSource = new Uri(symbolImage.ImagePath), ShowAsMonochrome = false };

		return null;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotImplementedException();
	}
}
