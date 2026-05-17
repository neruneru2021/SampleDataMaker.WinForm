---
title: ASCIIコード表 - curict 参照メモ
date: 2026-05-17
source: https://www.curict.com/reference/ascii/
tags:
  - reference/ascii
  - character-code
  - imported
---

# ASCIIコード表 - curict 参照メモ

出典: [ASCIIコード表 ASCII Code Table](https://www.curict.com/reference/ascii/)

## 概要

ASCII は、0から127までの7ビット文字コードです。0から31と127は制御文字、32は空白、33から126は印字可能文字です。

表では、各文字について10進、2進、8進、16進、文字種、表示、略語、キャレット表記、説明が整理されています。

> [!note]
> 以下はサイト本文をもとに、Obsidianで参照しやすい形に整えたメモです。標準との照合には RFC 20: ASCII format for Network Interchange も参照しました。

## ASCIIコード表

| Dec | Bin | Oct | Hex | 種別 | 表示/略語 | Caret | 説明 |
|---:|---|---:|---:|---|---|---|---|
| 0 | 000 0000 | 000 | 0x00 | 制御文字 | NUL | ^@ | Null / 空文字・終端 |
| 1 | 000 0001 | 001 | 0x01 | 制御文字 | SOH | ^A | Start of Heading / ヘッダ開始 |
| 2 | 000 0010 | 002 | 0x02 | 制御文字 | STX | ^B | Start of Text / テキスト開始 |
| 3 | 000 0011 | 003 | 0x03 | 制御文字 | ETX | ^C | End of Text / テキスト終了 |
| 4 | 000 0100 | 004 | 0x04 | 制御文字 | EOT | ^D | End of Transmission / 伝送終了 |
| 5 | 000 0101 | 005 | 0x05 | 制御文字 | ENQ | ^E | Enquiry / 問い合わせ |
| 6 | 000 0110 | 006 | 0x06 | 制御文字 | ACK | ^F | Acknowledge / 肯定応答 |
| 7 | 000 0111 | 007 | 0x07 | 制御文字 | BEL | ^G | Bell / 警告音 |
| 8 | 000 1000 | 010 | 0x08 | 制御文字 | BS | ^H | Backspace / 一文字後退 |
| 9 | 000 1001 | 011 | 0x09 | 制御文字 | HT | ^I | Horizontal Tabulation / 水平タブ |
| 10 | 000 1010 | 012 | 0x0A | 制御文字 | LF | ^J | Line Feed / New Line / 改行 |
| 11 | 000 1011 | 013 | 0x0B | 制御文字 | VT | ^K | Vertical Tabulation / 垂直タブ |
| 12 | 000 1100 | 014 | 0x0C | 制御文字 | FF | ^L | Form Feed / New Page / 改ページ |
| 13 | 000 1101 | 015 | 0x0D | 制御文字 | CR | ^M | Carriage Return / 行頭復帰 |
| 14 | 000 1110 | 016 | 0x0E | 制御文字 | SO | ^N | Shift Out |
| 15 | 000 1111 | 017 | 0x0F | 制御文字 | SI | ^O | Shift In |
| 16 | 001 0000 | 020 | 0x10 | 制御文字 | DLE | ^P | Data Link Escape |
| 17 | 001 0001 | 021 | 0x11 | 制御文字 | DC1 | ^Q | Device Control 1 |
| 18 | 001 0010 | 022 | 0x12 | 制御文字 | DC2 | ^R | Device Control 2 |
| 19 | 001 0011 | 023 | 0x13 | 制御文字 | DC3 | ^S | Device Control 3 |
| 20 | 001 0100 | 024 | 0x14 | 制御文字 | DC4 | ^T | Device Control 4 |
| 21 | 001 0101 | 025 | 0x15 | 制御文字 | NAK | ^U | Negative Acknowledge / 否定応答 |
| 22 | 001 0110 | 026 | 0x16 | 制御文字 | SYN | ^V | Synchronous Idle / 同期 |
| 23 | 001 0111 | 027 | 0x17 | 制御文字 | ETB | ^W | End of Transmission Block |
| 24 | 001 1000 | 030 | 0x18 | 制御文字 | CAN | ^X | Cancel |
| 25 | 001 1001 | 031 | 0x19 | 制御文字 | EM | ^Y | End of Medium |
| 26 | 001 1010 | 032 | 0x1A | 制御文字 | SUB | ^Z | Substitute / 文字置換。環境によってEOF扱いされることがある |
| 27 | 001 1011 | 033 | 0x1B | 制御文字 | ESC | ^[ | Escape |
| 28 | 001 1100 | 034 | 0x1C | 制御文字 | FS | ^\ | File Separator |
| 29 | 001 1101 | 035 | 0x1D | 制御文字 | GS | ^] | Group Separator |
| 30 | 001 1110 | 036 | 0x1E | 制御文字 | RS | ^^ | Record Separator |
| 31 | 001 1111 | 037 | 0x1F | 制御文字 | US | ^_ | Unit Separator |
| 32 | 010 0000 | 040 | 0x20 | 空白文字 | SP |  | Space / 空白 |
| 33 | 010 0001 | 041 | 0x21 | 記号 | ! |  | exclamation mark / 感嘆符 |
| 34 | 010 0010 | 042 | 0x22 | 記号 | " |  | quotation mark / double quote |
| 35 | 010 0011 | 043 | 0x23 | 記号 | # |  | number sign |
| 36 | 010 0100 | 044 | 0x24 | 記号 | $ |  | dollar sign |
| 37 | 010 0101 | 045 | 0x25 | 記号 | % |  | percent sign |
| 38 | 010 0110 | 046 | 0x26 | 記号 | & |  | ampersand |
| 39 | 010 0111 | 047 | 0x27 | 記号 | ' |  | apostrophe / single quote |
| 40 | 010 1000 | 050 | 0x28 | 記号 | ( |  | left parenthesis |
| 41 | 010 1001 | 051 | 0x29 | 記号 | ) |  | right parenthesis |
| 42 | 010 1010 | 052 | 0x2A | 記号 | * |  | asterisk |
| 43 | 010 1011 | 053 | 0x2B | 記号 | + |  | plus sign |
| 44 | 010 1100 | 054 | 0x2C | 記号 | , |  | comma |
| 45 | 010 1101 | 055 | 0x2D | 記号 | - |  | hyphen-minus |
| 46 | 010 1110 | 056 | 0x2E | 記号 | . |  | period / full stop |
| 47 | 010 1111 | 057 | 0x2F | 記号 | / |  | slash / slant |
| 48 | 011 0000 | 060 | 0x30 | 数字 | 0 |  | zero |
| 49 | 011 0001 | 061 | 0x31 | 数字 | 1 |  | one |
| 50 | 011 0010 | 062 | 0x32 | 数字 | 2 |  | two |
| 51 | 011 0011 | 063 | 0x33 | 数字 | 3 |  | three |
| 52 | 011 0100 | 064 | 0x34 | 数字 | 4 |  | four |
| 53 | 011 0101 | 065 | 0x35 | 数字 | 5 |  | five |
| 54 | 011 0110 | 066 | 0x36 | 数字 | 6 |  | six |
| 55 | 011 0111 | 067 | 0x37 | 数字 | 7 |  | seven |
| 56 | 011 1000 | 070 | 0x38 | 数字 | 8 |  | eight |
| 57 | 011 1001 | 071 | 0x39 | 数字 | 9 |  | nine |
| 58 | 011 1010 | 072 | 0x3A | 記号 | : |  | colon |
| 59 | 011 1011 | 073 | 0x3B | 記号 | ; |  | semicolon |
| 60 | 011 1100 | 074 | 0x3C | 記号 | &lt; |  | less-than mark |
| 61 | 011 1101 | 075 | 0x3D | 記号 | = |  | equals |
| 62 | 011 1110 | 076 | 0x3E | 記号 | &gt; |  | greater-than mark |
| 63 | 011 1111 | 077 | 0x3F | 記号 | ? |  | question mark |
| 64 | 100 0000 | 100 | 0x40 | 記号 | @ |  | commercial at / at sign |
| 65 | 100 0001 | 101 | 0x41 | アルファベット | A |  | uppercase A |
| 66 | 100 0010 | 102 | 0x42 | アルファベット | B |  | uppercase B |
| 67 | 100 0011 | 103 | 0x43 | アルファベット | C |  | uppercase C |
| 68 | 100 0100 | 104 | 0x44 | アルファベット | D |  | uppercase D |
| 69 | 100 0101 | 105 | 0x45 | アルファベット | E |  | uppercase E |
| 70 | 100 0110 | 106 | 0x46 | アルファベット | F |  | uppercase F |
| 71 | 100 0111 | 107 | 0x47 | アルファベット | G |  | uppercase G |
| 72 | 100 1000 | 110 | 0x48 | アルファベット | H |  | uppercase H |
| 73 | 100 1001 | 111 | 0x49 | アルファベット | I |  | uppercase I |
| 74 | 100 1010 | 112 | 0x4A | アルファベット | J |  | uppercase J |
| 75 | 100 1011 | 113 | 0x4B | アルファベット | K |  | uppercase K |
| 76 | 100 1100 | 114 | 0x4C | アルファベット | L |  | uppercase L |
| 77 | 100 1101 | 115 | 0x4D | アルファベット | M |  | uppercase M |
| 78 | 100 1110 | 116 | 0x4E | アルファベット | N |  | uppercase N |
| 79 | 100 1111 | 117 | 0x4F | アルファベット | O |  | uppercase O |
| 80 | 101 0000 | 120 | 0x50 | アルファベット | P |  | uppercase P |
| 81 | 101 0001 | 121 | 0x51 | アルファベット | Q |  | uppercase Q |
| 82 | 101 0010 | 122 | 0x52 | アルファベット | R |  | uppercase R |
| 83 | 101 0011 | 123 | 0x53 | アルファベット | S |  | uppercase S |
| 84 | 101 0100 | 124 | 0x54 | アルファベット | T |  | uppercase T |
| 85 | 101 0101 | 125 | 0x55 | アルファベット | U |  | uppercase U |
| 86 | 101 0110 | 126 | 0x56 | アルファベット | V |  | uppercase V |
| 87 | 101 0111 | 127 | 0x57 | アルファベット | W |  | uppercase W |
| 88 | 101 1000 | 130 | 0x58 | アルファベット | X |  | uppercase X |
| 89 | 101 1001 | 131 | 0x59 | アルファベット | Y |  | uppercase Y |
| 90 | 101 1010 | 132 | 0x5A | アルファベット | Z |  | uppercase Z |
| 91 | 101 1011 | 133 | 0x5B | 記号 | [ |  | left square bracket |
| 92 | 101 1100 | 134 | 0x5C | 記号 | \ |  | backslash / reverse solidus |
| 93 | 101 1101 | 135 | 0x5D | 記号 | ] |  | right square bracket |
| 94 | 101 1110 | 136 | 0x5E | 記号 | ^ |  | circumflex / caret / hat |
| 95 | 101 1111 | 137 | 0x5F | 記号 | _ |  | underscore |
| 96 | 110 0000 | 140 | 0x60 | 記号 | &#96; |  | grave accent / back quote / backtick |
| 97 | 110 0001 | 141 | 0x61 | アルファベット | a |  | lowercase a |
| 98 | 110 0010 | 142 | 0x62 | アルファベット | b |  | lowercase b |
| 99 | 110 0011 | 143 | 0x63 | アルファベット | c |  | lowercase c |
| 100 | 110 0100 | 144 | 0x64 | アルファベット | d |  | lowercase d |
| 101 | 110 0101 | 145 | 0x65 | アルファベット | e |  | lowercase e |
| 102 | 110 0110 | 146 | 0x66 | アルファベット | f |  | lowercase f |
| 103 | 110 0111 | 147 | 0x67 | アルファベット | g |  | lowercase g |
| 104 | 110 1000 | 150 | 0x68 | アルファベット | h |  | lowercase h |
| 105 | 110 1001 | 151 | 0x69 | アルファベット | i |  | lowercase i |
| 106 | 110 1010 | 152 | 0x6A | アルファベット | j |  | lowercase j |
| 107 | 110 1011 | 153 | 0x6B | アルファベット | k |  | lowercase k |
| 108 | 110 1100 | 154 | 0x6C | アルファベット | l |  | lowercase l |
| 109 | 110 1101 | 155 | 0x6D | アルファベット | m |  | lowercase m |
| 110 | 110 1110 | 156 | 0x6E | アルファベット | n |  | lowercase n |
| 111 | 110 1111 | 157 | 0x6F | アルファベット | o |  | lowercase o |
| 112 | 111 0000 | 160 | 0x70 | アルファベット | p |  | lowercase p |
| 113 | 111 0001 | 161 | 0x71 | アルファベット | q |  | lowercase q |
| 114 | 111 0010 | 162 | 0x72 | アルファベット | r |  | lowercase r |
| 115 | 111 0011 | 163 | 0x73 | アルファベット | s |  | lowercase s |
| 116 | 111 0100 | 164 | 0x74 | アルファベット | t |  | lowercase t |
| 117 | 111 0101 | 165 | 0x75 | アルファベット | u |  | lowercase u |
| 118 | 111 0110 | 166 | 0x76 | アルファベット | v |  | lowercase v |
| 119 | 111 0111 | 167 | 0x77 | アルファベット | w |  | lowercase w |
| 120 | 111 1000 | 170 | 0x78 | アルファベット | x |  | lowercase x |
| 121 | 111 1001 | 171 | 0x79 | アルファベット | y |  | lowercase y |
| 122 | 111 1010 | 172 | 0x7A | アルファベット | z |  | lowercase z |
| 123 | 111 1011 | 173 | 0x7B | 記号 | { |  | left curly bracket |
| 124 | 111 1100 | 174 | 0x7C | 記号 | &#124; |  | vertical bar / vertical line |
| 125 | 111 1101 | 175 | 0x7D | 記号 | } |  | right curly bracket |
| 126 | 111 1110 | 176 | 0x7E | 記号 | ~ |  | tilde |
| 127 | 111 1111 | 177 | 0x7F | 制御文字 | DEL | ^? | Delete character / 削除文字 |

## 印字可能文字

```text
 ! " # $ % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? @ A B C D E F G H I J K L M N O P Q R S T U V W X Y Z [ \ ] ^ _ ` a b c d e f g h i j k l m n o p q r s t u v w x y z { | } ~
```

## 記号のみ

```text
! " # $ % & ' ( ) * + , - . / : ; < = > ? @ [ \ ] ^ _ ` { | } ~
```

## 記事で気になった点

> [!warning] 確認メモ
> コード値そのものに大きな誤りは見つかりませんでした。以下は説明語・表記上の注意です。

- `ENQ` の英語説明がサイトでは `Enquery` になっていますが、RFC 20では `Enquiry` です。これは誤字と見てよさそうです。
- `%` の英語説明が `percent sing` になっていますが、正しくは `percent sign` です。
- `SO` / `SI` の日本語説明が「多バイト文字終了 / 開始」となっていますが、ASCII本来の説明としては不正確です。RFC 20では `SO` は標準集合外のコード解釈へ移る制御、`SI` は標準集合へ戻る制御として説明されています。日本語文字コード文脈の説明としても、これだけだと誤解を招きます。
- `SUB` の `End Of File` は、CP/MやMS-DOS系テキストファイルでの慣習としては見かけますが、ASCII本来の制御文字名は `Substitute` です。標準説明として読む場合は注意が必要です。

## 参考

- [curict: ASCIIコード表](https://www.curict.com/reference/ascii/)
- [RFC 20: ASCII format for Network Interchange](https://www.rfc-editor.org/rfc/rfc20)
