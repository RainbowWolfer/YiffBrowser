using System.ComponentModel;

namespace YiffBrowser.BaseFramework.Interfaces;

public interface IVariableSizedGridItem : INotifyPropertyChanged {
	int ColSpan { get; }
	int RowSpan { get; }
}
