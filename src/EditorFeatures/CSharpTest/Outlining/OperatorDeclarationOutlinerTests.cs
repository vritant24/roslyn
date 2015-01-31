﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editor.CSharp.Outlining;
using Microsoft.CodeAnalysis.Editor.Implementation.Outlining;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Test.Utilities;
using Roslyn.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Outlining
{
    public class OperatorDeclarationOutlinerTests :
        AbstractOutlinerTests<OperatorDeclarationSyntax>
    {
        internal override IEnumerable<OutliningSpan> GetRegions(OperatorDeclarationSyntax operatorDecl)
        {
            var outliner = new OperatorDeclarationOutliner();
            return outliner.GetOutliningSpans(operatorDecl, CancellationToken.None).WhereNotNull();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Outlining)]
        public void TestOperator()
        {
            var tree = ParseLines("class C",
                                        "{",
                                        "  public static int operator ++(int i)",
                                        "  {",
                                        "  }",
                                        "}");

            var typeDecl = tree.DigToFirstTypeDeclaration();
            var operatorDecl = typeDecl.DigToFirstNodeOfType<OperatorDeclarationSyntax>();

            var actualRegion = GetRegion(operatorDecl);
            var expectedRegion = new OutliningSpan(
                TextSpan.FromBounds(50, 60),
                TextSpan.FromBounds(14, 60),
                CSharpOutliningHelpers.Ellipsis,
                autoCollapse: true);

            AssertRegion(expectedRegion, actualRegion);
        }

        [Fact,
         Trait(Traits.Feature, Traits.Features.Outlining)]
        public void TestOperatorWithLeadingComments()
        {
            var tree = ParseLines("class C",
                                        "{",
                                        "  // Foo",
                                        "  // Bar",
                                        "  public static int operator ++(int i)",
                                        "  {",
                                        "  }",
                                        "}");

            var typeDecl = tree.DigToFirstTypeDeclaration();
            var operatorDecl = typeDecl.DigToFirstNodeOfType<OperatorDeclarationSyntax>();

            var actualRegions = GetRegions(operatorDecl).ToList();
            Assert.Equal(2, actualRegions.Count);

            var expectedRegion1 = new OutliningSpan(
                TextSpan.FromBounds(14, 30),
                "// Foo ...",
                autoCollapse: true);

            AssertRegion(expectedRegion1, actualRegions[0]);

            var expectedRegion2 = new OutliningSpan(
                TextSpan.FromBounds(70, 80),
                TextSpan.FromBounds(34, 80),
                CSharpOutliningHelpers.Ellipsis,
                autoCollapse: true);

            AssertRegion(expectedRegion2, actualRegions[1]);
        }
    }
}
