using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ActUtlType64Lib;

namespace WPFMxComponent
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ActUtlType64 mxComponent;
        bool isConnected = false;
        Stopwatch sw = new Stopwatch();

        public MainWindow()
        {
            // 종료시 이벤트 메서드 등록
            this.Closing += Window_Closing;

            InitializeComponent();
        }

        /// <summary>
        /// Open 버튼을 클릭하면, PLC 시뮬레이터와 연동된다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOpenClkEvent(object sender, RoutedEventArgs e)
        {
            mxComponent = new ActUtlType64();

            int iRet = mxComponent.Open();

            if (iRet == 0)
            {
                isConnected = true;
                MessageBox.Show("성공적으로 연결되었습니다.");
            }
            else
            {
                isConnected = false;
                MessageBox.Show(iRet.ToString("X"));
            }
        }

        private void OnCloseClkEvent(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Close()
        {
            if (isConnected)
            {
                int iRet = mxComponent.Close();

                if (iRet == 0)
                {
                    isConnected = false;
                    MessageBox.Show("성공적으로 해지되었습니다.");
                }
                else
                {
                    MessageBox.Show(iRet.ToString("X"));
                }
            }
        }

        // 파괴자(Decontructor)
        ~MainWindow()
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Close();
        }

        private void OnGetDeviceClkEvent(object sender, RoutedEventArgs e)
        {
            if (!isConnected) return;

            string input = deviceInput.Text;

            int output = 0;
            int iRet = mxComponent.GetDevice(input, out output);
        
            if(iRet == 0)
            {
                outputLabel.Content = input + "의 값은 " + output.ToString();
            }
            else
            {
                MessageBox.Show("잘못 입력하셨습니다.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnSetDeviceClkEvent(object sender, RoutedEventArgs e)
        {
            if (!isConnected) return;

            sw.Reset();
            sw.Start();

            string input = deviceInput.Text;
            string value = valueInput.Text;

            int convertedValue = 0;
            bool isConverted = int.TryParse(value, out convertedValue);

            if(!isConverted)
            {
                MessageBox.Show("Value에 1 또는 0을 입력해 주세요.", "Error");

                return;
            }

            int iRet = mxComponent.SetDevice(input, convertedValue);
            if (iRet == 0)
            {
                sw.Stop();

                outputLabel.Content = $"{input}에 {convertedValue}가 잘 입력되었습니다." +
                    $" {sw.ElapsedMilliseconds}ms";
            }
            else
            {
                MessageBox.Show(iRet.ToString("X"), "Error");
            }
        }

        private void OnReadDeviceBlockClkEvent(object sender, RoutedEventArgs e)
        {
            if (!isConnected) return;

            // X0 부터, 1개 블록
            int blockNum = 0;
            bool isConverted = int.TryParse(blockNumber.Text, out blockNum);

            if(!isConverted)
            {
                MessageBox.Show("블록의 개수를 정수형 양수로 입력해 주세요.", "Error");
                return;
            }

            int[] value = new int[blockNum]; // { 0, 0, 0, 0, 0 }
            // out 키워드 뒤에는 블록의 값이 들어갈 배열의 첫번째 인덱스를 입력해야함.
            int iRet = mxComponent.ReadDeviceBlock(StartDevice.Text, blockNum, out value[0]);
            if (iRet == 0)
            {
                string valueStr = "";
                foreach(int v in value)
                {
                    valueStr += v.ToString() + ",";
                }

                outputLabel.Content = $"{StartDevice.Text} 부터 {blockNum}개 블록의 값은 {valueStr}입니다.";
            }
            else
            {
                MessageBox.Show(iRet.ToString("X"), "Error");
            }
        }

        private void OnWriteDeviceBlockClkEvent(object sender, RoutedEventArgs e)
        {
            if (!isConnected) return;

            // X0 부터, 1개 블록
            int blockNum = 0;
            bool isConverted = int.TryParse(blockNumber.Text, out blockNum);

            if (!isConverted)
            {
                MessageBox.Show("블록의 개수를 정수형 양수로 입력해 주세요.", "Error");
                return;
            }

            int[] value = new int[blockNum];
            string[] valueArr = blockValue.Text.Split(","); // 55,1000,500 -> { "55","1000","500"}

            if(blockNum != valueArr.Length)
            {
                MessageBox.Show("입력된 블록의 개수와 입력한 값의 개수가 다릅니다.");
                return;
            }

            // { 55, 1000, 500 }
            int[] nums = Array.ConvertAll(valueArr, int.Parse);
            int iRet = mxComponent.WriteDeviceBlock(StartDevice.Text, blockNum, ref nums[0]);
            if (iRet == 0)
            {
                outputLabel.Content = $"{blockValue.Text} 가 {StartDevice.Text}부터" +
                    $" {blockNum}개의 블록에 적용되었습니다.";
            }
            else
            {
                MessageBox.Show(iRet.ToString("X"), "Error");
            }
        }

        /// <summary>
        /// 읽기를 원하는 디바이스들의 정보를 입력받아, outputLabel에 출력
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReadDeviceRandomClkEvent(object sender, RoutedEventArgs e)
        {
            if (!isConnected) return;

            string deviceNames = Devices.Text; // "D0,X6,Y20"
            string[] deviceNamesArr = deviceNames.Split(",");
            string totalDeviceName = "";
            for (int i = 0; i < deviceNamesArr.Length; i++) // D0\nX6\nY20
            {
                totalDeviceName += deviceNamesArr[i];

                if (i < deviceNamesArr.Length - 1)
                {
                    totalDeviceName += "\n";
                }
            }

            int[] values = new int[deviceNamesArr.Length];

            int iRet = mxComponent.ReadDeviceRandom(totalDeviceName, deviceNamesArr.Length, out values[0]);
            if (iRet == 0)
            {
                string result = "";

                foreach(int value in values)
                {
                    result += value.ToString() + ",";
                }

                MessageBox.Show($"{Devices.Text}의 값은 {result}입니다.");
            }
            else
            {
                MessageBox.Show(iRet.ToString("X"), "Error");
            }
        }

        /// <summary>
        /// Device들의 이름과 매칭되는 값들을 MxComponent의 WriteDeviceRandom으로 전달
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWriteDeviceRandomClkEvent(object sender, RoutedEventArgs e)
        {
            // 1. D0,X6,Y20 -> D0\nX6\nY20
            if (!isConnected) return;

            string deviceNames = Devices.Text; // "D0,X6,Y20"
            string[] deviceNamesArr = deviceNames.Split(",");
            string totalDeviceName = "";
            for (int i = 0; i < deviceNamesArr.Length; i++) // D0\nX6\nY20
            {
                totalDeviceName += deviceNamesArr[i];

                if (i < deviceNamesArr.Length - 1)
                {
                    totalDeviceName += "\n";
                }
            }

            // 2. 값들 int[]로 변환
            string valueRandStr = valueRandom.Text;
            string[] numbers = valueRandStr.Split(","); // {"300", "500", "20"}
            
            int[] values = new int[deviceNamesArr.Length];
            values = Array.ConvertAll(numbers, int.Parse);

            // 3. WriteDeviceRandom 예외처리와 함께 작성
            int iRet = mxComponent.WriteDeviceRandom(totalDeviceName, deviceNamesArr.Length, ref values[0]);

            if (iRet == 0)
            {
                outputLabel.Content = $"{Devices.Text}의 값이 {valueRandom.Text}로 변경이 완료되었습니다.";
            }
            else
            {
                MessageBox.Show(iRet.ToString("X"), "Error");
            }
        }
    }
}