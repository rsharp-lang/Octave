Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Octave

Module syntaxParserTest

    Sub run()
        Call run("x = 3 # value 3 is assigned to symbol x")
        Call run("x = 3 % value 3 is assigned to symbol x")
        Call run("
%{
value 3 is assigned to symbol x
%}
x = 3")
    End Sub

    Private Sub run(script As String)
        Dim scanner As New Scanner(script)
        Dim tokens = scanner.GetTokens.ToArray
        Dim highlight As New ConsoleFormat(AnsiColor.BrightBlue, Underline:=True)
        Dim strs As String() = tokens.Select(Function(t) $"{t.text}/{New TextSpan(t.name.ToString, highlight)}/").ToArray

        Call Console.WriteLine(strs.JoinBy(" "))
    End Sub
End Module
