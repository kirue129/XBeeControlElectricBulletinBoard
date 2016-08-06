using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace XBeeControlElectricBulletinBoard
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool bFirstChengedCOMPort = false;
        private bool bFirstChengedBaudRate = false;

        // シリアル通信クラスインスタンス生成
        SerialPort sp = new SerialPort();

        ObservableCollection<Member> list;

        public MainWindow()
        {
            InitializeComponent();

            list = new ObservableCollection<Member>();
            this.m_listLog.DataContext = list;
        }

        // アプリ起動時の初期化
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            // PCで使用されているCOMPortをすべて取得
            string[] sCOMPort = SerialPort.GetPortNames();
            // Arduinoのデフォルト転送速度      
            int[] nBaudRate = { 300, 1200, 2400, 4800, 9600, 14400, 19200, 28800, 38400, 57600, 115200 };   

            // COMPortの項目追加
            foreach (string COMPort in sCOMPort)
            {
                m_cbCOMPort.Items.Add(COMPort);
            }

            //転送速度の項目追加
            foreach (int BaudRate in nBaudRate)
            {
                m_cbBaudRate.Items.Add(BaudRate);
            }

            // オブジェクトの設定
            m_btnStart.IsEnabled = true;        // 通信開始ボタンを有効化
            m_btnStop.IsEnabled = false;        // 通信停止ボタンを無効化

            


        }

        // COMPort変更時
        private void m_cbCOMPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 初回変更時
            if (!bFirstChengedCOMPort)
            {
                bFirstChengedCOMPort = true;
            }
        }

        private void m_cbBaudRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 初回変更時
            if (!bFirstChengedBaudRate)
            {
                bFirstChengedBaudRate = true;
            }
        }

        public void OpenSerialPort()
        {
            // ポートが閉じているか
            if (!sp.IsOpen)
            {
                // ポート設定
                sp.PortName = m_cbCOMPort.SelectedItem.ToString();                      // Port名を設定
                sp.BaudRate = Convert.ToInt32(m_cbBaudRate.SelectedItem.ToString());    // 転送速度を設定
                sp.Open();                                                              // ポート開放
                m_btnStart.IsEnabled = false;                                           // 通信開始ボタンを無効化
                m_btnStop.IsEnabled = true;                                             // 通信停止ボタンを有効化

            }
        }

        public void CloseSerialPort()
        {
            // ポートが開いているか
            if (sp.IsOpen)
            {
                sp.Close();                         // ポートを閉じる
                m_btnStart.IsEnabled = true;        // 通信開始ボタンを有効化
                m_btnStop.IsEnabled = false;        // 通信停止ボタンを無効化
            }
        }

        private void m_btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (bFirstChengedCOMPort && bFirstChengedBaudRate)
            {
                // 通信開始
                OpenSerialPort();
            }
            AddListLog(GetStatus(), "開始", "通信開始", GetNowTime());
        }

        private void m_btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (bFirstChengedCOMPort && bFirstChengedBaudRate)
            {
                // 通信終了
                CloseSerialPort();
            }
            AddListLog(GetStatus(), "終了", "通信終了", GetNowTime());
        }

        private void AddListLog(String sStatus, String sOperation, String sContent, String sTime)
        {
            String[] item = { sStatus, sOperation, sContent, sTime };
            list.Add(new Member { Status = sStatus, Operation = sOperation, Content = sContent, Time = sTime });
        }

        private String GetStatus()
        {
            String sRet = "停止";

            if (sp.IsOpen)
            {
                sRet = "通信中";
            }

            return sRet;
            
        }

        private String GetNowTime()
        {
            return DateTime.Now.ToString("yyyy/MM/dd HH/mm/ss");
        }

        private void m_btnSend_Click(object sender, RoutedEventArgs e)
        {
            String buffer = m_tbOutputText.ToString();
            if(buffer.IndexOf('$') == -1)
            {
                sp.Write(buffer);
            }
            m_tbOutputText.Clear();
            AddListLog(GetStatus(), "送信", buffer, GetNowTime());
        }

        private void m_btnClean_Click(object sender, RoutedEventArgs e)
        {
            String buffer = "$";
            sp.Write(buffer);
            AddListLog(GetStatus(), "クリア", "画面を初期化しました", GetNowTime());
        }

        private void m_btnTest_Click(object sender, RoutedEventArgs e)
        {
            String buffer = "$Test";
            sp.Write(buffer);
            AddListLog(GetStatus(), "テスト", "テストモードを実行します", GetNowTime());
        }

        private void m_btnTime_Click(object sender, RoutedEventArgs e)
        {
            String time = GetNowTime();
            String buffer = "$Time" + time;
            sp.Write(buffer);
            AddListLog(GetStatus(), "現在時間", "現在時刻を表示しました", time);
        }

        private void m_scrDelay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            String buffer = "$speed" + Convert.ToInt32(m_scrDelay.ToString());
            sp.Write(buffer);
            AddListLog(GetStatus(), "遅延時間", "遅延時間を変更しました", GetNowTime());
        }

        private void m_scrBrightness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            String buffer = "$intensity" + Convert.ToInt32(m_scrBrightness.ToString());
            sp.Write(buffer);
            AddListLog(GetStatus(), "明るさ", "明るさを変更しました", GetNowTime());
        }

        private void m_btnReset_Click(object sender, RoutedEventArgs e)
        {
            //m_scrDelay.PointFromScreen(50);
            //m_scrBrightness.PointFromScreen(50);
        }
    }

    public static class Constants
    {
        public const string DEF_COMPORT = "COM3";
        public const int DEF_TRANSFARRATE = 9600;
        public const int DEF_DELAY = 100;
        public const int DEF_BRIGHTNESS = 5;
    }

}
