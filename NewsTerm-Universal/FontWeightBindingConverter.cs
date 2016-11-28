using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace NewsTerm_Universal
{
    class FontWeightBindingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;

            if (((bool)value) == true)
                return FontWeights.SemiBold;
            return FontWeights.Light;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            //throw new NotImplementedException();
            return null;
        }
    }
}
