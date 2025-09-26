Imports System.Net.Mime.MediaTypeNames
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
            Me.code = source.TryCast(Of String).SolveStream
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

                    Return New Token With {
                        .name = TokenType.comment,
                        .text = commentText
                    }
                End If
            Else
                buffer += c
            End If
        ElseIf c = "#"c OrElse c = "%"c AndAlso buffer = 0 Then
            escape.comment = True
            buffer += c
        ElseIf c = "#"c OrElse c = "%"c Then
            ' follow with the expression
            Dim token As Token = getToken(Nothing)

            escape.comment = True
            buffer += c

            Return token
        End If

        Return Nothing
    End Function

    Private Function getToken(Optional bufferNext As Char? = Nothing) As Token
        If buffer = 0 Then
            If Not bufferNext Is Nothing Then
                buffer += bufferNext
            End If

            Return Nothing
        End If

        Dim text As New String(buffer.PopAllChars)

        If escape.comment Then
            Return New Token With {.name = TokenType.comment, .text = Text}
        End If

        Return New Token With {
            .name = TokenType.invalid,
            .text = text
        }
    End Function
End Class
