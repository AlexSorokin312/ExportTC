using ExportTC.Services.Navigation;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ContentControl _contentArea;

    public NavigationService(IServiceProvider serviceProvider, ContentControl contentArea)
    {
        _serviceProvider = serviceProvider;
        _contentArea = contentArea;
    }

    public void NavigateTo<TViewModel>() where TViewModel : class
    {
        var viewModel = _serviceProvider.GetService<TViewModel>();
        var viewType = ViewLocator.GetViewTypeForViewModel(typeof(TViewModel));
        if (viewType == null)
            throw new InvalidOperationException($"No view found for {typeof(TViewModel).FullName}");

        var view = Activator.CreateInstance(viewType) as UserControl;
        view.DataContext = viewModel;
        _contentArea.Content = view;
    }
}