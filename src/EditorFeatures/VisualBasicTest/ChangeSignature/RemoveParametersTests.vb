' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis.Editor.UnitTests.ChangeSignature
Imports Microsoft.CodeAnalysis.Editor.UnitTests.Extensions

Namespace Microsoft.CodeAnalysis.Editor.VisualBasic.UnitTests.ChangeSignature
    Partial Public Class ChangeSignatureTests
        Inherits AbstractChangeSignatureTests

        <Fact, Trait(Traits.Feature, Traits.Features.ChangeSignature)>
        Public Sub RemoveParameters1()

            Dim markup = <Text><![CDATA[
Module Program
    ''' <summary>
    ''' See <see cref="M(String, Integer, String, Boolean, Integer, String)"/>
    ''' </summary>
    ''' <param name="o">o!</param>
    ''' <param name="a">a!</param>
    ''' <param name="b">b!</param>
    ''' <param name="c">c!</param>
    ''' <param name="x">x!</param>
    ''' <param name="y">y!</param>
    <System.Runtime.CompilerServices.Extension>
    Sub $$M(ByVal o As String, a As Integer, b As String, c As Boolean, Optional x As Integer = 0, Optional y As String = "Zero")
        Dim t = "Test"

        M(t, 1, "Two", True, 3, "Four")
        t.M(1, "Two", True, 3, "Four")

        M(t, 1, "Two", True, 3)
        M(t, 1, "Two", True)

        M(t, 1, "Two", True, 3, y:="Four")
        M(t, 1, "Two", c:=True)

        M(t, 1, "Two", True, y:="Four")
        M(t, 1, "Two", True, x:=3)

        M(t, 1, "Two", True, y:="Four", x:=3)
        M(t, 1, y:="Four", x:=3, b:="Two", c:=True)
        M(t, y:="Four", x:=3, c:=True, b:="Two", a:=1)
        M(y:="Four", x:=3, c:=True, b:="Two", a:=1, o:=t)
    End Sub
End Module

]]></Text>.NormalizedValue()
            Dim permutation = {0, 3, 1, 5}
            Dim updatedCode = <Text><![CDATA[
Module Program
    ''' <summary>
    ''' See <see cref="M(String, Boolean, Integer, String)"/>
    ''' </summary>
    ''' <param name="o">o!</param>
    ''' <param name="c">c!</param>
    ''' <param name="a">a!</param>
    ''' <param name="y">y!</param>
    ''' 
    ''' 
    <System.Runtime.CompilerServices.Extension>
    Sub M(ByVal o As String, c As Boolean, a As Integer, Optional y As String = "Zero")
        Dim t = "Test"

        M(t, True, 1, "Four")
        t.M(True, 1, "Four")

        M(t, True, 1)
        M(t, True, 1)

        M(t, True, 1, y:="Four")
        M(t, c:=True, a:=1)

        M(t, True, 1, y:="Four")
        M(t, True, 1)

        M(t, True, 1, y:="Four")
        M(t, a:=1, y:="Four", c:=True)
        M(t, y:="Four", c:=True, a:=1)
        M(y:="Four", c:=True, a:=1, o:=t)
    End Sub
End Module

]]></Text>.NormalizedValue()

            TestChangeSignatureViaCommand(LanguageNames.VisualBasic, markup, updatedSignature:=permutation, expectedUpdatedInvocationDocumentCode:=updatedCode)

        End Sub
    End Class
End Namespace
