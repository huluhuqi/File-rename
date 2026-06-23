# Inno 中文语言包缺失修复说明

## 问题

打包时报错：

```text
Couldn't open include file "C:\Program Files (x86)\Inno Setup 6\Languages\ChineseSimplified.isl"
```

原因是当前电脑安装的 Inno Setup 6 没有携带官方简体中文语言文件。

## 修复

当前版本已新增：

```text
installer\ChineseSimplified.local.isl
```

并把安装脚本语言配置改为：

```text
MessagesFile: "compiler:Default.isl,ChineseSimplified.local.isl"
```

这样会先使用 Inno Setup 自带的 `Default.isl`，再用项目内置的中文语言文件覆盖主要安装界面文案。

## 好处

不再依赖：

```text
C:\Program Files (x86)\Inno Setup 6\Languages\ChineseSimplified.isl
```

换一台电脑打包也不需要额外安装中文语言包。
