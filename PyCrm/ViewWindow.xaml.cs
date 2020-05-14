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
using System.Windows.Shapes;
using System.Windows.Navigation;
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
using Microsoft.Win32;

namespace PYCRM
{
    /// <summary>
    /// Interaction logic for ViewWindow.xaml
    /// </summary>
    public partial class ViewWindow : Window
    {
        DataTable dt = new DataTable();
        MainWindow MainWinObj;

        public HashSet<string> ListToDelete;
        public ViewWindow()
        {
            InitializeComponent();
            FillDataGrid();
            Modify.IsEnabled = false;
            Delete.IsEnabled = false;
        }
        public ViewWindow(MainWindow main)
        {
            MainWinObj = main;
            InitializeComponent();
            FillDataGrid();
            Modify.IsEnabled = false;
            Delete.IsEnabled = false;
        }
        
        public void FillDataGrid()
        {
            dt = BackEnd.LoadDataTableForDataGrid();
            viewPart.ItemsSource = dt.DefaultView;
        } 

        private void ViewPart_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Modify.IsEnabled = true;
            Delete.IsEnabled = true;
            DataRowView rowview = viewPart.SelectedItem as DataRowView;
            if (rowview != null)
            {
                tempbox.Text = rowview.Row.Field<string>("number").ToString();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            BackEnd.DeleteRecord("number",tempbox.Text);
            FillDataGrid();
            Delete.IsEnabled = false;
            Modify.IsEnabled = false;
        }

        private void Modify_Click(object sender, RoutedEventArgs e)
        {
            CustomerBluePrint NewCustomer = BackEnd.GetDetailsByNumberToCustomerObject(tempbox.Text);
            MainWinObj.ObjectToFields(NewCustomer);
            MainWinObj.Submit.Content = "Update";
            MainWinObj.Focus();
            Delete.IsEnabled = false;
            Modify.IsEnabled = false;
        }
        /*
        private void subWindow_View_Closing(object sender, CancelEventArgs e)
        {
            ((MainWindow)this.Owner).view.IsEnabled = true;
            this.Visibility = Visibility.Hidden;
        }*/

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            if(SearchBox.Text == "")
            {
                FillDataGrid();
            }
            else
            {
                dt = BackEnd.SearchDetailsByField(searchBy.SelectedValue.ToString(), SearchBox.Text);
                viewPart.ItemsSource = dt.DefaultView;
            }
        }

        private void viewPart_LostFocus(object sender, RoutedEventArgs e)
        {
            if (viewPart.SelectedItem == null)
            {
                Modify.IsEnabled = false;
                Delete.IsEnabled = false;
            }
        }

        private void viewPart_GotFocus(object sender, RoutedEventArgs e)
        {
            if(viewPart.SelectedItem != null)
            {
                Modify.IsEnabled = true;
                Delete.IsEnabled = true;
            }
        }

        private void PrintDemo_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog1.Title = "Export File";
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "csv";
            saveFileDialog1.Filter = "CSV file (*.csv)|*.csv| All Files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == true)
            {
                viewPart.SelectAllCells();
                viewPart.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
                ApplicationCommands.Copy.Execute(null, viewPart);
                viewPart.UnselectAllCells();
                string result = (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);
                try
                {
                    File.WriteAllText(saveFileDialog1.FileName, result, UnicodeEncoding.UTF8);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "File Access Error");
                }
            }
            /*
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog1.Title = "Export File";
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "csv";
            saveFileDialog1.Filter = "CSV file (*.csv)|*.csv| All Files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == true)
            {
                StreamWriter sw = new StreamWriter(saveFileDialog1.FileName, false);
                for (int k = 1; k < viewPart.Columns.Count; k++)
                {
                    sw.Write(viewPart.Columns[k].Header + ",");
                }
                sw.WriteLine();
                foreach (DataRowView dr in viewPart.ItemsSource)
                {
                    for(int k=0; k< viewPart.Columns.Count; k++)
                    {
                        sw.Write(dr[k].ToString()+ ",");
                    }
                    sw.WriteLine();
                }
                sw.Close();
            }*/
            //CustomerFileGenerator.WriteDataToFile(dt);
        }

        private void SearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter) return;
            e.Handled = true;
            searchButton_Click(sender,e);
        }
    }
}
