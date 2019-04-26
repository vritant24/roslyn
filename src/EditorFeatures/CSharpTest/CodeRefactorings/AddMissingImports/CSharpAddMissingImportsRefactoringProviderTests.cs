// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.CodeRefactorings.AddMissingImports;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.CodeRefactorings;
using Microsoft.CodeAnalysis.Editor.UnitTests;
using Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.PasteTracking;
using Microsoft.CodeAnalysis.SymbolSearch;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.VisualStudio.Composition;
using Roslyn.Test.Utilities;
using Roslyn.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.AddMissingImports
{
    [UseExportProvider]
    public class CSharpAddMissingImportsRefactoringProviderTests : AbstractCSharpCodeActionTest
    {
        // Add a SymbolSearchService that will find the "Point" class in "System.Drawing" used to test
        // adding reference assemblies
        private static readonly ComposableCatalog AddMissingImportsTestCatalog = TestExportProvider.EntireAssemblyCatalogWithCSharpAndVisualBasic
                .WithoutPartsOfType(typeof(ISymbolSearchService)).WithPart(typeof(TestSymbolSearchService))
                .WithoutPartsOfType(typeof(MetadataServiceFactory)).WithPart(typeof(TestMetadataService));

        protected override CodeRefactoringProvider CreateCodeRefactoringProvider(Workspace workspace, TestParameters parameters)
        {
            var testWorkspace = (TestWorkspace)workspace;
            var pasteTrackingService = testWorkspace.ExportProvider.GetExportedValue<PasteTrackingService>();
            return new CSharpAddMissingImportsRefactoringProvider(pasteTrackingService);
        }

        protected override TestWorkspace CreateWorkspaceFromFile(string initialMarkup, TestParameters parameters)
        {
            var exportProvider = ExportProviderCache.GetOrCreateExportProviderFactory(AddMissingImportsTestCatalog)
                .CreateExportProvider();
            var workspace = TestWorkspace.CreateCSharp(initialMarkup, exportProvider: exportProvider);

            // Treat the span being tested as the pasted span
            var hostDocument = workspace.Documents.First();
            var pastedTextSpan = hostDocument.SelectedSpans.FirstOrDefault();

            if (!pastedTextSpan.IsEmpty)
            {
                var pasteTrackingService = workspace.ExportProvider.GetExportedValue<PasteTrackingService>();

                // This tests the paste tracking service's resiliancy to failing when multiple pasted spans are
                // registered consecutively and that the last registered span wins.
                pasteTrackingService.RegisterPastedTextSpan(hostDocument.TextBuffer, default);
                pasteTrackingService.RegisterPastedTextSpan(hostDocument.TextBuffer, pastedTextSpan);
            }

            return workspace;
        }

        private Task TestInRegularAndScriptAsync(
            string initialMarkup, string expectedMarkup,
            bool placeSystemNamespaceFirst, bool separateImportDirectiveGroups)
        {
            var options = OptionsSet(
                SingleOption(GenerationOptions.PlaceSystemNamespaceFirst, placeSystemNamespaceFirst),
                SingleOption(GenerationOptions.SeparateImportDirectiveGroups, separateImportDirectiveGroups));
            return TestInRegularAndScriptAsync(initialMarkup, expectedMarkup, options: options);
        }

        [WpfFact]
        [Trait(Traits.Feature, Traits.Features.AddMissingImports)]
        public async Task AddMissingImports_AddImport_CanAddReferenceAssembly()
        {
            var code = @"
class C
{
    void Main()
    {
        var point = [|new Point(1, 1)|];
    }
}
";

            var expected = @"
using System.Drawing;

class C
{
    void Main()
    {
        var point = new Point(1, 1);
    }
}
";

            await TestInRegularAndScriptAsync(code, expected);
        }

        [WpfFact]
        [Trait(Traits.Feature, Traits.Features.AddMissingImports)]
        public async Task AddMissingImports_AddImport_PasteContainsSingleMissingImport()
        {
            var code = @"
class C
{
    public [|D|] Foo { get; }
}

namespace A
{
    public class D { }
}
";

            var expected = @"
using A;

class C
{
    public D Foo { get; }
}

namespace A
{
    public class D { }
}
";

            await TestInRegularAndScriptAsync(code, expected);
        }

        [WpfFact]
        [Trait(Traits.Feature, Traits.Features.AddMissingImports)]
        public async Task AddMissingImports_AddImportsBelowSystem_PlaceSystemFirstPasteContainsMultipleMissingImports()
        {
            var code = @"
using System;

class C
{
    [|public D Foo { get; }
    public E Bar { get; }|]
}

namespace A
{
    public class D { }
}

namespace B
{
    public class E { }
}
";

            var expected = @"
using System;
using A;
using B;

class C
{
    public D Foo { get; }
    public E Bar { get; }
}

namespace A
{
    public class D { }
}

namespace B
{
    public class E { }
}
";

            await TestInRegularAndScriptAsync(code, expected, placeSystemNamespaceFirst: true, separateImportDirectiveGroups: false);
        }

        [WpfFact]
        [Trait(Traits.Feature, Traits.Features.AddMissingImports)]
        public async Task AddMissingImports_AddImportsAboveSystem_DontPlaceSystemFirstPasteContainsMultipleMissingImports()
        {
            var code = @"
using System;

class C
{
    [|public D Foo { get; }
    public E Bar { get; }|]
}

namespace A
{
    public class D { }
}

namespace B
{
    public class E { }
}
";

            var expected = @"
using A;
using B;
using System;

class C
{
    public D Foo { get; }
    public E Bar { get; }
}

namespace A
{
    public class D { }
}

namespace B
{
    public class E { }
}
";

            await TestInRegularAndScriptAsync(code, expected, placeSystemNamespaceFirst: false, separateImportDirectiveGroups: false);
        }

        [WpfFact]
        [Trait(Traits.Feature, Traits.Features.AddMissingImports)]
        public async Task AddMissingImports_AddImportsUngrouped_SeparateImportGroupsPasteContainsMultipleMissingImports()
        {
            // The current fixes for AddImport diagnostics do not consider whether imports should be grouped.
            // This test documents this behavior and is a reminder that when the behavior changes 
            // AddMissingImports is also affected and should be considered.

            var code = @"
using System;

class C
{
    [|public D Foo { get; }
    public E Bar { get; }|]
}

namespace A
{
    public class D { }
}

namespace B
{
    public class E { }
}
";

            var expected = @"
using A;
using B;
using System;

class C
{
    public D Foo { get; }
    public E Bar { get; }
}

namespace A
{
    public class D { }
}

namespace B
{
    public class E { }
}
";

            await TestInRegularAndScriptAsync(code, expected, placeSystemNamespaceFirst: false, separateImportDirectiveGroups: true);
        }

        [WpfFact]
        [Trait(Traits.Feature, Traits.Features.AddMissingImports)]
        public async Task AddMissingImports_PartialFix_PasteContainsFixableAndAmbiguousMissingImports()
        {
            var code = @"
class C
{
    [|public D Foo { get; }
    public E Bar { get; }|]
}

namespace A
{
    public class D { }
}

namespace B
{
    public class D { }
    public class E { }
}
";

            var expected = @"
using B;

class C
{
    public D Foo { get; }
    public E Bar { get; }
}

namespace A
{
    public class D { }
}

namespace B
{
    public class D { }
    public class E { }
}
";

            await TestInRegularAndScriptAsync(code, expected);
        }

        [WpfFact]
        [Trait(Traits.Feature, Traits.Features.AddMissingImports)]
        public async Task AddMissingImports_NoAction_NoPastedSpan()
        {
            var code = @"
class C
{
    public D[||] Foo { get; }
}

namespace A
{
    public class D { }
}
";

            await TestMissingInRegularAndScriptAsync(code);
        }

        [WpfFact]
        [Trait(Traits.Feature, Traits.Features.AddMissingImports)]
        public async Task AddMissingImports_NoAction_PasteIsNotMissingImports()
        {
            var code = @"
class [|C|]
{
    public D Foo { get; }
}

namespace A
{
    public class D { }
}
";

            await TestMissingInRegularAndScriptAsync(code);
        }

        [WpfFact]
        [Trait(Traits.Feature, Traits.Features.AddMissingImports)]
        public async Task AddMissingImports_NoAction_PasteContainsAmibiguousMissingImport()
        {
            var code = @"
class C
{
    public [|D|] Foo { get; }
}

namespace A
{
    public class D { }
}

namespace B
{
    public class D { }
}
";

            await TestMissingInRegularAndScriptAsync(code);
        }

        [WorkItem(31768, "https://github.com/dotnet/roslyn/issues/31768")]
        [WpfFact]
        [Trait(Traits.Feature, Traits.Features.AddMissingImports)]
        public async Task AddMissingImports_AddMultipleImports_NoPreviousImports()
        {
            var code = @"
class C
{
    [|public D Foo { get; }
    public E Bar { get; }|]
}

namespace A
{
    public class D { }
}

namespace B
{
    public class E { }
}
";

            var expected = @"
using A;
using B;

class C
{
    public D Foo { get; }
    public E Bar { get; }
}

namespace A
{
    public class D { }
}

namespace B
{
    public class E { }
}
";

            await TestInRegularAndScriptAsync(code, expected, placeSystemNamespaceFirst: false, separateImportDirectiveGroups: false);
        }

        [ExportWorkspaceService(typeof(ISymbolSearchService)), Shared]
        private class TestSymbolSearchService : ISymbolSearchService
        {
            public Task<IList<PackageWithAssemblyResult>> FindPackagesWithAssemblyAsync(string source, string assemblyName, CancellationToken cancellationToken)
            {
                throw new System.NotImplementedException();
            }

            public Task<IList<PackageWithTypeResult>> FindPackagesWithTypeAsync(string source, string name, int arity, CancellationToken cancellationToken)
            {
                throw new System.NotImplementedException();
            }

            public Task<IList<ReferenceAssemblyWithTypeResult>> FindReferenceAssembliesWithTypeAsync(string name, int arity, CancellationToken cancellationToken)
            {
                if (name == "Point")
                {
                    return Task.FromResult((IList<ReferenceAssemblyWithTypeResult>)new List<ReferenceAssemblyWithTypeResult>()
                    {
                        new ReferenceAssemblyWithTypeResult("System.Drawing", "Point", new List<string> { "System.Drawing" })
                    });
                }

                return SpecializedTasks.EmptyList<ReferenceAssemblyWithTypeResult>();
            }
        }

        [ExportWorkspaceService(typeof(IMetadataService)), Shared]
        internal sealed class TestMetadataService : IMetadataService
        {
            PortableExecutableReference IMetadataService.GetReference(string resolvedPath, MetadataReferenceProperties properties)
            {
                return AssemblyMetadata.CreateFromImage(TestResources.NetFX.v4_0_30319.System_Drawing).GetReference();
            }
        }
    }
}
