using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace QSM.Windows;

public class ServerStatsModel
{
    // skipcq: CS-W1096
    public ISeries[] Series { get; set; } = [
        new LineSeries<double>
        {
            Values = [ 20, 20, 19, 16, 20, 20, 20 ],
            Fill = null,
            Name = "TPS"
        },
        new LineSeries<double>
        {
            Values = [ 5, 7, 10, 30, 5, 5, 5 ],
            Fill = null,
            Name = "MSPT"
        }
    ];
}
