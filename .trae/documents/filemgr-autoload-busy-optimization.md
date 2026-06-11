# 文件管理页面自动加载与 Busy 动画优化计划

## 摘要

修复文件管理页面的两个问题：
1. 切换到文件管理页面且设备已连接时，自动加载内部存储目录；在文件管理页面时设备连接也自动加载
2. Busy 动画触发时机延迟，点击按钮后过一段时间才出现

## 现状分析

### 自动加载逻辑

当前 `FilemgrViewModel` 已有自动加载框架：
- `TryAutoLoadAsync()` — 自动加载 `/sdcard`，但受 `_hasAutoLoaded` 标志保护，只允许加载一次
- `OnDeviceAdded` — 设备添加时，若当前页面活跃则调用 `TryAutoLoadAsync()`
- `OnMainViewModelPropertyChanged` — 页面切换时，若切换到本页面则调用 `TryAutoLoadAsync()`
- `InitializeAsync` — 初始化时检查设备状态

**问题**：`_hasAutoLoaded` 一旦设为 `true` 就永不重置。当设备断开再重连时，不会重新自动加载。此外，当设备断开时文件列表应清空以避免显示过期数据。

### Busy 动画时机

当前 `LoadDirectoryAsync` 在方法开头就设置了 `IsBusy = true`（第336行），理论上应该立即显示。但存在以下问题：

1. **`IsBusy` 属性变更通知可能不在 UI 线程**：`LoadDirectoryAsync` 是 async 方法，`IsBusy = true` 的赋值发生在调用线程上。如果调用来自非 UI 线程的事件处理器（如 `OnDeviceAdded`、`OnMainViewModelPropertyChanged`），属性变更通知可能无法立即反映到 UI。
2. **其他命令未设置 IsBusy**：`NewFileAsync`、`NewFolderAsync`、`RenameFileAsync`、`DeleteFileAsync`、`PasteFileAsync` 等命令执行耗时操作但未设置 `IsBusy = true`，导致用户点击后无任何反馈。
3. **Push/Pull 类命令的 IsBusy 设置过晚**：`PushFileAsync`、`PushFolderAsync`、`PullFileAsync` 等在文件选择对话框之后才设置 `IsBusy = true`，虽然文件选择期间不需要 busy，但 ADB 命令执行期间需要。

## 修改方案

### 文件：`UotanToolbox/Features/Filemgr/FilemgrViewModel.cs`

#### 1. 设备断开时重置自动加载状态

- 订阅 `DeviceManager.DeviceRemoved` 事件
- 在设备断开时重置 `_hasAutoLoaded = false`、`IsDeviceConnected = false`
- 清空文件列表，重置路径为根目录

```csharp
// 构造函数中添加
if (Global.DeviceManager != null)
{
    Global.DeviceManager.DeviceAdded += OnDeviceAdded;
    Global.DeviceManager.DeviceRemoved += OnDeviceRemoved;  // 新增
}

// 新增方法
private void OnDeviceRemoved(object? sender, DeviceEventArgs e)
{
    _hasAutoLoaded = false;
    IsDeviceConnected = false;
    Dispatcher.UIThread.Post(() =>
    {
        Files.Clear();
        CurrentPath = "/";
        PathInput = "/";
    });
}
```

#### 2. 确保 IsBusy 在 UI 线程上设置

将 `LoadDirectoryAsync` 中的 `IsBusy = true` 改为通过 `Dispatcher.UIThread.Post` 确保 UI 线程执行，使 busy 动画立即显示：

```csharp
[RelayCommand]
public async Task LoadDirectoryAsync(string path)
{
    // 在 UI 线程上立即设置 Busy 状态
    await Dispatcher.UIThread.InvokeAsync(() => IsBusy = true);

    // ... 后续逻辑不变
}
```

#### 3. 为缺少 IsBusy 的命令添加 Busy 状态

为以下命令添加 `IsBusy` 管理：
- `NewFileAsync` — ADB 命令执行期间
- `NewFolderAsync` — ADB 命令执行期间
- `RenameFileAsync` — ADB 命令执行期间
- `DeleteFileAsync` — 确认对话框后、ADB 命令执行期间
- `PasteFileAsync` — ADB 命令执行期间

模式：在 ADB 命令调用前设置 `IsBusy = true`，在 finally 中设置 `IsBusy = false`。

#### 4. 优化事件处理器中的线程安全

`OnDeviceAdded` 和 `OnMainViewModelPropertyChanged` 中的 `TryAutoLoadAsync()` 调用可能来自非 UI 线程，需确保线程安全：

```csharp
private void OnDeviceAdded(object? sender, DeviceEventArgs e)
{
    if (IsActivePage())
    {
        _ = Task.Run(async () =>
        {
            await CheckDeviceConnectionAsync();
            if (IsDeviceConnected)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await TryAutoLoadAsync();
                });
            }
        });
    }
}
```

#### 5. 页面切换时重新检查设备状态

`OnMainViewModelPropertyChanged` 中，当切换到文件管理页面时，应先重新检查设备连接状态再决定是否自动加载：

```csharp
private void OnMainViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
{
    if (e.PropertyName == nameof(MainViewModel.ActivePage) && IsActivePage())
    {
        _ = TryAutoLoadAsync();
    }
}
```

这已经可以工作，因为 `TryAutoLoadAsync` 内部会调用 `CheckDeviceConnectionAsync`。

## 假设与决策

1. **假设**：SukiUI 的 `BusyArea` 控件本身没有内置延迟，问题出在属性变更通知的线程调度上
2. **决策**：不修改 `BusyArea` 控件本身（属于 SukiUI 库），仅通过确保 `IsBusy` 在 UI 线程上设置来解决延迟问题
3. **决策**：设备断开时清空文件列表并重置路径，避免显示过期数据
4. **决策**：`_hasAutoLoaded` 在设备断开时重置，而非每次页面切换时重置，避免频繁自动加载

## 验证步骤

1. 启动应用，连接设备，切换到文件管理页面 → 应自动加载 `/sdcard` 目录
2. 在文件管理页面，断开设备 → 文件列表应清空，路径重置
3. 在文件管理页面，重新连接设备 → 应自动加载 `/sdcard` 目录
4. 先在文件管理页面，再连接设备 → 应自动加载
5. 点击刷新、新建文件/文件夹、重命名、删除、粘贴等按钮 → Busy 动画应立即出现
6. 点击目录导航按钮 → Busy 动画应立即出现
