namespace Dashik.Sdk.Mvvm;

/// <summary>
/// Extensions for <see cref="IMvvmService" />.
/// </summary>
public static class MvvmServiceExtensions
{
    /// <summary>
    /// Create a new instance of view model.
    /// </summary>
    /// <param name="mvvmService"><see cref="IMvvmService" /> instance.</param>
    /// <param name="parameters">Any constructor parameters to be passed to the view model.</param>
    /// <typeparam name="TViewModel">View model type.</typeparam>
    /// <returns>Created view model.</returns>
    public static TViewModel CreateViewModel<TViewModel>(
        this IMvvmService mvvmService,
        params object[] parameters) where TViewModel : class
        => (TViewModel)mvvmService.CreateViewModel(typeof(TViewModel), parameters);
}
