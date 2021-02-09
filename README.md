# AB-Modbus-Master
Standalone Windows application, Master for Allen Bradley and Modbus PLCs, using [libplctag](https://github.com/libplctag/libplctag) library and modified [C# Wrapper](https://github.com/mesta1/libplctag-csharp).

Intended to be used solely as a testing tool (not fit for any production environment).

It is designed to use embedded unmanaged plctag.dll C library and managed LibplctagWrapper.dll C# Wrapper.
This was done so the app can be in the form of a standalone executable file (somewhat unorthodox approach).

Once run, this app is supposed to create a copy of the plctag.dll file in the application folder (because this is unmanaged library), load it in memory when needed and delete the file when the app is closed.
It might hang on a still active TCP connection so give it a few seconds before deciding to close the app.
If the plctag.dll file is still present in the application folder then open the Task Manager, force close the app and delete the file manually.

## Note: Some AntiVirus software might detect this behavior as a Trojan

# Licensing
This is all licensed under Mozilla Public License 2.0 (the MIT license of the C# Wrapper is included in the Resources folder).

# Trademarks
Any and all trademarks, either directly on indirectly mentioned in this project, belong to their respective owners.
