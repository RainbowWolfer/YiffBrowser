using RW.Common.Helpers;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using YiffBrowser.E621.Enums;
using YiffBrowser.E621.Models.E621;

namespace YiffBrowser.E621.Controls;

internal class TypeHintControl : Control, INotifyPropertyChanged {
	public event PropertyChangedEventHandler? PropertyChanged;
	private void Raise(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


	public E621Post Post {
		get => (E621Post)GetValue(PostProperty);
		set => SetValue(PostProperty, value);
	}

	public static readonly DependencyProperty PostProperty = DependencyProperty.Register(
		nameof(Post),
		typeof(E621Post),
		typeof(TypeHintControl),
		new PropertyMetadata(null, OnPostChanged)
	);


	public E621FileType FileType {
		get => (E621FileType)GetValue(FileTypeProperty);
		private set => SetValue(FileTypePropertyKey, value);
	}

	private static readonly DependencyPropertyKey FileTypePropertyKey = DependencyProperty.RegisterReadOnly(
		nameof(FileType),
		typeof(E621FileType),
		typeof(TypeHintControl),
		new PropertyMetadata(E621FileType.Unknown)
	);

	public static readonly DependencyProperty FileTypeProperty = FileTypePropertyKey.DependencyProperty;



	public SoundWarningType SoundWarningType {
		get => (SoundWarningType)GetValue(SoundWarningTypeProperty);
		private set => SetValue(SoundWarningTypePropertyKey, value);
	}

	private static readonly DependencyPropertyKey SoundWarningTypePropertyKey = DependencyProperty.RegisterReadOnly(
		nameof(SoundWarningType),
		typeof(SoundWarningType),
		typeof(TypeHintControl),
		new PropertyMetadata(SoundWarningType.NoSound)
	);

	public static readonly DependencyProperty SoundWarningTypeProperty = SoundWarningTypePropertyKey.DependencyProperty;



	public string FileTypeHint {
		get {
			if (FileType is E621FileType.WEBM && Post != null && NumberHelper.ConvertDouble(Post.Duration, out double duration)) {
				return $"{Math.Round(duration)}s";
			} else {
				return $"{FileType}";
			}
		}
	}

	private static void OnPostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is TypeHintControl self) {
			if (e.NewValue is E621Post post) {
				self.FileType = post.GetFileType();
				self.SoundWarningType = post.GetSoundWarningType();
			} else {
				self.FileType = E621FileType.Unknown;
				self.SoundWarningType = SoundWarningType.None;
			}

			self.Raise(nameof(FileTypeHint));
		}
	}
}
