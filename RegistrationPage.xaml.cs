using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Test1
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RegistrationPage : Window
    {
        public RegistrationPage()
        {
            this.InitializeComponent();
        }


        private void GenerateMasterKey()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] keyBytes = new byte[32]; // 32 bytes = 256 bits
                rng.GetBytes(keyBytes); // Fill the array with random bytes

                // Convert the key to a hexadecimal string (optional)
                string hexKey = BitConverter.ToString(keyBytes).Replace("-", "").ToLower();

                // Store the key in a file called "config"
                File.WriteAllText("config", hexKey);

            }
        }

        private async void savebtn_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string email = EmailTextBox.Text;
            string password = PassWord.Password;

            HttpClient client = new HttpClient();
            string Registerurl = "https://localhost:7018/api/LoginInfoes/register";

            var newUser = new
            {
                Username = username,
                DNAddress = "none",
                EmailId = email,
                Password = password
            };

            string jsondata = JsonConvert.SerializeObject(newUser);
            var content = new StringContent(jsondata, Encoding.UTF8, "application/json");
            HttpResponseMessage postresponse = await client.PostAsync(Registerurl, content);
            string responseContent = await postresponse.Content.ReadAsStringAsync();

            if (postresponse.IsSuccessStatusCode)
            {
                //GenerateMasterKey();
                BlankWindow1 logW = new BlankWindow1();
                logW.Activate();
                this.Close();
            }
            else
            {
                // If not a success, handle error response
                ErrorMessageTextBlock.Text = responseContent; // Display the error message in a UI element
            }



        }


        private void cancelbtn_Click(object sender, RoutedEventArgs e)
        {
            UsernameTextBox.Text = "";
            PassWord.Password = "";
            EmailTextBox.Text = string.Empty;

        }
    }
}
