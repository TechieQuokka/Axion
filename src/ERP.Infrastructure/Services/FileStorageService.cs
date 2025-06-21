using ERP.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ERP.Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly ILogger<FileStorageService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _baseStoragePath;

        // 허용되는 파일 확장자
        private readonly HashSet<string> _allowedExtensions;
        private readonly long _maxFileSize;

        public FileStorageService(
            ILogger<FileStorageService> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            // 기본 저장소 경로 설정
            _baseStoragePath = _configuration.GetValue<string>("FileStorage:BasePath")
                ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            // 설정에서 허용 확장자와 최대 파일 크기 읽기
            _allowedExtensions = new HashSet<string>(
                _configuration.GetSection("FileStorage:AllowedExtensions").Get<string[]>()
                ?? new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt" },
                StringComparer.OrdinalIgnoreCase);

            _maxFileSize = _configuration.GetValue<long>("FileStorage:MaxFileSizeBytes", 10 * 1024 * 1024); // 10MB 기본값

            // 기본 디렉토리 생성
            if (!Directory.Exists(_baseStoragePath))
            {
                Directory.CreateDirectory(_baseStoragePath);
            }
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder = "uploads")
        {
            try
            {
                // 파일 유효성 검사
                if (!ValidateFile(fileName, fileStream.Length, out var exception))
                {
                    _logger.LogError(exception, "File validation failed: {FileName}", fileName);
                    throw exception!;
                }

                var uploadsPath = Path.Combine(_baseStoragePath, folder);

                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                var fileExtension = Path.GetExtension(fileName);
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                var uniqueFileName = $"{fileNameWithoutExtension}_{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, uniqueFileName);

                using var outputStream = new FileStream(filePath, FileMode.Create);
                await fileStream.CopyToAsync(outputStream);

                _logger.LogInformation("File saved successfully: {FileName} -> {UniqueFileName}", fileName, uniqueFileName);

                return $"/{folder}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file: {FileName}", fileName);
                throw;
            }
        }

        public Stream GetFile(string filePath)
        {
            try
            {
                var fullPath = GetFullPath(filePath);

                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file: {FilePath}", filePath);
                throw;
            }
        }

        public bool DeleteFile(string filePath)
        {
            try
            {
                var fullPath = GetFullPath(filePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
                    return true;
                }

                _logger.LogWarning("Attempted to delete non-existent file: {FilePath}", filePath);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
                return false;
            }
        }

        public bool FileExists(string filePath)
        {
            try
            {
                var fullPath = GetFullPath(filePath);
                return File.Exists(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking file existence: {FilePath}", filePath);
                return false;
            }
        }

        public FileInfo? GetFileInfo(string filePath)
        {
            try
            {
                var fullPath = GetFullPath(filePath);

                if (!File.Exists(fullPath))
                {
                    return null;
                }

                return new FileInfo(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file info: {FilePath}", filePath);
                return null;
            }
        }

        #region Private Methods

        private string GetFullPath(string filePath)
        {
            // 보안: 경로 조작 방지
            var normalizedPath = filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_baseStoragePath, normalizedPath);

            // 보안 검증: 기본 경로 밖으로 나가지 않도록 확인
            var resolvedBasePath = Path.GetFullPath(_baseStoragePath);
            var resolvedPath = Path.GetFullPath(fullPath);

            if (!resolvedPath.StartsWith(resolvedBasePath, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Access to file outside base storage path is not allowed.");
            }

            return resolvedPath;
        }

        private bool ValidateFile(string fileName, long fileSize, out ArgumentException? exception)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                exception = new ArgumentException("File name cannot be empty.", nameof(fileName));
                return false;
            }

            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
            {
                exception = new ArgumentException($"File extension '{extension}' is not allowed. Allowed extensions: {string.Join(", ", _allowedExtensions)}");
                return false;
            }

            if (fileSize > _maxFileSize)
            {
                exception = new ArgumentException($"File size ({fileSize:N0} bytes) exceeds maximum allowed size ({_maxFileSize:N0} bytes).");
                return false;
            }

            if (fileSize <= 0)
            {
                exception = new ArgumentException("File size must be greater than 0.", nameof(fileSize));
                return false;
            }

            exception = null;
            return true;
        }

        #endregion
    }
}
