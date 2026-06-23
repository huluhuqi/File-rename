# WPF 自包含版说明

这个版本用于彻底解决 WinUI 3 打包环境问题。

## 解决的问题

WinUI 3 项目会依赖 Windows App SDK 的 PRI/XAML 构建任务。你的打包机当前缺少：

```text
Microsoft.Build.Packaging.Pri.Tasks.dll
```

这属于打包机编译环境问题，不是用户运行环境问题。即使把 .NET 运行时和 Windows App Runtime 打包进安装包，编译阶段仍然会失败。

## WPF 版的变化

| 项目 | WinUI 3 版 | WPF 自包含版 |
|---|---|---|
| 是否需要 Windows App Runtime | 需要 | 不需要 |
| 是否需要 PRI 构建任务 | 需要 | 不需要 |
| 是否需要 Visual Studio Windows 应用开发组件 | 需要 | 不需要 |
| 是否可用 dotnet publish 打包 | 不稳定 | 可以 |
| 用户是否需要安装 .NET | 不需要，自包含 | 不需要，自包含 |

## 打包机需要什么

只需要：

```text
.NET 8 SDK
Inno Setup 6
```

不再需要：

```text
WindowsAppRuntimeInstall-x64.exe
windowsdesktop-runtime-8.0-win-x64.exe
Visual Studio PRI 任务
```

## 打包命令

双击：

```text
installer\Build-Installer.cmd
```

生成结果：

```text
artifacts\installer\FileRenameAssistant_1.0.0_WPF_Setup.exe
```
