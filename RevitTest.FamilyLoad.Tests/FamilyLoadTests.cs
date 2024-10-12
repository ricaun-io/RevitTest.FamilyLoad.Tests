using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace RevitTest.FamilyLoad.Tests
{
    public class FamilyLoadTests : Utils.OneTimeOpenDocumentTest
    {
        //protected override string FileName => @"Family\Project2019.rvt";
        string FamilyPath => $@"Family\Family{application.VersionNumber}.rfa";
        string FamilyName => Path.GetFileNameWithoutExtension(FamilyPath);

        [Test]
        public void RevitTests_LoadFamily()
        {
            using (Transaction transaction = new Transaction(document))
            {
                transaction.Start("LoadFamily");

                var family = FamilyUtils.SelectFamily(document, FamilyName);
                if (family is null)
                {
                    Console.WriteLine($"LoadFamily: \t{FamilyName}");
                    family = FamilyUtils.LoadFamily(document, FamilyPath);
                    if (family is null) Assert.Ignore("LoadFamily fail");
                }

                Console.WriteLine($"Family: \t{family.Name}");

                foreach (var familySymbolId in family.GetFamilySymbolIds())
                {
                    var familySymbol = document.GetElement(familySymbolId) as FamilySymbol;
                    Console.WriteLine($"FamilySymbol: \t{familySymbol.Name}");
                }

                transaction.Commit();
            }
        }

        [Test]
        [Order(1)]
        public void RevitTests_ChangeParameter()
        {
            using (Transaction transaction = new Transaction(document))
            {
                transaction.Start("ChangeParameter");

                var familySymbols = FamilyUtils.SelectFamilySymbols(document, FamilyName);

                Assert.IsNotEmpty(familySymbols, $"Family: \t{FamilyName}");

                foreach (var familySymbol in familySymbols)
                {
                    var parameterTypeComments = familySymbol.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS);
                    var typeComments = parameterTypeComments.AsString();

                    Console.WriteLine($"FamilySymbol: {familySymbol.Name}");
                    Console.WriteLine($"Type Comments: {typeComments}");

                    parameterTypeComments.Set($"This is a type comments - {familySymbol.Name} - {DateTime.UtcNow.Ticks}");
                    typeComments = parameterTypeComments.AsString();

                    Console.WriteLine($"Type Comments to: {typeComments}");
                }

                transaction.Commit();
            }
        }

        [Test]
        [Order(5)]
        public void RevitTests_LoadFamilySymbol()
        {
            using (Transaction transaction = new Transaction(document))
            {
                transaction.Start("LoadFamilySymbol");

                var familySymbol = FamilyUtils.SelectFamilySymbol(document, FamilyName);
                if (familySymbol is null) Assert.Ignore("LoadFamilySymbol fail");

                var parameterTypeComments = familySymbol.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS);
                var typeComments = parameterTypeComments.AsString();

                Console.WriteLine($"Type Comments: {typeComments}");

                familySymbol = FamilyUtils.LoadFamilySymbol(document, FamilyPath, familySymbol.Name);

                parameterTypeComments = familySymbol.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS);
                typeComments = parameterTypeComments.AsString();

                Console.WriteLine($"Type Comments: {typeComments}");

                transaction.Commit();
            }
        }

        [Test]
        [Order(6)]
        public void RevitTests_ReLoadFamily()
        {
            ForceToReloadFamily();

            using (Transaction transaction = new Transaction(document))
            {
                transaction.Start("LoadFamily");

                var family = FamilyUtils.LoadFamily(document, FamilyPath);
                if (family is null) Assert.Ignore("LoadFamily fail");

                var familySymbols = FamilyUtils.SelectFamilySymbols(document, FamilyName);
                foreach (var familySymbol in familySymbols)
                {
                    var parameterTypeComments = familySymbol.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS);
                    var typeComments = parameterTypeComments.AsString();
                    Console.WriteLine($"FamilySymbol: {familySymbol.Name}");
                    Console.WriteLine($"Type Comments: {typeComments}");
                }

                transaction.Commit();
            }
        }

        public void ForceToReloadFamily()
        {
            Console.WriteLine(">> ForceToReloadFamily");

            FamilyUtils.EditLoadFamily(document, FamilyUtils.SelectFamily(document, FamilyName), (familyDocument) =>
            {
                using (Transaction transaction = new Transaction(familyDocument))
                {
                    transaction.Start("Change Family");

                    var name = familyDocument.FamilyManager.CurrentType.Name;
                    familyDocument.FamilyManager.RenameCurrentType(name + $" {DateTime.UtcNow.Ticks}");
                    familyDocument.FamilyManager.RenameCurrentType(name);

                    transaction.Commit();
                }
            });
        }

        [Test]
        [Order(4)]
        public void RevitTests_EditLoadFamily()
        {
            FamilyUtils.EditLoadFamily(document, FamilyUtils.SelectFamily(document, FamilyName), (familyDocument) =>
            {
                using (Transaction transaction = new Transaction(familyDocument))
                {
                    transaction.Start("Change Family");

                    var name = familyDocument.FamilyManager.CurrentType.Name;
                    familyDocument.FamilyManager.RenameCurrentType(name + $" {DateTime.UtcNow.Ticks}");
                    familyDocument.FamilyManager.RenameCurrentType(name);

                    var familyManager = familyDocument.FamilyManager;
                    var familyParameter = familyManager.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS);
                    var lastCurrent = familyManager.CurrentType;

                    foreach (FamilyType familyType in familyManager.Types)
                    {
                        familyManager.CurrentType = familyType;
                        //Console.WriteLine($"FamilyParameter: {familyType.AsString(familyParameter)}");
                        familyManager.Set(familyManager.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS), "Edited");
                    }

                    foreach (FamilyType familyType in familyManager.Types)
                    {
                        familyManager.CurrentType = familyType;
                        //Console.WriteLine($"FamilyParameter: {familyType.AsString(familyParameter)}");
                    }
                    familyManager.CurrentType = lastCurrent;

                    transaction.Commit();
                }
            });

            {
                var familySymbols = FamilyUtils.SelectFamilySymbols(document, FamilyName);
                foreach (var familySymbol in familySymbols)
                {
                    var parameterTypeComments = familySymbol.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS);
                    var typeComments = parameterTypeComments.AsString();
                    Console.WriteLine($"FamilySymbol: {familySymbol.Name}");
                    Console.WriteLine($"Type Comments: {typeComments}");
                }
            }

            //using (Transaction transaction = new Transaction(document))
            //{
            //    transaction.Start("EditLoadFamily");

            //    var family = FamilyUtils.LoadFamily(document, FamilyPath);
            //    document.Regenerate();

            //    var familySymbols = FamilyUtils.SelectFamilySymbols(document, FamilyName);
            //    foreach (var familySymbol in familySymbols)
            //    {
            //        var parameterTypeComments = familySymbol.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS);
            //        var typeComments = parameterTypeComments.AsString();
            //        Console.WriteLine($"FamilySymbol: {familySymbol.Name}");
            //        Console.WriteLine($"Type Comments: {typeComments}");
            //    }

            //    transaction.Commit();
            //}
        }

        [Test]
        [Order(6)]
        public void RevitTests_ReLoadFamily_DifferentPath()
        {
            ForceToReloadFamily();

            using (Transaction transaction = new Transaction(document))
            {
                transaction.Start("ReLoadFamily_DifferentPath");

                var familyPathTemp = Path.Combine(Path.GetTempPath(), Path.GetFileName(FamilyPath));

                File.Copy(FamilyPath, familyPathTemp, true);

                var family = FamilyUtils.LoadFamily(document, familyPathTemp);
                if (family is null) Assert.Ignore("LoadFamily fail");

                var familySymbols = FamilyUtils.SelectFamilySymbols(document, FamilyName);
                foreach (var familySymbol in familySymbols)
                {
                    var parameterTypeComments = familySymbol.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS);
                    var typeComments = parameterTypeComments.AsString();
                    Console.WriteLine($"FamilySymbol: {familySymbol.Name}");
                    Console.WriteLine($"Type Comments: {typeComments}");
                }

                transaction.Commit();
            }
        }

        [Test]
        [Order(10)]
        public void RevitTests_DeleteFamily()
        {
            using (Transaction transaction = new Transaction(document))
            {
                transaction.Start("DeleteFamily");

                var family = FamilyUtils.SelectFamily(document, FamilyName);

                Assert.IsNotNull(family, $"Family: \t{FamilyName}");

                document.Delete(family.Id);

                Console.WriteLine($"Delete {FamilyName}");

                transaction.Commit();
            }
        }
    }

}
