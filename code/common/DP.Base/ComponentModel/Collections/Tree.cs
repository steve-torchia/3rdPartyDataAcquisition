//using System;
//using System.Collections.Generic;

//namespace DP.Base.Collections
//{
//    #region Delegates, EventArgs and Enumerations

//    public enum ENodeEvent
//    {
//        ValueAccessed,
//        ValueChanged,
//        NodeChanged,
//        ChildOrderChanged,
//        ChildAdded,
//        ChildRemoved,
//        ChildsCleared,
//    }

//    /// <summary>EventArgs for changes in a TreeNode</summary>
//    public class TreeEventArgs<T> : System.EventArgs
//    {
//        public TreeEventArgs(TreeNode<T> node, ENodeEvent change, int index)
//        {
//            this.Node = node;
//            this.Change = change;
//            this.Index = index;
//        }

//        /// <summary>The Node for which the event occured.</summary>
//        public TreeNode<T> Node { get; set; }

//        /// <summary>
//        /// <list>
//        ///   <item>ValueAccessed: the get - accessor for node.Value was called. index is unused</item>
//        ///   <item>ValueChanged: A new value was assigned to node.Value. index is unused</item>
//        ///   <item>NodeChanged: The node itself has changed (e.g. another node was assigned) All child nodes may have changed, too</item>
//        ///   <item>ChildOrderChanged: the order of the elements of node.Childs has changed</item>
//        ///   <item>ChildAdded: a child node was added at position <c>index</c></item>
//        ///   <item>ChildRemoved: a child node was removed at position <c>index</c>.
//        ///         This notification is <b>not</b> sent when all items are removed using Clear
//        ///   </item>
//        ///   <item>ChildsCleared: all childs were removed.</item>
//        /// </list>
//        /// </summary>
//        public ENodeEvent Change { get; set; }

//        /// <summary>Index of the child node affected. See the Change member for more information.</summary>
//        public int Index { get; set; }
//    }

//    public delegate string NodeToString<T>(TreeNode<T> node);

//    #endregion

//    #region Tree Root

//    /// <summary>
//    /// A TreeRoot object acts as source of tree node events. A single instance is associated
//    /// with each tree (the Root property of all nodes of a tree return the same instance, nodes
//    /// from different trees return different instances)
//    /// </summary>
//    /// <typeparam name="T">type of the data value at each tree node</typeparam>
//    public class TreeRoot<T>
//    {
//        private TreeNode<T> mRoot;

//        internal TreeRoot(TreeNode<T> root)
//        {
//            this.mRoot = root;
//        }

//        public TreeNode<T> RootNode
//        {
//            get { return this.mRoot; }
//        }

//        #region Events

//        /// <summary>
//        /// signals that a new value was assigned to a given node. <br/>
//        /// Note: if T is a reference type and modified indirectly, this event doesn't fire
//        /// </summary>
//        public event System.EventHandler OnValueChanged;

//        /// <summary>
//        /// signals that Node.Value was accessed.
//        /// This can be used by a tree view controller to implement a defered 
//        /// update even if T is a reference type and changed implicitely (i.e. 
//        /// for cases where OnValueChanged does not fire)
//        /// </summary>
//        public event System.EventHandler OnValueAccessed;

//        /// <summary>signals that the Node structure has changed</summary>
//        public event System.EventHandler OnNodeChanged;

//        #region Internal Helpers
//        internal void SendValueChanged(TreeNode<T> node)
//        {
//            if (this.OnValueChanged != null)
//            {
//                this.OnValueChanged(this, new TreeEventArgs<T>(node, ENodeEvent.ValueChanged, -1));
//            }
//        }

//        internal void SendValueAccessed(TreeNode<T> node)
//        {
//            if (this.OnValueAccessed != null)
//            {
//                this.OnValueAccessed(this, new TreeEventArgs<T>(node, ENodeEvent.ValueAccessed, -1));
//            }
//        }

