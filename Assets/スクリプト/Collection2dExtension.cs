using System;
using System.Collections.Generic;

public delegate void ForActionIn2d<in ElementType>(int x, int y, ElementType element);
public delegate void ForActionInIndex<in ElementType>(int i, ElementType element);
public delegate TReturn ForFuncIn2d<in ElementType, out TReturn>(int x, int y, ElementType element);
public delegate TReturn ForFuncInIndex<in ElementType, out TReturn>(int i, ElementType element);
public static class Collection2dExtension
{
    public static void For<ElementType>(this List<List<ElementType>> collection, int startValue, Func<int, bool> conditionCheck, Func<int, int> increment, ForActionIn2d<ElementType> loopAction)
    {
        int x = 0, y = 0;
        int stepToNext = collection[0].Count;
        for (int i = startValue; conditionCheck(i); i = increment(i))
        {
            while (i >= stepToNext)
            {
                x++;
                y = i - stepToNext;
                stepToNext += collection[x].Count;
                if (x >= collection.Count) return;
            }
            loopAction(x, y, collection[x][y]);
        }
    }
    public static void For<ElementType>(this List<List<ElementType>> collection, int startValue, Func<int, bool> conditionCheck, Func<int, int> increment, ForActionInIndex<ElementType> loopAction)
    {
        int x = 0, y = 0;
        int stepToNext = collection[0].Count;
        for (int i = startValue; conditionCheck(i); i = increment(i))
        {
            while (i >= stepToNext)
            {
                x++;
                y = i - stepToNext;
                stepToNext += collection[x].Count;
                if (x >= collection.Count) return;
            }
            loopAction(i, collection[x][y]);
        }
    }
    public static void For<ElementType>(this List<List<ElementType>> collection, int endValue, ForActionIn2d<ElementType> loopAction, int startValue = 0)
    {
        collection.For(startValue, (i) => i < endValue, (i) => i++, loopAction);
    }
    public static void For<ElementType>(this List<List<ElementType>> collection, int endValue, ForActionInIndex<ElementType> loopAction, int startValue = 0)
    {
        collection.For(startValue, (i) => i < endValue, (i) => i++, loopAction);
    }
    public static void Foreach<ElementType>(this List<List<ElementType>> collection, ForActionIn2d<ElementType> loopAction)
    {
        collection.For(0, (i) => true, (i) => i++, loopAction);
    }
    public static void Foreach<ElementType>(this List<List<ElementType>> collection, ForActionInIndex<ElementType> loopAction)
    {
        collection.For(0, (i) => true, (i) => i++, loopAction);
    }
    public static void Foreach<ElementType>(this List<List<ElementType>> collection, Action<ElementType> loopAction)
    {
        collection.Foreach((x, y, element) => loopAction(element));
    }
    public static List<TReturn> For<ElementType, TReturn>(this List<List<ElementType>> collection, int startValue, Func<int, bool> conditionCheck, Func<int, int> increment, ForFuncIn2d<ElementType, TReturn> loopAction)
    {
        int x = 0, y = 0;
        int stepToNext = collection[0].Count;
        List<TReturn> returnValues = new();
        for (int i = startValue; conditionCheck(i); i = increment(i))
        {
            while (i >= stepToNext)
            {
                x++;
                y = i - stepToNext;
                stepToNext += collection[x].Count;
                if (x >= collection.Count) return returnValues;
            }
            returnValues.Add(loopAction(x, y, collection[x][y]));
        }

        return returnValues;
    }
    public static List<TReturn> For<ElementType, TReturn>(this List<List<ElementType>> collection, int startValue, Func<int, bool> conditionCheck, Func<int, int> increment, ForFuncInIndex<ElementType, TReturn> loopAction)
    {
        int x = 0, y = 0;
        int stepToNext = collection[0].Count;
        List<TReturn> returnValues = new();
        for (int i = startValue; conditionCheck(i); i = increment(i))
        {
            while (i >= stepToNext)
            {
                x++;
                y = i - stepToNext;
                stepToNext += collection[x].Count;
                if (x >= collection.Count) return returnValues;
            }
            returnValues.Add(loopAction(i, collection[x][y]));
        }
        return returnValues;
    }
    public static List<TReturn> For<ElementType, TReturn>(this List<List<ElementType>> collection, int endValue, ForFuncIn2d<ElementType, TReturn> loopAction, int startValue = 0)
    {
        return collection.For(startValue, (i) => i < endValue, (i) => i++, loopAction);
    }
    public static List<TReturn> For<ElementType, TReturn>(this List<List<ElementType>> collection, int endValue, ForFuncInIndex<ElementType, TReturn> loopAction, int startValue = 0)
    {
        return collection.For(startValue, (i) => i < endValue, (i) => i++, loopAction);
    }
    public static List<TReturn> Foreach<ElementType, TReturn>(this List<List<ElementType>> collection, ForFuncIn2d<ElementType, TReturn> loopAction)
    {
        return collection.For(0, (i) => true, (i) => i++, loopAction);
    }
    public static List<TReturn> Foreach<ElementType, TReturn>(this List<List<ElementType>> collection, ForFuncInIndex<ElementType, TReturn> loopAction)
    {
        return collection.For(0, (i) => true, (i) => i++, loopAction);
    }
    public static List<TReturn> Foreach<ElementType, TReturn>(this List<List<ElementType>> collection, Func<ElementType, TReturn> loopAction)
    {
        return collection.Foreach((x, y, element) => loopAction(element));
    }
}