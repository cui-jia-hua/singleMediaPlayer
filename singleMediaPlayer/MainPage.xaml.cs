using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace singleMediaPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private MediaSource _mediaSource;
        private MediaPlayer _mediaPlayer;

        private async void MenuFlyoutItem_OnClickAsync(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            openPicker.FileTypeFilter.Add(".mp3");
            openPicker.FileTypeFilter.Add(".mp4");

            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                _mediaSource = MediaSource.CreateFromStorageFile(file);
                //简单访问
                if (_mediaPlayer == null)
                    _mediaPlayer = new MediaPlayer();

                _mediaPlayer.Source = _mediaSource;
                _mediaPlayer.Play();
                MediaPlayerElement.SetMediaPlayer(_mediaPlayer);

            }
        }

        private void OnlineListen_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri(Uri.Text);
            _mediaSource = MediaSource.CreateFromUri(uri);
            //简单访问
            if (_mediaPlayer == null)
                _mediaPlayer = new MediaPlayer();

            _mediaPlayer.Source = _mediaSource;
            _mediaPlayer.Play();
            MediaPlayerElement.SetMediaPlayer(_mediaPlayer);
        }

        private async void DownloadMedia_ClickAsync(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri(Uri.Text);
            using (var httpClient = new Windows.Web.Http.HttpClient())
            {
                // Always catch network exceptions for async methods
                try
                {
                    var downloadMusic = await httpClient.GetBufferAsync(uri);
                    StorageFile destinationFile = await KnownFolders.MusicLibrary.CreateFileAsync("123.mp3", CreationCollisionOption.GenerateUniqueName);
                    using (var stream = await destinationFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        await stream.WriteAsync(downloadMusic);
                        await stream.FlushAsync();
                    }
                }
                catch (Exception)
                {

                }
            }
            
        }

        private void Image_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            //FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            Flyout.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
        }

        private async void ButtonBase_OnClickAsync(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;

            QueryOptions queryOption = new QueryOptions
                (CommonFileQuery.OrderByTitle, new string[] { ".mp3", ".mp4", ".wma" });

            queryOption.FolderDepth = FolderDepth.Deep;

            Queue<IStorageFolder> folders = new Queue<IStorageFolder>();

            var files = await KnownFolders.MusicLibrary.CreateFileQueryWithOptions
                (queryOption).GetFilesAsync();

            ListView.Items.Clear();
            foreach (var file in files)
            {
                // do something with the music files.
                var items = new ListViewItem();
                items.Content = file.Name;
                items.Tag = file.Name;
                items.Background = new AcrylicBrush();
                ListView.Items.Add(items);
            }
        }

        private async void ListView_OnItemClickAsync(object sender, ItemClickEventArgs e)
        {
            StorageFolder folder = KnownFolders.MusicLibrary;
            string m = (string)e.ClickedItem;
            StorageFile a = await folder.GetFileAsync(m);

            _mediaSource = MediaSource.CreateFromStorageFile(a);
            //简单访问
            if (_mediaPlayer == null)
                _mediaPlayer = new MediaPlayer();

            _mediaPlayer.Source = _mediaSource;
            _mediaPlayer.Play();
            MediaPlayerElement.SetMediaPlayer(_mediaPlayer);
        }
    }
}
