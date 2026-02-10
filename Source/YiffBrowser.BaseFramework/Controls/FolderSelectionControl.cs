using YiffBrowser.BaseFramework.Helpers;
using RW.Common.Helpers;
using System.Windows;
using System.Windows.Controls;

namespace YiffBrowser.BaseFramework.Controls;

[TemplatePart(Name = PART_ButtonEdit, Type = typeof(Button))]
[TemplatePart(Name = PART_ButtonShow, Type = typeof(Button))]
public class FolderSelectionControl : Control {

	private const string PART_ButtonEdit = nameof(PART_ButtonEdit);
	private const string PART_ButtonShow = nameof(PART_ButtonShow);


	public string Path {
		get => (string)GetValue(PathProperty);
		set => SetValue(PathProperty, value);
	}

	public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
		nameof(Path),
		typeof(string),
		typeof(FolderSelectionControl),
		new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
	);



	public bool IsReadonly {
		get => (bool)GetValue(IsReadonlyProperty);
		set => SetValue(IsReadonlyProperty, value);
	}

	public static readonly DependencyProperty IsReadonlyProperty = DependencyProperty.Register(
		nameof(IsReadonly),
		typeof(bool),
		typeof(FolderSelectionControl),
		new PropertyMetadata(false)
	);




	public FolderSelectionControl() {

	}

	public override void OnApplyTemplate() {
		base.OnApplyTemplate();

		if (GetTemplateChild(PART_ButtonEdit) is Button buttonEdit) {
			buttonEdit.Click += (s, e) => {
				if (IsReadonly) {
					return;
				}
				if (PickPathHelper.PickFolder(out string? path)) {
					Path = path;
				}
			};
		}


		if (GetTemplateChild(PART_ButtonShow) is Button buttonShow) {
			buttonShow.Click += (s, e) => {
				if (Path.IsNotBlank()) {
					Path.OpenPathInSystemDefault();
				}
			};
		}

	}
}
