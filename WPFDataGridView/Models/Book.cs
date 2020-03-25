using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using PropertyChanged;
using ReactiveValidation;
using ReactiveValidation.Extensions;

namespace WPFDataGridView.Models {
    [AddINotifyPropertyChangedInterface]
    public class Book : ValidatableObject, IEditableObject, ITable {
        public string ISBN { get; set; }
        public void OnISBNChanged() {
            if (ISBNUtils.IsValid(ISBN))
                PublisherId = ISBNUtils.GetPublisherId(ISBN);
        }

        public int PublisherId { get; set; }

        public void OnPublisherIdChanged() {
            ISBN = Regex.Replace(ISBN, @"(?<=[\d]{3}-\d+-)\d+",
                                 PublisherId < 10 ? "0" + PublisherId : PublisherId.ToString());
        }

        public string Name { get; set; }

        public DateTime ReleaseDate { get; set; }

        public string AgeRestriction { get; set; }

        private Book _backupCopy;

        private bool _inEdit;

        public void BeginEdit() {
            if (_inEdit) return;
            _inEdit = true;
            _backupCopy = MemberwiseClone() as Book;
        }

        public void CancelEdit() {
            if (!_inEdit) return;
            _inEdit = false;
            ISBN = _backupCopy.ISBN;
            PublisherId = _backupCopy.PublisherId;
            Name = _backupCopy.Name;
            ReleaseDate = _backupCopy.ReleaseDate;
            AgeRestriction = _backupCopy.AgeRestriction;
        }

        public void EndEdit() {
            if (!_inEdit) return;
            _inEdit = false;
            _backupCopy = null;
        }

        public void InitializeValidator(IList<Book> books = null, IList<Publisher> publishers = null) {
            Validator = GetValidator(books, publishers);
        }

        private IObjectValidator GetValidator(IList<Book> books = null, IList<Publisher> publishers = null) {
            var builder = new ValidationBuilder<Book>();

            builder.RuleFor(book => book.ISBN)
                   .Must(ISBNUtils.IsValid)
                   .WithMessage("ISBN is not valid");
            builder.RuleFor(book => book.ISBN)
                   .Must(isbn => publishers == null ||
                                 publishers.Any(publisher => publisher.Id == ISBNUtils.GetPublisherId(isbn)))
                   .WithMessage("Publisher that owns such ISBN does not exist")
                   .Must(isbn => books == null ||
                                 !books.Any(book => book != this && book.ISBN == isbn))
                   .WithMessage("ISBN should be unique")
                   .AllWhen(book => ISBNUtils.IsValid(book.ISBN));
            builder.RuleFor(book => book.Name)
                   .NotEmpty()
                   .WithMessage("Name can't be empty");
            builder.RuleFor(book => book.AgeRestriction)
                   .NotEmpty()
                   .WithMessage("Age Restriction can't be empty")
                   .Matches(@"^\d+\+$")
                   .WithMessage("Age Restriction has wrong format");

            return builder.Build(this);
        }

        [SuppressPropertyChangedWarnings]
        public object this[string key, bool getOldValue = false] {
            get {
                switch (key) {
                    case "isbn":
                        return !getOldValue ? ISBN : _backupCopy.ISBN;
                    case "publisher_id":
                        return !getOldValue ? PublisherId : _backupCopy.PublisherId;
                    case "name":
                        return !getOldValue ? Name : _backupCopy.Name;
                    case "release_date":
                        return !getOldValue ? ReleaseDate : _backupCopy.ReleaseDate;
                    case "age_restriction":
                        return !getOldValue ? AgeRestriction : _backupCopy.AgeRestriction;
                }

                throw new ArgumentException($"Key {key} not found");
            }
            set {
                switch (key) {
                    case "isbn":
                        ISBN = (string) value;
                        return;
                    case "publisher_id":
                        PublisherId = (int) value;
                        return;
                    case "name":
                        Name = (string) value;
                        return;
                    case "release_date":
                        ReleaseDate = (DateTime) value;
                        return;
                    case "age_restriction":
                        AgeRestriction = (string) value;
                        return;
                }

                throw new ArgumentException($"Key {key} not found");
            }
        }
    }
}