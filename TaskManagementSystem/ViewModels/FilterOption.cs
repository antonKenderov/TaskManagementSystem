namespace TaskManagementSystem.ViewModels
{
    /// <summary>
    /// A single entry in a filter drop-down. The item itself is never null - only
    /// its value is - because a ComboBox will not select a null item.
    /// </summary>
    public sealed class FilterOption
    {
        public FilterOption(string label, object? value)
        {
            Label = label;
            Value = value;
        }

        public string Label { get; }

        public object? Value { get; }
    }
}
