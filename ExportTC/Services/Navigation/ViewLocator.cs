public static class ViewLocator
{
    private static readonly Dictionary<Type, Type> _mappings = new();

    public static void Register<TViewModel, TView>()
    {
        _mappings[typeof(TViewModel)] = typeof(TView);
    }

    public static Type GetViewTypeForViewModel(Type viewModelType)
    {
        _mappings.TryGetValue(viewModelType, out var viewType);
        return viewType;
    }
}