# Avatar Parameters Driver

VRC Parameter Driverを便利に使うコンポーネント

![Avatar Parameters Driver](docs~/AvatarParametersDriver.png)

コンポーネントに設定するだけでアニメーションの条件遷移とVRC Avatar Parameter Driverが生成されます。

アバター衣装の依存関係制御などにご活用下さい。

手動アニメーション編集やAvatarMenuCreatorなどでは面倒だった、水着と服同時に出さないみたいなパラメーターの依存関係が簡単に設定出来たりします。

NDMF・Modular Avatarを利用しています。

## インストール

### VCC用インストーラーunitypackageによる方法（VRChatプロジェクトおすすめ）

https://github.com/Narazaka/AvatarParametersDriver/releases/latest から `net.narazaka.vrchat.avatar-parameters-driver-installer.zip` をダウンロードして解凍し、対象のプロジェクトにインポートする。

### VCCによる方法

0. https://modular-avatar.nadena.dev/ja から「ダウンロード（VCC経由）」ボタンを押してリポジトリをVCCにインストールします。
1. [https://vpm.narazaka.net/](https://vpm.narazaka.net/?q=net.narazaka.vrchat.avatar-parameters-driver) から「Add to VCC」ボタンを押してリポジトリをVCCにインストールします。
2. VCCでSettings→Packages→Installed Repositoriesの一覧中で「Narazaka VPM Listing」にチェックが付いていることを確認します。
3. アバタープロジェクトの「Manage Project」から「Avatar Parameters Driver」をインストールします。

## 使い方

アバター内のGameObjectに「Add Component」ボタンなどから「Avatar Parameters Driver」コンポーネントを付けて設定します。

## 更新履歴

- 3.4.1
  - シーン上の邪魔なアイコンがデフォルトで無効になるように
- 3.4.0
  - 「戻り条件を設定しない（実験的）」機能を追加。ONにすると条件が成立している間パラメーターが設定され続けます。
- 3.3.0
  - アイコンが付きました <img src="Icons/AvatarParametersDriver.png" width="18" height="18">
- 3.2.0
  - 日本語化
- 3.1.3
  - 更新履歴URL等をマニフェストに追加
- 3.1.2
  - Greater / Less のラベルが`>=`等になっていたのを意味的に正しい`>`に修正
  - IntパラメーターについてGreater / Lessを設定したときに正しく戻るように修正
- 3.1.1
  - Drive ParameterのCopyの変数指定UIが見た目と逆になっていた（destination→sourceになっていた）バグの修正
- 3.1.0
  - 事前条件設定（PreConditions）追加
    - 通常は `idle`→（Conditionsを満たす）→`active`（VRCAvatarParameterDriver動作）→（Conditionsの逆を満たす）→`idle` というステートマシンになります。
    - `UsePreCondition`が有効だと `idle`→（PreConditionsを満たす）→`pre_active`→（Conditionsを満たす）→`active`（VRCAvatarParameterDriver動作）→（Conditionsの逆を満たす）→`idle` というステートマシンになります。
- 3.0.0
  - NDMF Parameter Provider対応
  - VCCのバグで非互換の依存関係をインストールできてしまう問題があるために緊急的にリリースとした物です。安定性が低い可能性があります。
- 3.0.0-rc.1
  - ビルドの問題を修正
- 3.0.0-rc.0
  - NDMF Parameter Provider対応
- 2.0.2
  - VCCでのUnity 2022プロジェクトへのインストールでUnityバージョン警告がでないように
  - 依存関係を更新
- 2.0.1
  - 依存関係を更新
- 2.0.0
  - 外部連携APIの変更
    - Avatar Menu Creator for MA は 1.9.2 以降にアップグレードして下さい (1.9.1以前だとパラメーターが正しく取れません)
    - Avatar Parameters Exclusive Group は 0.2.0 以降にアップグレードして下さい  (0.1.2以前だとパラメーターが正しく取れません)
- 1.2.3
  - Animator内にTriggerがあるとエラーになる問題を修正
- 1.2.2
  - Avatar Optimizerの警告を削減
- 1.2.1
  - 複数条件を指定した場合2回目の遷移が正しくならない問題を修正
- 1.2.0
  - 外部連携ができるようにAPIを整理
- 1.1.0
  - パラメーターを外部申告できるように
- 1.0.5
  - パラメーターキャッシュ更新タイミングを修正
- 1.0.3
  - Animatorなどにあるパラメーターを含める
- 1.0.0
  - リリース

## License

[Zlib License](LICENSE.txt)
