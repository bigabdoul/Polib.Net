# Polib.Net
An awesome yet simple set of libraries for handling .po translation files

## What is Polib.Net?
Have you ever wanted to make multi-lingual apps but was bored with .NET's 
way of doing it using ResourceManager? ResourceManager is good but one of its 
drawbacks is that you have to recompile (over and over again) your translations 
into binary files and re-deploy them whenever you add new culture-dependent bits 
to the app. But that's only the easy part. Managing translation files 
becomes a nightmare when you want to work as a team on the translations.

Instead of relying on ResourceManager, there're other ways to support multiple
cultures, such as the popular PO translations file format. The PO (.po) files
make it a breeze to support multiple languages within your applications.

The main goal of the Polib.Net Solution is to use a set of libraries to use and 
manage .po translation files within a .NET application in a simple yet effective 
way. By simple and effective I mean that you should be able to add new translations 
and make them appear in your apps on the fly, just by updating your .po files, 
without recompiling the whole stuff into binaries and re-deploy it. Check out 
the **Polib.NetCore.Web** project in the *demos* folder, an ASP.NET Core Web 
application under development to showcase this feature.

## Getting started
To get up and running, do the following:

1. Clone this repository into a directory (e.g. 'github-projects'):

   ```git clone https://github.com/bigabdoul/Polib.Net.git```

   This will create a subdirectory 'Polib.Net' in 'github-projects' like so: 
   'github-projects/Polib.net'

2. Clone this demo-dependent project also into 'github-projects' like so:

   ```git clone https://github.com/bigabdoul/Bootstrap.Themes.git```

   This project is not required by the Polib.Net Solution but is used in the demo 
   web app and is just a fancy way to change its appearence using many pre-built 
   Bootstrap themes.

3. Open the solution file **Polib.Net.sln** and build the solution. 
Once the dependencies restored, in the 'demos' Solution Folder, set the project
'Polib.NetCore.Web' as the **StartUp Project**, then hit F5 to run it.

This should you keep going for now.

## Exploring the Solution
The Solution has 4 folders, namely *demos*, *Solution Items*, *src*, and *test*.

### The *src* folder
It contains three projects:

1. **Polib.NetStandard**: This is the core project targeting .NET Standard 1.4, 
which means it's a cross-platform implementation. It responsible for all interactions 
with .po translation files.

2. **Polib.NetCore.Mvc**: This one is a translation library project targeting 
.NET Core 2.1 for use within an ASP.NET Core MVC application.

3. **Polib.Net.Mvc**: Like the one above, it's a translation library project for
ASP.NET MVC but targeting .NET Framework 4.6.1.

### The *test* folder
One project with a couple of unit tests is available for now. Only the core features
of the solution have been tested. Many more tests are expected to be done in the near
future.

### The *demos* folder
As previously mentioned, this folder contains the *Polib.NetCore.Web* project, which
showcases an ASP.NET Core MVC web application making use of the *Polib.NetStandard*,
*Polib.NetCore.Mvc*, and *Bootstrap.Themes* projects. For simplicity, it uses
AngularJS to provide client-side functionalities, such as making Web API calls and 
displaying the results in the browser.

### The *Solution Items* folder
This folder contains items that have no functional impact on the Solution itself, 
such as this *README.md* and other files. It just makes it easier to organize the 
non-vital items that are part of the Solution.

## Examples
After you managed to run the demo app, let's look at how you can integrate the 
libraries into a new project. For example, to build a multi-lingual console app:
1. In Visual Studio or Visual Studio Code, create a new console project.
2. Add a reference to the *Polib.NetStandard* project, or the *Polib.Net.dll* file.
3. In your console app, add the following *using* statements at the top of the 
'Program.cs' file:

   ```
   using Polib.Net;
   using System;
   ```
   In the *public static void Main(string[] args)* method, add the following:
   ```
   // get a reference to the default translation manager instance
   var manager = TranslationManager.Instance;
   manager.PoFilesDirectory = "path/to/po/files";
   
   var result = manager.TranslatePlural("fr-FR"
    , "{0} media file restored from the trash."
    , "{0} media files restored from the trash.", 3, 3);

    Console.WriteLine(result);
   ```

Of course you may be wondering: "Where are the translation files? Do I have to write
them manually?". Well, there're good news: you can use the GNU Gettext tools used to
extract translatable strings from your source code (like the one above) and generate
.po template files from them. On Windows, you can grab one of these tools from here: 
https://mlocati.github.io/articles/gettext-iconv-windows.html

Once downloaded and installed the binaries of gettext, you can execute the following
at the command prompt (supposing that the current directory is where 'Program.cs' is):
```
xgettext.exe -k -kTranslatePlural:2,3 --from-code=UTF-8 -LC# --omit-header -omessages.pot -fProgram.cs
```
More on that command, right now! First, there's 'xgettext.exe', which is a
command-line tool used to extract translatable strings from source code. It supports
multiple programming languages, one of which is C# (hence the -LC# switch).

The -k option stands for 'keyword', meaning the method name used to translate 
strings. In our case, it's the 'TranslatePlural' method, hence '-kTranslatePlural'. 
Then this is followed by a colon ':', and the numbers 2 (second parameter, which 
identifies the singular message) and 3 (third parameter, which identifies the plural 
message). The first '-k' option indicates that the xgettext program should not
provide its own default keywords.

The -o (output) option followed by 'messages.pot' indicates the .pot (PO template)
file to write on disk as the result of parsing our source code.

The -f (file) input option indicates the file to scan. This could be a list of files
or even another file that contains the list of files to scan; but here, we are using
just 'Program.cs'.

This command should generate a 'messages.pot' file in the same directory as the 
'Program.cs' is in. If you open this file with a program like *Poedit*, you can easily
start making translations. In our *Main(string args[])* method above, you can also 
simply rename the file's extension to .po and replace 
```manager.PoFilesDirectory = "path/to/po/files";``` with the directory name that
```messages.po``` is in. Or, you could just load this ```messages.po``` files:
```
using Polib.Net;
using Polib.Net.IO;
using System;
using System.Collections.Generic;

public class Program
{
    public static void Main(string[] args)
    {
        // the culture we're handling
        var culture = "fr-FR";

        // read the translation file; make sure to set the full path
        var catalog = PoFileReader.Read("messages.po", culture);

        // get a reference to the default translation manager instance
        var manager = TranslationManager.Instance;

        // add the catalog to the translation manager's catalogs dictionary;
        // we could add as many different cultures as we want to support;
        manager.Catalogs.Add(culture, new[] { catalog });

        // use a generic list instead of an array if you intend to monitor
        // changes that occur within the file system in the directory containing
        // the translation files, like so:
        // manager.Catalogs.Add(culture, new List<ICatalog> { catalog });

        // should come as a parameter from somewhere
        var filesRestored = 3;

        // translate the message and format the output on the fly
        var result = manager.TranslatePlural(culture
            , "{0} media file restored from the trash."
            , "{0} media files restored from the trash."
            , filesRestored
            , filesRestored);

        Console.WriteLine(result);
    }
}
```
To see how you can extract translation strings from your source code, look in the
*Solution Items* folder for an item named 'genpot.bat'. It operates on the web app
demo project by extracting translation strings from the models, views, and
controllers and generating .pot files in a 'temp' directory. Should I have more time,
I'll write a command-line app that does all this and more (such as merging
translation strings from the source into existing .po files). The command-line app 
should run automatically after a successful build of the demo web app.

Copyright (C) 2018 Abdoul Kaba https://www.djoola.com
