namespace Applenium
{
    /// <summary>
    ///     This is singleton class to cimunicate between threads. Mainly in use for stopping /kill execution thread
    /// </summary>
    public class Singleton
    {
        private static Singleton _instance;

        private Singleton()
        {
        }

        /// <summary>
        ///     Create instance od singleton class
        /// </summary>
        public static Singleton Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Singleton();
                }
                return _instance;
            }
        }

        /// <summary>
        ///     get or set stopexecution flag(bool true or false)
        /// </summary>
        public bool StopExecution { get; set; }

        /// <summary>
        ///     Set batch result multi threading scenario execution
        /// </summary>
        public bool BatchResult { get; set; }
    }
}