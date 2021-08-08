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

namespace UAV_Info
{
    /// <summary>
    /// Window2.xaml 的交互逻辑
    /// </summary>
    public partial class Window2 : Window
    {
        private DispatcherTimer Timer2 = new DispatcherTimer();
        public Window2()
        {
            InitializeComponent();
            Link_State_Load();
            Init_Timer();
            Timer2.Start();
        }

        #region Timer定时器
        private void Init_Timer()
        {
            Timer2.Interval = new TimeSpan(0, 0, 0, 0, 500);            //500ms刷新一次
            Timer2.Tick += new EventHandler(Timer2_Tick);

        }
        private void Timer2_Tick(object sender, EventArgs e)
        {
            canvas.Children.Clear();
            Link_State_Load();
        }
        #endregion

        #region 窗口调整
        private void Windows_Move(object sender, MouseEventArgs e)      //移动窗口
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try
                {
                    DragMove();
                }
                catch { }
            }
        }

        private void Min_Click(object sender, RoutedEventArgs e)        //最小化
        {
            WindowState = WindowState.Minimized;
        }
        Boolean flag_size = false;
        private void Change_Click(object sender, RoutedEventArgs e)     //最大化
        {
            flag_size = !flag_size;
            if (flag_size == false)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)      //关闭
        {
            Close();
        }
        #endregion

        #region 窗口跳转
        private void W1_Button_Click(object sender, RoutedEventArgs e)
        {
            Window1 windowpage = new Window1();
            windowpage.Show();
            Timer2.Stop();
            this.Close();
        }

        private void W3_Button_Click(object sender, RoutedEventArgs e)
        {
            Window3 windowpage = new Window3(GetConfigDefaultId().ToString());
            windowpage.Show();
            Timer2.Stop();
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
                    if (int.Parse(reader[0].ToString()) < min_id)
                    {
                        min_id = int.Parse(reader[0].ToString());
                    }
                }
                reader.Close();
            }
            finally
            {
                if (con != null)
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
            Timer2.Stop();
            this.Close();
        }

        //获取Unode表中最小ID
        private int GetHudDefaultId()
        {
            SQLiteConnection con = null;
            int min_id = 10000;
            try
            {
                con = new SQLiteConnection("data source=" + "UAV-System.db");
                con.Open();
                SQLiteCommand cmd = con.CreateCommand();
                cmd.CommandText = "select * from Unode";
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (int.Parse(reader[0].ToString()) < min_id)
                    {
                        min_id = int.Parse(reader[0].ToString());
                    }
                }
                reader.Close();
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                }
            }
            return min_id;
        }
        private void W5_Button_Click(object sender, RoutedEventArgs e)
        {
            Window5 windowpage = new Window5();
            windowpage.Show();
            Timer2.Stop();
            this.Close();
        }

        private void UAV_Button_Click(object sender, RoutedEventArgs e)
        {
            string tag = (sender as Button).Tag.ToString();
            Window3 windowpage = new Window3(tag);
            windowpage.Show();
            Timer2.Stop();
            this.Close();
        }

        private void Map_Button_Click(object sender, RoutedEventArgs e)
        {
            Window1 windowpage = new Window1();
            windowpage.Show();
            Timer2.Stop();
            this.Close();
        }
        #endregion

        #region Class
        public class UAV_Link
        {
            public Point SPoint { set; get; }
            public Point EPoint { set; get; }
            public int Quality { set; get; }
        }

        public class TransFormed_UAV
        {
            public int Id { set; get; }
            public string Devname { set; get; }
            public int X { set; get; }
            public int Y { set; get; }
        }


        #endregion

        #region DataBase
        public void SetNetPostion(List<TransFormed_UAV> trans)
        {            
            SQLiteConnection cn = null;
            try
            {
                string path = "UAV-System.db";
                //创建一个sqllite连接
                cn = new SQLiteConnection("data source=" + path);
                if (cn.State != System.Data.ConnectionState.Open)
                {
                    cn.Open();
                    SQLiteCommand cmd = cn.CreateCommand();
                    cmd.CommandText = "select * from Uconfig";
                    SQLiteDataReader reader = cmd.ExecuteReader();
                    int i = 0;
                    while (reader.Read())
                    {
                        TransFormed_UAV tfu = new TransFormed_UAV();
                        tfu.Id = int.Parse(reader[0].ToString());
                        tfu.Devname = "UAV" + tfu.Id.ToString();
                        int angel = (i % 2 == 0) ? 180 - i * 10 : -(i - 1) * 10;
                        tfu.X = 400 + (int)(300 * Math.Cos(angel * Math.PI / 180));
                        tfu.Y = 400 + (int)(300 * Math.Sin(angel * Math.PI / 180));
                        trans.Add(tfu);
                        i++;
                    }
                    reader.Close();
                }
            }
            finally
            {
                if (cn != null)
                    cn.Close();
            }
        }

        public void GetLinkTopo(List<TransFormed_UAV> UavsP, List<UAV_Link> UavsL)
        {            
            SQLiteConnection cn = null;
            try
            {
                string path = "UAV-System.db";
                //创建一个sqllite连接
                cn = new SQLiteConnection("data source=" + path);
                if (cn.State != System.Data.ConnectionState.Open)
                {
                    cn.Open();
                    SQLiteCommand cmd = cn.CreateCommand();
                    cmd.CommandText = "select * from Ulink";
                    SQLiteDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        UAV_Link entity = new UAV_Link();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    int ids = int.Parse(reader[i].ToString());
                                    foreach (TransFormed_UAV UavP in UavsP)
                                    {
                                        if (UavP.Id == ids)
                                            entity.SPoint = new Point(UavP.X, UavP.Y);
                                    }
                                    break;
                                case 1:
                                    int idt = int.Parse(reader[i].ToString());
                                    foreach (TransFormed_UAV UavP2 in UavsP)
                                    {
                                        if (UavP2.Id == idt)
                                            entity.EPoint = new Point(UavP2.X, UavP2.Y);
                                    }
                                    break;
                                case 2:
                                    entity.Quality = int.Parse(reader[i].ToString());
                                    break;
                            }
                        }
                        UavsL.Add(entity);
                    }
                    reader.Close();                    
                }
            }
            finally
            {
                if (cn != null)
                    cn.Close();
            }
        }
        #endregion


        #region Load
        private void Link_State_Load()
        {
            List<UAV_Link> UAVs_Lin = new List<UAV_Link>();
            List<TransFormed_UAV> UAVs_Tra = new List<TransFormed_UAV>();
            SetNetPostion(UAVs_Tra);
            Add_UAV(UAVs_Tra);
            GetLinkTopo(UAVs_Tra, UAVs_Lin);
            Add_Lin(UAVs_Lin);
        }

        //添加无人机
        public void Add_UAV(List<TransFormed_UAV> UavsPos)
        {
            foreach (TransFormed_UAV pos in UavsPos)
            {
                Button btn = new Button();
                btn.Style = this.FindResource("UavBtn") as Style;
                btn.ToolTip = pos.Devname;
                btn.Tag = pos.Id;
                Canvas.SetLeft(btn, pos.X - 20);
                Canvas.SetTop(btn, pos.Y - 20);
                canvas.Children.Add(btn);
            }            
        }

        public void Add_Lin(List<UAV_Link> UavsL)
        {
            foreach (UAV_Link link in UavsL)
            {                
                Point sp = link.SPoint;
                Point ep = link.EPoint;
                double Cx = sp.X + (ep.X - sp.X) / 4;
                int ConX = Convert.ToInt32(Cx);
                double Cy = sp.Y + (ep.Y - sp.Y) / 2;
                int ConY = Convert.ToInt32(Cy);
                Point cp = new Point(ConX, ConY);
                var v1 = new Vector(1, 0);
                var v2 = new Vector(3, 1);
                arrow.ArrowQuadraticBezierWithText Aqbw = new arrow.ArrowQuadraticBezierWithText();
                Aqbw.StartPoint = sp;
                Aqbw.EndPoint = ep;
                Aqbw.ControlPoint = cp;
                Aqbw.ArrowLength = 18;
                Aqbw.ArrowAngle = Vector.AngleBetween(v1, v2);
                if (link.Quality >= 30)
                    Aqbw.Stroke = Brushes.Green;
                else if (link.Quality >= 15)
                    Aqbw.Stroke = Brushes.Orange;
                else
                    Aqbw.Stroke = Brushes.Red;
                Aqbw.IsTextUp = true;
                Aqbw.StrokeThickness = 2;
                Aqbw.Text = link.Quality.ToString() + "dBm";
                canvas.Children.Add(Aqbw);
            }
        }
        #endregion
    }
}
