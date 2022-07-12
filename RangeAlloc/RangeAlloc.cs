using System;
using System.Diagnostics;
using System.Text;

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

    public string DebugPrint()
    {
        var sb = new StringBuilder();
        Rec(root, "", "○──", " ", " ");
        void Rec(in Node node, string prefix, string lr, string pl, string pr)
        {
            if (node.Sub != null) Rec(node.Sub.Right, $"{pr}  ", "╭──", $"{pr}  |", $"{pr}   ");
            PrintSelf(node, prefix, lr);
            if (node.Sub != null) Rec(node.Sub.Left, $"{pl}  ", "╰──", $"{pl}   ", $"{pl}  |");
        }
        void PrintSelf(in Node node, string prefix, string lr)
        {
            sb.Append(prefix);
            sb.Append(lr);
            if (node.Sub != null) sb.Append('┤');
            else sb.Append('─');
            sb.Append('(');
            sb.Append(node.Range);
            sb.Append(')');
            sb.AppendLine();
        }
        return sb.ToString();
    }

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    struct Node
    {
        public SpanRange<T> Range;
        public Sub? Sub;

        public Node(SpanRange<T> range, Sub? sub)
        {
            Range = range;
            Sub = sub;
        }

        private string GetDebuggerDisplay() => ToString();
        public override string ToString() => $"({Range}) {Sub}";
    }

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    class Sub
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

    public RangeAlloc(T index, T length) : this(new SpanRange<T>(index, length)) { }

    public RangeAlloc(SpanRange<T> root)
    {
        this.root = new RangeAlloc<T>.Node(range = root, null);
    }

    public bool Alloc(T index, T length) => Alloc(new(index, length));
    public bool Alloc(SpanRange<T> range) => Alloc(in range);
    public bool Alloc(in SpanRange<T> range)
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

        if (node.Sub == null)
        {
            goto make_sub;
        }

        ref var sub = ref node.Sub;

        for (; ; )
        {
            if (range <= sub.Left.Range)
            {
                if (sub.Left.Sub == null)
                {
                    if (range == sub.Left.Range)
                    {
                        node.Range = sub.Right.Range;
                        sub = null;
                        return true;
                    }
                    else if (range.Right == sub.Left.Range.Right)
                    {
                        sub.Left.Range = sub.Left.Range.SliceLeft(range);
                        return true;
                    }
                    else if (range.Right <= sub.Left.Range.Right)
                    {
                        goto make_sub;
                    }
                }
                else
                {
                    node = ref sub.Left;
                    continue;
                }
            }
            else if (range <= sub.Right.Range)
            {
                if (sub.Right.Sub == null)
                {
                    if (range == sub.Right.Range)
                    {
                        node.Range = sub.Left.Range;
                        sub = null;
                        return true;
                    }
                    else if (range.Left == sub.Right.Range.Left)
                    {
                        sub.Right.Range = sub.Right.Range.SliceRight(range);
                        return true;
                    }
                    else if (range.Left >= sub.Right.Range.Left)
                    {
                        goto make_sub;
                    }
                }
                else
                {
                    node = ref sub.Right;
                    continue;
                }
            }
            return false;

        }

    make_sub:
        node.Sub = new(new(node.Range.SliceLeft(range), null), new(node.Range.SliceRight(range), null));
        return true;
    }

}
