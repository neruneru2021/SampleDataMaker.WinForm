---
title: SampleDataMaker 外部キー設定の双方向化
date: 2026-05-18
tags:
  - csharp
  - SampleDataMaker
  - foreign-key
  - work-log
---

# SampleDataMaker 外部キー設定の双方向化

## 対象

- `SampleDataMaker.WinForm.ViewModels.ConnectionOperationViewModel`
- `SampleDataMaker.xUnit.ViewModelTests.ConnectionOperationViewModelTests`

## 目的

外部キー設定画面で関係を追加・解除したとき、設定元カラムと参照先カラムの両方向に外部キー関係を反映する。

## 追加仕様

### 外部キー設定時

次の2件を保存する。

- 外部キー設定をした側のカラム -> 外部キー設定をされた側のカラム
- 外部キー設定をされた側のカラム -> 外部キー設定をした側のカラム

例:

```text
USERS.CLINIC_ID -> CLINICS.CLINIC_ID
CLINICS.CLINIC_ID -> USERS.CLINIC_ID
```

### 外部キー解除時

次の2件を削除する。

- 外部キー設定をした側のカラム -> 外部キー設定をされた側のカラム
- 外部キー設定をされた側のカラム -> 外部キー設定をした側のカラム

## 実装内容

`ConnectionOperationViewModel.SaveForeignKeySettings` を修正した。

- 保存前に、対象カラムを参照元に持つ既存設定を取得。
- 対象カラムを参照元に持つ設定を削除。
- 対象カラムの既存設定に対応する逆方向設定も削除。
- 新しく確定された設定を、正方向と逆方向の2件に展開して保存。
- 同じ関係が重複しないよう、外部キー関係キーで `DistinctBy` する。

追加した補助処理:

- `IsSourceColumn`
- `CreateReverseSetting`
- `IsSameRelation`
- `CreateRelationKey`

## テスト更新

`ConnectionOperationViewModelTests` を更新した。

- 外部キー保存時に、正方向・逆方向の2件が保存されることを検証。
- 外部キー解除時に、正方向・逆方向の2件が削除され、無関係な外部キー設定は残ることを検証。

## 検証

```powershell
dotnet test SampleDataMaker.xUnit\SampleDataMaker.xUnit.csproj
```

結果:

- 合格: 24
- 失敗: 0
- スキップ: 0

> [!note]
> `ViewModelBase` の null 許容性 warning は既存の警告。今回の改修によるテスト失敗はなし。
