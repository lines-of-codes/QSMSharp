﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace QSM.Windows;

internal partial class NullVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		if (value == null) return Visibility.Collapsed;
		if (value is string && string.IsNullOrWhiteSpace((string)value)) return Visibility.Collapsed;
		return Visibility.Visible;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotImplementedException();
	}
}
