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

namespace ProjectTestWpfApp
{
    class MyPopUp
    {

        public static CustomerBluePrint NumberAlreadyExistWindow(string Number)
        {
            if (MessageBox.Show("Customer with mobile number'"+ Number+"' already exist. Do you want to continue with that.", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                return BackEnd.GetDetailsByNumberToCustomerObject(Number);
            }
            else
            {
                return BackEnd.GetDetailsByNumberToCustomerObject(Number);
            }
        }
    }
}
