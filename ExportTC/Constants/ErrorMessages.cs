namespace ExportTC
{
    public static class ErrorMessages
    {
        public const string ApplicationInitializationError = "Ошибка при инициализации приложения.";
        public const string ApplicationExitError = "Ошибка при завершении работы приложения.";
        public const string ApplicationExitInfo = "Приложение закрывается.";
        public const string MainWindowStartupError = "Ошибка при запуске MainWindow.";
        public const string ViewModelInitializationError = "Ошибка при инициализации PathViewModel.";
        public const string GenerateViewModelInitializationError = "Ошибка при инициализации GenerateViewModel.";
        public const string InitialDataServiceError = "Ошибка при получении службы InitialData.";
        public const string FileSearchServiceError = "Ошибка при получении службы IFileSearchService.";
        public const string FileDialogServiceError = "Ошибка при получении службы IFileDialogService.";
        public const string InitialDataSetterServiceError = "Ошибка при получении службы IInitialDataSetter.";
        public const string DirectoryPathChangeError = "Ошибка при изменении пути каталога.";
        public const string ExcelFilePathChangeError = "Ошибка при изменении пути к Excel файлу.";
        public const string HtmlFilePathChangeError = "Ошибка при изменении пути к HTML файлу.";
        public const string ExcelFileDialogError = "Ошибка при открытии диалога выбора Excel файла.";
        public const string HtmFileDialogError = "Ошибка при открытии диалога выбора HTML файла.";
        public const string DirectoryDialogError = "Ошибка при открытии диалога выбора каталога.";
        public const string ErrorExcelFIleIsBusy = " \"Ошибка при копировании ресурса в файл. Возможно, файл занят другим процессом.\"";

        // Новые сообщения об ошибках для проверки на null значений
        public const string InvalidDirectoryPath = "Недопустимое значение для пути каталога.";
        public const string InvalidExcelFilePath = "Недопустимое значение для пути к Excel файлу.";
        public const string InvalidHtmFilePath = "Недопустимое значение для пути к HTML файлу.";
    }
}
