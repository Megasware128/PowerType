﻿using PowerType.Model;

namespace PowerType.BackgroundProcessing;

internal partial class ExecutionEngine
{
    private ExecutionEngineThread? executionEngineThread;
    private readonly ThreadQueue<Command> threadQueue = new();

    public List<DictionarySuggestor> GetSuggestors() => executionEngineThread?.GetSuggestors() ?? new List<DictionarySuggestor>();

    public bool IsHealthy(out Exception? exception)
    {
        var thread = executionEngineThread;
        if (thread != null)
        {
            return thread.IsHealthy(out exception);
        }
        exception = new ThreadStateException("No execution thread is running");
        return false;
    }

    public void Start()
    {
        if (executionEngineThread != null)
        {
            throw new InvalidOperationException("Already started");
        }
        executionEngineThread = new ExecutionEngineThread(threadQueue);
    }

    public void Stop()
    {
        if (executionEngineThread == null)
        {
            throw new InvalidOperationException("Not started");
        }
        executionEngineThread.Dispose();
        executionEngineThread = null;
    }

    public void InitialDictionary(string dictionaryPath) =>
        threadQueue.Enqueue(new InitializeDictionaryCommand(dictionaryPath));

    public void Cache(PowerTypeDictionary dictionary, string currentWorkingDirectory) =>
        threadQueue.Enqueue(new CacheDictionaryDynamicSources(dictionary, currentWorkingDirectory));
}
