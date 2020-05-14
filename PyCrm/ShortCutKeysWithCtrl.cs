using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PYCRM
{
    class ShortCutKeysWithCtrl
    {
        ShortCutKeysWithCtrl()
        {
            SetCommand(Key.S, MyCommand);
        }
        public static RoutedCommand MyCommand = new RoutedCommand();

        public static void SetCommand(Key MyKey, RoutedCommand ThisCommand)
        {
            ThisCommand.InputGestures.Add(new KeyGesture(MyKey, ModifierKeys.Control));
        }
    }
}
