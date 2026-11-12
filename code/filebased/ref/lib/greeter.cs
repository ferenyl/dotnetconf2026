// A file-based LIBRARY: no top-level statements, OutputType=Library.
// Referenced from ../app.cs with `#:ref`. It's built as its own assembly,
// unlike #:include which compiles the file into the app.
#:property OutputType=Library
#:package Humanizer.Core@2.*

using Humanizer;

public static class Greeter
{
    // The Humanizer dependency flows to the app through the reference
    public static string Hello(string name, int count) =>
        $"Hello {name}, you have {"new message".ToQuantity(count, ShowQuantityAs.Words)}";
}
