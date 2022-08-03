using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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

    const int Cap = 6;
    const int CapSplit = 5;
    const int CapHalf = 3;

    public string DebugPrint()
    {
        return "";
    }

    #region Node

    abstract class Node { }

    abstract class Node<V, S, P> : Node, IEnumerable<V> where S : Node<V, S, P> where P : Node
    {
        public P? Parent;
        public int Length;
        public V[] Arr;

        public Node(P? parent, int len)
        {
            Parent = parent;
            Length = len;
            Arr = new V[Cap];
        }
        public bool IsEmpty => Length <= 0;
        public bool IsFull => Length >= Cap;

        public ref V this[int index] => ref Arr[index];

        public ref V First => ref Arr[0];
        public ref V Last => ref Arr[Length - 1];

        protected string GetDebuggerDisplay() => ToString();
        public override string ToString() => $"{{ {string.Join(", ", this)} }}";
        public IEnumerator<V> GetEnumerator()
        {
            for (var i = 0; i < Length; i++) yield return Arr[i];
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary><code>
        /// [a, b, c, _, _, _]
        /// (1) =>
        /// [a, b, b, c, _, _]
        /// </code></summary>
        public void Shift1(int index)
        {
            var src = new ReadOnlySpan<V>(Arr, index, Length - index);
            var dst = new Span<V>(Arr, index + 1, src.Length);
            src.CopyTo(dst);
        }
        /// <summary><code>
        /// [a, b, c, d, e, _] [_, _, _, _, _ ,_]
        /// =>
        /// [a, b, _, _, _, _] [c, d, e, _, _ ,_]
        /// </code></summary>
        public void SplitLeft(Node<V, S, P> other)
        {
            var len = Length - CapHalf;
            var src = new ReadOnlySpan<V>(Arr, len, CapHalf);
            var dst = new Span<V>(other.Arr, 0, CapHalf);
            src.CopyTo(dst);
            Length = len;
            other.Length = CapHalf;
        }
        /// <summary><code>
        /// [a, b, c, d, e, _] [_, _, _, _, _ ,_]
        /// =>
        /// [a, b, c, _, _, _] [d, e, _, _ ,_, _]
        /// </code></summary>
        public void SplitRight(Node<V, S, P> other)
        {
            var len = Length - CapHalf;
            var src = new ReadOnlySpan<V>(Arr, CapHalf, len);
            var dst = new Span<V>(other.Arr, 0, len);
            src.CopyTo(dst);
            Length = CapHalf;
            other.Length = len;
        }
    }

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    sealed class IndexNode : Node<Node?, IndexNode, IndexNode>
    {
        public SpanRange<T> Range;
        public IndexNode(IndexNode? parent, SpanRange<T> range) : base(parent, 0) { Range = range; }
        public IndexNode(IndexNode? parent, SpanRange<T> range, Node a) : base(parent, 1) { Range = range; Arr[0] = a; }
        public IndexNode(IndexNode? parent, SpanRange<T> range, Node a, Node b) : base(parent, 2) { Range = range; Arr[0] = a; Arr[1] = b; }
    }

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    sealed class LeafNode : Node<SpanRange<T>, LeafNode, IndexNode>
    {
        public LeafNode(IndexNode? parent) : base(parent, 0) { }
        public LeafNode(IndexNode? parent, SpanRange<T> a) : base(parent, 1) { Arr[0] = a; }
        public LeafNode(IndexNode? parent, SpanRange<T> a, SpanRange<T> b) : base(parent, 2) { Arr[0] = a; Arr[1] = b; }
    }

    #endregion

    public RangeAlloc(T index, T length) : this(SpanRange.FromLength(index, length)) { }

    public RangeAlloc(SpanRange<T> root)
    {
        this.root = new LeafNode(null, range = root);
    }

    public bool Alloc(T index, T length) => Alloc(SpanRange.FromLength(index, length));
    public bool Alloc(SpanRange<T> range)
    {
        // alias:
        // nl nr = node left ; node right
        // il ir = input left ; input right

        if (range.right < range.left) return false;
        if (range.IsEmpty) return true;

        var node = root;
        var parent_index = 0;

    rl: for (; ; )
        {
            if (node is LeafNode leaf)
            {
                for (var i = 0; i < leaf.Length; i++)
                {
                    ref var nref = ref leaf[i];
                    if (nref.left == range.left)
                    {
                        // nl .. nr
                        // il .. ir
                        // =>
                        // [] remove
                        if (nref.right == range.right)
                        {
                        //switch (leaf.Length)
                        //{
                        //    // [n, _, _]
                        //    case 1:
                        //        if (node == root) goto sub_len;
                        //        throw new NotImplementedException("todo");
                        //    // [x, x, _]
                        //    case 2:
                        //        // [n, x, _]
                        //        if (i == 0)
                        //        {
                        //            leaf.L = leaf.M;
                        //            goto sub_len;
                        //        }
                        //        // [x, n, _]
                        //        else goto sub_len;
                        //    // [x, x, x]
                        //    default:
                        //        // [n, x, x]
                        //        if (i == 0)
                        //        {
                        //            leaf.L = leaf.M;
                        //            leaf.M = leaf.R;
                        //            goto sub_len;
                        //        }
                        //        // [x, n, x]
                        //        else if (i == 0)
                        //        {
                        //            leaf.M = leaf.R;
                        //            goto sub_len;
                        //        }
                        //        // [x, x, n]
                        //        else goto sub_len;
                        //}
                        sub_len:
                            leaf.Length--;
                            return true;
                        }
                        // nl .. nr
                        // il ..    ir
                        else if (nref.right < range.right) return false;
                        // nl ..    nr
                        // il .. ir
                        // =>
                        // [ir .. nr] edit
                        else if (nref.right > range.right)
                        {
                            nref = new(range.right, nref.right);
                            return true;
                        }
                        // never or impl error
                        else return false;
                    }
                    else if (nref.right == range.right)
                    {
                        //    nl .. nr
                        // il    .. ir
                        if (nref.left > range.left) return false;
                        // nl    .. nr
                        //    il .. ir
                        // =>
                        // [nl .. il] edit
                        else if (nref.left < range.left)
                        {
                            nref = new(nref.left, range.left);
                            return true;
                        }
                        // never or impl error
                        else return false;
                    }
                    // nl .. nr
                    //          il .. ir
                    else if (nref.right <= range.left) continue;
                    //          nl .. nr
                    // il .. ir
                    else if (nref.left >= range.right) return false;
                    // nl    ..    nr
                    //    il .. ir
                    // =>
                    // [nl .. il, ir .. nr] split
                    else if (nref.left < range.left && nref.right > range.right)
                    {
                        var ni = i + 1;
                        // [n] => [n, n]
                        if (leaf.Length == 1) goto insert;
                        // [x, x...]
                        else if (leaf.Length < CapSplit)
                        {
                            // [x..., n, x...] => [x..., n, n, x...]
                            if (ni < leaf.Length)
                            {
                                leaf.Shift1(ni);
                                goto insert;
                            }
                            // [x..., n] => [x..., n, n]
                            else goto insert;
                        }
                        // [x, ...x] => [x...] [n, n, x...]
                        else
                        {
                            var new_leaf = new LeafNode(null);
                            // [x, x, x, x, x, x]
                            //     i   c
                            if (i < CapHalf)
                            {
                                leaf.SplitLeft(new_leaf);
                                insert(leaf, ni, in range, ref nref);
                            }
                            // [x, x, x, x, x, x]
                            //         c    i
                            else
                            {
                                leaf.SplitRight(new_leaf);
                                i -= CapHalf;
                                ni = i + 1;
                                nref = ref new_leaf[i];
                                insert(new_leaf, ni, in range, ref nref);
                            }
                            MakeIndex1(leaf, new_leaf);
                            return true;
                        }
                    insert:
                        insert(leaf, ni, in range, ref nref);
                        static void insert(LeafNode leaf, int ni, in SpanRange<T> range, ref SpanRange<T> nref)
                        {
                            leaf[ni] = new(range.right, nref.right);
                            nref.right = range.left;
                            leaf.Length++;
                        }
                        return true;
                    }
                    //    nl .. nr
                    // il    ..    ir
                    // ==============
                    //    nl ..    nr
                    // il    .. ir
                    // ==============
                    // nl    .. nr
                    //    il ..    ir
                    else return false;
                }
                return false;
            }
            else if (node is IndexNode index)
            {
                throw new NotImplementedException("todo");
            }
            // never or impl error
            else throw new NotImplementedException("never");
        }
    }

    void MakeIndex1(LeafNode leaf, LeafNode new_leaf)
    {
        var node = leaf.Parent;
        if (node == null)
        {
            node = new IndexNode(null, new(leaf.First.left, new_leaf.Last.right), leaf, new_leaf);
            new_leaf.Parent = leaf.Parent = node;
            root = node;
            return;
        }
        else
        {
            var parent = node.Parent;
            throw new NotImplementedException("todo");
        }
    }
}
