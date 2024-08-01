# AB-Modbus-Master
Standalone Windows application - Master for Allen Bradley, some Omron and Modbus PLCs, using [libplctag](https://github.com/libplctag/libplctag) library v2.1.22 (created by Kyle Hayes) and a modified [C# Wrapper](https://github.com/mesta1/libplctag-csharp) (originally created by Michele Cattafesta).

Intended to be used solely as a testing tool (not fit for any production environment):
- Try to resort to READING only, unless you really need to WRITE (which could potentially be dangerous when dealing with PLCs attached to some machines)

It is designed to use embedded dll libraries (added to resources):
- unmanaged `plctag.dll`
- managed `LibplctagWrapper.dll`
- This was done so the app can be in the form of a standalone executable file

Once run, this app is supposed to do the following:
- Create a copy of the `plctag.dll` file in the application folder where it resides
- Load this file copy into the memory when needed
- Delete this file copy when the app is closed
- All this is required because this is unmanaged library

This app will do `on-demand` connection and disconnect if there are no continuous requests:
- Using `Send` buttons ONLY will follow this `connect-disconnect` pattern
- Enabling the `AUTO READ` will try to maintain the continuous connection:
  - Use its `Interval` combobox to select the polling interval in milliseconds
  - Its `Status` indicator should only provide the current status:
    - GREEN for Success
    - RED for Failed
  - It will disable all the address fileds with the `Read` box checked so it can poll them continuously
  - Use the remaining address fields to manually `Read` or `Write` in parallel by clicking the `Send` button:
    - Enter the address first and if the `Write` option is selected then also enter the value to write

Possible BUG: The app might hang on a still active TCP connection so give it a few seconds before deciding to close the app.
Always check the Task Manager to see if the app is still running, force close the app if necessary and delete the file manually.

WORKAROUND would be to comment out all the code within the `AppShutdown` sub inside the `ApplicationEvents.vb` file. This way the extracted `plctag.dll` library will remain in the application folder. If you do ever update the plctag library to newer version then the app will overwrite the old file.

An easy alternative to use instead of this app would be the [AdvancedHMI](https://www.advancedhmi.com/) software since it is highly functional and free.

## Important Note:
~ Some AntiVirus software might detect this behavior of extracting the library as a Trojan, that's why you get the whole solution ~

# AB Screenshot

![AB Screenshot](screenshots/AB%20&%20Modbus%20Master%20(AB).png?raw=true)

# Modbus Screenshot

![Modbus Screenshot](screenshots/AB%20&%20Modbus%20Master%20(Modbus).png?raw=true)

# Functionality
- Either a single or multiple values can be displayed per tag entered, either of `string` `char` `integer` `float` ...etc.
- The app provides automated READ while, during this operation, unused tag spots can be populated and used to WRITE in parallel
- Here is a brief explanation of the screenshots above:
  - Using the address of `N7:0; Int8; 8` is equivalent of saying "Read 8 consecutive Int8 values starting at address N7:0"
  - Using `/` in the address indicates either bit or character reading:
    - Using the address of `B3:1/0; Int16; 16` is equivalent of saying "From the address B3:1 read 16 consecutive bits of Int16 value starting at bit 0"
    - Using the address of `hr0/7; String; 5; 20` is equivalent of saying "From holding register 0 and starting at character 7 read 5 consecutive characters of 20-character long string"
- The `Get Tags` button will fetch ControlLogix tags and selecting any of the fetched tags will copy it to the clipboard.
- You can specify the name of the Program to get tags for (the default is set to `MainProgram`).
- `Boolean Display` defaults to showing boolean values as `True:False` but can also use either `1:0` or `On:Off`
- MicroLogix PID addressing is also a part of this app (ex. `PD10:0` or `PD10:0.PV`), might not work with newer versions of the libplctag library
- As for AB tags, you will need to specify the Custom String Length when the `custom string` data type is selected.
- As for Modbus tags, you will need to specify the String Length when the `string` data type is selected.
- Modbus addressing: `CO = Coil`, `DI = Discrete Input`, `IR = Input Register`, `HR = Holding Register` (all these are set by 0, 1, 3 and 4 xxxxx addressing).
- Modbus byte/word swapping is a bit tricky but I hope most of it functions correctly.
- Some error handling has been built into the app but it is also relying on the libplctag library itself for additional error handling.
- Either or both dll files can be updated via the project's Properties/Resources page, where new dll(s) are added as existing resource files to the `Files` section. Depending on the changes made to the new versions of the plctag library, this app might lose some functionality (like MicroLogix PID addressing or some other).
- There is also an experimental support for 128-bit values.

The Modbus part of this app can be tested with the [ModbusSlaveSimulation](https://github.com/GitHubDragonFly/ModbusSlaveSimulation) (check the video further below).

There might be bugs in the app. Not everything could be tested by me, since I don't have access to all the different PLCs supported by the libplctag library. See the libplctag website for all PLCs supported by the library. Read comments inside the Form1 for any additional information.

# Build

All it takes is to:

## For Windows

- Either use Windows executable files from the `exe` folder or follow the instructions below to build it yourself:
  - Download and install Visual Studio community edition (ideally 2019).
  - Download and extract the zip file of this project.
  - Open this as an existing project in Visual Studio and, on the menu, do:
    - Build/Build Solution (or press Ctrl-Shift-B).
    - Debug/Start Debugging (or press F5) to run the app.
  - Locate created EXE file in the /bin/Debug folder and copy it over to your preferred folder or Desktop.

If you need to run this app on an `x86` based Windows computer then you will need to replace the `plctag` library with its `x86` version.

## For Mono

- Mac Mono version is the executable file of a slightly modified version of this project:
  - It has both libraries separated from the executable file (because Mono is a bit finicky)
  - You can also create the executable file yourself just by modifying this project
- The `libplctag.dylib` library file is version 2.1.22, compiled on iMac G5 PowerPC (32-bit), while version 2.3.6 of the library is available in the [Python_libplctag_Mac_PPC](https://github.com/GitHubDragonFly/Python_libplctag_Mac_PPC) project
- You can try replacing it with 64-bit Mac library available on the [libplctag releases](https://github.com/libplctag/libplctag/releases) page, which should be for Intel based cpu
- Running it from terminal should work fine with standard user account but if it doesn't then switch to superuser account (sudo su)
- This particular version should work fine in Windows, as it is, and also in Linux Mono (most distributions) for as long as you replace the `libplctag.dylib` library with the correct version of either `libplctag.dll` or `libplctag.so` file.

# Video

https://github.com/user-attachments/assets/fc14f971-62e6-4472-b3e1-55d63202b8be

# Licensing
This is all licensed under Mozilla Public License 2.0 (the MIT license of the C# Wrapper is included in the Resources folder as well as its zip file).

# Trademarks
Any and all trademarks, either directly or indirectly mentioned in this project, belong to their respective owners.

# Useful Resources
- The [AdvancedHMI](https://www.advancedhmi.com) project offers FREE software with a bunch of different drivers available:
  - It should be considered as a master application for multiple PLC brands, like AB or Modbus or Omron ... etc
  - It also has a lots of additional components available in its [forum](https://www.advancedhmi.com/forum/).
