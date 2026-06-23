# 文件重命名助手 v1.0

一款基于 WPF + .NET 8 的 Windows 桌面批量文件重命名工具。支持链式规则组合、多条件排序、Excel 模板导入导出、规则持久化与一键撤销。

## 功能概览

| 模块 | 说明 |
|------|------|
| **文件导入** | 支持单选/多选文件、选择文件夹，可选递归导入子目录 |
| **多条件排序** | 按文件名（自然排序）、扩展名、文件大小、创建日期、修改日期排序，支持升序/降序与优先级链 |
| **链式重命名规则** | 查找替换（支持正则）、添加前缀/后缀、删除文字、自动编号、全大写/全小写/首字母大写/单词首字母大写 |
| **修改对象切换** | 可单独修改文件名或扩展名，互不影响 |
| **实时预览** | 输入规则后自动刷新预览，支持双击编辑新文件名 |
| **选择性执行** | 可只执行预览列表中选中的文件 |
| **Excel 模板** | 导出当前文件列表为 Excel 模板，编辑后导入实现批量自定义命名 |
| **规则保存与加载** | 将当前规则链保存到本地 SQLite，随时加载复用 |
| **操作历史与撤销** | 每次重命名自动记录历史，支持一键撤销 |
| **冲突检测** | 自动检测空文件名、非法字符、路径冲突、目标已存在等问题 |

## 技术栈

- **UI 框架**：WPF (.NET 8)
- **打包方式**：.NET 自包含发布 + Inno Setup 6
- **数据持久化**：SQLite (Microsoft.Data.Sqlite)
- **Excel 处理**：ClosedXML
- **架构模式**：MVVM

## 项目结构

```
src/FileRenameAssistant/
├── MainWindow.xaml / .cs          主界面
├── App.xaml                       全局样式与按钮动画
├── ViewModels/
│   ├── MainViewModel.cs           核心业务逻辑
│   ├── ObservableObject.cs        MVVM 基类
│   ├── SortConditionViewModel.cs  排序条件 VM
│   └── RenameRuleStepViewModel.cs 重命名步骤 VM
├── Models/                        数据模型（FileItem、PreviewItem、RuleProfile 等）
├── Rules/                         重命名规则实现（IRenameRule 接口及具体规则）
├── Services/                      业务服务（扫描、排序、预览、执行、Excel、冲突检测）
├── Data/                          数据访问（SQLite 规则库与历史记录）
└── Assets/Icons/                  应用图标与按钮图标

installer/
├── FileRenameAssistant.iss        Inno Setup 安装脚本
├── Build-Release.ps1              一键构建发布脚本
└── ChineseSimplified.local.isl    中文安装界面语言文件
```

## 快速开始

### 环境要求

- Windows 10/11 (x64)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Inno Setup 6](https://jrsoftware.org/isdl.php)（如需打包安装程序）

### 本地运行

```bash
cd src/FileRenameAssistant
dotnet run
```

### 构建发布版

```powershell
cd installer
.\Build-Release.ps1
```

脚本会自动完成：
1. 清理旧构建产物
2. 还原 NuGet 包
3. 发布自包含 WPF 应用（win-x64）
4. 调用 Inno Setup 生成安装包

输出位置：`artifacts/installer/文件重命名助手_1.0.0_安装包.exe`

## 核心设计

### 规则链引擎

重命名规则基于 `IRenameRule` 接口实现，支持任意组合：

```csharp
public interface IRenameRule
{
    string Name { get; }
    string Apply(string fileNameWithoutExtension, RenameContext context);
}
```

`RuleChainEngine` 按顺序应用规则，每一步保留中间结果用于调试追踪。

### 多条件排序

排序条件按列表从上到下确定优先级，内部使用 `ThenBy` 链式调用实现多关键字排序。文件名采用自然排序（`NaturalStringComparer`），正确处理数字前缀如 `file2.txt` < `file10.txt`。

### 冲突检测

`ConflictDetector` 在预览阶段自动检测：
- 空文件名
- Windows 非法字符
- 原文件不存在
- 多个文件生成相同目标路径
- 目标路径已被占用

### 撤销机制

执行重命名时采用**临时文件中转**策略：
1. 所有文件先移动到临时路径（GUID 命名）
2. 再从临时路径移动到最终目标路径

这样即使中途失败，也不会出现部分文件已改名、部分未改名的不一致状态。撤销时按历史记录反向恢复。

## 安装包特性

- **自包含**：无需用户安装 .NET Runtime
- **中文界面**：安装向导完整中文化
- **卸载清理**：卸载时自动删除安装目录、本地数据、注册表项、桌面快捷方式
- **Defender 排除**：安装时自动将安装目录加入 Windows Defender 排除项（卸载时移除）
- **关闭运行中程序**：安装/卸载前自动关闭正在运行的实例

## 许可证

MIT License
