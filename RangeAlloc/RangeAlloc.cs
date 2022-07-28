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

    public string DebugPrint()
    {
        return "";
    }

    #region Node

    abstract class Node { }

    abstract class Node<V, S, P> : Node, IEnumerable<V> where S : Node<V, S, P> where P : Node
    {
        public P? Parent;
        public byte Length;
        public V L;
        public V M;
        public V R;

        public Node(P? parent, V l, V m, V r, byte len)
        {
            Parent = parent;
            L = l;
            M = m;
            R = r;
            Length = len;
        }
        public bool IsEmpty => Length <= 0;
        public bool IsFull => Length >= 3;

        public ref V this[byte index]
        {
            get
            {
                if (index == 0) return ref L;
                else if (index == 1) return ref M;
                else return ref R;
            }
        }

        protected string GetDebuggerDisplay() => ToString();
        public override string ToString() => $"{{ {string.Join(", ", this)} }}";
        public IEnumerator<V> GetEnumerator()
        {
            for (byte i = 0; i < Length; i++) yield return i switch { 0 => L, 1 => M, _ => R };
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    sealed class IndexNode : Node<Node?, IndexNode, IndexNode>
    {
        public SpanRange<T> Range;
        public IndexNode(IndexNode? parent, SpanRange<T> range, Node l) : base(parent, l, null, null, 1) { Range = range; }
        public IndexNode(IndexNode? parent, SpanRange<T> range, Node l, Node m) : base(parent, l, m, null, 2) { Range = range; }
    }

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    sealed class LeafNode : Node<SpanRange<T>, LeafNode, IndexNode>
    {
        public LeafNode(IndexNode? parent, SpanRange<T> l) : base(parent, l, default, default, 1) { }
        public LeafNode(IndexNode? parent, SpanRange<T> l, SpanRange<T> m) : base(parent, l, m, default, 2) { }
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

        ref var node = ref root;

    rl: for (; ; )
        {
            if (node is LeafNode leaf)
            {
                for (byte i = 0; i < leaf.Length; i++)
                {
                    ref var lmr = ref leaf[i];
                    if (lmr.left == range.left)
                    {
                        // nl .. nr
                        // il .. ir
                        // =>
                        // [] remove
                        if (lmr.right == range.right)
                        {
                            switch (leaf.Length)
                            {
                                // [n, _, _]
                                case 1:
                                    if (node == root) goto sub_len;
                                    throw new NotImplementedException("todo");
                                // [x, x, _]
                                case 2:
                                    // [n, x, _]
                                    if (i == 0)
                                    {
                                        leaf.L = leaf.M;
                                        goto sub_len;
                                    }
                                    // [x, n, _]
                                    else goto sub_len;
                                // [x, x, x]
                                default:
                                    // [n, x, x]
                                    if (i == 0)
                                    {
                                        leaf.L = leaf.M;
                                        leaf.M = leaf.R;
                                        goto sub_len;
                                    }
                                    // [x, n, x]
                                    else if (i == 0)
                                    {
                                        leaf.M = leaf.R;
                                        goto sub_len;
                                    }
                                    // [x, x, n]
                                    else goto sub_len;
                            }
                        sub_len:
                            leaf.Length--;
                            return true;
                        }
                        // nl .. nr
                        // il ..    ir
                        else if (lmr.right < range.right) return false;
                        // nl ..    nr
                        // il .. ir
                        // =>
                        // [ir .. nr] edit
                        else if (lmr.right > range.right)
                        {
                            lmr = new(range.right, lmr.right);
                            return true;
                        }
                        // never or impl error
                        else return false;
                    }
                    else if (lmr.right == range.right)
                    {
                        //    nl .. nr
                        // il    .. ir
                        if (lmr.left > range.left) return false;
                        // nl    .. nr
                        //    il .. ir
                        // =>
                        // [nl .. il] edit
                        else if (lmr.left < range.left)
                        {
                            lmr = new(lmr.left, range.left);
                            return true;
                        }
                        // never or impl error
                        else return false;
                    }
                    // nl .. nr
                    //          il .. ir
                    else if (lmr.right <= range.left) continue;
                    //          nl .. nr
                    // il .. ir
                    else if (lmr.left >= range.right) return false;
                    // nl    ..    nr
                    //    il .. ir
                    // =>
                    // [nl .. il, ir .. nr] split
                    else if (lmr.left < range.left && lmr.right > range.right)
                    {
                        switch (leaf.Length)
                        {
                            // [n, _, _]
                            case 1:
                                leaf.M = new(range.right, lmr.right);
                                goto nr2il;
                            // [x, x, _]
                            case 2:
                                // [n, x, _]
                                if (i == 0)
                                {
                                    leaf.R = leaf.M;
                                    goto case 1;
                                }
                                // [x, n, _]
                                else
                                {
                                    leaf.R = new(range.right, lmr.right);
                                    goto nr2il;
                                }
                            // [x, x, x]
                            default:
                                var l = i == 0 ? lmr.left : leaf.L.left;
                                var r = i == 2 ? lmr.right : leaf.R.right;
                                LeafNode rln;
                                switch (i)
                                {
                                    // [n, x, x] => [l, r, _] [x, x, _]
                                    case 0:
                                        rln = new LeafNode(null, leaf.M, leaf.R);
                                        leaf.M = new(range.right, lmr.right);
                                        goto nr2il2;
                                    // [x, n, x] => [x, l, _] [r, x, _]
                                    case 1:
                                        rln = new LeafNode(null, new(range.right, lmr.right), leaf.R);
                                        goto nr2il2;
                                    // [x, x, n] => [x, x, _] [l, r, _]
                                    default:
                                        rln = new LeafNode(null, lmr, new(range.right, lmr.right));
                                        lmr = ref rln.L;
                                        goto nr2il2;
                                }
                            nr2il2:
                                lmr.right = range.left;
                                leaf.Length--;
                                node = new IndexNode(leaf.Parent, new(l, r), leaf, rln);
                                goto balance;
                        }
                    nr2il:
                        lmr.right = range.left;
                        leaf.Length++;
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
            balance:
            // todo
            return true;
        }
    }

}
