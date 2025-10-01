Imports System.Data
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser
Imports SMRUCC.Rsharp.Language.TokenIcer

Module SyntaxTree

    Public Function BuildExpression(blocks As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
        If blocks > 3 Then
            If blocks(1).Length = 1 AndAlso blocks(1)(0) = (TokenType.operator, "=") Then
                ' is symbol assigned
                Return SymbolValueAssigned(symbol:=blocks(0)(0), blocks.Skip(2), opts)
            End If

            Return BinaryTree.ParseBinaryExpression(blocks, opts)
        ElseIf blocks = 1 AndAlso blocks(0).Length = 1 Then
            Dim value As Token = blocks(0)(0)

            Select Case value.name
                Case TokenType.identifier,
                     TokenType.keyword

                    Return New SymbolReference(value.text)
                Case TokenType.integerLiteral,
                     TokenType.missingLiteral,
                     TokenType.numberLiteral,
                     TokenType.stringLiteral,
                     TokenType.booleanLiteral

                    Return New Literal(value)
                Case Else
                    Throw New SyntaxErrorException($"{value.text}/{value.name}/")
            End Select
        End If

        Throw New SyntaxErrorException(blocks.IteratesALL.Select(Function(t) $"{t.text}/{t.name}/").JoinBy(" "))
    End Function

    Public Function SymbolValueAssigned(symbol As Token, valueTokens As IEnumerable(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
        Dim value As SyntaxResult = SyntaxTree.BuildExpression(valueTokens.AsList, opts)
        Dim assign As ValueAssignExpression

        If value.isException Then
            Return value
        Else
            assign = New ValueAssignExpression(symbol.text, value.expression)
        End If

        Return assign
    End Function
End Module
