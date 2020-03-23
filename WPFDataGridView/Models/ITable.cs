namespace WPFDataGridView.Models {
    public interface ITable {
        object this[string key, bool getOldValue = false] { get; set; }
    }
}