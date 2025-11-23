using BaseFramework.Interfaces;
using System.Windows;

namespace BaseFramework.Views;

public class WindowBase : Window, IWindowBase {
    Window IWindowBase.Window => this;

    public WindowBase() {

    }

}
