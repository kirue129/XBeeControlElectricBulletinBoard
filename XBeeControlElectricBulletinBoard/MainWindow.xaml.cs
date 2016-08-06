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
        private bool _bFirstChengedCOMPort = false;
        private bool _bFirstChengedBaudRate = false;

        // シリアル通信クラスインスタンス生成
        SerialPort _serialPort = new SerialPort();

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
            m_btnStart.IsEnabled = true;                        // 通信開始ボタンを有効化
            m_btnStop.IsEnabled = false;                        // 通信停止ボタンを無効化
            m_scrDelay.Value = Constants.DEF_DELAY;             // 遅延の初期値設定
            m_scrBrightness.Value = Constants.DEF_BRIGHTNESS;   // 明るさの初期値設定

        }

        // COMPort変更時ComboBox内の選択後
        private void m_cbCOMPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 初回変更時
            if (!_bFirstChengedCOMPort)
            {
                _bFirstChengedCOMPort = true;
            }
        }

        // 転送速度変更ComboBox内の選択後
        private void m_cbBaudRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 初回変更時
            if (!_bFirstChengedBaudRate)
            {
                _bFirstChengedBaudRate = true;
            }
        }

        // シリアル通信開始
        public bool OpenSerialPort()
        {
            bool bRet = false;

            // ポートが閉じているか
            if (!_serialPort.IsOpen)
            {
                // ポート設定
                _serialPort.PortName = m_cbCOMPort.SelectedItem.ToString();                      // Port名を設定
                _serialPort.BaudRate = Convert.ToInt32(m_cbBaudRate.SelectedItem.ToString());    // 転送速度を設定
                _serialPort.Open();                                                              // ポート開放
                m_btnStart.IsEnabled = false;                                                    // 通信開始ボタンを無効化
                m_btnStop.IsEnabled = true;                                                      // 通信停止ボタンを有効化
                bRet = true;

            }
            return bRet;
        }


        // シリアル通信停止
        public bool CloseSerialPort()
        {
            bool bRet = false;

            // ポートが開いているか
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();                // ポートを閉じる
                m_btnStart.IsEnabled = true;        // 通信開始ボタンを有効化
                m_btnStop.IsEnabled = false;        // 通信停止ボタンを無効化
                bRet = true;
            }
            return bRet;
        }

        // シリアル通信開始Buttonクリック時
        private void m_btnStart_Click(object sender, RoutedEventArgs e)
        {
            String sResult = "失敗";

            // ポートと転送速度が設定されているか？
            if (_bFirstChengedCOMPort && _bFirstChengedBaudRate)
            {
                // 通信開始
                if (OpenSerialPort())
                {
                    sResult = "成功";
                }
            }
            AddListLog(GetStatus(), "開始", "通信開始", sResult, GetNowTime());       // ログ出力
        }

        // シリアル通信停止Buttonクリック時
        private void m_btnStop_Click(object sender, RoutedEventArgs e)
        {
            String sResult = "失敗";

            // ポートと転送速度が設定されているか？
            if (_bFirstChengedCOMPort && _bFirstChengedBaudRate)
            {
                // 通信終了
                if (CloseSerialPort())
                {
                    sResult = "成功";
                }
            }
            AddListLog(GetStatus(), "終了", "通信終了", sResult, GetNowTime());       // ログ出力
        }

        // 動作ログListViewにログを出力
        private void AddListLog(String sStatus, String sOperation, String sContent,String sResult, String sTime)
        {
            if (list != null)
            {
                list.Insert(0, new Member { Status = sStatus, Operation = sOperation, Content = sContent, Result = sResult, Time = sTime });
            }
        }

        // 現在の通信状況を取得
        private String GetStatus()
        {
            String sRet = "停止";

            // シリアル通信が行われているか？
            if (_serialPort.IsOpen)
            {
                sRet = "通信中";
            }

            return sRet;
            
        }

        // 現在時刻を取得
        private String GetNowTime()
        {
            return DateTime.Now.ToString("yyyy/MM/dd HH/mm/ss");       // 現在時刻を取得し，整形
        }

        // テキスト送信Buttonクリック時
        private void m_btnSend_Click(object sender, RoutedEventArgs e)
        {
            String sResult = "失敗";
            String buffer = m_tbOutputText.Text;                   // テキストを出力テキストTextBoxから取得

            // '$'が含まれてないか、かつシリアル通信が行われているか？
            // <注>'$'をArduino側でコマンド文字として扱っているため，'$'は受け付けない
            if (buffer.IndexOf('$') == -1&& _serialPort.IsOpen)
            {
                _serialPort.Write(buffer);      // テキスト(buffer)を転送
                sResult = "成功";
            }
            m_tbOutputText.Clear();                                             // 出力テキストTextBoxをクリア
            AddListLog(GetStatus(), "送信", buffer, sResult, GetNowTime());   　// ログ出力
        }

        // 電光掲示板画面クリアButtonクリック時
        private void m_btnClean_Click(object sender, RoutedEventArgs e)
        {
            String sResult = "失敗";

            // シリアル通信が行われているか？
            if (_serialPort.IsOpen)
            {
                String buffer = Constants.CMD_CLEAR;    // 電光掲示板画面をクリアするコマンドをバッファに入れとく
                _serialPort.Write(buffer);              // バッファの内容を転送
                sResult = "成功";
            }   
            AddListLog(GetStatus(), "クリア", "画面を初期化します", sResult, GetNowTime());        // ログ出力
        }

        // テストモード実行Buttonクリック時
        private void m_btnTest_Click(object sender, RoutedEventArgs e)
        {
            String sResult = "失敗";

            // シリアル通信が行われているか？
            if (_serialPort.IsOpen)
            {
                String buffer = Constants.CMD_TEST;     // 電光掲示板の表示をテストモードに変更するコマンドをバッファに入れとく
                _serialPort.Write(buffer);              // バッファの内容を転送
                sResult = "成功";
            }
            AddListLog(GetStatus(), "テスト", "テストモードを実行します", sResult, GetNowTime());      // ログ出力
        }

        // 現在時刻Buttonクリック時
        private void m_btnTime_Click(object sender, RoutedEventArgs e)
        {
            String sResult = "失敗";
            String time = GetNowTime();             // 現在時刻の取得

            // シリアル通信が行われているか？
            if (_serialPort.IsOpen)          
            {
                String buffer = Constants.CMD_TIME + time;      // 電光掲示板に現在時刻を表示させるコマンドをバッファに入れとく
                _serialPort.Write(buffer);                      // バッファの内容を転送
                sResult = "成功";
            }
            AddListLog(GetStatus(), "現在時間", "現在時刻を表示します", sResult, time);      // ログ出力
        }

        // 遅延間隔変更ScrollBarの値変更時
        private void m_scrDelay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            String sResult = "失敗";

            // シリアル通信が行われているか？
            if (_serialPort.IsOpen)
            {
                String buffer = Constants.CMD_DELAY + Convert.ToInt32(m_scrDelay.ToString());          // 電光掲示板のスピードを変更するコマンドをバッファに入れとく
                _serialPort.Write(buffer);                                                             // バッファの内容を転送
                sResult = "成功";
            }
            AddListLog(GetStatus(), "遅延時間", "遅延時間を変更します", sResult, GetNowTime());      // ログ出力
        }

        // 明るさ変更ScrollBarの値変更時
        private void m_scrBrightness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            String sResult = "失敗";

            // シリアル通信が行われているか？
            if (_serialPort.IsOpen)
            {
                String buffer = Constants.CMD_BRIGHTNESS + Convert.ToInt32(m_scrBrightness.Value);      // 電光掲示板の明るさを変更するコマンドをバッファに入れとく
                _serialPort.Write(buffer);                                                              // バッファの内容を転送
                sResult = "成功";
            }
            AddListLog(GetStatus(), "明るさ", "明るさを変更します", sResult, GetNowTime());           // ログ出力
        }

        // リセットButtonクリック時
        private void m_btnReset_Click(object sender, RoutedEventArgs e)
        {
           m_scrDelay.Value = Constants.DEF_DELAY;                  // 遅延間隔を初期値に変更
           m_scrBrightness.Value = Constants.DEF_BRIGHTNESS;        // 明るさを初期値に変更
        }
    }

    // 定義集
    public static class Constants
    {
        // Definne
        public const string DEF_COMPORT = "COM3";               // COMPort
        public const int DEF_TRANSFARRATE = 9600;               // 転送速度
        public const int DEF_DELAY = 100;                       // 遅延
        public const int DEF_BRIGHTNESS = 5;                    // 明るさ
        
        // コマンド
        public const String CMD_CLEAR = "$";                    // 画面クリア
        public const String CMD_TEST = "$test";                 // テストモード
        public const String CMD_TIME = "$time";                 // 現在時刻表示
        public const String CMD_DELAY = "$speed";               // 遅延間隔
        public const String CMD_BRIGHTNESS = "$intensity";      // 明るさ
    }
}
