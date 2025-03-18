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

        private void savebtn_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string email = EmailTextBox.Text;

            HandleUserLogin(username, email);
            


        }


        private void EncrptMasterKey() 
        {
        
        }


        private void HandleUserLogin(string username, string email)
        {
            bool isUsernameAvailable = CheckUsernameAvailability(username);
            bool isEmailAvailable = CheckEmailAvailability(email);

            if (isUsernameAvailable && isEmailAvailable)
            {

                EncrptMasterKey();
                // Proceed to login
                BlankWindow1 logW = new BlankWindow1();
                logW.Activate();
                this.Close();
            }
            else if (!isUsernameAvailable)
            {
                 ErrorMessageTextBlock.Text = "Username is not available. Please choose another one.";
            }
            else if (!isEmailAvailable)
            {
                ErrorMessageTextBlock.Text = "Email Already Registered";
            }
        }

        // Dummy functions to check availability (Replace these with actual database/API calls)
        private bool CheckUsernameAvailability(string username)
        {
            // Simulating a database check
            return username == "testUser" ? false : true;  // Example: "testUser" is taken
        }

        private bool CheckEmailAvailability(string email)
        {
            // Simulating a database check
            return email == "test@example.com" ? false : true;  // Example: "test@example.com" is registered
        }

        private void cancelbtn_Click(object sender, RoutedEventArgs e)
        {
            UsernameTextBox.Text = "";
            PassWord.Password = "";
            EmailTextBox.Text = string.Empty;

        }
    }
}
