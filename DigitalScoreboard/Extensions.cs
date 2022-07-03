﻿namespace DigitalScoreboard;


public static class Extensions
{
    public static ICommand Command(this INavigationService navigator, string uri, Action<NavigationParameters>? setParams = null)
        => ReactiveCommand.CreateFromTask(() => navigator.Navigate(uri, setParams));


    public static async Task Navigate(this INavigationService navigator, string uri, Action<NavigationParameters>? setParams = null)
    {
        var p = new NavigationParameters();
        setParams?.Invoke(p);
        var result = await navigator.NavigateAsync(uri, p);
        if (!result.Success)
            throw new InvalidOperationException("Failed to navigate", result.Exception);
    }
}

