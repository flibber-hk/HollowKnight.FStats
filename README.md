# HollowKnight.FStats

Stats mod for Hollow Knight. Displays some stats on the screen after the end credits.

## Global Settings

By editing the values in the FStats.GlobalSettings file, you can choose to hide/show certain stats.

## API

FStats includes an API for other mods to add stat pages to the end screen. This is commonly done using one the following ways:

#### Option 1 - define a stat controller to track stats.

A `StatController` is an object which tracks stats from the beginning of a save file, and may produce any number (typically 0 or 1) of stat pages.
Defining them is done using the `FStats.API.OnGenerateFile` event. This can be used as follows:

```cs
public void SubscribeFStats()
{
    FStats.API.OnGenerateFile += gen => gen(new MyModStats());
}
````

This way, whenever a new save file is created, a MyModStats object (which must inherit from StatController) is added to the save file.
StatControllers defined in this way will have an entry in the FStats globalsettings, so can be hidden/shown by the user.

#### Option 2 - defining stat pages on the end screen

Each `DisplayInfo` object encapsulates the info that the end screen will use to build a page. Any number of pages can be defined this way.
Defining them is done using the `FStats.API.OnGenerateScreen` event. This can be used as follows:

```cs
public void RegisterPages(Action<DisplayInfo> registerPage)
{
    // Define any number of DisplayInfo objects, and call `registerPage` on each of them.
}

public void SubscribeFStats()
{
    FStats.API.OnGenerateScreen += RegisterPages;
}
```

This event is invoked just before the end screen is shown to the player.

For mods which do not want an entire screen, there is also an API for adding strings entries to the extension stats page (this page will be created
if at least one mod adds an entry). This can be used by subscribing to the `OnBuildExtensionStats` event - the `addEntry` parameter can be invoked
on a string to add a line to the extension stats page with that string.

Commonly, a subscription to the OnBuildExtensionStats event will be managed by a stat controller that gets registered via the OnGenerateFile event 
(as in, a stat controller subscribes to OnBuildExtensionStats in `Initialize` and unsubscribes in `Unload`).
