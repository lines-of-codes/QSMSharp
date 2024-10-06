using Microsoft.UI.Xaml.Controls;

namespace QSM.Windows;

internal class SymbolImage
{
    public Symbol? Symbol;
    public string ImagePath;

    public SymbolImage(Symbol symbol) => Symbol = symbol;
    public SymbolImage(string imagePath) => ImagePath = imagePath;
}
