using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;

namespace HPC_Chart.ViewModel
{
    internal class ChartViewModel : ObservableObject, INotifyPropertyChanged
    {
        private PlotModel plotModel;

        public ChartViewModel()
        {
            var myController = new PlotController();
            // 将控制器分配给PlotView
            PlotModelController = myController;
            // 绑定鼠标左键点击与平移命令
            myController.BindMouseDown(OxyMouseButton.Right, PlotCommands.PanAt);
            //myController.BindMouseDown(OxyMouseButton.Left, PlotCommands.ResetAt);

            // 绑定鼠标滚轮事件与缩放命令
            myController.BindMouseWheel(PlotCommands.ZoomWheel);

            BtnClickCommand = new RelayCommand<object>(BtnClick);
            PickerValue = DateTime.Now.ToString("yyyy-MM-dd HH:00:00");
            //PickerValue = "2023-09-20 16:00:00";

                        // 创建一个 PlotModel 对象
                        PlotModel = new PlotModel { Title = "HPC Chart" };

                        // 创建一个时间轴 X 轴
                        var xAxis = new DateTimeAxis
                        {
                            Position = AxisPosition.Bottom,
                            StringFormat = "HH:mm:ss", // 时间显示格式
                            Title = "时间"
                        };

                        // 添加 X 轴到 PlotModel
                        PlotModel.Axes.Add(xAxis);

                        // 创建一个数据点系列
                        var series = new LineSeries
                        {
                            Title = "数据",
                            Color = OxyColors.Blue
                        };
        }

        private void BtnClick(object parameter)
        {
            if (parameter is string tag)
            {
                switch (parameter.ToString())
                {
                    case "SearchTag":
                        if (ComboBoxSource.Contains(ComboBoxValue))
                        {
                            GetDataSource();
                        }
                        break;

                    default: break;
                }
            }
        }
        public void GetDataSource()
        {
            //"2023-10-05 12:27:09"
            
            string startTime, filePath, txtName;
            char[] separators = { ' ', ':', '-' };
            string[] timeItem;
            if (PickerValue == null) { PickerValue = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); }
            if (PickerValue.Split(separators)[3] == DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Split(separators)[3]) { return; }
            DateTime dateTime = DateTime.ParseExact(PickerValue, "yyyy-MM-dd HH:mm:ss", null); ;
            string customFormat = "yyyy-M-d H:m:s";

