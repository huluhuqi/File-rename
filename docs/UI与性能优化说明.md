# UI 与性能优化说明

## UI 设计原则

v2.0 使用“左侧规则、右侧摘要、下方预览”的稳定结构，不再频繁改变页面布局。用户的主要视线集中在规则配置和预览结果上。

## 自动刷新机制

所有规则属性通过 `SetAndRefresh` 进入统一刷新入口。

```text
用户输入
↓
属性变化
↓
DebouncedPreviewScheduler 延迟 250ms
↓
后台构建预览
↓
UI 线程一次性更新列表
```

## 为什么不每个字符立即刷新

几千到几万个文件时，每个字符都立即刷新会造成列表抖动和 CPU 占用上升。防抖刷新能明显减少无效计算。

## 动画策略

动效只用于反馈状态变化，不做复杂炫技动画。v2.0 主要使用：

- `EntranceThemeTransition`
- `RepositionThemeTransition`
- `ContentThemeTransition`

这些动画由 WinUI 原生支持，性能开销较低。
