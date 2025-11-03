namespace BaseFramework.Services;
public static class AppEventsService {
	public static EventAggregator EventAggregator { get; } = new();
}
