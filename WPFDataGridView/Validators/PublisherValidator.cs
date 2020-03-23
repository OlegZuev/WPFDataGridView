using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using WPFDataGridView.Models;

namespace WPFDataGridView.Validators {
    public class PublisherValidator : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            if (!(value is BindingGroup bindingGroup) || bindingGroup.Items.Count == 0)
                return ValidationResult.ValidResult;

            if (!(bindingGroup.Items[0] is Publisher publisher))
                return new ValidationResult(false, "Row doesn't exist");

            return new ValidationResult(publisher.Validator.IsValid, null);
        }
    }
}