# Contributing to Surge
All contributions are welcome! Don't feel nervous - if you have an idea, implement it and make a pull request. If I'm not ready to accept it, I'll discuss it with you. I follow Scott Hanselman's idea to ["Bring Kindness back to Open Source"](http://www.hanselman.com/blog/BringKindnessBackToOpenSource.aspx).

If you want to contribute but aren't sure what to do, look at the open [issues](https://github.com/alexhardwicke/Surge/issues "issues").

## Code structure
### Surge.Core
Surge.Core is an F# project. It contains the code that handles network communication, and the implementations of each individual torrent client.

Outside of the folders for specific clients, Surge.Core **must** remain client agnostic.

I'm much newer to F# than I am to C# - please feel free to make pull requests, or just give me feedback and tips on using the language.

### Surge.Shared
This C# shared project contains code that is used by both Surge.Windows8 and Surge.Windows10

### Surge.Windows8
This is a C# project, and is the entry-point for the app. It contains the app manifest, Views and ViewModels and other files that are specific to WinRT.

This project should for the most part be client agnostic - it should not have code that relies on a specific BitTorrent client. This is sometimes unavoidable (e.g. when one torrent client supports a feature and the other doesn't), but even these features should be client agnostic - e.g. support for tags should support more than just µTorrent tags.

### Surge.Windows10
This is an in-development Windows 10 app. F# is not currently supported by Windows 10 - until that changes, I won't be putting much focus here.

Otherwise, the same applies here as to Surge.Windows8.

## Code Formatting
Before you make any pull requests with changes to a C# project, please run your code through Microsoft's [https://github.com/dotnet/codeformatter](https://github.com/dotnet/codeformatter "Code Formatter"). The COPYRIGHT file in the root of the repository should be passed as a parameter if you've added any new files, e.g.:

    codeformatter.exe Surge.sln /copyright:COPYRIGHT

## FAQs

### Why don't Surge.Windows8 and Surge.Windows10 share ViewModels and other non-UI code?
The Windows 8.1 app uses Microsoft Prism, while the Windows 10 app uses [Template10](https://github.com/Windows-XAML/Template10 "Template10").

### What about xxx torrent client?
Surge is built so that support for other torrent clients can easily be added. Support for Deluge and µTorrent is planned - if you want to implement this before me, please do so!

### What about Windows Phone 8.1?
If you want to implement a Windows Phone 8.1 UI, go ahead! There'll be a Windows 10 Mobile once F# is supported by Windows 10 apps though, so don't worry too much. :)