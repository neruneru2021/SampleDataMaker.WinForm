---
title: SampleDataMaker 通常サンプルデータ仕様変更
date: 2026-05-17
tags:
  - csharp
  - SampleDataMaker
  - test-data
  - work-log
---

# SampleDataMaker 通常サンプルデータ仕様変更

## 対象

- `SampleDataMaker.Domain.Services.SimpleTestDataGenerator`
- `SampleDataMaker.Domain.Services.ITestDataGenerator`
- `SampleDataMaker.xUnit.DomainTests.SimpleTestDataGeneratorTests`

## 変更内容

- `SimpleTestDataGenerator` と `ITestDataGenerator` に XML ドキュメントコメントを追加。
- 通常サンプルデータのデフォルト値生成を、固定値から型別の連番値へ変更。
- `Generate` 開始時に値生成ファクトリの状態をリセットし、日付型用の日本時間開始時刻を確定するように変更。
- テストから日時を固定できるように、`SimpleTestValueFactory` に内部コンストラクタを追加。

## 新しい生成仕様

### 文字列型

- `varchar` / `char` / `varchar2` などは `Fixed` 扱い。
- `nvarchar` / `nchar` / `nvarchar2` などは `Adjustable` 扱い。
- 通常は `1-Fixed-VARCHAR(100)` や `1-Adjustable-NVARCHAR(100)` の形式で生成。
- カラムサイズに入らない場合は、指定仕様に沿って段階的に短い表記へフォールバック。
- SQL Server の `nvarchar` 系は `MaxLength` がバイト数で取得されるため、文字数として扱う際に 2 で割る。

### 数値型

- 整数系は `1`, `2`, `3` の連番。
- 小数系は `1.001`, `2.001`, `3.001` のように、小数最小桁に `1` を入れる。
- `NumericScale` がある場合は、その桁数を使う。

### 日付型

- テストデータ生成処理の開始時刻を日本時間で使用。
- 形式は `yyyy-MM-dd HH:mm:ss`。
- 同じ `Generate` 呼び出しの中では同じ時刻を使う。

### バイナリ型

- `0x01`, `0x02`, `0x03` のように項番ごとにカウントアップ。

## テスト更新

- 既存のデフォルト値テストを新仕様の期待値へ更新。
- 文字列型のフォールバック仕様を確認するテストを追加。
- 小数型とバイナリ型の連番仕様を確認するテストを追加。

## 検証

```powershell
dotnet test SampleDataMaker.xUnit\SampleDataMaker.xUnit.csproj
```

結果:

- 合格: 23
- 失敗: 0
- スキップ: 0

> [!note]
> 既存の `ViewModelBase` に null 許容性の警告が出ていますが、今回の変更による新規エラーではありません。
