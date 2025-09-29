namespace BrainWave.APP.Services;
public class NavigationService
{
    public Task GoAsync(string route, IDictionary<string, object>? parameters = null)
    {
        // Ensure parameters is never null
        var safeParams = parameters ?? new Dictionary<string, object>();
        return Shell.Current.GoToAsync(route, safeParams);
    }
}
