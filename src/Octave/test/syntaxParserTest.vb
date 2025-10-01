Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Octave
Imports SMRUCC.Rsharp.Runtime.Components

Module syntaxParserTest

    Sub run()
        '        Call run("x = 3 # value 3 is assigned to symbol x")
        '        Call run("x = 3 % value 3 is assigned to symbol x")
        '        Call run("
        '%{

        '%}
        'x = 3.")
        Call run("x = 1.36*(2+3. ...
        +5)+6;")
        ' Call run("disp('hello world!');")
        Call run("disp(['a' 'b' 'c']);")
    End Sub

    Private Sub run(script As String)
        Dim scanner As New Scanner(script)
        Dim tokens = scanner.GetTokens.ToArray
        Dim highlight As New ConsoleFormat(AnsiColor.BrightBlue, Underline:=True)
        Dim strs As String() = tokens _
            .Select(Function(t) $"{t.text}/{New TextSpan(t.name.ToString, highlight) & AnsiEscapeCodes.Reset}/") _
            .ToArray

        Call Console.WriteLine(strs.JoinBy(" "))

        Dim program = ProgramBuilder.CreateProgram(tokens, New OctaveSyntaxBuilderOptions(Rscript.AutoHandleScript(script)))



    End Sub
End Module
