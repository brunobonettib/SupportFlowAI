# Recalcula exclusivamente artefatos sintéticos locais; não chama nenhum modelo.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$review = Get-Content -Raw -Encoding UTF8 (Join-Path $root 'docs/quality-text-review.json') | ConvertFrom-Json

function Get-ClassMetrics($rows, [string[]]$labels, [string]$expectedProperty, [string]$outputProperty) {
    $matrix = [ordered]@{}
    foreach ($label in $labels) {
        $columns = [ordered]@{}
        foreach ($prediction in ($labels + @('SemResposta'))) { $columns[$prediction] = 0 }
        $matrix[$label] = $columns
    }
    foreach ($row in $rows) {
        $prediction = 'SemResposta'
        if ($null -ne $row.analysis -and $null -eq $row.errorCode) { $prediction = $row.analysis.$outputProperty }
        $matrix[$row.$expectedProperty][$prediction]++
    }
    $scores = [ordered]@{}
    foreach ($label in $labels) {
        $support = ($matrix[$label].Values | Measure-Object -Sum).Sum
        $predicted = 0
        foreach ($actual in $labels) { $predicted += $matrix[$actual][$label] }
        $tp = $matrix[$label][$label]
        $precision = $null; $recall = $null
        if ($predicted -gt 0) { $precision = $tp / $predicted }
        if ($support -gt 0) { $recall = $tp / $support }
        $scores[$label] = [ordered]@{support=$support;predicted=$predicted;truePositive=$tp;precision=$precision;recall=$recall}
    }
    return [ordered]@{byClass=$scores;confusionMatrix=$matrix}
}

$summaries = @()
foreach ($entry in $review.reports) {
    $path = Join-Path $root ('docs/' + $entry.file)
    $report = Get-Content -Raw -Encoding UTF8 $path | ConvertFrom-Json
    $rows = @($report.cases)
    $validRows = @($rows | Where-Object { $null -ne $_.analysis -and $null -eq $_.errorCode })
    $categoryCorrect = @($validRows | Where-Object {$_.analysis.categoria -eq $_.expectedCategory}).Count
    $priorityCorrect = @($validRows | Where-Object {$_.analysis.prioridade -eq $_.expectedPriority}).Count
    if ($rows.Count -ne 15) { throw 'Comparação espera quinze casos por relatório.' }
    foreach ($id in @($entry.unsupportedDraftIds) + @($entry.unsupportedDeliveredIds)) {
        if ($validRows.id -notcontains $id) { throw 'Anotação refere caso sem resposta revisável.' }
    }
    $category = Get-ClassMetrics $rows @('Entrega','Pagamento','Cancelamento','Reembolso','Outro') 'expectedCategory' 'categoria'
    $priority = Get-ClassMetrics $rows @('Baixa','Media','Alta') 'expectedPriority' 'prioridade'
    $rank = @{Baixa=0;Media=1;Alta=2}
    $under = @($validRows | Where-Object {$rank[$_.analysis.prioridade] -lt $rank[$_.expectedPriority]}).Count
    $over = @($validRows | Where-Object {$rank[$_.analysis.prioridade] -gt $rank[$_.expectedPriority]}).Count
    $mean = ($rows | Measure-Object -Property latencyMs -Average).Average
    # Conferência independente com as métricas gravadas pelo avaliador C#.
    if ($categoryCorrect -ne $report.metrics.categoryCorrect -or $priorityCorrect -ne $report.metrics.priorityCorrect -or
        [Math]::Abs($mean - $report.metrics.averageLatencyMs) -gt 0.001) { throw 'Divergência no recálculo.' }
    $summaries += [ordered]@{
        name=$entry.name;source=$entry.file;sourceSha256=(Get-FileHash $path).Hash.ToLowerInvariant()
        model=$report.modelConfigured;prompt=$report.promptVersion;datasetSha256=$report.datasetSha256
        attempted=$rows.Count;valid=$validRows.Count;categoryCorrect=$categoryCorrect;priorityCorrect=$priorityCorrect
        categoryAccuracy=$categoryCorrect/$rows.Count;priorityAccuracy=$priorityCorrect/$rows.Count
        validResponseRate=$validRows.Count/$rows.Count;averageLatencyMs=$mean
        highRecall=$priority.byClass['Alta'].recall
        highUnderestimated=@($validRows | Where-Object {$_.expectedPriority -eq 'Alta' -and $_.analysis.prioridade -ne 'Alta'}).Count
        falseHigh=@($validRows | Where-Object {$_.expectedPriority -ne 'Alta' -and $_.analysis.prioridade -eq 'Alta'}).Count
        underestimated=$under;overestimated=$over;categories=$category;priorities=$priority
        textReviewed=$validRows.Count;unavailableText=$rows.Count-$validRows.Count
        unsupportedDrafts=@($entry.unsupportedDraftIds).Count;unsupportedDelivered=@($entry.unsupportedDeliveredIds).Count
        controlledText=$entry.controlledText;reviewNotes=$entry.notes
    }
}
$result = [ordered]@{generatedAtUtc=[DateTimeOffset]::UtcNow.ToString('O');reviewer=$review.reviewer;reports=$summaries}
$result | ConvertTo-Json -Depth 15 | Set-Content -Encoding UTF8 (Join-Path $root 'docs/quality-comparison.json')
$summaries | ForEach-Object { [pscustomobject]@{name=$_.name;category=$_.categoryCorrect;priority=$_.priorityCorrect;highRecall=$_.highRecall;unsupportedDrafts=$_.unsupportedDrafts;unsupportedDelivered=$_.unsupportedDelivered} }
