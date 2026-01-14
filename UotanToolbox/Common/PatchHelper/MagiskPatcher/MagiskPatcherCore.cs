using System;
using System.Collections.Generic;
namespace MagiskPatcher
{
    // 配置数据传递对象
    public class PatcherConfig
    {
        public string? MagiskZipPath { get; set; }
        public string? OrigFilePath { get; set; }
        public string? NewFilePath { get; set; }
        public string? WorkDir { get; set; }
        public string? ZipToolPath { get; set; }
        public string? MagiskbootPath { get; set; }
        public string? CsvConfPath { get; set; }
        public bool? InstFullMagsikAPP { get; set; }
        public string? ChkNewFileSize { get; set; }
        public bool? CleanupAfterComplete { get; set; }
        public string? SaveSomeOutputInfoToBat { get; set; }
        public string? CpuType { get; set; }
        public bool? Flag_KEEPVERITY { get; set; }
        public bool? Flag_KEEPFORCEENCRYPT { get; set; }
        public bool? Flag_RECOVERYMODE { get; set; }
        public bool? Flag_PATCHVBMETAFLAG { get; set; }
        public bool? Flag_LEGACYSAR { get; set; }
        public string? Flag_PREINITDEVICE { get; set; }

        public ValidationResult Validate()
        {
            var errors = new List<string>();
            if (string.IsNullOrEmpty(MagiskZipPath)) errors.Add("MagiskZipPath is required.");
            if (string.IsNullOrEmpty(OrigFilePath)) errors.Add("OrigFilePath is required.");
            return new ValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors
            };
        }
    }

    public record ValidationResult
    {
        public bool IsValid { get; init; } = true;
        public List<string> Errors { get; init; } = [];
    }

    // 返回类型
    public record PatchDetails
    {
        public string? MagiskVersion { get; init; }
        public uint OriginalFileSize { get; init; }
        public uint NewFileSize { get; init; }
        public string? NewFilePath { get; init; }
    }

    public record PatchResult
    {
        public bool IsSuccess { get; init; }
        public string? Message { get; init; }
        public string? ErrorMessage { get; init; }
        public Exception? Exception { get; init; }
        public PatchDetails? Details { get; init; }
    }

    public class MagiskPatcherCore
    {
        public static IMagiskPatcherLogger? Logger { get; set; }

        private readonly PatcherConfig _config;

        public MagiskPatcherCore(PatcherConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public PatchResult Patch()
        {
            try
            {
                // 将配置写入 Patcher 的静态字段以供现有 Patcher 使用
                if (!string.IsNullOrEmpty(_config.MagiskZipPath)) Patcher.MagiskZipPath = _config.MagiskZipPath;
                if (!string.IsNullOrEmpty(_config.OrigFilePath)) Patcher.OrigFilePath = _config.OrigFilePath;
                if (!string.IsNullOrEmpty(_config.NewFilePath)) Patcher.NewFilePath = _config.NewFilePath;
                if (!string.IsNullOrEmpty(_config.WorkDir)) Patcher.WorkDir = _config.WorkDir;
                if (!string.IsNullOrEmpty(_config.ZipToolPath)) Patcher.ZipToolPath = _config.ZipToolPath;
                if (!string.IsNullOrEmpty(_config.MagiskbootPath)) Patcher.MagiskbootPath = _config.MagiskbootPath;
                if (!string.IsNullOrEmpty(_config.CsvConfPath)) Patcher.CsvConfPath = _config.CsvConfPath;
                if (_config.InstFullMagsikAPP.HasValue) Patcher.InstFullMagsikAPP = _config.InstFullMagsikAPP;
                if (!string.IsNullOrEmpty(_config.ChkNewFileSize)) Patcher.ChkNewFileSize = _config.ChkNewFileSize;
                if (_config.CleanupAfterComplete.HasValue) Patcher.CleanupAfterComplete = _config.CleanupAfterComplete;
                if (!string.IsNullOrEmpty(_config.SaveSomeOutputInfoToBat)) Patcher.SaveSomeOutputInfoToBat = _config.SaveSomeOutputInfoToBat;

                if (!string.IsNullOrEmpty(_config.CpuType)) Patcher.CpuType = _config.CpuType;
                if (_config.Flag_KEEPVERITY.HasValue) Patcher.Flag_KEEPVERITY = _config.Flag_KEEPVERITY;
                if (_config.Flag_KEEPFORCEENCRYPT.HasValue) Patcher.Flag_KEEPFORCEENCRYPT = _config.Flag_KEEPFORCEENCRYPT;
                if (_config.Flag_RECOVERYMODE.HasValue) Patcher.Flag_RECOVERYMODE = _config.Flag_RECOVERYMODE;
                if (_config.Flag_PATCHVBMETAFLAG.HasValue) Patcher.Flag_PATCHVBMETAFLAG = _config.Flag_PATCHVBMETAFLAG;
                if (_config.Flag_LEGACYSAR.HasValue) Patcher.Flag_LEGACYSAR = _config.Flag_LEGACYSAR;
                if (!string.IsNullOrEmpty(_config.Flag_PREINITDEVICE)) Patcher.Flag_PREINITDEVICE = _config.Flag_PREINITDEVICE;

                Patcher.Run();

                return new PatchResult
                {
                    IsSuccess = true,
                    Message = "Patch completed",
                    Details = new PatchDetails
                    {
                        MagiskVersion = Patcher.MagiskVersion,
                        OriginalFileSize = Patcher.OriginalFileSize,
                        NewFileSize = Patcher.PatchedFileSize,
                        NewFilePath = Patcher.NewFilePath
                    }
                };
            }
            catch (Exception ex)
            {
                return new PatchResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    Exception = ex
                };
            }
        }
    }
}
