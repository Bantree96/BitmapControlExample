using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Controls;
// 이미지에 대한 기본적인 작업을 가능하게 해주는 클래스
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace BitmapControlExample
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Fields
        private BitmapImage _image;
        private Bitmap _bitmap;
        private byte[] _imageArray;
        #endregion

        #region Properties
        public BitmapImage ImageSource 
        { 
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged("ImageSource");
            }
        }
        #endregion

        #region Event
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Method
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                // this는 명시해준것.
                // ? : Nullable
                // .Invoke : 메인쓰레드가 아니라도 View를 손 댈수 있게 권한을 줌
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        internal void ImageToGray()
        {

            _bitmap = BitmapImage2Bitmap(ImageSource);
            Color col;
            int Average;

            //x 는 image의 width
            //y 는 image의 hediht
            for (int x = 0; x < _bitmap.Width; x++)
            {
                for (int y = 0; y < _bitmap.Height; y++)
                {
                    //for문을 돌며 이미지 그레이스케일화
                    //그레이스케일은 약간식 다른방법으로도 가능하니 다른걸 사용해도 무관하다.
                    col = _bitmap.GetPixel(x, y);
                    Average = (col.R + col.G + col.B) / 3;

                    col = Color.FromArgb(Average, Average, Average);
                    _bitmap.SetPixel(x, y, col);
                }
            }
            
            ImageSource = GetBitmapImage2(_bitmap);
            _bitmap.Dispose(); // 비트맵은 메모리 해제가 필수!!
        }

        // 임의의 Array로 이미지 만들기
        internal void ArrayToImage(int width, int height)
        {
            // 배열의 사이즈를 width가아닌 stride로 잡아 패딩한만큼 
            int stride = width + (width % 4 != 0 ? 4 - width % 4 : 0);
            // array 갯수 지정 필수
            _imageArray = new byte[stride*height];
            for (int i=0; i< stride * height; i++)
            {
                _imageArray[i] = 100;
            }

            Bitmap _arrayBitmap = ByteToBitmap(_imageArray, width, 500);
            ImageSource = GetBitmapImage2(_arrayBitmap);

        }
    
        // dialog로 이미지 열기
        internal void Imageopen(string fileName)
        {
            ImageSource = GetBitmapImage(new Uri(fileName));
        }

        #endregion

        #region create
        public MainViewModel()
        {
            ImageSource = GetBitmapImage(new Uri(@"C:\Users\user\Desktop\C#자료\C#\비트맵다루기\TestBitmap.bmp", UriKind.Absolute));
            //Bitmap bitmap = (Bitmap)Bitmap.FromFile(@"C:\Users\jiwon\Desktop\새 폴더 (3)\MARBLES.BMP", true);
            //ImageSource = BitmapConversion.BitmapToBitmapSource(bitmap);
        }
        #endregion

        #region Bitmap -> BitmapImage
        // 비트맵(Uri) -> 비트맵 이미지
        public BitmapImage GetBitmapImage(Uri sourceURI)
        {
            BitmapImage bitmap = new BitmapImage();

            bitmap.BeginInit(); // 비트맵 초기화

            bitmap.UriSource   = sourceURI; // Uri의 비트맵이미지 소스를 가져오거나 설정한다.
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // 비트맵 캐시옵션의 
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;

            bitmap.EndInit(); // 비트맵 이미지의 초기화가 끝났음을 나타낸다.
            bitmap.Freeze(); // 이미지가 조작이 되거나 했을때 한번 기강 씨게 잡아준다.

            return bitmap;
        }
        // 비트맵(비트맵) -> 비트맵 이미지
        private BitmapImage GetBitmapImage2(Bitmap bitmap)
        {
            // 메모리 스트림을 사용한다
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Bmp);               // 메모리 스트림에 bitmap을 Bmp로 저장한다.
                stream.Position = 0;                                // 스트림 포지션 0으로 설정해 처음부터 잡음
                BitmapImage bitmapimage = new BitmapImage();        // 새 비트맵 이미지 객체 생성
                bitmapimage.BeginInit();                            // 비트맵 이미지 초기화
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad; // 비트맵 이미지가 다 생성되야 stream 닫게 캐시 설정
                bitmapimage.StreamSource = stream;                  // 
                bitmapimage.EndInit();                              // 비트맵 이미지 초기화 종료
                return bitmapimage;
            }
        }
        #endregion

        #region BitmapImage -> Bitmap
        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
        #endregion

        #region Byte[] -> Bitmap
        // 바이트를 비트맵으로 변환
        public Bitmap ByteToBitmap(byte[] data, int width, int height)
        {
            // byte[]->IntPtr->(stride)->Bitmap->BitmapImage
            // Marshal 
            // Stride // -> 4의 배수  Width 1 -> 4  5 -> 8 

            int stride = width + (width % 4 != 0 ? 4 - width % 4 : 0);

            // 포인트 사용한다고 정의 하며 마샬 메모리 등록
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(data.Length);
            
            // 마샬을가지고 바이트배열을 포인터에 복붙
            Marshal.Copy(data, 0, unmanagedPointer, data.Length);
            // stride를 사용한 비트맵 생성
            // Bitmap(width, height, strid, format, scan)
            Bitmap bmp = new Bitmap(width, height, stride, PixelFormat.Format8bppIndexed, unmanagedPointer);
            // 마샬 메모리 해제
            Marshal.FreeHGlobal(unmanagedPointer);
            return bmp;
        }

        #endregion
        // Bitmap -> BitmapSource
        public static BitmapSource BitmapToBitmapSource(Bitmap source)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                          source.GetHbitmap(),
                          IntPtr.Zero,
                          System.Windows.Int32Rect.Empty,
                          BitmapSizeOptions.FromEmptyOptions());
        }
    }   
}
