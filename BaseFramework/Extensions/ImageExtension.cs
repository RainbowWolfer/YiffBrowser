using BaseFramework.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XamlAnimatedGif;

namespace BaseFramework.Extensions {
	public static class ImageExtension {


		public static GifAutoPlayType GetGifAutoPlayType(DependencyObject obj) {
			return (GifAutoPlayType)obj.GetValue(GifAutoPlayTypeProperty);
		}

		public static void SetGifAutoPlayType(DependencyObject obj, GifAutoPlayType value) {
			obj.SetValue(GifAutoPlayTypeProperty, value);
		}

		public static readonly DependencyProperty GifAutoPlayTypeProperty = DependencyProperty.RegisterAttached(
			"GifAutoPlayType",
			typeof(GifAutoPlayType),
			typeof(ImageExtension),
			new PropertyMetadata(GifAutoPlayType.WhenMouseOver, OnGifAutoPlayTypeChanged, GifAutoPlayTypeCoerce)
		);

		private static object GifAutoPlayTypeCoerce(DependencyObject d, object baseValue) {
			if (d is Image image && baseValue is GifAutoPlayType type) {
				image.MouseEnter -= Image_MouseEnter;
				image.MouseLeave -= Image_MouseLeave;
				switch (type) {
					case GifAutoPlayType.Never:
						AnimationBehavior.SetAutoStart(image, false);
						break;
					case GifAutoPlayType.WhenMouseOver:
						AnimationBehavior.SetAutoStart(image, false);
						image.MouseEnter += Image_MouseEnter;
						image.MouseLeave += Image_MouseLeave;
						break;
					case GifAutoPlayType.Always:
						AnimationBehavior.SetAutoStart(image, true);
						break;
					default:
						throw new NotImplementedException();
				}
			}

			return baseValue;
		}

		private static void Image_MouseLeave(object sender, MouseEventArgs e) {
			if (sender is Image image) {
				AnimationBehavior.GetAnimator(image)?.Pause();
			}
		}

		private static void Image_MouseEnter(object sender, MouseEventArgs e) {
			if (sender is Image image) {
				AnimationBehavior.GetAnimator(image)?.Play();
			}
		}

		private static void OnGifAutoPlayTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {

		}
	}

}
