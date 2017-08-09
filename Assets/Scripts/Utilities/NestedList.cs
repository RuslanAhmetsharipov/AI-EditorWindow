using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NestedList<T> {
    public T[] list;

    public NestedList()
    {
        if (list!=null)
            Debug.Log(list.Length);
        list = new T[0];
    }

    public void Add(T obj)
    {
        List<T> genericList = new List<T>(list);
        genericList.Add(obj);
        list = genericList.ToArray();
    }
    public void RemoveAt(int num)
    {
        List<T> genericList = new List<T>(list);
        genericList.RemoveAt(num);
        list = genericList.ToArray();
    }
    public int Count()
    {
        return list.Length;
    }
    public T this[int key]
    {
        get
        {
            return list[key];
        }
        set
        {
            list[key] = value;
        }
    }
    void OnEnable()
    {
        if(list==null)
        {
            list = new T[0];
        }
    }
    
}
