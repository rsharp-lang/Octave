Imports System.Data
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser
Imports SMRUCC.Rsharp.Language.TokenIcer

Public Module ProgramBuilder

    Public Function CreateProgram(tokens As IEnumerable(Of Token), opts As SyntaxBuilderOptions) As Program
        Dim lines As New List(Of Expression)

        For Each line As SyntaxResult In tokens.CreateProgramInternal(opts)
            If line.isException Then
                Throw New SyntaxErrorException(line.error.ToString)
            Else
                Call lines.Add(line.expression)
            End If
        Next

        Return New Program(lines)
    End Function

    <Extension>
    Private Iterator Function CreateProgramInternal(tokens As IEnumerable(Of Token), opts As SyntaxBuilderOptions) As IEnumerable(Of SyntaxResult)
        For Each line As Token() In tokens.SplitLines
            Yield ParseExpression(line, opts)
        Next
    End Function

    Public Function ParseExpression(tokens As IEnumerable(Of Token), opts As SyntaxBuilderOptions) As SyntaxResult
        Dim blocks As List(Of Token()) = tokens.ToArray _
            .TrimTerminator _
            .SplitByTopLevelDelimiter(TokenType.operator, includeKeyword:=True)
        Dim expr As SyntaxResult = SyntaxTree.BuildExpression(blocks, opts)

        Return expr
    End Function

    <Extension>
    Public Function TrimTerminator(line As Token()) As IEnumerable(Of Token)
        If line.Any Then
            If line.Last = (TokenType.terminator, ";") Then
                Return line.Take(line.Length - 1).ToArray.TrimTerminator
            End If
        End If

        Return line
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
