Imports System.Net.Mime.MediaTypeNames
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Public Class Scanner

    Dim code As CharPtr
    Dim buffer As New CharBuffer
    Dim escape As New Escapes

    ''' <summary>
    ''' 当前的代码行号
    ''' </summary>
    Dim lineNumber As Integer = 1


    Sub New(source As [Variant](Of String, CharPtr))
        If source Like GetType(String) Then
            Me.code = source.TryCast(Of String).SolveStream.LineTokens.JoinBy(ASCII.LF)
        Else
            Me.code = source.TryCast(Of CharPtr)
        End If
    End Sub

    Public Overridable Iterator Function GetTokens() As IEnumerable(Of Token)
        Dim token As New Value(Of Token)
        Dim start As Integer = 0

        Do While Not code
            If Not (token = walkChar(++code)) Is Nothing Then
                Yield CType(token, Token)
            End If
        Loop

        If buffer > 0 Then
            Yield getToken(Nothing)
        End If
    End Function

    Shared ReadOnly shortOperators As Index(Of Char) = {"+"c, "-"c, "*"c, "/"c, "\"c, "^"c, ":"c, ";"c}
    Shared ReadOnly whitespace As Index(Of Char) = {ASCII.TAB, " "c, ASCII.CR, ASCII.LF}

    Private Function walkChar(c As Char) As Token
        If c = ASCII.LF Then
            lineNumber += 1
        End If

        If escape.comment Then
            If c = ASCII.CR OrElse c = ASCII.LF Then
                If escape.isBlockComment Then
                    buffer += c
                Else
                    Dim commentText As New String(buffer.PopAllChars)

                    escape.reset()

                    Return New Token With {
                        .name = TokenType.comment,
                        .text = commentText
                    }
                End If
            ElseIf c = "{"c AndAlso (buffer = "%"c OrElse buffer = "#"c) Then
                escape.isBlockComment = True
                buffer += c
            Else
                Dim last As Char = buffer.GetLastOrDefault
                Dim peek As Char = code.Current

                If (last = ASCII.CR OrElse last = ASCII.LF) AndAlso (c = escape.stringEscape AndAlso peek = "}"c) Then
                    ' end of block comment
                    buffer += c
                    buffer += ++code

                    Dim commentText As New String(buffer.PopAllChars)

                    escape.reset()

                    Return New Token With {
                        .name = TokenType.comment,
                        .text = commentText
                    }
                Else
                    buffer += c
                End If
            End If
        ElseIf c = "#"c OrElse c = "%"c AndAlso buffer = 0 Then
            escape.comment = True
            escape.stringEscape = c
            buffer += c
        ElseIf c = "#"c OrElse c = "%"c Then
            ' follow with the expression
            Dim token As Token = getToken(Nothing)

            escape.stringEscape = c
            escape.comment = True
            buffer += c

            Return token
        ElseIf c Like whitespace Then
            Return getToken(Nothing)
        ElseIf c Like shortOperators Then
            Dim token As Token = getToken(Nothing)
            buffer += c
            Return token
        ElseIf c = "." AndAlso buffer.isInteger Then
            ' xx.
            buffer += c
            Return Nothing
        Else
            buffer += c
        End If

        Return Nothing
    End Function

    Const identifier As String = "^[a-zA-Z][a-zA-Z0-9_]*$"

    Private Function getToken(Optional bufferNext As Char? = Nothing) As Token
        If buffer = 0 Then
            If Not bufferNext Is Nothing Then
                buffer += bufferNext
            End If

            Return Nothing
        End If

        Dim text As New String(buffer.PopAllChars)

        If escape.comment Then
            Return New Token With {.name = TokenType.comment, .text = text}
        End If

        Select Case text
            Case "+", "-", "*", "=", "/", "\", ">", "<", "~", "<=", ">="
                Return New Token With {
                    .text = text,
                    .name = TokenType.operator
                }
        End Select

        If text.IsPattern(identifier) Then
            Return New Token With {
                .text = text,
                .name = TokenType.identifier
            }
        ElseIf text.IsPattern("\d+") Then
            Return New Token With {.name = TokenType.integerLiteral, .text = text}
        ElseIf text.IsNumeric Then
            Return New Token With {.name = TokenType.numberLiteral, .text = text}
        End If

        Return New Token With {
            .name = TokenType.invalid,
            .text = text
        }
    End Function
End Class
