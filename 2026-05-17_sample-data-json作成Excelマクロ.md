---
title: sample-data.json作成Excelマクロ
date: 2026-05-17
tags:
  - csharp
  - SampleDataMaker
  - excel
  - vba
  - sample-data
---

# sample-data.json作成Excelマクロ

## 目的

Excelに入力したサンプルデータ一覧から、SampleDataMaker.WinForm が使用する `sample-data.json` を作成する。

出力先:

```text
C:\Works\SampleDataMaker\SampleDataMaker.WinForm\SampleDataMaker.WinForm\master-data\sample-data.json
```

既存の `sample-data.json` は以下の形式。

```json
[
  {
    "kind": "名前",
    "value": "佐藤"
  },
  {
    "kind": "名前",
    "value": "伊藤"
  }
]
```

## Excel入力仕様

- 1行目に `kind` を入力する。
- 2行目以降に `value` を入力する。
- 同じ列の `value` は、その列1行目の `kind` に属する。
- 1行目が空の列は出力対象外。
- 2行目以降が空のセルは出力対象外。

例:

| A列 | B列 |
|---|---|
| 名前 | 電話番号 |
| 佐藤 | 090-8888-8888 |
| 伊藤 | 080-1111-2222 |

出力:

```json
[
  {
    "kind": "名前",
    "value": "佐藤"
  },
  {
    "kind": "名前",
    "value": "伊藤"
  },
  {
    "kind": "電話番号",
    "value": "090-8888-8888"
  },
  {
    "kind": "電話番号",
    "value": "080-1111-2222"
  }
]
```

> [!note]
> マクロはセルの表示文字列を使います。日付や電話番号など、Excel側で表示形式を整えてから実行してください。列幅が狭く `####` 表示になっている場合は、その表示のまま出力されるため、列幅を広げてから実行してください。

## VBAマクロ

標準モジュールに貼り付けて使用する。

```vb
Option Explicit

Private Const SAMPLE_DATA_JSON_PATH As String = _
    "C:\Works\SampleDataMaker\SampleDataMaker.WinForm\SampleDataMaker.WinForm\master-data\sample-data.json"

Public Sub ExportSampleDataJson()
    Dim ws As Worksheet
    Dim lastColumn As Long
    Dim lastRow As Long
    Dim columnIndex As Long
    Dim rowIndex As Long
    Dim kind As String
    Dim value As String
    Dim json As String
    Dim isFirstItem As Boolean

    Set ws = ActiveSheet
    lastColumn = ws.Cells(1, ws.Columns.Count).End(xlToLeft).Column
    lastRow = GetLastUsedRow(ws)

    json = "[" & vbCrLf
    isFirstItem = True

    For columnIndex = 1 To lastColumn
        kind = Trim$(CStr(ws.Cells(1, columnIndex).Value))

        If Len(kind) > 0 Then
            For rowIndex = 2 To lastRow
                value = Trim$(ws.Cells(rowIndex, columnIndex).Text)

                If Len(value) > 0 Then
                    If Not isFirstItem Then
                        json = json & "," & vbCrLf
                    End If

                    json = json & _
                        "  {" & vbCrLf & _
                        "    ""kind"": """ & EscapeJson(kind) & """," & vbCrLf & _
                        "    ""value"": """ & EscapeJson(value) & """" & vbCrLf & _
                        "  }"

                    isFirstItem = False
                End If
            Next rowIndex
        End If
    Next columnIndex

    json = json & vbCrLf & "]" & vbCrLf

    WriteUtf8Text SAMPLE_DATA_JSON_PATH, json

    MsgBox "sample-data.jsonを作成しました。" & vbCrLf & SAMPLE_DATA_JSON_PATH, vbInformation
End Sub

Private Function GetLastUsedRow(ByVal ws As Worksheet) As Long
    Dim foundCell As Range

    Set foundCell = ws.Cells.Find( _
        What:="*", _
        After:=ws.Cells(1, 1), _
        LookIn:=xlFormulas, _
        LookAt:=xlPart, _
        SearchOrder:=xlByRows, _
        SearchDirection:=xlPrevious, _
        MatchCase:=False)

    If foundCell Is Nothing Then
        GetLastUsedRow = 1
    Else
        GetLastUsedRow = foundCell.Row
    End If
End Function

Private Function EscapeJson(ByVal text As String) As String
    Dim result As String

    result = text
    result = Replace(result, "\", "\\")
    result = Replace(result, """", "\""")
    result = Replace(result, vbCrLf, "\n")
    result = Replace(result, vbCr, "\n")
    result = Replace(result, vbLf, "\n")
    result = Replace(result, vbTab, "\t")

    EscapeJson = result
End Function

Private Sub WriteUtf8Text(ByVal filePath As String, ByVal text As String)
    Dim stream As Object

    Set stream = CreateObject("ADODB.Stream")

    With stream
        .Type = 2
        .Charset = "UTF-8"
        .Open
        .WriteText text
        .SaveToFile filePath, 2
        .Close
    End With
End Sub
```

## 使い方

1. Excelを開く。
2. `Alt + F11` でVBAエディタを開く。
3. `挿入` > `標準モジュール` を選ぶ。
4. 上記のVBAコードを貼り付ける。
5. シートの1行目に `kind`、2行目以降に `value` を入力する。
6. `ExportSampleDataJson` を実行する。

## 注意点

- 既存の `sample-data.json` は上書きされる。
- 空の `kind` 列は出力されない。
- 空の `value` セルは出力されない。
- 値はすべてJSON文字列として出力される。
- `"`、`\`、改行、タブはJSON用にエスケープされる。
