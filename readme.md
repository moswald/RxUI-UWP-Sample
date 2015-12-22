# RxUI and UWP .Net Native Sample

The official release of Reactive UI doesn't fully support UWP at this time.
There's a PR to get it working, but until v7 is released we're including the
necessary projects directly in the solution. Any fixes to the .Net Native issue(s)
we discover here will get rolled into that official PR.

### Organization

* The Reactive UI projects are in the ReactiveUI subfolder.
* To emulate a common solution project layout, the single view model is in a
separate ViewModels class library project.
* The main executable project contains a single view for that view model in the
Views subfolder.

### Usage

When working properly, the app just wants you to enter up to four characters in
the text box, and then click the button. When compiled with .Net Native enabled,
the application will throw an `ArgumentException` at line 956 in PropertyBinding.cs
on startup.
