Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser
Imports SMRUCC.Rsharp.Language.TokenIcer

Module BinaryTree

    ''' <summary>
    ''' the math operators
    ''' </summary>
    ReadOnly operatorPriority As String() = {"^", "*/%", "+-"}

    <Extension>
    Public Function ParseBinaryExpression(tokenBlocks As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
        Dim buf As New List(Of [Variant](Of SyntaxResult, String))
        Dim oplist As New List(Of String)
        Dim lineNum As Integer = tokenBlocks(Scan0)(Scan0).span.line
        Dim index As i32 = 1

        For i As Integer = Scan0 To tokenBlocks.Count - 1
            If ++index Mod 2 = 0 Then
                ' should be an operator token
                Call buf.Add(tokenBlocks(i)(0).text)
                Call oplist.Add(buf.Last.VB)
            Else
                Dim result = opts.ParseExpression(tokenBlocks(i))

                If result.isException Then
                    Return result
                Else
                    Call buf.Add(result)
                End If
            End If
        Next

        Return buf.ParseBinaryExpression(opts, oplist:=oplist, lineNum:=lineNum)
    End Function

    <Extension>
    Public Function ParseBinaryExpression(buf As List(Of [Variant](Of SyntaxResult, String)),
                                          opts As SyntaxBuilderOptions,
                                          Optional oplist As List(Of String) = Nothing,
                                          Optional lineNum As Integer = -1) As SyntaxResult

        Dim result As New Value(Of SyntaxResult)

        If buf = 1 Then
            Return buf(Scan0).TryCast(Of SyntaxResult)
        Else
            oplist = If(oplist, New List(Of String))
        End If

        ' 算数操作符以及字符串操作符按照操作符的优先度进行构建
        If Not (result = buf.ProcessOperators(oplist, operatorPriority, test:=Function(op, o) op.IndexOf(o) > -1, opts)) Is Nothing Then
            Return result
        End If

        If buf > 1 Then
            Return SyntaxResult.CreateError("error while parse binary expression!", opts)
        Else
            Return buf(Scan0)
        End If
    End Function
End Module
