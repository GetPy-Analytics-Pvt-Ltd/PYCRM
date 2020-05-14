using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;

namespace PYCRM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window
    {
        public HashSet<string> MobileNumberList;
        public string LastNumber;
        public CustomerBluePrint Customer = new CustomerBluePrint();
        string CountryCode = ConfigurationManager.AppSettings["DefaultCountryCode"];
        string DefaultDomain = ConfigurationManager.AppSettings["DefaultDomain"];
        string DefaultCity = ConfigurationManager.AppSettings["DefaultCity"];
        string DefaultState = ConfigurationManager.AppSettings["DefaultState"];
        string CustTable = ConfigurationManager.AppSettings["DetailsTable"];
        string StTable = ConfigurationManager.AppSettings["StateTable"];
        string CtTable = ConfigurationManager.AppSettings["CityTable"];
        string ArTable = ConfigurationManager.AppSettings["AreaTable"];
        string logFileName;
        string logFileDir;
        string logFilePathName;

        public MainWindow()
        {
            InitializeComponent();
            checkForLicenseFile();
            CheckForDBFile();
            LogFileDirectory();
            InitializeComboBox_FromConfigFile();
            InitializeComboBox_FromDataBase();
            countryCode.Text = CountryCode;

            refCountryCode.Text = CountryCode;
            countryCode.Text = CountryCode;
            refCountryCode.Text = CountryCode;
            ResetCmd.InputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(ResetCmd, reset_Click));
            SaveCmd.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(SaveCmd, SaveButton_Click));
        }
        public static RoutedCommand ResetCmd = new RoutedCommand();
        public static RoutedCommand SaveCmd = new RoutedCommand();

        public void CheckForDBFile()
        {
            String dbFileNameConfig = ConfigurationManager.ConnectionStrings["connection"].ConnectionString;
            string [] temp = dbFileNameConfig.Split('=');
            string directoryName = System.IO.Path.GetDirectoryName(temp[1]);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }            
            // Copy the old Log file which will be transferred to clould
            if (!(File.Exists(temp[1])))
            {
                File.Copy("InfoConfig.db", temp[1], true);
            }
            else
            {
                Console.WriteLine(" File exists");
            }
        }


        public void checkForLicenseFile()
        {
            string licenseFilePath = ConfigurationManager.AppSettings["licenseFilePath"];
            string licenseFileName = System.IO.Path.Combine(licenseFilePath, "pyserver.json");
            if (!File.Exists(licenseFileName))
            {
                MessageBox.Show(" License File is not found : Please copy file As "  + licenseFileName);
            }
        }



        public void LogFileDirectory()
        {
            logFileName = ConfigurationManager.AppSettings["logfilename"];
            logFileDir = ConfigurationManager.AppSettings["logFileDir"];
            string notSentDir = ConfigurationManager.AppSettings["NotSentDir"];
            logFilePathName = System.IO.Path.Combine(logFileDir, logFileName);
            if (!Directory.Exists(logFileDir))
            {
                Directory.CreateDirectory(logFileDir);
            }

            // Copy the old Log file which will be transferred to clould
            if (File.Exists(logFilePathName))
            {
                string logFileFullPathName = logFilePathName;
                // Get Log file name to transfer to cloud
                string logFileWithDate = fileNameCreator.logFileNameCreatetor(logFileName);
                string logFilePathNameForNotSent = System.IO.Path.Combine(notSentDir, logFileWithDate);

                File.Copy(logFileFullPathName, logFilePathNameForNotSent, true);
                File.Delete(logFileFullPathName);
            }
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void InitializeComboBox_FromDataBase()
        {
            HashSet<string> ItemList;
            area.Items.Clear();
            city.Items.Clear();
            state.Items.Clear();
            CustomerName.Items.Clear();
            mobileNumber.Items.Clear();
            refMobileNumber.Items.Clear();
            ItemList = BackEnd.LoadComboBox("area", ArTable);
            foreach (string Item in ItemList)
            {
                area.Items.Add(Item);
            }
            ItemList = BackEnd.LoadComboBox("city", CtTable);
            city.Items.Add(DefaultCity);
            foreach (string Item in ItemList)
            {
                city.Items.Add(Item);
            }
            ItemList = BackEnd.LoadComboBox("state", StTable);
            state.Items.Add(DefaultState);
            foreach (string Item in ItemList)
            {
                state.Items.Add(Item);
            }

            ItemList = BackEnd.LoadComboBox("name", CustTable);
            foreach (string Item in ItemList)
            {
                CustomerName.Items.Add(Item);
            }
            ItemList = BackEnd.LoadComboBox("number", CustTable);
            foreach (string Item in ItemList)
            {
                mobileNumber.Items.Add(Item);
            }
            MobileNumberList = BackEnd.LoadComboBox("number", CustTable);
            foreach (string Item in MobileNumberList)
            {
                refMobileNumber.Items.Add(Item);
            }
            BackEnd.Log("Connected to Local DataBase");
        }
        private void InitializeComboBox_FromConfigFile()
        {
            emailPart2.Items.Clear();
            ageGroup.Items.Clear();
            ethnicity.Items.Clear();
            gender.Items.Clear();
            HashSet<string> EmailDomainList= new HashSet<string>();
            EmailDomainList.Add(DefaultDomain);
            foreach (string s in Properties.Settings.Default.EmailDomains)
            {
                EmailDomainList.Add(s);
            }
            foreach (string s in EmailDomainList)
            {
                emailPart2.Items.Add(s);
            }
            foreach (string s in Properties.Settings.Default.AgeGroup)
            {
                ageGroup.Items.Add(s);
            }
            foreach (string s in Properties.Settings.Default.Ethnicity)
            {
                ethnicity.Items.Add(s);
            }
            foreach (string s in Properties.Settings.Default.Gender)
            {
                gender.Items.Add(s);
            }
            /*
            foreach (string s in Properties.Settings.Default.City)
            {
                city.Items.Add(s);
            }
            foreach (string s in Properties.Settings.Default.State)
            {
                state.Items.Add(s);
            }*/

        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (CustomerName.Text == "" || mobileNumber.Text == "")
            {
                numberError.Visibility = Visibility.Visible;
                return;
            }
            Customer.Name = CustomerName.Text;
            Customer.MobileNumber = mobileNumber.Text;

            string Email = emailPart1.Text + emailPart2.Text;
            if (emailPart1.Text == "" || emailPart1.Text == null)
            {
                Customer.Email = "";
            }
            else
            {
                Customer.Email = Email;
            }
            if (dateDoB.Text != null )
            {
                string[] DOB = dateDoB.Text.Split();
                Customer.BirthDay = DOB[0];
                Customer.BirthMonth = DOB[1];
            }
            else
            {
                Customer.BirthDay = "";
                Customer.BirthMonth = "";
            }
            if (dateAnniv.Text != null)
            {
                string[] DOA = dateAnniv.Text.Split();
                Customer.AnniversaryDay = DOA[0];
                Customer.AnniversaryMonth = DOA[1];
            }
            else
            {
                Customer.AnniversaryDay = "";
                Customer.AnniversaryMonth = "";
            }
            Customer.Gender = gender.Text;
            Customer.AgeGroup = ageGroup.Text;
            Customer.Ethnicity = ethnicity.Text;
            if(area.Text != "")
            {
                Customer.Area = area.Text;
                Customer.City = city.Text;
                Customer.State = state.Text;
            }
            Customer.PinCode = pinCode.Text;
            Customer.ReferredBy = refMobileNumber.Text;
            Customer.Remark = remark.Text;
            if(Submit.Content == "Update")
            {
                Customer.ModificationDates = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            }
            else if (Submit.Content == "Save")
            {
                Customer.CreationDate = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            }
            BackEnd.WriteToDataBase(Customer);
            personalDetails.Visibility = Visibility.Collapsed;
            moreDetails.Visibility = Visibility.Collapsed;
            BlobFileGenerator.WriteToBlob(Customer);
            resetAllFields();
        }
        private void MobileNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            numberError.Visibility = Visibility.Hidden;
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void CustomerName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            numberError.Visibility = Visibility.Hidden;
            //setComboboxList(CustomerName.Text);
            Regex regex = new Regex("[^a-zA-Z]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void pinCode_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void reset_Click(object sender, RoutedEventArgs e)
        {
            resetAllFields();
            personalDetails.Visibility = Visibility.Collapsed;
            moreDetails.Visibility = Visibility.Collapsed;
        }
        public void resetAllFields()
        {
            InitializeComboBox_FromConfigFile();
            InitializeComboBox_FromDataBase();
            countryCode.Text = CountryCode;
            CustomerName.Text = "";
            mobileNumber.Text = "";
            emailPart1.Clear();
            emailPart2.SelectedIndex = 0;
            dateDoB.Text = null;
            dateAnniv.Text = null;
            ageGroup.Text = "";
            ethnicity.Text = "";
            gender.Text = "";
            area.Text = "";
            pinCode.Clear();
            city.SelectedIndex = 0;
            state.SelectedIndex = 0;
            refCountryCode.Text = CountryCode;
            refMobileNumber.SelectedIndex = -1;
            remark.Clear();
            LastNumber = "";
            Submit.Content = "Save";
        }

        private void ethnicity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            ethnicity.IsDropDownOpen = true;
            //ethnicity.ItemsSource = DataBase.Persons.Where(p => p.Name.Contains(e.Text)).ToList();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        public void mobileNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            if (mobileNumber.Text != LastNumber)
            {
                if (MobileNumberList.Contains(mobileNumber.Text))
                {
                    Customer = BackEnd.GetDetailsByNumberToCustomerObject(mobileNumber.Text);
                    if(Customer != null)
                    {
                        Submit.Content = "Update";
                        ObjectToFields(Customer);
                    }
                }
                else
                {
                    Submit.Content = "Save";
                }
            }
            if (mobileNumber.Text.Length > 0)
            {
                LastNumber = mobileNumber.Text;
                personalDetails.Visibility = Visibility.Visible;
                moreDetails.Visibility = Visibility.Visible;
            }
            else
            {
                Submit.Content = "Save";
                personalDetails.Visibility = Visibility.Collapsed;
                moreDetails.Visibility = Visibility.Collapsed;
            }
        }
        public void ObjectToFields(CustomerBluePrint customer)
        {
            countryCode.Text = CountryCode;
            if (personalDetails.Visibility == Visibility.Collapsed)
            {
                personalDetails.Visibility = Visibility.Visible;
                moreDetails.Visibility = Visibility.Visible;
            }
            CustomerName.Text = customer.Name;
            mobileNumber.Text = customer.MobileNumber;
            try
            {
                if(customer.Email != null)
                {
                    string[] TempEmail = customer.Email.Split('@');
                    emailPart1.Text = TempEmail[0];
                    emailPart2.Text = "@" + TempEmail[1];
                }
            }
            catch (Exception)
            {
                emailPart1.Text = "";
                emailPart2.SelectedIndex = 0;
            }
            dateDoB.Text = customer.BirthDay + "-" + customer.BirthMonth;
            dateAnniv.Text = customer.AnniversaryDay + "-" + customer.AnniversaryMonth; ;
            ageGroup.Text = customer.AgeGroup;
            ethnicity.Text = customer.Ethnicity;
            gender.Text = customer.Gender;
            area.Text = customer.Area;
            pinCode.Text = customer.PinCode;
            city.Text = customer.City;
            state.Text = customer.State;
            refCountryCode.Text = CountryCode;
            remark.Text = customer.Remark;
            refMobileNumber.Text = customer.ReferredBy;
        }

        private void mobileNumber_GotFocus(object sender, RoutedEventArgs e)
        {
            //mobileNumber.IsDropDownOpen = true;
        }

        private void mobileNumber_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            mobileNumber.IsDropDownOpen = true;
        }

        private void view_Click(object sender, RoutedEventArgs e)
        {
            view.IsEnabled = false;
            ViewWindow SubWindow = new ViewWindow(this);
            SubWindow.Show();
            SubWindow.Closing += (s, ex) => view.IsEnabled = true;
        }

        private void mobileNumber_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void SettingBackBtn_Click(object sender, RoutedEventArgs e)
        {
            settingBtn.Foreground = Brushes.Black;
            settingBtn.Background = Brushes.LightGray;
            settingWindow.Visibility = Visibility.Collapsed;
            homewindow.Visibility = Visibility.Visible;
            countryCode.Text = CountryCode;
            refCountryCode.Text = CountryCode;
        }

        private void settingBtn_Click(object sender, RoutedEventArgs e)
        {   
            settingBtn.Foreground = Brushes.White;
            settingBtn.Background = Brushes.Black;
            settingWindow.Visibility = Visibility.Visible;
            homewindow.Visibility = Visibility.Collapsed;
            countryCodeSettingBox.Text = ConfigurationManager.AppSettings["DefaultCountryCode"]; ;
            stateSettingbix.Text = ConfigurationManager.AppSettings["DefaultState"]; ;
            citySettingBox.Text = ConfigurationManager.AppSettings["DefaultCity"]; ;
        }

        private void OpenComboBoxDropDown(object sender, RoutedEventArgs e)
        {
            try
            {
                ComboBox cBox = sender as ComboBox;
                cBox.IsDropDownOpen = true;
            }
            catch(Exception )
            {

            }
        }

        private void saveSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            if(countryCodeSettingBox.Text != "")
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings.Remove("DefaultCountryCode");
                config.AppSettings.Settings.Add("DefaultCountryCode", countryCodeSettingBox.Text);
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                CountryCode = ConfigurationManager.AppSettings["DefaultCountryCode"];
            }
            if (stateSettingbix.Text != "")
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings.Remove("DefaultState");
                config.AppSettings.Settings.Add("DefaultState", stateSettingbix.Text);
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                DefaultState = ConfigurationManager.AppSettings["DefaultState"];
            }
            if (citySettingBox.Text != "")
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings.Remove("DefaultCity");
                config.AppSettings.Settings.Add("DefaultCity", citySettingBox.Text);
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                DefaultCity = ConfigurationManager.AppSettings["DefaultCity"];
            }
            resetAllFields();
        }
    }

}
