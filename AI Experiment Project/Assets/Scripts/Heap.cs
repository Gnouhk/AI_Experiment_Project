using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Generic heap implementation for objects of type T that implement the IHeapItem interface
public class Heap<T> where T : IHeapItem<T>
{
    T[] items;

    int currentItemCount;

    // Constructor to initialize the heap with a maximum size
    public Heap(int maxHeapSize)
    {
        items = new T[maxHeapSize];
    }

    // Add an item to the heap
    public void Add(T item)
    {
        // Set the heap index of the item to its current count
        item.HeapIndex = currentItemCount;

        // Place the item at the end of the array
        items[currentItemCount] = item;

        // Restore the heap property by moving the item up
        SortUp(item);

        // Increment the item count
        currentItemCount++;
    }

    // Remove and return the highest priority item from the heap
    public T RemoveFirst()
    {
        // Get the first item (highest priority) in the heap
        T firstItem = items[0];

        // Decrement the item count
        currentItemCount--;

        // Replace the first item with the last item and update the heap index of the last item
        items[0] = items[currentItemCount];
        items[0].HeapIndex = 0;

        // Restore the heap property by moving the item down
        SortDown(items[0]);

        // Return the removed item
        return firstItem;
    }

    // Update the position of an item in the heap
    public void UpdateItem(T item)
    {
        // Restore the heap property by moving the item up
        SortUp(item);
    }

    // Get the current number of items in the heap
    public int Count
    {
        get
        {
            return currentItemCount;
        }
    }

    // Check if the heap contains a specific item
    public bool Contains(T item)
    {
        // Check if the stored item at its heap index is the same as the specified item
        return Equals(items[item.HeapIndex], item);
    }

    // Move an item down in the heap to maintain the heap property
    void SortDown (T item)
    {
        while (true)
        {
            // Calculate indices of the left and right children
            int childIndexLeft = item.HeapIndex * 2 + 1;
            int childIndexRight = item.HeapIndex * 2 + 2;
            int swapIndex = 0;

            // Check if the left child is within the bounds of the heap
            if (childIndexLeft < currentItemCount)
            {
                // Assume the left child is the one to swap with
                swapIndex = childIndexLeft;

                // Check if the right child is within the bounds and has higher priority
                if (childIndexRight < currentItemCount)
                {
                    if(items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                    {
                        // Swap with the right child.
                        swapIndex = childIndexRight;
                    }
                }

                // Check if the item should be swapped with the chosen child
                if (item.CompareTo(items[swapIndex]) < 0)
                {
                    // Swap the item with the chosen child
                    Swap(item, items[swapIndex]);
                } 
                else
                {
                    // If no swap is needed, the heap property is restored
                    return;
                }
            } 
            else
            {
                // If no left child is present, the heap property is restored
                return;
            }
        }
    }

    // Move an item up in the heap to maintain the heap property
    void SortUp(T item)
    {
        // Calculate the parent index of the item
        int parentIndex = (item.HeapIndex - 1) / 2;

        while (true)
        { 
            // Get the parent item
            T parentItem = items[parentIndex];

            // Check if the item should be swapped with the parent
            if (item.CompareTo(parentItem) > 0)
            {
                // Swap the item with the parent
                Swap(item, parentItem);
            }
            else
            {
                // If no swap is needed, the heap property is restored
                break;
            }

            // Update the parent index for the next iteration
            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    // Swap two items in the heap
    void Swap(T itemA, T itemB)
    {
        // Swap the items in the array
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;

        // Update the heap indices of the swapped items
        int itemAIndex = itemA.HeapIndex;
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = itemAIndex;
    }
}

// Interface for heap items, requiring implementation of CompareTo and HeapIndex properties
public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex
    {
        get;
        set;
    }
}
