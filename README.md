# FrameDebuggerSave
## 導入方法
コチラのリポジトリを、プロジェクト直下にあるPackageManagerフォルダ以下に配置してください。

## 動作確認環境
Unity 2018.4 / Unity 2019.3<br/>
Windows Editor実行時、及びWindowsStandへ繋げた時の実行

## 利用方法
Menuの「Tools/FrameDebuggerSave」を押してください。<br/>
FrameDebuggerウィンドウが立ち上がり、勝手に走査してデータを収集します。<br/>
収集した結果をファイルに自動書き出しを行います<br />

Profiler等で繋いでいる先の情報先から勝手に収集するようになっています。<br />

プロジェクト直下に「FrameDebugger」というフォルダを作成し、CSVの書き出しを行います。<br />
※今後、ShaderのパラメーターをJsonで書き出すなどを予定しているため、一つフォルダを掘っています。
