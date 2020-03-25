using System;
using System.Globalization;
using System.Windows.Data;
using WPFDataGridView.Models;

namespace WPFDataGridView.Converters {
    public class PublisherEditingDisplayConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var temp = value as Publisher;
            return temp?.Name + " " + temp?.Address;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}