            startTime = dateTime.ToString(customFormat);
            timeItem = startTime.Split(separators);
            filePath = string.Format("@{0}_{1}_{2}", timeItem[0], timeItem[1], timeItem[2]);
            txtName = filePath + "_" + timeItem[3];
            string Path = string.Format(@"D:\WinPc1\Sys\Data\Data\{0}\{1}.log", filePath, txtName);
            try
            {
                using (StreamReader reader = new StreamReader(Path))
                {
                    dataModels.Clear();
                    string line;

                    // 逐行读取文件内容
                    //"2023/09/20 16:36:52,1.1875,0,0,0,-25,0,4.25,1.375,0,0,0,0.5,0,436.875,0"
                    // time ,ch1arm2 n2,ch1 bsn,ch1 bsr,ch1 arm2di,ch1 arm1fi,ch1arm3n2,ch1 pre,
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] dataItems = line.Split(',');
                        DataModel dataModel = new DataModel();
                        dataModel.dataTime = dataItems[0].Replace('/', '-');
                        dataModel.Ch1.Arm2N2 = float.Parse(dataItems[1]);
                        dataModel.Ch1.BSNN2 = float.Parse(dataItems[2]);
                        dataModel.Ch1.BSRDI = float.Parse(dataItems[3]);
                        dataModel.Ch1.Arm2DI = float.Parse(dataItems[4]);
                        dataModel.Ch1.Arm1DI = float.Parse(dataItems[5]);
                        dataModel.Ch1.Arm3N2 = float.Parse(dataItems[6]);
                        dataModel.Ch1.Arm1Press = float.Parse(dataItems[7]);

                        dataModel.Ch2.Arm2N2 = float.Parse(dataItems[8]);
                        dataModel.Ch2.BSNN2 = float.Parse(dataItems[9]);
                        dataModel.Ch2.BSRDI = float.Parse(dataItems[10]);
                        dataModel.Ch2.Arm2DI = float.Parse(dataItems[11]);
                        dataModel.Ch2.Arm1DI = float.Parse(dataItems[12]);
                        dataModel.Ch2.Arm3N2 = float.Parse(dataItems[13]);
                        dataModel.Ch2.Arm1Press = float.Parse(dataItems[14]);
                        dataModel.Ch1.eFlow = float.Parse(dataItems[15]);
                        dataModels.Add(dataModel);
                    }
                    //Console.WriteLine(dataModels.Count);
                    SetPlotChart(ComboBoxValue);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("File Error");
            }
        }

        public void SetPlotChart(string comboValue)
        {

            List<float> YSource = new List<float>();
            List<DateTime> XSoure = new List<DateTime>();
            foreach (DataModel data in dataModels)
            {
                DateTime dateTime;
                if (DateTime.TryParse(data.dataTime, out dateTime))
                {
                    XSoure.Add(dateTime);
                };
            }
            int indexValue = ComboBoxSource.IndexOf(comboValue);
            //获取单位

            YSource = GetListSoure(indexValue);
                // 创建一个 PlotModel 对象
                PlotModel = new PlotModel { Title = "HPC Chart" };
          
                // 创建一个时间轴 X 轴
                var xAxis = new DateTimeAxis
                {
                    Position = AxisPosition.Bottom,
                    StringFormat = "HH:mm:ss", // 时间显示格式
                    Title = "时间"
                };
          
                // 添加 X 轴到 PlotModel
                PlotModel.Axes.Add(xAxis);

            // 创建一个数据点系列
            var series = new LineSeries
            {
                Title = "数据",
                Color = OxyColors.Blue,
                TrackerFormatString =String.Format("时间: {{2:HH:mm:ss}}\n数据: {{4:0.0}} {0}", GetDataUnit[indexValue])


            };
            for (int sourceIndex = 0; sourceIndex < XSoure.Count; sourceIndex++)
            {
                series.Points.Add(new DataPoint(DateTimeAxis.ToDouble(XSoure[sourceIndex]), YSource[sourceIndex]));
            }
            // 将系列添加到 PlotModel 中
            PlotModel.Series.Add(series);
            PlotModel.InvalidatePlot(true);
        }

        private List<float> GetListSoure(int index)
        {
            List<float> YSource = new List<float>();
            switch (index)
            {
                case 0:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch1.Arm1Press);
                    }
                    break;

                case 1:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch1.Arm1DI);
                    }
                    break;

                case 2:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch1.Arm2DI);
                    }
                    break;

                case 3:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch1.Arm2N2);
                    }
                    break;

                case 4:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch1.Arm3N2);
                    }
                    break;

                case 5:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch1.BSRDI);
                    }
                    break;

                case 6:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch1.BSNN2);
                    }
                    break;

                case 7:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch2.Arm1Press);
                    }
                    break;

                case 8:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch2.Arm1DI);
                    }
                    break;

                case 9:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch2.Arm2DI);
                    }
                    break;

                case 10:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch2.Arm2N2);
                    }
                    break;

                case 11:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch2.Arm3N2);
                    }
                    break;

                case 12:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch2.BSRDI);
                    }
                    break;

                case 13:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch2.BSNN2);
                    }
                    break;

                case 14:
                    foreach (DataModel data in dataModels)
                    {
                        YSource.Add(data.Ch1.eFlow);
                    }
                    break;
            }
            return YSource;
        }

        public PlotModel PlotModel
        {
            get { return plotModel; }
            set { SetProperty(ref plotModel, value); }
        }
        private IPlotController plotModelController;

        public IPlotController PlotModelController
        {
            get { return plotModelController; }
            set
            {
                SetProperty(ref plotModelController, value);
            }
        }
        public string PickerValue { get; set; }

        public string ComboBoxValue { get; set; }

        public List<String> ComboBoxSource { get; } = new List<string>()
        {
            "Ch1 Pressure","Ch1Arm1 DI","Ch1Arm2 DI","Ch1Arm2 N2","Ch1Arm3 N2","Ch1 BSR DI","Ch1 BSN N2",
            "Ch2 Pressure","Ch2Arm1 DI","Ch2Arm2 DI","Ch2Arm2 N2","Ch2Arm3 N2","Ch2 BSN DI","Ch2 BSN N2","Eflow"
        };
        public List<String> GetDataUnit { get; } = new List<string>()
        {
            "Psi","L/Min","ML/Min","ML/Min","ML/Min","ML/Min","ML/Min",
            "Psi","L/Min","ML/Min","ML/Min","ML/Min","ML/Min","ML/Min","Ω"
        };

        public RelayCommand<object> BtnClickCommand { get; private set; }
        public ICommand LoadCommand { get; set; }
        public List<DataModel> dataModels { get; private set; } = new List<DataModel>();
    }

    internal class DataModel
    {
        public string dataTime { get; set; }
        public ChanmberModel Ch1 { get; set; } = new ChanmberModel();
        public ChanmberModel Ch2 { get; set; } = new ChanmberModel();
    }
    internal class ChanmberModel
    {
        public float Arm1Press { get; set; }
        public float Arm1DI { get; set; }
        public float Arm2DI { get; set; }
        public float Arm2N2 { get; set; }
        public float Arm3N2 { get; set; }
        public float BSRDI { get; set; }
        public float BSNN2 { get; set; }
        public float eFlow { get; set; }
    }
}