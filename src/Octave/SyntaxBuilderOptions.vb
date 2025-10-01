Imports Microsoft.VisualBasic.Text.Parser
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Public Class OctaveSyntaxBuilderOptions : Inherits SyntaxBuilderOptions

    Sub New(octave As Rscript)
        source = octave
    End Sub

    Public Overrides Function ParseExpression(tokens As IEnumerable(Of Token)) As SyntaxResult
        Return ProgramBuilder.ParseExpression(tokens, Me)
    End Function

    Public Overrides Function NewScanner(buffer As CharPtr, stringInterpolateParser As Boolean) As IScanner
        Return New Scanner(buffer)
    End Function
End Class
