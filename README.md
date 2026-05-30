# Smart.Mvvm .NET - MVVM helper library

[![NuGet](https://img.shields.io/nuget/v/Usa.Smart.Mvvm.svg)](https://www.nuget.org/packages/Usa.Smart.Mvvm/)

## Features

* `ObservableObject` base class implementing `INotifyPropertyChanged`
* `[ObservableProperty]` source generator attribute for boilerplate-free property declarations
* Messaging system (`IMessenger`, `Messenger`) with event, cancel, and resolve request types
* `ViewModelBase` with `BusyState`, `Disposables`, and `ErrorInfo` helpers
* Resolver (DI container) provider integration via `ResolveProvider`
