namespace ERP.Application.Common.Interfaces
{
    public interface IFileStorageService
    {
        /// <summary>
        /// 파일을 저장하고 저장된 파일의 경로를 반환합니다.
        /// </summary>
        /// <param name="fileStream">저장할 파일 스트림</param>
        /// <param name="fileName">원본 파일명</param>
        /// <param name="folder">저장할 폴더 (기본값: uploads)</param>
        /// <returns>저장된 파일의 상대 경로</returns>
        Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder = "uploads");

        /// <summary>
        /// 파일을 읽어 스트림으로 반환합니다.
        /// </summary>
        /// <param name="filePath">파일 경로</param>
        /// <returns>파일 스트림</returns>
        Stream GetFile(string filePath);

        /// <summary>
        /// 파일을 삭제합니다.
        /// </summary>
        /// <param name="filePath">삭제할 파일 경로</param>
        /// <returns>삭제 성공 여부</returns>
        bool DeleteFile(string filePath);

        /// <summary>
        /// 파일 존재 여부를 확인합니다.
        /// </summary>
        /// <param name="filePath">확인할 파일 경로</param>
        /// <returns>파일 존재 여부</returns>
        bool FileExists(string filePath);

        /// <summary>
        /// 파일 정보를 반환합니다.
        /// </summary>
        /// <param name="filePath">파일 경로</param>
        /// <returns>파일 정보</returns>
        FileInfo? GetFileInfo(string filePath);
    }
}
