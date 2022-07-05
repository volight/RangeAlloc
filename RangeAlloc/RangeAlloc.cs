using System;
using System.Diagnostics;

namespace Volight.Allocators;

public class RangeAlloc : RangeAlloc<int>
{
    public RangeAlloc(SpanRange<int> root) : base(root) { }

    public RangeAlloc(int index, int length) : base(index, length) { }
}

public class RangeAlloc<T> where T : unmanaged, IEquatable<T>, IComparable<T>
{
    public SpanRange<T> Range => range;
    SpanRange<T> range;
    Node root;

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    struct Node
    {
        public SpanRange<T> Range;
        public Box<Sub>? Sub;

        public Node(SpanRange<T> range, Box<Sub>? sub)
        {
            Range = range;
            Sub = sub;
        }

        private string GetDebuggerDisplay() => ToString();
        public override string ToString() => $"({Range}) {Sub}";
    }

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    struct Sub
    {
        public Node Left;
        public Node Right;

        public Sub(Node left, Node right)
        {
            Left = left;
            Right = right;
        }

        private string GetDebuggerDisplay() => ToString();
        public override string ToString() => $"{{ L{Left}; R{Right} }}";
    }

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    class Box<V>
    {
        public V Value;

        public Box(V value) => Value = value;

        private string? GetDebuggerDisplay() => ToString();
        public override string? ToString() => Value?.ToString();
    }

    public RangeAlloc(T index, T length) : this(new SpanRange<T>(index, length)) { }

    public RangeAlloc(SpanRange<T> root)
    {
        this.root = new RangeAlloc<T>.Node(range = root, null);
    }

    public bool Alloc(T index, T length) => Alloc(new(index, length));
    public bool Alloc(SpanRange<T> range)
    {
        if (range.IsEmpty) return true;

        ref var node = ref root;

        if (range > node.Range) return false;

        if (range.Right == node.Range.Right)
        {
            node.Range = node.Range.SliceLeft(range);
            return true;
        }
        else if (range.Left == node.Range.Left)
        {
            node.Range = node.Range.SliceRight(range);
            return true;
        }

        for (; ; )
        {
            if (node.Sub == null)
            {
                node.Sub = new(new(new(node.Range.SliceLeft(range), null), new(node.Range.SliceRight(range), null)));
                return true;
            }
            else
            {
                ref var sub = ref node.Sub.Value;
                if (range <= sub.Left.Range)
                {
                    if (range == sub.Left.Range)
                    {
                        sub.Left.Range = node.Range.SliceLeft(range);
                        return true;
                    }
                    else if (range.Right == sub.Left.Range.Right)
                    {
                        sub.Left.Range = sub.Left.Range.SliceLeft(range);
                        return true;
                    }
                    else if (range.Right <= sub.Left.Range.Right)
                    {
                        node = ref sub.Left;
                        continue;
                    }
                }
                else if (range <= sub.Right.Range)
                {
                    if (range == sub.Right.Range)
                    {
                        sub.Right.Range = node.Range.SliceRight(range);
                        return true;
                    }
                    else if (range.Left == sub.Right.Range.Left)
                    {
                        sub.Right.Range = sub.Right.Range.SliceRight(range);
                        return true;
                    }
                    else if (range.Left >= sub.Right.Range.Left)
                    {
                        node = ref sub.Right;
                        continue;
                    }
                }
            }
            return false;
        }
    }

}
