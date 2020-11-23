namespace Robot.Management.Client
{
    using System;
    using System.Drawing;
    using System.Net.Sockets;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media.Imaging;

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ManagementClientConnection _clientConnection;

        public MainWindow()
        {
            InitializeComponent();

            var tcpClient = new TcpClient("localhost", 4652);
            _clientConnection = new ManagementClientConnection(tcpClient);
        }

        public void UpdateFrame(Bitmap bitmap)
        {
            if (bitmap is null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            var source = Imaging.CreateBitmapSourceFromHBitmap(
                bitmap: bitmap.GetHbitmap(),
                palette: IntPtr.Zero,
                sourceRect: Int32Rect.Empty,
                sizeOptions: BitmapSizeOptions.FromWidthAndHeight(bitmap.Width, bitmap.Height));

            ViewBitmap.Source = source;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StartCapture();
        }

        private async void StartCapture()
        {
            await _clientConnection.StartVideoCaptureAsync();

            while (true)
            {
                using var bitmap = await _clientConnection.VideoClient.ReceiveAsync();
                ViewBitmap.Dispatcher.Invoke(() => UpdateFrame(bitmap));
            }
        }
    }
}
