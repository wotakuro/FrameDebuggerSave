# FrameDebuggerSave

## About
This tool arrows you to capture the information of "FrameDebugger" and view the information.
Also this tool supports to generate ShaderVariantCollection assets from captured data.

![alt text](image/ReporterAb.png)


## Install
Clone this project to the "PackageManager" Folder at UnityProject.

## Enviroment
Unity 2018.4 / Unity 2019.3<br/>
Windows Editor and Player connected by FrameDebugger<br/ >
*Player mode doesn't supports to capture ShaderTexture and some futures.

## How to Use
Select "Tools/FrameDebuggerSave" from Menu <br/>
FrameDebuggerウィンドウが立ち上がり、勝手に走査してデータを収集します。<br/>
収集した結果をファイルに自動書き出しを行います<br />

Profiler等で繋いでいる先の情報先から勝手に収集するようになっています。<br />

プロジェクト直下に「FrameDebugger」というフォルダを作成し、CSVの書き出しを行います。<br />
※今後、ShaderのパラメーターをJsonで書き出すなどを予定しているため、一つフォルダを掘っています。