//        internal void SendNodeChanged(TreeNode<T> node, ENodeEvent change, int index)
//        {
//            if (this.OnNodeChanged != null)
//            {
//                this.OnNodeChanged(this, new TreeEventArgs<T>(node, change, index));
//            }
//        }
//        #endregion // Internal Helpers

//        #endregion // Events
//    }

//    #endregion // Tree Events

//    #region TreeNode<T>
//    /// <summary>
//    /// Represents a single Tree Node
//    /// </summary>
//    /// <typeparam name="T">Type of the Data Value at the node</typeparam>
//    public class TreeNode<T>
//    {
//        #region Node Data
//        private T mValue;
//        private TreeNode<T> mParent = null;
//        private TreeNodeCollection<T> mNodes = null;
//        private TreeRoot<T> mRoot = null;
//        #endregion // Node Data

//        #region CTORs

//        public TreeNode()
//        {
//            this.mValue = default(T);
//            this.mRoot = new TreeRoot<T>(this);
//        }

//        /// <summary>
//        /// creates a new root node, and sets Value to value.
//        /// </summary>
//        /// <param name="value"></param>
//        public TreeNode(T value)
//        {
//            this.mValue = value;
//            this.mRoot = new TreeRoot<T>(this);
//        }

//        /// <summary>
//        /// Creates a new node as child of parent, and sets Value to value
//        /// </summary>
//        /// <param name="value"></param>
//        /// <param name="parent"></param>
//        internal TreeNode(T value, TreeNode<T> parent)
//        {
//            this.mValue = value;
//            this.InternalSetParent(parent);
//        }
//        #endregion // CTORs

//        #region Data Access
//        /// <summary>
//        /// Node data
//        /// Setting the value fires Tree<T>.OnNodeChanged
//        /// </summary>
//        [System.Xml.Serialization.XmlElement(ElementName = "val")]
//        public T Value
//        {
//            get
//            {
//                this.mRoot.SendValueAccessed(this);
//                return this.mValue;
//            }
//            set
//            {
//                this.mValue = value;
//                this.mRoot.SendValueChanged(this);
//            }
//        }
//        #endregion // Data Access

//        #region Navigation

//        /// <summary>returns the parent node, or null if this is a root node</summary>
//        [System.Xml.Serialization.XmlIgnore]
//        public TreeNode<T> Parent
//        {
//            get { return this.mParent; }
//        }

//        /// <summary>
//        /// returns all siblings as a NodeList<T>. If this is a root node, the function returns null.
//        /// </summary>
//        [System.Xml.Serialization.XmlIgnore]
//        public TreeNodeCollection<T> Siblings
//        {
//            get { return this.mParent != null ? this.mParent.Nodes : null; }
//        }

//        /// <summary>
//        /// returns all child nodes as a NodeList<T>. 
//        /// <para><b>Implementation note:</b> Childs always returns a non-null collection. 
//        /// This collection is created on demand at the first access. To avoid unnecessary 
//        /// creation of the collection, use HasChilds to check if the node has any child nodes</para>
//        /// </summary>
//        [System.Xml.Serialization.XmlArrayItem("node")]
//        public TreeNodeCollection<T> Nodes
//        {
//            get
//            {
//                if (this.mNodes == null)
//                {
//                    this.mNodes = new TreeNodeCollection<T>(this);
//                }

//                return this.mNodes;
//            }
//        }

//        /// <summary>
//        /// The Root object this Node belongs to. never null
//        /// </summary>
//        [System.Xml.Serialization.XmlIgnore]
//        public TreeRoot<T> Root
//        {
//            get { return this.mRoot; }
//        }

//        internal void SetRootLink(TreeRoot<T> root)
//        {
//            if (this.mRoot != root) // assume sub trees are consistent
//            {
//                this.mRoot = root;
//                if (this.HasChildren)
//                {
//                    foreach (TreeNode<T> n in this.Nodes)
//                    {
//                        n.SetRootLink(root);
//                    }
//                }
//            }
//        }

