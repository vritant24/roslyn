﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editor.CSharp.Outlining;
using Microsoft.CodeAnalysis.Editor.Implementation.Outlining;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Test.Utilities;
using Roslyn.Utilities;
using Xunit;
using MaSOutliners = Microsoft.CodeAnalysis.Editor.CSharp.Outlining.MetadataAsSource;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Outlining.MetadataAsSource
{
    public class IndexerDeclarationOutlinerTests :
        AbstractOutlinerTests<IndexerDeclarationSyntax>
    {
        internal override IEnumerable<OutliningSpan> GetRegions(IndexerDeclarationSyntax node)
        {
            var outliner = new MaSOutliners.IndexerDeclarationOutliner();
            return outliner.GetOutliningSpans(node, CancellationToken.None).WhereNotNull();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.MetadataAsSource)]
        public void NoCommentsOrAttributes()
        {
            var tree = ParseCode(
@"class Foo
{
    public string this[int x] { get; set; }
}");
            var typeDecl = tree.DigToFirstTypeDeclaration();
            var indexer = typeDecl.DigToFirstNodeOfType<IndexerDeclarationSyntax>();

            Assert.Empty(GetRegions(indexer));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.MetadataAsSource)]
        public void WithAttributes()
        {
            var tree = ParseCode(
@"class Foo
{
    [Foo]
    public string this[int x] { get; set; }
}");
            var typeDecl = tree.DigToFirstTypeDeclaration();
            var indexer = typeDecl.DigToFirstNodeOfType<IndexerDeclarationSyntax>();

            var actualRegion = GetRegion(indexer);
            var expectedRegion = new OutliningSpan(
                TextSpan.FromBounds(18, 29),
                TextSpan.FromBounds(18, 68),
                CSharpOutliningHelpers.Ellipsis,
                autoCollapse: true);

            AssertRegion(expectedRegion, actualRegion);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.MetadataAsSource)]
        public void WithCommentsAndAttributes()
        {
            var tree = ParseCode(
@"class Foo
{
    // Summary:
    //     This is a summary.
    [Foo]
    string this[int x] { get; set; }
}");
            var typeDecl = tree.DigToFirstTypeDeclaration();
            var indexer = typeDecl.DigToFirstNodeOfType<IndexerDeclarationSyntax>();

            var actualRegion = GetRegion(indexer);
            var expectedRegion = new OutliningSpan(
                TextSpan.FromBounds(18, 77),
                TextSpan.FromBounds(18, 109),
                CSharpOutliningHelpers.Ellipsis,
                autoCollapse: true);

            AssertRegion(expectedRegion, actualRegion);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.MetadataAsSource)]
        public void WithCommentsAttributesAndmodifiers()
        {
            var tree = ParseCode(
@"class Foo
{
    // Summary:
    //     This is a summary.
    [Foo]
    public string this[int x] { get; set; }
}");
            var typeDecl = tree.DigToFirstTypeDeclaration();
            var indexer = typeDecl.DigToFirstNodeOfType<IndexerDeclarationSyntax>();

            var actualRegion = GetRegion(indexer);
            var expectedRegion = new OutliningSpan(
                TextSpan.FromBounds(18, 77),
                TextSpan.FromBounds(18, 116),
                CSharpOutliningHelpers.Ellipsis,
                autoCollapse: true);

            AssertRegion(expectedRegion, actualRegion);
        }
    }
}
