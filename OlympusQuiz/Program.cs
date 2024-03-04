class Program
{
    // Global line counter
    static int _lineCount = 0;
    static Random _random = new Random();
    private static readonly SemaphoreSlim _semaphoreAsyncLock = new SemaphoreSlim(1, 1);

    static void Main(string[] args)
    {
        try
        {
            string filePath = InitializeOutFile();

            StartWorkOnThreadPool(filePath);

            // Wait for a character press before exiting
            Console.WriteLine("Press any key to exit...");
            Console.Read();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private static string InitializeOutFile()
    {
        try
        {
            // Output directory
            const string outputDirectory = "/log/";
            // File path
            string filePath = Path.Combine(outputDirectory, "out.txt");

            // Ensure output directory exists
            Directory.CreateDirectory(outputDirectory);

            // Create or overwrite the file and initialize with the first line
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine($"{_lineCount}, 0, {DateTime.Now:HH:mm:ss.fff}");
            }

            return filePath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to create output file: {ex.Message}");
            return string.Empty;
        }
    }

    private static void StartWorkOnThreadPool(string filePath)
    {
        try
        {
            // Number of tasks to execute
            int numTasks = 10;

            // Use a countdown event to synchronize the start of all tasks
            CountdownEvent countdown = new CountdownEvent(numTasks);

            // Start tasks on the thread pool
            for (int i = 0; i < numTasks; i++)
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    // Task logic goes here
                    Console.WriteLine($"Thread {state} started. Thread ID: {Environment.CurrentManagedThreadId}");
                    // Write 10 lines simultaneously
                    for (int j = 0; j < 10; j++)
                    {
                        // Write to the file
                        WriteToFile(filePath);
                    }

                    // Signal that the task has started
                    countdown.Signal();
                }, i); // Pass the task index as state
            }

           
            // Wait for all tasks to start
            countdown.Wait();
            Console.WriteLine("All threads are done");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Thread pool failed on a thread {Environment.CurrentManagedThreadId}: {ex.Message}");
        }
    }

    private static void StartWorkOnThread(string filePath)
    {
        try
        {
            // Array to hold the threads
            Thread[] threads = new Thread[10];

            // Start 10 threads
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(() =>
                {
                    // Write 10 lines simultaneously
                    for (int j = 0; j < 10; j++)
                    {
                        // Write to the file
                        WriteToFile(filePath);
                    }
                });

                threads[i].Start();
            }

            // Wait for all threads to complete
            foreach (Thread t in threads)
            {
                t.Join();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failure on a thread {Environment.CurrentManagedThreadId}: {ex.Message}");
        }
    }

    static void WriteToFile(string filePath)
    {
        try
        {
            // Lock to ensure exclusive access to file writing
            lock (filePath)
            {
                Thread.Sleep(_random.Next(1, 100));//assume that work actually takes some time
                // Increment the global line counter and get the current value
                int currentLine = Interlocked.Increment(ref _lineCount);
                int threadId = Environment.CurrentManagedThreadId;
                // Append content to the file
                File.AppendAllText(filePath, $"{currentLine}, {threadId}, {DateTime.Now.ToString("HH:mm:ss.fff")}{Environment.NewLine}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Thread {Environment.CurrentManagedThreadId} threw an exception: {ex.Message}");
        }
    }
}