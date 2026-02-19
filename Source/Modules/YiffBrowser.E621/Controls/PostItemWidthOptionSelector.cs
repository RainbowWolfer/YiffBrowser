using HandyControl.Controls;
using System.Windows;
using System.Windows.Controls;
using YiffBrowser.E621.Services;

namespace YiffBrowser.E621.Controls;

internal class PostItemWidthOptionSelector : Control {




	public IReadOnlyList<PostItemWidthOption> PostItemWidthOptions {
		get => (IReadOnlyList<PostItemWidthOption>)GetValue(PostItemWidthOptionsProperty);
		set => SetValue(PostItemWidthOptionsProperty, value);
	}

	public static readonly DependencyProperty PostItemWidthOptionsProperty = DependencyProperty.Register(
		nameof(PostItemWidthOptions),
		typeof(IReadOnlyList<PostItemWidthOption>),
		typeof(PostItemWidthOptionSelector),
		new PropertyMetadata(null, OnPostItemWidthOptionsChanged)
	);

	private static void OnPostItemWidthOptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is PostItemWidthOptionSelector self) {
			self.UpdateItemsSource();
		}
	}

	public PostItemWidthOption SelectedPostItemWidthOption {
		get => (PostItemWidthOption)GetValue(SelectedPostItemWidthOptionProperty);
		set => SetValue(SelectedPostItemWidthOptionProperty, value);
	}

	public static readonly DependencyProperty SelectedPostItemWidthOptionProperty = DependencyProperty.Register(
		nameof(SelectedPostItemWidthOption),
		typeof(PostItemWidthOption),
		typeof(PostItemWidthOptionSelector),
		new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedPostItemWidthOptionChanged)
	);

	private static void OnSelectedPostItemWidthOptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is PostItemWidthOptionSelector self) {
			self.UpdateSelection();
		}
	}

	private ButtonGroup? buttonGroup;

	public override void OnApplyTemplate() {
		base.OnApplyTemplate();

		buttonGroup = (ButtonGroup?)GetTemplateChild("PART_ButtonGroup");

		UpdateItemsSource();
	}

	private void UpdateItemsSource() {
		if (buttonGroup is null) {
			return;
		}

		buttonGroup.Items.Clear();

		if (PostItemWidthOptions != null) {
			foreach (PostItemWidthOption item in PostItemWidthOptions) {
				RadioButton button = new() {
					Content = item.Name,
					Tag = item,
				};
				button.Click += Button_Click;
				buttonGroup.Items.Add(button);
			}
		}

		buttonGroup.InvalidateVisual();
	}

	private void UpdateSelection() {
		if (buttonGroup is null) {
			return;
		}

		if (SelectedPostItemWidthOption is null) {
			return;
		}

		foreach (RadioButton button in buttonGroup.Items.OfType<RadioButton>()) {
			if (button.Tag is PostItemWidthOption item) {
				button.IsChecked = item == SelectedPostItemWidthOption;
			}
		}

		buttonGroup.InvalidateVisual();
	}

	private void Button_Click(object sender, RoutedEventArgs e) {
		if (sender is RadioButton radioButton) {
			SelectedPostItemWidthOption = (PostItemWidthOption)radioButton.Tag;
		}
	}
}
