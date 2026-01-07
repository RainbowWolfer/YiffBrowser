using DevExpress.Mvvm;
using System.Windows;

namespace BaseFramework.ViewModels;

public class GridDefinitionModel : BindableBase {

    public bool IsExpanded {
        get => GetProperty(() => IsExpanded);
        set => SetProperty(() => IsExpanded, value);
    }

    public double Min {
        get => GetProperty(() => Min);
        set => SetProperty(() => Min, value);
    }

    public GridLength LastSize {
        get => GetProperty(() => LastSize);
        set => SetProperty(() => LastSize, value);
    }


    private DelegateCommand? toggleIsExpandedCommand;
    public IDelegateCommand ToggleIsExpandedCommand => toggleIsExpandedCommand ??= new(ToggleIsExpanded);
    private void ToggleIsExpanded() {
        IsExpanded = !IsExpanded;
    }

    public GridDefinitionModel() {
        IsExpanded = true;
    }

    public GridDefinitionModel(bool isExpanded, double min, GridLength lastSize) {
        IsExpanded = isExpanded;
        Min = min;
        LastSize = lastSize;
    }

}