//        /// <summary>
//        /// returns true if the node has child nodes.
//        /// See also Implementation Note under this.Childs
//        /// </summary>
//        [System.Xml.Serialization.XmlIgnore]
//        public bool HasChildren
//        {
//            get { return this.mNodes != null && this.mNodes.Count != 0; }
//        }

//        /// <summary>
//        /// returns true if this node is a root node. (Equivalent to this.Parent==null)
//        /// </summary>
//        [System.Xml.Serialization.XmlIgnore]
//        public bool IsRoot
//        {
//            get { return this.mParent == null; }
//        }

//        public bool IsAncestorOf(TreeNode<T> node)
//        {
//            if (node.Root != this.Root)
//            {
//                return false; // different trees
//            }

//            TreeNode<T> parent = node.Parent;
//            while (parent != null && parent != this)
//            {
//                parent = parent.Parent;
//            }

//            return parent != null;
//        }

//        public bool IsChildOf(TreeNode<T> node)
//        {
//            return !this.IsAncestorOf(node);
//        }

//        public bool IsInLineWith(TreeNode<T> node)
//        {
//            return node == this ||
//                   node.IsAncestorOf(this) ||
//                   node.IsChildOf(node);
//        }

//        [System.Xml.Serialization.XmlIgnore]
//        public int Depth
//        {
//            get
//            {
//                int depth = 0;
//                TreeNode<T> node = this.mParent;
//                while (node != null)
//                {
//                    ++depth;
//                    node = node.mParent;
//                }

//                return depth;
//            }
//        }

//        #endregion // Navigation

//        #region Node Path

//        public TreeNode<T> GetNodeAt(int index)
//        {
//            return this.Nodes[index];
//        }

//        public TreeNode<T> GetNodeAt(IEnumerable<int> index)
//        {
//            TreeNode<T> node = this;
//            foreach (int elementIndex in index)
//            {
//                node = node.Nodes[elementIndex];
//            }

//            return node;
//        }

//        public TreeNode<T> GetNodeAt(params int[] index)
//        {
//            return this.GetNodeAt(index as IEnumerable<int>);
//        }

//        public int[] GetIndexPathTo(TreeNode<T> node)
//        {
//            if (this.Root != node.Root)
//            {
//                throw new ArgumentException("parameter node must belong to the same tree");
//            }

//            List<int> index = new List<int>();

//            while (node != this && node.mParent != null)
//            {
//                index.Add(node.mParent.Nodes.IndexOf(node));
//                node = node.mParent;
//            }

//            if (node != this)
//            {
//                throw new ArgumentException("node is not a child of this");
//            }

//            index.Reverse();
//            return index.ToArray();
//        }

//        public int[] GetIndexPath()
//        {
//            return this.Root.RootNode.GetIndexPathTo(this);
//        }

//        public System.Collections.IList GetNodePath()
//        {
//            List<TreeNode<T>> list = new List<TreeNode<T>>();
//            TreeNode<T> node = this.mParent;

//            while (node != null)
//            {
//                list.Add(node);
//                node = node.Parent;
//            }

//            list.Reverse();
//            list.Add(this);

//            return list;
//        }

//        public IList<T> GetElementPath()
//        {
//            List<T> list = new List<T>();
//            TreeNode<T> node = this.mParent;

//            while (node != null)
//            {
//                list.Add(node.Value);
//                node = node.Parent;
//            }

//            list.Reverse();
//            list.Add(this.mValue);

//            return list;
//        }

//        public string GetNodePathAsString(char separator, NodeToString<T> toString)
//        {
//            string s = string.Empty;
//            TreeNode<T> node = this;

//            while (node != null)
//            {
//                if (s.Length != 0)
//                {
//                    s = toString(node) + separator + s;
//                }
//                else
//                {
//                    s = toString(node);
//                }

