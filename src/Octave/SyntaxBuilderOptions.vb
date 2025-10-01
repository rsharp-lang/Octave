Imports Microsoft.VisualBasic.Text.Parser
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser
Imports SMRUCC.Rsharp.Language.TokenIcer

Public Class OctaveSyntaxBuilderOptions : Inherits SyntaxBuilderOptions

    Public Overrides Function ParseExpression(tokens As IEnumerable(Of Token)) As SyntaxResult
        Throw New NotImplementedException()
    End Function

    Public Overrides Function NewScanner(buffer As CharPtr, stringInterpolateParser As Boolean) As IScanner
        Return New Scanner(buffer)
    End Function
End Class
