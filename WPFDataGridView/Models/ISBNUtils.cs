using System.Text.RegularExpressions;
using PropertyChanged;
using ReactiveValidation;
using ReactiveValidation.Extensions;

namespace WPFDataGridView.Models {
    public static class ISBNUtils  {
        public static bool IsValid(string isbn) {
            return IsValidIsbn13(isbn);
        }

        public static int GetPublisherId(string isbn) {
            return int.Parse(Regex.Match(isbn, @"(?<=\d+-\d+-)\d+(?=-\d+-\d+)").Value);
        }

        private static bool IsValidIsbn13(string isbn13) {
            if (string.IsNullOrEmpty(isbn13)) {
                return false;
            }

            if (isbn13.Contains("-")) {
                isbn13 = isbn13.Replace("-", string.Empty);
            }

            if (isbn13.Length != 13) {
                return false;
            }

            if (!long.TryParse(isbn13, out long _)) {
                return false;
            }

            int sum = 0;
            for (int i = 0; i < 12; i++) {
                sum += (isbn13[i] - '0') * (i % 2 == 1 ? 3 : 1);
            }

            int remainder = sum % 10;
            int checkDigit = 10 - remainder;
            if (checkDigit == 10) {
                checkDigit = 0;
            }
            bool result = checkDigit == isbn13[12] - '0';
            return result;
        }
    }
}