//                node = node.Parent;
//            }

//            return s;
//        }

//        public string GetNodePathAsString(char separator)
//        {
//            return this.GetNodePathAsString(separator, (node) => { return node.Value.ToString(); });
//        }

//        #endregion // Node Path

//        #region Modify
//        /// <summary>
//        /// Removes the current node and all child nodes recursively from it's parent.
//        /// Throws an InvalidOperationException if this is a root node.
//        /// </summary>
//        public void Remove()
//        {
//            if (this.mParent == null)
//            {
//                throw new InvalidOperationException("cannot remove root node");
//            }

//            this.Detach();
//        }

//        /// <summary>
//        /// Detaches this node from it's parent. 
//        /// Postcondition: this is a root node.
//        /// </summary>
//        /// <returns></returns>
//        public TreeNode<T> Detach()
//        {
//            if (this.mParent != null)
//            {
//                this.Siblings.Remove(this);
//            }

//            return this;
//        }
//        #endregion // Modify

//        #region Enumerators
//        public IEnumerable<T> DepthFirstEnumerator
//        {
//            get
//            {
//                foreach (TreeNode<T> node in this.DepthFirstNodeEnumerator)
//                {
//                    yield return node.Value;
//                }
//            }
//        }

//        public IEnumerable<TreeNode<T>> DepthFirstNodeEnumerator
//        {
//            get
//            {
//                yield return this;
//                if (this.mNodes != null)
//                {
//                    foreach (TreeNode<T> child in this.mNodes)
//                    {
//                        IEnumerator<TreeNode<T>> childEnum = child.DepthFirstNodeEnumerator.GetEnumerator();
//                        while (childEnum.MoveNext())
//                        {
//                            yield return childEnum.Current;
//                        }
//                    }
//                }
//            }
//        }

//        public IEnumerable<TreeNode<T>> BreadthFirstNodeEnumerator
//        {
//            get
//            {
//                Queue<TreeNode<T>> todo = new Queue<TreeNode<T>>();
//                todo.Enqueue(this);
//                while (0 < todo.Count)
//                {
//                    TreeNode<T> node = todo.Dequeue();
//                    if (node.mNodes != null)
//                    {
//                        foreach (TreeNode<T> child in node.mNodes)
//                        {
//                            todo.Enqueue(child);
//                        }
//                    }

//                    yield return node;
//                }
//            }
//        }

//        public IEnumerable<T> BreadthFirstEnumerator
//        {
//            get
//            {
//                foreach (TreeNode<T> node in this.BreadthFirstNodeEnumerator)
//                {
//                    yield return node.Value;
//                }
//            }
//        }
//        #endregion

//        #region Internal Helper

//        internal void InternalDetach()
//        {
//            if (this.mParent != null)
//            {
//                this.mParent.Parent.Nodes.Remove(this);
//            }

//            this.mParent = null;
//            this.SetRootLink(new TreeRoot<T>(this));
//        }

//        internal void InternalSetParent(TreeNode<T> parent)
//        {
//            this.mParent = parent;
//            if (this.mParent != null)
//            {
//                this.SetRootLink(parent.Root);
//            }
//        }

//        #endregion // Internal Helper
//    }

//    #endregion // TreeNode<T>

//    /// <summary>
//    /// Implements a collection of Tree Nodes (Node<T>)
//    /// <para><b>Implementation Note:</b> The root of a data tree is always a Node<T>. You cannot
//    /// create a standalone NodeList<T>.
//    /// </para>
//    /// </summary>
//    /// <typeparam name="T">typeof the data value of each node</typeparam>
//    public class TreeNodeCollection<T>
//          : System.Collections.CollectionBase, IEnumerable<TreeNode<T>>
//    {
//        #region CTORs
//        internal TreeNodeCollection(TreeNode<T> owner)
//        {
//            if (owner == null)
//            {
//                throw new ArgumentNullException("owner");
//            }

//            this.mOwner = owner;
//        }

