using FileReaderApplication.BusinessLogic.Impl;

namespace FileReaderApplication.BusinessLogic.Interface
{
    /// <summary>
    /// Фабрика создания "читатетей" файлов
    /// </summary>
    public interface IFileReaderFactory
    {
        /// <summary>
        /// Создать "читателя" фала
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <returns>Читатель</returns>
        FileReader CreateFileReader(string fileName);
    }
}