using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Provides utility methods for common operations, such as shuffling collections.
/// </summary>
public static class Utility 
{
    private static Random rng = new Random();  

    /// <summary>
    /// Randomly reorders the elements of the specified list using the Fisher-Yates Shuffle algorithm.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to shuffle. The list is modified in place.</param>
    /// <remarks>
    /// This method ensures an unbiased shuffle with a time complexity of O(n), 
    /// where n is the number of elements in the list.
    /// </remarks>
    /// <example>
    /// Example usage:
    /// <code>
    /// List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };
    /// numbers.Shuffle(); // Randomly reorders the elements in the list
    /// </code>
    /// </example>
    public static void Shuffle<T>(this IList<T> list)  
    {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }
}
