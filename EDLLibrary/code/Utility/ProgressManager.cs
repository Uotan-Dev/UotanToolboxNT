using System;
using System.Threading;

namespace EDLLibrary.code.Utility
{
    public static class ProgressManager
    {
        private static float progress;
        private static long processed;
        private static long totalSize;
        private static readonly object lockObject = new();

        /// <summary>
        /// 获取当前进度百分比（0-100）
        /// </summary>
        public static float Progress
        {
            get
            {
                lock (lockObject)
                {
                    return progress;
                }
            }
        }

        /// <summary>
        /// 获取已处理的块数
        /// </summary>
        public static long ProcessedBlocks
        {
            get
            {
                lock (lockObject)
                {
                    return processed;
                }
            }
        }

        /// <summary>
        /// 获取总块数
        /// </summary>
        public static long TotalBlocks
        {
            get
            {
                lock (lockObject)
                {
                    return totalSize;
                }
            }
        }

        /// <summary>
        /// 进度更新事件
        /// </summary>
        public static event EventHandler<ProgressEventArgs> ProgressUpdated;

        /// <summary>
        /// 获取当前进度信息
        /// </summary>
        /// <returns>返回当前进度事件参数</returns>
        public static ProgressEventArgs GetCurrentProgress()
        {
            lock (lockObject)
            {
                return new ProgressEventArgs(progress, processed, totalSize);
            }
        }

        /// <summary>
        /// 触发进度更新事件
        /// </summary>
        private static void OnProgressUpdated(ProgressEventArgs e)
        {
            var handler = Volatile.Read(ref ProgressUpdated);
            handler?.Invoke(null, e);
        }

        /// <summary>
        /// 更新进度信息
        /// </summary>
        internal static void UpdateProgress(float? newProgress = null, long? newProcessed = null, long? newTotalSize = null, string status = null)
        {
            lock (lockObject)
            {
                if (newProgress.HasValue)
                {
                    if (newProgress.Value < 0 || newProgress.Value > 100)
                    {
                        throw new ArgumentOutOfRangeException(nameof(newProgress), "Progress must be between 0 and 100");
                    }
                    progress = newProgress.Value;
                }

                if (newProcessed.HasValue)
                {
                    if (newProcessed.Value < 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(newProcessed), "Processed value cannot be negative");
                    }
                    processed = newProcessed.Value;
                }

                if (newTotalSize.HasValue)
                {
                    if (newTotalSize.Value <= 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(newTotalSize), "Total size must be positive");
                    }
                    totalSize = newTotalSize.Value;
                }

                // 如果有processed和totalSize，自动计算进度百分比
                if (totalSize > 0)
                {
                    progress = (float)processed * 100 / totalSize;
                }

                OnProgressUpdated(new ProgressEventArgs(progress, processed, totalSize, status));
            }
        }

        /// <summary>
        /// 更新已处理的块数
        /// </summary>
        internal static void IncrementProcessedBlocks(long increment = 1)
        {
            lock (lockObject)
            {
                processed += increment;
                if (totalSize > 0)
                {
                    progress = (float)processed * 100 / totalSize;
                }
                OnProgressUpdated(new ProgressEventArgs(progress, processed, totalSize));
            }
        }

        /// <summary>
        /// 重置进度信息
        /// </summary>
        public static void Reset()
        {
            lock (lockObject)
            {
                progress = 0;
                processed = 0;
                totalSize = 0;
                OnProgressUpdated(new ProgressEventArgs(0, 0, 0, "Reset"));
            }
        }
    }

    /// <summary>
    /// 进度事件参数类
    /// </summary>
    /// <remarks>
    /// 初始化进度事件参数的新实例
    /// </remarks>
    /// <param name="progress">当前进度（0-100）</param>
    /// <param name="processed">已处理数量</param>
    /// <param name="totalSize">总数量</param>
    /// <param name="status">当前状态描述</param>
    public class ProgressEventArgs(float progress, long processed, long totalSize, string status = null) : EventArgs
    {
        /// <summary>
        /// 获取当前进度百分比（0-100）
        /// </summary>
        public float Progress { get; } = progress;

        /// <summary>
        /// 获取已处理的数量
        /// </summary>
        public long Processed { get; } = processed;

        /// <summary>
        /// 获取总数量
        /// </summary>
        public long TotalSize { get; } = totalSize;

        /// <summary>
        /// 获取当前状态描述
        /// </summary>
        public string Status { get; } = status;
    }
}