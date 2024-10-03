namespace BaseFramework.Models {
	public readonly record struct BitmapLoadingModel(
		bool HasStarted,
		bool HasError,
		bool HasCompleted,
		int Progress,
		Exception? Exception = null
	);
}
