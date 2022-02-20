using Lucompressor.Controls;
using Lucompressor_RTComp;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PInvoke;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Lucompressor
{
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {
        private AppWindow m_appWindow;

        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(myWndId);
        }

        public MainWindow()
        {
            this.InitializeComponent();

            try
            {
                m_appWindow = GetAppWindowForCurrentWindow();
                if (m_appWindow != null)
                {
                    if (AppWindowTitleBar.IsCustomizationSupported())
                    {
                        m_appWindow.Title = "Lucompressor";
                        m_appWindow.SetIcon("Assets/AppIcon.ico");
                        m_appWindow.TitleBar.BackgroundColor =
                            (Color)Application.Current.Resources["SystemAltHighColor"];
                    }
                    else
                    {
                        Title = "Lucompressor?";
                        
                        var Hwnd = WindowNative.GetWindowHandle(this);
                        var HIcon = User32.LoadImage(IntPtr.Zero, "Assets/AppIcon.ico",
                            User32.ImageType.IMAGE_ICON, 16, 16, User32.LoadImageFlags.LR_LOADFROMFILE);

                        User32.SendMessage(Hwnd, User32.WindowMessage.WM_SETICON, (IntPtr)0, HIcon);                        
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception throwed while trying to instanciate a AppWindow: " + e.ToString());
            }

            var SettingsContainer = ApplicationData.Current.LocalSettings;
            if (SettingsContainer.Values.ContainsKey("OutputDirectory"))
            {
                TBX_OutDirectory.Text = SettingsContainer.Values["OutputDirectory"].ToString();
            }
            else
            {
                TBX_OutDirectory.Text = KnownFolders.PicturesLibrary.Path;
                SettingsContainer.Values.Add("OutputDirectory", KnownFolders.PicturesLibrary.Path);
            }
        }

        private const string PlaceholderImg_Path = "ms-appx:///Assets/Img_Placeholder.jpg";

        private ObservableCollection<ImageStackedData> m_imgsourcearr = new ObservableCollection<ImageStackedData>
                {
                    new ImageStackedData(),
                };

        public ObservableCollection<ImageStackedData> ImgSourceArr
        {
            get => m_imgsourcearr;
            set
            {
                m_imgsourcearr = value;
                OnPropertyChanged(nameof(ImgSourceArr));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void BT_LoadImg_Click(object sender, RoutedEventArgs e)
        {
            var HWND = WindowNative.GetWindowHandle(this);

            var FilePicker = new FileOpenPicker();

            FilePicker.FileTypeFilter.Add(".jpg");
            FilePicker.FileTypeFilter.Add(".jpeg");
            FilePicker.FileTypeFilter.Add(".png");

            InitializeWithWindow.Initialize(FilePicker, HWND);

            FilePicker.ViewMode = PickerViewMode.Thumbnail;
            FilePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            var FileArr = await FilePicker.PickMultipleFilesAsync();

            if (FileArr.Count == 0) return;

            foreach (var file in FileArr)
            {
                Debug.WriteLine("Adding path " + file.Path + " to ImgSourceArr.");

                if (ImgSourceArr[0].ImgSettings.ImgPath == PlaceholderImg_Path)
                {
                    ImgSourceArr[0].ImgSettings = new ImageProps { ImgPath = file.Path };
                    continue;
                }

                ImgSourceArr.Add(new ImageStackedData { ImgSettings = new ImageProps { ImgPath = file.Path } });
            }
        }

        private void BT_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (ImgSourceArr.Count == 1)
            {
                ImgSourceArr[0].ImgSettings = new ImageProps();
            }
            else
            {                
                ImgSourceArr.RemoveAt(Gallery.SelectedIndex);
            }
        }

        private async void BT_CompressImgs_Click(object sender, RoutedEventArgs e)
        {
            if (ImgSourceArr[0].ImgSettings.ImgPath != PlaceholderImg_Path)
            {
                Loading_Dialog.ShowAsync();

                foreach (ImageStackedData stackedData in ImgSourceArr)
                {                    
                    if (!await stackedData.ImgSettings.CompressImageAndSave(TBX_OutDirectory.Text, ImgSourceArr.IndexOf(stackedData)))
                    {
                        Loading_Dialog.Hide();

                        ContentDialog ErrDialog = new ContentDialog();
                        ErrDialog.Title = "An error has ocurred";
                        ErrDialog.CloseButtonText = "Close";
                        ErrDialog.XamlRoot = Loading_Dialog.XamlRoot;
                        ErrDialog.ShowAsync();
                        break;
                    }
                }

                Loading_Dialog.Hide();
                
                Gallery.SelectedIndex = 0;

                ImgSourceArr.Clear();
                ImgSourceArr.Add(new ImageStackedData ());

                ContentDialog CompletedDialog = new ContentDialog();
                CompletedDialog.Title = "Compression completed";
                CompletedDialog.CloseButtonText = "Close";
                CompletedDialog.XamlRoot = Loading_Dialog.XamlRoot;
                CompletedDialog.ShowAsync();
            }
        }

        private async void BT_SearchPath_Click(object sender, RoutedEventArgs e)
        {
            var FolderPicker = new FolderPicker();

            var HWND = WindowNative.GetWindowHandle(this);

            InitializeWithWindow.Initialize(FolderPicker, HWND);

            FolderPicker.ViewMode = PickerViewMode.Thumbnail;
            FolderPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            var SelectedFolder = await FolderPicker.PickSingleFolderAsync();

            if (SelectedFolder != null)
            {
                TBX_OutDirectory.Text = SelectedFolder.Path;
                
                var SettingsContainer = ApplicationData.Current.LocalSettings;

                if (SettingsContainer.Values.ContainsKey("OutputDirectory"))
                    SettingsContainer.Values["OutputDirectory"] = SelectedFolder.Path;
                else
                    SettingsContainer.Values.Add("OutputDirectory", SelectedFolder.Path);
            }
        }
    }
}
