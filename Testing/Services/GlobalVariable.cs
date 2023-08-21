

namespace Testing.Services;

public class GlobalVariable
{
    private static GlobalVariable? instance;
    private int value;
    private static readonly object lockObject = new(); // Объект для блокировки

    private GlobalVariable()
    {
        value = 0;
    }

    public static GlobalVariable Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    instance ??= new GlobalVariable();
                }
            }
            return instance;
        }
    }

    public int Value
    {
        get
        {
            lock (lockObject)
            {
                return value;
            }
        }
        set
        {
            lock (lockObject)
            {
                this.value = value;
            }
        }
    }
}
