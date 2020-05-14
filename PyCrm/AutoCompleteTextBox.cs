using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectTestWpfApp
{
    class AutoCompleteTextBox
    {
        public static void setComboboxList(string comboBox,HashSet<string> list, string text)
        {
            foreach(string item in list)
            {
                if(!item.Contains(text))
                {
                    list.Remove(item);
                }
            }
        }
    }
}
