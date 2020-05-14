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

namespace ProjectTestWpfApp
{
    /// <summary>
    /// Interaction logic for TempWin.xaml
    /// </summary>
    public partial class TempWin : Window
    {
        public TempWin()
        {
            InitializeComponent();
            this.Visibility = Visibility.Visible;
        }
    }
}