//        #endregion

//        #region Additional public interface

//        /// <summary>
//        /// The Node to which this collection belongs (this==Owner.Childs). 
//        /// Never null.
//        /// </summary>
//        public TreeNode<T> Owner
//        {
//            get { return this.mOwner; }
//        }

//        #endregion // public interface

//        #region Collection implementation (indexer, add, remove)

//        // Provide the strongly typed member for ICollection.
//        public void CopyTo(TreeNode<T>[] array, int index)
//        {
//            ((ICollection<TreeNode<T>>)this).CopyTo(array, index);
//        }

//        public new IEnumerator<TreeNode<T>> GetEnumerator()
//        {
//            foreach (TreeNode<T> node in this.InnerList)
//            {
//                yield return node;
//            }
//        }

//        public void Insert(int index, TreeNode<T> node)
//        {
//            this.List.Insert(index, node);
//        }

//        public bool Contains(TreeNode<T> node)
//        {
//            return this.List.Contains(node);
//        }

//        /// <summary>
//        /// Indexer accessing the index'th Node.
//        /// If the owning node belongs to a tree, Setting the node fires a NodeChanged event
//        /// </summary>
//        /// <param name="index"></param>
//        /// <returns></returns>
//        public TreeNode<T> this[int index]
//        {
//            get { return ((TreeNode<T>)this.List[index]); }
//            set { this.List[index] = value; }
//        }

//        /// <summary>
//        /// Appends a new node with the specified value
//        /// </summary>
//        /// <param name="value">value for the new node</param>
//        /// <returns>the node that was created</returns>
//        public TreeNode<T> Add(T value)
//        {
//            TreeNode<T> n = new TreeNode<T>(value);
//            this.List.Add(n);

//            this.SendOwnerNodeChanged(ENodeEvent.ChildAdded, this.List.Count - 1);

//            return n;
//        }

//        // required for XML Serializer, not to bad to have...
//        public void Add(TreeNode<T> node)
//        {
//            this.List.Add(node);
//            this.SendOwnerNodeChanged(ENodeEvent.ChildAdded, this.List.Count - 1);
//        }

//        /// <summary>
//        /// Adds a range of nodes created from a range of values
//        /// </summary>
//        /// <param name="range">range of values </param>
//        public void AddRange(IEnumerable<T> range)
//        {
//            foreach (T value in range)
//            {
//                this.Add(value);
//            }
//        }

//        /// <summary>
//        /// Adds a range of nodes created from a range of values passed as parameters
//        /// </summary>
//        /// <param name="range">range of values </param>
//        public void AddRange(params T[] args)
//        {
//            this.AddRange(args as IEnumerable<T>);
//        }

//        /// <summary>
//        /// Adds a new node with the given value at the specified index.
//        /// </summary>
//        /// <param name="index">Position where to insert the item.
//        /// All values are accepted, if index is out of range, the new item is inserted as first or 
//        /// last item</param>
//        /// <param name="value">value for the new node</param>
//        /// <returns></returns>
//        public TreeNode<T> InsertAt(int index, T value)
//        {
//            TreeNode<T> n = new TreeNode<T>(value, this.mOwner);

//            // "tolerant insert"
//            if (index < 0)
//            {
//                index = 0;
//            }

//            if (index >= this.Count)
//            {
//                index = this.Count;
//                this.List.Add(n);
//            }
//            else
//            {
//                this.List.Insert(index, n);
//            }

//            this.SendOwnerNodeChanged(ENodeEvent.ChildAdded, index);

//            return n;
//        }

//        /// <summary>
//        /// Inserts a range of nodes created from a range of values
//        /// </summary>
//        /// <param name="index">index where to start inserting. As with InsertAt, all values areaccepted</param>
//        /// <param name="values">a range of values set for the nodes</param>
//        public void InsertRangeAt(int index, IEnumerable<T> values)
//        {
//            foreach (T value in values)
//            {
//                this.InsertAt(index, value);
//                ++index;
//            }
//        }

