using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SQLite;
using System.Windows.Threading;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;

namespace UAV_Info
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        private DispatcherTimer Timer1 = new DispatcherTimer();
        public Window1()
        {
            InitializeComponent();
            Init_Timer();
            MapLoad();
            Timer1.Start();
        }
        #region Timer定时器
        private void Init_Timer()
        {
            Timer1.Interval = new TimeSpan(0, 0, 0, 0, 500);            //500ms刷新一次
            Timer1.Tick += new EventHandler(Timer1_Tick);

        }
        private void Timer1_Tick(object sender, EventArgs e)
        {
            double zoom = mapControl.Zoom;
            PointLatLng tcenter = mapControl.Position;
            mapControl.Markers.Clear();
            MapLoad(zoom);
            mapControl.Position = tcenter;
        }
        #endregion

        #region 窗口调整
        private void Windows_Move(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                try
                {
                    DragMove();
                }
                catch { }
            }
        }

        private void Min_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        Boolean flag_size = false;
        private void Change_Click(object sender, RoutedEventArgs e)
        {
            flag_size = !flag_size;
            if(flag_size == false)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion
        #region 窗口跳转
        private void W2_Button_Click(object sender, RoutedEventArgs e)
        {
            Window2 windowpage = new Window2();
            windowpage.Show();
            Timer1.Stop();
            this.Close();
        }

        private void W3_Button_Click(object sender, RoutedEventArgs e)
        {
            Window3 windowpage = new Window3(GetConfigDefaultId().ToString());
            windowpage.Show();
            Timer1.Stop();
            this.Close();
        }
        //获取Uconfig表中最小ID
        private int GetConfigDefaultId()
        {
            SQLiteConnection con = null;
            int min_id = 10000;
            try
            {
                con = new SQLiteConnection("data source=" + "UAV-System.db");
                con.Open();
                SQLiteCommand cmd = con.CreateCommand();
                cmd.CommandText = "select * from Uconfig";
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if(int.Parse(reader[0].ToString()) < min_id)
                    {
                        min_id = int.Parse(reader[0].ToString());
                    }
                }
                reader.Close();
            }
            finally
            {
                if(con != null)
                {
                    con.Close();
                }
            }
            return min_id;
        }
        private void W4_Button_Click(object sender, RoutedEventArgs e)
        {            
            Window4 windowpage = new Window4(GetHudDefaultId().ToString());
            windowpage.Show();
            Timer1.Stop();
            this.Close();
        }
        //获取Unode表中最小ID
        private int GetHudDefaultId()
        {
            int min_id = 10000;
            foreach(UAV_Position el in UAVs_Pos)
            {
                if(el.Id < min_id)
                {
                    min_id = el.Id;
                }
            }
            return min_id;
        }
        private void W5_Button_Click(object sender, RoutedEventArgs e)
        {
            Window5 windowpage = new Window5();
            windowpage.Show();
            Timer1.Stop();
            this.Close();
        }

        private void Net_Button_Click(object sender, RoutedEventArgs e)
        {
            Window2 windowpage = new Window2();
            windowpage.Show();
            Timer1.Stop();
            this.Close();
        }
        #endregion
        #region 数据操作
        //无人机位置类
        private class UAV_Position
        {
            public int Id;
            public string Devname;
            public double Latitude;
            public double Longitude;
        }

        private List<UAV_Position> UAVs_Pos = new List<UAV_Position>();
        //读取数据库
        private void GetPositionTopo()
        {
            UAVs_Pos.Clear();
            SQLiteConnection cn = null;
            try
            {
                string path = "UAV-System.db";
                //创建sqlite连接
                cn = new SQLiteConnection("data source=" + path);
                if(cn.State != System.Data.ConnectionState.Open)
                {
                    cn.Open();
                    SQLiteCommand cmd = cn.CreateCommand();
                    cmd.CommandText = "select * from Unode";
                    SQLiteDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        UAV_Position entity = new UAV_Position();
                        for(int i = 0; i < 3; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    entity.Id = int.Parse(reader[i].ToString());
                                    break;
                                case 1:
                                    entity.Latitude = double.Parse(reader[i].ToString());
                                    break;
                                case 2:
                                    entity.Longitude = double.Parse(reader[i].ToString());
                                    break;
                            }
                        }
                        entity.Devname = "UAV" + entity.Id.ToString();
                        UAVs_Pos.Add(entity);
                    }
                    reader.Close();
                }
            }
            finally
            {
                if(cn != null)
                {
                    cn.Close();
                }
            }            
        }
        #endregion

        #region Map
        //加载地图
        private void MapLoad(double zm = 16)
        {
            GetPositionTopo();
            mapControl.MapProvider = Map.AMapProvider.Instance;
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            mapControl.Zoom = zm;
            mapControl.ShowCenter = false; //不显示中心十字点
            mapControl.DragButton = MouseButton.Left; //左键拖拽地图
            mapControl.Position = GetCenter();
            mapControl.MouseRightButtonDown += new MouseButtonEventHandler(Map_MouseDown);
            AddMarkers(UAVs_Pos);            //添加无人机
        }
        //获取地图中心点
        public PointLatLng GetCenter()
        {
            double[] lat = new double[UAVs_Pos.Count];
            double[] lon = new double[UAVs_Pos.Count];
            int i = 0, j = 0;
            foreach (UAV_Position original in UAVs_Pos)
            {
                lat[i++] = Convert.ToDouble(original.Latitude);
                lon[j++] = Convert.ToDouble(original.Longitude);
            }
            PointLatLng result = new PointLatLng();
            try
            {
                double max_lat = lat.Max();
                double min_lat = lat.Min();
                double mid_lat = (max_lat + min_lat) / 2;
                double max_lon = lon.Max();
                double min_lon = lon.Min();
                double mid_lon = (max_lon + min_lon) / 2;
                result = new PointLatLng(mid_lat, mid_lon);
            }
            catch (Exception)
            {
                result = new PointLatLng(30.527958961746585, 114.36127855405148);
            }

            return result;
        }


        //点击无人机
        private void Map_MouseDown(object sender, MouseEventArgs e)
        {
            Point pt = e.GetPosition(mapControl);
            PointLatLng point = mapControl.FromLocalToLatLng((int)pt.X, (int)pt.Y);

            PointHitTestParameters parameters = new PointHitTestParameters(pt);
            VisualTreeHelper.HitTest(mapControl, null, HitTestCallback, parameters);
        }
        GMapMarker _currentElement;
        private HitTestResultBehavior HitTestCallback(HitTestResult result)
        {
            Image image = result.VisualHit as Image;
            if (image != null)
            {
                _currentElement = image.Tag as GMapMarker;
                Window3 windowpage = new Window3(_currentElement.Tag.ToString());
                windowpage.Show();
                Timer1.Stop();
                this.Close();
                return HitTestResultBehavior.Stop;
            }
            return HitTestResultBehavior.Continue;
        }

        //添加无人机
        private void AddMarkers(List<UAV_Position> uAV_s)
        {
            foreach (UAV_Position uav_position in uAV_s)
            {
                PointLatLng pt = new PointLatLng(uav_position.Latitude, uav_position.Longitude);
                GMapMarker marker = new GMapMarker(pt);
                marker.Shape = CreatePinImage(marker);
                marker.Tag = uav_position.Id;
                mapControl.Markers.Add(marker);
            }
        }
        BitmapImage _pinSrcImage;
        Image CreatePinImage(GMapMarker marker)
        {
            Image img = new Image();
            img.Tag = marker;
            img.Width = 36;
            img.Height = 36;

            if (_pinSrcImage == null)
            {
                //多个标注共用一个图像源，节省内存。
                _pinSrcImage = new BitmapImage(new Uri("uav.png", UriKind.Relative));
                _pinSrcImage.Freeze();
            }
            img.Source = _pinSrcImage;
            //鼠标热点位置
            marker.Offset = new Point(-img.Width / 2, -img.Height / 2);
            return img;
        }
        #endregion
    }
}
