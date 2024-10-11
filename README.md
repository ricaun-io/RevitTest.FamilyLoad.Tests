# RevitTest.FamilyLoad.Tests

[![Revit 2021](https://img.shields.io/badge/Revit-2021+-blue.svg)](../..)
[![Visual Studio 2022](https://img.shields.io/badge/Visual%20Studio-2022-blue)](../..)
[![License MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

This project test how `LoadFamily` and `LoadFamilySymbol` works with `IFamilyLoadOptions` to overwrite the parameters using the [ricaun.RevitTest](https://ricaun.com/RevitTest) Framework.

## Tests

Looks like the `IFamilyLoadOptions` is not working as expected when the family is loaded for the second time.

This probably happens because the family file is the same, and the family is already loaded inside the document and Revit is not gonna reload the file because is the same family.

Even if you change some types parameter inside the document the family still the same and the `IFamilyLoadOptions` is not gonna be called.

## Revit API Forum

* [Reloading family symbol fails for the second time](https://forums.autodesk.com/t5/revit-api-forum/reloading-family-symbol-fails-for-the-second-time/m-p/13079519)

## License

This project is [licensed](LICENSE) under the [MIT License](https://en.wikipedia.org/wiki/MIT_License).

---

Do you like this project? Please [star this project on GitHub](../../stargazers)!