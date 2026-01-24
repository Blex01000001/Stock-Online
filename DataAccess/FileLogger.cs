namespace Stock_Online.DataAccess
{
    public class FileLogger
    {
        private readonly string _path;

        public FileLogger(string path)
        {
            _path = path;
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        }

        public void Info(string msg) =>
            Write("INFO ", msg);

        public void Error(string msg) =>
            Write("ERROR", msg);

        private void Write(string level, string msg)
        {
            File.AppendAllText(
                _path,
                $"[{DateTime.Now:HH:mm:ss.fff}] [{level}] {msg}{Environment.NewLine}"
            );
        }
    }
}
