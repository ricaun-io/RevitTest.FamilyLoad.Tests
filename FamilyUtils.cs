using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitTest.FamilyLoad.Tests
{
    public class FamilyUtils
    {
        public static Family SelectFamily(Document document, string familyName)
        {
            return new FilteredElementCollector(document)
                .OfClass(typeof(Family))
                .Cast<Family>()
                .FirstOrDefault(x => x.Name == familyName);
        }

        public static IEnumerable<FamilySymbol> SelectFamilySymbols(Document document, string familyName)
        {
            return new FilteredElementCollector(document)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .Where(x => x.FamilyName == familyName);
        }

        public static FamilySymbol SelectFamilySymbol(Document document, string familyName, string familySymbolName = null)
        {
            return SelectFamilySymbols(document, familyName)
                .FirstOrDefault(x => x.Name == familySymbolName || string.IsNullOrEmpty(familySymbolName));
        }

        public static Family LoadFamily(Document document, string familyPath, IFamilyLoadOptions familyLoadOptions = null)
        {
            familyLoadOptions ??= new OverwriteFamilyLoadOptions();
            var loaded = document.LoadFamily(familyPath, familyLoadOptions, out Family family);
            Console.WriteLine($" LoadFamily: {loaded}");
            return family;
        }

        public static FamilySymbol LoadFamilySymbol(Document document, string familyPath, string familySymbolName, IFamilyLoadOptions familyLoadOptions = null)
        {
            familyLoadOptions ??= new OverwriteFamilyLoadOptions();
            var loaded = document.LoadFamilySymbol(familyPath, familySymbolName, familyLoadOptions, out FamilySymbol familySymbol);
            Console.WriteLine($" LoadFamilySymbol: {loaded}");
            return familySymbol;
        }

        public static Family EditLoadFamily(Document document, Family family, Action<Document> familyDocumentAction = null, IFamilyLoadOptions familyLoadOptions = null)
        {
            familyLoadOptions ??= new OverwriteFamilyLoadOptions();
            using (Document familyDocument = document.EditFamily(family))
            {
                if (familyDocument.IsFamilyDocument)
                {
                    familyDocumentAction?.Invoke(familyDocument);
                    family = familyDocument.LoadFamily(document, familyLoadOptions);
                }
                familyDocument.Close(false);
            }
            return family;
        }

        private class OverwriteFamilyLoadOptions : IFamilyLoadOptions
        {
            public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
            {
                Console.WriteLine($"OnFamilyFound: \tFamilyInUse: {familyInUse}");
                overwriteParameterValues = true;
                return true;
            }

            public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
            {
                Console.WriteLine($"OnSharedFamilyFound: \tFamilyInUse: {familyInUse}");
                source = FamilySource.Family;
                overwriteParameterValues = true;
                return true;
            }
        }
    }
    
}
