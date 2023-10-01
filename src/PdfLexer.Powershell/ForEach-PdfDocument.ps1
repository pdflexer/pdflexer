function ForEach-PdfDocument {
    [CmdletBinding()]
    param(
      [Parameter(ValueFromPipeline)]
      [Object] $InputObject
      ,
      [Parameter(Position=0)]
      [ScriptBlock] $ScriptBlock
    )

    process {
      $doc = Open-PdfDocument $InputObject;
      try {
        # hack to use script scope
        ForEach-Object -InputObject $doc -Process $ScriptBlock
      } finally {
        $doc.Dispose();
      }
    }
  }