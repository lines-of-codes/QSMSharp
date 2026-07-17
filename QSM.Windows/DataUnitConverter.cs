using Microsoft.UI.Xaml.Data;
using QSM.Windows.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace QSM.Windows;

internal partial class DataUnitConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		return SizeUnitConversion.BytesToAppropriateUnit((long)value);
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		return null;
	}
}