//        /// <summary>
//        /// Inserts a new node before the specified node.
//        /// </summary>
//        /// <param name="insertPos">Existing node in front of which the new node is inserted</param>
//        /// <param name="value">value for the new node</param>
//        /// <returns>The newly created node</returns>
//        public TreeNode<T> InsertBefore(TreeNode<T> insertPos, T value)
//        {
//            int index = this.IndexOf(insertPos);
//            return this.InsertAt(index, value);
//        }

//        /// <summary>
//        /// Inserts a new node after the specified node
//        /// </summary>
//        /// <param name="insertPos">Existing node after which the new node is inserted</param>
//        /// <param name="value">value for the new node</param>
//        /// <returns>The newly created node</returns>
//        public TreeNode<T> InsertAfter(TreeNode<T> insertPos, T value)
//        {
//            int index = this.IndexOf(insertPos) + 1;
//            if (index == 0)
//            {
//                index = this.Count;
//            }

//            return this.InsertAt(index, value);
//        }

//        public int IndexOf(TreeNode<T> node)
//        {
//            return (this.List.IndexOf(node));
//        }

//        public void Remove(TreeNode<T> node)
//        {
//            int index = this.IndexOf(node);
//            if (index < 0)
//            {
//                throw new ArgumentException("the node to remove is not a in this collection");
//            }

//            this.RemoveAt(index);
//        }

//        #endregion

//        #region CollectionBase overrides (action handler)

//        protected override void OnValidate(object value)
//        {
//            // Verify: value.Parent must be null or this.mOwner)
//            base.OnValidate(value);
//            TreeNode<T> parent = ((TreeNode<T>)value).Parent;
//            if (parent != null && parent != this.mOwner)
//            {
//                throw new ArgumentException("Cannot add a node referenced in another node collection");
//            }
//        }

//        protected override void OnInsert(int index, object value)
//        {
//            // set parent note to this.mOwner
//            ((TreeNode<T>)value).InternalSetParent(this.mOwner);
//        }

//        protected override void OnRemoveComplete(int index, object value)
//        {
//            ((TreeNode<T>)value).InternalDetach();

//            this.SendOwnerNodeChanged(ENodeEvent.ChildRemoved, index);

//            base.OnRemoveComplete(index, value);
//        }

//        protected override void OnSet(int index, object oldValue, object newValue)
//        {
//            if (oldValue != newValue)
//            {
//                ((TreeNode<T>)oldValue).InternalDetach();
//                ((TreeNode<T>)newValue).InternalSetParent(this.mOwner);
//            }

//            base.OnSet(index, oldValue, newValue);
//        }

//        protected override void OnSetComplete(int index, object oldValue, object newValue)
//        {
//            this.SendOwnerNodeChanged(ENodeEvent.NodeChanged, index);
//            base.OnSetComplete(index, oldValue, newValue);
//        }

//        protected override void OnClear()
//        {
//            // set parent to null for all elements
//            foreach (TreeNode<T> node in this.InnerList)
//            {
//                node.InternalDetach();
//            }

//            base.OnClear();
//        }

//        protected override void OnClearComplete()
//        {
//            this.SendOwnerNodeChanged(ENodeEvent.ChildsCleared, 0);
//            base.OnClearComplete();
//        }

//        #endregion // CollectionBase overrides

//        #region protected helpers
//        private TreeNode<T> mOwner = null;

//        protected void SendOwnerNodeChanged(ENodeEvent changeHint, int index)
//        {
//            this.mOwner.Root.SendNodeChanged(this.Owner, changeHint, index);
//        }
//        #endregion // Internal Helpers

//        /* TODO:
//       * Exists(rpedicate), Find* 
//       * Swap (internal, expose at node?)
//       * Reverse, Sort
//       * TrueForAll
//       * 
//       */
//    }
//} // namespace ph.tree