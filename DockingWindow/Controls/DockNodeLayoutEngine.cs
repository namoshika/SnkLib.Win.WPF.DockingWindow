using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SunokoLibrary.Windows.Controls
{
    public abstract class DockNodeLayoutEngine
    {
        public DockNodeLayoutEngine(DockNode target)
        {
            Target = target;
            _children = new List<DockPaneLayoutEngine>();
            _ownNodes = new List<DockNodeLayoutEngine>();
        }
        protected List<DockPaneLayoutEngine> _children;
        protected List<DockNodeLayoutEngine> _ownNodes;

        public DockNode Target { get;private set;}
        public ReadOnlyCollection<DockPaneLayoutEngine> Children
        { get { return _children.AsReadOnly(); } }
        public ReadOnlyCollection<DockNodeLayoutEngine> OwnNodes
        { get { return _ownNodes.AsReadOnly(); } }
        ///<summary>
        ///保持している子要素をPane.Childrenのような構造に変換します。
        ///PaneではChildrenをそのまま返しているだけです。
        ///</summary>
        public abstract IEnumerable<DockPaneLayoutEngine> PaneVisualTree { get; }
        
        public void Add(DockPaneLayoutEngine pane, DockDirection align)
        { Add(pane, align, Children.Count); }
        public virtual void Add(DockPaneLayoutEngine pane, DockDirection align, int i)
        {
            _children.Insert(i, pane);
            pane.Removed += pane_Removed;
            AddEngine(pane);
            OnPaneAdded(new LayoutEngineAddedEventArgs(this, pane, align, i));
        }
        public virtual void Remove(DockPaneLayoutEngine pane)
        {
            if (pane.Children.Count > 0)
            {
                var newParent = pane.Children.Last();
                pane._children.Remove(newParent);
                ChildReplace(pane, newParent);
                newParent._children.InsertRange(0, pane.Children);
                newParent.Initialize(this, pane.Owner, pane.Align);
            }
            else
                _children.Remove(pane);

            RemoveEngine(pane);
            pane._ownNodes.Clear();
            pane._ownNodes.Add(pane);
            pane.Top = null;
            pane.Bottom = null;
            pane.Left = null;
            pane.Right = null;
            pane._children.Clear();
            //Initializeは実行前にChildrenをClearしないと子要素が巻き添え食らう
            pane.Initialize(null, null, DockDirection.None);
            pane.OnRemoved(new EventArgs());
            OnPaneRemoved(new LayoutEngineEventArgs(pane));
        }
        public abstract DockBaseConfig GetConfig();
        public virtual void SetConfig(DockBaseConfig config)
        {
            if (Target != null)
                Target.InternalApplyConfig(config);
            foreach (var conf in config.Children)
                conf.Target.SetConfig(conf);
        }
        protected virtual void InitConfig(DockBaseConfig config)
        {
            var conf = (DockBaseConfig)config;
            conf.Children = Children
                .Select(layout => layout.GetConfig())
                .Cast<DockPaneBaseConfig>().ToArray();
        }

        internal abstract IEnumerable<DockPaneLayoutEngine> GetChildrenOf(DockDirection align, int skip = 0, bool deep = true);
        internal virtual void ChildReplace(DockPaneLayoutEngine oldNeigh, DockPaneLayoutEngine newNeigh)
        {
            _children[_children.IndexOf(oldNeigh)] = newNeigh;
            OnPaneReplaced(new LayoutEngineReplaceEventArgs(oldNeigh, newNeigh));
        }
        internal virtual void AddEngine(DockPaneLayoutEngine layout)
        {
            foreach (var item in layout.OwnNodes)
                _ownNodes.Add(item);
        }
        internal virtual void RemoveEngine(DockPaneLayoutEngine layout)
        {
            _ownNodes.Remove(layout);
        }
        void pane_Removed(object sender, EventArgs e)
        {
            _children.Remove((DockPaneLayoutEngine)sender);
        }

        public event LayoutEngineAddedEventHandler PaneAdded;
        protected virtual void OnPaneAdded(LayoutEngineAddedEventArgs e)
        {
            if (PaneAdded != null)
                PaneAdded(this, e);
        }
        public event LayoutEngineAddedEventHandler PaneAdding;
        protected virtual void OnPaneAdding(LayoutEngineAddedEventArgs e)
        {
            if (PaneAdding != null)
                PaneAdding(this, e);
        }
        public event LayoutEngineEventHandler PaneRemoved;
        protected virtual void OnPaneRemoved(LayoutEngineEventArgs e)
        {
            if (PaneRemoved != null)
                PaneRemoved(this, e);
        }
        public event LayoutEngineReplaceEventHandler PaneReplaced;
        protected virtual void OnPaneReplaced(LayoutEngineReplaceEventArgs e)
        {
            if (PaneReplaced != null)
                PaneReplaced(this, e);
        }
    }

    public enum DockDirection
    {
        None = 0,
        Top = 1, Bottom = ~Top,
        Left = 2, Right = ~Left,
    }

    public delegate void LayoutEngineEventHandler(object sender, LayoutEngineEventArgs e);
    public class LayoutEngineEventArgs : EventArgs
    {
        public LayoutEngineEventArgs(DockPaneLayoutEngine pane)
        {
            DockPane = pane;
        }
        public DockPaneLayoutEngine DockPane { get; protected set; }
    }
    public delegate void LayoutEngineAddedEventHandler(object sender, LayoutEngineAddedEventArgs e);
    public class LayoutEngineAddedEventArgs : LayoutEngineEventArgs
    {
        public LayoutEngineAddedEventArgs(DockNodeLayoutEngine target, DockPaneLayoutEngine pane, DockDirection align, int insertIndex)
            : base(pane)
        {
            Target = target;
            Align = align;
            InsertIndex = insertIndex;
        }
        public int InsertIndex { get; protected set; }
        public DockDirection Align { get; protected set; }
        public DockNodeLayoutEngine Target { get; private set; }
    }
    public delegate void LayoutEngineReplaceEventHandler(object sender, LayoutEngineReplaceEventArgs e);
    public class LayoutEngineReplaceEventArgs
    {
        public LayoutEngineReplaceEventArgs(DockPaneLayoutEngine oldPane, DockPaneLayoutEngine newPane)
        {
            NewPane = newPane;
            OldPane = oldPane;
        }
        public DockPaneLayoutEngine NewPane { get; protected set; }
        public DockPaneLayoutEngine OldPane { get; protected set; }
    }
}
