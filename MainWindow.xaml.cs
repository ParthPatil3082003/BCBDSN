using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Microsoft.UI;
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
using Windows.Graphics;
using Microsoft.UI.Windowing;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Test1
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    public static class UdpBackgroundPinger
    {
        private static CancellationTokenSource _cts;

        public static void StartBackgroundPinging(string serverIp, int serverPort, string clientId, int intervalSeconds = 20)
        {
            _cts = new CancellationTokenSource();
            Task.Run(async () =>
            {
                using var udpClient = new UdpClient(0); // Random local port
                var serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);

                Console.WriteLine($"[PINGER] Starting UDP pings to {serverIp}:{serverPort} every {intervalSeconds} seconds...");

                try
                {
                    while (!_cts.Token.IsCancellationRequested)
                    {
                        byte[] message = Encoding.UTF8.GetBytes(clientId);
                        await udpClient.SendAsync(message, message.Length, serverEndPoint);
                        Console.WriteLine($"[PINGER] Ping sent at {DateTime.Now}");

                        await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), _cts.Token);
                    }
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("[PINGER] Ping loop canceled.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PINGER] Error in UDP ping: {ex.Message}");
                }
            });
        }

        public static void StopPinging()
        {
            _cts?.Cancel();
            Console.WriteLine("[PINGER] Stopping background pinging...");
        }
    }

    public sealed partial class MainWindow : Window
    {

        private ObservableCollection<FileItem> files = new ObservableCollection<FileItem>();
        public MainWindow()
        {
            this.InitializeComponent();
            FileListView.ItemsSource = files;
            this.Closed += MainWindow_Closed;
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            // Get the AppWindow from the handle
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);


            appWindow.Resize(new SizeInt32(1200, 775));



        }

        public class FileItem
        {
            public string Name { get; set; }
            public string Size { get; set; }
            public string Date { get; set; }
        }

        public class OnlineNode
        {
            public string dnAddress { get; set; }
            public string ipAddress { get; set; }
            public int port { get; set; }
        }


        private byte[] Combine(byte[] first, byte[] second)
        {
            byte[] combined = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, combined, 0, first.Length);
            Buffer.BlockCopy(second, 0, combined, first.Length, second.Length);
            return combined;
        }

        private byte[] ComputeMerkleRoot(List<byte[]> hashes)
        {
            if (hashes == null || hashes.Count == 0)
                return null;

            List<byte[]> currentLevel = new List<byte[]>(hashes);

            using (SHA256 sha256 = SHA256.Create())
            {
                while (currentLevel.Count > 1)
                {
                    List<byte[]> nextLevel = new List<byte[]>();

                    for (int i = 0; i < currentLevel.Count; i += 2)
                    {
                        if (i + 1 < currentLevel.Count)
                        {
                            byte[] combined = Combine(currentLevel[i], currentLevel[i + 1]);
                            byte[] parentHash = sha256.ComputeHash(combined);
                            nextLevel.Add(parentHash);
                        }
                        else
                        {
                            // Duplicate last hash if odd number
                            byte[] combined = Combine(currentLevel[i], currentLevel[i]);
                            byte[] parentHash = sha256.ComputeHash(combined);
                            nextLevel.Add(parentHash);
                        }
                    }

                    currentLevel = nextLevel;
                }

                return currentLevel[0]; // Merkle Root
            }
        }
        private void WriteChunkHashes(List<byte[]> hashes, string folderPath)
        {
            byte[] merkleRoot = ComputeMerkleRoot(hashes);

            StringBuilder sb = new StringBuilder();

            foreach (var hash in hashes)
            {
                string hashHex = BitConverter.ToString(hash).Replace("-", ""); // Convert to hex string
                sb.Append(hashHex + ";");
            }

            // Remove last ';' if necessary
            if (sb.Length > 0)
                sb.Length--;

            string merkleRootHex = BitConverter.ToString(merkleRoot).Replace("-", "");

            // Use hex string in filename
            string filename = $"mkr.{merkleRootHex}.dn";

            string hashFilePath = Path.Combine(folderPath, filename);
            File.WriteAllText(hashFilePath, sb.ToString());

            Console.WriteLine($"Chunk hashes written to: {hashFilePath}");
        }

        static byte[] EncryptData(byte[] dataToEncrypt, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (MemoryStream msEncrypt = new MemoryStream())
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    csEncrypt.Write(dataToEncrypt, 0, dataToEncrypt.Length);
                    csEncrypt.FlushFinalBlock();

                    return msEncrypt.ToArray(); // Encrypted data
                }
            }
        }

        static (byte[] key, byte[] iv) LoadAESKeyIV(string filePath)
        {
            string content = File.ReadAllText(filePath);
            string[] parts = content.Split(';');

            if (parts.Length != 2)
                throw new Exception("Invalid key file format!");

            byte[] key = Convert.FromBase64String(parts[0]);
            byte[] iv = Convert.FromBase64String(parts[1]);

            return (key, iv);
        }

        async Task SendEncryptedDataAsync(OnlineNode node, byte[] encryptedData)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    //await client.ConnectAsync(node.ipAddress, node.port);
                    await client.ConnectAsync("127.0.0.1", 5000);

                    using (NetworkStream stream = client.GetStream())
                    {
                        // Send encrypted data length first (optional, for easier parsing)
                        byte[] lengthBytes = BitConverter.GetBytes(encryptedData.Length);
                        await stream.WriteAsync(lengthBytes, 0, lengthBytes.Length);

                        // Send encrypted data
                        await stream.WriteAsync(encryptedData, 0, encryptedData.Length);
                        await stream.FlushAsync();
                    }
                }

                Console.WriteLine($"Data sent to {node.ipAddress}:{node.port}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send data: {ex.Message}");
            }
        }

        async Task<OnlineNode> GetRandomOnlineNodeAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                // Accept self-signed certs for localhost
                HttpClientHandler handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };

                client.BaseAddress = new Uri("https://dbserver01.azurewebsites.net");
                client.DefaultRequestHeaders.Accept.Clear();

                HttpResponseMessage response = await client.GetAsync("/api/OnlineNodes");
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();

                // Deserialize
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var nodes = JsonSerializer.Deserialize<List<OnlineNode>>(jsonResponse, options);

                if (nodes == null || nodes.Count == 0)
                    throw new Exception("No online nodes found!");

                // Select random node
                Random rand = new Random();
                int index = rand.Next(nodes.Count);

                return nodes[index];
            }
        }

        private async void StoreChunks()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dnStorePath = Path.Combine(documentsPath, "DNStore");
            string userPath = Path.Combine(dnStorePath, MainpageUsername.Text);
            string uploadtemp = Path.Combine(userPath, "uploadData");

            string[] allFiles = Directory.GetFiles(uploadtemp);
            var tempFiles = allFiles.Where(f => Path.GetFileName(f).StartsWith("temp"));
            foreach (var file in tempFiles) {
                string configPath = Path.Combine(userPath, "config");
                string secretPath = Path.Combine(configPath, "secret.txt");
                var (key, iv) = LoadAESKeyIV(secretPath);
                byte[] data = File.ReadAllBytes(file);
                byte[] encryptedData = EncryptData(data, key, iv);
                //string finalSendData = Convert.ToBase64String(encryptedData);
                OnlineNode selectedNode = await GetRandomOnlineNodeAsync();
                await SendEncryptedDataAsync(selectedNode, encryptedData);

                File.Delete(file);

            }
        }

        private async void ProcessFile(Windows.Storage.StorageFile file)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dnStorePath = Path.Combine(documentsPath, "DNStore");
            string userPath = Path.Combine(dnStorePath, MainpageUsername.Text);
            string uploadtemp = Path.Combine(userPath, "uploadData");

            List<byte[]> chunkHashes = new List<byte[]>();
            const int chunkSize = 256 * 1024; // 256 KB
            byte[] buffer = new byte[chunkSize];

            var filepath = file.Path;

            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                int bytesRead;
                int partNumber = 1;
                using (SHA256 sha256 = SHA256.Create())
                {
                    while ((bytesRead = fs.Read(buffer, 0, chunkSize)) > 0)
                    {
                        string tempFileName = $"temp{partNumber}";
                        string tempFilePath = Path.Combine(uploadtemp, tempFileName);

                        using (FileStream chunkStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                        {
                            chunkStream.Write(buffer, 0, bytesRead);
                        }

                        byte[] chunkData = new byte[bytesRead];
                        Array.Copy(buffer, chunkData, bytesRead);
                        byte[] hash = sha256.ComputeHash(chunkData);
                        chunkHashes.Add(hash);
                        partNumber++;
                    }

                }
            }

            WriteChunkHashes(chunkHashes, uploadtemp);

            StoreChunks();
        }

        public void UpdateUI(string username, string address)
        {
            string fmtadd = "@" + address;
            MainpageUsername.Text = username;
            MainpageAddress_display.Text = fmtadd;  // Display the address

            string serverIp = "4.188.232.157";
            int serverPort = 12345; // Your server UDP listener port
            string clientId = address;

            // Start background ping thread
            UdpBackgroundPinger.StartBackgroundPinging(serverIp, serverPort, clientId, 20);

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
                double fileSize = properties.Size / 1024;  // Get file size
                string filesizeinkb = $"{fileSize:F2} KB";

                var newItem = new FileItem { Name = file.Name, Size = filesizeinkb, Date = DateTime.Now.ToShortDateString() };
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
            string goOnlineURL = "https://dbserver01.azurewebsites.net/api/OnlineNodes/GoOffline";
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
        private void MainWindow_Closed(object sender, WindowEventArgs e)
        {
            LogoutFromDB(); // Call your logout function
        }

    }
}
