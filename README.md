# CustomDataTip

## Overview

This is a Visual Studio extension that places an adornment next to the variable being hovered over when debugging, much like the Data Tips that Visual Studio provides by default.
The adornment displays the variable contents in a tree view, which can be expanded and collapsed when clicked.
At the moment, the adornment will only appear when debugging TypeScript files, because the initial purpose of this extension is to address the lack of Data Tips when debugging; however,
with simple modifications, the adornment can appear when debugging any file type.

## Usage Info

To install, build the solution and double click the generated VSIX file from the folder.
To uninstall, select Tools > Extensions and updates, and then search for the extension and click uninstall.

Requirements: Visual Studio 2015

Note: The extension is still fairly rough around the edges and very much under development, so please report issues on the GitHub repo.

## Development Info

### Built With

* VS SDK - Framework for creating Visual Studio extensions.
* EnvDTE - COM library containing the objects and members for Visual Studio core automation, including the debugger.

### Upcoming Features

This project is still in its infancy, so at the moment, there are a lot of features that I would like to build out in the future, such as:
* Copy a variable to clipboard as JSON
* Quickly adding a variable to watch
* Querying an array variable with something like LINQ
