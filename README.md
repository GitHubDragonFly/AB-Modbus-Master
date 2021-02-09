# AB-Modbus-Master
Standalone Windows application, Master for Allen Bradley and Modbus PLCs, using [libplctag](https://github.com/libplctag/libplctag) library (by Kyle Hayes) and modified [C# Wrapper](https://github.com/mesta1/libplctag-csharp) (by Michele Cattafesta).

Intended to be used solely as a testing tool (not fit for any production environment).

It is designed to use embedded unmanaged plctag.dll C library and managed LibplctagWrapper.dll C# Wrapper.
This was done so the app can be in the form of a standalone executable file (somewhat unorthodox approach).

Once run, this app is supposed to create a copy of the plctag.dll file in the application folder (because this is unmanaged library), load it in memory when needed and delete the file when the app is closed.
It might hang on a still active TCP connection so give it a few seconds before deciding to close the app.
If the plctag.dll file is still present in the application folder then open the Task Manager, force close the app and delete the file manually.
It would be a good habit to always check the Task Manager.
See the ApplicationEvents.vb file for code that etracts the plctag.dll library.

## Important Note: Some AntiVirus software might detect this behavior as a Trojan

# Functionality
- Either a single or multiple values can be displayed per tag entered, either of string/char/integer/float...etc.
- The app provides automated READ while, during this operation, unused tag spots can be populated and used to write in parallel.
- The "Get Tags" button will fetch ControlLogix tags and selecting any of the fetched tags will copy it to the clipboard.
- You can specify the name of the Program to get tags for (the default is set to MainProgram).
- As for AB tags, you will need to specify the Custom String Length when the "custom string" data type is selected.
- As for Modbus tags, you will need to specify the String Length when the "string" data type is selected.
- Modbus addressing: CO = Coil, DI = Discrete Input, IR = Input Register, HR = Holding Register (all these set by 0, 1, 3 and 4 xxxxx addressing).
- Modbus byte/word swapping is a bit tricky but I hope most of it functions correctly.
- Some error handling has been built into the app but it is also relying on the libplctag library itself for additional error handling.

There might be bugs in the app. Not everything could be tested by me, since I don't have access to all the different PLCs supported by the libplctag library. See the libplctag website for all PLCs supported by the library. Read comments inside the Form1 for any additional information.

# Build
All it takes is to:

- Download and install Visual Studio community edition (ideally 2019).
- Download and extract the zip file of this project.
- Open this as an existing project in Visual Studio and, on the menu, do:
-- Build/Build Solution (or press Ctrl-Shift-B).
-- Debug/Start Debugging (or press F5) to run the app.
-- Locate created EXE file in the /bin/Debug folder and copy it over to your preferred folder or Desktop.

# Licensing
This is all licensed under Mozilla Public License 2.0 (the MIT license of the C# Wrapper is included in the Resources folder as well as its zip file).

# Trademarks
Any and all trademarks, either directly on indirectly mentioned in this project, belong to their respective owners.
