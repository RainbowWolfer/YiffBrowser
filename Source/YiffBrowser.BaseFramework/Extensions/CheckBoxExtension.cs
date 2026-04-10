using RW.Common.Helpers;
using System.Windows;
using System.Windows.Controls;

namespace YiffBrowser.BaseFramework.Extensions;

public static class CheckBoxExtension {
	private static readonly Dictionary<string, HashSet<CheckBox>> groups = [];

	public static readonly DependencyProperty GroupNameProperty = DependencyProperty.RegisterAttached(
		"GroupName",
		typeof(string),
		typeof(CheckBoxExtension),
		new PropertyMetadata(string.Empty, OnGroupNameChanged)
	);

	public static string GetGroupName(DependencyObject obj) {
		return (string)obj.GetValue(GroupNameProperty);
	}

	public static void SetGroupName(DependencyObject obj, string value) {
		obj.SetValue(GroupNameProperty, value);
	}

	private static void OnGroupNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is not CheckBox checkBox) {
			return;
		}

		string oldGroup = (string)e.OldValue;
		string newGroup = (string)e.NewValue;

		if (string.IsNullOrEmpty(oldGroup) == false) {
			checkBox.Checked -= CheckBox_Checked;
			checkBox.Unloaded -= CheckBox_Unloaded;

			if (groups.TryGetValue(oldGroup, out HashSet<CheckBox>? oldSet)) {
				oldSet.Remove(checkBox);
			}
		}

		if (string.IsNullOrEmpty(newGroup) == false) {
			if (groups.TryGetValue(newGroup, out HashSet<CheckBox>? newSet) == false) {
				newSet = [];
				groups.Add(newGroup, newSet);
			}

			newSet.Add(checkBox);

			checkBox.Checked += CheckBox_Checked;
			checkBox.Unloaded += CheckBox_Unloaded;
		}
	}

	private static void CheckBox_Checked(object sender, RoutedEventArgs e) {
		if (sender is not CheckBox currentCheckBox) {
			return;
		}

		string groupName = GetGroupName(currentCheckBox);
		if (groupName.IsBlank()) {
			return;
		}

		if (groups.TryGetValue(groupName, out HashSet<CheckBox>? set)) {
			foreach (CheckBox cb in set) {
				if (ReferenceEquals(cb, currentCheckBox) == false && cb.IsChecked == true) {
					cb.IsChecked = false;
				}
			}
		}
	}

	private static void CheckBox_Unloaded(object sender, RoutedEventArgs e) {
		if (sender is not CheckBox checkBox) {
			return;
		}

		string groupName = GetGroupName(checkBox);
		if (groupName.IsNotBlank() && groups.TryGetValue(groupName, out HashSet<CheckBox>? set)) {
			set.Remove(checkBox);
			checkBox.Checked -= CheckBox_Checked;
			checkBox.Unloaded -= CheckBox_Unloaded;
		}
	}
}
