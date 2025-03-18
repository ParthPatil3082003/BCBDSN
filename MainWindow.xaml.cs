using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json;
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

        private async void ProcessFile(Windows.Storage.StorageFile file) 
        {
            string destination = "C:\\Users\\kshit\\OneDrive\\Desktop\\temp";
            var filepath = file.Path;
            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[fs.Length]; // Buffer to store the raw file data

                // Read the data into the buffer
                fs.Read(buffer, 0, buffer.Length);

                using (FileStream tempFileStream = new FileStream(destination, FileMode.Create, FileAccess.Write))
                {
                    tempFileStream.Write(buffer, 0, buffer.Length); // Write the raw data to the "temp" file
                }
            }

        }

        public void UpdateUI(string username, string address)
        {
            string fmtadd = "@" + address;
            MainpageUsername.Text = username;
            MainpageAddress_display.Text = fmtadd;  // Display the address
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
                ProcessFile(file);
                var properties = await file.GetBasicPropertiesAsync();
                var fileSize = properties.Size.ToString();  // Get file size

                var newItem = new FileItem { Name = file.Name, Size = fileSize, Date = DateTime.Now.ToShortDateString() };
                files.Add(newItem);
            }
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private async void LogoutFromDB()
        {
            HttpClient GoOnlineclient = new HttpClient();
            var goonline = new
            {
                DNAddress = MainpageAddress_display.Text.Substring(1)
            };
            string goOnlineURL = "https://localhost:7018/api/OnlineNodes/GoOffline";
            string goOnlinejson = JsonConvert.SerializeObject(goonline);
            var goOnlinecontent = new StringContent(goOnlinejson, Encoding.UTF8, "application/json");
            HttpResponseMessage goOnlineresponse = await GoOnlineclient.PostAsync(goOnlineURL, goOnlinecontent);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            LogoutFromDB();

            BlankWindow1 logW = new BlankWindow1();
            logW.Activate();

            this.Close();

        }
    }
}
