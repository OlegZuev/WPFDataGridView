using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using WPFDataGridView.Models;

namespace WPFDataGridView.Validators {
    public class BookValidator : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            if (!(value is BindingGroup bindingGroup) || bindingGroup.Items.Count == 0)
                return ValidationResult.ValidResult;

            if (!(bindingGroup.Items[0] is Book book))
                return new ValidationResult(false, "Row doesn't exist");

            return new ValidationResult(book.Validator.IsValid, null);
        }
    }
}