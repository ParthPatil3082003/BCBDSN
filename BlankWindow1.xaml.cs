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
using System.Threading.Tasks;
using Microsoft.UI;
using WinRT.Interop;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Composition;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.UI.Xaml.Hosting;
using System.Numerics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Test1
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BlankWindow1 : Window
    {

        private async Task<bool> AuthenticateUser(string username, string password)
        {

            if (username == "User1" && password == "123")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public BlankWindow1()
        {
            this.InitializeComponent();
        }

        private async void savebtn_Click(object sender, RoutedEventArgs e)
        {
           bool isValid = await AuthenticateUser(UsernameTextBox.Text, PassWord.Password);
            if (isValid) {
                MainWindow m_window = new MainWindow();
                m_window.Activate();

                this.Close();




            }
            else
            {
                // Display error message if login fails
                ErrorMessageTextBlock.Text = "Invalid username or password.";
            }
        }

        private void cancelbtn_Click(object sender, RoutedEventArgs e)
        {
            UsernameTextBox.Text = "";
            PassWord.Password = "";

       }

        private void RegPage_Click(object sender, RoutedEventArgs e)
        {
            RegistrationPage RegW = new RegistrationPage();
            RegW.Activate();

            this.Close();
        }
    }
}
