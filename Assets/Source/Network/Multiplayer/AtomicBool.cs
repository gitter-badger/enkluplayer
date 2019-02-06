using System;
using System.Threading;

/// <summary>
/// This class is used to compare and set a bool value in an atomic way.
/// </summary>
public class AtomicBool
{
    private int _value;

    /// <summary>
    /// Creates a new <see cref="AtomicBool"/> instance with an initial value of <c>false</c>.
    /// </summary>
    public AtomicBool()
        : this(false)
    {
    }

    /// <summary>
    /// Creates a new <see cref="AtomicBool"/> instance with the initial value provided.
    /// </summary>
    public AtomicBool(bool value)
    {
        _value = value ? 1 : 0;
    }

    /// <summary>
    /// This method returns the current value.
    /// </summary>
    /// <returns>The <see cref="bool"/> value to be accessed atomically.</returns>
    public bool Get()
    {
        return _value != 0;
    }

    /// <summary>
    /// This method sets the current value atomically.
    /// </summary>
    /// <param name="value">The new value to set.</param>
    public void Set(bool value)
    {
        Interlocked.Exchange(ref _value, value ? 1 : 0);
    }

    /// <summary>
    /// Atomically sets the value to the given updated value if the current value <c>==</c> the expected value.
    /// </summary>
    /// <param name="expected">The value to compare against.</param>
    /// <param name="result">The value to set if the value is equal to the <c>expected</c> value.</param>
    /// <returns><c>true</c> if the comparison and set was successful. A <c>false</c> indicates the comparison failed.</returns>
    public bool CompareAndSet(bool expected, bool result)
    {
        int e = expected ? 1 : 0;
        int r = result ? 1 : 0;
        return Interlocked.CompareExchange(ref _value, r, e) == e;
    }
}
