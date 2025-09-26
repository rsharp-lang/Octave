Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser
Imports SMRUCC.Rsharp.Language.TokenIcer

Public Module ProgramBuilder

    Public Iterator Function CreateProgram(tokens As IEnumerable(Of Token)) As IEnumerable(Of Expression)
        For Each line As Token() In tokens.SplitLines
            Dim blocks = line.SplitByTopLevelDelimiter(TokenType.close, , ")")
            Dim expr As Expression = SyntaxTree.BuildExpression(blocks)

            Yield expr
        Next
    End Function

    <Extension>
    Public Iterator Function SplitLines(tokens As IEnumerable(Of Token)) As IEnumerable(Of Token())
        Dim split = tokens _
            .Split(Function(a)
                       Return a.name = TokenType.newLine OrElse
                           a.name = TokenType.terminator
                   End Function, DelimiterLocation.PreviousLast) _
            .ToArray
        Dim buffer As New List(Of Token)

        For Each line As Token() In split
            line = line.Where(Function(t) t.name <> TokenType.newLine).ToArray

            If line.Length > 0 Then
                If line.Last.name = TokenType.lineContinue Then
                    Call buffer.AddRange(line)
                Else
                    If buffer > 0 Then
                        If buffer.Last.name <> TokenType.lineContinue Then
                            Yield buffer _
                                .PopAll _
                                .Where(Function(a) a.name <> TokenType.lineContinue) _
                                .ToArray
                        End If
                    End If

                    Call buffer.AddRange(line)
                End If
            End If
        Next

        If buffer > 0 Then
            Yield buffer _
                .PopAll _
                .Where(Function(a) a.name <> TokenType.lineContinue) _
                .ToArray
        End If
    End Function

End Module
