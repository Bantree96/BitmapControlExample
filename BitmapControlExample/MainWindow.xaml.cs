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
using System.Windows.Navigation;
using System.Windows.Shapes;
// OpenFileDialog 64 비트 사용
using System.Windows.Forms;


namespace BitmapControlExample
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _mainViewModel;
        public MainWindow(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            InitializeComponent();
            this.DataContext = _mainViewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // 파일 확장자 필터 설정
            openFileDialog.DefaultExt = ".bmp";
            openFileDialog.Filter = "이미지|*.bmp|모든파일|*.*";

            // Show open file dialog box
            DialogResult result = openFileDialog.ShowDialog();

            // Process open file dialog box results
            _mainViewModel.Imageopen(openFileDialog.FileName);
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _mainViewModel.ImageToGray();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _mainViewModel.ArrayToImage(500, 500);
        }
    }
}
