using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Test1
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        private ObservableCollection<FileItem> files = new ObservableCollection<FileItem>();
        public MainWindow()
        {
            this.InitializeComponent();
            FileListView.ItemsSource = files;


        }

        public class FileItem
        {
            public string Name { get; set; }
            public string Size { get; set; }
            public string Date { get; set; }
        }

        private async void FileDetails(Windows.Storage.StorageFile file) 
        { 
            var f1 = file.Path;
            Console.WriteLine(f1);
        
        }

        private async void FabButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add("*");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                FileDetails(file);
                var properties = await file.GetBasicPropertiesAsync();
                var fileSize = properties.Size.ToString();  // Get file size

                var newItem = new FileItem { Name = file.Name, Size = fileSize, Date = DateTime.Now.ToShortDateString() };
                files.Add(newItem);
            }
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            BlankWindow1 logW = new BlankWindow1();
            logW.Activate();

            this.Close();

        }
    }
}
