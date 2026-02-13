namespace YiffBrowser.BaseFramework.Models;

public readonly record struct CacheLoadingModel(
    bool HasStarted,
    bool HasError,
    bool HasCompleted,
    int Progress,
    Exception? Exception = null
);
