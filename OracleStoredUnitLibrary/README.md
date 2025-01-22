# StoredUnitLibrary

## 概要

Oracleデータベースで作成された、PL/SQLのファンクション、プロシージャをテスト実行するためのライブラリ。

## 使用方法

1. 設定ファイルを作成 <br/>
   - AppSettings.json.templateをプロジェクトルートにコピー
   - 必要な項目を記載

   例：
   ``` json:AppSettings.json
	{
		"ConnectionStrings": {
			"DefaultConnection": "User Id=yuta;Password=password;Data Source=//localhost:1521/FREEPDB1;Connection Timeout=60;"
		}
	}
   ```

2. テストクラスを作成
   - IClassFixtureにStoredUnitFictureを指定し継承する
   - コンストラクタ内でfixtureを取得

3. テスト実行
   - テストエクスプローラーを開き実行する

