using HPC_Chart.ViewModel;
using OxyPlot;
using System.Threading;

namespace HPC_Chart
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            ChartViewModel viewModel = new ChartViewModel();
            DataContext = viewModel;
        }

    }
}